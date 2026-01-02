using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventorymenager : MonoBehaviour
{
    public GameObject inventory_menu;
    private bool Inventory_activated = false;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // код відкривання Інвентарю
        if (Input.GetButtonDown("Inventory"))
        {
            Inventory_activated = !Inventory_activated;
            inventory_menu.SetActive(Inventory_activated);

        }
    }
}
