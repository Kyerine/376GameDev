﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bow : MonoBehaviour {
    private float weaponStr = 10f;
    private int durability;

    private void Start()
    {
        durability = Random.Range(15, 25);
        gameObject.GetComponent<PlayerController>().setDurInit(durability);
        gameObject.GetComponent<PlayerController>().setDurCur(durability);

    }

    public int weaponAttack(float attackVar, int attack)
    {
        weaponStr = Random.Range(10, 25);
        durability--;
        gameObject.GetComponent<PlayerController>().setDurCur(durability);
        if (durability == 0)
        {
            gameObject.GetComponent<PlayerController>().unequip();
        }
        return (int)Mathf.Floor((attack + weaponStr) * (1 + attackVar));


    }

    public int getDurability()
    {
        return durability;
    }
}
