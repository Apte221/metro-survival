using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class map_menu : MonoBehaviour
{
    [SerializeField] private GameObject map;
     private bool Map_menu_activated = false;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // ��� ���������� ���������
        if (Input.GetButtonDown("Map"))
        {
            Map_menu_activated = !Map_menu_activated;
            map.SetActive(Map_menu_activated);

        }
    }
}

