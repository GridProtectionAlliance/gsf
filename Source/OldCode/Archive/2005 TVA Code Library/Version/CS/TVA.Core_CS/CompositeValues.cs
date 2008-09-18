//*******************************************************************************************************
//  CompositeValues.cs
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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/29/2005 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Math).
//  08/23/2007 - Darrell Zuercher
//       Edited code comments.
//  09/18/2008 - J. Ritchie Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;

/// <summary>Temporarily caches composite values until all values been received, so that a compound value can be created.</summary>
public class CompositeValues
{
    #region [ Members ]

    // Nested Types
    private struct CompositeValue
    {
        public double Value;
        public bool Received;
    }

    // Fields
    private CompositeValue[] m_compositeValues;
    private bool m_allReceived;

    #endregion

    #region [ Constructors ]

    /// <summary>Creates a new instance of the CompositeValues class, specifing the total number of composite values to track.</summary>
    /// <param name="count">Total number of composite values to track.</param>
    public CompositeValues(int count)
    {
        m_compositeValues = new CompositeValue[count];
    }

    #endregion

    #region [ Properties ]

    /// <summary>Gets or sets the composite value at the specified index in composite value collection.</summary>
    /// <param name="index">The zero-based index of the composite value to get or set.</param>
    /// <returns>The composite value at the specified index in composite value collection.</returns>
    public double this[int index]
    {
        get
        {
            return m_compositeValues[index].Value;
        }
        set
        {
            CompositeValue compositeValue = m_compositeValues[index];
            compositeValue.Value = value;
            compositeValue.Received = true;
        }
    }

    /// <summary>Gets the number of composite values in the composite value collection.</summary>
    /// <returns>To be provided.</returns>
    public int Count
    {
        get
        {
            return m_compositeValues.Length;
        }
    }

    /// <summary>Gets a boolean value indicating if all composite values are received.</summary>
    /// <returns>True, if all composite values are received; otherwise, false.</returns>
    public bool AllReceived
    {
        get
        {
            if (m_allReceived)
            {
                return true;
            }
            else
            {
                bool allValuesReceived = true;

                for (int x = 0; x <= m_compositeValues.Length - 1; x++)
                {
                    if (!m_compositeValues[x].Received)
                    {
                        allValuesReceived = false;
                        break;
                    }
                }

                if (allValuesReceived)
                    m_allReceived = true;

                return allValuesReceived;
            }
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>Gets a boolean value indicating if composite value at the specified index is received.</summary>
    /// <param name="index">The zero-based index of the composite value.</param>
    /// <returns>True, if composite value at the specified index is received; otherwise, false.</returns>
    public bool Received(int index)
    {
        return m_compositeValues[index].Received;
    }

    #endregion
}