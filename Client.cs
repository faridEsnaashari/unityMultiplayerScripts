using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    private bool _connected;
    public bool connected
    {
        get
        {
            if(_connection != null)
            {
                _connected = _connection.connected;
            }
            return _connected;
        }
    }

    private TcpClient socketConnection;

    private ConnectionManager _connection;
    public ConnectionManager connection
    {
        get
        {
            return _connection;
        }
    }

    
    public void connect(string IpAddress, int port)
    {
        DontDestroyOnLoad(this.gameObject);

        socketConnection = new TcpClient();

        IPAddress serverIp = IPAddress.Parse(IpAddress);

        socketConnection.BeginConnect(serverIp, port, connectedToTheServer, null);
    }
    private void connectedToTheServer(IAsyncResult ar)
    {
        socketConnection.EndConnect(ar);

        _connection = new ConnectionManager(socketConnection, 0);
    }
    public void closeConnection()
    {
        _connection.close();
    }
}
