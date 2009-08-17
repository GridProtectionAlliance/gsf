//*******************************************************************************************************
//  AdapterCommandAttribute.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO PCS, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/17/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents an attribute that allows a method in an <see cref="IAdapter"/> class to be exposed as
    /// an invokable command.
    /// </summary>
    /// <remarks>
    /// Only public methods will be exposed as invokable.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AdapterCommandAttribute : Attribute
    {
        #region [ Members ]

        // Fields
        private string m_description;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AdapterCommandAttribute"/> with the specified <paramref name="description"/> value.
        /// </summary>
        /// <param name="description">Assigns the description for this adapter command.</param>
        public AdapterCommandAttribute(string description)
        {
            m_description = description;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the description of this adapter command.
        /// </summary>
        public string Description
        {
            get
            {
                return m_description;
            }
        }

        #endregion
    }
}