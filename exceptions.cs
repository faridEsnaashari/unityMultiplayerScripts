using System;

public class connectionExceptions : Exception
{
    private string _msg;
    public connectionExceptions(string msg)
    {
        _msg = msg;
    }
}
