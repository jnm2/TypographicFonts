using System;
using System.IO;

namespace jnm2.TypographicFonts
{
    // Short and sweet.
    internal sealed class BigEndianBinaryReader : IDisposable 
    {
        private readonly Stream input;
        private readonly bool leaveOpen;

        public Stream BaseStream { get { return input; } }

        public BigEndianBinaryReader(Stream input, bool leaveOpen = false)
        {
            if (input == null) throw new ArgumentNullException("input");
            this.input = input;
            this.leaveOpen = leaveOpen;
        }
        
        public ushort ReadUInt16()
        {
            return (ushort)((input.ReadByte() << 8) | input.ReadByte());
        }

        public uint ReadUInt32()
        {
            return ((uint)input.ReadByte() << 24) | ((uint)input.ReadByte() << 16) | ((uint)input.ReadByte() << 8) | (uint)input.ReadByte();
        }
        
        public byte[] ReadBytes(int numBytes)
        {
            var r = new byte[numBytes];
            if (input.Read(r, 0, numBytes) != numBytes) throw new EndOfStreamException();
            return r;
        }

        public void Dispose()
        {
            if (leaveOpen) return;
            this.BaseStream.Dispose();
        }
    }
}
