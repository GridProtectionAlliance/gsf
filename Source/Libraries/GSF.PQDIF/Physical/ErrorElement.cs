//******************************************************************************************************
//  ErrorElement.cs - Gbtc
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
//  06/21/2016 - Stephen Jenks
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.PQDIF.Physical
{
    /// <summary>
    /// Represents an <see cref="Element"/> that the parser
    /// ran into an exception while trying to parse.
    /// </summary>
    public class ErrorElement : Element
    {
        #region [ Members ]

        private ElementType m_typeOfElement;
        private Exception m_exception;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ErrorElement" /> class.
        /// </summary>
        /// <param name="exception">The exception that occurred when parsing an element.</param>
        public ErrorElement(Exception exception)
            : this(0, exception)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ErrorElement" /> class.
        /// </summary>
        /// <param name="typeOfElement">The type of the element being parsed when the error occurred.</param>
        /// <param name="exception">The exception that occurred when parsing an element.</param>
        public ErrorElement(ElementType typeOfElement, Exception exception)
        {
            m_typeOfElement = typeOfElement;
            m_exception = exception;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the type of <see cref="Element"/> it is.
        /// </summary>
        public override ElementType TypeOfElement
        {
            get
            {
                return m_typeOfElement;
            }
        }

        /// <summary>
        /// Gets the exception that the <see cref="PhysicalParser"/> ran into
        /// while trying to parse the <see cref="Element"/>
        /// </summary>
        public Exception Exception
        {
            get
            {
                return m_exception;
            }
        }

        #endregion
    }
}
