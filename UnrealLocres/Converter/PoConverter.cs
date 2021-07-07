using Karambolo.PO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnrealLocres.Converter
{
    public sealed class PoConverter : BaseConverter
    {
        public override string ExportExtension => "pot";

        public override string ImportExtension => "po";

        protected override List<TranslationEntry> Read(TextReader reader)
        {
            var list = new List<TranslationEntry>();

            var parser = new POParser();
            POParseResult result = parser.Parse(reader);

            if (!result.Success)
                throw new IOException("Failed to parse PO file : " + result.Diagnostics.ToString());

            var catalog = result.Catalog;

            foreach (var item in catalog)
            {
                var poentry = item as POSingularEntry;
                var trentry = new TranslationEntry(poentry.Key.ContextId, poentry.Key.Id, poentry.Translation);
                list.Add(trentry);
            }

            return list;
        }

        protected override void Write(List<TranslationEntry> data, TextWriter writer)
        {
            var catalog = new POCatalog();
            catalog.Encoding = "UTF-8";

            foreach (var item in data)
            {
                var pokey = new POKey(item.Source, contextId: item.Key);
                catalog.Add(new POSingularEntry(pokey));
            }

            var gen = new POGenerator();
            gen.Generate(writer, catalog);
        }
    }
}
