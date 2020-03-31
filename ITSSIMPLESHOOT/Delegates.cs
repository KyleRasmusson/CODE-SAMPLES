using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void EventDamaged(int Damage, int CurrentHealth, GameObject Instigator);

public delegate void EventDeath(GameObject Instigator);

public delegate void EventHealed(int Health, int CurrentHealth, GameObject Healer);

public delegate void EventRespawned();

public delegate void EventFullDeath();

public delegate void EventLivesAdded(int curLives);

public delegate void EventPlayerDeath();
public delegate void EventPlayerRespawn();

public delegate void EventGameOver();

public delegate void EventOnWeaponCooldown();

public delegate void EventOnWeaponFired(int ammoCurrent, int ammoMax);

public delegate void EventKillAdded();

public delegate void EventDashUsed(float CooldownAmt);

public delegate void EventPowerupUpdated(float max, float min);
public delegate void EventPowerupAdded(string PowerupName);

public delegate void OnAudioChanged();