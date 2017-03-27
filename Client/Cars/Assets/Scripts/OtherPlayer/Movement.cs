using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {


    public void Move(Vector3 position)
    {
        transform.position = position;
    }

    public void Turn(Quaternion rot)
    {
        Rigidbody rigid = GetComponent<Rigidbody>();
        rigid.MoveRotation(rot);
    }
}
