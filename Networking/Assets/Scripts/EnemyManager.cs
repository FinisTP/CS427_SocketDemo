using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    public int id;
    public float health;
    public float maxHealth = 100f;
    public Image hpFill;

    public void Initialize(int _id)
    {
        id = _id;
        health = maxHealth;
    }

    public void SetHealth(float _health)
    {
        health = _health;
        if (health <= 0)
        {
            health = 0f;
            Game_Manager.enemies.Remove(id);
            Destroy(gameObject);
        }
        hpFill.transform.localScale = new Vector3(health / maxHealth, hpFill.transform.localScale.y, hpFill.transform.localScale.z);
    }

}
