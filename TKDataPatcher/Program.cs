using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TKDataPatcher
{
    class Program
    {
        const string TerminationString = "Press any key to close this window . . .";
        private static string _basePath;
        private static string _binaryPath;
        private static string _binListPath;
        private static string _backupPath;
        private static string _modDataPath;
        private static string _tkDataBin;
        private static string _tkDataBinBackup;

        private static readonly Dictionary<string, BinListData> SupportedBinLists = new Dictionary<string, BinListData>{
            { "customize_item_data", new CustomizeItemData() },
            //{ "stage_list_console", new StageListConsole() }
        };

        static bool Initialize()
        {
            if (!VerifyGamePath())
            {
                Log.Warning(Path.Combine(_basePath, "TEKKEN 7.exe"));
                Log.Error("Please put the patcher files and the exe in the game's base directory (Where TEKKEN 7.exe is located) and restart the patcher.");
                return false;
            }
            
            if (!File.Exists(@"Resources\tkdata.bms"))
            {
                Log.Error("The tkdata script file was not found.");
                return false;
            }

            if (!File.Exists(@"Resources\quickbms.exe"))
            {
                Log.Error("QuickBMS was not found.");
                return false;
            }

            if (File.Exists(_tkDataBin)) // tkdata.bin exists, extract it
            {
                var process = new Process() 
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"Resources\quickbms.exe",
                        Arguments = $"-o \"Resources\\tkdata.bms\" \"{_tkDataBin}\" \"{_binaryPath}\""
                    }
                };
                process.Start();
                process.WaitForExit();
                
                if(!Directory.Exists("Backup")) Directory.CreateDirectory("Backup");
                
                // Move tkdata.bin to backup directory
                File.Move(_tkDataBin, _tkDataBinBackup, true);
            }
            
            // Patch
            for (int i = 0; i < SupportedBinLists.Count; i++)
            {
                BinListData data = SupportedBinLists.ElementAt(i).Value;
                string name = SupportedBinLists.ElementAt(i).Key;

                string csvDataPath = Path.Combine(_modDataPath, name);
                string dataListPath = Path.Combine(_binListPath, $"{name}.bin");
                string binListBackupFilePath = Path.Combine(_backupPath, $"{name}.bin");
                
                // Check if backup doesn't exist
                if (!File.Exists(binListBackupFilePath))
                {
                    File.Copy(dataListPath, binListBackupFilePath);
                    // Backup the file and create a CSV file for it
                    data.Initialize(binListBackupFilePath);
                    data.ReadData();
                    data.WriteCSVData(Path.Combine(csvDataPath, "TEKKEN PROJECT"));
                }

                data.PatchCSVData(csvDataPath);
                data.Save(dataListPath);
            }

            return true;
        }
        
        static int Main(string[] args)
        {
            _basePath = Directory.GetCurrentDirectory();
            _binaryPath = Path.Combine(_basePath, @"TekkenGame\Content\Binary");
            _binListPath = Path.Combine(_basePath, @"TekkenGame\Content\Binary\list");
            _modDataPath = Path.Combine(_basePath, @"TekkenGame\Content\ModData");
            _tkDataBin = Path.Combine(_binaryPath, "tkdata.bin");
            _backupPath = Path.Combine(_basePath, "Backup");
            _tkDataBinBackup = Path.Combine(_backupPath, @"tkdata.bin");

            if (!Initialize())
            {
                Console.WriteLine(TerminationString);
                Console.ReadKey();
                return 1;
            }

            return 0;
        }


        public static bool VerifyGamePath()
        {
            if (!Directory.Exists(_binaryPath)) { return false; }
            if (!File.Exists(Path.Combine(_basePath, "TEKKEN 7.exe"))) return false;

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
