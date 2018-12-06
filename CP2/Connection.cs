using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

class Connection
{
    public StreamReader Read;
    public StreamWriter Write;

    //Dit is de client
    public Connection(int port)
    {
        TcpClient client = new TcpClient("localhost", port);
        Read = new StreamReader(client.GetStream());
        Write = new StreamWriter(client.GetStream());
        Write.AutoFlush = true;

        Write.WriteLine("Poort: " + Program.thisport);

        new Thread(ReaderThread).Start();
    }

    //Dit is de server
    public Connection(StreamReader read, StreamWriter write)
    {
        Read = read; Write = write;

        // Start het reader-loopje
        new Thread(ReaderThread).Start();
    }

    public void ReaderThread()
    {
        try
        {
            while (true)
                Console.WriteLine(Read.ReadLine());
        }
        catch { } // Verbinding is kennelijk verbroken
    }
}

