using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
    IPointerDownHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Text countText;
    [SerializeField] private Image selectionFrame;

    public int Index { get; private set; }
    private InventoryUI ui;

    public void Init(InventoryUI ui, int index)
    {
        this.ui = ui;
        Index = index;
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

    // ===== Drag events =====

    public void OnBeginDrag(PointerEventData eventData)
    {
        ui?.BeginDrag(Index, icon.sprite, icon.enabled, countText.text);
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
        ui?.DropOn(Index);
        ui?.Select(Index); // виділення по дропу
    }
    public void SetSelected(bool selected)
    {
        if (selectionFrame != null)
            selectionFrame.enabled = selected;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        ui?.Select(Index); // виділення одразу по натисканню
    }




}
