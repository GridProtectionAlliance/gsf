//******************************************************************************************************
//  Element.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/30/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.PQDIF.Physical
{
    #region [ Enumerations ]

    /// <summary>
    /// Enumeration that defines the types of
    /// elements found in the body of a record.
    /// </summary>
    public enum ElementType : byte
    {
        /// <summary>
        /// Collection element.
        /// Represents a collection of elements.
        /// </summary>
        Collection = 1,

        /// <summary>
        /// Scalar element.
        /// Represents a single value.
        /// </summary>
        Scalar = 2,

        /// <summary>
        /// Vector element.
        /// Represents a collection of values.
        /// </summary>
        Vector = 3
    }

    #endregion

    /// <summary>
    /// Base class for elements. Elements are part of the physical structure
    /// of a PQDIF file. They exist within the body of a <see cref="Record"/>.
    /// </summary>
    public abstract class Element
    {
        #region [ Members ]

        // Fields
        private Guid m_tagOfElement;
        private PhysicalType m_typeOfValue;
        //private bool m_isError = false;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the tag which identifies the element.
        /// </summary>
        public virtual Guid TagOfElement
        {
            get
            {
                return m_tagOfElement;
            }
            set
            {
                m_tagOfElement = value;
            }
        }

        /// <summary>
        /// Gets the type of the element. The element can be a
        /// <see cref="ScalarElement"/>, a <see cref="VectorElement"/>,
        /// or a <see cref="CollectionElement"/>.
        /// </summary>
        public abstract ElementType TypeOfElement { get; }

        /// <summary>
        /// Gets or sets the physical type of the value or values contained
        /// by the element.
        /// </summary>
        /// <remarks>
        /// This determines the data type and size of the
        /// value or values. The value of this property is only relevant when
        /// <see cref="TypeOfElement"/> is either <see cref="ElementType.Scalar"/>
        /// or <see cref="ElementType.Vector"/>.
        /// </remarks>
        public virtual PhysicalType TypeOfValue
        {
            get
            {
                return m_typeOfValue;
            }
            set
            {
                m_typeOfValue = value;
            }
        }

        #endregion
    }
}