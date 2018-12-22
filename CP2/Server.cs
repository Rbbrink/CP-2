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

            //The server doens't know what the port is that makes connection, the client gives as a part of the protocol first a message with his portnumber
            int foreignport = int.Parse(clientIn.ReadLine().Split()[1]);

            //Add incoming client to the list of neighbours with the connection to send input to you
            lock(Program.neighboursGET)
            {
                if (!Program.neighboursGET.ContainsKey(foreignport))
                {
                    // Zet de nieuwe verbinding in de verbindingslijst   
                    Program.neighboursGET.Add(foreignport, new Connection(clientIn, clientOut, foreignport));
                    Console.WriteLine("Client connects: " + foreignport);                                    
                }
            }
            //Add the incoming client to the list of neighbours with the connection that is able to send input to them 
            lock(Program.neighboursSEND)
            {
                if (!Program.neighboursSEND.ContainsKey(foreignport))                
                    Program.neighboursSEND.Add(foreignport, new Connection(foreignport));                
            }
        }
    }
}