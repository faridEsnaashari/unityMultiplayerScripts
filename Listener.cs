using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Listener : MonoBehaviour
{
    //fields and properties
    public Text socketIpUI = null;

    private TcpListener listener;

    private List<ConnectionManager> _connections = new List<ConnectionManager>();
    public List<ConnectionManager> connections
    {
        get {return _connections;}
    }

    public delegate void DataRecivedEventHandler(string data, int length, int id);
    public event DataRecivedEventHandler _dataRecived;

    private const int MAXIMUM_CONNECTION = 10;
    private int connectionsCounter = 0;
    private int connectionId = 0;

    private bool allowAcceptTcpClient = true;
    
    public int connectionsNumber
    {
        get
        {
            int counter = 0;
            for(int i = 0; i < _connections.Count; i++)
            {
                if(_connections[i].connected)
                {
                    counter++;
                }
                else
                {
                    _connections.RemoveAt(i);
                    i--;
                    connectionsCounter--;
                }
            }
            return counter;
        }
    }

    //methods
    public void stopListening()
    {
        allowAcceptTcpClient = false;
    }
    public string configListenerAndStartListening(int port)
    {
        DontDestroyOnLoad(this.gameObject);

        string socketIp = configListener(port);
        beginListening();
        return socketIp;
    }
    public void beginListening()
    {
        try
        {
            listener.BeginAcceptTcpClient(clientConnected, null);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }
    private void clientConnected(IAsyncResult ar)
    {
        if(allowAcceptTcpClient)
        {
            ConnectionManager cm = new ConnectionManager(listener.EndAcceptTcpClient(ar), connectionId);
            Debug.Log("client detected");
            cm.subscribeToDataRecivedEvent(invokeDataRecivedEvent);

            if(isTcpClientExist(cm))
            {
                replaceCurrentCmWithOldCm(cm);
            }
            else if(connectionsCounter <= MAXIMUM_CONNECTION)
            {
                _connections.Add(cm);
                connectionsCounter++;
                connectionId++;
            }
            else
            {
                throw new connectionExceptions("maximum number of connections reached!!!");
            }
            try
            {
                listener.BeginAcceptTcpClient(clientConnected, null);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
    private bool isTcpClientExist(ConnectionManager cm)
    {
        IPEndPoint receivingClientRemoteEndPoint = cm.remoteEndPoint;
        for(int i = 0; i < connections.Count; i++)
        {
            if(receivingClientRemoteEndPoint.Address.ToString() == connections[i].remoteEndPoint.Address.ToString())
            {
                return true;
            }
        }
        return false;
    }
    private void replaceCurrentCmWithOldCm(ConnectionManager currentCm)
    {
        IPEndPoint receivingClientRemoteEndPoint = currentCm.remoteEndPoint;
        for(int i = 0; i < connections.Count; i++)
        {
            if(receivingClientRemoteEndPoint.Address.ToString() == connections[i].remoteEndPoint.Address.ToString())
            {
                connections[i] = currentCm;
                break;
            }
        }
    }
    public string configListener(int port)
    {
        DontDestroyOnLoad(this.gameObject);

        IPHostEntry ihe = Dns.GetHostEntry(Dns.GetHostName());
        listener = new TcpListener(ihe.AddressList[0], port);
        listener.Start();
        
        string socketIp = ihe.AddressList[0].ToString();
        if(socketIpUI != null)
        {
            socketIpUI.text = socketIp;
        }   
        return socketIp;
    }

    public void getData(DataRecivedEventHandler dataRecived)
    {
        _dataRecived += dataRecived;
    }
    public void invokeDataRecivedEvent(string data, int length, int id)
    {
        if(_dataRecived != null)
        {
            _dataRecived(data, length, id);
        }
    }
    public void sendData(string data, int id)
    {
        _connections[id].sendDataToSocket(data);
    }
    public void sendData(string data)
    {
        for(int i = 0; i < _connections.Count; i++)
        {
            _connections[i].sendDataToSocket(data);
        }
    }
    void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
    }
    public void closeConnection()
    {
        for(int i = 0; i < _connections.Count; i++)
        {
            _connections[i].close();
        }
    }
    public void closeConnection(int id)
    {
        _connections[id].close();
    }
}
