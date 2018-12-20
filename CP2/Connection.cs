﻿using System;
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
    int foreignport;

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
        string message = "B";
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
            {
                string result = string.Empty;
                string input = Read.ReadLine();
                if(input.StartsWith("B"))
                {
                    result = input.Remove(0, 1);
                }
                else if(input == "RT")
                {
                    //Program.server.ReadRT(foreignport);
                }
                if (result.Length > 0)
                    Console.WriteLine(result);    
            }
        }
        catch {Console.WriteLine("Connection with port " + foreignport + " broke unexpectedly");} // Verbinding is kennelijk verbroken
    }
}