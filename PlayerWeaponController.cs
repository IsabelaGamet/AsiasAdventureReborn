/********************************************
 *  Created by Isabela Rangel
 *  Asia's Adventure Reborn
 *  Creation Date:  2021-1-2
 *  
 ********************************************/
using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEditor.Animations;
using System.Collections;

/// <summary>
/// Class that controls player active weapon and controls its commands.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerController))]
public class PlayerWeaponController : MonoBehaviour
{
    /* Note:
     * By creating a list of WeaponSO's, can respond to input to switch weapons very easily and spawn them
     * from the SO object. Destroying and creating them as needed. The first one can be a sword, and only swords can 
     * occupy that slot, while the second one is just for weapons.
     * 
     * Would have to change the equippable weapon script so that it allows for being picked up and dropped. Basically 
     * adding and removing a rigidbody and collider to it programmatically. 
     */

    public event EventHandler<WeaponSO> OnWeaponChanged;

    [Header("References")]
    [SerializeField] private Transform[] weaponParent;
    [SerializeField] private Animator rigAnimator;
    [SerializeField] private Transform crosshairTarget;
    [SerializeField] private Transform lookAt;

    // Player input object
    private PlayerInput playerInput;
    private Animator playerModelAnimator;


    // Weapon logic
    private EquipableWeapon[] equippedWeapons = new EquipableWeapon[2];
    private int activeWeaponIndex = 0;
    private bool isFiring = false;







    /// <summary>
    /// Gets required components, consistency checks, etc.
    /// </summary>
    private void Awake()
    {
        // Gets components needed on awake.
        playerInput = GetComponent<PlayerInput>();
        EquipableWeapon tempWeapon = GetComponentInChildren<EquipableWeapon>();

        // Subscribes to input events.
        playerInput.actions["Fire"].started += OnFireStarted;
        playerInput.actions["Fire"].canceled += OnFireCanceled;
        playerInput.actions["Aim"].started += OnAimStarted;
        playerInput.actions["Aim"].canceled += OnAimCanceled;
        playerInput.actions["Reload"].started += OnReloadStarted;

        #region NULL CHECKS
        // Needs to have a crosshair target to supply weapons.
        if (crosshairTarget == null)
        {
            Debug.LogError("Set crosshair target on " + gameObject.name);
        }
        if (lookAt == null)
        {
            Debug.LogError("Set lookat target on " + gameObject.name);
        }

        playerModelAnimator = GetComponent<PlayerController>().GetAnimator();
        #endregion

        // If there is an active weapon, activate it.
        if (equippedWeapons != null)
            Equip(tempWeapon);

    }

    /// <summary>
    /// Updates equipped weapon as needed based on input.
    /// </summary>
    private void LateUpdate()
    {
        // If there is a weapon to use, use it.
        if (equippedWeapons[activeWeaponIndex] != null)
        {
            // Updates active weapon
            equippedWeapons[activeWeaponIndex].UpdateWeapon(Time.deltaTime);
            if (!isFiring && equippedWeapons[activeWeaponIndex].IsFiring())
            {
                equippedWeapons[activeWeaponIndex].StopFiring();
            }
        }
    }


    #region EVENT CALLBACKS
    /// <summary>
    /// Responds to on fire started event.
    /// </summary>
    /// <param name="obj">Event Args</param>
    private void OnFireStarted(InputAction.CallbackContext obj)
    {
        isFiring = true;
        if(equippedWeapons[activeWeaponIndex] != null)
            equippedWeapons[activeWeaponIndex].StartFiring();
    }

    /// <summary>
    /// Responds to on fire stopped event.
    /// </summary>
    /// <param name="obj">Event arguments</param>
    private void OnFireCanceled(InputAction.CallbackContext obj)
    {
        isFiring = false;
    }
    /// <summary>
    /// Responds to on aim event being cancelled.
    /// </summary>
    /// <param name="obj"></param>
    private void OnAimCanceled(InputAction.CallbackContext obj)
    {
        Debug.Log("Aim not implemented yet");
    }

    /// <summary>
    /// Responds to on aim event.
    /// </summary>
    /// <param name="obj"></param>
    private void OnAimStarted(InputAction.CallbackContext obj)
    {
        Debug.Log("Aim not implemented yet");

    }

    /// <summary>
    /// Responds to reload event.
    /// </summary>
    /// <param name="obj"></param>
    private void OnReloadStarted(InputAction.CallbackContext obj)
    {
        SetActiveWeapon(GetIncreaseWeaponIndex(1));
    }
    #endregion


    #region WEAPON INDEX GETS/SETS

    /// <summary>
    /// Function  to return weapon of given index.
    /// </summary>
    /// <param name="index">Weapon index.</param>
    /// <returns></returns>
    private EquipableWeapon GetWeapon(int index)
    {
        // Consistency checks
        if (index < 0 || index >= equippedWeapons.Length)
        {
            return null;
        }
        return equippedWeapons[index];
    }

