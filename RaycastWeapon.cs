/********************************************
 *  Created by Isabela Rangel
 *  Asia's Adventure Reborn
 *  Creation Date:  2021-1-1
 *  
 ********************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inherits equippable weapon. Defines a weapon that uses raycasts and a scriptable object's 
/// data to control shooting projectiles. Deals with simulating bullet movement.
/// </summary>
public class RaycastWeapon : EquipableWeapon
{
    [Header("Aiming Settings")]
    [SerializeField] private Transform raycastOrigin;


    [Header("Effects")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem muzzleFlash;


    Ray ray;
    RaycastHit hitInfo;

    List<Bullet> bulletsFired = new List<Bullet>();




    /// <summary>
    /// Subclass bullet for calculations of the bullet physics.
    /// </summary>
    class Bullet
    {
        public float time;
        public Vector3 initialPosition;
        public Vector3 initialVelocity;
        public TrailRenderer trail;
        public int piercing;
        public bool isAlive;
        public int bounces;

        public bool IsAlive(float maxBulletLifetime)
        {
            if(!isAlive)
            {
                return false;
            }
            if(bounces <= 0 || time > maxBulletLifetime)
            {
                isAlive = false;
                return false;
            }
            return true;
        }
    }




    private void OnEnable()
    {
        #region CONSISTENCY CHECKS
        // Checks if raycast origin is null. If it is, look between children.
        if (raycastOrigin == null)
        {
            Debug.Log(gameObject.name + ": Raycast origin not checked, looking.");
            raycastOrigin = transform.Find("RaycastOrigin");

            // If can't find a raycast origin in children, use self transform location.
            if (raycastOrigin == null)
            {
                Debug.LogError(gameObject.name + ": Can't find raycast origin. Using myself as origin.");
                raycastOrigin = transform;
            }
        }
        #endregion
        
    }

    private void OnDestroy()
    {
        DestroyAllBullets();
    }




    #region INPUT RESPONSE
    /// <summary>
    /// Overrides start firing behavior.
    /// </summary>
    public override void StartFiring()
    {
        isFiring = true;
        timePassed = 0f;
    }

    /// <summary>
    /// Causes weapon to stop firing.
    /// </summary>
    public override void StopFiring()
    {
        isFiring = false;

    }
    #endregion



    #region UPDATE BEHAVIOR
    /// <summary>
    /// Updates weapon behavior. Should be called every frame by the weapon controller.
    /// </summary>
    /// <param name="deltaTime"> time passed</param>
    public override void UpdateWeapon(float deltaTime)
    {

        SimulateBullets(deltaTime);

        if (isFiring && !isHolstered)
        {
            UpdateFiring(deltaTime);
        }

        if (!isFiring || isHolstered)
        {
            StopFiring();
        }
    }

    /// <summary>
    /// Updates bullets fired based on firing interval.
    /// </summary>
    /// <param name="deltaTime">Takes in time passed as it doesn't use monobehaior's update</param>
    private void UpdateFiring(float deltaTime)
    {
        timePassed += deltaTime;
        float fireInterval = 1.0f / weapon.fireRate;
        while(timePassed >= 0f)
        {
            GenerateRecoil();
            FireBullet();
            timePassed -= fireInterval;
        }
    }
    /// <summary>
    /// Simulates each bullet based on deltatime. 
    /// </summary>
    /// <param name="deltaTime">Time passed since last frame.</param>
    private void SimulateBullets(float deltaTime)
    {
        // Begins by destroying any bullets that should not be there. (That might have run out of time)
        DestroyBullets();
        // Iterates through bullets in the list and performs neded calculations.
        foreach (Bullet bullet in bulletsFired)
        {
            Vector3 startPosition = GetBulletPosition(bullet);
            bullet.time += deltaTime;
            Vector3 endPosition = GetBulletPosition(bullet);
            RayscastSegment(startPosition, endPosition, bullet);
        }
        // Destroys bullets that it just calculated should be destroyed.
        DestroyBullets();
    }
    #endregion

    /// <summary>
    /// Raycasts bullet segment.
    /// </summary>
    /// <param name="start">Start position</param>
    /// <param name="end">End position</param>
    /// <param name="bullet">Bullet to simulate.</param>
    private void RayscastSegment(Vector3 start, Vector3 end, Bullet bullet)
    {
        // determines direction based on endpoint.
        Vector3 direction = end - start;
        // Distance is the magnitude
        float distance = (end - start).magnitude;
        // Sets the necessary start/end points.
        ray.origin = start;
        ray.direction = direction;

        // If there is a hit, emit particles needed and check collision. 
        if (Physics.Raycast(ray, out hitInfo, distance))
        {
            // Moves hit effects as needed.
            hitEffect.transform.position = hitInfo.point;
            hitEffect.transform.forward = hitInfo.normal;
            hitEffect.Emit(1);

            bullet.trail.transform.position = hitInfo.point;

            //collision check
            BulletCollision(hitInfo);

            // If there are bounces left, change bullet info
            if (bullet.bounces >= 0)
            {
                // reset time
                bullet.time = 0;
                // Update initial position to where the bounce happened.
                bullet.initialPosition = hitInfo.point;
                // Reflect velocity.
                bullet.initialVelocity = Vector3.Reflect(bullet.initialVelocity * weapon.bulletBounceSpeedModifier, hitInfo.normal);
                // Decrease bounce amount.
                bullet.bounces--;
            }
            else
            {
                // destroys bullet
                bullet.time += weapon.bulletMaxLifetime;
                bullet.isAlive = false;
            }
        }
        else
        {
            // If didn't hit anything, move trail anyways.
            bullet.trail.transform.position = end;
        }
    }

    /// <summary>
    /// Fires a single bullet.
    /// </summary>
    private void FireBullet()
    {
        // Determines velocity.
        Vector3 velocity = (raycastTarget.position - raycastOrigin.position).normalized * weapon.bulletSpeed;
        // Creates bullet object.
        Bullet bullet = CreateBullet(raycastOrigin.position, velocity);
        // Adds to list.
        bulletsFired.Add(bullet);
        
    }


    #region BULLET DESTROY BEHAVIOR
    /// <summary>
    /// Destroys bullets.
    /// </summary>
    private void DestroyBullets()
    {
        for (int i = 0; i < bulletsFired.Count; i++)
        {
            if (!bulletsFired[i].IsAlive(weapon.bulletMaxLifetime))
            {
                // Destroys trailrendered gameobject for cleanup purposes.
                Destroy(bulletsFired[i].trail.gameObject);

                bulletsFired.Remove(bulletsFired[i]);
            }
        }
    }
    /// <summary>
    /// Destroys all bullets. For cleanup purposes.
    /// </summary>
    private void DestroyAllBullets()
    {
        for (int i = 0; i < bulletsFired.Count; i++)
        {
            // Clears every bullet that is not null.
            if (bulletsFired[i] != null)
            {
                if (bulletsFired[i].trail.gameObject != null)
                    Destroy(bulletsFired[i].trail.gameObject);

                bulletsFired.Remove(bulletsFired[i]);
            }
        }
    }
    #endregion


    /// <summary>
    /// Creates new bullet object with the specified position and velocity.
    /// </summary>
    /// <param name="position">Starting position.</param>
    /// <param name="velocity">Starting velocity.</param>
    /// <returns></returns>
    private Bullet CreateBullet(Vector3 position, Vector3 velocity)
    {
        // Creates empty bullet object.
        Bullet bullet = new Bullet();
        // Initiates position and velocity.
        bullet.initialPosition = position;
        bullet.initialVelocity = velocity;
        // Zeroes time.
        bullet.time = 0.0f;
        // Configures the remaining fields.
        bullet.piercing = weapon.piercing;
        bullet.isAlive = true;
        bullet.bounces = weapon.bulletMaxBounces;
        bullet.trail = Instantiate(weapon.pfTrailRenderer, position, Quaternion.identity);
        // Adds initial position to tracer effect
        bullet.trail.AddPosition(position);
        // Returns created bullet.
        return bullet;
    }

    /// <summary>
    /// Calculares bullet position at any given time following the equation
    /// </summary>
    /// <param name="bullet">Raycast weapon subclass bullet is required as input.</param>
    /// <returns></returns>
    private Vector3 GetBulletPosition(Bullet bullet)
    {
        // Consistency check, ensures that there is a bullet as input before trying to return a vector 3.
        if (bullet != null)
        {
            // creates custom gravity factor for bullet
            Vector3 gravity = Vector3.down * weapon.bulletDrop;

            // Follows the equation p + v * t + .5 * g * t * t
            return bullet.initialPosition + (bullet.initialVelocity * bullet.time) + (.5f * gravity * bullet.time * bullet.time);
        }
        else
            return Vector3.zero;
    }


    /// <summary>
    /// Damage behavior for when the bullet collides with the object.
    /// </summary>
    /// <param name="hitInfo">Raycast hit object that determines path travelled and contains collision information.</param>
    private void BulletCollision(RaycastHit hitInfo)
    {
        /*
        Collider collider = hitInfo.collider;
        if (!isEnemy)
        {
            // need to add code to damage enemeies
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null) // checks if it has an enemy script attached to it
            {
                enemy.Damage(weapon.bulletDamage);
                Vector3 knockbackDir = transform.forward;
                enemy.gameObject.GetComponent<Rigidbody>().AddForceAtPosition(knockbackDir * weapon.knockbackAmount, hitInfo.point, ForceMode.Impulse);
            }
        }
        else
        {
            Player player = collider.GetComponent<Player>();
            if (player != null)
            {
                player.Damage(weapon.bulletDamage);
            }
        }*/
    }



}
