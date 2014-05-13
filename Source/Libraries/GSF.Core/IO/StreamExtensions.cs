//******************************************************************************************************
//  StreamExtensions.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/19/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  10/24/2008 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/23/2011 - J. Ritchie Carroll
//       Modified copy stream to use buffer pool.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;

namespace GSF.IO
{
    /// <summary>
    /// Defines extension functions related to <see cref="Stream"/> manipulation.
    /// </summary>
    public static class StreamExtensions
    {
        private const int BufferSize = 32768;

        /// <summary>
        /// Copies input <see cref="Stream"/> onto output <see cref="Stream"/>.
        /// </summary>
        /// <param name="source">The input <see cref="Stream"/>.</param>
        /// <param name="destination">The output <see cref="Stream"/>.</param>
        [Obsolete("Since .NET 4.0, any class inheriting from Stream has a native \"CopyTo\" function; also, .NET 4.5 has a \"CopyToAsync\" method.")]
        public static void CopyStream(this Stream source, Stream destination)
        {
            byte[] buffer = BufferPool.TakeBuffer(BufferSize);

            try
            {
                int bytesRead = source.Read(buffer, 0, BufferSize);

                while (bytesRead > 0)
                {
                    destination.Write(buffer, 0, bytesRead);
                    bytesRead = source.Read(buffer, 0, BufferSize);
                }
            }
            finally
            {
                if ((object)buffer != null)
                    BufferPool.ReturnBuffer(buffer);
            }
        }

        /// <summary>
        /// Reads entire <see cref="Stream"/> contents, and returns <see cref="byte"/> array of data.
        /// </summary>
        /// <param name="source">The <see cref="Stream"/> to be converted to <see cref="byte"/> array.</param>
        /// <returns>An array of <see cref="byte"/>.</returns>
        public static byte[] ReadStream(this Stream source)
        {
            using (BlockAllocatedMemoryStream outStream = new BlockAllocatedMemoryStream())
            {
                source.CopyTo(outStream);
                return outStream.ToArray();
            }
        }
    }
}
