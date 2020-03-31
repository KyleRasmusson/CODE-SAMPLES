// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "WeaponBase.generated.h"

UENUM(BlueprintType)
enum class EFireMode : uint8
{
	Semi,
	Burst,
	Auto
};

UENUM(BlueprintType)
enum class EWeaponType : uint8
{
	Pistol,
	Rifle,
	SMG,
	Shotgun,
	Heavy
};

#pragma region WeaponVisualData
/*
USTRUCT()
struct  SCosmetics
{

};*/

UENUM(BlueprintType)
enum class EBarrelList : uint8
{
	Barrel01,
	Barrel02,
	Barrel02b,
	Barrel03,
	Barrel03b,
	Barrel04,
	Barrel04b,
	Barrel04c
};

UENUM(BlueprintType)
enum class EButtList : uint8
{
	Butt01,
	Butt01b,
	Butt02,
	Butt03,
	Butt04,
	Butt05
};

UENUM(BlueprintType)
enum class EPistolGripList : uint8
{
	PGrip01,
	PGrip02,
	PGrip03,
	PGrip03b,
	PGrip04
};

UENUM(BlueprintType)
enum class EForeGripList : uint8
{
	FGrip01,
	FGrip02,
	FGrip03,
	FGrip03b
};

UENUM(BlueprintType)
enum class EHandguardList : uint8
{
	Handguard01,
	Handguard02,
	Handguard03,
	Handguard03b,
	Handguard04,
	Handguard04b,
	Handguard05,
	Handguard05b,
	Handguard06,
	Handguard07,
	Handguard07b,
	Handguard08,
	Handguard09
};

UENUM(BlueprintType)
enum class EHullList : uint8
{
	Hull01,
	Hull01a,
	Hull02,
	Hull03,
	Hull04,
	Hull04b,
	Hull05,
	Hull06
};

UENUM(BlueprintType)
enum class EMagMountList : uint8
{
	MagMount01,
	MagMount02,
	MagMount03
};

UENUM(BlueprintType)
enum class EMagList : uint8
{
	MagMed01,
	MagMed02,
	MagMed03,
	MagMed04,
	MagMed05,
	MagSmall01,
	MagSmall02
};

UENUM(BlueprintType)
enum class EMuzzleList : uint8
{
	Muzzle01,
	Muzzle02,
	Muzzle03,
	Muzzle04,
	Muzzle04b,
	Muzzle05,
	Muzzle06
};

UENUM(BlueprintType)
enum class ESightList : uint8
{
	Scope01,
	Scope02,
	Scope03,
	Scope04
};

#pragma endregion WeaponVisualData

class USkeletalMeshComponent;
class USphereComponent;
class UDamageType;
class USoundCue;
class UParticleSystem;
class UWeaponComponent;
class USkeletalMesh;
class UAnimMontage;

UCLASS()
class BLACKSTONE_API AWeaponBase : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	AWeaponBase();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapon|Firing")
	EWeaponType WeaponType;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapon|Firing")
	EFireMode FireMode;

	float timeTillCanFire;
	float adjustedFireRate;

	bool bTriggerPulled;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Firing")
	float Damage;

	TSubclassOf<UDamageType> DamageClass;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Firing")
	float FireRate;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Firing")
	int NumberProjectiles;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Firing")
	bool bHasSemi;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Firing")
	bool bHasBurst;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Firing")
	bool bHasAuto;

	FTimerHandle T_FiringTimer;
	FTimerHandle T_BurstFireDelayTimer;

	//UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapon|Firing")
	//int CurrentMag;

	//UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapon|Firing")
	//int MagSize;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Firing")
	int RoundsInBurst;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Effects")
	USoundCue* ShotSFX;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Effects")
	USoundCue* EmptyClickSFX;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Effects")
	UParticleSystem* MuzzleFlash;

	int currentRoundInBurst;
	bool bIsOnBurstDelay;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapon|Firing")
	float delayBetweenBursts;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Firing")
	float ShotDistance;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Firing")
	float Accuracy;

	float RecoilAlpha;
	float RecoilDelta;
	float RecoilTimer;
	float RecoilDivider;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Firing|Accuracy")
	FVector2D PitchRecoil;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Firing|Accuracy")
	FVector2D YawRecoil;

	float FRecoilAmount;
	float PitchRecoilAmount;
	float YawRecoilAmount;

	FTimerHandle T_RecoilTimer;
	FTimerHandle T_RecoilResetTimer;

	virtual void DisperseRecoil();
	void CalculateRecoil();
	void AllowRecoilReset();

	virtual void FiringLogic();
	virtual void ProjectileLogic();

	virtual void FiringEffects();
	virtual void MagEmpty();

	virtual bool CanFire();

	void BurstDelayOver();

