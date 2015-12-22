//******************************************************************************************************
//  Identifier.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  12/18/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GSF.PQDIF
{
    /// <summary>
    /// Represents an identifier as defined by the PQDIF standard.
    /// </summary>
    public class Identifier
    {
        #region [ Members ]

        // Fields
        private string m_name;
        private string m_standardName;
        private string m_value;
        private string m_description;

        #endregion

        #region [ Constructors ]

        // Identifiers are readonly and can only be created by
        // defining them in the TagDefinitions.xml file.
        private Identifier()
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the name of the identifier as defined by the GSF.PQDIF library.
        /// </summary>
        public string Name => m_name;

        /// <summary>
        /// Gets the name of the identifier as defined by the standard.
        /// </summary>
        public string StandardName => m_standardName;

        /// <summary>
        /// Gets a string representation of the value of this identifier.
        /// </summary>
        public string Value => m_value;

        /// <summary>
        /// Gets a description of the identifier.
        /// </summary>
        public string Description => m_description;

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Generates a list of identifiers valid for the given tag from the given XML document.
        /// </summary>
        /// <param name="doc">The XML document containing tag definitions.</param>
        /// <param name="tag">The tag for which valid identifier values are to be generated.</param>
        /// <returns>The list of identifiers valid for the given tag.</returns>
        public static List<Identifier> GenerateIdentifiers(XDocument doc, Tag tag)
        {
            XElement tagValues = doc.Descendants("tagValues").Single();
            XElement tagElement = tagValues.Element(tag.StandardName);

            if ((object)tagElement == null)
                return new List<Identifier>();

            return tagElement
                .Elements()
                .Select(element => new Identifier()
                {
                    m_name = (string)element.Element("name"),
                    m_standardName = (string)element.Element("standardName"),
                    m_value = (string)element.Element("value"),
                    m_description = (string)element.Element("description")
                })
                .ToList();
        }

        #endregion

    }
}
