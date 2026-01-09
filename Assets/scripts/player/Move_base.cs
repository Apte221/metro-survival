using UnityEngine;

/// <summary>
/// Головний контролер руху персонажа 2D-платформера:
/// - ходьба/біг
/// - стрибок
/// - coyote time (після сходу з землі ще можна стрибнути)
/// - jump buffer (натиснув Jump перед приземленням — стрибок виконається)
/// - ledge hang / ledge climb (хапання за виступ + підтягування)
/// - передача параметрів в Animator
/// </summary>
public class PlayerMovement2D : MonoBehaviour
{
    // Простий state-machine: щоб різні режими руху не конфліктували між собою.
    private enum State
    {
        Normal,     // звичайний рух: біг/стрибок/падіння
        LedgeHang,  // висимо на виступі
        LedgeClimb  // відбувається анімація підтягування (фізика вимкнена)
    }

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;      // фізика персонажа
    [SerializeField] private Animator anim;       // аніматор (можна null, якщо ще немає)
    [SerializeField] private LedgeGrabber ledge;  // окремий компонент для пошуку виступів

    [Header("Move")]
    [SerializeField] private float moveSpeed = 8f; // швидкість бігу

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;        // сила стрибка
    [SerializeField] private float coyoteTime = 0.12f;     // "вікно" койота
    [SerializeField] private float jumpBufferTime = 0.12f; // "вікно" буфера

    // NEW: “урізання” стрибка при ранньому відпусканні кнопки
    [SerializeField, Range(0f, 1f)]
    private float jumpCutMultiplier = 0.5f;
    // 0.5 = відпустив Jump рано → вдвічі менше "підльоту"

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;                 // точка перевірки землі
    [SerializeField] private Vector2 groundCheckSize = new(0.55f, 0.15f); // розмір бокса
    [SerializeField] private LayerMask groundMask;                  // що вважаємо землею



    // Поточний режим
    private State state = State.Normal;

    // ===== INPUT =====
    private float inputX;               // -1..0..1 рух по X
    private bool jumpPressedThisFrame;  // натиснення Jump в цьому кадрі (одноразове)
    private bool jumpReleasedThisFrame;  // NEW: відпустив Jump в цьому кадрі


    // ===== CHECKS / TIMERS =====
    private bool isGrounded;      // стоїмо на землі?
    private float coyoteTimer;    // таймер "ще можна стрибнути після сходу"
    private float bufferTimer;    // таймер "ми пам’ятаємо натиснутий Jump"

    // Куди дивиться персонаж: 1 вправо, -1 вліво
    private int facing = 1;

