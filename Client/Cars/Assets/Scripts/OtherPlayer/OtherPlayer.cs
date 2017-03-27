using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayer : MonoBehaviour {

    public string name;

    private void Start()
    {
        gameObject.name = name;
    }
}
