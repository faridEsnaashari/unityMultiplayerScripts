using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ConnectionManager
{
    //fields and properties
    private TcpClient _socketConnection;
    private NetworkStream _socketStream;

    private byte[] dataToSend = new byte[1024];
    private byte[] recivedData = new byte[1024];

    public delegate void DataRecivedEventHandler(string data, int length, int id);
    public event DataRecivedEventHandler _dataRecived;

    private int _id;
    public int id
    {
        get {return _id;}
    }

    public IPEndPoint remoteEndPoint
    {
        get {return ((IPEndPoint)_socketConnection.Client.RemoteEndPoint);}
    }

    public bool connected
    {
        get {return _socketConnection.Connected;}
    }

    //cunstructurs
    public ConnectionManager(TcpClient socketConnection, int id)
    {
        _socketConnection = socketConnection;
        _socketStream = _socketConnection.GetStream();

        _id = id;
    }

    //methods
    public void subscribeToDataRecivedEvent(DataRecivedEventHandler dataRecived)
    {
        _dataRecived += dataRecived;
        getData();
    }
    public void close()
    {
        _socketStream.Close();
        _socketConnection.Close();
    }
    private void getData()
    {
        try
        {
            _socketStream.BeginRead(recivedData, 0, recivedData.Length, dataReceivedFromSocket, null);
        }
        catch(Exception e)
        {
            Debug.LogError("some error occure in reading data from socket. the error is : " + e);
        }
    }
    public void sendDataToSocket(string data)
    {
        try
        {
            dataToSend = Encoding.ASCII.GetBytes(data);
            _socketStream.BeginWrite (dataToSend, 0, dataToSend.Length, dataSent, null);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }
    private void dataSent(IAsyncResult ar)
    {
        _socketStream.EndWrite(ar);
    }
    private void dataReceivedFromSocket(IAsyncResult ar)
    {
        if(!isConnected(_socketConnection.Client))
        {
            Debug.Log("connectionClosed");
            close();
            return;
        }
        int dataRecivedLength = _socketStream.EndRead(ar);
        string data = Encoding.ASCII.GetString(recivedData, 0, dataRecivedLength);
        Debug.Log("data recived : " + data);
        _dataRecived(data, dataRecivedLength, _id);
        try
        {
            _socketStream.BeginRead(recivedData, 0, recivedData.Length, dataReceivedFromSocket, null);
        }
        catch(Exception e)
        {
            Debug.LogError("some error occure in reading data from socket. the error is : " + e);
        }
    }
    private bool isConnected(Socket connectionSocket)
    {
        try
        {
            return !(connectionSocket.Poll(1, SelectMode.SelectRead) && connectionSocket.Available == 0);
        }
        catch (Exception e) 
        {
            Debug.Log(e);
            return false;
        }
    }
}
