using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    private Transform target;

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = new Vector3(target.position.x,
                transform.position.y, target.position.z);
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
