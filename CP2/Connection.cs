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
        //Laat server weten welke poort verbinding met hem maakt
        Write.AutoFlush = true;
        Write.WriteLine("Port: " + Program.thisport);
        foreignport = port;

        Console.WriteLine("Verbonden: " + port);
        //new Thread(ReaderThread).Start();
    }

    public void SendRT()
    {
        Console.WriteLine("//SendRT " + foreignport);
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
        for (int i = 1; i < parts.Length; i++)
            message += " " + parts[i];
        Write.WriteLine(message);
    }

    public void Disconnect()
    {
        string[] parts = new string[]{"D", foreignport.ToString()};
        SendMessage(parts);
        Program.RemoveConnection(int.Parse(parts[1]));
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
        bool broken = false;
        while(true)
        {
            try
            {
                if (broken)
                    Console.WriteLine("//Connection with port " + foreignport + " regained");
                broken = false;
                while (true)            
                {
                    string result = string.Empty;
                    string[] input = Read.ReadLine().Split(' ');
                    if(input[0] == "B")
                    {
                        int sendToPort = int.Parse(input[1]);
                        if (sendToPort != Program.thisport)
                        {                        
                            int Key = Program.RoutingTable[sendToPort].Item2;
                            Program.neighboursSEND[Key].SendMessage(input);
                            Console.WriteLine("Bericht voor " + input[1] + " doorgestuurd naar " + Key);
                        }
                        else
                        {
                            string message = string.Empty;
                            for (int i = 2; i < input.Length; i++)
                            {
                                message += input[i] + " ";
                            }
                            Console.WriteLine(message);
                        }
                    }
                    else if (input[0] == "D")                    
                        Program.RemoveConnection(foreignport);                    
                    else if (input[0] == "RT")                    
                        ReadRT();     
                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    else if (false)//input[0] == "Del")
                    {
                        List<int> deletekeys = new List<int>();
                        string[] parts = new string[]{"Del", input[1]};
                        lock (Program.RoutingTable)
                        {
                            foreach (KeyValuePair<int, Tuple<int, int>> rtkvp in Program.RoutingTable)
                            {
                                if (rtkvp.Key == int.Parse(input[1]) && rtkvp.Value.Item2 == foreignport)                                
                                    deletekeys.Add(rtkvp.Key);                                
                            }
                            foreach (int key in deletekeys)
                            {
                                Program.RoutingTable.Remove(key);
                            }
                        }
                        if (deletekeys.Count > 0)
                        {
                            Program.SendUpdatedRT();
                            lock (Program.neighboursSEND)
                            {
                                foreach (KeyValuePair<int, Connection> kvp in Program.neighboursSEND)
                                {
                                    Program.neighboursSEND[kvp.Key].SendMessage(parts);
                                }
                            }
                        }
                    }
                }
            }        
            catch 
            {
                if (!broken)
                    Console.WriteLine("//Connection with port " + foreignport + " broke unexpectedly"); // Verbinding is kennelijk verbroken
                else
                    Console.WriteLine("//Retrying connection with porth " + foreignport);
                broken = true;
                Thread.Sleep(10);
            }
        }
    }

    public void ReadRT()
    {
        // de server weet dat de client klaar is met zijn table doorsturen als hij END ontvangt
        bool changed = false;
        while (true)
        {
            string input = Read.ReadLine();
            if (input == "END")
            {
                if (changed)                
                    Program.SendUpdatedRT();                              
                break;
            }
            string[] parts = input.Split(' ');
            int pzero = int.Parse(parts[0]), pone = int.Parse(parts[1]);
            lock (Program.RoutingTable)
            {
                if (!Program.RoutingTable.ContainsKey(pzero))
                {
                    Console.WriteLine("//New: " + pzero);
                    changed = true;
                    Program.RoutingTable.Add(pzero, Tuple.Create(pone + 1, foreignport));
                }
                else if (pone + 1 < Program.RoutingTable[pzero].Item1)
                {
                    changed = true;
                    Console.WriteLine("Afstand naar " + pzero + " is nu " + (pone + 1) + " via " + foreignport);
                    Program.RoutingTable.Remove(pzero);
                    Program.RoutingTable.Add(pzero, Tuple.Create(pone + 1, foreignport));
                }
                else
                {
                   
                }
            }
        }
        Console.WriteLine("//ReadRT");
    }
}