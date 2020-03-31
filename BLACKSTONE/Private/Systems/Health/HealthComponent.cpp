// Fill out your copyright notice in the Description page of Project Settings.


#include "HealthComponent.h"

// Sets default values for this component's properties
UHealthComponent::UHealthComponent()
{
	Health = 100;

	// ...
}


// Called when the game starts
void UHealthComponent::BeginPlay()
{
	Super::BeginPlay();

	maxHealth = Health;

	OnHealthChanged.Broadcast(Health, maxHealth, Health / maxHealth);

	AActor* _Owner = GetOwner();
	if (_Owner != nullptr) {
		_Owner->OnTakeAnyDamage.AddDynamic(this, &UHealthComponent::TakeDamage);
	}
	// ...
	
}

void UHealthComponent::TakeDamage(AActor* DamagedActor, float Damage, const class UDamageType* DamageType, class AController* InstigatedBy, AActor* DamageCauser)
{
	if (bIsDead || Damage <= 0)
	{
		return;
	}

	Health -= Damage;

	if (Health <= 0)
	{
		Health = 0;
		bIsDead = true;

		OnDeath.Broadcast(DamageCauser);
		//return;
	}

	OnHealthChanged.Broadcast(Health, maxHealth, Health / maxHealth);
	OnDamaged.Broadcast(Health, Damage, DamageCauser);

}

void UHealthComponent::AddHealth(AActor* HealedActor, float Healing, const class UDamageType* HealthType, class AController* InstigatedBy, AActor* HealingCauser)
{
	if (bIsDead)
	{
		return;
	}

	Health += Healing;

	if (Health >= maxHealth)
	{
		Health = maxHealth;
	}

	OnHealthChanged.Broadcast(Health, maxHealth, Health / maxHealth);
	OnDamaged.Broadcast(Health, Healing, HealingCauser);
}
