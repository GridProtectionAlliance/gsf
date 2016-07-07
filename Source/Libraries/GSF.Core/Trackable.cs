//******************************************************************************************************
//  Trackable.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/07/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;

namespace GSF
{
    /// <summary>
    /// Represents the change history for a property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is primarily designed to track changes to the
    /// property of an object, as in the following example.
    /// </para>
    /// 
    /// <code>
    /// public class MyClass
    /// {
    ///     private Trackable&lt;string&gt; m_trackableText = new Trackable&lt;string&gt;(nameof(Text), true);
    ///     
    ///     public string Text
    ///     {
    ///         get
    ///         {
    ///             return m_trackableText.CurrentValue;
    ///         }
    ///         set
    ///         {
    ///             m_trackableText.CurrentValue = value;
    ///         }
    ///     }
    /// }
    /// </code>
    /// </remarks>
    public class Trackable<T> : ITrackable
    {
        #region [ Members ]

        // Fields
        private string m_propertyName;
        private T m_initialValue;
        private List<T> m_changeList;
        private bool m_autoTrack;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Trackable{T}"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property being tracked.</param>
        /// <param name="autoTrack">Determines the behavior of the setter for the <see cref="CurrentValue"/> property. Set to false to track only the original and current values.</param>
        public Trackable(string propertyName, bool autoTrack)
            : this(propertyName, default(T), autoTrack)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Trackable{T}"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property being tracked.</param>
        /// <param name="initialValue">The initial value of the property.</param>
        /// <param name="autoTrack">Determines the behavior of the setter for the <see cref="CurrentValue"/> property. Set to false to track only the original and current values.</param>
        public Trackable(string propertyName, T initialValue = default(T), bool autoTrack = false)
        {
            m_propertyName = propertyName;
            m_initialValue = initialValue;
            m_changeList = new List<T>() { initialValue };
            m_autoTrack = autoTrack;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the name of the property being tracked.
        /// </summary>
        public string PropertyName
        {
            get
            {
                return m_propertyName;
            }
        }

        /// <summary>
        /// Gets or sets the current value after all changes have been applied to the property.
        /// </summary>
        public T CurrentValue
        {
            get
            {
                return ChangeList.Last();
            }
            set
            {
                SetCurrentValue(value, !m_autoTrack);
            }
        }

        /// <summary>
        /// Gets the original value before the first change was made to the property.
        /// </summary>
        public T OriginalValue
        {
            get
            {
                return ChangeList.First();
            }
        }

        /// <summary>
        /// Gets a value to indicate whether the property's value has been
        /// changed since the last time the current value was committed.
        /// </summary>
        public bool IsChanged
        {
            get
            {
                return !Equals(OriginalValue, CurrentValue);
            }
        }

        /// <summary>
        /// Gets the list of changes to the property's value,
        /// starting from its original value and up to the current value.
        /// </summary>
        public List<T> ChangeList
        {
            get
            {
                if (m_changeList.Count == 0)
                    m_changeList.Add(m_initialValue);

                return m_changeList;
            }
        }

        /// <summary>
        /// Gets the original value before the first change was made to the property.
        /// </summary>
        object ITrackable.OriginalValue
        {
            get
            {
                return OriginalValue;
            }
        }

        /// <summary>
        /// Gets the current value after all changes have been applied to the property.
        /// </summary>
        object ITrackable.CurrentValue
        {
            get
            {
                return CurrentValue;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Sets the current value of the property.
        /// </summary>
        /// <param name="value">The new value of the property.</param>
        /// <param name="overwrite">True to track the change; false to overwrite the previous change. Does not overwrite the original value.</param>
        public void SetCurrentValue(T value, bool overwrite)
        {
            if (overwrite && m_changeList.Count > 1)
                m_changeList.RemoveAt(m_changeList.Count - 1);

            if (Equals(CurrentValue, value))
                return;

            m_changeList.Add(value);
        }

        /// <summary>
        /// Erases all history in the change list and commits
        /// the current value as the new original value.
        /// </summary>
        public void AcceptChanges()
        {
            T currentValue = CurrentValue;
            m_changeList.Clear();
            m_changeList.Add(currentValue);
        }

        /// <summary>
        /// Erases all history in the change list and reverts
        /// to the original value before the changes were made.
        /// </summary>
        public void Revert()
        {
            T originalValue = OriginalValue;
            m_changeList.Clear();
            m_changeList.Add(originalValue);
        }

        /// <summary>
        /// Erases all history in the change list and sets
        /// the original value back to its initial value.
        /// </summary>
        public void Reset()
        {
            m_changeList.Clear();
            m_changeList.Add(m_initialValue);
        }

        /// <summary>
        /// Returns a string that represents the current value.
        /// </summary>
        /// <returns>A string that represents the current value.</returns>
        public override string ToString()
        {
            return CurrentValue.ToString();
        }

        #endregion
    }
}