    // Unity викликає Reset, коли ти додаєш компонент або натискаєш Reset в інспекторі
    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        ledge = GetComponent<LedgeGrabber>();
    }

    private void Update()
    {
        // 1) Зчитуємо інпут (в Update — правильно для Input)
        ReadInput();

        // 2) Перевіряємо землю (в Update можна, це лише OverlapBox)
        UpdateGroundCheck();

        // 3) Оновлюємо таймери койота і буфера
        UpdateJumpTimers();

        // 4) Логіка залежить від стану
        switch (state)
        {
            case State.Normal:
                HandleFacing();     // фліп спрайта
                TryConsumeJump();   // намагаємось зробити стрибок з урахуванням таймерів
                ApplyJumpCutIfNeeded();
                TryStartLedgeHang();// якщо падаємо біля виступу — хапаємось
                break;

            case State.LedgeHang:
                // На виступі фізикою не рухаємося.
                // Тут ти вирішуєш: Jump = підтягування, або Jump = відстрибнути.
                if (jumpPressedThisFrame)
                {
                    Debug.Log("Ledge Climb");
                    StartLedgeClimb();
                }
                break;

            case State.LedgeClimb:
                // Поки йде підтягування — зазвичай керує анімація.
                // Завершення робиться Animation Event-ом.
                break;
        }

        // 5) Передаємо параметри в Animator
        UpdateAnimator();

        // 6) Скидаємо одноразове натискання
        jumpPressedThisFrame = false;
        jumpReleasedThisFrame = false;
    }

    private void FixedUpdate()
    {
        // Фізичний рух краще робити в FixedUpdate.
        if (state != State.Normal) return;

        // Встановлюємо швидкість по X, Y залишаємо як є.
        rb.velocity = new Vector2(inputX * moveSpeed, rb.velocity.y);
    }

    // ---------------- INPUT ----------------

    private void ReadInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        // натиснув Jump
        if (Input.GetButtonDown("Jump"))
            jumpPressedThisFrame = true;

        // NEW: відпустив Jump
        if (Input.GetButtonUp("Jump"))
            jumpReleasedThisFrame = true;

        // Якщо Jump натиснули — запускаємо jump buffer
        if (jumpPressedThisFrame)
            bufferTimer = jumpBufferTime;
    }


    // ---------------- CHECKS ----------------

    private void UpdateGroundCheck()
    {
        // Перевірка "чи є земля" під персонажем.
        // OverlapBox повертає колайдер, якщо є перетин з groundMask.
        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0f,
            groundMask
        );
    }

    // ---------------- TIMERS ----------------

    private void UpdateJumpTimers()
    {
        // Coyote: якщо на землі — таймер скидається.
        // Якщо в повітрі — таймер зменшується.
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Buffer: якщо буфер активний — зменшуємо.
        // (Якщо натиснув Jump, ми вже виставили bufferTimer в ReadInput)
        if (bufferTimer > 0f)
            bufferTimer -= Time.deltaTime;
    }

    // ---------------- MOVEMENT HELPERS ----------------

    private void HandleFacing()
    {
        // Якщо немає руху — не міняємо напрям
        if (Mathf.Abs(inputX) < 0.01f) return;

        // Визначаємо напрям
        int newFacing = inputX > 0 ? 1 : -1;

        // Якщо змінився — фліпаємо scale.x
        if (newFacing != facing)
        {
            facing = newFacing;

            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * facing;
            transform.localScale = s;
        }
    }

    private void TryConsumeJump()
    {
        // Ми можемо стрибнути лише якщо:
        // 1) буфер активний (натиск Jump "запам’ятався")
        // 2) ми на землі або ще в койот-таймі
        // 3) ми в Normal стані (ми тут і так в Normal)
        if (bufferTimer <= 0f) return;

        bool canJump = isGrounded || coyoteTimer > 0f;
        if (!canJump) return;

        // "Споживаємо" буфер і койот, щоб не було подвійних стрибків
        bufferTimer = 0f;
        coyoteTimer = 0f;

        // Обнуляємо Y-швидкість щоб стрибок був однаковим,
        // навіть якщо падав/рухався по Y
        rb.velocity = new Vector2(rb.velocity.x, 0f);

        // Додаємо імпульс вгору
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Анімація стрибка (тригер)
        if (anim) anim.SetTrigger("Jump");
    }

    // ---------------- LEDGE ----------------

    private void TryStartLedgeHang()
    {
        // Якщо немає компонента — нема що робити
        if (!ledge) return;

        // На землі не хапаємось
        if (isGrounded) return;

        // Зазвичай хапаємося, коли падаємо або майже падаємо
        if (rb.velocity.y > 0.1f) return;

        // Питаємо LedgeGrabber: чи є виступ?
        if (ledge.TryGetLedge(out Vector2 hangPos))
        {
            state = State.LedgeHang;

            // Зупиняємо фізику
            rb.velocity = Vector2.zero;

            // Робимо тіло кінематичним — щоб гравітація/колізії не тягнули персонажа
            rb.bodyType = RigidbodyType2D.Kinematic;

            // Ставимо в точку "висіння"
            transform.position = hangPos;

            // Анімація
            if (anim) anim.SetBool("LedgeHang", true);
        }
    }

    private void StartLedgeClimb()
    {
        // Перехід у підтягування
        state = State.LedgeClimb;

        // Вимикаємо "висіння"
        if (anim)
        {
            anim.SetBool("LedgeHang", false);
            anim.SetTrigger("LedgeClimb");
        }

        // У цей момент персонаж ще кінематичний,
        // позицію ми “доставимо” в кінці анімації через event.
    }

    /// <summary>
    /// Викликається Animation Event-ом в кінці кліпу підтягування.
    /// Ставить персонажа у "точку стояння" і повертає фізику.
    /// </summary>
    public void OnLedgeClimbFinished()
    {
        Debug.Log("Ledge Climb Finished");
        if (!ledge) return;

        // Переміщаємо на верх виступу (точка підготовлена LedgeGrabber-ом)
        transform.position = ledge.GetStandPosition();

        // Повертаємо фізику
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Повертаємось у Normal
        state = State.Normal;
    }

    // ---------------- ANIMATOR ----------------

    private void UpdateAnimator()
    {
        if (!anim) return;

        // Швидкість бігу (для blend tree Run/Idle)
        anim.SetFloat("Speed", Mathf.Abs(inputX));

        // Чи на землі
        anim.SetBool("Grounded", isGrounded);

        // Вертикальна швидкість (для Jump/Fall)
        float vy = rb.bodyType == RigidbodyType2D.Dynamic ? rb.velocity.y : 0f;
        anim.SetFloat("VerticalSpeed", vy);

        // Падіння (для перемикання на fall анімацію)
        anim.SetBool("Falling", !isGrounded && vy < -0.1f);
    }

    // ---------------- GIZMOS ----------------

    private void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }

    private void ApplyJumpCutIfNeeded()
    {
        // Працює тільки в нормальному стані (не на виступі)
        if (state != State.Normal) return;

        // Якщо відпустили кнопку, і ми ще летимо вгору — урізаємо швидкість
        if (jumpReleasedThisFrame && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }
    }



}
