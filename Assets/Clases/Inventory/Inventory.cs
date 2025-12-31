using System;
using UnityEngine;

public class Inventory
{
    public event Action OnChanged;

    public InventorySlot[] Slots { get; private set; }
    private readonly ItemDatabase _db;

    public Inventory(ItemDatabase db, int size)
    {
        _db = db;
        Slots = new InventorySlot[Mathf.Max(1, size)];
    }

    public bool Add(string itemId, int amount)
    {
        if (amount <= 0) return false;

        var def = _db.Get(itemId);
        if (def == null) return false;

        int left = amount;

        // 1) Дозаповнюємо існуючі стаки
        for (int i = 0; i < Slots.Length && left > 0; i++)
        {
            if (Slots[i].IsEmpty) continue;
            if (Slots[i].itemId != itemId) continue;

            int space = def.maxStack - Slots[i].count;
            if (space <= 0) continue;

            int add = Mathf.Min(space, left);
            Slots[i].count += add;
            left -= add;
        }

        // 2) Кладемо у пусті слоти
        for (int i = 0; i < Slots.Length && left > 0; i++)
        {
            if (!Slots[i].IsEmpty) continue;

            int add = Mathf.Min(def.maxStack, left);
            Slots[i].itemId = itemId;
            Slots[i].count = add;
            left -= add;
        }

        bool success = left == 0;
        OnChanged?.Invoke();
        return success;
    }

    public bool Remove(string itemId, int amount)
    {
        if (amount <= 0) return false;

        int left = amount;

        for (int i = 0; i < Slots.Length && left > 0; i++)
        {
            if (Slots[i].IsEmpty) continue;
            if (Slots[i].itemId != itemId) continue;

            int take = Mathf.Min(Slots[i].count, left);
            Slots[i].count -= take;
            left -= take;

            if (Slots[i].count <= 0)
                Slots[i].Clear();
        }

        bool success = left == 0;
        OnChanged?.Invoke();
        return success;
    }

    public int CountOf(string itemId)
    {
        int total = 0;
        for (int i = 0; i < Slots.Length; i++)
        {
            if (!Slots[i].IsEmpty && Slots[i].itemId == itemId)
                total += Slots[i].count;
        }
        return total;
    }
}
