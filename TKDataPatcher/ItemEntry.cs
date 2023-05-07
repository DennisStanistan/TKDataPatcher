using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace TKDataPatcher
{
    struct ItemEntry
    {
        public uint itemId;
        public int packageIndex; // always -1
        public string fileNameOffset;
        public int unk; // always 0
        public byte charId;
        public byte slotId;
        public short veryUnk; // ?
        public int nameById; // parent cmn ID
        public string nameByString; // usually null
        public int unk2; // always 0
        public int packageIndex2; // Group id. unk?
        public int unk3; // group id
        public byte playerCus; //  1 = 1p, 2 = 2p, 0 = generic
        public byte isColorable; // 0 - non coloarable, n = slot colors
        public byte unk4; // usually 0
        public byte rarity; // stars
        public int itemFlagPackageIndex; // package id - DLC,locked,unlocked, slot flag
        public int itemCost;
        public int packageIndex3;
        public string nullOffset;
        public int unk7;

        internal ItemEntry(string line)
        {
            string[] split = line.Split(',');
            if(split.Length != 21)
            {
                throw new Exception("Could not parse line");
            }

            itemId = uint.Parse(split[0]);
            packageIndex = int.Parse(split[1]);
            fileNameOffset = split[2];
            unk = int.Parse(split[3]);
            charId = byte.Parse(split[4]);
            slotId = byte.Parse(split[5]);
            veryUnk = short.Parse(split[6]);
            nameById = int.Parse(split[7]);
            nameByString = split[8];
            unk2 = int.Parse(split[9]);
            packageIndex2 = int.Parse(split[10]);
            unk3 = int.Parse(split[11]);
            playerCus = byte.Parse(split[12]);
            isColorable = byte.Parse(split[13]);
            unk4 = byte.Parse(split[14]);
            rarity = byte.Parse(split[15]);
            itemFlagPackageIndex = int.Parse(split[16]);
            itemCost = int.Parse(split[17]);
            packageIndex3 = int.Parse(split[18]);
            nullOffset = split[19];
            unk7 = int.Parse(split[20]);
        }

        internal ItemEntry(IOMemoryStream stream)
        {
            itemId = stream.ReadUInt();
            packageIndex = stream.ReadInt();
            fileNameOffset = stream.ReadStringOffset(stream.ReadUInt());
            unk = stream.ReadInt();
            charId = stream.ReadByte();
            slotId = stream.ReadByte();
            veryUnk = stream.ReadShort();
            nameById = stream.ReadInt();
            nameByString = stream.ReadStringOffset(stream.ReadUInt());
            unk2 = stream.ReadInt();
            packageIndex2 = stream.ReadInt();
            unk3 = stream.ReadInt();
            playerCus = stream.ReadByte();
            isColorable = stream.ReadByte();
            unk4 = stream.ReadByte();
            rarity = stream.ReadByte();
            itemFlagPackageIndex = stream.ReadInt();
            itemCost = stream.ReadInt();
            packageIndex3 = stream.ReadInt();
            nullOffset = stream.ReadStringOffset(stream.ReadUInt());
            unk7 = stream.ReadInt();
        }
    }

    
}
