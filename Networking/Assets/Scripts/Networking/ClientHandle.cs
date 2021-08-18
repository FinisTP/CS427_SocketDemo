using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;

        // send packet back to server
        ClientSend.WelcomeReceived();
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        Game_Manager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void UDPTest(Packet _packet)
    {
        string _msg = _packet.ReadString();
        Debug.Log($"Received packet via UDP. Contains message: {_msg}");
        ClientSend.UDPTestReceived();
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        if (Game_Manager.players.ContainsKey(_id))
        {
            Game_Manager.players[_id].destinationPos = _position;
        }
    }

    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();
        if (Game_Manager.players.ContainsKey(_id))
            Game_Manager.players[_id].transform.rotation = _rotation;
    }

    public static void PlayerDisconnect(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Destroy(Game_Manager.players[_id].gameObject);
        Game_Manager.players.Remove(_id);
    }

    public static void PlayerHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        Game_Manager.players[_id].SetHealth(_health);
    }

    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Game_Manager.players[_id].Respawn();
    }

    public static void CreateItemSpawner(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        Vector3 _spawnPosition = _packet.ReadVector3();
        bool _hasItem = _packet.ReadBool();

        Game_Manager.instance.CreateItemSpawner(_spawnerId, _spawnPosition, _hasItem);
    }

    public static void ItemSpawned(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        Game_Manager.itemSpawners[_spawnerId].ItemSpawned();
    }

    public static void ItemPickedUp(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        int _byPlayer = _packet.ReadInt();
        Game_Manager.itemSpawners[_spawnerId].ItemPickedUp();
        Game_Manager.players[_byPlayer].itemCount++;
    }

    public static void SpawnProjectile(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        int _thrownByPlayer = _packet.ReadInt();
        Game_Manager.instance.SpawnProjectile(_projectileId, _position);
        Game_Manager.players[_thrownByPlayer].itemCount--;
    }

    public static void ProjectilePosition (Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (Game_Manager.projectiles.TryGetValue(_projectileId, out ProjectileManager _projectile))
        {
            _projectile.transform.position = _position;
        }
    }

    public static void ProjectileExploded(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        Game_Manager.projectiles[_projectileId].Explode(_position);
    }

    public static void SpawnEnemy(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        Game_Manager.instance.SpawnEnemy(_enemyId, _position);
    }

    public static void EnemyPosition(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (Game_Manager.enemies.TryGetValue(_enemyId, out EnemyManager _enemy))
        {
            _enemy.transform.position = _position;
        }
        
    }

    public static void EnemyHealth(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        Game_Manager.enemies[_enemyId].SetHealth(_health);
    }

}
