using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    public Transform theCamera; //the camera point the player's head is using
    [SerializeField] private Transform gunHoldPosition; //the position in space attached to the camera that weapons are held at
    [SerializeField] private Transform gunAimPosition; //the position in space attached to the camera that weapons are moved to for aiming
    public float aimTime = 4f; //The time it takes for the gun to get to your eye for ADS, roughly.
    private bool aiming = false; //Whether the player is aiming or not. 
    public float pickupRadius; //The radius of the sphere cast check
    public float interactDistance = 2; //The length of the raycast for interactions
    [SerializeField] private LayerMask magazineLayer; //Magazines out in the world should be assigned to this same layer
    [SerializeField] private LayerMask weaponLayer; //Weapons out in the world should be assigned to this same layer
    [SerializeField] private GameObject text1; //The text that says "Pick up Magazine" When possible
    [SerializeField] private GameObject text2; //The text that says "Pick up Gun" When possible

    [SerializeField] public GameObject currentWeapon; //The gun in hand to be used and fired
    //[SerializeField] public GameObject startingWeapon; //The gun the player starts with
    [SerializeField] private Gun weaponScript;

    [Header("Keybinds")]
    [SerializeField] private KeyCode pickupWeaponKey = KeyCode.E;
    [SerializeField] private KeyCode dropWeaponKey = KeyCode.G;
    [SerializeField] private KeyCode throwWeaponKey = KeyCode.F;
    [SerializeField] private KeyCode reloadWeaponKey = KeyCode.R;
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode aimSightKey = KeyCode.Mouse1;

    private void Start()
    {
        weaponScript = currentWeapon.GetComponent<Gun>();
    }

    private void Update()
    {
        SphereCheck();

        if (Input.GetKeyDown(pickupWeaponKey)) PickupWeapon();

        if (Input.GetKeyDown(fireKey) && currentWeapon != null) weaponScript.GunShoot();

        if (Input.GetKeyDown(dropWeaponKey)) DropWeapon();

        if (Input.GetKeyDown(reloadWeaponKey) && currentWeapon != null) weaponScript.ReloadWeapon();

        if (Input.GetKeyDown(throwWeaponKey)) ThrowWeapon();

        if (Input.GetKeyDown(aimSightKey)) StartCoroutine(AimWeapon());

    }

    private IEnumerator AimWeapon()
    {
        aiming = true;
        Vector3 currentVelocity = Vector3.zero;
        AimingAgain:
        while (Input.GetKey(aimSightKey))
        {
            if (currentWeapon.transform.position != gunAimPosition.position)
            {
                currentWeapon.transform.position = Vector3.SmoothDamp(currentWeapon.transform.position, 
                    gunAimPosition.position, ref currentVelocity, aimTime);
            }
            yield return null;
        }
        while (currentWeapon.transform.position != gunHoldPosition.position)
        {
            currentWeapon.transform.position = Vector3.SmoothDamp(currentWeapon.transform.position,
                    gunHoldPosition.position, ref currentVelocity, aimTime);
            if (Input.GetKey(aimSightKey)) goto AimingAgain; //label shortcut if you start aiming again while lowering the weapon
            yield return null;
        }
        aiming = false;
        yield break;
    }



    private void PickupWeapon()
    {

        if (Physics.SphereCast(theCamera.position, pickupRadius, theCamera.forward, out RaycastHit hitInfo, interactDistance, weaponLayer))
        {
            if (hitInfo.transform.tag == "Weapon" && currentWeapon != hitInfo.transform.gameObject)
            {
                if (hitInfo.collider.gameObject.GetComponent<Gun>().wielder == null)
                {
                    if (currentWeapon != null) DropWeapon();

                    Debug.Log("Picking up new weapon, transform name: " + hitInfo.transform.name);
                    currentWeapon = hitInfo.transform.gameObject;
                    currentWeapon.GetComponent<Rigidbody>().isKinematic = true;
                    currentWeapon.GetComponent<BoxCollider>().enabled = false;
                    currentWeapon.transform.parent = theCamera;
                    currentWeapon.transform.position = gunHoldPosition.position;
                    currentWeapon.transform.rotation = gunHoldPosition.rotation;

                    weaponScript = currentWeapon.GetComponent<Gun>();
                    weaponScript.wielder = this;
                }
                    
            }
        }
    }

    private void DropWeapon()
    {

        if (currentWeapon != null)
        {
            Gun currentWeaponScript = currentWeapon.GetComponent<Gun>();
            currentWeaponScript.DropGun();
            currentWeapon = null;
        }
        else Debug.Log("You aren't holding a weapon to drop.");

    }

    private void ThrowWeapon()
    {

        if (currentWeapon != null)
        {
            Gun currentWeaponScript = currentWeapon.GetComponent<Gun>();
            currentWeaponScript.ThrowGun();
            currentWeapon = null;
        }
        else Debug.Log("You threw your weapon.");

    }

    private void SphereCheck()
    {
        if (Physics.SphereCast(theCamera.position, pickupRadius, theCamera.forward, out RaycastHit hitInfo, interactDistance, magazineLayer | weaponLayer))
        {
            if (hitInfo.transform.tag == "Magazine" && currentWeapon != null) text1.SetActive(true); //Can pick up new Mag
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
