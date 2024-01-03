using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class PlayerMovement : MonoBehaviour
{
    
    [SerializeField]
    private PlayerInput playerInput;
    [SerializeField]
    private float speed = 1.0f;
    [SerializeField]
    private int groundLectureNumber = 4;
    [SerializeField]
    private int lateralLectureNumber = 4;
    [SerializeField]
    private float lectureDistance = 0.1f;
    [SerializeField]
    private LayerMask layersToIgnore = new LayerMask();
    [SerializeField]
    private float jumpHeigth = 0.0f;
    [SerializeField]
    private float timeToJumpApex = 0.0f;
    [SerializeField]
    private float CoyoteTimePeriod = 0.1f;
    [SerializeField]
    private float jumpInputBuffer = 0.1f;
    [SerializeField]
    private float appexPoint = 0.1f;
    private float gravityForce = 1.6f;
    private float jumpForce = 1.6f;
    private List<Vector2> collisionDetectionPoints = new List<Vector2>();
    private CapsuleCollider2D playerCollider;
    private Vector2 desiredMovement = new Vector2();
    private Vector2 finalMovement = new Vector2();
    private Bounds colliderBounds;
    private float finalGravityForce = 0.0f;
    private float lastTimeGrounded = 0.0f;
    private float lastTimeJumpPressed = float.MaxValue;
    private bool isGrounded = false;
    private bool jumpBufferActive = false;
    private bool jumpButtonPressed = false;
    private bool CanJump
    {
        get
        {
            return (Time.time - lastTimeGrounded < CoyoteTimePeriod);
        }
    }


    private void Start()
    {
        SetCollisionDetectionPoints();
        gravityForce = -(2 * jumpHeigth) / Mathf.Pow(timeToJumpApex, 2);
        jumpForce = Mathf.Abs(gravityForce) * timeToJumpApex;
        finalGravityForce = gravityForce;
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
        playerInput.OnJumpStopEvent += JumpStop;
    }
    private void OnDisable()
    {
        playerInput.OnMoveEvent -= ReadPlayerMovement;
        playerInput.OnJumpEvent -= Jump;
        playerInput.OnJumpStopEvent -= JumpStop;
    
    }
    private void ReadPlayerMovement(Vector2 value)
    {
        desiredMovement.x = value.x * speed;
    }
    private void Update()
    {
        if (GameManager.Instance.IsStoped)
        {
            return;
        }
       
        CheckJumpBuffer();
        Move();
    }
    private float CalculateVerticalVelocity()
    {
        finalGravityForce = gravityForce;
        //if (isGrounded && desiredMovement.y <= 0.0f)
        //{
        //    return 0.0f;
        //}
        if (Mathf.Abs(desiredMovement.y) < appexPoint && jumpButtonPressed)
        {
            finalGravityForce = gravityForce / 10;
        }
        if (desiredMovement.y < 0.0f)
        {
            finalGravityForce = gravityForce * 2;
        }
        return desiredMovement.y + finalGravityForce * Time.deltaTime;

    }
    private void Jump()
    {
        jumpButtonPressed = true;
        JumpAction();
    }
    private void JumpAction()
    {
        
        if (CanJump)
        {
            desiredMovement.y = jumpForce;
            jumpBufferActive = false;
            return;
        }
        lastTimeJumpPressed = Time.time;
    }
    private void JumpStop()
    {
        jumpButtonPressed = false;
        if (desiredMovement.y > 0.0f)
        {
            desiredMovement.y = desiredMovement.y / 2;
        }
    }
    private float MaxVerticalSpeed(float currentSpeed)
    {
        if (currentSpeed > 0.0f)
        {
            return currentSpeed;
        }
        RaycastHit2D ray;
        float auxCurrentSpeed = Mathf.Abs(currentSpeed);
        colliderBounds = playerCollider.bounds;
        float width = colliderBounds.max.x - colliderBounds.min.x;
        float spaceBetweenLectures = width / (float)groundLectureNumber;
        collisionDetectionPoints = new List<Vector2>();
        for (int i = 0; i <= groundLectureNumber; i++)
        {
            ray = Physics2D.Raycast(colliderBounds.min.ToVector2() + Vector2.right * spaceBetweenLectures * i, Vector2.down, auxCurrentSpeed, ~layersToIgnore);
            Debug.DrawLine(colliderBounds.min.ToVector2() + Vector2.right * spaceBetweenLectures * i, colliderBounds.min.ToVector2() + Vector2.right * spaceBetweenLectures * i + Vector2.down * auxCurrentSpeed );
            if (ray.collider != null)
            {
                auxCurrentSpeed = ray.distance;
            }
            //lastTimeGrounded = Time.time;
            //ShouldJumpAfterFall();
        }
        return auxCurrentSpeed * Mathf.Sign(currentSpeed);
    }
    private void ShouldJumpAfterFall()
    {
        if (isGrounded)
        {
            return;
        }
        if (Mathf.Abs(Time.time - lastTimeJumpPressed) < jumpInputBuffer)
        {
            jumpBufferActive = true;
        }
    }
    private void CheckJumpBuffer()
    {
        if (jumpBufferActive)
        {
            JumpAction();
        }
    }
    private bool HorizontalCollisionCheck(Vector2 direction)
    {
        Vector2 origin;
        float width = colliderBounds.max.y - colliderBounds.min.y;
        if (direction == Vector2.right)
        {
            origin = Vector2.up * colliderBounds.min + Vector2.up * 0.025f + Vector2.right * colliderBounds.max;
            return CheckCollisions(origin, Vector2.up, Vector2.right, width, lectureDistance, lateralLectureNumber);
        }
        origin = Vector2.up * colliderBounds.min + Vector2.up * 0.025f + Vector2.right * colliderBounds.min;
        return CheckCollisions(origin, Vector2.up, Vector2.left, width, lectureDistance, lateralLectureNumber);
    }
    private bool CheckCollisions(Vector2 origin, Vector2 lectureDirection, Vector2 direction, float width,  float distance, int numLectures)
    {
        RaycastHit2D ray;
        colliderBounds = playerCollider.bounds;
        float spaceBetweenLectures = width / (float)numLectures;
        collisionDetectionPoints = new List<Vector2>();
        for (int i = 0; i <= numLectures; i++)
        {
            ray = Physics2D.Raycast(origin + lectureDirection * spaceBetweenLectures * i, direction, distance, ~layersToIgnore);
            Debug.DrawLine(origin + lectureDirection * spaceBetweenLectures * i, origin + lectureDirection * spaceBetweenLectures * i + direction * distance);
            if (ray.collider == null)
            {
                continue;
            }
            return true;

        }
        return false;
    }
    private void Move()
    {
        desiredMovement.y = CalculateVerticalVelocity();
        transform.position += ContraintMovement(desiredMovement * Time.deltaTime).ToVector3();
    }
    private Vector2 ContraintMovement(Vector2 currentMovement)
    {
        if (HorizontalCollisionCheck((currentMovement * Vector2.right).normalized))
        {
            currentMovement.x = 0.0f;
        }
        currentMovement.y = MaxVerticalSpeed(currentMovement.y);
        CheckIfGrounded(currentMovement);
        return currentMovement;
    }
    private void CheckIfGrounded(Vector2 currentMovement)
    {
        if (currentMovement.y == 0.0f)
        {
            if (desiredMovement.y < 0.0f)
            {
                //We just landed, do the input buffer check here
            }
            lastTimeGrounded = Time.time;
            desiredMovement.y = 0.0f;
        }
    }
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up * 2, desiredMovement.y.ToString()) ;
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
