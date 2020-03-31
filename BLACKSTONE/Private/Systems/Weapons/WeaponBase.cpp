// Fill out your copyright notice in the Description page of Project Settings.


#include "WeaponBase.h"
#include "Components/SkeletalMeshComponent.h"
#include "Components/SphereComponent.h"
#include "DrawDebugHelpers.h"
#include "Kismet/GameplayStatics.h"
#include "TimerManager.h"
#include "Kismet/KismetMathLibrary.h"
#include "Sound/SoundCue.h"
#include "Particles/ParticleSystem.h"
#include "Systems/Player/WeaponComponent.h"
#include "Engine/SkeletalMesh.h"
#include "Blackstone/Core/Player/BlackstoneCharacter.h"
#include "Math/Vector2D.h"

// Sets default values
AWeaponBase::AWeaponBase()
{
	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	//PrimaryActorTick.bCanEverTick = true;

	//WeaponMesh = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("WeaponMesh"));
	//TPMesh = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("ThirdPersonWeapon"));
	AttachMeshes();
	SetMeshes();
	SetMeshVisibility();
	SocketMeshes();

	MuzzlePoint = CreateDefaultSubobject<USphereComponent>(TEXT("MuzzlePoint"));
	MuzzlePoint->SetSphereRadius(5.f);

	//RootComponent = WeaponMesh;
	//TPMesh->SetupAttachment(RootComponent);
	MuzzlePoint->SetupAttachment(RootComponent);

	//WeaponMesh->bOnlyOwnerSee = true;
	//TPMesh->bOwnerNoSee = true;

	FireRate = 600;
	FireMode = EFireMode::Auto;
	NumberProjectiles = 1;
	Damage = 10;
	ShotDistance = 12000;
	Accuracy = 5;
	RoundsInBurst = 3;
	delayBetweenBursts = .1f;

	//CurrentMag = 120;
	//MagSize = 120;

	bHasSemi = true;
	bHasBurst = false;
	bHasAuto = true;
	WeaponType = EWeaponType::Rifle;

	PitchRecoil.X = 2.f;
	PitchRecoil.Y = 2.f;

	YawRecoil.X = 2.f;
	YawRecoil.Y = 2.f;
}

// Called when the game starts or when spawned
void AWeaponBase::BeginPlay()
{
	Super::BeginPlay();


	adjustedFireRate = 60 / FireRate;

	//How fast the timer will iterate
	RecoilTimer = 0.01f;

	//Calculate how much we need to divide the recoil rotator by
	RecoilDivider = adjustedFireRate / RecoilTimer;

	//Honestly only using this so I don't have to do the divide frequently, feels less costly
	RecoilDelta = 1 / RecoilDivider;
}

// Called every frame
void AWeaponBase::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

//Player is trying to fire
void AWeaponBase::TriggerPulled()
{
	bTriggerPulled = true;

	if (CanFire())
	{
		timeTillCanFire = GetWorld()->TimeSeconds + adjustedFireRate;

		switch (FireMode)
		{
		case EFireMode::Semi:
			FiringLogic();
			break;

		case EFireMode::Burst:
			//currentRoundInBurst = 0;
			bIsOnBurstDelay = true;
			//break;

		case EFireMode::Auto:
			//GetWorld()->TimerManager->SetTimer(T_FiringTimer, adjustedFireRate, )
			GetWorld()->GetTimerManager().SetTimer(T_FiringTimer, this, &AWeaponBase::FiringLogic, adjustedFireRate, true, 0.f);
			break;
		}
	}
}

void AWeaponBase::TriggerReleased()
{
	bTriggerPulled = false;
	CeaseFire();
}

//Stop firing our weapon
void AWeaponBase::CeaseFire()
{
	//CurrentMag > 0
	int _Mag = 0;
	WeaponComp->GetAmmoForType(WeaponType, _Mag);

	if (FireMode == EFireMode::Burst && _Mag > 0) {
		return;
	}

	GetWorld()->GetTimerManager().PauseTimer(T_FiringTimer);
	GetWorld()->GetTimerManager().ClearTimer(T_FiringTimer);

}

