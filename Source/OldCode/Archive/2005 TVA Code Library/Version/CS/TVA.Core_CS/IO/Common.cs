using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
//using TVA.Common;

//*******************************************************************************************************
//  TVA.IO.Common.vb - Common IO Related Functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/24/2006 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Common).
//  03/06/2007 - J. Ritchie Carroll
//       Added "CompareBuffers" method to compare to binary buffers.
//  04/15/2008 - Pinal C. Patel
//       Changed the return type for "CompareBuffers" from Boolean to Integer.
//  08/22/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************


namespace TVA
{
    namespace IO
    {

        /// <summary>Defines common IO related functions (e.g., common stream and buffer functions).</summary>
        public sealed class Common
        {


            private const int BufferSize = 32768;

            private Common()
            {

                // This class contains only global functions and is not meant to be instantiated.

            }

            /// <summary>Copies input stream onto output stream.</summary>
            public static void CopyStream(System.IO.Stream inStream, System.IO.Stream outStream)
            {

                byte[] buffer = new byte[BufferSize];
                int bytesRead = inStream.Read(buffer, 0, BufferSize);

                while (bytesRead > 0)
                {
                    outStream.Write(buffer, 0, bytesRead);
                    bytesRead = inStream.Read(buffer, 0, BufferSize);
                }

            }

            /// <summary>Reads entire stream contents, and returns byte array of data.</summary>
            /// <remarks>Note: You should only use this on streams where you know the data size is small.</remarks>
            public static byte[] ReadStream(System.IO.Stream inStream)
            {

                System.IO.MemoryStream outStream = new System.IO.MemoryStream();

                CopyStream(inStream, outStream);

                return outStream.ToArray();

            }

            /// <summary>Returns a copy of the specified portion of the source buffer.</summary>
            /// <remarks>Grows or shrinks returned buffer, as needed, to make it the desired length.</remarks>
            public static byte[] CopyBuffer(byte[] buffer, int startIndex, int length)
            {

                byte[] copiedBytes = TVA.Common.CreateArray<byte>(TVA.Common.IIf(buffer.Length - startIndex < length, buffer.Length - startIndex, length));

                System.Buffer.BlockCopy(buffer, startIndex, copiedBytes, 0, copiedBytes.Length);

                return copiedBytes;

            }

            /// <summary>Returns comparision results of two binary buffers.</summary>
            public static int CompareBuffers(byte[] buffer1, byte[] buffer2)
            {

                if (buffer1 == null && buffer2 == null)
                {
                    // Both buffers are assumed equal if both are nothing.
                    return 0;
                }
                else if (buffer1 == null)
                {
                    // Buffer 2 has data, and buffer 1 is nothing. Buffer 2 is assumed larger.
                    return 1;
                }
                else if (buffer2 == null)
                {
                    // Buffer 1 has data, and buffer 2 is nothing. Buffer 1 is assumed larger.
                    return -1;
                }
                else
                {
                    // Code replicated here as an optimization, instead of calling overloaded CompareBuffers
                    // to prevent duplicate "Is Nothing" checks for empty buffers. This function needs to
                    // execute as quickly as possible given possible intended uses.
                    int length1 = buffer1.Length;
                    int length2 = buffer2.Length;

                    if (length1 == length2)
                    {
                        int comparision;

                        // Compares elements of buffers that are of equal size.
                        for (int x = 0; x <= length1 - 1; x++)
                        {
                            comparision = buffer1[x].CompareTo(buffer2[x]);
                            if (comparision != 0)
                            {
                                break;
                            }
                        }

                        return comparision;
                    }
                    else
                    {
                        // Buffer lengths are unequal. Buffer with largest number of elements is assumed to be largest.
                        return length1.CompareTo(length2);
                    }
                }

            }

            public static int CompareBuffers(byte[] buffer1, int offset1, int length1, byte[] buffer2, int offset2, int length2)
            {

                if (buffer1 == null && buffer2 == null)
                {
                    // Both buffers are assumed equal if both are nothing.
                    return 0;
                }
                else if (buffer1 == null)
                {
                    // Buffer 2 has data, and buffer 1 is nothing. Buffer 2 is assumed larger.
                    return 1;
                }
                else if (buffer2 == null)
                {
                    // Buffer 1 has data, and buffer 2 is nothing. Buffer 1 is assumed larger.
                    return -1;
                }
                else
                {
                    if (length1 == length2)
                    {
                        int comparision;

                        // Compares elements of buffers that are of equal size.
                        for (int x = 0; x <= length1 - 1; x++)
                        {
                            comparision = buffer1[offset1 + x].CompareTo(buffer2[offset2 + x]);
                            if (comparision != 0)
                            {
                                break;
                            }
                        }

                        return comparision;
                    }
                    else
                    {
                        // Buffer lengths are unequal. Buffer with largest number of elements is assumed to be largest.
                        return length1.CompareTo(length2);
                    }
                }

            }

        }

    }

}
