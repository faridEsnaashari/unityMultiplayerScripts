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


    public delegate void DataRecivedEventHandler(string data, int length);
    public event DataRecivedEventHandler _dataRecived;
    
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
        _connection.subscribeToDataRecivedEvent(invokeDataRecivedEvent);
    }

    public void getData(DataRecivedEventHandler dataRecived)
    {
        _dataRecived += dataRecived;
    }
    public void invokeDataRecivedEvent(string data, int length, int id)
    {
        if(_dataRecived != null)
        {
            _dataRecived(data, length);
        }
    }
    public void sendData(string data)
    {
        _connection.sendDataToSocket(data);
    }
    void OnApplicationQuit()
    {
        Debug.Log("quit");
        Debug.Log("Application ending after " + Time.time + " seconds");
    }
    public void closeConnection()
    {
        _connection.close();
    }
}
