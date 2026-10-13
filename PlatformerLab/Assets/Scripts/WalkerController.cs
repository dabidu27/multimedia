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
    }

    //to compensate for different monitors and different speeds
    private void FixedUpdate()
    {
        //moveInput = -1 (left) => x = -6f => we move with -6 speed => we move to left
        //same for moveInput = 1, and moveInput = 0
        //if moveInput = 0 => x = 0 => move with 0 speed => don't move
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocityY);
    }
}
