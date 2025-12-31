using System;
using UnityEngine;

[Serializable]
public struct InventorySlot
{
    public string itemId;
    public int count;

    public bool IsEmpty => string.IsNullOrEmpty(itemId) || count <= 0;

    public void Clear()
    {
        itemId = "";
        count = 0;
    }
}
