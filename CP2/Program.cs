using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static public int thisport;
    static public Dictionary<int, Connection> neighboursSEND = new Dictionary<int, Connection>();
    static public Dictionary<int, Connection> neighboursGET = new Dictionary<int, Connection>();
    static public Dictionary<int, Tuple<int, int>> RoutingTable = new Dictionary<int, Tuple<int, int>>();
    static public Dictionary<int, List<Tuple<int, int>>> backups = new Dictionary<int, List<Tuple<int, int>>>();
    static public Server server;

    static void Main(string[] args)
    {
        Program p = new Program();
        Console.Title = "poortnummer " + args[0];
        p.Initialize(args);
    }

    public void Initialize(string[] args)
    {
        thisport = int.Parse(args[0]);
        server = new Server(thisport);
        
        //Add all the given neighbours to your preferred neighbour list
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
            checkinput();
        }
    }

    //Check for user input and act accordingly
    public void checkinput()
    {
        string input = Console.ReadLine();
        if (input.StartsWith("R") || input.StartsWith("B ") || input.StartsWith("C ") || input.StartsWith("D ") || input.StartsWith("E"))
        {

            string[] parts = input.Split();
            //Print routing table
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
            //Send routingtable
            else if(parts[0] == "E")
            {
                SendUpdatedRT();
            }
            else 
            {
                int serverport = int.Parse(parts[1]);
                //Send message to the given port
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
                //Add connection 
                else if (parts[0] == "C")
                {         
                    bool update = false;
                    lock (neighboursSEND)
                    {
                        if (!neighboursSEND.ContainsKey(serverport))
                        {
                            neighboursSEND.Add(serverport, new Connection(serverport));
                            update = true;
                        }
                        else
                            Console.WriteLine("//Already connected");
                    }
                    if (update)
                        SendUpdatedRT();                         

                }
                //Break connection
                else if (parts[0] == "D")
                {                    
                    lock (neighboursSEND)
                    {
                        lock (neighboursGET)
                        {
                            if (neighboursSEND.ContainsKey(serverport) && neighboursGET.ContainsKey(serverport))
                            {
                                neighboursSEND[serverport].Disconnect();       
                                lock (neighboursSEND)
                                {		
                                    foreach (KeyValuePair<int, Connection> kvp in neighboursSEND)
                                    {
                                        neighboursSEND[kvp.Key].SendMessage(new string[]{"U"});
                                    }                            
                                }
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

    //Removes all connection which make use of the given port
    static public void RemoveConnection (int foreignport)
    {
        neighboursGET.Remove(foreignport);
        neighboursSEND.Remove(foreignport);
        Console.WriteLine("//Connection broken with port " + foreignport);
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
        string[] parts = new string[]{"Del", foreignport.ToString()};
        lock (neighboursSEND)
        {
            foreach (KeyValuePair<int, Connection> kvp in neighboursSEND)
            {
                neighboursSEND[kvp.Key].SendMessage(parts);
            }
        }        
    }

    //Add your neighbours to your routingtable
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

    //Send your routingtable
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

