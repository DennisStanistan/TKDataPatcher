using System.IO;

namespace TKDataPatcher
{
    public class DataFilesRelationship
    {
        public string Name { get; private set; }

        public DataFilesRelationship(string name)
        {
            Name = name;
        }
    }
}