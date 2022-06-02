using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRUIPrefabs : MonoBehaviour
{
    public static XRUIPrefabs Instance;

    public GameObject EntityHoverInfoPrefab;
    public GameObject EventHoverInfoPrefab;

    void Awake()
    {
        Instance = this;
    }
}
