/********************************************
 *  Created by Isabela Rangel
 *  Asia's Adventure Reborn
 *  Creation Date:  Nov-2020
 *  
 ********************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that determines position of where the camera is currently pointing at and changes target accordingly.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CrosshairTarget : MonoBehaviour
{
    // Variable to cache main camera to minimize calls during update.
    [SerializeField] private Transform crosshairTarget;
    [SerializeField] private Transform player;

    [Header("Debug Functionality")]
    [SerializeField] private bool debugFunctions = false;

    // Raycast variables for position calculation.
    private Ray ray;
    private RaycastHit hitInfo;

    /// <summary>
    /// Finds reference needed for main camera.
    /// </summary>
    private void Start()
    {
        #region Null Checks
        // Consistency check if crosshair target has not been properly set.
        if (crosshairTarget == null)
        {
            // Tries to identify crosshair target.
            crosshairTarget = transform.Find("CrosshairTarget");
            // If it is still null, log error and destroy self.
            if(crosshairTarget == null)
            {
                Debug.LogError("No crosshair target detected, aiming and other functions have to be set for player to behave properly. Destroying this script.");
                Destroy(this);
            }
        }
        #endregion
    }

    /// <summary>
    /// Updates position with every frame.
    /// </summary>
    private void Update()
    {
        // Origin is the main camera, aka this gameobject.
        if(player != null)
        {
            ray.origin = transform.position + transform.forward * Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(player.position.x, 0, player.position.z));

        }
        else
        {
            ray.origin = transform.position + transform.forward * 6f;
        }

        // Direction is forward.
        ray.direction = transform.forward;

        // If it has hit anything, put itself into that position.
        if (Physics.Raycast(ray, out hitInfo))
        {
            crosshairTarget.position = hitInfo.point;
        }
        // Otherwise set it to be at 1,000 units away from the character.
        else
        {
            crosshairTarget.position = ray.origin + ray.direction * 1000.0f;
        }

        // Debug functionality allows for the drawing of a gizmo onscreen for ease of seeing.
        if (debugFunctions)
        {
            Debug.DrawLine(ray.origin, hitInfo.point, Color.blue, 1f);
        }
    }
}
