using UnityEngine;
using UnityEngine;



public class DualInventoryUI : MonoBehaviour
{
    public static DualInventoryUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject windowRoot;

    [Header("Slot prefab + parents")]
    [SerializeField] private DualSlotView slotPrefab;
    [SerializeField] private Transform playerGridParent;
    [SerializeField] private Transform chestGridParent;
    public bool IsOpen => windowRoot != null && windowRoot.activeSelf;
    public Inventory CurrentChest => chestInv; // щоб знати, яка скриня відкрита


    [Header("Optional")]
    [SerializeField] private ItemInfoPanel infoPanel;

    [Header("Drag Visual")]
    [SerializeField] private InventoryDragVisual dragVisual;


    private Inventory playerInv;
    private Inventory chestInv;

    private DualSlotView[] playerViews;
    private DualSlotView[] chestViews;

    private (Inventory inv, int index) selected = (null, -1);

    private (Inventory inv, int index) draggingFrom = (null, -1);
    private bool draggingValid;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (windowRoot != null) windowRoot.SetActive(false);
        if (dragVisual != null) dragVisual.Hide();
    }


    public void Open(Inventory chestInventory)
    {
        Debug.Log("DualInventoryUI.Open called");
        Debug.Log($"windowRoot={(windowRoot ? windowRoot.name : "NULL")} activeBefore={(windowRoot ? windowRoot.activeSelf : false)}");

        var controller = PlayerInventoryController.Instance;
        if (controller == null) return;

        playerInv = controller.Inventory;
        chestInv = chestInventory;

        BuildIfNeeded();

        // підпишемось на оновлення
        playerInv.OnChanged += RefreshAll;
        chestInv.OnChanged += RefreshAll;

        RefreshAll();

        if (windowRoot != null) windowRoot.SetActive(true);
    }

    public void Close()
    {
        if (playerInv != null) playerInv.OnChanged -= RefreshAll;
        if (chestInv != null) chestInv.OnChanged -= RefreshAll;

        draggingFrom = (null, -1);
        draggingValid = false;
        selected = (null, -1);

        if (infoPanel != null) infoPanel.Normal();
        if (dragVisual != null) dragVisual.Hide();
        if (windowRoot != null) windowRoot.SetActive(false);
    }


    private void Update()
    {
        if (windowRoot != null && windowRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    private void BuildIfNeeded()
    {
        if (playerViews == null || playerViews.Length != playerInv.Slots.Length)
            playerViews = BuildGrid(playerGridParent, playerInv);

        if (chestViews == null || chestViews.Length != chestInv.Slots.Length)
            chestViews = BuildGrid(chestGridParent, chestInv);
    }

    private DualSlotView[] BuildGrid(Transform parent, Inventory inv)
    {
         foreach (Transform c in parent) Destroy(c.gameObject);

        var arr = new DualSlotView[inv.Slots.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            var v = Instantiate(slotPrefab, parent);
            v.Init(this, inv, i);
            arr[i] = v;
        }
        return arr;
    }

    private void RefreshAll()
    {
        RefreshGrid(playerInv, playerViews);
        RefreshGrid(chestInv, chestViews);

        // освіжити інфо по виділеному
        if (selected.inv != null && selected.index >= 0)
            ShowInfo(selected.inv, selected.index);
    }

    private void RefreshGrid(Inventory inv, DualSlotView[] views)
    {
        var controller = PlayerInventoryController.Instance;
        if (controller == null) return;

        for (int i = 0; i < inv.Slots.Length; i++)
        {
            var s = inv.Slots[i];
            if (s.IsEmpty)
            {
                views[i].SetEmpty();
                views[i].SetSelected(selected.inv == inv && selected.index == i);
            }
            else
            {
                var def = controller.GetItemData(s.itemId);
                views[i].SetItem(def != null ? def.icon : null, s.count);
                views[i].SetSelected(selected.inv == inv && selected.index == i);
            }
        }
    }

    // ===== API для слотів =====

    public void Select(Inventory inv, int index)
    {
        if (inv == null) return;
        if (index < 0 || index >= inv.Slots.Length) return;

        // якщо клік по порожньому слоту — зняти виділення
        if (inv.Slots[index].IsEmpty)
        {
            selected = (null, -1);
            if (infoPanel != null) infoPanel.Normal();
            RefreshAll(); // щоб обводка зникла
            return;
        }

        // інакше — виділити слот
        selected = (inv, index);
        ShowInfo(inv, index);
        RefreshAll(); // щоб рамки оновились
    }


    private void ShowInfo(Inventory inv, int index)
    {
        if (infoPanel == null) return;

        var controller = PlayerInventoryController.Instance;
        if (controller == null) return;

        var s = inv.Slots[index];
        if (s.IsEmpty) { infoPanel.Normal(); return; }

        var def = controller.GetItemData(s.itemId);
        infoPanel.Show(def, s.count);
    }

    public void BeginDrag(Inventory fromInv, int fromIndex, Sprite sprite, bool hasIcon, string countStr)
    {
        if (fromInv == null) return;
        if (fromIndex < 0 || fromIndex >= fromInv.Slots.Length) return;

        if (fromInv.Slots[fromIndex].IsEmpty || !hasIcon)
        {
            draggingValid = false;
            draggingFrom = (null, -1);
            if (dragVisual != null) dragVisual.Hide();
            return;
        }

        draggingValid = true;
        draggingFrom = (fromInv, fromIndex);

        if (dragVisual != null)
            dragVisual.Show(sprite, countStr);
    }

    public void Drag(Vector2 screenPos)
    {
        if (!draggingValid) return;
        if (dragVisual != null)
            dragVisual.SetPosition(screenPos);
    }

    public void EndDrag()
    {
        draggingValid = false;
        draggingFrom = (null, -1);
        if (dragVisual != null) dragVisual.Hide();
    }

    public void DropOn(Inventory toInv, int toIndex)
    {
        if (!draggingValid) return;
        if (draggingFrom.inv == null) return;

        InventoryTransfer.SwapOrMove(draggingFrom.inv, draggingFrom.index, toInv, toIndex);

        EndDrag();
    }
}
