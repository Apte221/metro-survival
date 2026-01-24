using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DualSlotView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Text countText;
    [SerializeField] private Image selectionFrame;


    private DualInventoryUI ui;
    private Inventory inv;
    private int index;

    public void Init(DualInventoryUI ui, Inventory inv, int index)
    {
        this.ui = ui;
        this.inv = inv;
        this.index = index;
        SetSelected(false);
    }

    public void SetEmpty()
    {
        icon.enabled = false;
        icon.sprite = null;
        countText.text = "";
    }

    public void SetItem(Sprite sprite, int count)
    {
        icon.enabled = true;
        icon.sprite = sprite;
        countText.text = count > 1 ? count.ToString() : "";
    }

    public void SetSelected(bool selected)
    {
        if (selectionFrame != null)
            selectionFrame.enabled = selected;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

            ui?.Select(inv, index);

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ui?.BeginDrag(inv, index, icon.sprite, icon.enabled, countText.text);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ui?.Drag(eventData.position);
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        ui?.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        ui?.DropOn(inv, index);
        ui?.Select(inv, index);
    }
}
