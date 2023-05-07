using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace TKDataPatcher
{
    public class CustomizeItemData : BinListData
    {
        private ItemEntry[] _entries;

        public override void ReadData()
        {
            _entries = new ItemEntry[Header.itemCount];
            for(int i = 0; i < Header.itemCount; i++)
            {
                _entries[i] = new ItemEntry(Stream);
            }
        }

        public override void Save(string path)
        {
            // Sort items by ID
            // TODO: optimize this horrible mess
            var list = _entries.ToList();
            list.Sort((s1, s2) =>
            {
                int charId = s1.charId.CompareTo(s2.charId);
                if (charId != 0)
                {
                    return charId;
                }

                int slotId = s1.slotId.CompareTo(s2.slotId);
                if(slotId != 0)
                {
                    return slotId;
                }
                
                int item = s1.itemId.CompareTo(s2.itemId);
                return item;
            });
            
            _entries = list.ToArray();

            List<string> strings = new List<string>();
            Dictionary<string, uint> stringDictionary = new Dictionary<string, uint>();
            for (int i = 0; i < _entries.Length; i++)
            {
                if(!strings.Exists(x => x == _entries[i].fileNameOffset))
                {
                    strings.Add(_entries[i].fileNameOffset);
                }

                if (!strings.Exists(x => x == _entries[i].nameByString))
                {
                    strings.Add(_entries[i].nameByString);
                }
            }

            using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(new byte[0x10]);
                writer.Write(new byte[_entries.Length * 0x40]);
                
                uint customizeItemDataOffset = (uint)writer.BaseStream.Position;
                
                writer.Write(Encoding.ASCII.GetBytes("customize_item_data"));
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
                writer.Write(customizeItemDataOffset);
                writer.Write((int)0x0);
                writer.Write(_entries.Length);
                writer.Write((int)0x0);

                // write entries
                for(int i = 0; i < _entries.Length; i++)
                {
                    // write 0x40 bytes of bullshit
                    var entry = _entries[i];
                    
                    uint fileNameOffsetStr = stringDictionary.GetValueOrDefault(entry.fileNameOffset);
                    uint nameByStringStr = stringDictionary.GetValueOrDefault(entry.nameByString);
                    
                    writer.Write(entry.itemId);
                    writer.Write(entry.packageIndex);
                    writer.Write(fileNameOffsetStr != 0 ? fileNameOffsetStr : nullOffset);
                    writer.Write(entry.unk);
                    writer.Write(entry.charId);
                    writer.Write(entry.slotId);
                    writer.Write(entry.veryUnk);
                    writer.Write(entry.nameById);
                    writer.Write(nameByStringStr != 0 ? nameByStringStr : nullOffset);
                    writer.Write(entry.unk2);
                    writer.Write(entry.packageIndex2);
                    writer.Write(entry.unk3);
                    writer.Write(entry.playerCus);
                    writer.Write(entry.isColorable);
                    writer.Write(entry.unk4);
                    writer.Write(entry.rarity);
                    writer.Write(entry.itemFlagPackageIndex);
                    writer.Write(entry.itemCost);
                    writer.Write(entry.packageIndex3);
                    writer.Write(nullOffset);
                    writer.Write(entry.unk7);
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
                    catch (Exception exception)
                    {
                        Log.Warning($"Could not parse line {i} in {file}. Skipping file. " + exception);
                        continue;
                    }
                    
                    entriesToAdd.Add(entry);
                    Log.Info($"Adding file entry: {file} with the ID: {entry.itemId}");
                }
            }

            _entries = entriesToAdd.ToArray();
        }

        public override void WriteCSVData(string path)
        {
            base.WriteCSVData(path);

            string itemsFile = Path.Combine(path, "items.csv");
            StringBuilder sb = new StringBuilder();

            foreach (var entry in _entries)
            {
                string fileNameOffset = entry.fileNameOffset == "\x00" ? "\\x00" : entry.fileNameOffset;
                string keyOffset = entry.nameByString == "\x00" ? "\\x00" : entry.nameByString;
                string nullOffset = entry.nullOffset == "\x00" ? "\\x00" : entry.nullOffset;
                
                sb.AppendLine(
                    $"{entry.itemId},{entry.packageIndex},{fileNameOffset},{entry.unk},{entry.charId},{entry.slotId},{entry.veryUnk}," +
                    $"{entry.nameById},{keyOffset},{entry.unk2},{entry.packageIndex2},{entry.unk3},{entry.playerCus},{entry.isColorable}," +
                    $"{entry.unk4},{entry.rarity},{entry.itemFlagPackageIndex},{entry.itemCost},{entry.packageIndex3},{nullOffset},{entry.unk7}");
            }
            
            File.WriteAllText(itemsFile, sb.ToString());
        }
    }
}