//Can we fire our weapon?
bool AWeaponBase::CanFire()
{
	//CurrentMag
	int _Mag = 0;
	WeaponComp->GetAmmoForType(WeaponType, _Mag);

	if (_Mag <= 0)
	{
		MagEmpty();
		return false;
	}

	if (GetWorld()->TimeSeconds < timeTillCanFire)
	{
		return false;
	}

	if (FireMode == EFireMode::Burst && bIsOnBurstDelay)
	{
		return false;
	}

	return true;
}

void AWeaponBase::BurstDelayOver()
{
	bIsOnBurstDelay = false;

	if (bTriggerPulled && FireMode == EFireMode::Burst)
	{
		TriggerPulled();
	}
}

//Handles firing the weapon
void AWeaponBase::FiringLogic()
{
	//CurrentMag--;
	if (WeaponComp == nullptr)
	{
		//UE_LOG(LogTemp, Log, TEXT("AWeaponBase::FiringLogic - WeaponComp Null, not firing weapons"));
		TriggerReleased();
		return;
	}

	int _Mag = 0;
	WeaponComp->FireAmmo(WeaponType, _Mag);

	//Spawn all projectiles (useful for shotgun weapons)
	for (int curRound = 0; curRound < NumberProjectiles; curRound++)
	{
		ProjectileLogic();
	}

	//Burst fire
	if (FireMode == EFireMode::Burst) {
		currentRoundInBurst++;

		if (currentRoundInBurst >= RoundsInBurst) {
			//CeaseFire();
			GetWorld()->GetTimerManager().PauseTimer(T_FiringTimer);
			GetWorld()->GetTimerManager().ClearTimer(T_FiringTimer);

			bIsOnBurstDelay = true;

			currentRoundInBurst = 0;

			timeTillCanFire = GetWorld()->TimeSeconds + delayBetweenBursts;
			GetWorld()->GetTimerManager().SetTimer(T_BurstFireDelayTimer, this, &AWeaponBase::BurstDelayOver, delayBetweenBursts, false);
		}
	}

	FiringEffects();

	//if mag is empty, stop firing
	if (_Mag <= 0) {
		MagEmpty();
		CeaseFire();
	}
}

//Handles logic for the projectile
void AWeaponBase::ProjectileLogic()
{
	//Trace weapon from pawn eyes (center of screen) to cross-hair location
	//AActor* Owner = this->GetOwner();

	AActor* _Owner = nullptr;
	_Owner = GetOwner();

	if (_Owner)
	{
		//Getting Shot location and rotation via actor eyes viewpoint
		FVector EyeLocation;
		FRotator EyeRotation;
		_Owner->GetActorEyesViewPoint(EyeLocation, EyeRotation);

		FVector shotDirection = EyeRotation.Vector();

		FVector _Accuracy = UKismetMathLibrary::RandomUnitVectorInConeInDegrees(shotDirection, Accuracy);

		//Getting shot direction and distance via converting rotation to vector and multiplying it by the distance
		FVector TraceEnd = EyeLocation + (_Accuracy * ShotDistance);

		FCollisionQueryParams QueryParams;
		QueryParams.AddIgnoredActor(_Owner);
		QueryParams.AddIgnoredActor(this);
		QueryParams.bTraceComplex = true;

		FHitResult Hit;
		//Line-trace returns a bool, and we only want to deal damage and such if we returned a hit
		if (GetWorld()->LineTraceSingleByChannel(Hit, EyeLocation, TraceEnd, ECC_Visibility, QueryParams))
		{
			//Blocking hit, do deal damage

			AActor* hitActor = Hit.GetActor();
			if (hitActor)
			{
				UGameplayStatics::ApplyPointDamage(hitActor, Damage, _Accuracy, Hit, _Owner->GetInstigatorController(), this, DamageClass);
			}
		}

		//DEBUGGING LINETRACE
		//Determine color of line-trace based on blocking hit or not
		//FColor hitColor = Hit.bBlockingHit ? FColor::Red : FColor::Yellow;
		//DrawDebugLine(GetWorld(), EyeLocation, TraceEnd, hitColor, false, 1.0f, 0, 2.0f);

		//PlayEffects();
	}
}

