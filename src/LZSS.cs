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
        static private byte GetBit(byte value, byte number)
        {
            /* The auxiliary method for _unpack method
             * Determines the bit value of the specified byte
             * Takes:
             * - value - the targeted byte
             * - number - bit number of the byte
             * Returns:
             * - the bit value */
            return (byte)((value >> number) & 0x01);
        }

        static private void SwapHalvs(ref byte value)
        {
            /* The auxiliary method for _unpack method
             * Swaps halves of the specified byte
             * Takes:
             * - value - the targeted byte */
            byte juniorHalf = (byte)(value & 0xF);
            byte SeniorHalf = (byte)((value >> 4) & 0xF);

            value = (byte)((juniorHalf << 4) | SeniorHalf);
        }

        static private void CopyFromBuffer(ref List<byte> bufferArray, ref List<byte> destinationArray, int offset, byte count, ref int positionBuffer)
        {
            /* The auxiliary method for _unpack method
             * Copies the specified count of bytes from the buffer array to the desination array
             * Takes:
             * - bufferArray - the array 
             * - destinationArray - the destination array
             * - offset - the position in the buffer array from which the bytes will be copied
             * - count - the count of copied bytes
             * - positionBuffer - current(into the unpack method) position of the buffer array */
            try
            {
                for (int i = 0; i < count; i++)
                {
                    bufferArray[positionBuffer] = bufferArray[(offset + i) & 0x3FF];
                    positionBuffer = ++positionBuffer & 0x3FF;
                    destinationArray.Add(bufferArray[(offset + i) & 0x3FF]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static private List<byte> _unpack(string pathOpen, List<byte> bufferArray, int begin, int lengthUnpackedArray, out int packedCount)
        {
            /* Basic method to decompressing data
             * Unpacks data packed by LZSS algorithm
             * Takes:
             * pathOpen - the path to file with packed data
             * bufferArray - the buffer array
             * lengthUnpackedArray - the required count of bytes in array with unpacked data
             * packedCount - the count of bytes in the sequence of packed data
             * Returns:
             * - the array with unpacked data
             * Date of creating:
             * - October 2017 */

            List<byte> unpackedArray = new List<byte>(); // an array for unpacked data
            packedCount = 0; // the count of bytes in the sequence of packed data

            try
            {
                int positionBuffer = 0x3DE; // a position in buffer array
                Byte flagByte = 0; // a flag-byte
                byte countBuffer = 0; // a number of bytes which will be copied from the buffer to array with unpacked data
                int offsetBuffer = 0; // a position in a buffer from which bytes will be copied
                byte firstCurrent = 0; // a byte which will be copied to a unpacked array in case a bit of a flag-byte equals 1; a part of offset if a bit of a flag-byte equals 0
                byte secondCurrent = 0; // is used in case a flag-byte equals 0

                BinaryReader openFile = new BinaryReader(File.Open(pathOpen, FileMode.Open));
                openFile.BaseStream.Seek(begin, SeekOrigin.Begin);

                while (unpackedArray.Count < lengthUnpackedArray) // while the unpacked array's count less than the required count of bytes in array with unpacked data
                {
                    flagByte = openFile.ReadByte(); // reading flag-byte
                    packedCount++; // increasing the count of bytes in the sequence of packed data by 1
                    for (byte i = 0; i < 8; i++) // a loop in which we pass through all bits of the flag-byte
                    {
                        if (GetBit(flagByte, i) == 1) // if a specified bit equals 1 then
                        {
                            firstCurrent = openFile.ReadByte(); // reading byte which will be copied in array with unpacked data
                            packedCount++; // increasing the count of bytes in the sequence of packed data by 1
                            unpackedArray.Add(firstCurrent); // adding the byte to array with unpacked data
                            bufferArray[positionBuffer] = firstCurrent; // copying the byte to buffer
                            positionBuffer = ++positionBuffer & 0x3FF; // increasing the position in the buffer and make sure that the buffer size does not exceed $400 bytes
                        }
                        else // if a bit equals 0 then
                        {
                            firstCurrent = openFile.ReadByte(); // reading first byte
                            secondCurrent = openFile.ReadByte(); // reading second byte
                            packedCount += 2; // increasing the count of bytes in the sequence of packed data by 2
                            countBuffer = (byte)((secondCurrent & 0x1F) + 0x03); // calculating the number of bytes which will be copied from the buffer to array with unpacked data
                            SwapHalvs(ref secondCurrent); // swapping the halves of the second byte
                            offsetBuffer = (((secondCurrent >> 1) & 0x03) << 8) | firstCurrent; // calculating the position from which bytes will be copied from the buffer to array with unpacked data
                            CopyFromBuffer(ref bufferArray, ref unpackedArray, offsetBuffer, countBuffer, ref positionBuffer); // calling method of copying the bytes from the buffer to the array with unpacked data
                        }
                        if (unpackedArray.Count >= lengthUnpackedArray) // if we reached the required count of bytes in array with unpacked data then
                            break; // leaving the loop
                    }
                }

                openFile.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        
            return unpackedArray;
        }

        public static void Unpack(string pathOpen, string pathSave, string pathBuffer, int begin, int lengthUnpackedArray)
        {
            /* Public method to decompressing the packed data
             * Takes:
             * - pathOpen - the path to file with packed data
             * - pathsave - the path to file in which the unpacked data will be saved
             * - pathBuffer - the path to file with buffer's data
             * - lengthUnpackedArray - the required count of bytes in array with unpacked data
             * - begin - initial position in the file to be opened */
            try
            {
                #region Opening buffer file

                List<byte> bufferArray = new List<byte>();
                BinaryReader bufferFile = new BinaryReader(File.Open(pathBuffer, FileMode.Open));
                while (bufferArray.Count < bufferFile.BaseStream.Length)
                {
                    bufferArray.Add(bufferFile.ReadByte());
                }
                bufferFile.Close();

                #endregion

                #region Decompressing data

                int packedCount;
                List<byte> unpackedArray = _unpack(pathOpen, bufferArray, begin, lengthUnpackedArray, out packedCount);

                #endregion

                #region Saving unpacked data

                BinaryWriter saveFile = new BinaryWriter(File.Open(pathSave, FileMode.Create));
                for (int i = 0; i < unpackedArray.Count; i++)
                {
                    saveFile.Write(unpackedArray[i]);
                }
                saveFile.Close();

                #endregion

                Console.WriteLine("Uncompressing was successful. Compressed data takes {0} bytes, decompressed data takes {1} bytes.", packedCount, unpackedArray.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static bool FindMaxLengthSequence(List<byte> bufferArray, int bufferBegin, int bufferEnd, List<byte> unpackedArray, int positionUnpacked, out int foundSequencePosition, out int foundSequenceCount)
        {
            /* The auxiliary method for _pack method
             * It finds a sequence of maximum length in the buffer array
             * Takes:
             * - bufferArray - the buffer array in that we will look for the sequence
             * - bufferBegin - position of begin's buffer array
             * - bufferEnd - position of end's buffer array
             * - unpackedArray - the array of decompressed data from that we will take the sequence for search
             * - positionUnpacked - position in the array of decompressed data
             * Returns:
             * - true - if the required sequence was found, false - if it was not found
             * - foundSequencePosition - position of the found sequence in the buffer array
             * - foundSequenceCount - number of the found sequence's bytes
             * Date of creasting:
             * - October 2017 */
            bool result = false; // result of the method, initial value is false
            foundSequencePosition = 0; // position of the found sequence in the buffer array
            foundSequenceCount = 0; // number of the found sequence's bytes
            int countSameBytes = 0; // number of unpacked array's bytes are coincided with buffer array's bytes
            int positionSameBytes = 0; // position of the begin of the matching bytes;
            int positionBuffer = bufferBegin; // current position in the buffer array
            int positionSequence = positionUnpacked; // current position in the unpacked array
            int lengthRleSequence = 1; // current length of sequence, when we check fro RLE
            int minCountSameBytes = 2; // the minimum length of sequence that we will compress
            int maxLengthSequence = 0x1F + 3; // the maximum length of sequence that we can compress
            int maxLengthRleSequence = (int)(maxLengthSequence / 3); // the maximum length of sequence that we can compress during check for RLE
            bool foundRLE; // true - we found RLE, false - we didn't find RLE

            do 
            {
                if (bufferArray[positionBuffer] == unpackedArray[positionSequence] && countSameBytes <= maxLengthSequence) // if current byte of buffer array and current byte of unpacked array are equal, then
                {
                    if (countSameBytes == 0) // if this is first matching
                    {
                        positionSameBytes = positionBuffer; // write its position
                    }
                    countSameBytes++; // increase counter the same bytes
                    positionSequence++; // increase position in the unpacked array
                    positionBuffer = ++positionBuffer & 0x3FF; // increase position in the buffer array
                }
                else // if current byte of buffer array and current byte of unpacked array are not equal, then
                {
                    if (countSameBytes > minCountSameBytes && countSameBytes <= maxLengthSequence && countSameBytes > foundSequenceCount) // if the number the same bytes more the minimum length of sequence, less or equal to the maximum length of sequence and more the number of bytes of found sequence (if it is), then
                    {
                        result = true; // set result as true
                        foundSequenceCount = countSameBytes; // write found number the same bytes
                        foundSequencePosition = positionSameBytes; // write found position the sequence
                    }
                    positionBuffer = (positionBuffer - countSameBytes >= 0) ? positionBuffer - countSameBytes : 0x400 + (positionBuffer - countSameBytes); // return to position of buffer array minus found number the same bytes
                    positionBuffer = ++positionBuffer & 0x3FF; // increase position in the buffer array
                    positionSequence = positionUnpacked; // return to begin of the unpacked array
                    countSameBytes = 0; // zeroing of number the same bytes
                }
            } while (positionSequence < unpackedArray.Count && positionBuffer != bufferEnd); // the cycle runs while position in the unpacked array less the length of the unpacked array and position in the buffer array doesn't equal to the end position of it
            if (countSameBytes > minCountSameBytes && countSameBytes <= maxLengthSequence && countSameBytes > foundSequenceCount) // check the found sequence (if it is) again, it is necessary if we leave the loop from true condition
            {
                result = true; // set result as true
                foundSequenceCount = countSameBytes; // write found number of the same bytes
                foundSequencePosition = positionSameBytes; // write found position the sequence
            }

            #region Check for RLE // here is checking for RLE

            while (lengthRleSequence <= maxLengthRleSequence) // while current length sequence that we search less or equal to the maximum length of sequence that we can compress by RLE
            {
                positionBuffer = (bufferEnd - lengthRleSequence >= 0) ? bufferEnd - lengthRleSequence : 0x400 + (bufferEnd - lengthRleSequence);
                positionSequence = positionUnpacked;
                countSameBytes = 0;
                foundRLE = true;
                while (positionSequence < unpackedArray.Count)
                {
                    int i = 0;
                    while (i < lengthRleSequence && positionSequence + i < unpackedArray.Count && countSameBytes < maxLengthSequence)
                    {
                        if (bufferArray[(positionBuffer + i) & 0x3FF] == unpackedArray[positionSequence + i])
                        {
                            countSameBytes++;
                        }
                        else
                        {
                            foundRLE = false;
                            break;
                        }
                        i++;
                    }
                    if (countSameBytes != 0)
                    {
                        positionSequence += lengthRleSequence;
                    }
                    if (!foundRLE)
                    {
                        break;
                    }
                }
                if (countSameBytes > minCountSameBytes && countSameBytes > foundSequenceCount)
                {
                    result = true;
                    foundSequenceCount = countSameBytes;
                    foundSequencePosition = positionBuffer;
                }
                lengthRleSequence++;
            }

            #endregion

            return result;
        }

        static private void GetBytes(int position, int count, out byte firstByte, out byte secondByte)
        {
            firstByte = 0;
            secondByte = 0;
            firstByte = (byte)(position & 0xFF);
            secondByte = (byte)(((position >> 8) << 5) | count - 3);
        }

        static private void IncreaseBufferPosition(ref int begin, ref int end)
        {
            if (end - begin > 0)
            {
                begin = ((end + 1) - begin < 0x400) ? begin : ++begin;
                end = ++end & 0x3FF;
            }
            else
            {
                begin = ++begin & 0x3FF;
                end = ++end & 0x3FF;
            }
        }

        private static List<byte> _pack(List<byte> unpackedArray, List<byte> bufferArray)
        {
            List<byte> packedArray = new List<byte>();

            try
            {
                int positionBeginBuffer = 0;
                int positionEndBuffer = 0x3DE;
                int positionUnpacked = 0;
                byte flagByte = 0;
                int positionFlagByte = 0;
                int foundSequencePosition;
                int foundSequenceCount;
                byte firstByte = 0;
                byte secondByte = 0;
                do
                {
                    flagByte = 0;
                    positionFlagByte = packedArray.Count;
                    packedArray.Add(flagByte);



                    for (int i = 0; i < 8; i++)
                    {
                        if (positionUnpacked > 0x750)
                        {
                            int h = 0;
                        }
                        if (FindMaxLengthSequence(bufferArray, positionBeginBuffer, positionEndBuffer, unpackedArray, positionUnpacked, out foundSequencePosition, out foundSequenceCount) == true)
                        {
                            packedArray[positionFlagByte] = (byte)(packedArray[positionFlagByte] | (0x00 << i));
                            GetBytes(foundSequencePosition, foundSequenceCount, out firstByte, out secondByte);
                            packedArray.AddRange(new byte[] { firstByte, secondByte });
                            while (foundSequenceCount > 0)
                            {
                                bufferArray[positionEndBuffer] = unpackedArray[positionUnpacked];
                                IncreaseBufferPosition(ref positionBeginBuffer, ref positionEndBuffer);
                                positionUnpacked++;
                                foundSequenceCount--;
                            }
                        }
                        else
                        {
                            packedArray[positionFlagByte] = (byte)(packedArray[positionFlagByte] | (0x01 << i));
                            packedArray.Add(unpackedArray[positionUnpacked]);
                            bufferArray[positionEndBuffer] = unpackedArray[positionUnpacked];
                            IncreaseBufferPosition(ref positionBeginBuffer, ref positionEndBuffer);
                            positionUnpacked++;
                        }
                        if (positionUnpacked >= unpackedArray.Count)
                        {
                            for (int j = i; j < 8; j++)
                            {
                                packedArray[positionFlagByte] = (byte)(packedArray[positionFlagByte] | (0x01 << i));
                            }
                            break;
                        }
                    }
                } while (positionUnpacked < unpackedArray.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return packedArray;
        }

        public static void Pack(string pathOpen, string pathSave, string pathBuffer, int begin)
        {
            try
            {
                #region Opening buffer file

                List<byte> bufferArray = new List<byte>();
                BinaryReader bufferFile = new BinaryReader(File.Open(pathBuffer, FileMode.Open));
                while (bufferArray.Count < bufferFile.BaseStream.Length)
                {
                    bufferArray.Add(bufferFile.ReadByte());
                }
                bufferFile.Close();

                #endregion

                #region Opening file with unpacking data

                List<byte> unpackedArray = new List<byte>();
                BinaryReader openFile = new BinaryReader(File.Open(pathOpen, FileMode.Open));
                openFile.BaseStream.Seek(begin, SeekOrigin.Begin);
                while (unpackedArray.Count < openFile.BaseStream.Length)
                {
                    unpackedArray.Add(openFile.ReadByte());
                }
                openFile.Close();

                #endregion

                #region Compressing data

                List<byte> packedArray = _pack(unpackedArray, bufferArray);

                #endregion

                #region Saving packed data 

                BinaryWriter saveFile = new BinaryWriter(File.Open(pathSave, FileMode.Create));
                for (int i = 0; i < packedArray.Count; i++)
                {
                    saveFile.Write(packedArray[i]);
                }
                saveFile.Close();

                #endregion

                Console.WriteLine("Compressing was successful. Decompressed data takes {0} bytes, compressed data takes {1} bytes.", unpackedArray.Count, packedArray.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
