using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Generic;

class Server
{
    StreamReader clientIn;
    StreamWriter clientOut;

    public Server(int port)
    {
        // Luister op de opgegeven poort naar verbindingen
        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();

        // Start een aparte thread op die verbindingen aanneemt
        new Thread(() => AcceptLoop(server)).Start();
    }

    private void AcceptLoop(TcpListener handle)
    {
        while (true)
        {            
            TcpClient client = handle.AcceptTcpClient();
            clientIn = new StreamReader(client.GetStream());
            clientOut = new StreamWriter(client.GetStream());
            clientOut.AutoFlush = true;

            // De server weet niet wat de poort is van de client die verbinding maakt, de client geeft dus als onderdeel van het protocol als eerst een bericht met zijn poort
            int foreignport = int.Parse(clientIn.ReadLine().Split()[1]);
            // De client stuurt zijn eigen routing table door, en de server update zijn eigen routingtable als hij een betere connectie langs ziet komen
            if (clientIn.ReadLine() == "RT")
                ReadRT(foreignport);

            lock(Program.neighboursGET)
            {
                if (!Program.neighboursGET.ContainsKey(foreignport))
                {
                    Program.neighboursGET.Add(foreignport, new Connection(clientIn, clientOut));
                    Program.neighboursGET[foreignport].foreignport = foreignport;
                    Console.WriteLine("Client connects: " + foreignport);

                    // Zet de nieuwe verbinding in de verbindingslijst                   
                }
            }
            lock(Program.neighboursSEND)
            {
                if (!Program.neighboursSEND.ContainsKey(foreignport))                
                    Program.neighboursSEND.Add(foreignport, Tuple.Create(new Connection(foreignport), 1, foreignport));                
            }
        }
    }

    public void ReadRT(int foreignport)
    {
        // de server weet dat de client klaar is met zijn table doorsturen als hij END ontvangt
        bool changed = false;
        while (true)
        {
            string input = clientIn.ReadLine();
            if (input == "END")
            {
                lock(Program.neighboursSEND)
                {
                    if (changed)
                    {                    
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
            lock(Program.RoutingTable)
            {
                if (!Program.RoutingTable.ContainsKey(pzero))
                {
                    changed = true;
                    Program.RoutingTable.Add(pzero, Tuple.Create(pone + 1, foreignport));
                }
                else if (pone < Program.RoutingTable[pzero].Item1)
                {
                    changed = true;
                    Program.RoutingTable.Remove(pzero);
                    Program.RoutingTable.Add(pzero, Tuple.Create(pone + 1, foreignport));
                }
            }
        }            
    }
}