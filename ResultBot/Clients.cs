using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ResultBot
{
    class Clients
    {
        public HttpClient Client = new HttpClient();

        public Clients()
        {
            Client.BaseAddress = new Uri(@"https://localhost:5001/");

        }
    }
}

