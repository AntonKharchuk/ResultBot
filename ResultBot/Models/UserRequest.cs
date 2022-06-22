using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultBot.Models
{
    class UserRequest
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Request { get; set; }
        public string Time { get; set; }
        public string UserName { get; set; }
    }
}
