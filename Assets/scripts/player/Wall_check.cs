using UnityEngine;

/// <summary>
/// Відповідає за детекцію виступу:
/// - нижній промінь (low) має впертися в стіну
/// - верхній промінь (up) НЕ має впертися (тобто над стіною є "повітря")
/// Тоді вважаємо, що є край виступу і можна хапатися.
/// Також готує дві точки:
/// - hangPosition: де персонаж має висіти
/// - standPosition: де персонаж має опинитися після підтягування
/// </summary>
public class LedgeGrabber : MonoBehaviour
{
    [Header("Raycasts")]
    [SerializeField] private Transform rayOrigin;       // звідки пускаємо промені (зазвичай з torso)
    [SerializeField] private float rayDistance = 0.45f; // довжина променів вперед
    [SerializeField] private float upperRayHeight = 0.65f; // висота верхнього променя
    [SerializeField] private LayerMask groundMask;      // що є стіною/землею

    [Header("Snap Offsets (tune)")]
    [SerializeField] private Vector2 hangOffset = new(0.22f, 0.20f);  // як “приклеїти” руки до краю
    [SerializeField] private Vector2 standOffset = new(0.35f, 1.00f); // де стояти після підтягування

    // Збережена точка, куди ставимо персонажа після climb
    private Vector2 cachedStandPos;

    private void Reset()
    {
        rayOrigin = transform;
    }

    /// <summary>
    /// Повертає true, якщо знайдено виступ.
    /// hangPosition — точка, куди треба перемістити персонажа в режим LedgeHang.
    /// </summary>
    public bool TryGetLedge(out Vector2 hangPosition)
    {
        // Визначаємо напрям по scale.x:
        // якщо scale.x >= 0 — дивимось вправо, інакше вліво
        int facing = transform.localScale.x >= 0 ? 1 : -1;

        // Нижня точка старту променя
        Vector2 originLow = rayOrigin.position;

        // Верхня точка старту променя (вище на upperRayHeight)
        Vector2 originUp = (Vector2)rayOrigin.position + Vector2.up * upperRayHeight;

        // Напрям променя — вперед
        Vector2 dir = Vector2.right * facing;

        // Промінь 1: чи є стіна попереду на рівні "низ"
        RaycastHit2D hitLow = Physics2D.Raycast(originLow, dir, rayDistance, groundMask);

        // Промінь 2: чи є стіна попереду на рівні "верх"
        RaycastHit2D hitUp = Physics2D.Raycast(originUp, dir, rayDistance, groundMask);

        bool wallAtLow = hitLow.collider != null; // низ вдарився в стіну
        bool freeAtUp = hitUp.collider == null;   // верх не вдарився => зверху порожньо

        if (wallAtLow && freeAtUp)
        {
            Debug.Log("Ledge found!");
            // Точка дотику нижнього променя — приблизна "грань" стіни
            Vector2 corner = hitLow.point;

            // Позиція висіння:
            // - по X відступаємо назад від стіни (щоб не влізти в колайдер)
            // - по Y піднімаємо на висоту рук
            hangPosition = new Vector2(
                corner.x - hangOffset.x * facing,
                corner.y + hangOffset.y
            );

            // Позиція стояння після підтягування:
            // - по X трохи вперед на платформу
            // - по Y вище, щоб поставити корпус на верх
            cachedStandPos = new Vector2(
                corner.x + standOffset.x * facing,
                corner.y + standOffset.y
            );

            return true;
        }

        hangPosition = default;
        return false;
    }

    /// <summary>
    /// Повертає позицію, куди треба поставити персонажа після підтягування.
    /// </summary>
    public Vector2 GetStandPosition() => cachedStandPos;

    private void OnDrawGizmosSelected()
    {
        if (!rayOrigin) return;

        int facing = transform.localScale.x >= 0 ? 1 : -1;
        Vector3 dir = Vector3.right * facing;

        // Нижній промінь
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + dir * rayDistance);

        // Верхній промінь
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(rayOrigin.position + Vector3.up * upperRayHeight,
                        rayOrigin.position + Vector3.up * upperRayHeight + dir * rayDistance);
    }
}
