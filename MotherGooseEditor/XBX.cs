using AdvancedBinary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MotherGooseEditor {
    public class XBX
    {
        XBXHeader Header;
        public Encoding Encoding = Encoding.GetEncoding(932);
        byte[] Script;
        public XBX(byte[] Script) {
            this.Script = Script;
        }

        public string[] Import() {
            Header = new XBXHeader();
            Header.OffsetWork = GetOffsetWork();
            Header.StringWork = GetStringWork();
            Tools.ReadStruct(Script, ref Header);

            List<string> Strings = new List<string>();
            for (uint i = 0; i < Header.Strings.Length; i++) {
                if (!Header.IsString[i % Header.EntryLen])
                    continue;
                Strings.Add(Header.Strings[i]);
            }

            return Strings.ToArray();
        }

        public byte[] Export(string[] Strings) {
            for (uint i = 0, x = 0; i < Header.Strings.Length; i++) {
                if (!Header.IsString[i % Header.EntryLen])
                    continue;
               Header.Strings[i] = Strings[x++];
            }

            return Tools.BuildStruct(ref Header);
        }

        public FieldInvoke GetOffsetWork() {
            return new FieldInvoke(
                (Stream, FromReader, Struct) => {
                    XBXHeader Header = Struct;

                    uint Count = (Header.EntryLen * Header.EntriesCount);
                    if (FromReader) {
                        StructReader Reader = new StructReader(Stream);
                        Header.Offsets = new uint[Count];
                        for (uint i = 0; i < Count; i++) {
                            Header.Offsets[i] = Reader.ReadRawType(Const.UINT32);
                        }

                        Header.IsString = new bool[Header.EntryLen];
                        for (uint i = 0; i < Header.EntryLen; i++)
                            Header.IsString[i] = IsString(Stream, Header.Offsets[i] + Header.StrPos);

                    } else {
                        for (uint i = 0, x = 0; i < Header.Offsets.Length; i++) {
                            if (!Header.IsString[i % Header.EntryLen])
                                continue;

                            Header.Offsets[i] = x;
                            x += (uint)Encoding.GetByteCount(Header.Strings[i]) + 1;
                        }
                        StructWriter Writer = new StructWriter(Stream);
                        for (uint i = 0; i < Count; i++) {
                            Writer.WriteRawType(Const.UINT32, Header.Offsets[i]);
                        }

                    }

                    return Header;
                });
        }

        public FieldInvoke GetStringWork() {
            return new FieldInvoke(
                (Stream, FromReader, Struct) => {
                    XBXHeader Header = Struct;

                    if (FromReader) {
                        using (StructReader Reader = new StructReader(Stream, Encoding: Encoding)) {
                            Header.Strings = new string[Header.Offsets.Length];
                            for (uint i = 0; i < Header.Offsets.Length; i++) {
                                Reader.BaseStream.Position = Header.Offsets[i] + Header.StrPos;
                                if (Header.IsString[i%Header.EntryLen])
                                    Header.Strings[i] = Reader.ReadString(StringStyle.CString);
                            }
                        }
                    } else {
                        using (StructWriter Writer = new StructWriter(Stream, Encoding: Encoding)) {
                            Writer.BaseStream.Position = Header.StrPos;
                            for (uint i = 0; i < Header.Offsets.Length; i++) {
                                if (Header.IsString[i % Header.EntryLen])
                                    Writer.Write(Header.Strings[i], StringStyle.CString);
                            }
                        }
                    }

                    return Header;
                });
        }

        private bool InString(Stream Stream) {
            Stream.Position--;
            byte Before = (byte)Stream.ReadByte();
            byte Current = (byte)Stream.ReadByte();
            Stream.Position--;
            if (Before == 0x00 && Current != 0x00)
                return true;
            return false;
        }

        private bool IsString(Stream Stream, long At) {
            long Pos = Stream.Position;
            Stream.Position = At;
            bool IsStr = InString(Stream);
            Stream.Position = Pos;
            return IsStr;
        }
    }

    public struct XBXHeader {
        [FString(Length = 4)]
        public string Signature;

        public uint EntriesCount;
        public uint EntryLen;
        public uint StrPos;

        public FieldInvoke OffsetWork;
        public FieldInvoke StringWork;

        [Ignore]
        public uint[] Offsets;

        [Ignore]
        public string[] Strings;

        [Ignore]
        public bool[] IsString;
    }
}
