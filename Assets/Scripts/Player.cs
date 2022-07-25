using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    //public Transform mainCamera; //the camera point the player's head is using
    [SerializeField, Tooltip("The position in space attached to the camera that weapons are held at")] 
    private Transform gunHoldTransform;
    [SerializeField, Tooltip("The position in space attached to the camera that weapons are moved to for aiming")] 
    private Transform gunAimTransform;
    [SerializeField, Tooltip("The position in space attached to the camera that magazines move to just before throwing")]
    public Transform magThrowPosition;

    public float aimTime = 4f; //The time it takes for the gun to get to your eye for ADS, roughly.
    public float pickupRadius; //The radius of the sphere cast check
    public float interactDistance = 2; //The length of the raycast for interactions
    [SerializeField, Tooltip("Magazines out in the world should be assigned to this same layer")] private LayerMask magazineLayer;
    [SerializeField, Tooltip("Weapons out in the world should be assigned to this same layer")] private LayerMask weaponLayer;
    [SerializeField, Tooltip("The text that says 'Pick up Magazine' When possible")] private GameObject text1;
    [SerializeField, Tooltip("The text that says 'Pick up Gun' When possible")] private GameObject text2;

    [SerializeField, Tooltip("The gun in hand to be used and fired")] public Gun currentWeapon;
    //[SerializeField, Tooltip("The gun the player starts with")] public GameObject startingWeapon;

    [Header("Keybinds")]
    [SerializeField] private KeyCode pickupWeaponKey = KeyCode.E;
    [SerializeField] private KeyCode dropWeaponKey = KeyCode.G;
    [SerializeField] private KeyCode throwWeaponKey = KeyCode.F;
    [SerializeField] private KeyCode reloadWeaponKey = KeyCode.R;
    [SerializeField] private KeyCode throwMagazineKey = KeyCode.T;
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode aimSightKey = KeyCode.Mouse1;

    public Transform Cam => Camera.main.transform; //Getter for the main camera transform

    private void Update()
    {
        SphereCheck();

        if (Input.GetKeyDown(pickupWeaponKey)) PickupWeapon();

        if (currentWeapon is Gun)
        {
            if (Input.GetKeyDown(fireKey)) currentWeapon.GunShoot();

            if (Input.GetKeyDown(dropWeaponKey)) DropWeapon();

            if (Input.GetKeyDown(reloadWeaponKey)) currentWeapon.ReloadWeapon();

            if (Input.GetKeyDown(throwWeaponKey)) ThrowWeapon();

            if (Input.GetKeyDown(throwMagazineKey)) ThrowMagazine();

            if (Input.GetKeyDown(aimSightKey)) StartCoroutine(AimWeapon());
        }
    }

    private IEnumerator AimWeapon()
    {
        Vector3 currentVelocity = Vector3.zero;
        AimingAgain:
        while (currentWeapon && Input.GetKey(aimSightKey))
        {
            if (currentWeapon.transform.position != gunAimTransform.position)
            {
                currentWeapon.transform.position = Vector3.SmoothDamp(currentWeapon.transform.position, 
                    gunAimTransform.position, ref currentVelocity, aimTime);
            }
            yield return null;
        }
        while (currentWeapon && currentWeapon.transform.position != gunHoldTransform.position)
        {
            currentWeapon.transform.position = Vector3.SmoothDamp(currentWeapon.transform.position,
                    gunHoldTransform.position, ref currentVelocity, aimTime);
            if (Input.GetKey(aimSightKey)) goto AimingAgain; //label shortcut if you start aiming again while lowering the weapon
            yield return null;
        }
        yield break;
    }



    private void PickupWeapon()
    {

        if (Physics.SphereCast(Cam.position, pickupRadius, Cam.forward, out RaycastHit hitInfo, interactDistance, weaponLayer))
        {
            if (hitInfo.transform.tag == "Weapon" && currentWeapon != hitInfo.transform.gameObject)
            {
                if (hitInfo.collider.gameObject.GetComponent<Gun>().wielder == null)
                {
                    if (currentWeapon != null) DropWeapon();

                    Debug.Log("Picking up new weapon, transform name: " + hitInfo.transform.name);
                    currentWeapon = hitInfo.transform.GetComponent<Gun>();
                    currentWeapon.GetComponent<Rigidbody>().isKinematic = true;
                    //currentWeapon.GetComponent<BoxCollider>().enabled = false;

                    BoxCollider[] boxCollidersOnGun = currentWeapon.GetComponentsInChildren<BoxCollider>();
                    foreach (BoxCollider collider in boxCollidersOnGun)
                    {
                        collider.enabled = false;
                    }

                    currentWeapon.transform.parent = Cam;
                    currentWeapon.transform.position = gunHoldTransform.position;
                    currentWeapon.transform.rotation = gunHoldTransform.rotation;

                    currentWeapon.wielder = this;
                }
                    
            }
        }
    }

    private void DropWeapon()
    {
        Gun currentWeaponScript = currentWeapon.GetComponent<Gun>();
        currentWeaponScript.DropGun();
        currentWeapon = null;
    }

    private void ThrowWeapon()
    {
        Gun currentWeaponScript = currentWeapon.GetComponent<Gun>();
        currentWeaponScript.ThrowGun();
        currentWeapon = null;
    }

    private void ThrowMagazine()
    {
        Gun currentWeaponScript = currentWeapon.GetComponent<Gun>();
        if (!currentWeaponScript.magazineObject.activeSelf) return;
        currentWeaponScript.ThrowMagazine();
    }

    private void SphereCheck()
    {
        if (Physics.SphereCast(Cam.position, pickupRadius, Cam.forward, out RaycastHit hitInfo, interactDistance, magazineLayer | weaponLayer))
        {
            if (hitInfo.transform.tag == "Magazine" && currentWeapon != null)
            {
                text1.SetActive(true); //Can pick up new Mag
            }

            if (hitInfo.transform.tag == "Weapon")
            {
                if (hitInfo.collider.gameObject.GetComponent<Gun>().wielder == null)
                {
                    text2.GetComponent<Text>().text = $"Pick up {hitInfo.transform.name} ({pickupWeaponKey})";
                    text2.SetActive(true); //Can pick up Gun
                }
            }
        }
        else
        {
            text1.SetActive(false);
            text2.SetActive(false);
        }
    }
}
