using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TKDataPatcher
{
    public class StageListConsole : BinListData
    {
        private StageEntry[] _entries;

        public override void ReadData()
        {
            _entries = new StageEntry[Header.itemCount];
            for (int i = 0; i < Header.itemCount; i++)
            {
                _entries[i] = new StageEntry(Stream);
            }
        }

        public override void Save(string path)
        {
            List<string> strings = new List<string>();
            Dictionary<string, uint> stringDictionary = new Dictionary<string, uint>();
            for (int i = 0; i < _entries.Length; i++)
            {
                if (!strings.Exists(x => x == _entries[i].stgStringOffset)) strings.Add(_entries[i].stgStringOffset);
                if (!strings.Exists(x => x == _entries[i].stageNameOffset)) strings.Add(_entries[i].stageNameOffset);
                if (!strings.Exists(x => x == _entries[i].stageNameOffset2)) strings.Add(_entries[i].stageNameOffset2);
                if (!strings.Exists(x => x == _entries[i].unkStringOffset)) strings.Add(_entries[i].unkStringOffset);
                if (!strings.Exists(x => x == _entries[i].stageNameOffset3)) strings.Add(_entries[i].stageNameOffset3);
            }

            using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(new byte[0x10]);
                writer.Write(new byte[_entries.Length * 0x50]);

                uint itemDataOffset = (uint)writer.BaseStream.Position;

                writer.Write(Encoding.ASCII.GetBytes("stage_list_console"));
                writer.Write((byte)0x00);

                // Write null offset
                uint nullOffset = (uint)writer.BaseStream.Position;
                writer.Write((byte)0x00);
                for (int i = 0; i < strings.Count; i++)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(strings[i]);

                    if (strings[i] == "\\x00")
                    {
                        continue;
                    }

                    stringDictionary.Add(strings[i], (uint)writer.BaseStream.Position);
                    writer.Write(bytes);
                    writer.Write((byte)0x00);
                }

                writer.BaseStream.Position = 0x0;

                // Write header
                writer.Write(itemDataOffset);
                writer.Write((int)0x0);
                writer.Write(_entries.Length);
                writer.Write((int)0x0);

                // Write entries
                for (int i = 0; i < _entries.Length; i++)
                {
                    // Write 0x50 bytes of bullshit
                    var entry = _entries[i];

                    uint stgStringOffset = stringDictionary.GetValueOrDefault(entry.stgStringOffset);
                    uint stageNameOffset = stringDictionary.GetValueOrDefault(entry.stageNameOffset);
                    uint stageNameOffset2 = stringDictionary.GetValueOrDefault(entry.stageNameOffset2);
                    uint unkStringOffset = stringDictionary.GetValueOrDefault(entry.unkStringOffset);
                    uint stageNameOffset3 = stringDictionary.GetValueOrDefault(entry.stageNameOffset3);

                    writer.Write(entry.stageId);
                    //writer.Write(entry.unk2);
                    writer.Write(entry.unk2l);
                    writer.Write(entry.unk2s);
                    writer.Write(stgStringOffset != 0 ? stgStringOffset : nullOffset);
                    writer.Write(entry.unk3);
                    writer.Write(stageNameOffset != 0 ? stageNameOffset : nullOffset);
                    writer.Write(entry.unk4);
                    writer.Write(entry.unk5);
                    writer.Write(entry.unk6);
                    writer.Write(entry.unk7);
                    writer.Write(entry.unk8);
                    writer.Write(nullOffset);
                    writer.Write(entry.unk9);
                    writer.Write(stageNameOffset2 != 0 ? stageNameOffset2 : nullOffset);
                    writer.Write(entry.unk10);
                    writer.Write(unkStringOffset != 0 ? unkStringOffset : nullOffset);
                    writer.Write(entry.unk11);
                    writer.Write(stageNameOffset3 != 0 ? stageNameOffset3 : nullOffset);
                    writer.Write(entry.unk12);
                    writer.Write(entry.unk13);
                    writer.Write(entry.unk14);
                    writer.Write(entry.unk15);
                    writer.Write(entry.unk16);
                    writer.Write(entry.unk17);
                }
            }
        }

        public override void PatchCSVData(string path)
        {
            var files = Directory.EnumerateFiles(path, "*.csv", SearchOption.AllDirectories);
            var enumerable = files as string[] ?? files.ToArray();
            if (!enumerable.Any())
            {
                return;
            }

            List<StageEntry> entriesToAdd = new List<StageEntry>();
            foreach (var file in enumerable)
            {
                string[] fileLines = File.ReadAllLines(file);

                for (int i = 0; i < fileLines.Length; i++)
                {
                    StageEntry entry;
                    try
                    {
                        entry = new StageEntry(fileLines[i]);
                    }
                    catch
                    {
                        Log.Warning($"Could not parse line {i} in {file}. Skipping file.");
                        continue;
                    }

                    entriesToAdd.Add(entry);
                    Log.Info($"Adding stage entry: {file} with the ID: {entry.stageId}");
                }
            }

            _entries = entriesToAdd.ToArray();
        }
        
        public override void WriteCSVData(string path)
        {
            base.WriteCSVData(path);
            
            string itemsFile = Path.Combine(path, "stages.csv");
            StringBuilder sb = new StringBuilder();

            foreach (var entry in _entries)
            {
                string stgStringOffset = entry.stgStringOffset == "\x00" ? "\\x00" : entry.stgStringOffset;
                string stageNameOffset = entry.stageNameOffset == "\x00" ? "\\x00" : entry.stageNameOffset;
                string nullOffset = entry.nullOffset == "\x00" ? "\\x00" : entry.nullOffset;
                string stageNameOffset2 = entry.stageNameOffset2 == "\x00" ? "\\x00" : entry.stageNameOffset2;
                string unkStringOffset = entry.unkStringOffset == "\x00" ? "\\x00" : entry.unkStringOffset;
                string stageNameOffset3 = entry.stageNameOffset3 == "\x00" ? "\\x00" : entry.stageNameOffset3;

                /*sb.AppendLine(
                    $"{entry.stageId},{entry.unk2},{entry.unk2l},{entry.unk2s},{stgStringOffset},{entry.unk3},{stageNameOffset},{entry.unk4},{entry.unk5},{entry.unk6},{entry.unk7},{entry.unk8}," +
                    $"{nullOffset},{entry.unk9},{stageNameOffset2},{entry.unk10},{unkStringOffset},{entry.unk11},{stageNameOffset3},{entry.unk12}," +
                    $"{entry.unk12},{entry.unk13},{entry.unk14},{entry.unk15},{entry.unk16},{entry.unk17}");*/
            }
            
            File.WriteAllText(itemsFile, sb.ToString());
        }
    }
}