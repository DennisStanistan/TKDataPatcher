using System;
using System.Collections.Generic;
using System.Text;

namespace TKDataPatcher
{
    struct ItemDataFooter
    {
        public int unk;
        public int packageIndex;
        public uint nullOffset;
        public int unk2;

        internal ItemDataFooter(IOMemoryStream stream)
        {
            unk = stream.ReadInt();
            packageIndex = stream.ReadInt();
            nullOffset = stream.ReadUInt();
            unk2 = stream.ReadInt();
        }
    }
}
