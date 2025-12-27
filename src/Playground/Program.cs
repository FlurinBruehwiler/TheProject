using System;
using System.Text;
using LightningDB;

class Program
{
    static void Main()
    {
        int i = 1;
        int y = 2;

        var x = () =>
        {
            Console.WriteLine(i);
        };

        i++;

        x();
    }
}