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
    [SerializeField] private string enemyLayer;
    [SerializeField] private string objectLayer;
    [SerializeField] private GameObject magazineObject;
    [SerializeField] private int bulletsLeft;
    [SerializeField] private int startingBulletCount;
    [SerializeField] private GameObject gunOriginPoint;
    [SerializeField] private float dampTime = 0.2f;
    [SerializeField] private float impactForce = 2f;

    [Header("Reloading")]
    [SerializeField] private KeyCode reloadButton = KeyCode.R;
    [SerializeField] private LayerMask magazineLayer;
    [SerializeField] private GameObject text;
    [SerializeField] private float pickupRadius;
    [SerializeField] private float interactDistance = 2;
    private Vector3 randomVector3;

    [Header("Dropping and Throwing")]
    [SerializeField] private KeyCode dropGunButton = KeyCode.G;
    [SerializeField] private float dropRotationForce = 50f;
    [SerializeField] private KeyCode throwGunButton = KeyCode.F;
    [SerializeField] private float throwForce = 1f;
    [SerializeField] private KeyCode pickupGunButton = KeyCode.E;

    private void Start() 
    {
        if (!bulletOrigin) bulletOrigin = transform;
        bulletsLeft = startingBulletCount;
        if (bulletsLeft > 0) magazineObject.SetActive(true);
        randomVector3 = new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), Random.Range(-2, 2));
    }

    void Update() //
    {

        if (bulletsLeft == 0) magazineObject.SetActive(false);

        if (Input.GetKeyDown(fireKey) && bulletsLeft > 0) GunShoot();

        if (Input.GetKeyDown(reloadButton)) Reload();

        if (Input.GetKeyDown(dropGunButton)) DropGun();

        if (Input.GetKeyDown(throwGunButton)) ThrowGun();

        if (Input.GetKeyDown(pickupGunButton)) PickupGun();

        //Always checking so I can notify the player with text
        SphereCheck();

    }

    

    private void GunShoot() //
    {
        Debug.Log("Bang!");
        bulletsLeft--;

        if (Physics.Raycast(bulletOrigin.position, bulletOrigin.forward, out RaycastHit hitInfo, Mathf.Infinity, hitMask))
        {
            Debug.Log(hitInfo.collider.name);
            GameObject newhole = Instantiate(bulletHole, hitInfo.point, Quaternion.FromToRotation(bulletHole.transform.forward, -hitInfo.normal), hitInfo.collider.transform);
            newhole.transform.position += hitInfo.normal.normalized * 0.001f;
            if (hitInfo.transform)
            {

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

    private void DropGun() //
    {
        transform.parent = null;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (magazineObject.activeSelf) magazineObject.AddComponent<BoxCollider>();
        gameObject.GetComponent<BoxCollider>().enabled = true;
        rb.isKinematic = false;
        rb.AddTorque(randomVector3 * dropRotationForce);
    }

    private void ThrowGun() //
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

    private void PickupGun() //
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
    }

    private void Reload() 
    {
        Debug.Log("Trying to Reload...");
        if (Physics.SphereCast(bulletOrigin.position, pickupRadius, bulletOrigin.forward, out RaycastHit hitInfo, interactDistance, magazineLayer))
        {
            if (bulletsLeft > 0)
            {
                Debug.Log("Discarding previous Magazine...");
                //debug log "dropping previous magazine"
                //spawn a game object that is the previous mag, it has the amount of bullets you had left
            }
            Debug.Log("Found a Magazine, " + hitInfo.collider.name);
            Magazine newMagazine = hitInfo.collider.GetComponent<Magazine>();
            hitInfo.collider.gameObject.SetActive(false);
            magazineObject.SetActive(true);
            bulletsLeft = newMagazine.ammoCount;
        }

        //If more than one, pick one at random 
        //Disable it DONE
        //Enable the "Magazine" on the gun DONE
        //Set the bullets left in your gun to the amount from the magazine DONE
    }

    private void SphereCheck() //
    {
        if (Physics.SphereCast(bulletOrigin.position, pickupRadius, bulletOrigin.forward, out RaycastHit hitInfo, interactDistance, magazineLayer))
        {
            text.SetActive(true);
        }
        else text.SetActive(false);
    }
}
