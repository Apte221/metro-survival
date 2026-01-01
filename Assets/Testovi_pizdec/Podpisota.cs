using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Podpisota : MonoBehaviour
{


    private SpriteRenderer rend;
    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
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
        float v = a1 / 255f;
        rend.color = new Color(v, v, v);
    }



}
