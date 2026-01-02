using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Podpisota : MonoBehaviour
{


    private PlayerInventoryController rend;
    private void Awake()
    {
        rend = GetComponent<PlayerInventoryController>();
    }
    private void OnEnable()
    {
        Sluz.Test123 += color_change;
    }
    private void OnDisable()
    {
        Sluz.Test123 -= color_change;
    }

    void color_change(int a1)
    {
        rend.AddItem("1", a1);
    }



}
