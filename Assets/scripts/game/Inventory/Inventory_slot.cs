using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text countText;

    public void SetEmpty()
    {
        icon.enabled = false;
        countText.text = "";
    }

    public void SetItem(Sprite sprite, int count)
    {
        icon.enabled = true;
        icon.sprite = sprite;
        countText.text = count > 1 ? count.ToString() : "";
    }
}
