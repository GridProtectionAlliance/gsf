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
    /// Generic process progress class.
    /// </summary>
    /// <remarks>
    /// Used to track total progress of an identified operation.
    /// </remarks>
    /// <typeparam name="TUnit">Unit of progress used (double, int, etc.)</typeparam>
    [Serializable()]
    public class ProcessProgress<TUnit> where TUnit : struct
    {
        #region [ Members ]

        // Nested Types
        public class Handler
        {
            #region [ Members ]

            // Fields
            Action<ProcessProgress<TUnit>> m_progressHandler;
            ProcessProgress<TUnit> m_progressInstance;

            #endregion

            #region [ Constructors ]

            public Handler(Action<ProcessProgress<TUnit>> progressHandler, string processName)
            {
                m_progressHandler = progressHandler;
                m_progressInstance = new ProcessProgress<TUnit>(processName);
            }

            public Handler(Action<ProcessProgress<TUnit>> progressHandler, string processName, TUnit total)
                : this(progressHandler, processName)
            {
                m_progressInstance.Total = total;
            }

            #endregion

            #region [ Properties ]

            public ProcessProgress<TUnit> ProcessProgress
            {
                get
                {
                    return m_progressInstance;
                }
            }

            public Action<ProcessProgress<TUnit>> ProgressHandler
            {
                get
                {
                    return m_progressHandler;
                }
                set
                {
                    m_progressHandler = value;
                }
            }

            public TUnit Complete
            {
                get
                {
                    return m_progressInstance.Complete;
                }
                set
                {
                    UpdateProgress(value);
                }
            }

            public TUnit Total
            {
                get
                {
                    return m_progressInstance.Total;
                }
                set
                {
                    m_progressInstance.Total = value;
                }
            }

            #endregion

            #region [ Methods ]

            public void UpdateProgress(TUnit completed)
            {
                // Update bytes completed
                m_progressInstance.Complete = completed;

                // Call user function
                m_progressHandler(m_progressInstance);
            }

            #endregion
        }

        // Fields
        private string m_processName;
        private string m_progressMessage;
        private TUnit m_total;
        private TUnit m_complete;

        #endregion

        #region [ Constructors ]

        public ProcessProgress(string processName)
        {
            m_processName = processName;
        }

        #endregion

        #region [ Properties ]

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