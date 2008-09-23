//*******************************************************************************************************
//  AsyncResult.cs
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
//  05/22/2003 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections;

namespace TVA.Net.Ftp
{
    public class AsyncResult
    {
        #region [ Members ]

        // Constants
        public const int Complete = 0;
        public const int Fail = 1;
        public const int Abort = 2;

        // Fields
        private BitArray m_result;
        private string m_message;
        private int m_ftpResponse;

        #endregion

        #region [ Constructors ]

        internal AsyncResult()
            : this("Success.", System.Convert.ToInt32(Response.InvalidCode), Complete)
        {
        }

        internal AsyncResult(string message, int result)
            : this(message, System.Convert.ToInt32(Response.InvalidCode), result)
        {
        }

        internal AsyncResult(string message, int ftpCode, int result)
        {
            m_result = new BitArray(3);
            m_message = message;
            m_ftpResponse = ftpCode;
            m_result[result] = true;
        }

        #endregion

        #region [ Properties ]

        public bool IsSuccess
        {
            get
            {
                return m_result[Complete];
            }
        }

        public bool IsFailed
        {
            get
            {
                return m_result[Fail];
            }
        }

        public bool IsAborted
        {
            get
            {
                return m_result[Abort];
            }
        }

        public int ResponseCode
        {
            get
            {
                return m_ftpResponse;
            }
        }

        public string Message
        {
            get
            {
                return m_message;
            }
        }

        #endregion
    }
}