    /// <summary>
    /// For scrolling/toggling, increases the weapon index.
    /// </summary>
    /// <param name="increment">How much to increment or decrement out of the current weapons.</param>
    private int GetIncreaseWeaponIndex(int increment)
    {
        int newIndex = activeWeaponIndex + increment;

        if (newIndex >= equippedWeapons.Length)
        {
            newIndex = 0;
        }
        if (newIndex < 0)
        {
            newIndex = equippedWeapons.Length - 1;
        }
        return newIndex;
    }
    /// <summary>
    /// Sets and clamps weapon index so it doesn't go out of bounds.
    /// </summary>
    /// <param name="newIndex">New index to jump to.</param>
    private int GetWeaponIndex(int newIndex)
    {
        return Mathf.Clamp(newIndex, 0, equippedWeapons.Length);
    }
    #endregion
    
    /// <summary>
    /// Allows equipping of a different weapon. 
    /// </summary>
    /// <param name="newWeapon">New weapon to be equipped.</param>
    public void Equip(EquipableWeapon newWeapon)
    {
        // If there is no secondary weapon, unarm.
        if (newWeapon == null)
        {
            rigAnimator.Play("unarmed");
            return;
        }
        // Gets index that new weapon has to go to.
        int weaponSlotIndex = (int)newWeapon.GetWeapon().weaponSlot;

        // If it already has a weapon, destroy that weapon
        if (equippedWeapons[weaponSlotIndex] != newWeapon && equippedWeapons[weaponSlotIndex] != null)
        {
            Destroy(equippedWeapons[weaponSlotIndex].gameObject);
        }
        // Change reference
        equippedWeapons[weaponSlotIndex] = newWeapon;
        // Update coordinates
        equippedWeapons[weaponSlotIndex].transform.SetParent(weaponParent[weaponSlotIndex], false);
        // Sets needed variables
        equippedWeapons[weaponSlotIndex].SetRaycastTarget(crosshairTarget);
        equippedWeapons[weaponSlotIndex].SetLookAt(lookAt);
        equippedWeapons[weaponSlotIndex].SetAnimator(playerModelAnimator);

        // Invokes event.
        OnWeaponChanged?.Invoke(this, equippedWeapons[weaponSlotIndex].GetWeapon());

        // Sets the holster position of the object to be of the specified type. Should be set up as a child of the holster
        // If it is a primary weapon that is.
        //if(weaponSlotIndex == 1)
        //    weaponParent[weaponSlotIndex].GetComponent<MultiAimConstraint>().data.sourceObjects.SetTransform(1, weaponSlots[weaponSlotIndex].GetChild((int)newWeapon.GetWeapon().weaponType));
        
        // Starts animation coroutine
        SetActiveWeapon(weaponSlotIndex);
    }

    #region HOLSTERING/ACTIVATING ANIMATION COROUTINES
    /// <summary>
    /// Sets active weapon.
    /// </summary>
    /// <param name="weaponSlotIndex"></param>
    private void SetActiveWeapon(int weaponSlotIndex)
    {
        int holsterIndex = activeWeaponIndex;
        int activateIndex = weaponSlotIndex;
        StartCoroutine(SwitchWeapon(activeWeaponIndex, weaponSlotIndex));
    }

    /// <summary>
    /// Controls swtich weapon flow.
    /// </summary>
    /// <param name="holsterIndex">Index of weapon to be holstered.</param>
    /// <param name="activateIndex">Index of weapon to be activated.</param>
    /// <returns></returns>
    private IEnumerator SwitchWeapon(int holsterIndex, int activateIndex)
    {
        yield return StartCoroutine(HolsterWeapon(holsterIndex));
        yield return StartCoroutine(ActivateWeapon(activateIndex));
        // sets new active weapon index
        activeWeaponIndex = GetWeaponIndex(activateIndex);
    }

    /// <summary>
    /// Handles logic to holster weapon.
    /// </summary>
    /// <param name="index">Weapon to be holstered.</param>
    /// <returns></returns>
    private IEnumerator HolsterWeapon(int index)
    {
        EquipableWeapon weaponToHolster = GetWeapon(index);
        if(weaponToHolster != null)
        {
            // Set bool to indicate it is holstering
            weaponToHolster.SetHolstered(true);

            // Sets animator boolean.
            rigAnimator.SetBool("isHolstering", true);
            // Waits for end of fixed update because it's animating physics. Waits till animation is done.
            yield return new WaitForFixedUpdate();
            do
            {
                yield return new WaitForEndOfFrame();
            }
            while (rigAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
        }
    }
    /// <summary>
    /// Handles logic to activate a weapon.
    /// </summary>
    /// <param name="index">Weapon to be activated.</param>
    /// <returns></returns>
    private IEnumerator ActivateWeapon(int index)
    {
        EquipableWeapon weaponToActivate = GetWeapon(index);
        if (weaponToActivate != null)
        {
            // Set bool to indicate it is holstering
            rigAnimator.SetBool("isHolstering", false);
            // Equip weapon animation.
            rigAnimator.Play("equip" + weaponToActivate.GetWeapon().name);

            // Waits until animation is done.
            yield return new WaitForFixedUpdate();
            do
            {
                yield return new WaitForEndOfFrame();
            }
            while (rigAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
            weaponToActivate.SetHolstered(false);
        }

    }
    #endregion
}
