
using System;
using ResultBot;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Extensions.Polling;
using System.Threading;

namespace ResultBot
{
    class Program
    {
        static void Main(string[] args)
        {

            Bot tGBot = new Bot();
            tGBot.Start();
            Console.ReadKey();


        }

    }
}
