/********************************************
 *  Created by Isabela Rangel
 *  Asia's Adventure Reborn
 *  Creation Date:  2021-1-1
 *  
 ********************************************/
using UnityEngine;

/// <summary>
/// Creates a Scriptable Object of type weapon that allows for custom properties of a weapon.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Weapon")]
public class WeaponSO : ScriptableObject
{
    // Determines which slot the weapon will take.
    public enum WeaponSlot 
    {
        Primary = 0,
        Secondary = 1
    }

    // Weapon type determines how the weapon will be holstered.
    public enum WeaponType
    {
        Rifle = 0,
        Pistol = 1,
        Sword = 2
    }


    // Basic settings
    [Header("Weapon Description")]
    public string nameString;
    public Sprite icon;


    // Bullet and reload settings
    [Header("Weapon Properties")]
    public WeaponSlot weaponSlot;
    public WeaponType weaponType;
    public int fireRate = 25;
    public int clipSize = 120;
    public float reloadTime = 2.0f;
    public float verticalRecoil = 1;
    public Vector2 horizontalRecoilRange = new Vector2(-10,10);
    public float recoilInterval = 1;

    // Bullet properties
    [Header("Bullet Properties")]
    public float bulletSpeed = 1000.0f;
    public float bulletDrop = 0.0f;
    public float bulletMaxLifetime = 3f;
    public int bulletDamage = 10;
    public int bulletMaxBounces = 1;
    public float bulletBounceSpeedModifier = .5f;
    public int piercing = 2;
    public float knockbackAmount = .2f;
    public float sprayAmount = 0f;


    // Gameobjects and graphics associated.
    [Header("Gameobjects")]
    public Transform pfObject;
    public GameObject pfWeaponGraphics;
    public TrailRenderer pfTrailRenderer;


}