void AWeaponBase::MagEmpty()
{
	FVector _Location = Muzzle->GetComponentLocation();

	if (EmptyClickSFX != nullptr)
	{
		UGameplayStatics::PlaySoundAtLocation(this, EmptyClickSFX, _Location);
	}
}

//Spawning firing effects
void AWeaponBase::FiringEffects()
{
	FVector _Location = MuzzlePoint->GetComponentLocation();

	if (ShotSFX != nullptr)
	{
		UGameplayStatics::PlaySoundAtLocation(this, ShotSFX, _Location);
	}

	if (MuzzleFlash != nullptr)
	{
		FName _Name = "";
		_Location = FVector::ZeroVector;
		FRotator _Rotation = MuzzlePoint->GetRelativeTransform().Rotator();

		UGameplayStatics::SpawnEmitterAttached(MuzzleFlash, MuzzlePoint, _Name, _Location, _Rotation);
	}

	CalculateRecoil();

	if (WeaponComp != nullptr)
	{
		WeaponComp->PlayWeaponAnimations(FiringMontageFP, FiringMontageTP, adjustedFireRate);
	}
}

void AWeaponBase::CalculateRecoil() 
{
	//ensuring timers are reset and cleared
	GetWorld()->GetTimerManager().PauseTimer(T_RecoilTimer);
	GetWorld()->GetTimerManager().ClearTimer(T_RecoilTimer);
	GetWorld()->GetTimerManager().PauseTimer(T_RecoilResetTimer);
	GetWorld()->GetTimerManager().ClearTimer(T_RecoilResetTimer);

	//clearing default values
	RecoilAlpha = 0;
	PitchRecoilAmount = 0.f;
	YawRecoilAmount = .0f;

	//Debugging
	//GEngine->AddOnScreenDebugMessage(-1, 1.f, FColor::Red, TEXT("WeaponBase::DisperseRecoil, Starting Rotation increase"));

	//Calculating recoil amount
	float _CalculatedPitch = FMath::RandRange(PitchRecoil.X, PitchRecoil.Y);
	float _CalculatedYaw = FMath::RandRange(YawRecoil.X, YawRecoil.Y);
	
	//Dispersing recoil amount over time
	PitchRecoilAmount = _CalculatedPitch / RecoilDivider;
	YawRecoilAmount = _CalculatedYaw / RecoilDivider;

	//Creating timer to disperse recoil
	GetWorld()->GetTimerManager().SetTimer(T_RecoilTimer, this, &AWeaponBase::DisperseRecoil, RecoilTimer, true, 0.f);
}

void AWeaponBase::DisperseRecoil() 
{
	//Same as RecoilAlpha += 1 / ((60/Firerate) / RecoilTimer)
	//Thought this was more efficient and more readable
	RecoilAlpha += RecoilDelta;

	ABlackstoneCharacter* _Character = Cast<ABlackstoneCharacter>(GetOwner());
	if (_Character != nullptr)
	{
		_Character->AddViewRecoil(PitchRecoilAmount, YawRecoilAmount);
	}

	if (RecoilAlpha >= 1)
	{
		RecoilAlpha = 0;
		//GEngine->AddOnScreenDebugMessage(-1, 1.f, FColor::Red, TEXT("WeaponBase::DisperseRecoil, Stopping Rotation increase"));
		GetWorld()->GetTimerManager().PauseTimer(T_RecoilTimer);
		GetWorld()->GetTimerManager().ClearTimer(T_RecoilTimer);

		GetWorld()->GetTimerManager().SetTimer(T_RecoilResetTimer, this, &AWeaponBase::AllowRecoilReset, .05f);
	}
}

