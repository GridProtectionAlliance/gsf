//*******************************************************************************************************
//  AssignedValue.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/07/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.NumericalAnalysis
{
    /// <summary>
    /// Represents a value that tracks if its value has been assigned.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of the value needing assignment tracking.</typeparam>
    public class AssignedValue<T>
    {
        #region [ Members ]

        /// <summary>
        /// This event is raised when either the <see cref="Value"/> or <see cref="Assigned"/> properties are changed.
        /// </summary>
        public event EventHandler Changed;

        // Fields
        private T m_value;          // Actual assigned value
        private bool m_assigned;    // Flag that tracks if value has been assigned

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AssignedValue{T}"/> with a default <see cref="Value"/> and <c><see cref="Assigned"/> = false</c>.
        /// </summary>
        public AssignedValue()
        {
            m_value = default(T);
            m_assigned = false;
        }

        /// <summary>
        /// Creates a new <see cref="AssignedValue{T}"/> with the specified <paramref name="value"/> and <c><see cref="Assigned"/> = true</c>.
        /// </summary>
        /// <param name="value">Actual assigned value.</param>
        public AssignedValue(T value)
        {
            m_value = value;
            m_assigned = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="received"></param>
        public AssignedValue(T value, bool received)
        {
            m_value = value;
            m_assigned = received;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets value of <see cref="AssignedValue{T}"/>.
        /// </summary>
        /// <remarks>
        /// Assigning any value to the <see cref="Value"/> property automatically sets <c><see cref="Assigned"/> = true</c>.
        /// </remarks>
        public T Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
                m_assigned = true;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets assigned state of <see cref="AssignedValue{T}"/>.
        /// </summary>
        public bool Assigned
        {
            get
            {
                return m_assigned;
            }
            set
            {
                m_assigned = value;
                OnChanged();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Raises the <see cref="Changed"/> event.
        /// </summary>
        protected void OnChanged()
        {
            if (Changed != null)
                Changed(this, EventArgs.Empty);
        }

        #endregion
    }
}
