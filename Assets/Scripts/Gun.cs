using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private Transform bulletOrigin;
    [SerializeField] private GameObject bulletHole;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private LayerMask myMagsLayer;
    [SerializeField] private string enemyLayer;
    [SerializeField] private string objectLayer;
    [SerializeField] private GameObject magazineObject;
    [SerializeField] private int bulletsLeft;
    [SerializeField] private int startingBulletCount;
    [SerializeField] private float dampTime = 0.2f;
    [SerializeField] private float impactForce = 2f;

    public Player wielder; //Who is holding this weapon

    [Header("Reloading")]
    private Vector3 randomVector3;

    [Header("Dropping and Throwing")]
    [SerializeField] private float dropRotationForce = 50f;
    [SerializeField] private float throwForce = 1f;

    private void Start() 
    {
        if (!bulletOrigin) bulletOrigin = transform;
        bulletsLeft = startingBulletCount;
        if (bulletsLeft > 0) magazineObject.SetActive(true);
        if (!transform.parent) 
        {
            gameObject.GetComponent<BoxCollider>().enabled = true;
            transform.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    void Update() //
    {
        //if (Input.GetKeyDown(reloadButton)) Reload();
    }

    

    public void GunShoot() 
    {

        if (bulletsLeft == 0)
        {
            Debug.Log("Click!");
            magazineObject.SetActive(false);
            return;
        }

        Debug.Log("Bang!");
        bulletsLeft--;

        if (Physics.Raycast(bulletOrigin.position, bulletOrigin.forward, out RaycastHit hitInfo, Mathf.Infinity, hitMask))
        {
            Debug.Log(hitInfo.collider.name);
            GameObject newhole = Instantiate(bulletHole, hitInfo.point, Quaternion.FromToRotation(bulletHole.transform.forward, -hitInfo.normal), hitInfo.collider.transform);
            newhole.transform.position += hitInfo.normal.normalized * 0.001f;
            if (hitInfo.transform)
            {

                if (hitInfo.collider.gameObject.GetComponent<Rigidbody>()) 
                    hitInfo.collider.gameObject.GetComponent<Rigidbody>().AddForce(bulletOrigin.forward.normalized * impactForce, ForceMode.Impulse);
                    
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
                    //Well I'll be damned.
                }
            }
        }
    }

    public void DropGun()
    {
        transform.parent = null;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (magazineObject.activeSelf) magazineObject.AddComponent<BoxCollider>();
        gameObject.GetComponent<BoxCollider>().enabled = true;
        rb.isKinematic = false;
        rb.AddTorque(randomVector3 * dropRotationForce);
    }

    public void ThrowWeapon()
    {
        Vector3 trajectoryVector = bulletOrigin.forward + Vector3.up * 0.2f;
        transform.parent = null;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (magazineObject.activeSelf) magazineObject.AddComponent<BoxCollider>();
        gameObject.GetComponent<BoxCollider>().enabled = true;
        rb.isKinematic = false;
        rb.AddTorque(randomVector3 * dropRotationForce);
        rb.AddForce(trajectoryVector * throwForce, ForceMode.Impulse);
    }

    /*private void PickupGun() //
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        StartCoroutine(GunDamp(rb));
    }

    private IEnumerator GunDamp(Rigidbody rb)
    {

        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        Vector3 currentVelocity = Vector3.zero;
        while (transform.position != gunOriginPoint.transform.position)
        {
            transform.position = Vector3.SmoothDamp(transform.position, gunOriginPoint.transform.position, ref currentVelocity, dampTime);
            Debug.Log("GunDamping Loop");

            yield return null;
        }

        while (transform.rotation != gunOriginPoint.transform.rotation)
        {
            transform.rotation =
                Quaternion.Euler(Vector3.SmoothDamp(transform.rotation.eulerAngles,
                gunOriginPoint.transform.rotation.eulerAngles,
                ref currentVelocity, dampTime));

            yield return null;
        }

        transform.parent = bulletOrigin;
        Debug.Log("Finishing GunDamp Loop");
        yield break;
    }*/

    public void ReloadWeapon() 
    {
        Debug.Log("Trying to Reload...");
        if (Physics.SphereCast(wielder.theCamera.position, wielder.pickupRadius, bulletOrigin.forward, out RaycastHit hitInfo, wielder.interactDistance, myMagsLayer))
        {
            if (bulletsLeft > 0)
            {
                Debug.Log("Discarding previous Magazine...");
                //spawn a game object that is the previous mag, it has the amount of bullets you had left
            }
            Debug.Log("Found a Magazine, " + hitInfo.collider.name);
            Magazine newMagazine = hitInfo.collider.GetComponent<Magazine>();
            hitInfo.collider.gameObject.SetActive(false);
            magazineObject.SetActive(true);
            bulletsLeft = newMagazine.ammoCount;
        }

        //If more than one, pick one at random?
    }

}
