using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [SerializeField] private GameObject itemsToSpawn;   
    [SerializeField] private Transform spawnPoints;     
    [SerializeField] private int spawnCount;         

    void Start()
    {
        Spawn();
    }

    void Spawn()
    {


        for (int i = 0; i < spawnCount; i++)
        {

            Instantiate(itemsToSpawn, spawnPoints.position, spawnPoints.rotation);

             
        }
    }
}
