//******************************************************************************************************
//  Tag.cs - Gbtc
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
//  12/14/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using GSF;
using GSF.PQDIF.Physical;

namespace PQDIFExplorer
{
    /// <summary>
    /// Represents a tag as defined by the PQDIF standard.
    /// </summary>
    public class Tag
    {
        #region [ Members ]

        // Fields
        private Guid m_id;
        private string m_name;
        private string m_standardName;

        private string m_description;
        private ElementType m_elementType;
        private PhysicalType m_physicalType;
        private bool m_required;

        #endregion

        #region [ Constructors ]

        // Tags are readonly and can only be created by
        // defining them in the TagDefinitions.xml file.
        private Tag()
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the globally unique identifier for the tag.
        /// </summary>
        public Guid ID => m_id;

        /// <summary>
        /// Gets the name of the tag as defined by the GSF.PQDIF library.
        /// </summary>
        public string Name => m_name;

        /// <summary>
        /// Gets the name of the tag as defined by the standard.
        /// </summary>
        public string StandardName => m_standardName;

        /// <summary>
        /// Gets a description of the tag.
        /// </summary>
        public string Description => m_description;

        /// <summary>
        /// Gets the type of the element identified by the tag
        /// according to the logical structure of a PQDIF file.
        /// </summary>
        public ElementType ElementType => m_elementType;

        /// <summary>
        /// Gets the physical type of the value of the element identified
        /// by the tag according to the logical structure of a PQDIF file.
        /// </summary>
        public PhysicalType PhysicalType => m_physicalType;

        public bool Required => m_required;

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Generates a list of tags from the given XML document.
        /// </summary>
        /// <param name="doc">The XML document containing the tag definitions.</param>
        /// <returns>The list of tags as defined by the given XML document.</returns>
        public static List<Tag> GenerateTags(XDocument doc)
        {
            return doc.Descendants("tag")
                .Select(element => new Tag()
                {
                    m_id = Guid.Parse((string)element.Element("id")),
                    m_name = (string)element.Element("name"),
                    m_standardName = (string)element.Element("standardName"),
                    m_description = (string)element.Element("description"),
                    m_elementType = GetElementType(element),
                    m_physicalType = GetPhysicalType(element),
                    m_required = ((string)element.Element("required") ?? "False").ParseBoolean()
                })
                .ToList();
        }

        // Attempts to parse the element type via the ElementType enumeration.
        // Failing that, attempts to parse it as an integer instead.
        private static ElementType GetElementType(XElement element)
        {
            string elementTypeName = (string)element.Element("elementType");
            ElementType elementType;
            byte elementTypeID;

            if (Enum.TryParse(elementTypeName, out elementType))
                return elementType;
            else if (byte.TryParse(elementTypeName, out elementTypeID))
                return (ElementType)elementTypeID;

            return 0;
        }

        // Attempts to parse the physical type via the PhysicalType enumeration.
        // Failing that, attempts to parse it as an integer instead.
        private static PhysicalType GetPhysicalType(XElement element)
        {
            string physicalTypeName = (string)element.Element("physicalType");
            PhysicalType physicalType;
            byte physicalTypeID;

            if (Enum.TryParse(physicalTypeName, out physicalType))
                return physicalType;
            else if (byte.TryParse(physicalTypeName, out physicalTypeID))
                return (PhysicalType)physicalTypeID;

            return 0;
        }

        #endregion
    }
}
