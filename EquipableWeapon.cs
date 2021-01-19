/********************************************
 *  Created by Isabela Rangel
 *  Asia's Adventure Reborn
 *  Creation Date:  Dec-2020
 *  
 ********************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Defines necessary functions an equippable weapon needs to have.
/// </summary>
public abstract class EquipableWeapon : MonoBehaviour
{

    [Header("Weapon")]
    [SerializeField] protected WeaponSO weapon;

    protected Transform raycastTarget;
    protected Transform lookAt;

    protected Animator playerModelAnimator;
    protected CinemachineImpulseSource cinemachineImpulse;

    protected bool isFiring = false;
    protected bool isHolstered = false;
    protected float timePassed;

    
    private List<float> horizontalRecoilPattern = new List<float>();
    private int recoilIndex = 0;

    private void Awake()
    {
        cinemachineImpulse = GetComponent<CinemachineImpulseSource>();

        if (weapon.horizontalRecoilRange != Vector2.zero)
        {
            for(float i = weapon.horizontalRecoilRange.x; i <= weapon.horizontalRecoilRange.y; i += weapon.recoilInterval)
            {
                horizontalRecoilPattern.Add(i);
            }
        }
        else
            horizontalRecoilPattern.Add(0);

    }

    /// <summary>
    /// Function that updates weapon behavior as needed. Should be called every frame.
    /// </summary>
    /// <param name="deltaTime"></param>
    public abstract void UpdateWeapon(float deltaTime);

    /// <summary>
    /// Function that defines when a weapon has started to fire.
    /// </summary>
    public abstract void StartFiring();

    /// <summary>
    /// Function to cancel fire behavior.
    /// </summary>
    public abstract void StopFiring();

    /// <summary>
    /// Returns weaponSO for weapon information.
    /// </summary>
    /// <returns>WeaponSO of thios script.</returns>
    public WeaponSO GetWeapon()
    {
        return weapon;
    }

    /// <summary>
    /// Returns if weapon is currently firing.
    /// </summary>
    /// <returns>True if weapon is currently firing.</returns>
    public bool IsFiring()
    {
        return isFiring;
    }
    /// <summary>
    /// Allows for control of holstered value.
    /// </summary>
    /// <param name="newValue"></param>
    public void SetHolstered(bool newValue)
    {
        isHolstered = newValue;
    }

    /// <summary>
    /// Function to set crosshair target on start since crosshair target is not a singleton that can be accessed.
    /// Class that manages this weapon should change it.
    /// </summary>
    /// <param name="raycastTarget">Reference to crosshair target gameobject that defines aiming.</param>
    public void SetRaycastTarget(Transform raycastTarget)
    {
        this.raycastTarget = raycastTarget;
    }
    /// <summary>
    /// Function to set lookat target so as to add recoil.
    /// </summary>
    /// <param name="lookAt">Reference to active lookat object.</param>
    public void SetLookAt(Transform lookAt)
    {
        this.lookAt = lookAt;
    }

    /// <summary>
    /// Function to set animator on start in case animator is needed.
    /// </summary>
    /// <param name="playerModelAnimator"></param>
    public void SetAnimator(Animator playerModelAnimator)
    {
        this.playerModelAnimator = playerModelAnimator;
    }

    /// <summary>
    /// Function that adds recoil to lookat target as needed.
    /// </summary>
    protected void GenerateRecoil()
    {
        Vector3 newRotationEuler = lookAt.rotation.eulerAngles - new Vector3(weapon.verticalRecoil, horizontalRecoilPattern[recoilIndex] * .05f , 0);

        newRotationEuler = PlayerController.ClampCameraRotation(newRotationEuler);

        lookAt.rotation = Quaternion.Euler(newRotationEuler);

        IncrementRecoilIndex();

        cinemachineImpulse.GenerateImpulse();
    }


    private void IncrementRecoilIndex()
    {
        recoilIndex = (recoilIndex + 1) % horizontalRecoilPattern.Count;
    }

}
