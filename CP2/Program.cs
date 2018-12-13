using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static public int thisport;
    static public Dictionary<int, Tuple<Connection, int, int>> neighboursSEND = new Dictionary<int, Tuple<Connection, int, int>>();
    static public Dictionary<int, Connection> neighboursGET = new Dictionary<int, Connection>();
    static public Dictionary<int, Tuple<int, int>> RoutingTable = new Dictionary<int, Tuple<int, int>>();
    bool complete = true;

    static void Main(string[] args)
    {
        Program p = new Program();
        Console.Title = "poortnummer " + args[0];
        p.Initialize(args);
    }

    public void Initialize(string[] args)
    {
        int nrconn = args.Length - 1;
        thisport = int.Parse(args[0]);
        Server server = new Server(thisport); 
        foreach (string s in args)
        {
            int i = int.Parse(s);
            lock(neighboursSEND)
            {
                if (s != args[0] && !neighboursSEND.ContainsKey(i))                     
                    neighboursSEND.Add(i, Tuple.Create(new Connection(i), 1, i));                      
            }
        }

        foreach (KeyValuePair<int, Tuple<Connection, int, int>> a in neighboursSEND)
        {
            int i = a.Key;
            if (!RoutingTable.ContainsKey(i))
            {
                Console.WriteLine("p.add " + i);
                RoutingTable.Add(i, Tuple.Create(1, thisport));
            }
            else if (RoutingTable[i].Item1 > 1)
            {
                Console.WriteLine("p.replace " + i);
                RoutingTable.Remove(i);
                RoutingTable.Add(i, Tuple.Create(1, thisport));
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
                }
            }
            else if (complete)
            {
                Console.WriteLine("New connections pending");
                complete = false;
            }
            if (Console.KeyAvailable)
            {
                string input = Console.ReadLine();
                string[] parts = input.Split();
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
                        if (!neighboursSEND.ContainsKey(serverport))                    
                            neighboursSEND.Add(serverport, Tuple.Create(new Connection(serverport), 1, serverport));                    
                        else
                            Console.WriteLine("Already connected");
                    }
                    //break connection
                    else if (parts[0] == "D")
                    {
                        if (neighboursSEND.ContainsKey(serverport) && neighboursGET.ContainsKey(serverport))
                        {
                            neighboursSEND.Remove(serverport);
                            neighboursGET.Remove(serverport);
                        }
                        else
                            Console.WriteLine("Error: cannot break connection; not directly connected");
                    }
                }
            }
        }
    }
}

