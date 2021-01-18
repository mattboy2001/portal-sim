
//Directives
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Customisable attributes to alter movement
    [SerializeField] Transform playerCamera = null;
    [SerializeField] float mouseSensitivity = 3f;
    [SerializeField] float walkSpeed = 6.0f;

    [SerializeField] float gravity = -13.0f;

    //Hide cursor
    [SerializeField] bool lockCursor = true;


    //Motion smoothing
    [SerializeField][Range(0.0f, 0.5f)] float moveSmoothTime = 0.3f;


    
    [SerializeField][Range(0.0f, 0.5f)] float mouseSmoothTime = 0.03f;



    //Used to move the character's transform
    CharacterController controller;


    //Camera angle
    float cameraPitch = 0.0f;


    //Controls gravitational movement
    float velocityY = 0.0f;


    //Movements for the player and camera

    Vector2 currentDir = Vector2.zero;

    Vector2 currentDirVelocity = Vector2.zero;


    Vector2 currentMouseDelta = Vector2.zero;

    Vector2 currentMouseDeltaVelocity = Vector2.zero;


    void Start()
    {
        //Get the character controller
        controller = GetComponent<CharacterController>();

        //Hide the cursor if it is locked
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
        
    }

    void Update()
    {
        UpdateMouseMovement();

        UpdateMovement();
    }


    void UpdateMouseMovement()
    {

        //Get the mouse input
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));


        //Smooth the camera movement relative to mouse movement
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);


        //Set the camera y angle
        cameraPitch -= currentMouseDelta.y * mouseSensitivity;


        //Clamp between these angles to disable the player from looking over or under the character
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);


        //Alter the camera angle
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;

        //Rotate the x angle of the camera
        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);
    }


    void UpdateMovement()
    {
        
        //Get the input target direction
        Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        //Normalize the vector's magnitude to 1
        targetDir.Normalize();


        //Smooth the movement
        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);

        //Check if we should apply gravity to the player
        if (controller.isGrounded)
        {
            //Set to zero to stop player movement in the y direction
            velocityY = 0.0f;
        }
        //Apply gravity
        velocityY += gravity * Time.deltaTime;

        //Move the character controller
        Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * walkSpeed + Vector3.up * velocityY;

        controller.Move(velocity * Time.deltaTime);



        

    }
}