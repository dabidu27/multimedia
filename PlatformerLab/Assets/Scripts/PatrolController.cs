using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
       

    //the edgeCheckOffset and wallCheckOffset push 
    [Header("Sensors")]
    [SerializeField] private Vector2 edgeCheckOffset = new Vector2(0.45f, 0.1f);
    [SerializeField] private float edgeCheckDistance = 0.3f;
    [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.45f, 0.45f);
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private int direction = 1;

    //the edgeCheckOffset and wallCheckOffset push the sensor lines a bit in front of the patroler, relative to its current position
    private Vector2 EdgeOrigin => (Vector2)transform.position
        + new Vector2(edgeCheckOffset.x * direction, edgeCheckOffset.y);
    private Vector2 WallOrigin => (Vector2)transform.position
        + new Vector2(wallCheckOffset.x * direction, wallCheckOffset.y);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.flipX = direction > 0;   // the frames face left; it starts walking right
    }

    void FixedUpdate()
    {   
        //We use the Raycast as a sensor, to check if the patroller is still on the ground that it does not touches walls
        //We cannot use the box collider, as the patroller would be detected off the ground too late, when it is already falling - we need to detect it a bit before
        //Also, the box collider cannot detect walls separately, as the walls and the ground are part of the same Ground layer

        //The sensors are 2 lines, one vertical, a bit in front of the patroler (to check for ground), and one horizontal, also a bit in front (to check for walls)

        //edgeCheckDistance, wallCheckDistance = how long the lines are

        //The Raycast function returns a chunk of data about what it hit. If the collider property is != null, it means it actually hit and return something so Physics2D.Raycast(...) != null will return true
        //Physics2D.Raycast casts the "invisible lines" we mentioned above: Physics2D.Raycast(the start of the line, the direction it goes, how long it is, the layer from which it detects object)

        bool groundAhead = Physics2D.Raycast(
            EdgeOrigin, Vector2.down, edgeCheckDistance, groundLayer).collider != null;
        bool wallAhead = Physics2D.Raycast(
            WallOrigin, Vector2.right * direction, wallCheckDistance, groundLayer).collider != null;

        //If the patrol is about to fall or to hit a wall, flip the direction of movement
        if (!groundAhead || wallAhead)
        {
            Flip();
        }
        
        //and make the patroler move at each fixed update (to maintain the same speed for each device)
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    private void Flip()
    {
        direction *= -1;
        spriteRenderer.flipX = direction > 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(EdgeOrigin, EdgeOrigin + Vector2.down * edgeCheckDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(WallOrigin, WallOrigin + Vector2.right * direction * wallCheckDistance);
    }
}