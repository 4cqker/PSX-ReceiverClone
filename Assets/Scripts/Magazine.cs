using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magazine : MonoBehaviour
{
    public int ammoCount;
    public bool isEmpty = false;
    void Start()
    {
        if (!isEmpty) ammoCount = Random.Range(6, 10);
    }

}
