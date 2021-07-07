using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Csv;

namespace UnrealLocres.Converter
{
    public sealed class CsvConverter : BaseConverter
    {

        private static readonly CsvOptions options = new CsvOptions() { AllowNewLineInEnclosedFieldValues = true, AllowSingleQuoteToEncloseFieldValues = true };

        public override string ExportExtension => "csv";

        public override string ImportExtension => "csv";

        protected override List<TranslationEntry> Read(TextReader reader)
        {
            var result = new List<TranslationEntry>();
            foreach (var line in CsvReader.Read(reader, options))
            {
                if (line.ColumnCount < 3)
                    continue;

                var key = line[0];
                var src = line[1];
                var dst = line[2];

                result.Add(new TranslationEntry(key, src, dst));
            }

            return result;
        }

        protected override void Write(List<TranslationEntry> data, TextWriter writer)
        {
            var rows = data.Select(x => new string[] { x.Key, x.Source, "" });

            CsvWriter.Write(writer, new string[] { "key", "source", "target" }, rows);
        }
    }
}
