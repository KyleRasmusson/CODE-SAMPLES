// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "BlackstoneCharacter.generated.h"

class UCameraComponent;
class UWeaponComponent;
class UHealthComponent;

UCLASS()
class BLACKSTONE_API ABlackstoneCharacter : public ACharacter
{
	GENERATED_BODY()

public:
	// Sets default values for this character's properties
	ABlackstoneCharacter();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

	UPROPERTY(Category = Components, VisibleAnywhere, BlueprintReadWrite)
	USkeletalMeshComponent* FPArms;

	UPROPERTY(Category = Components, VisibleAnywhere, BlueprintReadWrite)
	UCameraComponent* Camera;

	UPROPERTY(Category = Components, VisibleAnywhere, BlueprintReadWrite)
	UWeaponComponent* Weapon;

	UPROPERTY(Category = Components, VisibleAnywhere, BlueprintReadWrite)
	UHealthComponent* HealthComp;

	UPROPERTY(EditDefaultsOnly, Category = "View")
	float PitchCenteringSpeed;

	UPROPERTY(EditDefaultsOnly, Category = "View")
	float YawCenteringSpeed;

	FRotator Recoil;
	FRotator TargetRecoil;
	bool isIncreasingRotation;

	float Pitch;
	float RecoilPitch;

	UPROPERTY(BlueprintReadOnly)
	float Yaw;
	UPROPERTY(BlueprintReadOnly)
	float RecoilYaw;

	void PitchLook(float Value);
	void YawLook(float Value);
	void AdjustYaw(float Value);
	void CameraLook();
	void PlayerRotation();
	void InterpRecoil();

	void MoveForward(float Value);
	void MoveRight(float Value);

	void Fire();
	void CeaseFire();

	void IncrementWeapon();
	void DecrementWeapon();

	void SwitchFireMode();

public:	
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	virtual void GetActorEyesViewPoint(FVector& Location, FRotator& Rotation) const override;

	// Called to bind functionality to input
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;

	UFUNCTION(BlueprintCallable, Category = "View")
	void AddRecoil(FRotator RecoilAmount);

	UFUNCTION(BlueprintCallable, Category = "View")
	void AddViewRecoil(float PitchValue, float YawValue);

	void NoRecoilApplied();

};
