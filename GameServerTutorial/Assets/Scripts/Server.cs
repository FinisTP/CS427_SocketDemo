using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    public static void Start(int _maxPlayers, int _port)
    {
        MaxPlayers = _maxPlayers;
        Port = _port;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallBack, null);


        Debug.Log($"Server started on port {Port}");

    }

    private static void TCPConnectCallback(IAsyncResult ar)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(ar);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        Debug.Log($"Incoming connection from: {_client.Client.RemoteEndPoint}");

        for (int i = 1; i <= MaxPlayers; ++i)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }

        }
        Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect to server: Server Full!");
    }

    private static void UDPReceiveCallBack(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallBack, null);

            if (_data.Length < 4)
            {
                return;
            }
            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();
                if (_clientId == 0)
                {
                    return;
                }
                if (clients[_clientId].udp.endPoint == null)
                {
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }
                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error receiving TCP data: {e}");
        }
    }

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {e}");
        }
    }

    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; ++i)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived},
            { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement},
            { (int)ClientPackets.playerShoot, ServerHandle.PlayerShoot},
            { (int)ClientPackets.playerThrowItem, ServerHandle.PlayerThrowItem},
            { (int)ClientPackets.udpTestReceive, ServerHandle.UDPTestReceived}
        };
        Debug.Log("Initialized player data");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}