void AWeaponBase::AllowRecoilReset() {
	ABlackstoneCharacter* _Character = Cast<ABlackstoneCharacter>(GetOwner());
	if (_Character != nullptr)
	{
		_Character->NoRecoilApplied();
	}
}

//Set weapon firing mode, falls through bad cases and returns to whichever it has
void AWeaponBase::SetFireMode(EFireMode newmode)
{
	TriggerReleased();
	//CeaseFire();

	if (bHasSemi || bHasBurst || bHasAuto)
	{
		switch (newmode)
		{
		case EFireMode::Semi:
			if (bHasBurst)
			{
				FireMode = EFireMode::Burst;
				break;
			}

		case EFireMode::Burst:
			if (bHasAuto)
			{
				FireMode = EFireMode::Auto;
				break;
			}

		case EFireMode::Auto:
			if (bHasSemi)
			{
				FireMode = EFireMode::Semi;
				break;
			}

			SetFireMode(EFireMode::Semi);
		}
	}
}

void AWeaponBase::SwitchFireMode()
{
	SetFireMode(FireMode);
}

//Returns the weapon type
void AWeaponBase::GetWeaponType(EWeaponType& GunType)
{
	GunType = WeaponType;
}

void AWeaponBase::AttachMeshes()
{
	//Creating Objects
	WeaponMesh = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("WeaponBase"));
	HullMesh = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("Hull"));
	HandGuard = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("Handguard"));
	Barrel = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("Barrel"));
	Muzzle = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("Muzzle"));
	GripAttachment = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("GripAttachment"));
	Butt = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("Butt"));
	MagMount = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("MagMount"));
	Mag = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("Mag"));
	WeaponScope = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("Scope"));
	GripIKLocator = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("GripIKLocator"));

	TPMesh = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("TPMesh"));
	TPHull = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("TPHull"));
	TPHandGuard = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("TPHandGuard"));
	TPBarrel = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("TPBarrel"));
	TPMuzzle = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("TPMuzzle"));
	TPGripAttachment = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("TPGripAttach"));
	TPButt = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("TPButt"));
	TPMagMount = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("TPMagMount"));
	TPMag = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("TPMag"));
	TPScope = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("TPScope"));

	//Attachment rules
	RootComponent = WeaponMesh;

	//Attachment FP
	HullMesh->SetupAttachment(WeaponMesh);
	HandGuard->SetupAttachment(HullMesh);
	Barrel->SetupAttachment(HandGuard);
	Muzzle->SetupAttachment(Barrel);

	GripAttachment->SetupAttachment(HandGuard);
	Butt->SetupAttachment(HullMesh);
	MagMount->SetupAttachment(HullMesh);
	Mag->SetupAttachment(MagMount);
	WeaponScope->SetupAttachment(HullMesh);
	GripIKLocator->SetupAttachment(WeaponMesh);

	//EndFP Meshes
	TPMesh->SetupAttachment(WeaponMesh);

	//Attachment Third person
	TPHull->SetupAttachment(TPMesh);
	TPHandGuard->SetupAttachment(TPHull);
	TPBarrel->SetupAttachment(TPHandGuard);
	TPMuzzle->SetupAttachment(TPBarrel);

	TPGripAttachment->SetupAttachment(TPHandGuard);
	TPButt->SetupAttachment(TPHull);
	TPMagMount->SetupAttachment(TPHull);
	TPMag->SetupAttachment(TPMagMount);
	TPScope->SetupAttachment(TPHull);
}

