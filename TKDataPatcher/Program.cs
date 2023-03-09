using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace TKDataPatcher
{
    class Program
    {
        const string terminationString = "Press any key to close this window . . .";
        static string BasePath;
        static string BinaryPath;
        static string TKDataBin;
        static string TKDataBinBackup;
        static string CustomizeItemDataBackupPath;
        static string CustomizeItemDataPath;
        static string CustomItemDataPath;

        static void Main(string[] args)
        {
            BasePath = Directory.GetCurrentDirectory();
            BinaryPath = Path.Combine(BasePath, @"TekkenGame\Content\Binary");
            TKDataBin = Path.Combine(BinaryPath, "tkdata.bin");
            TKDataBinBackup = Path.Combine(BasePath, @"Backup\tkdata.bin");
            CustomizeItemDataBackupPath = Path.Combine(BasePath, @"Backup\customize_item_data.bin");
            CustomizeItemDataPath = Path.Combine(BinaryPath, @"list\customize_item_data.bin");
            CustomItemDataPath = Path.Combine(BasePath, @"TekkenGame\Content\ModData\customize_item_data");

            if (!VerifyGamePath())
            {
                Console.WriteLine(Path.Combine(BasePath, "TEKKEN 7.exe"));
                Console.WriteLine("Please put the patcher files and the exe in the game's base directory (Where TEKKEN 7.exe is located) and restart the patcher.");
                Console.WriteLine(terminationString);
                Console.ReadKey();
                return;
            }

            // Create the ModData directory if it doesn't exist
            if (!Directory.Exists(CustomItemDataPath)) Directory.CreateDirectory(CustomItemDataPath);

            if (!File.Exists(@"Resources\tkdata.bms"))
            {
                Console.WriteLine("The tkdata script file was not found.");
                goto termination; // yes i am using goto, sue me fuckers
            }

            if (!File.Exists(@"Resources\quickbms.exe"))
            {
                Console.WriteLine("QuickBMS was not found.");
                goto termination;
            }

            if (File.Exists(TKDataBin)) // tkdata.bin exists, extract it
            {
                var process = new Process() 
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"Resources\quickbms.exe",
                        Arguments = $"-o \"Resources\\tkdata.bms\" \"{TKDataBin}\" \"{BinaryPath}\""
                    }
                };
                process.Start();
                process.WaitForExit();
                if(!Directory.Exists("Backup")) Directory.CreateDirectory("Backup");
                File.Move(TKDataBin, TKDataBinBackup, true); // move tkdata.bin
                File.Copy(CustomizeItemDataPath, CustomizeItemDataBackupPath, true);
            }
            
            if(!File.Exists(CustomizeItemDataPath))
            {
                Console.WriteLine("Extraction failed.");
                goto termination;
            }
            else
            {
                CustomizeItemData data = new CustomizeItemData(CustomizeItemDataBackupPath);
                // Extract original entries as csv if they doesn't exist
                string namcoCustomizeItemData = Path.Combine(CustomItemDataPath, "TEKKEN PROJECT");

                if (!Directory.Exists(namcoCustomizeItemData))
                {
                    Directory.CreateDirectory(namcoCustomizeItemData);
                    string fileName = Path.Combine(namcoCustomizeItemData, $"items.csv");
                    StringBuilder sb = new StringBuilder();

                    foreach (var entry in data.Entries)
                    {
                        string fileNameOffset = entry.fileNameOffset == "\x00" ? "\\x00" : entry.fileNameOffset;
                        string keyOffset = entry.nameByString == "\x00" ? "\\x00" : entry.nameByString;
                        string nullOffset = entry.nullOffset == "\x00" ? "\\x00" : entry.nullOffset;
                        sb.AppendLine(
                            $"{entry.itemId},{entry.packageIndex},{fileNameOffset},{entry.unk},{entry.charId},{entry.slotId},{entry.veryUnk}," +
                            $"{entry.nameById},{keyOffset},{entry.unk2},{entry.packageIndex2},{entry.unk3},{entry.playerCus},{entry.isColorable}," +
                            $"{entry.unk4},{entry.rarity},{entry.itemFlagPackageIndex},{entry.itemCost},{entry.packageIndex3},{nullOffset},{entry.unk7}");
                    }

                    File.WriteAllText(fileName, sb.ToString());
                }


                Console.WriteLine("Patching files...");
                // Patch items
                var files = Directory.EnumerateFiles(CustomItemDataPath, "*.csv", SearchOption.AllDirectories);
                var enumerable = files as string[] ?? files.ToArray();
                if (!enumerable.Any())
                {
                    Console.WriteLine("No files to patch.");
                    //goto termination;
                }

                //data = new CustomizeItemData(CustomizeItemDataPath);
                List<ItemEntry> entriesToAdd = new List<ItemEntry>();

                foreach (var file in enumerable)
                {
                    string[] fileLines = File.ReadAllLines(file);
                    for (int i = 0; i < fileLines.Length; i++)
                    {
                        ItemEntry entry;
                        try
                        {
                            entry = new ItemEntry(fileLines[i]);
                        }
                        catch
                        {
                            Console.WriteLine($"[ERROR]: Could not parse line {i} in: {file}. Skipping the file.");
                            continue;
                        }

                        entriesToAdd.Add(entry);
                        Console.WriteLine($"[INFO]: Adding file entry: {file} with the ID {entry.itemId}");
                    }

                }

                // TODO: oh no
                //var list = data.Entries.ToList();
                //list.AddRange(entriesToAdd);
                data.Entries = entriesToAdd.ToArray();
                data.SaveAndDispose(CustomizeItemDataPath);
            }

            termination: // ahahahahahahahahaha
                Console.WriteLine(terminationString);
                Console.ReadKey();
                return;
        }


        public static bool VerifyGamePath()
        {
            if (!Directory.Exists(BinaryPath)) { return false; }
            if (!File.Exists(Path.Combine(BasePath, "TEKKEN 7.exe"))) return false;

            return true;
        }

        public static IEnumerable<int> PatternAt(byte[] source, byte[] pattern, int offset)
        {
            for (int i = offset; i < source.Length; i++)
            {
                if (source.Skip((int)i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    yield return i;
                }
            }

            yield return 0;
        }

        public static unsafe int IndexOfPattern(byte[] src, byte[] pattern)
        {
            fixed (byte* srcPtr = &src[0])
            fixed (byte* patternPtr = &pattern[0])
            {
                for (int x = 0; x < src.Length; x++)
                {
                    byte currentValue = *(srcPtr + x);

                    if (currentValue != *patternPtr) continue;

                    bool match = false;

                    for (int y = 0; y < pattern.Length; y++)
                    {
                        byte tempValue = *(srcPtr + x + y);
                        if (tempValue != *(patternPtr + y))
                        {
                            match = false;
                            break;
                        }

                        match = true;
                    }

                    if (match)
                    {
                        return x;
                    }

                }
            }

            return -1;
        }
    }
}
