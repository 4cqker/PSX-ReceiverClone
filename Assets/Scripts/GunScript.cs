using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunScript : MonoBehaviour
{
    [SerializeField]
    private KeyCode fireKey = KeyCode.Mouse0;

    void Update()
    {
        if (Input.GetKeyDown(fireKey)) GunShoot();
    }

    private void GunShoot()
    {
        Debug.Log("Bang!");
    }
}