void AWeaponBase::SocketMeshes()
{
	FAttachmentTransformRules _Rules = FAttachmentTransformRules::KeepRelativeTransform;
	_Rules.bWeldSimulatedBodies = true;

	//Attach components to socket locations
	if (WeaponMesh->DoesSocketExist("HullMnt"))
		HullMesh->AttachToComponent(WeaponMesh, _Rules, "HullMnt");

	if (HullMesh->DoesSocketExist("HandguardMnt"))
		HandGuard->AttachToComponent(HullMesh, _Rules, "HandguardMnt");

	if (HandGuard->DoesSocketExist("BarrelMnt"))
		Barrel->AttachToComponent(HandGuard, _Rules, "BarrelMnt");

	if (Barrel->DoesSocketExist("MuzzleMnt"))
		Muzzle->AttachToComponent(Barrel, _Rules, "MuzzleMnt");

	if (HullMesh->DoesSocketExist("ButtMnt"))
		Butt->AttachToComponent(HullMesh, _Rules, "ButtMnt");

	if (HullMesh->DoesSocketExist("MagMnt"))
		MagMount->AttachToComponent(HullMesh, _Rules, "MagMnt");

	if (MagMount->DoesSocketExist("MagMnt"))
		Mag->AttachToComponent(MagMount, _Rules, "MagMnt");

	if (HullMesh->DoesSocketExist("ScopeMnt"))
		WeaponScope->AttachToComponent(HullMesh, _Rules, "ScopeMnt");

	if (HandGuard->DoesSocketExist("GripMnt"))
		GripAttachment->AttachToComponent(HandGuard, _Rules, "GripMnt");

	//Attach components to socket locations
	if (TPMesh->DoesSocketExist("HullMnt"))
		TPHull->AttachToComponent(TPMesh, _Rules, "HullMnt");

	if (TPHull->DoesSocketExist("HandguardMnt"))
		TPHandGuard->AttachToComponent(TPHull, _Rules, "HandguardMnt");

	if (TPHandGuard->DoesSocketExist("BarrelMnt"))
		TPBarrel->AttachToComponent(TPHandGuard, _Rules, "BarrelMnt");

	if (TPBarrel->DoesSocketExist("MuzzleMnt"))
		TPMuzzle->AttachToComponent(TPBarrel, _Rules, "MuzzleMnt");

	if (TPHull->DoesSocketExist("ButtMnt"))
		TPButt->AttachToComponent(TPHull, _Rules, "ButtMnt");

	if (TPHull->DoesSocketExist("MagMnt"))
		TPMagMount->AttachToComponent(TPHull, _Rules, "MagMnt");

	if (TPMagMount->DoesSocketExist("MagMnt"))
		TPMag->AttachToComponent(TPMagMount, _Rules, "MagMnt");

	if (TPHull->DoesSocketExist("ScopeMnt"))
		TPScope->AttachToComponent(TPHull, _Rules, "ScopeMnt");

	if (TPHandGuard->DoesSocketExist("GripMnt"))
		TPGripAttachment->AttachToComponent(TPHandGuard, _Rules, "GripMnt");
}

void AWeaponBase::SetMeshVisibility()
{
	//Setting up visibilities
	WeaponMesh->SetOnlyOwnerSee(true);
	HullMesh->SetOnlyOwnerSee(true);
	HandGuard->SetOnlyOwnerSee(true);
	Barrel->SetOnlyOwnerSee(true);
	Muzzle->SetOnlyOwnerSee(true);

	GripAttachment->SetOnlyOwnerSee(true);
	Butt->SetOnlyOwnerSee(true);
	MagMount->SetOnlyOwnerSee(true);
	Mag->SetOnlyOwnerSee(true);
	WeaponScope->SetOnlyOwnerSee(true);
	GripIKLocator->SetHiddenInGame(true);

	TPMesh->SetOwnerNoSee(true);
	TPHull->SetOwnerNoSee(true);
	TPHandGuard->SetOwnerNoSee(true);
	TPBarrel->SetOwnerNoSee(true);
	TPMuzzle->SetOwnerNoSee(true);

	TPGripAttachment->SetOwnerNoSee(true);
	TPButt->SetOwnerNoSee(true);
	TPMagMount->SetOwnerNoSee(true);
	TPMag->SetOwnerNoSee(true);
	TPScope->SetOwnerNoSee(true);
}

