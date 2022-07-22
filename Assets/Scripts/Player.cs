using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Transform theCamera; //the camera point the player's head is using
    [SerializeField] private Transform gunHoldPosition; //the position in space attached to the camera that weapons are held at
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

        if (Input.GetKeyDown(throwWeaponKey) && currentWeapon != null) weaponScript.ThrowWeapon();

    }

    private void PickupWeapon()
    {

        if (currentWeapon != null) DropWeapon();

        if (Physics.SphereCast(theCamera.position, pickupRadius, theCamera.forward, out RaycastHit hitInfo, interactDistance, weaponLayer))
        {
            if (hitInfo.transform.tag == "Weapon")
            {
                currentWeapon = hitInfo.collider.gameObject;
                currentWeapon.GetComponent<Rigidbody>().isKinematic = true;
                currentWeapon.GetComponent<BoxCollider>().enabled = false;
                currentWeapon.transform.parent = theCamera;
                currentWeapon.transform.position = gunHoldPosition.position;
                currentWeapon.transform.rotation = gunHoldPosition.rotation;

                weaponScript = currentWeapon.GetComponent<Gun>();
                weaponScript.wielder = this;
            }
        }
        //Sphere cast in front of player
        //If hit info finds anything on weaponLayer,
        //Pick it up, attach it to the player.
    }

    private void DropWeapon()
    {

        if (currentWeapon != null)
        {
            Gun currentWeaponScript = currentWeapon.GetComponent<Gun>();
            currentWeaponScript.DropGun();
        }
        else Debug.Log("You aren't holding a weapon to drop.");

    }

    private void SphereCheck()
    {
        if (Physics.SphereCast(theCamera.position, pickupRadius, theCamera.forward, out RaycastHit hitInfo, interactDistance, magazineLayer | weaponLayer))
        {
            if (hitInfo.transform.tag == "Magazine") text1.SetActive(true); //Can pick up Mag
            if (hitInfo.transform.tag == "Weapon" && currentWeapon == null)
            {
                text2.SetActive(true); //Can pick up Gun
            }

        }
        else
        {
            text1.SetActive(false);
            text2.SetActive(false);
        }
    }
}
