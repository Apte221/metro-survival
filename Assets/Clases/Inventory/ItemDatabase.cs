using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemDefinition> items = new();

    private Dictionary<string, ItemDefinition> _map;

    public void Build()
    {
        _map = new Dictionary<string, ItemDefinition>();
        foreach (var it in items)
        {
            if (it == null || string.IsNullOrWhiteSpace(it.id)) continue;
            _map[it.id] = it;
        }
    }

    public ItemDefinition Get(string id)
    {
        if (_map == null) Build();
        return _map.TryGetValue(id, out var it) ? it : null;
    }
}
