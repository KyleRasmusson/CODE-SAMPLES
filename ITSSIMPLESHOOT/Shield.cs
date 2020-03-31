using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

    [SerializeField]
    MeshRenderer ShieldMesh;

    [SerializeField] float initialShieldDelay = 0.25f;

    void Awake()
    {
        Color newColor = ShieldMesh.material.color;
        newColor.a = 0;
        ShieldMesh.material.color = newColor;

        //Override shield time for player
        Controller controller = GetComponent<Controller>();
        if (controller) initialShieldDelay = controller.GetPowerup.GetInvulnerabilityTime - 0.5f;
    }

    public void StartShield()
    {
        ShieldMesh.enabled = true;
        Color newColor = ShieldMesh.material.color;
        newColor.a = 0;
        ShieldMesh.material.color = newColor;

        InvokeRepeating("ShowShield", 0, .01f);
        //InvokeRepeating("ShieldAlpha", initialShieldDelay, .01f);
        InvokeRepeating("RotateShield", 0, .015f);
    }

    void HideShield()
    {
        CancelInvoke("RotateShield");
        CancelInvoke("ShieldAlpha");
        ShieldMesh.enabled = false;
        //Debug.Log("Should hide shield");
    }

    void ShowShield()
    {
        //Debug.Log("Should show shield");
        Color newColor = ShieldMesh.material.color;
        newColor.a += .05f;
        ShieldMesh.material.color = newColor;

        if(newColor.a >= 1)
        {
            CancelInvoke("ShowShield");
            InvokeRepeating("ShieldAlpha", initialShieldDelay, .01f);
        }
    }

    void ShieldAlpha()
    {
        //Debug.Log("Should start to hide shield");
        Color newColor = ShieldMesh.material.color;
        newColor.a -= .02f;
        ShieldMesh.material.color = newColor;

        if (newColor.a <= 0)
        {
            HideShield();
        }
    }

    void RotateShield()
    {
        Vector3 newRot = ShieldMesh.gameObject.transform.localRotation.eulerAngles;

        newRot.y += 1;

        ShieldMesh.gameObject.transform.localRotation = Quaternion.Euler(newRot);
    }
}
