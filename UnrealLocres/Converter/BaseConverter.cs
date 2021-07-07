using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnrealLocres.Converter
{
    public abstract class BaseConverter
    {
        public abstract string ExportExtension { get; }
        public abstract string ImportExtension { get; }

        public void Export(LocresFile locres, string outputPath)
        {
            var data = new List<TranslationEntry>();

            foreach (var ns in locres)
            {
                foreach (var str in ns)
                {
                    var key = ns.Name + "/" + str.Key;
                    data.Add(new TranslationEntry(key, str.Value, string.Empty));
                }
            }

            using (var file = File.Create(outputPath))
            using (var writer = new StreamWriter(file))
            {
                Write(data, writer);
            }

            Console.WriteLine($"Exported {data.Count} strings.");
        }

        protected abstract void Write(List<TranslationEntry> data, TextWriter writer);

        public void Import(LocresFile locres, string inputPath)
        {
            List<TranslationEntry> data;

            using (var file = File.OpenRead(inputPath))
            using (var reader = new StreamReader(file))
            {
                data = Read(reader);
            }

            var dict = data.ToDictionary(x => x.Key);

            foreach (var ns in locres)
            {
                foreach (var str in ns)
                {
                    var key = ns.Name + "/" + str.Key;

                    if (!dict.TryGetValue(key, out var tr))
                        continue;
                    if (string.IsNullOrEmpty(tr.Target))
                        continue;

                    str.Value = tr.Target;
                    dict.Remove(key);
                }
            }
        }

        protected abstract List<TranslationEntry> Read(TextReader stream);

        protected class TranslationEntry
        {
            public TranslationEntry(string key, string source, string target)
            {
                Key = key;
                Source = source;
                Target = target;
            }

            public string Key { get; set; }
            public string Source { get; set; }
            public string Target { get; set; }
        }
    }
}
