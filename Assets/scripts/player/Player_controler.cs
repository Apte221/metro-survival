using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_controler : MonoBehaviour
{

    [Header("Health")]
    [SerializeField] private int Max_hp = 100;
    
    private Health health;

    void Awake()
    {
        health = new Health(Max_hp); 
    }

    int Get_Curent()
    {
        return health.Curent;
    }
    int Get_Max()
    {
        return health.Max;
    }

    bool Take_damage (int damage)
    {
        return health.TakeDamage(damage);

    }
    void Heal (int amount)
    {
        health.heal(amount);
    }


}
