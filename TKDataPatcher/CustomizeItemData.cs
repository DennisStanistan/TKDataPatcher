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
        internal ItemDataHeader Header;

        private IOMemoryStream stream;

        public CustomizeItemData(string path)
        {
            stream = new IOMemoryStream(File.OpenRead(path));
            stream.Position = 0;
            Header = new ItemDataHeader(stream);

            Entries = new ItemEntry[Header.itemCount];
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < Header.itemCount; i++)
            {
                Entries[i] = new ItemEntry(stream);
            }
            // Custom entry
            //Entries[Header.itemCount] = new ItemEntry();

            /*Entries[Header.itemCount].index = 190577;
            Entries[Header.itemCount].packageIndex = -1;
            Entries[Header.itemCount].fileNameOffset = "asa_bdf_two_booty";
            Entries[Header.itemCount].unk = 0x0;

            Entries[Header.itemCount].unk2 = -62957;
            Entries[Header.itemCount].unk3 = 0x0;
            Entries[Header.itemCount].keyOffset = "\x00";
            Entries[Header.itemCount].unk4 = 0x0;

            Entries[Header.itemCount].packageIndex2 = -217;
            Entries[Header.itemCount].unk5 = 2517;
            Entries[Header.itemCount].unk6 = 0x3000500;
            Entries[Header.itemCount].itemFlagPackageIndex = -253;

            Entries[Header.itemCount].itemCost = 200000;
            Entries[Header.itemCount].packageIndex3 = -1;
            Entries[Header.itemCount].nullOffset = "\x00";
            Entries[Header.itemCount].unk7 = 0x0;

            List<ItemEntry> newEntries = new List<ItemEntry>();*/
            //newEntries.AddRange(Entries);
            //newEntries.RemoveAt(newEntries.FindIndex(x => x.index == 0x249FA));
            
            //Entries = newEntries.ToArray();
            //File.WriteAllText("output.txt", sb.ToString());
            /*while(stream.Position != stream.Stream.Length)
            {
                Console.WriteLine($"{stream.Position:X8}: {stream.ReadASCIIString()}");
            }*/

            //Save();
        }


        public void SaveAndDispose(string path)
        {
            stream.Dispose();
            stream = null;

            // Sort items by ID
            // TODO: optimize this horrible mess
            var list = Entries.ToList();
            list.Sort((s1, s2) => s1.itemId.CompareTo(s2.itemId));
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

                if (!strings.Exists(x => x == Entries[i].nullOffset))
                {
                    strings.Add(Entries[i].nullOffset);
                }
            }

            uint nullOffset = 0x0;
            uint customize_item_data_offset = 0x0;
            using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(new byte[0x10]);
                writer.Write(new byte[Entries.Length * 0x40]);
                customize_item_data_offset = (uint)writer.BaseStream.Position;
                writer.Write(Encoding.ASCII.GetBytes("customize_item_data"));
                writer.Write((byte)0x00);
                for (int i = 0; i < strings.Count; i++)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes("\x00");
                    try
                    {
                        bytes = Encoding.ASCII.GetBytes(strings[i]);
                    }
                    catch
                    {

                    }

                    if(bytes.Length == 1 && bytes[0] == 0x00)
                    {
                        nullOffset = (uint)writer.BaseStream.Position;
                    }
                    strings[i] = strings[i] == null ? "\x00" : strings[i];
                    stringDictionary.Add(strings[i], (uint)writer.BaseStream.Position);
                    writer.Write(bytes);
                    writer.Write((byte)0x00);
                }

                writer.BaseStream.Position = 0x0;

                // Write header
                writer.Write(customize_item_data_offset);
                writer.Write((int)0x0);
                writer.Write(Entries.Length);
                writer.Write((int)0x0);

                // write entries
                for(int i = 0; i < Entries.Length; i++)
                {
                    // write 0x40 bytes of bullshit
                    var entry = Entries[i];
                    /*writer.Write(entry.index);
                    writer.Write(entry.packageIndex);
                    writer.Write(stringDictionary.GetValueOrDefault(entry.fileNameOffset));
                    writer.Write(entry.unk);
                    writer.Write(entry.unk2);
                    writer.Write(entry.unk3);
                    writer.Write(stringDictionary.GetValueOrDefault(entry.keyOffset));
                    writer.Write(entry.unk4);
                    writer.Write(entry.packageIndex2);
                    writer.Write(entry.unk5);
                    writer.Write(entry.unk6);
                    writer.Write(entry.itemFlagPackageIndex);
                    writer.Write(entry.itemCost);
                    writer.Write(entry.packageIndex3);
                    writer.Write(nullOffset);
                    writer.Write(entry.unk7);*/
                    writer.Write(entry.itemId);
                    writer.Write(entry.packageIndex);
                    writer.Write(stringDictionary.GetValueOrDefault(entry.fileNameOffset));
                    writer.Write(entry.unk);
                    writer.Write(entry.charId);
                    writer.Write(entry.slotId);
                    writer.Write(entry.veryUnk);
                    writer.Write(entry.nameById);
                    writer.Write(stringDictionary.GetValueOrDefault(entry.nameByString));
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
