using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

namespace cards_game
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waiting for a card...");
            var reader = new MFRC522();

            while (!reader.IsCardAvailable())
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            
            var id = BitConverter.ToString(reader.GetCardId());

            Console.WriteLine($"card id: {id}");
        }
    }
}
