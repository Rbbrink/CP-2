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

    //Deze thread als client
    public Connection(int port)
    {
        TcpClient client = new TcpClient("localhost", port);
        Read = new StreamReader(client.GetStream());
        Write = new StreamWriter(client.GetStream());
        Write.AutoFlush = true;

        //Laat server weten welke poort verbinding met hem maakt
        Write.WriteLine("Poort: " + Program.thisport);

        Console.WriteLine("Connected with port " + port);
        new Thread(ReaderThread).Start();
    }

    public void SendMessage(string[] parts)
    {
        string message = string.Empty;
        for (int i = 2; i < parts.Length; i++)
            message += parts[i] + " ";
        Write.WriteLine(message);
    }

    //Deze thread als server
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