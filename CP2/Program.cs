using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static public int thisport;
    int nrconn;
    static public Dictionary<int, Connection> neighboursSEND = new Dictionary<int, Connection>();
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
            if (s == args[0])
                continue;
            int i = int.Parse(s);
            lock(neighboursSEND)
            {
                if (!neighboursSEND.ContainsKey(i))                     
                    neighboursSEND.Add(i, new Connection(i));                      
            }
        }

        AddNeighboursToRT();

        while (true)
        {
            if (neighboursGET.Count == nrconn && neighboursSEND.Count == nrconn)
            {
                if (!complete)
                {   
                    //if connections are made to all neighbours, share your routingtable with them
                    complete = true;
                    Console.WriteLine("All connections set up");
                    lock (neighboursSEND)
                    {
                        foreach (KeyValuePair<int, Connection> rtkvp in neighboursSEND)
                        {
                            neighboursSEND[rtkvp.Key].SendRT();
                        }
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
                lock(RoutingTable)
                {
                    foreach (KeyValuePair<int, Tuple<int, int>> rtkvp in RoutingTable)
                    {
                        if (rtkvp.Key == thisport)
                            Console.WriteLine(rtkvp.Key + " " + rtkvp.Value.Item1 + " local");
                        else
                            Console.WriteLine(rtkvp.Key + " " + rtkvp.Value.Item1 + " " + rtkvp.Value.Item2);
                    }
                }

            }
            else if (parts[0] == "B")
            {
                int serverport = int.Parse(parts[1]);
                //send message

                if (!RoutingTable.ContainsKey(serverport))
                    Console.WriteLine("Error: unknown port number");
                else
                {
                    if (!neighboursSEND.ContainsKey(serverport))
                        Console.WriteLine("Error: unkown port number");
                    else
                        (neighboursSEND[serverport]).SendMessage(parts);
                }
            }
            //add connection
            else if (parts[0] == "C")
            {
                int serverport = int.Parse(parts[1]);
                lock (neighboursSEND)
                {
                    if (!neighboursSEND.ContainsKey(serverport))
                    {
                        neighboursSEND.Add(serverport, new Connection(serverport));
                        nrconn++;
                    }
                    else
                        Console.WriteLine("Already connected");
                }
            }
            //break connection
            else if (parts[0] == "D")
            {
                int serverport = int.Parse(parts[1]);
                lock (neighboursSEND)
                {
                    lock (neighboursGET)
                    {
                        if (neighboursSEND.ContainsKey(serverport) && neighboursGET.ContainsKey(serverport))
                        {
                            neighboursSEND[serverport].SendMessage(parts);
                            RemoveConnection(int.Parse(parts[1]));
                        }
                        else
                            Console.WriteLine("Error: cannot break connection; not directly connected");
                    }
                }
            }            
        }
    }

    static public void RemoveConnection (int foreignport)
    {
        neighboursGET.Remove(foreignport);
        neighboursSEND.Remove(foreignport);
        Console.WriteLine("Conncetion broken with port " + foreignport);
        //nrconn--;
    }

    public void AddNeighboursToRT()
    {
        lock(neighboursSEND)
        {
            lock(RoutingTable)
            {
                RoutingTable[thisport] = Tuple.Create(0, thisport);
                foreach (KeyValuePair<int, Connection> directNeighbours in neighboursSEND)
                {
                    int i = directNeighbours.Key;
                    if (!RoutingTable.ContainsKey(i))
                    {
                        Console.WriteLine("p.add " + i);
                        RoutingTable.Add(i, Tuple.Create(1, i));
                    }
                    else if (RoutingTable[i].Item1 > 1)
                    {
                        Console.WriteLine("p.replace " + i);
                        RoutingTable[i] = Tuple.Create(1, i);
                    }   
                }
            }
        }
    }
}

