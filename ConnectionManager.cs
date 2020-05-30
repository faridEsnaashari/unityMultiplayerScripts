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
    public event DataRecivedEventHandler dataRecived;

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
    //public void subscribeToDataRecivedEvent(DataRecivedEventHandler _dataRecived)
    //{
    //    dataRecived = _dataRecived;
    //    getData();
    //}
    private void getData()
    {
        try
        {
            _socketStream.BeginRead(recivedData, 0, recivedData.Length, recivingDataFromSocket, null);
        }
        catch(Exception e)
        {
            Debug.Log("***************************");
            Debug.Log("some error occure in reading data from socket. the error is : " + e);
            Debug.Log("***************************");
        }
    }
    public void sendDataToSocket(string data)
    {
        dataToSend = Encoding.ASCII.GetBytes(data);
        _socketStream.BeginWrite (dataToSend, 0, dataToSend.Length, dataSent, null);
    }
    private void dataSent(IAsyncResult ar)
    {
        _socketStream.EndWrite(ar);
    }
    private void recivingDataFromSocket(IAsyncResult ar)
    {
        try
        {
            int dataRecivedLength = _socketStream.EndRead(ar);
            string data = Encoding.ASCII.GetString(recivedData, 0, dataRecivedLength);
            Debug.Log("data recived : " + data);
            dataRecived(data, dataRecivedLength, _id);
            _socketStream.BeginRead(recivedData, 0, recivedData.Length, recivingDataFromSocket, null);
        }
        catch(Exception e)
        {
            Debug.Log("***************************");
            Debug.Log("some error occure in reading data from socket. the error is : " + e);
            Debug.Log("***************************");
            _socketStream.BeginRead(recivedData, 0, recivedData.Length, recivingDataFromSocket, null);
        }
    }
}

