using UnityEngine;
using UnityEngine.InputSystem;

//Don't forget to add the WalkerController script to Walker's componenets inside the Unity editor
public class WalkerController : MonoBehaviour
{

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private InputAction moveAction;

    private float moveInput;
     
    //the 2 decorators make Movement a component inside the Unity editor, with a moveSpeed property, so we can change it inside the editor
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 16.6f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float coyoteTime = 0.1f;
    

    private InputAction jumpAction;
    private bool jumpRequested;
    private bool jumpCutRequested;
    private bool isGrounded;

    private float coyoteTimer;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int GroundedHash = Animator.StringToHash("IsGrounded");

    private Animator animator;

    //called once, before the game stars
    void Awake()
    {
        //get references to the components of our Walker, which we set in the Unity editor
        //RigidBody is the component that handles the physics
        rb = GetComponent<Rigidbody2D>(); 
        //spriteRenderer is the component that renders the Walker object
        spriteRenderer = GetComponent<SpriteRenderer>();
        //InputSystem covers the entire user interaction, it listens to our keyboard and it triggers an event
        moveAction = InputSystem.actions.FindAction("Move");
        //output the fact we have access to the componenets
        //Debug.Log($"Awake: rb = {rb != null}, spriteRenderer = {spriteRenderer != null}, moveAction = { moveAction != null}");
        jumpAction = InputSystem.actions.FindAction("Jump");

        animator = GetComponent<Animator>();

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        //Vector2 is a vector of 2 values, x-coordinate and y-coordinate
        moveInput = moveAction.ReadValue<Vector2>().x; //get the x-axis coordinate of moving
        //Debug.Log($"x-axis = {moveInput}");

        //if we are moving to the left x-axis = -1, when we are moving to the right, x-axis = 1, when we don't move, x-axis = 0
        if(moveInput != 0f)
        {
            //move input = 1 (right) => true, move input = -1 (left) => left
            //if we move to the left, we change the side the walker is "looking at", by flipping its x-coordinate
            spriteRenderer.flipX = moveInput > 0f;
        }

        //if space was pressed in this frame, save it
        if (jumpAction.WasPerformedThisFrame())
        {
            jumpRequested = true;
        }
           
        //also save if in the same frame, the space bar was released
        if (jumpAction.WasReleasedThisFrame())
        {
            jumpCutRequested = true;
        }

        animator.SetFloat(SpeedHash, Mathf.Abs(rb.linearVelocityX));
        animator.SetBool(GroundedHash, isGrounded);
    }

    //to compensate for different monitors and different speeds
    private void FixedUpdate()
    {
        //moveInput = -1 (left) => x = -6f => we move with -6 speed => we move to left
        //same for moveInput = 1, and moveInput = 0
        //if moveInput = 0 => x = 0 => move with 0 speed => don't move
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocityY);

        //preserve state
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null; //check if the 2 layers overlap

        //if the walker is grounded, reset the coyoteTimer
        if (isGrounded)
            coyoteTimer = coyoteTime;
        //if the walker is in air, start reducing the coyoteTimer
        else
            coyoteTimer -= Time.fixedDeltaTime;
        if (isGrounded != wasGrounded) //if state changed
            Debug.Log($"isGrounded -> {isGrounded}"); //the log runs only when the state changes

        //if we are on the ground and a jump was requested, move y up
        //if(jumpRequested && isGrounded)
        //now, instead of using isGrounded for the jump, we use coyoteTimer > 0f - if we check isGrounded, we can only jump while being on the ground; now, we have the coyoteTimer (0.1s) of leeway of jumping "in the air"
        if(jumpRequested && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            coyoteTimer = 0f; //consume the coyote timer so we cannot jump twice
        }

        //if in the same frame, the space bar was also released, cut the jump by half
        if(jumpCutRequested && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        //clear the flags
        jumpRequested = false;
        jumpCutRequested = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;
        Gizmos.color = Color.yellow;
        //Draw a circle at the position of the ground check
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
