using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPowerup : MonoBehaviour {

    [SerializeField] EPowerupType PowerupType = EPowerupType.INFINITEAMMO;

    [SerializeField] private new MeshRenderer renderer;

    float lifetime = 10.0f;

    void Start()
    {
        if (renderer)
        {
            InvokeRepeating("Hide", lifetime, .01f);
        }

    }

    void OnTriggerEnter(Collider other)
    {
        PowerupComp powerup = other.GetComponent<PowerupComp>();

        if (!powerup) return;

        GivePowerup(powerup);
    }

    void GivePowerup(PowerupComp comp)
    {
        if (!comp) return;

        if (comp.GetPowerupActive) return;

        switch (PowerupType)
        {
            case EPowerupType.INFINITEAMMO:
                comp.EnableInfiniteAmmo();
                break;

            case EPowerupType.INVULNERABILITY:
                comp.EnableInvulnerability();
                break;

            case EPowerupType.SPEEDBOOST:
                comp.EnableBoost();
                break;
        }

        GameManager.UpdatePowerupCount(false);

        Destroy(gameObject);
    }

    float alphaValue;

    void Hide()
    {
        Color meshColor = renderer.material.color;
        meshColor.a -= .01f;

        renderer.material.color = meshColor;

        if(meshColor.a <= 0)
        {
            CancelInvoke();

            GameManager.UpdatePowerupCount(false);

            Destroy(gameObject);
        }
    }
}
