//*******************************************************************************************************
//  TimeStampParser.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/23/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Net.Ftp
{
    internal class TimeStampParser
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

        public TimeStampParser()
        {
            Style = RawDataStyle.Undetermined;
        }

        public TimeStampParser(string RawValue, RawDataStyle Style)
        {
            this.RawValue = RawValue;
            this.Style = Style;
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
