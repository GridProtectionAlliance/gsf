//******************************************************************************************************
//  ListInfoChunk.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/27/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  10/8/2012 - Danyelle Gilliam
//        Modified Header
//
//******************************************************************************************************




#region [ Contributor License Agreements ]

/**************************************************************************\
   Copyright © 2011 - J. Ritchie Carroll
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  
\**************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GSF.Media
{
    /// <summary>
    /// Represents the list info chunk in a WAVE media format file.
    /// </summary>
    public class ListInfoChunk : RiffChunk
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Type ID of a WAVE list chunk.
        /// </summary>
        public const string RiffTypeID = "LIST";

        // Fields
        private Dictionary<string, string> m_infoStrings;

        #endregion

        #region [ Constructors ]

        /// <summary>Reads a new WAVE list info section from the specified stream.</summary>
        /// <param name="preRead">Pre-parsed <see cref="RiffChunk"/> header.</param>
        /// <param name="source">Source stream to read data from.</param>
        /// <exception cref="InvalidOperationException">WAVE list info section is too small, wave file corrupted.</exception>
        public ListInfoChunk(RiffChunk preRead, Stream source)
            : base(preRead, RiffTypeID)
        {
            byte[] buffer = new byte[preRead.ChunkSize];
            byte[] nullByte = new byte[] { 0 };
            int bytesRead = source.Read(buffer, 0, buffer.Length);
            int length, index = 0;
            string key, value;

            m_infoStrings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if (bytesRead >= 4 && string.Compare(Encoding.ASCII.GetString(buffer, index, 4), "INFO", true) == 0)
            {
                index += 4;

                while (index < bytesRead)
                {
                    // Read key
                    length = buffer.IndexOfSequence(nullByte, index) - index + 1;
                    key = Encoding.ASCII.GetString(buffer, index, length - 1).RemoveNull().RemoveControlCharacters();
                    index += length;

                    // Skip through null values
                    while (index < bytesRead && buffer[index] == 0)
                        index++;

                    // Read value
                    length = buffer.IndexOfSequence(nullByte, index) - index + 1;
                    value = Encoding.ASCII.GetString(buffer, index, length - 1).RemoveNull().RemoveControlCharacters();
                    index += length;

                    // Skip through null values
                    while (index < bytesRead && buffer[index] == 0)
                        index++;

                    if (!string.IsNullOrWhiteSpace(key))
                        m_infoStrings[key] = value;
                }
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets list of info strings from list info blocks of this <see cref="ListInfoChunk"/>.
        /// </summary>
        public Dictionary<string, string> InfoStrings
        {
            get
            {
                return m_infoStrings;
            }
        }

        #endregion
    }
}
