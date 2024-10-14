using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share
{
    public class GifData
    {
        public string url;
        public int[] delays;

        public GifData(string url, int[] delays)
        {
            this.url = url;
            this.delays = delays;
        }
    }
}
