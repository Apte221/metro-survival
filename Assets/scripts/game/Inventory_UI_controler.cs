using System;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] private InventorySlotView slotPrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private ItemInfoPanel infoPanel;

    private int selectedIndex = -1;



    [Header("Drag Visual")]
    [SerializeField] private InventoryDragVisual dragVisual;

    private PlayerInventoryController controller;
    private InventorySlotView[] views;
    private bool built;

    private int draggingFrom = -1;
    private bool draggingValid;

    private void OnEnable()
    {
        TryBind();
        if (controller != null)
            controller.Inventory.OnChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (controller != null && controller.Inventory != null)
            controller.Inventory.OnChanged -= Refresh;
    }

    private void TryBind()
    {
        if (controller != null) return;

        controller = PlayerInventoryController.Instance;
        if (controller == null) return;

        if (!built)
            BuildSlots();
    }

    private void BuildSlots()
    {
        var inv = controller.Inventory;

        views = new InventorySlotView[inv.Slots.Length];
        for (int i = 0; i < views.Length; i++)
        {
            views[i] = Instantiate(slotPrefab, gridParent);
            views[i].Init(this, i); // <=== важливо 
        }

        built = true;

        if (dragVisual != null)
            dragVisual.Hide();
    }

    private void Refresh()
    {
        if (controller == null)
        {
            TryBind();
            if (controller == null) return;
        }

        var inv = controller.Inventory;
        if (!built) BuildSlots();

        for (int i = 0; i < inv.Slots.Length; i++)
        {
            var slot = inv.Slots[i];

            if (slot.IsEmpty)
            {
                views[i].SetEmpty();
            }
            else
            {
                var def = controller.GetItemData(slot.itemId);
                views[i].SetItem(def != null ? def.icon : null, slot.count);
            }
        }
    }

    // ===== DRAG API (викликаЇтьс€ з≥ слот≥в) =====

    public void BeginDrag(int fromIndex, Sprite sprite, bool hasIcon, string countStr)
    {
        if (controller == null) return;

        var inv = controller.Inventory;
        if (fromIndex < 0 || fromIndex >= inv.Slots.Length) return;

        // €кщо слот порожн≥й Ч не починаЇмо drag
        if (inv.Slots[fromIndex].IsEmpty || !hasIcon)
        {
            draggingFrom = -1;
            draggingValid = false;
            if (dragVisual) dragVisual.Hide();
            return;
        }

        draggingFrom = fromIndex;
        draggingValid = true;

        if (dragVisual)
            dragVisual.Show(sprite, countStr);
    }

    public void Drag(Vector2 screenPos)
    {
        if (!draggingValid) return;
        if (dragVisual) dragVisual.SetPosition(screenPos);
    }

    public void EndDrag()
    {
        draggingFrom = -1;
        draggingValid = false;
        if (dragVisual) dragVisual.Hide();
    }

    public void DropOn(int toIndex)
    {
        if (!draggingValid) return;
        if (controller == null) return;

        var inv = controller.Inventory;

        if (draggingFrom < 0 || draggingFrom >= inv.Slots.Length) return;
        if (toIndex < 0 || toIndex >= inv.Slots.Length) return;

        if (toIndex == draggingFrom)
        {
            EndDrag();
            return;
        }

        // 1) м≥н≥мум: Swap
        inv.Swap(draggingFrom, toIndex); // <-- треба додати метод у Inventory
        // Refresh() викличетьс€ через OnChanged

        EndDrag();
    }

    public void Select(int index)
    {
        if (controller == null) return;

        var inv = controller.Inventory;

        // зн€ти попередн≥й
        if (selectedIndex >= 0 && selectedIndex < views.Length)
            views[selectedIndex].SetSelected(false);

        // €кщо кл≥к по порожньому Ч сховати опис
        if (inv.Slots[index].IsEmpty)
        {
            selectedIndex = -1;
            infoPanel.Normal();

            return;
        }

        selectedIndex = index;
        views[index].SetSelected(true);

        var slot = inv.Slots[index];
        var def = controller.GetItemData(slot.itemId);

        infoPanel.Show(def, slot.count);
    }

}
