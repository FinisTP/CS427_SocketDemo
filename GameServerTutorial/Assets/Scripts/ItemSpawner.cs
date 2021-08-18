using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static Dictionary<int, ItemSpawner> spawners = new Dictionary<int, ItemSpawner>();
    public static int nextId = 1;

    public int spawnerId;
    public bool hasItem = false;

    private void Start()
    {
        hasItem = false;
        spawnerId = nextId;
        nextId++;
        spawners.Add(spawnerId, this);

        StartCoroutine(SpawnItem());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasItem && other.CompareTag("Player"))
        {
            Player _player = other.GetComponent<Player>();
            if (_player.AttemptPickupItem())
            {
                PickupItem(_player.id);
            }
        }
    }

    private IEnumerator SpawnItem()
    {
        yield return new WaitForSeconds(5f);

        hasItem = true;
        ServerSend.ItemSpawned(spawnerId);

    }

    private void PickupItem(int _playerId)
    { 
        hasItem = false;
        ServerSend.ItemPickedUp(spawnerId, _playerId);
        StartCoroutine(SpawnItem());

    }

}
