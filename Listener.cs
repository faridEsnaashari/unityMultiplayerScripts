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

    private const int MAXIMUM_CONNECTION = 10;
    private int connectionsCounter = 0;
    private int connectionId = 0;

    private bool allowAcceptTcpClient = true;
    
    public int connectionsNumber
    {
        get
        {
            int counter = 0;
            for(int i = 0; i < connections.Count; i++)
            {
                if(connections[i].connected)
                {
                    counter++;
                }
                else
                {
                    connections.RemoveAt(i);
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
            Debug.Log(e);
        }
    }
    private void clientConnected(IAsyncResult ar)
    {
        if(allowAcceptTcpClient)
        {
            ConnectionManager cm = new ConnectionManager(listener.EndAcceptTcpClient(ar), connectionId);
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
                Debug.Log(e);
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
}
