using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static public int thisport;
    static int nrconn = 0;
    static public Dictionary<int, Connection> neighboursSEND = new Dictionary<int, Connection>();
    static public Dictionary<int, Connection> neighboursGET = new Dictionary<int, Connection>();
    static public Dictionary<int, Tuple<int, int>> RoutingTable = new Dictionary<int, Tuple<int, int>>();
    static public Dictionary<int, List<Tuple<int, int>>> backups = new Dictionary<int, List<Tuple<int, int>>>();
    //bool complete = true;
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
        SendUpdatedRT();

        while (true)
        {
            //if (neighboursGET.Count == nrconn && neighboursSEND.Count == nrconn)
            //{
            //    if (!complete)
            //    {   
            //        //if connections are made to all neighbours, share your routingtable with them
            //        complete = true;
            //        Console.WriteLine("//All connections set up");
            //        lock (neighboursSEND)
            //        {
            //            foreach (KeyValuePair<int, Connection> rtkvp in neighboursSEND)
            //            {
            //                neighboursSEND[rtkvp.Key].SendRT();
            //            }
            //        }
            //    }
            //}
            //else if (complete)
            //{
            //    Console.WriteLine("//New connections pending");
            //    complete = false;
            //}
            checkinput();
        }
    }

    public void checkinput()
    {
        string input = Console.ReadLine();
        if (input.StartsWith("R") || input.StartsWith("B ") || input.StartsWith("C ") || input.StartsWith("D ") || input.StartsWith("E"))
        {

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
            else if(parts[0] == "E")
            {
                SendUpdatedRT();
            }
            else 
            {
                int serverport = int.Parse(parts[1]);
                //send message
                if (parts[0] == "B")
                {
                    if (!RoutingTable.ContainsKey(serverport))
                        Console.WriteLine("Poort " + parts[1] + " is niet bekend");
                    else
                    {
                        int key = RoutingTable[serverport].Item2;
                        (neighboursSEND[key]).SendMessage(parts);
                    }
                }
                //add connection 
                else if (parts[0] == "C")
                {         
                    bool update = false;
                    lock (neighboursSEND)
                    {
                        if (!neighboursSEND.ContainsKey(serverport))
                        {
                            neighboursSEND.Add(serverport, new Connection(serverport));
                            nrconn++;
                            update = true;
                        }
                        else
                            Console.WriteLine("//Already connected");
                    }
                    if (update)
                        SendUpdatedRT();                         

                }
                //break connection
                else if (parts[0] == "D")
                {                    
                    lock (neighboursSEND)
                    {
                        lock (neighboursGET)
                        {
                            if (neighboursSEND.ContainsKey(serverport) && neighboursGET.ContainsKey(serverport))
                            {
                                neighboursSEND[serverport].Disconnect();          
                                Console.WriteLine("Verbroken: " + parts[1]);
                            }
                            else
                                Console.WriteLine("Poort " + parts[1] + " is niet bekend");
                        }
                    }
                }            
            }
        }
    }

    static public void RemoveConnection (int foreignport)
    {
        neighboursGET.Remove(foreignport);
        neighboursSEND.Remove(foreignport);
        Console.WriteLine("//Connection broken with port " + foreignport);
        nrconn--;
        lock (RoutingTable)
        {
            List<int> deletekeys = new List<int>();
            foreach (KeyValuePair<int, Tuple<int, int>> rtkvp in RoutingTable)
            {
                if (rtkvp.Value.Item2 == foreignport)
                    deletekeys.Add(rtkvp.Key);
            }
            foreach (int key in deletekeys)
            {
                RoutingTable.Remove(key);
            }
        }
        lock (neighboursSEND)
        {
            string[] parts = new string[]{"Delete", foreignport.ToString()};
            foreach (KeyValuePair<int, Connection> kvp in neighboursSEND)
            {
                neighboursSEND[kvp.Key].SendMessage(parts);
            }
        }
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
                        RoutingTable.Add(i, Tuple.Create(1, i));                    
                    else if (RoutingTable[i].Item1 > 1)                    
                        RoutingTable[i] = Tuple.Create(1, i);                    
                }
            }
        }
    }

    static public void SendUpdatedRT()
    {
        lock (neighboursSEND)
        {
            foreach (KeyValuePair<int, Connection> rtkvp in neighboursSEND)
            {
                neighboursSEND[rtkvp.Key].SendRT();
            }
        }
    }
}

