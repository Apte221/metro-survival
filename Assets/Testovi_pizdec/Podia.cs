using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Podia : MonoBehaviour
{

    private Collider2D col1;
    private void Awake()
    {
        col1 = GetComponentInParent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {

        Sluz.Test123?.Invoke(5);    
    }



}
