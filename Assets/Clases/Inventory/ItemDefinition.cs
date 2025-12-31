using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string id;                 // "wood", "stone"
    public string displayName;        // "Дерево"
    public Sprite icon;
    public int maxStack = 99;
}
