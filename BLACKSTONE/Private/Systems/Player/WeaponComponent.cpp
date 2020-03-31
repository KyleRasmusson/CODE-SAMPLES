// Fill out your copyright notice in the Description page of Project Settings.


#include "WeaponComponent.h"
#include "Systems/Weapons/WeaponBase.h"
#include "Engine/World.h"
#include "Animation/AnimMontage.h"

// Sets default values for this component's properties
UWeaponComponent::UWeaponComponent()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = false;//true;

	// ...
	maxWeapons = 3;
	currentWeaponIndex = 0;

	AttachPointName = "WeaponSocket";

	RifleAmmo = 400;
	PistolAmmo = 200;
	SMGAmmo = 300;
	ShotgunAmmo = 80;
	HeavyAmmo = 12;
}


// Called when the game starts
void UWeaponComponent::BeginPlay()
{
	Super::BeginPlay();

	//At the moment, weapons would start off with their specific type at max ammo;
	MaxRifleAmmo = RifleAmmo;
	MaxPistolAmmo = PistolAmmo;
	MaxSMGAmmo = SMGAmmo;
	MaxShotgunAmmo = ShotgunAmmo;
	MaxHeavyAmmo = HeavyAmmo;

	// ...
	//SpawnInitialWeapons();
}


void UWeaponComponent::SpawnInitialWeapons()
{
	/*for (TSubclassOf<AWeaponBase> _Weapon : StartingWeapons) 
	{
		//Spawn Weapons

	}*/

	//Checking to ensure we do have weapons to spawn
	if (StartingWeapons.Num() == 0)
	{
		UE_LOG(LogTemp, Log, TEXT("WeaponComponent::SpawnInitialWeapons - Starting Weapons array length 0, Can Not Spawn Weapons"));
		return;
	}
	UE_LOG(LogTemp, Log, TEXT("WeaponComponent::SpawnInitialWeapons - Spawning Weapons"));

	//Making sure that we do in fact have an owner for this component... if this fails, start praying.
	AActor* _Owner = GetOwner();
	if (_Owner == nullptr) {
		UE_LOG(LogTemp, Log, TEXT("WeaponComponent::SpawnInitialWeapons - Owner Null, not spawning weapons"));
		return;
	}

	//Getting owner transform for spawn params
	FTransform _transform = _Owner->GetTransform();

	//Setting up spawn params
	FActorSpawnParameters _spawnParams;
	_spawnParams.bNoFail = true;
	_spawnParams.Owner = _Owner;

	//Casting to pawn to get owner
	APawn* Pawn = Cast<APawn>(_Owner);
	if (Pawn) {
		_spawnParams.Instigator = Pawn;
	}

	for (int i = 0; i < StartingWeapons.Num(); i++)
	{
		//Spawning weapon
		AWeaponBase* SpawnedWeapon = GetWorld()->SpawnActor<AWeaponBase>(StartingWeapons[i], _transform, _spawnParams);
		//Adding spawned weapon to array of owned weapons
		Weapons.Add(SpawnedWeapon);

		SpawnedWeapon->WeaponComp = this;

		//hiding each weapon
		SpawnedWeapon->SetActorHiddenInGame(true);

		//Attaching weapon
		//SpawnedWeapon->AttachToComponent(AttachPoint, 
		//	FAttachmentTransformRules::SnapToTargetNotIncludingScale, 
		//	AttachPointName);
		SpawnedWeapon->AttachWeapon(AttachPoint, TPAttachPoint, AttachPointName);

		//If this is the first spawned weapon, set it as the equipped weapon
		if (i == 0) {
			CurrentWeapon = SpawnedWeapon;
			CurrentWeapon->SetActorHiddenInGame(false);
			UE_LOG(LogTemp, Log, TEXT("WeaponComponent::SpawnInitialWeapons - Set Default Current Weapon"));
		}
	}

	UE_LOG(LogTemp, Log, TEXT("WeaponComponent::SpawnInitialWeapons - Finished Spawning"));
}

//Getting the pointer value of the ammo, so that we can modify it.
//I totally could have just made a function that just does this, and not done pointers, but whatever, I learned stuff
int* UWeaponComponent::GetAmmoPointer(EWeaponType GunType)
{
	switch (GunType)
	{
	case EWeaponType::Pistol:
		return &PistolAmmo;
		break;

	case EWeaponType::Rifle:
		return &RifleAmmo;
		break;

	case EWeaponType::SMG:
		return &SMGAmmo;
		break;

	case EWeaponType::Shotgun:
		return &ShotgunAmmo;
		break;

	case EWeaponType::Heavy:
		return &HeavyAmmo;
		break;

	default :
		return &RifleAmmo;
		break;
	}
}

// Called every frame
void UWeaponComponent::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	// ...
}

