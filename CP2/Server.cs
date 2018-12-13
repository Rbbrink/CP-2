using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

class Server
{
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
            StreamReader clientIn = new StreamReader(client.GetStream());
            StreamWriter clientOut = new StreamWriter(client.GetStream());
            clientOut.AutoFlush = true;

            // De server weet niet wat de poort is van de client die verbinding maakt, de client geeft dus als onderdeel van het protocol als eerst een bericht met zijn poort
            int foreignport = int.Parse(clientIn.ReadLine().Split()[1]);
            while (true)
            {
                string input = clientIn.ReadLine();
                if (input == "END")
                    break;
                string[] parts = input.Split(' ');
                int pzero = int.Parse(parts[0]), pone = int.Parse(parts[1]);

                if (!Program.RoutingTable.ContainsKey(pzero))
                {
                    Console.WriteLine("add " + pzero);
                    Program.RoutingTable.Add(pzero, Tuple.Create(pone, foreignport));
                }
                else if (pone < Program.RoutingTable[pzero].Item1)
                {
                    Console.WriteLine("replace: " + pzero);
                    Program.RoutingTable.Remove(pzero);
                    Program.RoutingTable.Add(pzero, Tuple.Create(pone, foreignport));
                }
            }            

            if (!Program.neighboursGET.ContainsKey(foreignport))
            {
                Console.WriteLine("Client connects: " + foreignport);

                // Zet de nieuwe verbinding in de verbindingslijst   
                
                Program.neighboursGET.Add(foreignport, new Connection(clientIn, clientOut));
            }
            lock(Program.neighboursSEND)
            {
                if (!Program.neighboursSEND.ContainsKey(foreignport))                
                    Program.neighboursSEND.Add(foreignport, Tuple.Create(new Connection(foreignport), foreignport, 2));                
            }
        }
    }
}