using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static public int thisport;
    static public Dictionary<int, Connection> neighboursSEND = new Dictionary<int, Connection>(), neighboursGET = new Dictionary<int, Connection>();
    static public List<int> connected = new List<int>();

    static void Main(string[] args)
    {
        Program p = new Program();
        Console.Title = "poortnummer " + args[0];
        p.Initialize(args);
    }

    public void Initialize(string[] args)
    {
        thisport = int.Parse(args[0]);
        Server server = new Server(thisport);
        foreach (string s in args)
        {
            int i = int.Parse(s);
            if (s != args[0] && !neighboursSEND.ContainsKey(i))            
                neighboursSEND.Add(i, new Connection(i));            
        }

        while (true)
        {
            string input = Console.ReadLine();
            string[] parts = input.Split();
            if (parts[0] == "R")
            {


            }
            else
            {
                int serverport = int.Parse(parts[1]);
                if (parts[0] == "B")
                {
                    if (!neighboursSEND.ContainsKey(serverport))
                        Console.WriteLine("Error: unkown port number");
                    else
                        SendMessage(parts, serverport);
                }
                else if (parts[0] == "C")
                {
                    if (!neighboursSEND.ContainsKey(serverport))                    
                        neighboursSEND.Add(serverport, new Connection(serverport));                    
                    else
                        Console.WriteLine("Already connected");
                }
                else if (parts[0] == "D")
                {
                    if (neighboursSEND.ContainsKey(serverport))
                        neighboursSEND.Remove(serverport);
                    else
                        Console.WriteLine("Error: cannot break connection; not directly connected");
                }
            }
        }
    }

    void SendMessage(string[] parts, int serverport)
    {
        string message = string.Empty;
        for (int i = 2; i < parts.Length; i++)
            message += parts[i] + " ";
        neighboursSEND[serverport].SendMessage(message);
    }
}

