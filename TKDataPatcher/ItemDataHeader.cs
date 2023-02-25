using System;
using System.Collections.Generic;
using System.Text;

namespace TKDataPatcher
{
    struct ItemDataHeader
    {
        public uint offset;
        public int unk;
        public uint itemCount;
        public int unk2;

        internal ItemDataHeader(IOMemoryStream stream)
        {
            offset = stream.ReadUInt();
            unk = stream.ReadInt();
            itemCount = stream.ReadUInt();
            unk2 = stream.ReadInt();
        }
    }
}
