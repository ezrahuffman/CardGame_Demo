using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats
{
    public float health;
    public List<Card> cards;

    // Default Constructor
    public EnemyStats()
    {
        cards = new List<Card>();
    }
}
