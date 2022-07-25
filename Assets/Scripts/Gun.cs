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
    public GameObject magazineObject;
    [SerializeField] private GameObject emptyMagazineObject;
    public int bulletsLeft;
    [SerializeField] private bool isAutomatic;
    [SerializeField] private float refireRate = 1f;
    [SerializeField] private int startingBulletCount;
    [SerializeField] private float dampTime = 0.2f;
    [SerializeField] private float impactForce = 2f;

    public Player wielder = null; //Who is holding this weapon

    [Header("Reloading")]
    private Vector3 randomVector3;

    [Header("Dropping and Throwing")]
    [SerializeField] private float dropRotationForce = 50f;
    [SerializeField] private float dropLiftForce = 3f;
    [SerializeField] private float dropSideForce = 5f;
    [SerializeField] private float throwForce = 1f;
    [SerializeField] private float throwMagForce = 1f;
    [SerializeField] private float throwMagRotForce = 3f;
    [SerializeField] private float throwMagTransitionSpeed = 1f;

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

    public void GunShoot() 
    {

        if (bulletsLeft == 0)
        {
            Debug.Log("No bullets!");
            return;
        }

            Debug.Log("Bang!");
        bulletsLeft--;

        if (Physics.Raycast(bulletOrigin.position, bulletOrigin.forward, out RaycastHit hitInfo, Mathf.Infinity, hitMask))
        {
            Debug.Log(hitInfo.collider.name);
            GameObject newhole = Instantiate(bulletHole, hitInfo.point, Quaternion.FromToRotation(bulletHole.transform.forward, -hitInfo.normal), hitInfo.collider.transform);
            newhole.transform.position += hitInfo.normal.normalized * 0.001f;
            newhole.transform.Rotate(Vector3.forward, Random.Range(0f, 360f));
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

        if (bulletsLeft == 0)
        {
            Debug.Log("Click!");
            if (magazineObject.activeSelf)
            {
                GameObject discardedmag = Instantiate(emptyMagazineObject, magazineObject.transform.position, magazineObject.transform.rotation);
                discardedmag.GetComponent<Rigidbody>().AddRelativeTorque(randomVector3 * dropRotationForce);
                discardedmag.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(1f, 0, 0) * dropSideForce);
                magazineObject.SetActive(false);
            }
            return;
        }

        StartCoroutine(AutomaticRefire());

    }

    private IEnumerator AutomaticRefire()
    {

        yield return new WaitForSeconds(refireRate);
        if (Input.GetKey(fireKey)) GunShoot();

        yield break;

    }

    public void DropGun()
    {
        transform.parent = null;
        wielder = null;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (magazineObject.activeSelf) magazineObject.AddComponent<BoxCollider>();
        gameObject.GetComponent<BoxCollider>().enabled = true;
        rb.isKinematic = false;
        rb.AddTorque(new Vector3(0.3f, 0.3f, 0.2f) * dropRotationForce);
        rb.AddRelativeForce(new Vector3(dropSideForce, dropLiftForce, 0) * dropRotationForce);
    }

    public void ThrowGun()
    {
        Vector3 trajectoryVector = bulletOrigin.forward + Vector3.up * 0.2f;
        transform.parent = null;
        wielder = null;
        Rigidbody gunRB = gameObject.GetComponent<Rigidbody>();
        if (magazineObject.activeSelf) magazineObject.AddComponent<BoxCollider>();
        gameObject.GetComponent<BoxCollider>().enabled = true;
        gunRB.isKinematic = false;
        gunRB.AddRelativeTorque(RandVector3() * dropRotationForce);
        gunRB.AddForce(trajectoryVector * throwForce, ForceMode.Impulse);
    }

    public void ThrowMagazine()
    {
        magazineObject.SetActive(false);
        bulletsLeft = 0;
        GameObject thrownMag = Instantiate(emptyMagazineObject, magazineObject.transform.position, magazineObject.transform.rotation);
        Rigidbody magRB = thrownMag.GetComponent<Rigidbody>();
        StartCoroutine(MagDampAndThrow(thrownMag, magRB));
    }

    private IEnumerator MagDampAndThrow(GameObject thrownMag, Rigidbody magazineRigidBody)
    {
        
        Vector3 currentVel = Vector3.zero;
        magazineRigidBody.isKinematic = true;
        float localTSpeed = throwMagTransitionSpeed;

        while (thrownMag.transform.position != wielder.magThrowPosition.position)
        { 
            thrownMag.transform.position = Vector3.SmoothDamp(thrownMag.transform.position,
                wielder.magThrowPosition.position, ref currentVel, localTSpeed * 0.1f);
            localTSpeed -= 0.0004f; //Over time, the damp speed gets more and more aggressive. 
            if (localTSpeed < 0.0005f)
            {
                Debug.Log("Magazine has been waiting to throw for too long, throwing now.");
                goto JustThrowIt; //If it takes too long, break out by using this label
            }
            yield return null;
        }
        JustThrowIt:
        Vector3 trajectoryVector = bulletOrigin.forward + Vector3.up * 0.2f;
        magazineRigidBody.isKinematic = false;
        magazineRigidBody.AddForce(trajectoryVector * throwMagForce, ForceMode.Impulse);
        magazineRigidBody.AddTorque(RandVector3() * throwMagRotForce, ForceMode.Impulse);
        Debug.Log("Player threw the magazine directly from their gun!");
        yield break;
    }

    private Vector3 RandVector3()
    {
        randomVector3 = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
        return randomVector3;
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

    //Todo This functionality relates to the player, not to the weapon. Better off moving the spherecast part to the player and keeping the updates to the gun here.
    public void ReloadWeapon() 
    {
        Debug.Log("Trying to Reload...");
        if (Physics.SphereCast(wielder.Cam.position, wielder.pickupRadius, bulletOrigin.forward, out RaycastHit hitInfo, wielder.interactDistance, myMagsLayer))
        {
            if (bulletsLeft > 0)
            {
                Debug.Log("Discarding previous Magazine...");
                Instantiate(emptyMagazineObject, magazineObject.transform.position, magazineObject.transform.rotation);
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
