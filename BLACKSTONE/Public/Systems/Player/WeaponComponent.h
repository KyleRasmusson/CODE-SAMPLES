// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "Weapons/WeaponBase.h"
#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "WeaponComponent.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnWeaponUpdatedSignature);

class AWeaponBase;
class USkeletalMeshComponent;
class UAnimMontage;
//class EWeaponType;

UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class BLACKSTONE_API UWeaponComponent : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UWeaponComponent();

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	UPROPERTY(EditDefaultsOnly, Category = "Weapons")
	TArray<TSubclassOf<AWeaponBase>> StartingWeapons;

	TArray<AWeaponBase*> Weapons;

	AWeaponBase* CurrentWeapon;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Weapons")
	FName AttachPointName;

	int currentWeaponIndex;
	
	int maxWeapons;

	int MaxRifleAmmo;
	int MaxPistolAmmo;
	int MaxSMGAmmo;
	int MaxShotgunAmmo;
	int MaxHeavyAmmo;

	int* GetAmmoPointer(EWeaponType GunType);

public:	

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapons|Ammo")
		int RifleAmmo;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapons|Ammo")
		int PistolAmmo;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapons|Ammo")
		int SMGAmmo;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapons|Ammo")
		int ShotgunAmmo;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapons|Ammo")
		int HeavyAmmo;

	UPROPERTY(BlueprintReadWrite, Category = "Weapons")
	USkeletalMeshComponent* AttachPoint;

	UPROPERTY(BlueprintReadWrite, Category = "Weapons")
	USkeletalMeshComponent* TPAttachPoint;

	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	void SpawnInitialWeapons();

	void SwitchWeapon(bool increase);

	void AddWeapon(AWeaponBase* WeaponToAdd);

	UFUNCTION(BlueprintPure)
	void GetCurrentWeapon(AWeaponBase*& Weapon);

	UFUNCTION(BlueprintPure)
	void GetAmmoForType(EWeaponType GunType, int& Ammo);

	UFUNCTION(BlueprintPure)
	void GetMaxAmmoForType(EWeaponType GunType, int& Ammo);

	UFUNCTION(BlueprintCallable)
	void FireAmmo(EWeaponType GunType, int& AmmoRemaining);

	UFUNCTION(BlueprintPure)
	void GetAmmoForCurrentGun(int& Ammo);
		
	void FireWeapon();
	void CeaseFire();

	void SwitchFire();

	void PlayWeaponAnimations(UAnimMontage* FPAnim, UAnimMontage* TPAnim, float rateModifier);

	UPROPERTY(BlueprintAssignable)
	FOnWeaponUpdatedSignature OnWeaponSwitched;
};
