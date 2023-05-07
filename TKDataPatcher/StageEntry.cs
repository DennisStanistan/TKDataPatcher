namespace TKDataPatcher
{
    struct StageEntry
    {
        public byte stageId; // 0x0
        //public byte[] padding;
        public short unk2l; // 0x4
        public byte unk2s;
        public string stgStringOffset; // 0x8
        public uint unk3; // 0xC
        public string stageNameOffset; // 0x10
        public uint unk4; // 0x14
        public uint unk5; // 0x18
        public uint unk6; // 0x1C
        public uint unk7; // 0x20
        public uint unk8; // 0x24
        public string nullOffset; // 0x28
        public uint unk9; // 0x2C
        public string stageNameOffset2; // 0x30
        public uint unk10; // 0x34
        public string unkStringOffset; // 0x38
        public uint unk11; // 0x3C
        public string stageNameOffset3; // 0x40
        public uint unk12; // 0x44
        public uint unk13; // 0x48
        public uint unk14; // 0x4C
        public uint unk15; // 0x50
        public uint unk16; // 0x50
        public byte unk17; // 0x50
        
        internal StageEntry(string line)
        {
            string[] split = line.Split(',');
            stageId = byte.Parse(split[0]);
            //unk2 = uint.Parse(split[1]);
            unk2l = short.Parse(split[2]);
            unk2s = byte.Parse(split[3]);
            stgStringOffset = split[4];
            unk3  = uint.Parse(split[5]);
            stageNameOffset = split[6];
            unk4  = uint.Parse(split[7]);
            unk5  = uint.Parse(split[8]);
            unk6  = uint.Parse(split[9]);
            unk7  = uint.Parse(split[10]);
            unk8  = uint.Parse(split[11]);
            nullOffset = split[12];
            unk9 = uint.Parse(split[13]);
            stageNameOffset2 = split[14];
            unk10 = uint.Parse(split[15]);
            unkStringOffset = split[16];
            unk11 = uint.Parse(split[17]);
            stageNameOffset3 = split[18];
            unk12 = uint.Parse(split[19]);
            unk13 = uint.Parse(split[20]);
            unk14 = uint.Parse(split[21]);
            unk15 = uint.Parse(split[22]);
            unk16 = uint.Parse(split[23]);
            unk17 = byte.Parse(split[24]);
        }

        internal StageEntry(IOMemoryStream stream)
        {
            stageId = stream.ReadByte();
            //unk2 = stream.ReadUInt();
            unk2l = stream.ReadShort();
            unk2s = stream.ReadByte();
            stgStringOffset = stream.ReadStringOffset(stream.ReadUInt());
            unk3 = stream.ReadUInt();
            stageNameOffset = stream.ReadStringOffset(stream.ReadUInt());
            unk4 = stream.ReadUInt();
            unk5 = stream.ReadUInt();
            unk6 = stream.ReadUInt();
            unk7 = stream.ReadUInt();
            unk8 = stream.ReadUInt();
            nullOffset = stream.ReadStringOffset(stream.ReadUInt());
            unk9 = stream.ReadUInt();
            stageNameOffset2 = stream.ReadStringOffset(stream.ReadUInt());
            unk10 = stream.ReadUInt();
            unkStringOffset = stream.ReadStringOffset(stream.ReadUInt());
            unk11 = stream.ReadUInt();
            stageNameOffset3 = stream.ReadStringOffset(stream.ReadUInt());
            unk12 = stream.ReadUInt();
            unk13 = stream.ReadUInt();
            unk14 = stream.ReadUInt();
            unk15 = stream.ReadUInt();
            unk16 = stream.ReadUInt();
            unk17 = stream.ReadByte();
        }
    }
}