void UWeaponComponent::SwitchWeapon(bool increase)
{
	//Iterate through Weapons
	UE_LOG(LogTemp, Log, TEXT("WeaponComponent::SwitchWeapon - Change Weapon Called"));

	if (Weapons.Num() <= 1) {
		UE_LOG(LogTemp, Log, TEXT("WeaponComponent::SwitchWeapon - 1 or fewer weapons, switch cancelled"));
		return;
	}

	//Stop firing weapon
	CurrentWeapon->TriggerReleased();

	UE_LOG(LogTemp, Log, TEXT("WeaponComponent::SwitchWeapon - Current Index %d"), currentWeaponIndex);
	//currentWeaponIndex = increase ? currentWeaponIndex++ : currentWeaponIndex--;
	if (increase) {
		currentWeaponIndex++;
	}
	else {
		currentWeaponIndex--;
	}

	UE_LOG(LogTemp, Log, TEXT("WeaponComponent::SwitchWeapon - Updated Index %d"), currentWeaponIndex);

	if (increase)
	{
		if (currentWeaponIndex == Weapons.Num())
		{
			currentWeaponIndex = 0;
		}
	}
	else
	{
		if (currentWeaponIndex < 0)
		{
			currentWeaponIndex = Weapons.Num() - 1;
		}
	}
	UE_LOG(LogTemp, Log, TEXT("WeaponComponent::SwitchWeapon - Index after Adjusting %d"), currentWeaponIndex);

	if (Weapons[currentWeaponIndex])
	{
		CurrentWeapon->CeaseFire();
		CurrentWeapon->SetActorHiddenInGame(true);

		CurrentWeapon = Weapons[currentWeaponIndex];
		CurrentWeapon->SetActorHiddenInGame(false);
	}

	OnWeaponSwitched.Broadcast();
}

void UWeaponComponent::AddWeapon(AWeaponBase* WeaponToAdd)
{
	//Maybe comment this out, if doing randomized weapons
	for (int i = 0; i < Weapons.Num(); i++)
	{
		if (Weapons[i] != nullptr) 
		{
			if (Weapons[i] == WeaponToAdd)
			{
				return;
			}
		}
	}
	//Didn't trigger the return, so we don't already have this weapon

	WeaponToAdd->AttachToComponent(AttachPoint, FAttachmentTransformRules::SnapToTargetNotIncludingScale, AttachPointName);

	//Telling it where to draw ammo from
	WeaponToAdd->WeaponComp = this;

	//Add singular weapon, check name, or some other identifier for the weapon to make sure we're not adding the same weapon
	//If weapons are decidedly randomized, this shouldn't be an issue
}

void UWeaponComponent::GetCurrentWeapon(AWeaponBase*& Weapon)
{
	if (CurrentWeapon != nullptr) {
		Weapon = CurrentWeapon;
		return;
	}

	Weapon = nullptr;
}

void UWeaponComponent::GetAmmoForType(EWeaponType GunType, int& Ammo)
{
	switch (GunType)
	{
	case EWeaponType::Pistol:
		Ammo = PistolAmmo;
		break;
	case EWeaponType::Rifle:
		Ammo = RifleAmmo;
		break;
	case EWeaponType::SMG:
		Ammo = SMGAmmo;
		break;
	case EWeaponType::Shotgun:
		Ammo = ShotgunAmmo;
		break;
	case EWeaponType::Heavy:
		Ammo = HeavyAmmo;
		break;

	default: Ammo = -1;
		break;
	}
}

void UWeaponComponent::GetMaxAmmoForType(EWeaponType GunType, int& Ammo)
{
	switch (GunType)
	{
	case EWeaponType::Pistol:
		Ammo = MaxPistolAmmo;
		break;
	case EWeaponType::Rifle:
		Ammo = MaxRifleAmmo;
		break;
	case EWeaponType::SMG:
		Ammo = MaxSMGAmmo;
		break;
	case EWeaponType::Shotgun:
		Ammo = MaxShotgunAmmo;
		break;
	case EWeaponType::Heavy:
		Ammo = MaxHeavyAmmo;
		break;

	default: Ammo = -1;
		break;
	}
}

void UWeaponComponent::FireAmmo(EWeaponType GunType, int& AmmoRemaining)
{

	int* ammo = GetAmmoPointer(GunType);

	//() parens keep us from dereferencing the pointer
	//or do *ammo -= (value) for example, *ammo -= 1
	(*ammo)--;

	//output the value remaining
	AmmoRemaining = *ammo;
}

void UWeaponComponent::GetAmmoForCurrentGun(int& Ammo)
{
	if (CurrentWeapon == nullptr) {
		Ammo = -1;
		return;
	}

	int _ammo = -1;

	EWeaponType _gun;
	CurrentWeapon->GetWeaponType(_gun);

	GetAmmoForType(_gun, _ammo);
}

void UWeaponComponent::FireWeapon()
{
	if (CurrentWeapon != nullptr) 
	{
		CurrentWeapon->TriggerPulled();
	}
}

void UWeaponComponent::CeaseFire()
{
	if (CurrentWeapon != nullptr)
	{
		CurrentWeapon->TriggerReleased();
		//CurrentWeapon->CeaseFire();
	}
}

void UWeaponComponent::SwitchFire()
{
	if (CurrentWeapon != nullptr) {
		CurrentWeapon->SwitchFireMode();
	}
}

void UWeaponComponent::PlayWeaponAnimations(UAnimMontage* FPAnim, UAnimMontage* TPAnim, float rateModifier)
{
	if (AttachPoint != nullptr)
	{
		if (FPAnim)
		{
			float _PlayRate = 0.f;
			_PlayRate = rateModifier / FPAnim->GetPlayLength();

			AttachPoint->GetAnimInstance()->Montage_Play(FPAnim, _PlayRate);
			//sAttachPoint->PlayAnimation(FPAnim, false);
		}
	}

	if (TPAttachPoint)
	{
		if (TPAnim != nullptr)
		{
			float _PlayRate = 0.f;
			_PlayRate = rateModifier / TPAnim->GetPlayLength();

			TPAttachPoint->GetAnimInstance()->Montage_Play(TPAnim, _PlayRate);
			//TPAttachPoint->PlayAnimation(TPAnim, false);
		}
	}
}