using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CoorHistoryBot
{
    class Program
    {
        private static Bbot _bot;
        //private String _token = "651848966:AAGGhzrElnpvYnZ1gNfb_78T93xXDaplf5Q";

        static void Main(string[] args)
        {
            _bot = new Bbot();
            _bot.Start(ConfigurationManager.AppSettings["BotToken"]);
            while (true)
            {
                Console.ReadKey();
                //Console.WriteLine("KLok");
            }
        }        
    }
}
