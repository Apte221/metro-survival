using System;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] private InventorySlotView slotPrefab;
    [SerializeField] private Transform gridParent;


    private PlayerInventoryController controller;
    private InventorySlotView[] views;
    private bool built;

    private void OnEnable()
    {
        // якщо UI з'явився пізніше — підв'яжемося
        TryBind();
        if (controller != null)
            controller.Inventory.OnChanged += Refresh;

        // на випадок якщо вже є дані
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
            views[i] = Instantiate(slotPrefab, gridParent);

        built = true;
    }

    private void Refresh()
    {
        // якщо контролер ще не готовий (наприклад UI завантажився першим) — спробуємо ще раз
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
}
