using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossBuilder
{
    public class ElfReader
    {
        private readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public bool TryProcessElfFile(string filePath, out string soName, out IList<string> depends)
        {
            try
            {
                if (ELFReader.TryLoad(filePath, out var elf))
                {
                    if (elf.Class == Class.Bit32)
                    {
                        (soName, depends) = Process32BitElfFile((ELF<uint>)elf);

                        return true;
                    }
                    else if (elf.Class == Class.Bit64)
                    {
                        (soName, depends) = Process64BitElfFile((ELF<ulong>)elf);

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to process ELF file: {filePath}");
            }

            soName = null;
            depends = null;

            return false;
        }

        private static (string soName, IList<string> depends) Process32BitElfFile(ELF<uint> elf)
        {
            var dynamicSection = elf.GetSections<DynamicSection<uint>>().FirstOrDefault();
            if (dynamicSection == null)
            {
                throw new Exception($"Unable to process the ELF file '{elf}' as it does not have a Dynamic Section.");
            }

            var stringTableEntry = dynamicSection.Entries.FirstOrDefault(x => x.Tag == DynamicTag.StrTab);
            if (stringTableEntry == null)
            {
                throw new Exception($"Unable to process the ELF file '{elf}' as it does not have a Dynamic String Table.");
            }

            var dynStringTable = elf.GetSections<StringTable<uint>>().FirstOrDefault(x => x.Offset == stringTableEntry.Value);
            var soNameEntry = dynamicSection.Entries.FirstOrDefault(x => x.Tag == DynamicTag.SoName);
            var neededEntries = dynamicSection.Entries.Where(x => x.Tag == DynamicTag.Needed);

            var soName = soNameEntry != null ? dynStringTable[soNameEntry.Value] : null;
            var needed = neededEntries.Select(x => dynStringTable[x.Value]).ToList();

            return (soName, needed);
        }

        private static (string soName, IList<string> depends) Process64BitElfFile(ELF<ulong> elf)
        {
            var dynamicSection = elf.GetSections<DynamicSection<ulong>>().FirstOrDefault();
            if (dynamicSection == null)
            {
                throw new Exception($"Unable to process the ELF file '{elf}' as it does not have a Dynamic Section.");
            }

            var stringTableEntry = dynamicSection.Entries.FirstOrDefault(x => x.Tag == DynamicTag.StrTab);
            if (stringTableEntry == null)
            {
                throw new Exception($"Unable to process the ELF file '{elf}' as it does not have a Dynamic String Table.");
            }

            var dynStringTable = elf.GetSections<StringTable<ulong>>().FirstOrDefault(x => x.Offset == stringTableEntry.Value);
            var soNameEntry = dynamicSection.Entries.FirstOrDefault(x => x.Tag == DynamicTag.SoName);
            var neededEntries = dynamicSection.Entries.Where(x => x.Tag == DynamicTag.Needed);

            var soName = soNameEntry != null ? dynStringTable[(long)soNameEntry.Value] : null;
            var needed = neededEntries.Select(x => dynStringTable[(long)x.Value]).ToList();

            return (soName, needed);
        }
    }
}
