using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player controller class determines player movement behavior based on player input. 
/// It requires a PlayerInput, Rigidbody and Animator component to be attached to the game
/// object.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform lookAt;
    [SerializeField] private float cameraSensitivity = .5f;
    [SerializeField] private bool rotatesWithCamera = true;

    [Header("Movement settings")]
    [SerializeField] private Transform characterGraphics;
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private float jumpHeight = 30f;
    [SerializeField] private bool hasStrafeAnimations = false;
    [SerializeField] private float strafeRotationOffset = 50f;
    [SerializeField] private float rotationTime = .5f;

    
    Vector2 movement;
    private bool canJump;

    private Vector3 horizontalMovement;
    private Vector3 forwardMovement;

    private InputAction moveInput;

    private PlayerInput playerInput;
    private Rigidbody rb;
    [SerializeField] private Animator animator;




    /// <summary>
    /// On awake gets the needed components and ensures they are all valid.
    /// </summary>
    private void Awake()
    {
        // Gets select components. Since they are required, no consistency check is needed.
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();

        #region CONSISTENCY CHECKS
        // Look at animator for graphic purposes
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        if (animator == null)
        {
            Debug.LogError("No animator found.");
        }
        // Look at object setup for camera purposes.
        if (lookAt == null)
        {
            lookAt = transform.Find("LookAt");
            // If can't find a lookat object, log error.
            if (lookAt == null)
                Debug.LogError("Missing lookat object in character hierarchy.");
        }
        // Character root for configuration purposes.
        if (characterGraphics == null)
        {
            characterGraphics = transform.Find("Graphics");
            // If can't find a character graphics object, log error.
            if (characterGraphics == null)
                Debug.LogError("Missing character graphics object in character hierarchy.");
        }
        #endregion

        // Player input setup.
        // Subscribe to events.
        playerInput.actions["Jump"].started += OnJumpStarted;
        playerInput.actions["Look"].started += OnLookStarted;

        // Cache move input reference for movement in fixed movement.
        moveInput = playerInput.actions["Move"];


    }

    #region MOVEMENT
    private void Update()
    {
        // Gets current value of the moveInput.
        movement = moveInput.ReadValue<Vector2>();

        // Sets animator to animate character
        animator.SetFloat("speed", movement.magnitude);
        animator.SetFloat("speedY", movement.y);

    }

    private void FixedUpdate()
    {
        // If no movement input is being added then don't rotate character.
        if (movement != Vector2.zero || rotatesWithCamera)
        {
            // Calculates offset if character is strafes with rotation. 
            // Else, strafe should be set with blending tree.
            float strafeOffset = 0f;
            if (!hasStrafeAnimations && movement.x != 0)
            {
                strafeOffset = (movement.x) * strafeRotationOffset;
            }

            // Calculates Euler angles for desired rotation.
            Quaternion targetRotation = Quaternion.Euler(characterGraphics.rotation.eulerAngles.x, lookAt.rotation.eulerAngles.y + strafeOffset, characterGraphics.rotation.eulerAngles.z);
            // Rotates character graphic.
            characterGraphics.rotation = Quaternion.Slerp(characterGraphics.rotation, targetRotation, Time.time * rotationTime);
        }

        // Calculates movve input and sets it to character.
        horizontalMovement = movement.x * lookAt.right;
        forwardMovement = movement.y * characterGraphics.forward;

        rb.velocity = new Vector3(0, rb.velocity.y, 0) + (horizontalMovement + forwardMovement).normalized * movementSpeed;
    }

    /// <summary>
    /// Event callback for when jump input is pressed. Checks if jumping is possible and if it is, allow jump.
    /// </summary>
    /// <param name="obj">Inputs</param>
    private void OnJumpStarted(InputAction.CallbackContext obj)
    {
        // Casts a raycast down to see if it's in contact with a surface
        canJump = Physics.Raycast(transform.position + new Vector3(0, 1, 0), Vector3.down, 1.3f);

        // If can jump, perform jumping behavior.
        if (canJump)
        {
            animator.SetTrigger("onJump");
            rb.velocity += new Vector3(0, jumpHeight, 0);
        }
    }
    #endregion


    #region CAMERA
    /// <summary>
    /// Event callback for look movement. Calculates camera rotation behavior on mouse movement.
    /// </summary>
    /// <param name="obj">Input action callback that contains delta mouse with each event call.</param>
    private void OnLookStarted(InputAction.CallbackContext obj)
    {
        // Returns value of change in x and y
        Vector2 mouseDelta = obj.ReadValue<Vector2>();


        // Needs to swap values and add the rotation to the lookAtObject.
        // Need to swap them because need to rotate them to their respective axis.
        // Multiplies the delta mouse with camera sensitivity to determine gain.
        Vector3 newRotationEuler;
        newRotationEuler = lookAt.rotation.eulerAngles + new Vector3(-mouseDelta.y, mouseDelta.x) * cameraSensitivity;

        // Clamps to defined values so it cannot rotate up and down around character. X and Y are inverted here.
        newRotationEuler = ClampCameraRotation(newRotationEuler);
        

        // Sets look at object to be of the certain rotation.
        lookAt.rotation = Quaternion.Euler(newRotationEuler);
    }
    #endregion

    /// <summary>
    /// For functions that need a reference to the character model animator, this function can be used.
    /// </summary>
    /// <returns></returns>
    public Animator GetAnimator()
    {
        if (animator == null)
            Debug.LogError("Player controller does not have an animator. Please assign one.");

        return animator;
    }


    /// <summary>
    /// Method to clamp rotation to acceptable angles
    /// </summary>
    /// <param name="rotationEuler">Rotation to be clamped in Euler</param>
    /// <returns>Clamped roation</returns>
    public static Vector3 ClampCameraRotation(Vector3 rotationEuler)
    {
        if (rotationEuler.x >= 85 && rotationEuler.x <= 280)
        {
            if (rotationEuler.x >= 85 && rotationEuler.x <= 150)
            {
                rotationEuler.x = 85;
            }
            else if (rotationEuler.x <= 280)
            {
                rotationEuler.x = 280;
            }
        }
        return rotationEuler;
    }
}
