using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Program
{
    int ownNumber;

    static void Main(string[] args)
    {
        Program p = new Program();
        Console.Title = args[0];
        p.Initialize();
    }

    public void Initialize()
    {
        string[] input = Console.ReadLine().Split(' ');
        ownNumber = int.Parse(input[0]);

        for (int i = 2; i < input.Length; i++)
        {

            //read nodenumbers
        }
        Console.ReadLine();
    }
}

