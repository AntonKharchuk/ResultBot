
using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using ResultBot.Models;

namespace ResultBot
{
    class UserWatching
    {

        public static async Task AddTextAsync(string text)
        {
            await File.AppendAllTextAsync(@"D:\Code\C#\ynik\ResultBot\ResultBot\BotFiles\BotHistory.txt", text + "\n");
        }
        public static async Task AddRecuestAsync( string CurrentId, string UserIdt, string UserNamet, string Requestt, string Timet)
        { 
            UserRequest request = new UserRequest
            {
                Id = CurrentId.ToString(),
                UserId = UserIdt,
                UserName = UserNamet,
                Request = Requestt,
                Time = Timet
            };

            var json = JsonConvert.SerializeObject(request);

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            Clients clients = new Clients();

            var post = await clients.Client.PostAsync("/Values/add", data);

            post.EnsureSuccessStatusCode();

            var postcontent = post.Content.ReadAsStringAsync().Result;
            Console.WriteLine(postcontent);

        }

    }
}
