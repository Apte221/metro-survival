using UnityEngine;

public class FollowPlayerEdgeOffset2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                  // гравець
    public Vector3 followOffset = new Vector3(0, 0, -10); // офсет камери в≥д гравц€

    [Header("Edge zones")]
    public float edgeSizePx = 30f;

    [Header("Offset limits (world units)")]
    public float maxOffsetX = 1.5f;
    public float maxOffsetY = 1.0f;

    [Header("Smoothing")]
    public float followSpeed = 10f;   // швидк≥сть сл≥дуванн€
    public float edgeSpeed = 6f;      // швидк≥сть Ут€гнетьс€Ф до краю
    public float returnSpeed = 10f;   // швидк≥сть поверненн€ в центр

    void LateUpdate()
    {
        if (!target) return;

        Vector3 mouse = Input.mousePosition;

        float x = 0f;
        float y = 0f;

        if (mouse.x < edgeSizePx)
            x = -Mathf.InverseLerp(edgeSizePx, 0f, mouse.x);
        else if (mouse.x > Screen.width - edgeSizePx)
            x = Mathf.InverseLerp(Screen.width - edgeSizePx, Screen.width, mouse.x);

        if (mouse.y < edgeSizePx)
            y = -Mathf.InverseLerp(edgeSizePx, 0f, mouse.y);
        else if (mouse.y > Screen.height - edgeSizePx)
            y = Mathf.InverseLerp(Screen.height - edgeSizePx, Screen.height, mouse.y);

        // база = гравець + офсет
        Vector3 basePos = target.position + followOffset;

        // ц≥льовий зсув (не б≥льше н≥ж на X/Y)
        Vector3 targetEdgeOffset = new Vector3(x * maxOffsetX, y * maxOffsetY, 0f);
        Vector3 desiredPos = basePos + targetEdgeOffset;

        // €кщо мишка в центр≥ Ч швидше УпружинитьФ назад
        float s = (x == 0f && y == 0f) ? returnSpeed : edgeSpeed;

        // згладжуванн€ (можна одним lerp)
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * s);
    }
}
