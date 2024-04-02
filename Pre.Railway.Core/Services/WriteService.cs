using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Services
{
    public static class WriteService
    {
        public static void WriteToFile(string path, List<string> content)
        {
            using (StreamWriter writer  = new StreamWriter(path, append:false))
            {
                foreach (string line in content)
                {
                    writer.WriteLine(line);
                }
            }
        }
    }
}
