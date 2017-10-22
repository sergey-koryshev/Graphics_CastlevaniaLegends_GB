using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace src
{
    static class LZSS
    {
        static private byte SeniorBit(byte value, byte number)
        {
            value = (byte)(value >> number);
            return (byte)(value & 0x01);
        }

        static private void SwapHalfs(ref byte value)
        {
            byte seniorHalf = (byte)(value & 0xF);
            byte juniorHalf = (byte)((value >> 4) & 0xF);

            value = (byte)((seniorHalf << 4) | juniorHalf);
        }

        static private void CopyFromBuffer(ref List<byte> bufferArray, ref List<byte> destinationArray, int offset, byte count, ref int positionBuffer)
        {
            for (int i = 0; i < count; i++)
            {
                bufferArray[positionBuffer] = bufferArray[(offset + i) & 0x3FF];
                positionBuffer = ++positionBuffer & 0x3FF;
                destinationArray.Add(bufferArray[(offset + i) & 0x3FF]);
            }
        }

        static public List<byte> _unpack(string pathOpen, List<byte> bufferArray, int lengthOriginal, int begin, out int packedCount)
        {
            List<byte> unpackedArray = new List<byte>();
            packedCount = 0;

            int positionBuffer = 0x3DE;
            Byte key = 0;
            byte countBuffer = 0;
            int offsetBuffer = 0;
            byte firstCurrent = 0;
            byte secondCurrent = 0;

            BinaryReader openFile = new BinaryReader(File.Open(pathOpen, FileMode.Open));
            openFile.BaseStream.Seek(begin, SeekOrigin.Begin);

            while (unpackedArray.Count < lengthOriginal)
            {
                firstCurrent = openFile.ReadByte();
                packedCount++;
                key = firstCurrent;
                for (byte i = 0; i < 8; i++)                {
                    if (SeniorBit(key, i) == 1)
                    {
                        firstCurrent = openFile.ReadByte();
                        packedCount++;
                        unpackedArray.Add(firstCurrent);
                        bufferArray[positionBuffer] = firstCurrent;
                        positionBuffer = ++positionBuffer & 0x3FF;
                    }
                    else
                    {
                        firstCurrent = openFile.ReadByte();
                        secondCurrent = openFile.ReadByte();
                        packedCount += 2;
                        countBuffer = (byte)((secondCurrent & 0x1F) + 0x03);
                        SwapHalfs(ref secondCurrent);
                        offsetBuffer = (((secondCurrent >> 1) & 0x03) << 8) | firstCurrent;
                        CopyFromBuffer(ref bufferArray, ref unpackedArray, offsetBuffer, countBuffer, ref positionBuffer);
                    }
                    if (unpackedArray.Count >= lengthOriginal) 
                        break;
                }
            }

            openFile.Close();

            return unpackedArray;
        }

        public static void Unpack(string pathOpen, string pathSave, string pathBuffer, int lengthOriginal, int begin)
        {
            List<byte> bufferArray = new List<byte>();
            BinaryReader bufferFile = new BinaryReader(File.Open(pathBuffer, FileMode.Open));

            while (bufferArray.Count < bufferFile.BaseStream.Length)
            {
                bufferArray.Add(bufferFile.ReadByte());
            }

            bufferFile.Close();

            int packedCount;

            List<byte> unpackedArray = _unpack(pathOpen, bufferArray, lengthOriginal, begin, out packedCount);

            BinaryWriter saveFile = new BinaryWriter(File.Open(pathSave, FileMode.Create));

            for (int i = 0; i < unpackedArray.Count; i++)
            {
                saveFile.Write(unpackedArray[i]);
            }

            saveFile.Close();

            Console.WriteLine("Uncompressing was successed. Compressed data takes {0} bytes, decompressed data takes {1} bytes.", packedCount, unpackedArray.Count);
        }
    }
}
