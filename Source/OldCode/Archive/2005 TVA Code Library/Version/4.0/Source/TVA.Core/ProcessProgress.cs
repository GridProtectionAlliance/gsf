//*******************************************************************************************************
//  ProcessProgress.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/23/2007 - Pinal C. Patel
//      Generated original version of source code.
//  09/09/2008 - J. Ritchie Carroll
//      Converted to C#.
//  09/26/2008 - J. Ritchie Carroll
//      Added a ProcessProgress.Handler class to allow functions with progress delegate
//      to update progress information using the ProcessProgress class.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>
    /// Represents current process progress for an operation.
    /// </summary>
    /// <remarks>
    /// Used to track total progress of an identified operation.
    /// </remarks>
    /// <typeparam name="TUnit">Unit of progress used (long, double, int, etc.)</typeparam>
    [Serializable()]
    public class ProcessProgress<TUnit> where TUnit : struct
    {
        #region [ Members ]

        // Fields
        private string m_processName;
        private string m_progressMessage;
        private TUnit m_total;
        private TUnit m_complete;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="ProcessProgress{TUnit}"/> class using specified process name.
        /// </summary>
        /// <param name="processName">Name of process for which progress is being monitored.</param>
        public ProcessProgress(string processName)
        {
            m_processName = processName;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="ProcessProgress{TUnit}"/> class using specified parameters.
        /// </summary>
        /// <param name="processName">Name of process for which progress is being monitored.</param>
        /// <param name="processMessage">Current processing message, if any.</param>
        /// <param name="total">Total number of units to be processed.</param>
        /// <param name="complete">Number of units completed processing so far.</param>
        public ProcessProgress(string processName, string processMessage, TUnit total, TUnit complete)
        {
            m_processName = processName;
            m_progressMessage = processMessage;
            m_total = total;
            m_complete = complete;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets name of process for which progress is being monitored.
        /// </summary>
        public string ProcessName
        {
            get
            {
                return m_processName;
            }
            set
            {
                m_processName = value;
            }
        }

        /// <summary>
        /// Gets or sets current progress message (e.g., current file being copied, etc.)
        /// </summary>
        public string ProgressMessage
        {
            get
            {
                return m_progressMessage;
            }
            set
            {
                m_progressMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets total number of units to be processed.
        /// </summary>
        public TUnit Total
        {
            get
            {
                return m_total;
            }
            set
            {
                m_total = value;
            }
        }

        /// <summary>
        /// Gets or sets number of units completed processing so far.
        /// </summary>
        public TUnit Complete
        {
            get
            {
                return m_complete;
            }
            set
            {
                m_complete = value;
            }
        }

        #endregion
    }
}