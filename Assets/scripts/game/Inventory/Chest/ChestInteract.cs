using UnityEngine;

public class ChestInteract : MonoBehaviour
{
    [SerializeField] private ChestInventoryController chest;
    [SerializeField] private KeyCode openKey = KeyCode.E;

    private bool playerInRange;

    private void Reset()
    {
        chest = GetComponent<ChestInventoryController>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (DualInventoryUI.Instance == null || chest == null) return;

        if (Input.GetKeyDown(openKey))
        {
            // якщо вже відкрита ця скриня — закрити
            if (DualInventoryUI.Instance.IsOpen && DualInventoryUI.Instance.CurrentChest == chest.Inventory)
            {
                DualInventoryUI.Instance.Close();
            }
            else
            {
                // якщо відкрита інша скриня — просто відкрити цю (перемкнеться)
                DualInventoryUI.Instance.Open(chest.Inventory);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        // автозакриття, якщо відкрита саме ця скриня
        if (DualInventoryUI.Instance != null &&
            DualInventoryUI.Instance.IsOpen &&
            DualInventoryUI.Instance.CurrentChest == chest.Inventory)
        {
            DualInventoryUI.Instance.Close();
        }
    }
}
