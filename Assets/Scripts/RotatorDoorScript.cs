using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorDoorScript : MonoBehaviour
{

    void Update()
    {
        transform.Rotate(new Vector3(30f,15f,45f)*Time.deltaTime);      
    }
}
