using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{


 public static MapManager Instance;

    
    [SerializeField] private GameObject _largeMap;

    public bool IsLargeMapOpen { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        CloseLagreMap();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if(!IsLargeMapOpen)
            {
                OpenLagreMap(); 
            }
            else
            {
                CloseLagreMap();            }
        }
    }
    private void OpenLagreMap()
    {
        _largeMap.SetActive(true);
        IsLargeMapOpen = true;
    }
    private void CloseLagreMap()
    {
        _largeMap.SetActive(false);
        IsLargeMapOpen = false;
    }
}
