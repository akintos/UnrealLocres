using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Diagnostics;
using System.Reflection;

using LocresLib;
using UnrealLocres.Converter;

using CommandLine;

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

        public static string EscapeTrimString(string value, int length)
        {
            value = value.Replace("\r", "").Replace("\n", " ");
            if (value.Length > length)
                value = value.Substring(0, length);
            return value;
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


        [Verb("merge", HelpText = "Merge two locres files into one.")]
        class MergeOptions
        {
            [Value(0, HelpText = "Merge target locres file path, the file you want to translate", MetaName = "TargetLocresPath", Required = true)]
            public string TargetLocresPath { get; set; }

            [Value(1, HelpText = "Merge source locres file path, the file that has additional lines", MetaName = "SourceLocresPath", Required = true)]
            public string SourceLocresPath { get; set; }

            [Option('o', HelpText = "Merged locres file output path, default is [TargetLocresPath].new")]
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

        private static int MergeAndExit(MergeOptions opt)
        {
            var targetLocres = new LocresFile();

            try
            {
                using (var file = File.OpenRead(opt.TargetLocresPath))
                {
                    targetLocres.Load(file);
                }
            }
            catch (IOException)
            {
                Console.Error.WriteLine($"Failed to open merge target locres file {opt.TargetLocresPath}");
                throw;
            }

            var sourceLocres = new LocresFile();

            try
            {
                using (var file = File.OpenRead(opt.SourceLocresPath))
                {
                    sourceLocres.Load(file);
                }
            }
            catch (IOException)
            {
                Console.Error.WriteLine($"Failed to open merge source locres file {opt.SourceLocresPath}");
                throw;
            }

            foreach (var targetNs in targetLocres)
            {
                var sourceNs = sourceLocres.FirstOrDefault(x => x.Name == targetNs.Name);
                if (sourceNs is null)
                    continue;

                var targetKeySet = new HashSet<string>(targetNs.Select(x => x.Key));

                foreach (var stringEntry in sourceNs)
                {
                    if (!targetKeySet.Contains(stringEntry.Key))
                    {
                        targetNs.Add(new LocresString(stringEntry.Key, stringEntry.Value, stringEntry.SourceStringHash));
                        Console.WriteLine($"Added [{targetNs.Name}/{stringEntry.Key}] {EscapeTrimString(stringEntry.Value, 40)}");
                    }
                }

                sourceLocres.Remove(sourceNs);
            }

            string outputPath = opt.LocresOutputPath;

            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = opt.TargetLocresPath + ".new";
            }
            
            using (var file = File.Create(outputPath))
            {
                targetLocres.Save(file, targetLocres.Version);
            }

            Console.WriteLine($"\nSaved to {outputPath}");

            return 0;
        }

        static int Main(string[] args)
        {
            RegisterConverter(new PoConverter());
            RegisterConverter(new CsvConverter());

            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            Console.WriteLine($"UnrealLocres v{version}");
            Console.WriteLine("https://github.com/akintos/UnrealLocres");
            Console.WriteLine("");

            var result = Parser.Default.ParseArguments<ExportOptions, ImportOptions, MergeOptions>(args)
                .MapResult(
                (ExportOptions opt) => ExportAndExit(opt),
                (ImportOptions opt) => ImportAndExit(opt),
                (MergeOptions opt) => MergeAndExit(opt),
                errs => 1);

            return result;
        }
    }
}
