using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace TKDataPatcher
{
    enum ProgramRuntimePlatform : int
    {
        Windows,
        Linux,
        Unknown
    }

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

        private static string _quickbmsPath;

        private static ProgramRuntimePlatform _runtimePlatform;
        
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
            
            if (!File.Exists(GetPlatformFriendlyPath(@"Resources\tkdata.bms")))
            {
                Log.Error("The tkdata script file was not found.");
                return false;
            }

            if (!File.Exists(_quickbmsPath))
            {
                Log.Error($"The path for QuickBMS: \"{_quickbmsPath}\" was not found.");
                return false;
            }

            if (File.Exists(_tkDataBin)) // tkdata.bin exists, extract it
            {
                string _tkDataBms = GetPlatformFriendlyPath(@"Resources\tkdata.bms");
                var process = new Process() 
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _quickbmsPath,
                        Arguments = $"-o \"{_tkDataBms}\" \"{_tkDataBin}\" \"{_binaryPath}\""
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

        static void CheckRuntimePlatform()
        {
            _runtimePlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? ProgramRuntimePlatform.Windows
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? ProgramRuntimePlatform.Linux
                    : ProgramRuntimePlatform.Unknown;
        }

        static string GetPlatformFriendlyPath(string path) {
            return path.Replace('\\', Path.DirectorySeparatorChar);
        }
        
        static int Main(string[] args)
        {
            CheckRuntimePlatform();
            
            _basePath = Directory.GetCurrentDirectory();
            _binaryPath = Path.Combine(_basePath, GetPlatformFriendlyPath(@"TekkenGame\Content\Binary"));
            _binListPath = Path.Combine(_basePath, GetPlatformFriendlyPath(@"TekkenGame\Content\Binary\list"));
            _modDataPath = Path.Combine(_basePath, GetPlatformFriendlyPath(@"TekkenGame\Content\ModData"));
            _tkDataBin = Path.Combine(_binaryPath, "tkdata.bin");
            _backupPath = Path.Combine(_basePath, "Backup");
            _tkDataBinBackup = Path.Combine(_backupPath, @"tkdata.bin");

            _quickbmsPath = _runtimePlatform == ProgramRuntimePlatform.Windows 
                ? GetPlatformFriendlyPath(@"Resources\quickbms.exe") : GetPlatformFriendlyPath(@"Resources\quickbms");

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
            if (!Directory.Exists(_binaryPath)) { 
                Log.Error($"The directory: {_binaryPath} does not exist");
                return false; 
            }

            string tekken7ExePath = Path.Combine(_basePath, "TEKKEN 7.exe");

            if (!File.Exists(tekken7ExePath)) {
                Log.Error($"The file: {tekken7ExePath} does not exist");
                return false;
            }

            return true;
        }
    }
}
