//******************************************************************************************************
//  CollectionElement.cs - Gbtc
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
//  05/02/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  06/09/2016 - Stephen Jenks, Grid Protection Alliance
//       Added readSize member
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSF.PQDIF.Physical
{
    /// <summary>
    /// Represents an <see cref="Element"/> which is a collection of other
    /// elements. Collection elements are part of the physical structure of
    /// a PQDIF file. They exist within the body of a <see cref="Record"/>.
    /// </summary>
    public class CollectionElement : Element
    {
        #region [ Members ]

        // Fields
        private readonly IList<Element> m_elements;
        private int m_readSize; // In case the actual size differs from the read size.

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CollectionElement"/> class.
        /// </summary>
        public CollectionElement()
        {
            m_elements = new List<Element>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of elements in the collection.
        /// </summary>
        public int Size
        {
            get
            {
                return m_elements.Count;
            }
        }

        /// <summary>
        /// Gets or sets the size that the file says the collection is.
        /// This may differ from the actual size if, upon parsing the 
        /// file the end of file is reached before the collection becomes
        /// as large as the read size.
        /// </summary>
        public int ReadSize
        {
            get
            {
                return m_readSize;
            }
            set
            {
                m_readSize = value;
            }
        }

        /// <summary>
        /// Gets the type of the element.
        /// Returns <see cref="ElementType.Collection"/>.
        /// </summary>
        public override ElementType TypeOfElement
        {
            get
            {
                return ElementType.Collection;
            }
        }

        /// <summary>
        /// Gets a list of the element in this collection.
        /// </summary>
        public IList<Element> Elements
        {
            get
            {
                return m_elements;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds the given element to the collection.
        /// </summary>
        /// <param name="element">The element to be added.</param>
        public void AddElement(Element element)
        {
            m_elements.Add(element);
        }

        /// <summary>
        /// Removes the given element from the collection.
        /// </summary>
        /// <param name="element">The element to be removed.</param>
        public void RemoveElement(Element element)
        {
            m_elements.Remove(element);
        }

        /// <summary>
        /// Removes all elements identified by the given tag from the collection.
        /// </summary>
        /// <param name="tag">The tag of the elements to be removed.</param>
        public void RemoveElementsByTag(Guid tag)
        {
            for (int i = m_elements.Count - 1; i >= 0; i--)
            {
                if (m_elements[i].TagOfElement == tag)
                    m_elements.RemoveAt(i);
            }
        }

        /// <summary>
        /// Gets the elements whose tag matches the one given as a parameter.
        /// </summary>
        /// <param name="tag">The tag of the elements to be retrieved.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Element"/>s
        /// identified by the given <paramref name="tag"/>.
        /// </returns>
        public IEnumerable<Element> GetElementsByTag(Guid tag)
        {
            return m_elements.Where(element => element.TagOfElement == tag);
        }

        /// <summary>
        /// Gets the element whose tag matches the one given as a
        /// parameter, type cast to <see cref="CollectionElement"/>.
        /// </summary>
        /// <param name="tag">The tag to search by.</param>
        /// <returns>The element whose tag matches the one given, or null if no matching collection element exists.</returns>
        public CollectionElement GetCollectionByTag(Guid tag)
        {
            return m_elements.SingleOrDefault(element => element.TagOfElement == tag) as CollectionElement;
        }

        /// <summary>
        /// Gets the element whose tag matches the one given as a
        /// parameter, type cast to <see cref="ScalarElement"/>.
        /// </summary>
        /// <param name="tag">The tag to search by.</param>
        /// <returns>The element whose tag matches the one given, or null if no matching scalar element exists.</returns>
        public ScalarElement GetScalarByTag(Guid tag)
        {
            return m_elements.SingleOrDefault(element => element.TagOfElement == tag) as ScalarElement;
        }

        /// <summary>
        /// Gets the element whose tag matches the one given as a
        /// parameter, type cast to <see cref="VectorElement"/>.
        /// </summary>
        /// <param name="tag">The tag to search by.</param>
        /// <returns>The element whose tag matches the one given, or null if no matching vector element exists.</returns>
        public VectorElement GetVectorByTag(Guid tag)
        {
            return m_elements.SingleOrDefault(element => element.TagOfElement == tag) as VectorElement;
        }

        /// <summary>
        /// Returns a string that represents the collection.
        /// </summary>
        /// <returns>A string that represents the collection.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            string[] lines;

            builder.AppendFormat("Collection -- Size: {0}, Tag: {1}", Size, TagOfElement);

            foreach (Element element in m_elements)
            {
                lines = element.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                foreach (string line in lines)
                {
                    builder.AppendLine();
                    builder.AppendFormat("    {0}", line);
                }
            }

            return builder.ToString();
        }

        #endregion
    }
}