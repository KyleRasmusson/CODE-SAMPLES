// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Systems/Weapons/WeaponBase.h"
#include "WeaponTrace.generated.h"

/**
 * 
 */

class UParticleSystem;

UCLASS()
class AWeaponTrace : public AWeaponBase
{
	GENERATED_BODY()

protected:

	UPROPERTY(EditDefaultsOnly, Category = "Weapon|Effects")
	UParticleSystem* ShotTrace;

	void ProjectileLogic() override;

	void TraceEffects(FVector TracePoint);
	
};