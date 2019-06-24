using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.IO;

namespace OpenKh.Kh2
{
    public static class Vsb
    {
        private struct Entry
        {
            public int Offset, Length;
        }

        private const ulong MagicCode = 0x006563696F56706F49U;

        public static List<Vag> Open(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var reader = new BinaryReader(stream);
            if (stream.Length < 16L || reader.ReadUInt64() != MagicCode)
                throw new InvalidDataException("Invalid header");

            reader.BaseStream.Position = 0xC;
            var entriesCount = reader.ReadInt32();
            var entries = new List<Entry>(entriesCount);
            for (int i = 0; i < entriesCount; i++)
            {
                reader.BaseStream.Position = 0x10 + (i * 0x8);
                var offset = reader.ReadInt32();
                var length = reader.ReadInt32();

                //IopVoice can have "ghost entries"
                //These entries' offsets and length are FFFFFFFF
                //This entries should also be added in some way, because if you save this file once again it might cause problems in KH
                if (offset == -1 && length == -1)
                    continue;

                //Offsets in IopVoice don't count the IopVoice header
                //Add IopVoice Header to offset so VAGs are read properly
                offset += 0x10 + (entriesCount * 0x8);

                entries.Add(new Entry()
                {
                    Offset = offset,
                    Length = length,
                });
            }

            return entries
                .Select(x => new Vag(new SubStream(stream, x.Offset, x.Length)))
                .ToList();
        }

        public static void Save(Stream stream, IEnumerable<Vag> vags)
        {

        }

        public static int Count(Stream stream)
        {
            return Open(stream).Count;
        }
    }
}