#pragma region WeaponVisuals

	void AttachMeshes();

	UFUNCTION(BlueprintCallable)
	void SetMeshes();

	UFUNCTION(BlueprintCallable)
	void SocketMeshes();

	UFUNCTION(BlueprintCallable)
	void SetMeshVisibility();

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals")
		EBarrelList WeaponBarrel;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals")
		EButtList WeaponButt;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals")
		EPistolGripList PistolGrip;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals")
		EForeGripList ForeGrip;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals")
		EHandguardList WeaponHandGuard;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals")
		EHullList Receiver;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals")
		EMagList Magazine;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals")
		EMagMountList MagazineMount;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals")
		EMuzzleList WeaponMuzzle;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals")
		ESightList WeaponSight;

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals|Data")
		TMap<EBarrelList, USkeletalMesh*> Barrells;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals|Data")
		TMap<EButtList, USkeletalMesh*> Butts;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals|Data")
		TMap<EPistolGripList, USkeletalMesh*> PistolGrips;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals|Data")
		TMap<EForeGripList, USkeletalMesh*> ForeGrips;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals|Data")
		TMap<EHandguardList, USkeletalMesh*> HandGuards;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals|Data")
		TMap<EHullList, USkeletalMesh*> Hulls;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals|Data")
		TMap<EMagMountList, USkeletalMesh*> MagMounts;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals|Data")
		TMap<EMagList, USkeletalMesh*> Mags;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals|Data")
		TMap<EMuzzleList, USkeletalMesh*> Muzzles;
	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Visuals|Data")
		TMap<ESightList, USkeletalMesh*> Sights;

#pragma endregion WeaponVisuals


#pragma region Meshes

	//FP Meshes
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* WeaponMesh;

	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* HullMesh;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* HandGuard;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* Barrel;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* Muzzle;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* GripAttachment;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* Butt;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* MagMount;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* Mag;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* WeaponScope;
	//GripIKLocator was here, put back here if you decide to make a "Getter" function instead;

	//EndFP Meshes

	UPROPERTY(VisibleDefaultsOnly, Category = "Components")
	USkeletalMeshComponent* TPMesh;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* TPHull;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* TPHandGuard;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* TPBarrel;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* TPMuzzle;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* TPGripAttachment;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* TPButt;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* TPMagMount;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* TPMag;
	UPROPERTY(VisibleDefaultsOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* TPScope;

	UPROPERTY(VisibleDefaultsOnly, Category = "Components")
	USphereComponent* MuzzlePoint;


#pragma endregion

public:	

	UPROPERTY(VisibleDefaultsOnly, BlueprintReadOnly, Category = "Components|FPWeaponMesh")
	USkeletalMeshComponent* GripIKLocator;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapon|Animation")
	UAnimMontage* FiringMontageFP;
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapon|Animation")
	UAnimMontage* FiringMontageTP;

	void AttachWeapon(USceneComponent* AttachPoint, USceneComponent* AttachPointTP, FName AttachName);

	// Called every frame
	virtual void Tick(float DeltaTime) override;

	UFUNCTION(BlueprintCallable)
	void TriggerPulled();

	UFUNCTION(BlueprintCallable)
	void TriggerReleased();

	UFUNCTION(BlueprintCallable)
	void CeaseFire();

	UFUNCTION(BlueprintCallable)
	void SetFireMode(EFireMode newmode);

	UFUNCTION(BlueprintCallable)
	void SwitchFireMode();

	UFUNCTION(BlueprintPure)
	void GetWeaponType(EWeaponType& GunType);

	UWeaponComponent* WeaponComp;

};
