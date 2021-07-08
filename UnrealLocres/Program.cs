using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using LocresLib;
using UnrealLocres.Converter;

namespace UnrealLocres
{
    class Program
    {
        private readonly static Dictionary<string, BaseConverter> ExporterDict = new Dictionary<string, BaseConverter>();
        private readonly static Dictionary<string, BaseConverter> ImporterDict = new Dictionary<string, BaseConverter>();

        public static void RegisterConverter(BaseConverter converter)
        {
            ExporterDict[converter.ExportExtension] = converter;
            ImporterDict[converter.ImportExtension] = converter;
        }

        public static List<string> GetExporterExtensions()
        {
            return ExporterDict.Keys.ToList();
        }

        public static List<string> GetImporterExtensions()
        {
            return ImporterDict.Keys.ToList();
        }

        public static bool GetExporter(string extension, out BaseConverter exporter)
        {
            return ExporterDict.TryGetValue(extension, out exporter);
        }

        public static bool GetImporter(string extension, out BaseConverter importer)
        {
            return ImporterDict.TryGetValue(extension, out importer);
        }

        [Verb("export", HelpText = "Export po file into other format.")]
        class ExportOptions
        {
            [Value(0, HelpText = "Input locres file path", MetaName = "InputPath", Required = true)]
            public string InputPath { get; set; }

            [Option('f', "format", Default = "csv", HelpText = "Output format [pot, csv]")]
            public string OutputFormat { get; set; }

            [Option('o', HelpText = "Output path including extension")]
            public string OutputPath { get; set; }
        }

        [Verb("import", HelpText = "Import translation data to locres file.")]
        class ImportOptions
        {
            [Value(0, HelpText = "Input locres file path", MetaName = "LocresInputPath", Required = true)]
            public string LocresInputPath { get; set; }

            [Value(1, HelpText = "Input translation file path", MetaName = "TranslationInputPath", Required = true)]
            public string TranslationInputPath { get; set; }

            [Option('f', "format", Default = "csv", HelpText = "Translation file input format [pot, csv]")]
            public string TranslationInputFormat { get; set; }

            [Option('o', HelpText = "Translated locres file output path, default is [INPUT PATH].new")]
            public string LocresOutputPath { get; set; }
        }

        private static int ExportAndExit(ExportOptions opt)
        {
            var ext = opt.OutputFormat.ToLower();

            if (!GetExporter(ext, out var exporter))
            {
                Console.Error.WriteLine($"Invalid output format {ext}");
                return -1;
            }

            string outputPath = opt.OutputPath;

            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = Path.GetFileNameWithoutExtension(opt.InputPath) + "." + ext;
            }

            var locres = new LocresFile();

            using (var file = File.OpenRead(opt.InputPath))
            {
                locres.Load(file);
            }

            exporter.Export(locres, outputPath);

            return 0;
        }

        private static int ImportAndExit(ImportOptions opt)
        {
            var ext = opt.TranslationInputFormat.ToLower();

            if (!GetImporter(ext, out var importer))
            {
                Console.Error.WriteLine($"Invalid input format {ext}");
                return -1;
            }

            string translationPath = opt.TranslationInputPath;

            if (!File.Exists(translationPath))
            {
                Console.Error.WriteLine($"Failed to find translation input file {translationPath}");
                return 2;
            }

            string outputPath = opt.LocresOutputPath;

            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = opt.LocresInputPath + ".new";
            }

            var locres = new LocresFile();

            using (var file = File.OpenRead(opt.LocresInputPath))
            {
                locres.Load(file);
            }

            importer.Import(locres, opt.TranslationInputPath);

            using (var file = File.Create(outputPath))
            {
                locres.Save(file, locres.Version);
            }

            Console.WriteLine($"Saved to {outputPath}");

            return 0;
        }

        static int Main(string[] args)
        {
            RegisterConverter(new PoConverter());
            RegisterConverter(new CsvConverter());

            var result = CommandLine.Parser.Default.ParseArguments<ExportOptions, ImportOptions>(args)
                .MapResult(
                (ExportOptions opt) => ExportAndExit(opt),
                (ImportOptions opt) => ImportAndExit(opt),
                errs => 1);

            return result;
        }
    }
}
