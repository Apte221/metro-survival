public static class InventoryTransfer
{
    public static void SwapOrMove(Inventory fromInv, int fromIndex, Inventory toInv, int toIndex)
    {
        if (fromInv == null || toInv == null) return;

        if (fromInv == toInv)
        {
            fromInv.Swap(fromIndex, toIndex);
            return;
        }

        var a = fromInv.Slots[fromIndex];
        var b = toInv.Slots[toIndex];

        if (a.IsEmpty) return;

        // перенос/свап
        toInv.Slots[toIndex] = a;
        fromInv.Slots[fromIndex] = b;

        fromInv.NotifyChanged();
        toInv.NotifyChanged();
    }
}
