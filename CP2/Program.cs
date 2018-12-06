using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Program
{
    static public int thisport;

    static void Main(string[] args)
    {
        Program p = new Program();
        Console.Title = "poortnummer" + args[0];
        p.Initialize(args);
    }

    public void Initialize(String[] args)
    {
        thisport = int.Parse(args[0]);
        Console.ReadLine();
    }
}

