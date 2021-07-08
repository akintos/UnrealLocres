using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using LocresLib;

namespace LocresLib.Tests
{
    public static class TestHelper
    {
        public static string GetTestDataDirectory()
        {
            string startupPath = AppDomain.CurrentDomain.BaseDirectory;
            var pathItems = startupPath.Split(Path.DirectorySeparatorChar);
            var pos = pathItems.Reverse().ToList().FindIndex(x => string.Equals("bin", x));
            string projectPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathItems.Take(pathItems.Length - pos - 1));
            return Path.Combine(projectPath, "Data");
        }

        public static LocresFile LoadTestFile(string filename)
        {
            var filePath = Path.Combine(GetTestDataDirectory(), filename);
            using (var file = File.OpenRead(filePath))
            {
                var locres = new LocresFile();
                locres.Load(file);

                return locres;
            }
        }
    }
}
