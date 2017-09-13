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
            int status = MFRC522.MI_ERR;
            do
            {
                (status,_) = reader.Request(MFRC522.PICC_REQIDL);
                Console.WriteLine($"{status}");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }while(status != MFRC522.MI_OK);
            
            List<byte> data; 
            (status, data) = reader.Anticoll();

            Console.WriteLine($"{status} - {BitConverter.ToString(data.ToArray())}");
        }
    }
}
