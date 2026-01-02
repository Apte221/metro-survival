using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerInventoryController controller;

    [SerializeField] private InventorySlotView slotPrefab;
    [SerializeField] private Transform gridParent;

    private InventorySlotView[] views;

    void Start()
    {
        var inv = controller.Inventory;

        // 1) створюємо UI слоти під розмір інвентаря
        views = new InventorySlotView[inv.Slots.Length];
        for (int i = 0; i < views.Length; i++)
            views[i] = Instantiate(slotPrefab, gridParent);

        // 2) підписка на подію (але через інвентар усередині контролера)
        inv.OnChanged += Refresh;

        // 3) перше малювання
        Refresh();
    }

    void OnDestroy()
    {
        if (controller != null && controller.Inventory != null)
            controller.Inventory.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        var inv = controller.Inventory;

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
}
