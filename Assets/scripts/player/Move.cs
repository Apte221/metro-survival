using UnityEngine;

public class Move2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private SpriteRenderer spr;

    [Header("Ledge Climb")]
    [SerializeField] private Transform ledgeCheck;                 // точка біля верхньої частини тіла (біля голови)
    [SerializeField] private Vector2 ledgeCheckSize = new Vector2(0.2f, 0.2f);
    [SerializeField] private float ledgeClimbDuration = 0.18f;     // час "залазіння"
    [SerializeField] private Vector2 ledgeClimbOffset = new Vector2(0.55f, 1.05f); // куди ставимо гравця (відносно стіни)
    private bool isLedgeClimbing;
    private float ledgeClimbTimer;
    private Vector2 ledgeTargetPos;

    [Header("Ability Toggles")]
    [SerializeField] private bool enableWallInteraction = true; // wall slide + wall push



    [Header("Checks")]
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.8f);

    [Header("Move")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float groundAccel = 80f;
    [SerializeField] private float groundDecel = 90f;
    [SerializeField] private float airAccel = 55f;

    [Header("Jump")]
    [SerializeField] private float jumpSpeed = 14f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBuffer = 0.12f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
        
    [Header("Gravity")]
    [SerializeField] private float fallGravityMultiplier = 2.2f;
    [SerializeField] private float maxFallSpeed = 22f;

    [Header("Wall Slide / Cling")]
    [SerializeField] private float wallSlideSpeed = 3.5f;
    [SerializeField] private float wallClingTime = 0.10f;
    [SerializeField] private float wallClingFallSpeed = 0f;

    [Header("Wall Push (replaces jump)")]
    [SerializeField] private float wallPushXSpeed = 10f;
    [SerializeField] private float wallPushYSpeed = 12f;

    [Header("Anti Wall-Climb Lock")]
    [SerializeField] private float wallDetachTime = 0.12f;       // не чіплятися до стіни після push
    [SerializeField] private float wallControlLockTime = 0.16f;  // короткий lock X після push
    private float wallDetachTimer;
    private float wallControlLockTimer;
    private int lastWallPushDir; // напрямок відштовхування (-1/ +1)

    // input
    private float inputX;
    private bool jumpDown;
    private bool jumpUp;

    // timers
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float wallClingTimer;

    // state
    private bool ignoreJumpCutThisRise;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spr == null) spr = GetComponent<SpriteRenderer>();

    }

    private void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpDown = true;

        if (Input.GetButtonUp("Jump"))
            jumpUp = true;

        if (inputX > 0)
        {
            spr.flipX = false;
        }
        else if (inputX < 0)
        {
            spr.flipX = true;
        }


        // Coyote time
        if (IsGrounded())
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Jump buffer
        if (jumpDown)
            jumpBufferTimer = jumpBuffer;
        else
            jumpBufferTimer -= Time.deltaTime;

        // Detach / lock timers
        if (wallDetachTimer > 0f)
            wallDetachTimer -= Time.deltaTime;

        if (wallControlLockTimer > 0f)
            wallControlLockTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {

        if (isLedgeClimbing)
        {
            UpdateLedgeClimb();
            return;
        }

        bool grounded = IsGrounded();
        bool touchingWall = IsTouchingWall(out int wallSide); // -1 left, +1 right, 0 none

        ApplyHorizontal(grounded, touchingWall, wallSide);
        ApplyJumpAndWallPush(grounded, touchingWall, wallSide);
        ApplyBetterGravity();
        ApplyWallClingSlide(grounded, touchingWall, wallSide);
        TryStartLedgeClimb();

        // consume one-frame inputs
        jumpDown = false;
        jumpUp = false;

        // reset ignore jump-cut when falling or grounded
        if (grounded || rb.velocity.y <= 0f)
            ignoreJumpCutThisRise = false;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    private bool IsTouchingWall(out int side)
    {
        side = 0;

        // під час detach ми спеціально “не бачимо” стіни
        if (wallDetachTimer > 0f)
            return false;

        bool left = wallCheckLeft != null &&
                    Physics2D.OverlapBox(wallCheckLeft.position, wallCheckSize, 0f, groundLayer);

        bool right = wallCheckRight != null &&
                     Physics2D.OverlapBox(wallCheckRight.position, wallCheckSize, 0f, groundLayer);

        if (left) side = -1;
        else if (right) side = 1;

        return left || right;
    }

    private void ApplyHorizontal(bool grounded, bool touchingWall, int wallSide)
    {
        // після wall push короткий час блокуємо керування X, щоб не "втиснутися" назад у стіну
        if (wallControlLockTimer > 0f)
        {
            float lockedTargetX = lastWallPushDir * wallPushXSpeed;
            float lockedX = Mathf.MoveTowards(rb.velocity.x, lockedTargetX, airAccel * 2f * Time.fixedDeltaTime);
            rb.velocity = new Vector2(lockedX, rb.velocity.y);
            return;
        }

        bool pushingIntoWall = !grounded && touchingWall && wallSide != 0
                               && Mathf.Abs(inputX) > 0.01f
                               && Mathf.Sign(inputX) == wallSide;

        // якщо тиснемо В стіну в повітрі — не даємо X йти в колайдер
        float targetX = pushingIntoWall ? 0f : inputX * moveSpeed;

        // ✅ ГОЛОВНИЙ ФІКС ПРО “втрачає всю швидкість в повітрі”:
        // у повітрі, коли НЕМА інпуту (inputX≈0), ми НЕ тягнемо швидкість до 0 (або тягнемо дуже слабо).
        float accelStep;

        if (Mathf.Abs(inputX) > 0.01f)
        {
            // є інпут -> керуємо швидкістю
            accelStep = (grounded ? groundAccel : airAccel) * Time.fixedDeltaTime;
        }
        else
        {
            // нема інпуту -> на землі гальмуємо, в повітрі зберігаємо інерцію
            accelStep = (grounded ? groundDecel : 0f) * Time.fixedDeltaTime;
        }

        float x = Mathf.MoveTowards(rb.velocity.x, targetX, accelStep);
        rb.velocity = new Vector2(x, rb.velocity.y);
    }

    private void ApplyJumpAndWallPush(bool grounded, bool touchingWall, int wallSide)
    {
        if (jumpBufferTimer <= 0f) return;

        // --- WALL PUSH replaces jump ---
        if (enableWallInteraction && !grounded && touchingWall && wallSide != 0)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            int pushDir = -wallSide;
            lastWallPushDir = pushDir;

            rb.velocity = new Vector2(pushDir * wallPushXSpeed, wallPushYSpeed);

            ignoreJumpCutThisRise = true;          // не застосовувати jump-cut на цьому підйомі
            wallDetachTimer = wallDetachTime;      // не “бачити” стіну
            wallControlLockTimer = wallControlLockTime; // lock X
            wallClingTimer = 0f;                   // щоб cling/slide не з’їв рух
            return;
        }

        // --- NORMAL JUMP ---
        if (coyoteTimer > 0f)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            ignoreJumpCutThisRise = false;
        }
    }

    private void ApplyBetterGravity()
    {
        // Jump cut (коротший стрибок) — тільки для normal jump
        if (jumpUp && rb.velocity.y > 0f && !ignoreJumpCutThisRise)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }

        // швидше падіння
        if (rb.velocity.y < 0f)
        {
            float extra = (fallGravityMultiplier - 1f) * Physics2D.gravity.y;
            rb.velocity += Vector2.up * extra * Time.fixedDeltaTime;
        }

        // ліміт падіння
        if (rb.velocity.y < -maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
    }

    private void ApplyWallClingSlide(bool grounded, bool touchingWall, int wallSide)
    {
        if (!enableWallInteraction)
        {
            wallClingTimer = 0f;
            return;
        }

        if (grounded) { wallClingTimer = 0f; return; }
        if (!touchingWall || wallSide == 0) { wallClingTimer = 0f; return; }

        if (rb.velocity.y > 0f) return;

        if (wallClingTimer <= 0f)
            wallClingTimer = wallClingTime;

        if (wallClingTimer > 0f)
        {
            wallClingTimer -= Time.fixedDeltaTime;
            if (rb.velocity.y < wallClingFallSpeed)
                rb.velocity = new Vector2(rb.velocity.x, wallClingFallSpeed);
        }
        else
        {
            if (rb.velocity.y < -wallSlideSpeed)
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
    }



    private bool IsTouchingWallAtPoint(Vector2 worldPos, Vector2 size, out int side)
    {
        side = 0;

        // Перевіряємо коробкою праворуч і ліворуч від точки (щоб знати сторону)
        Vector2 rightPos = worldPos + Vector2.right * (size.x * 0.6f);
        Vector2 leftPos = worldPos + Vector2.left * (size.x * 0.6f);

        bool right = Physics2D.OverlapBox(rightPos, size, 0f, groundLayer);
        bool left = Physics2D.OverlapBox(leftPos, size, 0f, groundLayer);

        if (left) side = -1;
        else if (right) side = 1;

        return left || right;
    }

    private bool IsLedgeDetected(out int ledgeSide)
    {
        ledgeSide = 0;
        if (ledgeCheck == null) return false;

        // 1) Середина торкається стіни (в тебе це вже є)
        bool wallMid = IsTouchingWall(out int midSide);
        if (!wallMid || midSide == 0) return false;

        // 2) Верхня точка НЕ торкається стіни -> значить є край (виступ)
        bool wallTop = IsTouchingWallAtPoint(ledgeCheck.position, ledgeCheckSize, out int topSide);

        // Виступ: середина в стіні, а верх — ні
        if (!wallTop)
        {
            ledgeSide = midSide;
            return true;
        }

        return false;
    }


    private void TryStartLedgeClimb()
    {
        if (isLedgeClimbing) return;

        // умови: в повітрі, не летимо вгору
        if (IsGrounded()) return;
        if (rb.velocity.y > 0.1f) return;

        // спочатку визначаємо, чи є виступ, і з якої сторони стіна
        if (!IsLedgeDetected(out int side)) return;

        // потрібно тиснути В СТІНУ, щоб хапатися за виступ
        if (Mathf.Abs(inputX) < 0.01f) return;
        if (Mathf.Sign(inputX) != side) return; // side = сторона стіни (-1/+1)

        // Стартуємо залазіння
        isLedgeClimbing = true;
        ledgeClimbTimer = ledgeClimbDuration;

        // Зупиняємо фізику на час залазіння
        rb.velocity = Vector2.zero;
        rb.simulated = false;

        // Розрахунок точки, куди поставити гравця:
        float dir = side; // якщо стіна справа (+1), залазимо вліво (-), і навпаки
        Vector2 offset = new Vector2(ledgeClimbOffset.x * dir, ledgeClimbOffset.y);

        ledgeTargetPos = (Vector2)ledgeCheck.position + offset;
    }



    private void UpdateLedgeClimb()
    {
        if (!isLedgeClimbing) return;

        ledgeClimbTimer -= Time.fixedDeltaTime;

        // Можна зробити плавне переміщення (Lerp), але простіше — телепорт в кінці.
        if (ledgeClimbTimer <= 0f)
        {
            transform.position = ledgeTargetPos;

            rb.simulated = true;
            isLedgeClimbing = false;
        }
    }




    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }

        Gizmos.color = Color.cyan;
        if (wallCheckLeft != null) Gizmos.DrawWireCube(wallCheckLeft.position, wallCheckSize);
        if (wallCheckRight != null) Gizmos.DrawWireCube(wallCheckRight.position, wallCheckSize);

        if (ledgeCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(ledgeCheck.position, ledgeCheckSize);
        }
    }
}
