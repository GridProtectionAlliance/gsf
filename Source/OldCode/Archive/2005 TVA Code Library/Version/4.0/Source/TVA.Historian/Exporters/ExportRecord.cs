//*******************************************************************************************************
//  ExportRecord.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/12/2007 - Pinal C. Patel
//       Original version of source code generated.
//  04/17/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA.Historian.Exporters
{
    /// <summary>
    /// A class that can be used to define the time series data to be exported for an <see cref="Export"/>.
    /// </summary>
    /// <seealso cref="Export"/>
    [Serializable()]
    public class ExportRecord : IComparable
    {
        #region [ Members ]

        // Fields
        private string m_instance;
        private int m_identifier;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportRecord"/> class.
        /// </summary>
        public ExportRecord()
            : this("Default", -1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportRecord"/> class.
        /// </summary>
        /// <param name="instance">Name of the historian instance providing the time series data.</param>
        /// <param name="identifier">Historian identifier of the <paramref name="instance"/> whose time series data is to be exported.</param>
        public ExportRecord(string instance, int identifier)
        {
            this.Instance = instance;
            this.Identifier = identifier;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the historian instance providing the time series data.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string Instance
        {
            get
            {
                return m_instance;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_instance = value;
            }
        }

        /// <summary>
        /// Gets or sets the historian identifier of the <paramref name="Instance"/> whose time series data is to be exported.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not positive or -1.</exception>
        public int Identifier
        {
            get
            {
                return m_identifier;
            }
            set
            {
                if (value < 1 && value != -1)
                    throw new ArgumentException("Value must be positive or -1.");

                m_identifier = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Compares the current <see cref="ExportRecord"/> object to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Object against which the current <see cref="ExportRecord"/> object is to be compared.</param>
        /// <returns>
        /// Negative value if the current <see cref="ExportRecord"/> object is less than <paramref name="obj"/>, 
        /// Zero if the current <see cref="ExportRecord"/> object is equal to <paramref name="obj"/>, 
        /// Positive value if the current <see cref="ExportRecord"/> object is greater than <paramref name="obj"/>.
        /// </returns>
        public virtual int CompareTo(object obj)
        {
            ExportRecord other = obj as ExportRecord;
            if (other == null)
            {
                return 1;
            }
            else
            {
                int result = string.Compare(m_instance, other.Instance, true);
                if (result != 0)
                    return result;
                else
                    return m_identifier.CompareTo(other.Identifier);
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="ExportRecord"/> object is equal to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Object against which the current <see cref="ExportRecord"/> object is to be compared for equality.</param>
        /// <returns>true if the current <see cref="ExportRecord"/> object is equal to <paramref name="obj"/>; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ExportRecord"/> object.
        /// </summary>
        /// <returns>A 32-bit signed integer value.</returns>
        public override int GetHashCode()
        {
            return m_instance.GetHashCode();
        }

        #endregion
    }
}