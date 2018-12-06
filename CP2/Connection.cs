using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

class Connection
{
    public StreamReader Read;
    public StreamWriter Write;

    public Connection(int port)
    {
        TcpClient client = new TcpClient("localhost", port);
        Read = new StreamReader(client.GetStream());
        Write = new StreamWriter(client.GetStream());
        Write.AutoFlush = true;
        //;asfsajfs
    }
}

