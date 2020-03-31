// Fill out your copyright notice in the Description page of Project Settings.


#include "BlackstoneCharacter.h"
#include "Camera/CameraComponent.h"
#include "Systems/Player/WeaponComponent.h"
#include "Systems/Health/HealthComponent.h"
#include "Kismet/KismetMathLibrary.h"

// Sets default values
ABlackstoneCharacter::ABlackstoneCharacter()
{
	// Set this character to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	Camera = CreateDefaultSubobject<UCameraComponent>(TEXT("Camera"));
	FPArms = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("Arms"));

	Weapon = CreateDefaultSubobject<UWeaponComponent>(TEXT("WeaponComp"));
	HealthComp = CreateDefaultSubobject<UHealthComponent>(TEXT("HealthComp"));


	Camera->SetupAttachment(RootComponent);
	FPArms->SetupAttachment(Camera);

	Camera->bUsePawnControlRotation = false;// true;
	Camera->SetRelativeLocation(FVector(-10, 0, 50));

	//because we're overriding this with our own pitch and yaw values
	this->bUseControllerRotationYaw = false;

	//Uncomment when we have mesh for arms
	Weapon->AttachPoint = FPArms;
	Weapon->TPAttachPoint = GetMesh();

	FPArms->bOnlyOwnerSee = true;
	GetMesh()->bOwnerNoSee = true;

	PitchCenteringSpeed = 1.5f;
	YawCenteringSpeed = 20.f;
}

// Called when the game starts or when spawned
void ABlackstoneCharacter::BeginPlay()
{
	Super::BeginPlay();

	Weapon->AttachPoint = FPArms;
	Weapon->TPAttachPoint = GetMesh();

	Weapon->SpawnInitialWeapons();
}

// Called every frame
void ABlackstoneCharacter::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	InterpRecoil();
	CameraLook();
	PlayerRotation();

	//GEngine->AddOnScreenDebugMessage(-1, 0.f, FColor::Green, FString::Printf(TEXT("target Pitch: %f"), TargetRecoil.Pitch));
	//GEngine->AddOnScreenDebugMessage(-1, 0.f, FColor::Red, FString::Printf(TEXT("recoil Pitch: %f"), Recoil.Pitch));
}

//Overriding actor eyes viewpoint (and subsequently pawn view location)
//So that the view location is accurately tied to the camera, and not the "suggested" point
void ABlackstoneCharacter::GetActorEyesViewPoint(FVector& Location, FRotator& Rotation) const
{
	if (Camera != nullptr) {
		Location = Camera->GetComponentLocation();
		Rotation = Camera->GetComponentRotation();

		return;
	}

	Super::GetActorEyesViewPoint(Location, Rotation);
}

#pragma region  Player Movement

void ABlackstoneCharacter::PitchLook(float Value)
{
	float _AdjustedValue = -Value;

	//UE_LOG(LogTemp, Log, TEXT("ABlackstoneCharacter::PitchLook() - Adjusted Value is %f"), _AdjustedValue);

	//FMath::IsNearlyZero(Recoil.Pitch, 0.5f)
	//Recoil pitch is > 0
	//Dont contribute to recoil calcs if looking up
	if (_AdjustedValue < 0)
	{

		if (RecoilPitch > 0)
		{
			float _Recoil = RecoilPitch;

			_Recoil = _Recoil + _AdjustedValue;

			RecoilPitch = FMath::Max(_Recoil, 0.f);

			if (_Recoil < 0)
			{
				_AdjustedValue = _Recoil;
			}
			else
			{
				return;
			}
		}
	}

	float _newPitch = Pitch + _AdjustedValue;

	_newPitch = FMath::Clamp(_newPitch, -90.f, 90.f);

	Pitch = _newPitch;
}

void ABlackstoneCharacter::AdjustYaw(float Value)
{
	//Yaw += Value;
	Yaw = Yaw + Value;

	if (Yaw < 0.f)
	{
		Yaw = 359.5f;
	}

	if (Yaw > 359.9f)
	{
		Yaw = 0;
	}
}

void ABlackstoneCharacter::YawLook(float Value)
{
	//If there's no recoil, then proceed as normal
	if (FMath::IsNearlyEqual(RecoilYaw, 0.f, 0.1f) != true)//(FMath::IsNearlyZero(RecoilYaw, 0.05f))
	{
		float _adjustedYaw = 0.f;

		if (RecoilYaw > 0)
		{
			if (Value < 0)
			{
				RecoilYaw = FMath::Max(RecoilYaw + Value, 0.f);

				if (FMath::IsNearlyEqual(RecoilYaw, 0.f, 0.1f))
				{
					AdjustYaw(Value);
				}
			}
			else
			{
				AdjustYaw(Value);
			}
		}
		else
		{
			if (Value < 0)
			{
				AdjustYaw(Value);
			}
			else
			{
				RecoilYaw = FMath::Min(RecoilYaw + Value, 0.f);

				if (FMath::IsNearlyEqual(RecoilYaw, 0.f, 0.1f))
				{
					AdjustYaw(Value);
				}
			}
		}

		return;
	}

	AdjustYaw(Value);

	//GEngine->AddOnScreenDebugMessage(-1, 1.f, FColor::Red, TEXT("BSCharacter::YawLook, Is going through Adjust YAW"));
}

