using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private Transform bulletOrigin;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private string enemyLayer;
    [SerializeField] private string objectLayer;

    private void Start()
    {
        if (!bulletOrigin) bulletOrigin = transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(fireKey)) GunShoot();
    }

    private void GunShoot()
    {
        Debug.Log("Bang!");

        if (Physics.Raycast(bulletOrigin.position, bulletOrigin.forward, out RaycastHit hitInfo, Mathf.Infinity, hitMask))
        {
            Debug.Log(hitInfo.collider.name);

            if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer(enemyLayer))
            {
                Destroy(hitInfo.collider.gameObject);
                Debug.Log("Hit target!");
            }
            else if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer(objectLayer))
            {
                Debug.Log("Hit the ground!");
            }
            else
            {
                //idk
            }
        }
    }
}
