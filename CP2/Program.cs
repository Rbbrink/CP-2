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

        p.Initialize();
    }

    public void Initialize()
    {
        string[] input = Console.ReadLine().Split(' ');
        ownNumber = int.Parse(input[0]);

        for (int i = 0; i < input.Length; i++)
        {
            //read nodenumbers
        }
    }
}

