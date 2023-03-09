using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace TKDataPatcher
{
    public class CustomizeItemData
    {
        internal ItemEntry[] Entries;

        private IOMemoryStream _stream;

        public CustomizeItemData(string path)
        {
            _stream = new IOMemoryStream(File.OpenRead(path))
            {
                Position = 0
            };
            
            var header = new ItemDataHeader(_stream);

            Entries = new ItemEntry[header.itemCount];
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < header.itemCount; i++)
            {
                Entries[i] = new ItemEntry(_stream);
            }
        }


        public void SaveAndDispose(string path)
        {
            _stream.Dispose();
            _stream = null;

            // Sort items by ID
            // TODO: optimize this horrible mess
            var list = Entries.ToList();
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
            
            Entries = list.ToArray();

            List<string> strings = new List<string>();
            Dictionary<string, uint> stringDictionary = new Dictionary<string, uint>();
            for (int i = 0; i < Entries.Length; i++)
            {
                if(!strings.Exists(x => x == Entries[i].fileNameOffset))
                {
                    strings.Add(Entries[i].fileNameOffset);
                }

                if (!strings.Exists(x => x == Entries[i].nameByString))
                {
                    strings.Add(Entries[i].nameByString);
                }
            }

            using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(new byte[0x10]);
                writer.Write(new byte[Entries.Length * 0x40]);
                
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
                writer.Write(Entries.Length);
                writer.Write((int)0x0);

                // write entries
                for(int i = 0; i < Entries.Length; i++)
                {
                    // write 0x40 bytes of bullshit
                    var entry = Entries[i];
                    
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
        
    }
}
