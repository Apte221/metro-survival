using UnityEngine;
using UnityEngine.UI;

public class InventoryDragVisual : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text countText;

    public void Show(Sprite sprite, string countStr)
    {
        icon.enabled = sprite != null;
        icon.sprite = sprite;
        countText.text = countStr ?? "";
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetPosition(Vector2 screenPos)
    {
        // В Canvas Screen Space Overlay достатньо просто позиції
        transform.position = screenPos;
    }
}
