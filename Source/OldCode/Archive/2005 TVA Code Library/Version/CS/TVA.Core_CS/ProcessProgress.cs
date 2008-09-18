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
//
//*******************************************************************************************************

using System;

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