void ABlackstoneCharacter::CameraLook()
{
	float _Pitch = FMath::Clamp(Pitch + RecoilPitch, -90.f, 90.f);

	FRotator _LookRotation = FRotator::ZeroRotator;

	_LookRotation.Pitch = _Pitch;

	Camera->SetRelativeRotation(_LookRotation);
}

void ABlackstoneCharacter::PlayerRotation()
{
	FRotator _PlayerRotation = FRotator::ZeroRotator;

	_PlayerRotation.Yaw = Yaw + RecoilYaw;

	this->SetActorRotation(_PlayerRotation);
}

void ABlackstoneCharacter::InterpRecoil()
{
	//Ensure that we're not increasing the recoil values, this helps ensure no wonkiness
	if (isIncreasingRotation)
	{
		return;
	}
	//Reducing recoil values to 0 over time
	//RecoilPitch = FMath::FInterpTo(RecoilPitch, 0.0f, GetWorld()->DeltaTimeSeconds, PitchCenteringSpeed);
	RecoilPitch = FMath::FInterpConstantTo(RecoilPitch, 0.0f, GetWorld()->DeltaTimeSeconds, PitchCenteringSpeed);

	RecoilYaw = FMath::FInterpConstantTo(RecoilYaw, 0.0f, GetWorld()->DeltaTimeSeconds, YawCenteringSpeed);

}

//Cheap and dirty way of adding recoil, potentially make this a Timeline function,
//Or at least some smooth (InterpTo) operation later
void ABlackstoneCharacter::AddRecoil(FRotator RecoilAmount)
{
	//Prevents interping of recoil
	isIncreasingRotation = true;

	FRotator _NewRecoil = UKismetMathLibrary::ComposeRotators(Recoil, RecoilAmount);

	float _PitchAdjustment = UKismetMathLibrary::Abs(Pitch - 90.f);

	_NewRecoil.Pitch = FMath::Clamp(_NewRecoil.Pitch, 0.f, _PitchAdjustment);

	Recoil = _NewRecoil;

}

void ABlackstoneCharacter::AddViewRecoil(float PitchValue, float YawValue)
{
	//float _PitchAdjustment = 
	isIncreasingRotation = true;
	RecoilPitch = FMath::Clamp(RecoilPitch + PitchValue, 0.0f, 180.f);
	RecoilYaw += YawValue;
}

void ABlackstoneCharacter::NoRecoilApplied() {
	isIncreasingRotation = false;
	//GEngine->AddOnScreenDebugMessage(-1, 1.f, FColor::Red, TEXT("BSCharacter::NoRecoilApplied, recoil should depreciate"));
}

void ABlackstoneCharacter::MoveForward(float Value)
{
	if (Value != 0.0f)
	{
		// add movement in that direction
		AddMovementInput(GetActorForwardVector(), Value);
	}
}

void ABlackstoneCharacter::MoveRight(float Value)
{
	if (Value != 0.0f)
	{
		// add movement in that direction
		AddMovementInput(GetActorRightVector(), Value);
	}
}

#pragma endregion

#pragma region Weapons

void ABlackstoneCharacter::Fire()
{
	Weapon->FireWeapon();
}

void ABlackstoneCharacter::CeaseFire()
{
	Weapon->CeaseFire();
}

//Switching between weapons, as two different functions because C++ hates me
void ABlackstoneCharacter::IncrementWeapon()
{
	Weapon->SwitchWeapon(true);
}

void ABlackstoneCharacter::DecrementWeapon()
{
	Weapon->SwitchWeapon(false);
}

void ABlackstoneCharacter::SwitchFireMode()
{
	Weapon->SwitchFire();
}

#pragma endregion



// Called to bind functionality to input
void ABlackstoneCharacter::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

	//Weapon input
	PlayerInputComponent->BindAction("Fire", IE_Pressed, this, &ABlackstoneCharacter::Fire);
	PlayerInputComponent->BindAction("Fire", IE_Released, this, &ABlackstoneCharacter::CeaseFire);
	PlayerInputComponent->BindAction("SelectFire", IE_Released, this, &ABlackstoneCharacter::SwitchFireMode);

	//Weapon Selection
	PlayerInputComponent->BindAction("SelectUp", IE_Released, this, &ABlackstoneCharacter::IncrementWeapon);
	PlayerInputComponent->BindAction("SelectDown", IE_Released, this, &ABlackstoneCharacter::DecrementWeapon);

	//Movement input
	PlayerInputComponent->BindAction("Jump", IE_Pressed, this, &ACharacter::Jump);
	PlayerInputComponent->BindAxis("MoveForward", this, &ABlackstoneCharacter::MoveForward);
	PlayerInputComponent->BindAxis("MoveRight", this, &ABlackstoneCharacter::MoveRight);

	//Look input
	//Look calls to built in functions, so we're not creating our own.
	PlayerInputComponent->BindAxis("LookRight", this, &ABlackstoneCharacter::YawLook); // AddControllerYawInput);
	PlayerInputComponent->BindAxis("LookUp", this, &ABlackstoneCharacter::PitchLook); //&APawn::AddControllerPitchInput);

}