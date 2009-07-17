//*******************************************************************************************************
//  FtpTimeStampParser.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/23/2008 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Net.Ftp
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
            this.RawValue = rawValue;
            this.Style = style;
        }

        #endregion

        #region [ Properties ]

        public DateTime Value
        {
            get
            {
                if (RawValue.Length > 0)
                {
                    try
                    {
                        switch (Style)
                        {
                            case RawDataStyle.UnixDate:
                                return DateTime.Parse(RawValue);
                            case RawDataStyle.UnixDateTime:
                                string[] sa = RawValue.Split(' ');
                                return Convert.ToDateTime(sa[0] + " " + sa[1] + " " + DateTime.Now.Year + " " + sa[2]);
                            case RawDataStyle.DosDateTime:
                                return DateTime.Parse(RawValue);
                            default:
                                return DateTime.Parse(RawValue);
                        }
                    }
                    catch
                    {
                        return DateTime.MinValue;
                    }
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        #endregion
    }
}
