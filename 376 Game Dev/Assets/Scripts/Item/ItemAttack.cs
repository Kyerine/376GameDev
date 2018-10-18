﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAttack : MonoBehaviour {

    public int attackDamage;
    public int level = 0;
    public GameObject nextItem;
    private int maxLevel = 5;

    public void Start()
    {
        StartCoroutine(delete());
    }

    IEnumerator delete()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }

    public int damage()
    {
        return attackDamage * level;
    }

    public GameObject addLevel()
    {
        if (level < maxLevel)
        {
            return nextItem;
        }
        else
        {
            return gameObject;
        }
    }

    private void destroyItem()
    {
        Destroy(gameObject);
    }
}
