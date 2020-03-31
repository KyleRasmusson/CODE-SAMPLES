using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    [SerializeField] Transform ToFollow;

    [SerializeField] Transform ToMove;

    [SerializeField] float speed = 0.1f;

    [SerializeField] Vector2 CameraSize = new Vector2(12, 18);

    float lastAlphaValue;

    [SerializeField] float sizeDecreaseRate = 1.0f;

    void Awake()
    {
        //We take the camera and make it part of the parent prefab, but remove it on spawn
        ToMove.transform.parent = null;

        InvokeRepeating("ModifyAlphaValue", 0, .01f);
    }

    public void MoveCamera()
    {
        Vector3 MoveToPosition = ToFollow.position;
        MoveToPosition.y = ToMove.position.y;

        ToMove.position = Vector3.Lerp(ToMove.position, MoveToPosition, speed);
    }

    public void ResizeCamera(float alpha)
    {
        if(alpha > lastAlphaValue)
        {
            lastAlphaValue = alpha;
        }

        Camera.main.orthographicSize = Mathf.SmoothStep(CameraSize.x, CameraSize.y, lastAlphaValue);
    }

    void ModifyAlphaValue()
    {
        lastAlphaValue -= .01f * sizeDecreaseRate;
    }

    void OnDestroy()
    {
        CancelInvoke();
    }

    /*
    void FixedUpdate()
    {
        lastAlphaValue -= Time.fixedDeltaTime * sizeDecreaseRate;
    }
    */
}