void AWeaponBase::SetMeshes()
{
	if (Hulls.Contains(Receiver))
	{
		USkeletalMesh* _ReceiverMesh = *Hulls.Find(Receiver);
		if (_ReceiverMesh != nullptr)
		{
			HullMesh->SetSkeletalMesh(_ReceiverMesh);
			TPHull->SetSkeletalMesh(_ReceiverMesh);
		}
	}

	if (Barrells.Contains(WeaponBarrel))
	{
		USkeletalMesh* _BarrelMesh = *Barrells.Find(WeaponBarrel);
		if (_BarrelMesh != nullptr)
		{
			Barrel->SetSkeletalMesh(_BarrelMesh);
			TPBarrel->SetSkeletalMesh(_BarrelMesh);
		}
	}

	if (PistolGrips.Contains(PistolGrip))
	{
		USkeletalMesh* _PistolGrip = *PistolGrips.Find(PistolGrip);
		if (_PistolGrip != nullptr)
		{
			WeaponMesh->SetSkeletalMesh(_PistolGrip);
			TPMesh->SetSkeletalMesh(_PistolGrip);
		}
	}

	if (HandGuards.Contains(WeaponHandGuard))
	{
		USkeletalMesh* _HandGuardMesh = *HandGuards.Find(WeaponHandGuard);
		if (_HandGuardMesh != nullptr)
		{
			HandGuard->SetSkeletalMesh(_HandGuardMesh);
			TPHandGuard->SetSkeletalMesh(_HandGuardMesh);
		}
	}

	if (ForeGrips.Contains(ForeGrip))
	{
		USkeletalMesh* _ForeGripMesh = *ForeGrips.Find(ForeGrip);
		if (_ForeGripMesh != nullptr)
		{
			GripAttachment->SetSkeletalMesh(_ForeGripMesh);
			TPGripAttachment->SetSkeletalMesh(_ForeGripMesh);
		}
	}

	if (Butts.Contains(WeaponButt))
	{
		USkeletalMesh* _ButtMesh = *Butts.Find(WeaponButt);
		if (_ButtMesh != nullptr)
		{
			Butt->SetSkeletalMesh(_ButtMesh);
			TPButt->SetSkeletalMesh(_ButtMesh);
		}
	}

	if (MagMounts.Contains(MagazineMount))
	{
		USkeletalMesh* _MagMountMesh = *MagMounts.Find(MagazineMount);
		if (_MagMountMesh != nullptr)
		{
			MagMount->SetSkeletalMesh(_MagMountMesh);
			TPMagMount->SetSkeletalMesh(_MagMountMesh);
		}
	}

	if (Mags.Contains(Magazine))
	{
		USkeletalMesh* _MagazineMesh = *Mags.Find(Magazine);
		if (_MagazineMesh != nullptr)
		{
			Mag->SetSkeletalMesh(_MagazineMesh);
			TPMag->SetSkeletalMesh(_MagazineMesh);
		}
	}

	if (Sights.Contains(WeaponSight))
	{
		USkeletalMesh* _SightMesh = *Sights.Find(WeaponSight);
		if (_SightMesh != nullptr)
		{
			WeaponScope->SetSkeletalMesh(_SightMesh);
			TPScope->SetSkeletalMesh(_SightMesh);
		}
	}

	if (Muzzles.Contains(WeaponMuzzle))
	{
		USkeletalMesh* _MuzzleMesh = *Muzzles.Find(WeaponMuzzle);
		if (_MuzzleMesh != nullptr)
		{
			Muzzle->SetSkeletalMesh(_MuzzleMesh);
			TPMuzzle->SetSkeletalMesh(_MuzzleMesh);
		}
	}
}

void AWeaponBase::AttachWeapon(USceneComponent* AttachPoint, USceneComponent* AttachPointTP, FName AttachName)
{
	FAttachmentTransformRules attach = FAttachmentTransformRules::SnapToTargetNotIncludingScale;

	this->AttachToComponent(AttachPoint, attach, AttachName);

	TPMesh->AttachToComponent(AttachPointTP, attach, AttachName);
}
