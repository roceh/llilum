﻿using System;
using System.Collections.Generic;
using Microsoft.binutils.elflib;
using System.IO;

namespace Microsoft.binutils.readelf
{
    class Program
    {
        static void Main( string[] args )
        {
            bool fDisplayFileHeader         = false;
            bool fDisplayProgramHeaders     = false;
            bool fDisplaySectionHeaders     = false;
            bool fDisplaySymbolTable        = false;
            bool fDisplayRelocations        = false;

            var  files = new List<string>();

            // No options provided
            if(args.Length == 0 || !args[0].StartsWith("-"))
            {
                DisplayUsage();
            }

RESTART:
            if(args == null || args.Length == 0)
            {
                if( System.Diagnostics.Debugger.IsAttached )
                {
                    Console.Write( ">" );
                    args = Console.ReadLine().Split( new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries );
                }
                if( args == null || args.Length == 0 )
                {
                    return;
                }
            }


            int index = 0;
            while (args[index].StartsWith("-"))
            {
                var str = args[index];

                switch (str.Substring(1))
                {
                    case "a":
                        fDisplayFileHeader      = true;
                        fDisplayProgramHeaders  = true;
                        fDisplaySectionHeaders  = true;
                        fDisplaySymbolTable     = true;
                        fDisplayRelocations     = true;
                        break;

                    case "e":
                        fDisplayFileHeader      = true;
                        fDisplayProgramHeaders  = true;
                        fDisplaySectionHeaders  = true;
                        break;

                    case "h":
                        fDisplayFileHeader      = true;
                        break;

                    case "l":
                        fDisplayProgramHeaders  = true;
                        break;

                    case "S":
                        fDisplaySectionHeaders  = true;
                        break;

                    case "s":
                        fDisplaySymbolTable     = true;
                        break;

                    case "r":
                        fDisplayRelocations     = true;
                        break;

                    case "?":
                        DisplayUsage();
                        if( System.Diagnostics.Debugger.IsAttached )
                        {
                            args = null;
                            goto RESTART;
                        }
                        return;
                }

                index++;

                // No files provided
                if (index >= args.Length)
                {
                    DisplayUsage();
                    if( System.Diagnostics.Debugger.IsAttached )
                    {
                        args = null;
                        goto RESTART;
                    }
                    return;
                }
            }

            while (index < args.Length)
            {
                files.Add(args[index]);
                index++;
            }

            foreach (var file in files)
            {
                ElfObject[] objs;

                if( !File.Exists( file ) ) continue;

                objs = ElfObject.FileUtil.Parse(file);

                if(objs == null)
                {
                    Console.WriteLine("Could not open file {0}", file);
                    continue;
                }
                foreach( ElfObject obj in objs )
                {
                    if( fDisplayFileHeader )
                    {
                        Console.WriteLine( OutputFormatter.PrintElfHeader( obj.Header ) );
                    }

                    if( fDisplaySectionHeaders )
                    {
                        Console.WriteLine( OutputFormatter.PrintSectionHeaders( obj.Sections ) );
                    }

                    if( fDisplayProgramHeaders )
                    {
                        Console.WriteLine( OutputFormatter.PrintProgramHeaders( obj ) );
                    }

                    if( fDisplaySymbolTable )
                    {
                        Console.WriteLine( OutputFormatter.PrintSymbolTable( obj.SymbolTable ) );
                    }

                    if( fDisplayRelocations )
                    {
                        Console.WriteLine( OutputFormatter.PrintRelocationEntries( obj.RelocationSections ) );
                    }
                }
            }
            if( System.Diagnostics.Debugger.IsAttached )
            {
                args = null;
                files.Clear();
                goto RESTART;
            }
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("Usage: readelf <option(s)> elf-file(s)");
            Console.WriteLine(" Display information about the contents of ELF format files");
            Console.WriteLine(" Options are:");
            Console.WriteLine("  -a                     Equivalent to: -h -l -S -s -r");
            Console.WriteLine("  -h                     Display the ELF file header");
            Console.WriteLine("  -l                     Display the program headers");
            Console.WriteLine("  -S                     Display the section headers");
            Console.WriteLine("  -e                     Equivalent to -h -l -S");
            Console.WriteLine("  -s                     Display the symbol table");
            Console.WriteLine("  -r                     Display the relocations (if present)");
            Console.WriteLine("  -H                     Display this information");
            Console.WriteLine();
        }
    }
}
