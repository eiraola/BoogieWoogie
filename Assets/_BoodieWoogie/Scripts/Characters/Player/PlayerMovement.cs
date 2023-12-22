using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private PlayerInput playerInput;
    [SerializeField]
    private float speed = 1.0f;
    [SerializeField]
    private float gravityForce = 1.6f;
    [SerializeField]
    private float jumpForce = 1.6f;
    [SerializeField]
    private int groundLectureNumber = 4;
    [SerializeField]
    private int lateralLectureNumber = 4;
    [SerializeField]
    private float lectureDistance = 0.1f;
    [SerializeField]
    private LayerMask layersToIgnore = new LayerMask();
    private List<Vector2> collisionDetectionPoints = new List<Vector2>();
    private CapsuleCollider2D playerCollider;
    private Vector2 desiredMovement = new Vector2();
    private Vector2 finalMovement = new Vector2();
    private Bounds colliderBounds;
    

    private void Start()
    {
        SetCollisionDetectionPoints();
    }
    private void SetCollisionDetectionPoints()
    {
        playerCollider = GetComponent<CapsuleCollider2D>();
        if (!playerCollider)
        {
            return;
        }
        colliderBounds = playerCollider.bounds;
    }
    private void OnEnable()
    {
        playerInput.OnMoveEvent += ReadPlayerMovement;
        playerInput.OnJumpEvent += Jump;
    }
    private void OnDisable()
    {
        playerInput.OnMoveEvent -= ReadPlayerMovement;
        playerInput.OnJumpEvent -= Jump;
    }
    private void ReadPlayerMovement(Vector2 value)
    {
        desiredMovement.x = value.x * speed;
    }
    private void Update()
    {
        Move();
        IsGrounded();
    }
    private void CalculateHorizontalVelocity()
    {
        if (IsGrounded() && desiredMovement.y <= 0.0f)
        {
            Debug.LogError("Groudned");
            desiredMovement.y = 0.0f;
            return;
        }
        Debug.LogError("NotGroudned");
        desiredMovement.y -= gravityForce;

    }
    private void Jump()
    {
        if (IsGrounded() )
        {
            desiredMovement.y = jumpForce;
        }
    }
    private bool IsGrounded()
    {
        RaycastHit2D ray;
        colliderBounds = playerCollider.bounds;
        float width = colliderBounds.max.x - colliderBounds.min.x;
        float spaceBetweenLectures = width / (float)groundLectureNumber;
        collisionDetectionPoints = new List<Vector2>();
        for (int i = 0; i < groundLectureNumber; i++)
        {
            ray = Physics2D.Raycast(colliderBounds.min.ToVector2() + Vector2.right * spaceBetweenLectures * i, Vector2.down, lectureDistance, ~layersToIgnore);
            Debug.DrawLine(colliderBounds.min.ToVector2() + Vector2.right * spaceBetweenLectures * i, colliderBounds.min.ToVector2() + Vector2.right * spaceBetweenLectures * i + Vector2.down * lectureDistance);
            if (ray.collider == null)
            {
                continue;
            }
            Debug.LogError(ray.collider.name);
            return true;
  
        }
        ray = Physics2D.Raycast(colliderBounds.min.ToVector2() + Vector2.right * width, Vector2.down, lectureDistance, ~layersToIgnore);
        Debug.DrawLine(colliderBounds.min.ToVector2() + Vector2.right * width, colliderBounds.min.ToVector2() + Vector2.right * width + Vector2.down * lectureDistance);
        if (ray.collider == null)
        {
            return false;
        }
        return true;

    }
    private bool HorizontalCollisionCheck(Vector2 direction)
    {
        Vector2 origin;
        float width = colliderBounds.max.y - colliderBounds.min.y;
        if (direction == Vector2.right)
        {
            origin = Vector2.up * colliderBounds.min + Vector2.right * colliderBounds.max;
            return CheckCollisions(origin, Vector2.up, Vector2.right, width, lectureDistance, lateralLectureNumber);
        }
        origin = Vector2.up * colliderBounds.min + Vector2.right * colliderBounds.min;
        return CheckCollisions(origin, Vector2.up, Vector2.left, width, lectureDistance, lateralLectureNumber);
    }
    private bool CheckCollisions(Vector2 origin, Vector2 lectureDirection, Vector2 direction, float width,  float distance, int numLectures)
    {
        RaycastHit2D ray;
        colliderBounds = playerCollider.bounds;
        float spaceBetweenLectures = width / (float)numLectures;
        collisionDetectionPoints = new List<Vector2>();
        for (int i = 0; i < numLectures; i++)
        {
            ray = Physics2D.Raycast(origin + lectureDirection * spaceBetweenLectures * i, direction, distance, ~layersToIgnore);
            Debug.DrawLine(origin + lectureDirection * spaceBetweenLectures * i, colliderBounds.min.ToVector2() + lectureDirection * spaceBetweenLectures * i + direction * distance);
            if (ray.collider == null)
            {
                continue;
            }
            Debug.LogError(ray.collider.name);
            return true;

        }
        ray = Physics2D.Raycast(origin + lectureDirection * width, direction, distance, ~layersToIgnore);
        Debug.DrawLine(origin + lectureDirection * width, colliderBounds.min.ToVector2() + lectureDirection * width + direction * distance);
        if (ray.collider == null)
        {
            return false;
        }
        return true;
    }
    private void Move()
    {
        CalculateHorizontalVelocity();
        finalMovement = desiredMovement;
        if (HorizontalCollisionCheck((desiredMovement * Vector2.right).normalized))
        {
            finalMovement.x = 0.0f;
        }
        transform.position += finalMovement.ToVector3() * Time.deltaTime;
    }
    private void OnDrawGizmos()
    {
        if (!playerCollider)
        {
            return;
        }
        Bounds colliderBounds = playerCollider.bounds;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(colliderBounds.max, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(colliderBounds.min, 0.1f);
        float width = colliderBounds.max.x - colliderBounds.min.x;
        Gizmos.color = Color.green;
        foreach (Vector2 item in collisionDetectionPoints)
        {
            Gizmos.DrawSphere(item.ToVector3(), 0.1f);
        }
    }
}
