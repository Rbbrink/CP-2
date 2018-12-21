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
    public int foreignport;

    //Deze thread als client
    public Connection(int port)
    {
        TcpClient client = new TcpClient("localhost", port);
        Read = new StreamReader(client.GetStream());
        Write = new StreamWriter(client.GetStream());
        Write.AutoFlush = true;
        Write.WriteLine("Port: " + Program.thisport);
        foreignport = port;

        Console.WriteLine("Connected with port " + port);
        new Thread(ReaderThread).Start();
    }

    public void SendRT()
    {
        Console.WriteLine("SendRT " + foreignport);
        //Laat server weten welke poort verbinding met hem maakt
        Write.WriteLine("RT");
        lock(Program.RoutingTable)
        {
            foreach (KeyValuePair <int, Tuple<int, int>> rtConnections in Program.RoutingTable)
            {
                Write.WriteLine(rtConnections.Key + " " + rtConnections.Value.Item1 + " " + rtConnections.Value.Item2);
            }
        }
        Write.WriteLine("END");
    }

    public void SendMessage(string[] parts)
    {
        string message = parts[0];
        for (int i = 2; i < parts.Length; i++)
            message += parts[i] + " ";
        Write.WriteLine(message);
    }

    //Deze thread als server
    public Connection(StreamReader read, StreamWriter write, int port)
    {
        foreignport = port;
        Read = read; 
        Write = write;
        Write.WriteLine("BClient connects: " + Program.thisport);
        // Start het reader-loopje
        new Thread(ReaderThread).Start();
    }

    public void ReaderThread()
    {
        try
        {
            while (true)            
            {
                string result = string.Empty;
                string input = Read.ReadLine();
                if(input.StartsWith("B"))
                {
                    Console.WriteLine(input.Remove(0, 1));
                }
                else if (input.StartsWith("D"))
                {
                    Program.RemoveConnection(foreignport);
                }
                else if(input == "RT")
                {
                    ReadRT();                    
                }   
            }
        }
        catch {Console.WriteLine("Connection with port " + foreignport + " broke unexpectedly");} // Verbinding is kennelijk verbroken
    }

    public void ReadRT()
    {
        // de server weet dat de client klaar is met zijn table doorsturen als hij END ontvangt
        bool changed = false;
        Console.WriteLine("ReadRT");
        while (true)
        {
            string input = Read.ReadLine();
            if (input == "END")
            {
                lock (Program.neighboursSEND)
                {
                    if (changed)
                    {
                        Console.WriteLine("changed");
                        foreach (KeyValuePair<int, Tuple<Connection, int, int>> rtkvp in Program.neighboursSEND)
                        {
                            Program.neighboursSEND[rtkvp.Key].Item1.SendRT();
                        }
                    }
                }
                break;
            }
            string[] parts = input.Split(' ');
            int pzero = int.Parse(parts[0]), pone = int.Parse(parts[1]);
            lock (Program.RoutingTable)
            {
                if (!Program.RoutingTable.ContainsKey(pzero))
                {
                    changed = true;
                    Program.RoutingTable.Add(pzero, Tuple.Create(pone + 1, foreignport));
                }
                else if (pone + 1 < Program.RoutingTable[pzero].Item1)
                {
                    changed = true;
                    Program.RoutingTable.Remove(pzero);
                    Program.RoutingTable.Add(pzero, Tuple.Create(pone + 1, foreignport));
                }
            }
        }
    }
}