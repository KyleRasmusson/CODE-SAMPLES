// Fill out your copyright notice in the Description page of Project Settings.


#include "WeaponTrace.h"
#include "Particles/ParticleSystemComponent.h"
#include "Components/SphereComponent.h"
#include "Kismet/GameplayStatics.h"
#include "Kismet/KismetMathLibrary.h"
#include "Particles/ParticleSystem.h"

void AWeaponTrace::ProjectileLogic()
{
	//Trace weapon from pawn eyes (center of screen) to cross-hair location
	AActor* _Owner = nullptr;
	_Owner = GetOwner();

	if (_Owner)
	{
		//Getting Shot location and rotation via actor eyes viewpoint
		FVector EyeLocation;
		FRotator EyeRotation;
		_Owner->GetActorEyesViewPoint(EyeLocation, EyeRotation);

		//Getting forward vector of player
		FVector shotDirection = EyeRotation.Vector();

		//Getting weapon accuracy, currently a cone modifier
		FVector _Accuracy = UKismetMathLibrary::RandomUnitVectorInConeInDegrees(shotDirection, Accuracy);

		//Getting shot direction and distance via converting rotation to vector and multiplying it by the distance
		FVector TraceEnd = EyeLocation + (_Accuracy * ShotDistance);

		//Setting up trace params
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

		FVector _HitLocation = Hit.bBlockingHit ? Hit.ImpactPoint : Hit.TraceEnd;

		TraceEffects(_HitLocation);
	}
}

void AWeaponTrace::TraceEffects(FVector TracePoint) 
{
	if (ShotTrace == nullptr)
	{
		return;
	}

	FVector _StartLocation = MuzzlePoint->GetComponentLocation();

	UParticleSystemComponent* _Beam = nullptr;

	_Beam = UGameplayStatics::SpawnEmitterAtLocation(GetWorld(), ShotTrace, _StartLocation);

	if (_Beam != nullptr)
	{
		_Beam->SetBeamSourcePoint(0, _StartLocation, 0);
		_Beam->SetBeamEndPoint(0, TracePoint);
	}
}