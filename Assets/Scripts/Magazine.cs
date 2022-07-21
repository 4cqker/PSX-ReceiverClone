using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magazine : MonoBehaviour
{
    public int ammoCount;
    void Start()
    {
        ammoCount = Random.Range(6, 10);
    }
}
