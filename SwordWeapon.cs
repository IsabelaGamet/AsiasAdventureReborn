using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwordWeapon : EquipableWeapon
{
    public override void StartFiring()
    {
        isFiring = true;
        timePassed = 0;
    }

    public override void StopFiring()
    {
        isFiring = false;
    }

    public override void UpdateWeapon(float deltaTime)
    {
        if(isFiring)
        {
            Attack2(deltaTime);
        }

    }

    private void Attack2(float deltaTime)
    {
        timePassed += deltaTime;
        float fireInterval = 1.0f / weapon.fireRate;
        while (timePassed >= 0f)
        {
            GenerateRecoil();
            playerModelAnimator.SetTrigger("onAttack2");
            timePassed -= fireInterval;
        }
    }
}
