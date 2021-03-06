using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public int id;
    public GameObject explosionPrefab;

    public void Initialize(int _id)
    {
        id = _id;
    }

    public void Explode (Vector3 _position)
    {
        transform.position = _position;
        GameObject eff = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(eff, 3f);
        Game_Manager.projectiles.Remove(id);
        Destroy(gameObject);
    }
}
