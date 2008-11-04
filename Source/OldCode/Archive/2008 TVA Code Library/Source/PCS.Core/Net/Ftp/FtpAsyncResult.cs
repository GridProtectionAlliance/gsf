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

using System.Collections;

namespace PCS.Net.Ftp
{
    /// <summary>
    /// Asynchronous transfer result.
    /// </summary>
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
            : this("Success.", Response.InvalidCode, Complete)
        {
        }

        internal AsyncResult(string message, int result)
            : this(message, Response.InvalidCode, result)
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

        /// <summary>
        /// Returns true if asynchronous transfer completed successfully.
        /// </summary>
        public bool IsSuccess
        {
            get
            {
                return m_result[Complete];
            }
        }

        /// <summary>
        /// Returns true if asynchronous transfer failed.
        /// </summary>
        public bool IsFailed
        {
            get
            {
                return m_result[Fail];
            }
        }

        /// <summary>
        /// Returns true if asynchronous transfer was aborted.
        /// </summary>
        public bool IsAborted
        {
            get
            {
                return m_result[Abort];
            }
        }

        /// <summary>
        /// Gets response code from asynchronous transfer.
        /// </summary>
        public int ResponseCode
        {
            get
            {
                return m_ftpResponse;
            }
        }

        /// <summary>
        /// Gets any message associated with asynchronous transfer.
        /// </summary>
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