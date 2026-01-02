using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    [Header("Inventory setup")]
    [SerializeField] private ItemDatabase database;
    [SerializeField] private int inventorySize = 20;

    public Inventory Inventory { get; private set; }

    void Awake()
    {
        // гарантуємо, що база готова
        database.Build();

        // створюємо інвентар (це чиста логіка, не MonoBehaviour)
        Inventory = new Inventory(database, inventorySize);

        // підписка на зміну інвентаря
        Inventory.OnChanged += OnInventoryChanged;
    }

    void OnDestroy()
    {
        // ОБОВ?ЯЗКОВО відписуємось
        if (Inventory != null)
            Inventory.OnChanged -= OnInventoryChanged;
    }

    // ===== ПУБЛІЧНИЙ API ДЛЯ ГРИ =====

    public bool AddItem(string itemId, int amount)
    {
        return Inventory.Add(itemId, amount);
    }

    public bool RemoveItem(string itemId, int amount)
    {
        return Inventory.Remove(itemId, amount);
    }

    public int CountOf(string itemId)
    {
        return Inventory.CountOf(itemId);
    }

    // ===== РЕАКЦІЯ НА ЗМІНИ =====

    private void OnInventoryChanged()
    {
        // тут зазвичай:
        // - оновлюється UI
        // - відправляється івент
        // - робиться autosave

        Debug.Log("Inventory changed");

        // Debug-вивід
        for (int i = 0; i < Inventory.Slots.Length; i++)
        {
            var slot = Inventory.Slots[i];

            if (slot.IsEmpty)
                Debug.Log($"[{i}] пусто");
            else
                Debug.Log($"[{i}] {slot.itemId} x{slot.count}");
        }
    }
}
