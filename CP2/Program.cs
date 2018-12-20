using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static public int thisport;
    int nrconn;
    static public Dictionary<int, Tuple<Connection, int, int>> neighboursSEND = new Dictionary<int, Tuple<Connection, int, int>>();
    static public Dictionary<int, Connection> neighboursGET = new Dictionary<int, Connection>();
    static public Dictionary<int, Tuple<int, int>> RoutingTable = new Dictionary<int, Tuple<int, int>>();
    bool complete = true;
    static public Server server;

    static void Main(string[] args)
    {
        Program p = new Program();
        Console.Title = "poortnummer " + args[0];
        p.Initialize(args);
    }

    public void Initialize(string[] args)
    {
        nrconn = args.Length - 1;
        thisport = int.Parse(args[0]);
        server = new Server(thisport); 
        foreach (string s in args)
        {
            int i = int.Parse(s);
            lock(neighboursSEND)
            {
                if (s != args[0] && !neighboursSEND.ContainsKey(i))                     
                    neighboursSEND.Add(i, Tuple.Create(new Connection(i), 1, i));                      
            }
        }

        RoutingTable[thisport] = Tuple.Create(0, thisport);
        lock(neighboursSEND)
        {
            lock(RoutingTable)
            {
                foreach (KeyValuePair<int, Tuple<Connection, int, int>> directNeighbours in neighboursSEND)
                {
                    int i = directNeighbours.Key;
                    if (!RoutingTable.ContainsKey(i))
                    {
                        Console.WriteLine("p.add " + i);
                        RoutingTable.Add(i, Tuple.Create(1, directNeighbours.Key));
                    }
                    else if (RoutingTable[i].Item1 > 1)
                    {
                        Console.WriteLine("p.replace " + i);
                        RoutingTable.Remove(i);
                        RoutingTable.Add(i, Tuple.Create(1, directNeighbours.Key));
                    }   
                }
            }
        }
        lock(neighboursSEND)
        {
            foreach (KeyValuePair<int, Tuple<Connection, int, int>> rtkvp in neighboursSEND)
            {
                neighboursSEND[rtkvp.Key].Item1.SendRT();
            }
        }

        while (true)
        {
            if (neighboursGET.Count == nrconn && neighboursSEND.Count == nrconn)
            {
                if (!complete)
                {   
                    Console.WriteLine("All connections set up");
                    complete = true;
                    foreach (KeyValuePair<int, Tuple<int, int>> kvp in RoutingTable)
                    {
                        Console.WriteLine(kvp.Key + " " + kvp.Value.Item1 + " " + kvp.Value.Item2);
                    }
                }
            }
            else if (complete)
            {
                Console.WriteLine("New connections pending");
                complete = false;
            }
            checkinput();
        }
    }

    public void checkinput()
    {
        if (Console.KeyAvailable)
        {
            string input = Console.ReadLine();
            string[] parts = input.Split();
            //show routing table
            if (parts[0] == "R")
            {


            }
            else
            {
                int serverport = int.Parse(parts[1]);
                //send message
                if (parts[0] == "B")
                {
                    if (!neighboursSEND.ContainsKey(serverport))
                        Console.WriteLine("Error: unkown port number");
                    else
                        (neighboursSEND[serverport]).Item1.SendMessage(parts);
                }
                //add connection
                else if (parts[0] == "C")
                {       
                    lock(neighboursSEND)
                    {
                        if (!neighboursSEND.ContainsKey(serverport))                    
                        {
                            neighboursSEND.Add(serverport, Tuple.Create(new Connection(serverport), 1, serverport));   
                            nrconn++;
                        }
                        else 
                            Console.WriteLine("Already connected");
                    }
                }
                //break connection
                else if (parts[0] == "D")
                {
                    lock(neighboursSEND)
                    {
                        lock(neighboursGET)
                        {
                            if (neighboursSEND.ContainsKey(serverport) && neighboursGET.ContainsKey(serverport))
                            {
                                (neighboursSEND[serverport]).Item1.SendMessage(parts);
                                RemoveConnection(int.Parse(parts[1]));
                            }
                            else 
                                Console.WriteLine("Error: cannot break connection; not directly connected");
                        }
                    }
                }
            }
        }
    }

    static public void RemoveConnection (int foreignport)
    {
        Console.WriteLine("Conncetion broken with port " + foreignport);
        neighboursGET.Remove(foreignport);
        neighboursSEND.Remove(foreignport);
        nrconn--;
    }
}

