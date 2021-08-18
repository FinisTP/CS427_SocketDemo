using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;


    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;
    private bool isConnected = false;

    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(this);
    }

    public void ConnectToServer()
    {
        InitializeClientData();

        tcp = new TCP();
        udp = new UDP();

        isConnected = true;
        tcp.Connect();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Packet receivedData;
        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            socket.EndConnect(ar);
            if (!socket.Connected)
            {
                return;
            }
            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
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
                Debug.Log($"Error sending data to server: {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int _byteLength = stream.EndRead(ar);
                if (_byteLength <= 0)
                {
                    instance.Disconnect();
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
                Disconnect();
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
                        packetHandlers[_packedId](_packet);
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

        private void Disconnect()
        {
            instance.Disconnect();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }

    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endpoint;

        public UDP()
        {
            endpoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error sending data to server via UDP: {e}");
            }
        
        }

        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);
            socket.Connect(endpoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endpoint);
                socket.BeginReceive(ReceiveCallback, null);
                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(_data);

            } catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_data))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });
            }
        }

        private void Disconnect()
        {
            instance.Disconnect();
            endpoint = null;
            socket = null;
        }
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int)ServerPackets.welcome, ClientHandle.Welcome },
            {(int)ServerPackets.udpTest, ClientHandle.UDPTest },
            {(int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            {(int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
            {(int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            {(int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnect },
            {(int)ServerPackets.playerHealth, ClientHandle.PlayerHealth },
            {(int)ServerPackets.playerRespawned, ClientHandle.PlayerRespawned },
            {(int)ServerPackets.createItemSpawner, ClientHandle.CreateItemSpawner },
            {(int)ServerPackets.itemSpawned, ClientHandle.ItemSpawned },
            {(int)ServerPackets.itemPickedUp, ClientHandle.ItemPickedUp },
            {(int)ServerPackets.spawnProjectile, ClientHandle.SpawnProjectile },
            {(int)ServerPackets.projectilePosition, ClientHandle.ProjectilePosition },
            {(int)ServerPackets.projectileExploded, ClientHandle.ProjectileExploded },
            {(int)ServerPackets.spawnEnemy, ClientHandle.SpawnEnemy },
            {(int)ServerPackets.enemyPosition, ClientHandle.EnemyPosition },
            {(int)ServerPackets.enemyHealth, ClientHandle.EnemyHealth }
        };
        Debug.Log("Initialized packets");
    }

    private void Disconnect()
    {
        if (isConnected)
        {
            Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected from server.");
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();
            
        }
    }

}
