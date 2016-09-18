using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter2
{
    public class Parser
    {
        public static Int64[] parseLM(string path, byte signal)
        {
            byte[] buf = File.ReadAllBytes(path);

            int time_code = 0;
            List<Int64> events = new List<long>();

            for (int i = 0; i < buf.Length; i += 4)
            {
                switch (buf[i + 3])
                {
                    case 0xf4: time_code++; break;
                    case 0xf2:
                        events.Add(Parser.bytesToLongTime(buf[i], buf[i + 1], buf[i + 2], time_code));
                        break;
                }
            }
            return events.ToArray();
        }

        public static long bytesToLongTime(long b0, long b1, long b2, long tc)
        {
            long t = (b0) | (b1 << 8) | (b2 << 16) | (tc << 24);
            return t;
        }
    }


}
