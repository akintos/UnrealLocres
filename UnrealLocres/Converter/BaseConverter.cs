using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

using LocresLib;

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
                    if (string.IsNullOrEmpty(str.Value))
                        continue;

                    var key = ns.Name + "/" + str.Key;
                    data.Add(new TranslationEntry(key, str.Value, string.Empty));
                }
            }

            using (var file = File.Create(outputPath))
            using (var writer = new StreamWriter(file))
            {
                Write(data, writer);
            }

            Console.WriteLine($"Exported {data.Count} strings to {outputPath}");
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

            var translatedList = data.Where(x => !string.IsNullOrEmpty(x.Target)).ToList();

            int total = data.Count;
            int translated = translatedList.Count;

            Console.WriteLine($"Loaded {inputPath}");
            Console.WriteLine($"Translated {translated} / {total} ({translated/total:P})");

            var dict = translatedList.ToDictionary(x => x.Key);

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

            if (dict.Count > 0)
            {
                Console.WriteLine($"\nWARNING: {dict.Count} translations are not used. Please check translation key.");
                foreach (var kvpair in dict)
                {
                    var source = kvpair.Value.Source;
                    if (source.Length > 40)
                        source = source.Substring(0, 40) + "...";
                    source = source.Replace("\r", "\\r").Replace("\n", "\\n");
                    Console.WriteLine($"\t[{kvpair.Key}] \"{source}\"");
                }
            }

            Console.WriteLine($"\nImported {translated - dict.Count} translations.");
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
