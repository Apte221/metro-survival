using UnityEngine;
using UnityEngine.UI;

public class ItemInfoPanel : MonoBehaviour
{
    [SerializeField] private Text itemName;
    [SerializeField] private Text description;
    [SerializeField] private Image icon;



    public void Show(ItemDefinition def, int count)
    {
        if (def == null)
        {
            return;
        }



        itemName.text = def.displayName;
        description.text = def.description;
        if (icon)
            icon.sprite = def.icon;
    }
    public void Normal() { 
    itemName.text = "Назва придмета";
        description.text = "Опис придмета";


    }

   
}
