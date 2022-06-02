using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRCamera : MonoBehaviour
{
    public static XRCamera Instance;

    void Awake()
    {
        Instance = this;
    }
}
