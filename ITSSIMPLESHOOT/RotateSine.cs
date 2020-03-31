using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Author:
Purpose:
Resources:
*/

public class RotateSine : MonoBehaviour 
{
    [SerializeField] Vector3 StartRot = Vector3.zero, EndRot = Vector3.zero;

    [SerializeField] float rate = 2;

    // FixedUpdate is called at a fixed interval
    void FixedUpdate()
    {
        float _alpha = Mathf.Sin(Time.time * rate);

        float _fixedTime = (_alpha + 1) / 2;

        //Debug.Log("alpha value is " + _fixedTime);

        Vector3 _eulerRot = Vector3.Lerp(StartRot, EndRot, _fixedTime);

        Quaternion _newRot = Quaternion.Euler(_eulerRot);
        transform.rotation = _newRot;
    }

}