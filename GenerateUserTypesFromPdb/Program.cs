﻿using CommandLine;
using CommandLine.Text;
using Dia2Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenerateUserTypesFromPdb
{
    class Options
    {
        [Option('p', "pdb", Required = true, HelpText = "Path to PDB which will be used to generate the code")]
        public string PdbPath { get; set; }

        [OptionList('t', "types", ',', Required = true, HelpText = "List of types to be exported", MutuallyExclusiveSet = "cmdSettings")]
        public IList<string> Types { get; set; }

        [Option("no-type-info-comment", DefaultValue = false, HelpText = "Generate filed type info comment", Required = false, MutuallyExclusiveSet = "cmdSettings")]
        public bool DontGenerateFieldTypeInfoComment { get; set; }

        [Option("multi-line-properties", DefaultValue = false, HelpText = "Generate properties as multi line", Required = false, MutuallyExclusiveSet = "cmdSettings")]
        public bool MultiLineProperties { get; set; }

        [Option("use-dia-symbol-provider", DefaultValue = false, HelpText = "Use DIA symbol provider and access fields for specific type", Required = false, MutuallyExclusiveSet = "cmdSettings")]
        public bool UseDiaSymbolProvider { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class Program
    {
        private static void OpenPdb(string path, out IDiaDataSource dia, out IDiaSession session)
        {
            dia = new DiaSource();
            dia.loadDataFromPdb(path);
            dia.openSession(out session);
        }

        private static void DumpSymbol(IDiaSymbol symbol)
        {
            Type type = typeof(IDiaSymbol);

            foreach (var property in type.GetProperties())
            {
                Console.WriteLine("{0} = {1}", property.Name, property.GetValue(symbol));
            }
        }

        static void Main(string[] args)
        {
            var error = Console.Error;
            var options = new Options();

            Parser.Default.ParseArgumentsStrict(args, options);

            string pdbPath = options.PdbPath;
            IList<string> typeNames = options.Types;
            UserTypeGenerationFlags generationOptions = UserTypeGenerationFlags.None;

            if (!options.DontGenerateFieldTypeInfoComment)
                generationOptions |= UserTypeGenerationFlags.GenerateFieldTypeInfoComment;
            if (!options.MultiLineProperties)
                generationOptions |= UserTypeGenerationFlags.SingleLineProperty;
            if (options.UseDiaSymbolProvider)
                generationOptions |= UserTypeGenerationFlags.UseClassFieldsFromDiaSymbolProvider;

            string moduleName = Path.GetFileNameWithoutExtension(pdbPath).ToLower();
            Dictionary<string, UserType> symbols = new Dictionary<string, UserType>();
            IDiaDataSource dia;
            IDiaSession session;

            OpenPdb(pdbPath, out dia, out session);
            foreach (var typeName in typeNames)
            {
                IDiaSymbol symbol = session.globalScope.GetChild(typeName, SymTagEnum.SymTagUDT);

                if (symbol == null)
                {
                    error.WriteLine("Symbol not found: {0}", typeName);
                }
                else
                {
                    symbols.Add(typeName, new UserType(symbol, moduleName));
                }
            }

            foreach (var symbolEntry in symbols)
            {
                var userType = symbolEntry.Value;

                if (userType.Symbol.name.Contains("::"))
                {
                    string[] names = userType.Symbol.name.Split(new string[] { "::" }, StringSplitOptions.None);

                    string parentTypeName = string.Join("::", names.Take(names.Length - 1));
                    if (symbols.ContainsKey(parentTypeName))
                    {
                        userType.SetDeclaredInType(symbols[parentTypeName]);
                    }
                    else
                    {
                        throw new Exception("Unsupported namespace of class " + userType.Symbol.name);
                    }
                }
            }

            string currentDirectory = Directory.GetCurrentDirectory();
            string outputDirectory = currentDirectory + "\\output\\";
            Directory.CreateDirectory(outputDirectory);

            string[] allUDTs = session.globalScope.GetChildren(SymTagEnum.SymTagUDT).Select(s => s.name).Distinct().OrderBy(s => s).ToArray();

            File.WriteAllLines(outputDirectory + "symbols.txt", allUDTs);

            foreach (var symbolEntry in symbols)
            {
                var userType = symbolEntry.Value;
                var symbol = userType.Symbol;

                Console.WriteLine(symbolEntry.Key);
                if (userType.DeclaredInType != null)
                {
                    continue;
                }

                using (TextWriter output = new StreamWriter(string.Format("{0}{1}.exported.cs", outputDirectory, symbol.name)))
                {
                    userType.WriteCode(new IndentedWriter(output), error, symbols, generationOptions);
                }
            }
        }
    }
}
