//******************************************************************************************************
//  ByteBuffer.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/02/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.TimeSeries.Transport.TSSC
{
    internal class ByteBuffer
    {
        public byte[] Data;
        public int Position;

        public ByteBuffer(int size)
        {
            Data = new byte[size];
        }

        public ByteBuffer(byte[] data, int position)
        {
            Data = data;
            Position = position;
        }

        public void Grow()
        {
            var data = new byte[Data.Length * 2];
            Data.CopyTo(data, 0);
            Data = data;
        }
    }
}
