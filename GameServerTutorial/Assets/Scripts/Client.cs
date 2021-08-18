using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client
{
    public static int dataBufferSize = 4096;
    public int id;
    public TCP tcp;
    public UDP udp;
    public Player player;


    public Client(int _clientid)
    {
        id = _clientid;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;
        private byte[] receiveBuffer;
        private readonly int id;
        private Packet receivedData;

        public TCP(int _id)
        {
            id = _id;
        }

        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();
            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            // Send welcome packet
            ServerSend.Welcome(id, "Welcome to the server!");

        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error sending data to player {id} via TCP: {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int _byteLength = stream.EndRead(ar);
                if (_byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }
                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                // handle data
                receivedData.Reset(HandleData(_data));

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            }
            catch (Exception _e)
            {
                Debug.Log($"Error receiving TCP data: {_e}");
                Server.clients[id].Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;
            receivedData.SetBytes(_data);
            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packedId = _packet.ReadInt();
                        // Debug.Log(_packedId);
                        Server.packetHandlers[_packedId](id, _packet);
                    }
                });
                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1) return true;
            return false;
        }

        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint;
        private int id;
        public UDP(int _id)
        {
            id = _id;
        }
        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
            ServerSend.UDPTest(id);
        }
        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endPoint, _packet);
        }
        public void HandleData(Packet _packetData)
        {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet);

                    // if (_packetId == (int)ClientPackets.playerShoot) Debug.Log("Shot");

                }
            });
        }
        public void Disconnect()
        {
            endPoint = null;
        }
    }

    public void SendIntoGame(string _playerName)
    {
        // player = new Player(id, _playerName, new Vector3(0, 0, 0));
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initialize(id, _playerName);
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                {
                    ServerSend.SpawnPlayer(id, _client.player);
                }
            }
        }

        foreach (Client _client in Server.clients.Values)
        {
            ServerSend.SpawnPlayer(_client.id, player);
        }

        foreach(ItemSpawner _itemSpawner in ItemSpawner.spawners.Values)
        {
            ServerSend.CreateItemSpawner(id, _itemSpawner.spawnerId, _itemSpawner.transform.position, _itemSpawner.hasItem);
        }

        foreach(Enemy _enemy in Enemy.enemies.Values)
        {
            ServerSend.SpawnEnemy(id, _enemy);
        }
    }

    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

        ThreadManager.ExecuteOnMainThread(() =>
        {
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        //UnityEngine.Object.Destroy(player.gameObject);
        //player = null;
        tcp.Disconnect();
        udp.Disconnect();

        ServerSend.PlayerDisconnect(id);
    }
}
