/***************************************
 *Created by Isabela Rangel
 *  Asia's Adventure Reborn
 *  Creation Date:  2021 - 1 - 4
 *
 ********************************************/
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines weapon pickup behavior for a trigger object. Spawns its own graphic of the weapon and rotates it around object as needed.
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponSO weapon;
    [SerializeField] private Transform objectOrigin;
    private Transform weaponGraphics;

    private void Awake()
    {
        #region NULL CHECKS
        if(weapon == null)
        {
            Debug.LogError("WeaponSO needs to be assigned");
            Destroy(this);
        }
        if (weapon.pfObject == null || weapon.pfWeaponGraphics == null)
        {
            Debug.LogError("WeaponSO NEEDS both its object prefab and the weapon graphics prefab.");
            Destroy(this);
        }
        if (objectOrigin == null)
        {
            objectOrigin = transform.Find("Origin");

            if (objectOrigin == null)
            {
                Debug.LogError("Need an origin for the weapon to rotate about.");
                Destroy(this);
            }
        }
        #endregion
        SpawnWeaponGraphic();
    }

    private void Update()
    {
        UpdateWeaponGraphic();
    }

    private void SpawnWeaponGraphic()
    {
        weaponGraphics = Instantiate(weapon.pfWeaponGraphics.transform, transform);
        weaponGraphics.position = objectOrigin.position;
    }

    private void UpdateWeaponGraphic()
    {
        weaponGraphics.Rotate(transform.up, .2f);

        float velocity = Time.time * 1f;
        float pingPongState = (Mathf.PingPong(velocity, 2) - 1);
        Vector3 pingPongMaxDistance = new Vector3(0, 1, 0) * .05f;

        weaponGraphics.position = objectOrigin.position + pingPongMaxDistance * pingPongState;
    }

    /// <summary>
    /// Activated on physics trigger.
    /// </summary>
    /// <param name="collider">Collider object.</param>
    private void OnTriggerEnter(Collider collider)
    {
        // If object that collided has a playerweapon component, equip this new weapon.
        PlayerWeaponController playerWeapon = collider.GetComponent<PlayerWeaponController>();
        if(playerWeapon != null)
        {
            EquipableWeapon newWeapon = Instantiate(weapon.pfObject.GetComponent<EquipableWeapon>());
            playerWeapon.Equip(newWeapon);
        }
    }


}
