using System.Collections.Generic;
using System.IO;

namespace TKDataPatcher
{
    public class BinListData
    {
        internal IOMemoryStream Stream { get; private set; }

        internal ItemDataHeader Header;

        public void Initialize(string filePath)
        {
            InitializeStream(filePath);
        }
        
        public void InitializeStream(string path)
        {
            Stream = new IOMemoryStream(File.OpenRead(path))
            {
                Position = 0
            }; 
            Header = new ItemDataHeader(Stream);
        }

        public virtual void ReadData() { }

        public virtual void Save(string path)
        {
        }
        
        public virtual void PatchCSVData(string path) { }

        public virtual void WriteCSVData(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}