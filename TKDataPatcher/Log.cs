using System;

namespace TKDataPatcher
{
    public static class Log
    {
        internal static void WriteLine(string output, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(output);
        }
        
        public static void Error(string output)
        {
            WriteLine($"[ERROR] {output}", ConsoleColor.Red);
        }
        
        public static void Warning(string output)
        {
            WriteLine($"[WARNING] {output}", ConsoleColor.Yellow);
        }
        
        public static void Info(string output)
        {
            WriteLine($"[INFO] {output}", ConsoleColor.Gray);
        }
    }
}