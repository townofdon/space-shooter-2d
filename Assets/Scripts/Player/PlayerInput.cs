using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    void Awake() {
        Layers.Init();
        Debug.Log(Layers.UI);
        Debug.Log(Layers.Water);
    }
}
