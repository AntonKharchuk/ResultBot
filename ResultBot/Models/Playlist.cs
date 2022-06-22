using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultBot.Models
{
    class Playlist
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string PlaylistName { get; set; }
        public List<string> VideoIds { get; set; }
        public List<string> VideoTitles { get; set; }

    }
}
