//******************************************************************************************************
//  TransferOptionSet.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/12/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//*******************************************************************************************************
//
//   Code based on the following project:
//        https://github.com/Callisto82/tftp.net
//  
//   Copyright © 2011, Michael Baer
//
//*******************************************************************************************************

#endregion

using System.Collections.Generic;
using GSF.Net.TFtp.Commands;

namespace GSF.Net.TFtp.Transfer
{
    internal class TransferOptionSet
    {
        public const int DefaultBlocksize = 512;
        public const int DefaultTimeoutSecs = 5;

        public bool IncludesBlockSizeOption;
        public int BlockSize = DefaultBlocksize;

        public bool IncludesTimeoutOption;
        public int Timeout = DefaultTimeoutSecs;

        public bool IncludesTransferSizeOption;
        public long TransferSize;

        public static TransferOptionSet NewDefaultSet()
        {
            return new TransferOptionSet
            {
                IncludesBlockSizeOption = true,
                IncludesTimeoutOption = true,
                IncludesTransferSizeOption = true
            };
        }

        public static TransferOptionSet NewEmptySet()
        {
            return new TransferOptionSet 
            {
                IncludesBlockSizeOption = false,
                IncludesTimeoutOption = false,
                IncludesTransferSizeOption = false
            };
        }

        private TransferOptionSet()
        {
        }

        public TransferOptionSet(IEnumerable<TransferOption> options)
        {
            IncludesBlockSizeOption = IncludesTimeoutOption = IncludesTransferSizeOption = false;

            foreach (TransferOption option in options)
                Parse(option);
        }

        private void Parse(TransferOption option)
        {
            switch (option.Name)
            {
                case "blksize":
                    IncludesBlockSizeOption = ParseBlockSizeOption(option.Value);
                    break;

                case "timeout":
                    IncludesTimeoutOption = ParseTimeoutOption(option.Value);
                    break;

                case "tsize":
                    IncludesTransferSizeOption = ParseTransferSizeOption(option.Value);
                    break;
            }
        }

        public List<TransferOption> ToOptionList()
        {
            List<TransferOption> result = new List<TransferOption>();

            if (IncludesBlockSizeOption)
                result.Add(new TransferOption("blksize", BlockSize.ToString()));

            if (IncludesTimeoutOption)
                result.Add(new TransferOption("timeout", Timeout.ToString()));

            if (IncludesTransferSizeOption)
                result.Add(new TransferOption("tsize", TransferSize.ToString()));

            return result;
        }

        private bool ParseTransferSizeOption(string value)
        {
            return long.TryParse(value, out TransferSize) && TransferSize >= 0;
        }

        private bool ParseTimeoutOption(string value)
        {
            if (!int.TryParse(value, out int timeout))
                return false;

            // Only accept timeouts in the range [1, 255]
            if (timeout < 1 || timeout > 255)
                return false;

            Timeout = timeout;
            return true;
        }

        private bool ParseBlockSizeOption(string value)
        {
            if (!int.TryParse(value, out int blockSize))
                return false;

            // Only accept block sizes in the range [8, 65464]
            if (blockSize < 8 || blockSize > 65464)
                return false;

            BlockSize = blockSize;
            return true;
        }
    }
}
