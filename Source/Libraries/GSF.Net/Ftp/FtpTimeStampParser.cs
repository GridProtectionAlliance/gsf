//******************************************************************************************************
//  FtpTimeStampParser.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  09/23/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//*******************************************************************************************************
//
//   Code based on the following project:
//        http://www.codeproject.com/KB/IP/net_ftp_upload.aspx
//  
//   Copyright Alex Kwok & Uwe Keim 
//
//   The Code Project Open License (CPOL):
//        http://www.codeproject.com/info/cpol10.aspx
//
//*******************************************************************************************************

#endregion

using System;

namespace GSF.Net.Ftp
{
    internal class FtpTimeStampParser
    {
        #region [ Members ]

        // Nested Types
        public enum RawDataStyle
        {
            UnixDate,
            UnixDateTime,
            DosDateTime,
            Undetermined
        }

        // Fields
        public string RawValue;
        public RawDataStyle Style;

        #endregion

        #region [ Constructors ]

        public FtpTimeStampParser()
        {
            Style = RawDataStyle.Undetermined;
        }

        public FtpTimeStampParser(string rawValue, RawDataStyle style)
        {
            RawValue = rawValue;
            Style = style;
        }

        #endregion

        #region [ Properties ]

        public DateTime Value
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RawValue))
                    return DateTime.MinValue;

                try
                {
                    if (Style != RawDataStyle.UnixDateTime)
                        return DateTime.Parse(RawValue);

                    string[] sa = RawValue.Split(' ');
                    return Convert.ToDateTime($"{sa[0]} {sa[1]} {DateTime.UtcNow.Year} {sa[2]}");
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }

        #endregion
    }
}
