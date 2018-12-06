using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Program
{
    static public int thisport;
    static public Dictionary<int, Connection> neighbours = new Dictionary<int, Connection>();

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
                    if (!neighbours.ContainsKey(serverport))
                        Console.WriteLine("Error: unkown port number");
                    else
                    {
                        string message = "";
                        for (int i = 2; i < parts.Length; i++)
                            message += parts[i] + " ";
                        neighbours[serverport].SendMessage(message);
                    }
                }
                else if (parts[0] == "C")
                {
                    if (!neighbours.ContainsKey(serverport))                    
                        neighbours.Add(serverport, new Connection(serverport));                    
                    else
                        Console.WriteLine("Already connected");
                }
                else if (parts[0] == "D")
                {
                    if (neighbours.ContainsKey(serverport))
                        neighbours.Remove(serverport);
                    else
                        Console.WriteLine("Error: cannot break connection; not directly connected");
                }
            }
        }
    }
}

