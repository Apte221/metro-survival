using UnityEngine;

public class ChestInventoryController : MonoBehaviour
{
    [SerializeField] private ItemDatabase database;
    [SerializeField] private int size = 16;

    public Inventory Inventory { get; private set; }

    private void Awake()
    {
        if (database != null) database.Build();
        Inventory = new Inventory(database, size);
    }
}
