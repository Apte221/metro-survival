using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventorymenager : MonoBehaviour
{
    public enum MenuType
    {
        None,
        Inventory,
        Map

    }

    [SerializeField] private GameObject inventory_menu;
    [SerializeField] private GameObject map_menu;
    [SerializeField] private GameObject main_menu;

    private bool Inventory_activated = false;
    private bool map_activated = false;
    

    MenuType currectMenu = MenuType.None;

    void Start()
    {

    }


    void Update()
    {
        Openmenu_button();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Closeall();

        }
    }
    public void Toggle(MenuType menu)
    {
        if (currectMenu == menu)
        {
            Closeall();
            currectMenu = MenuType.None;
            return;
        }

        Closeall();
        Activemainmenu();
        Get(menu).SetActive(true);
        currectMenu = menu;
    }
    void Closeall()
    {
        inventory_menu.SetActive(false);
        map_menu.SetActive(false);
        main_menu.SetActive(false);
    }
    GameObject Get(MenuType menu)
    {
        return menu switch
        {
            MenuType.Inventory => inventory_menu,
            MenuType.Map => map_menu,
            _ => null
        };
    }
    void Openmenu_button()
    {
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            Inventory_activated = !Inventory_activated;
            inventory_menu.SetActive(Inventory_activated);
            Toggle(MenuType.Inventory);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            map_activated =! map_activated;
            map_menu.SetActive(map_activated);
            Toggle(MenuType.Map);
        }
    }
    void Activemainmenu()
    {
        main_menu.SetActive(true);
    }
}
