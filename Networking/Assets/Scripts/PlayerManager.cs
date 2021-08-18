using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth = 100f;
    public int itemCount = 0;
    public Vector3 destinationPos;
    public MeshRenderer model;

    private void Update()
    {
        Move();
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            health = 0f;
            Die();
        }
        Vector3 scale = Game_Manager.instance.playerHP.transform.localScale;
        scale.x = health / maxHealth;
        Game_Manager.instance.playerHP.transform.localScale = scale;
    }

    public void Die()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }

    private void Move()
    {
        transform.position = Vector3.Lerp(transform.position, destinationPos, 0.1f);
    }
}