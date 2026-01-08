using UnityEngine;
using System;

public class PlayerInventoryController : MonoBehaviour
{
    public static PlayerInventoryController Instance { get; private set; }

    [Header("Inventory setup")]
    [SerializeField] private ItemDatabase database;
    [SerializeField] private int inventorySize = 20;

    public Inventory Inventory { get; private set; }

    public event Action<PlayerInventoryController> OnReady; // якщо комусь треба знати коли готовий

    private void Awake()
    {
        // Singleton + не знищувати між сценами
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ініціалізація
        database.Build();
        Inventory = new Inventory(database, inventorySize);
        Inventory.OnChanged += OnInventoryChanged;

        OnReady?.Invoke(this);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (Inventory != null)
            Inventory.OnChanged -= OnInventoryChanged;
    }

    private void Update()
    {
        // тимчасовий код для тестування
        if (Input.GetKeyDown(KeyCode.I))
        {
            AddItem("1", 1);
        }


    }

    // ===== ПУБЛІЧНИЙ API =====
    public bool AddItem(string itemId, int amount) => Inventory.Add(itemId, amount);
    public bool RemoveItem(string itemId, int amount) => Inventory.Remove(itemId, amount);
    public int CountOf(string itemId) => Inventory.CountOf(itemId);
    public ItemDefinition GetItemData(string itemId) => database.Get(itemId);

    private void OnInventoryChanged()
    {
        Debug.Log("Inventory changed");
    }
}
