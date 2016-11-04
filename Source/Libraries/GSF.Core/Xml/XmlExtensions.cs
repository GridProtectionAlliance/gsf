//******************************************************************************************************
//  XmlExtensions.cs - Gbtc
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
//  02/23/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  01/23/2006 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (GSF.Shared.Common).
//  01/24/2007 - J. Ritchie Carroll
//       Added GetDataSet method to convert an XML string of data into a DataSet object.
//  12/13/2007 - Darrell Zuercher
//       Edited code comments.
//  09/12/2008 - J. Ritchie Carroll
//       Converted to C# extension functions.
//  08/10/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//******************************************************************************************************

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace GSF.Xml
{
    /// <summary>
    /// Defines extension functions related to Xml elements.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>Gets an XML node from given path, creating the entire path if it does not exist.</summary>
        /// <remarks>This overload just allows the start of the given XML document by using its root element.</remarks>
        /// <param name="xmlDoc">An <see cref="XmlDocument"/> to query.</param>
        /// <param name="xpath">A <see cref="System.String"/> xpath query.</param>
        /// <returns>An <see cref="XmlNode"/> corresponding to the xpath query.</returns>
        public static XmlNode GetXmlNode(this XmlDocument xmlDoc, string xpath)
        {
            bool isDirty;
            return xmlDoc.DocumentElement.GetXmlNode(xpath, out isDirty);
        }

        /// <summary>Gets an XML node from given path, creating the entire path if it does not exist.</summary>
        /// <remarks>
        /// <para>This overload just allows the start of the given XML document by using its root element.</para>
        /// <para>Note that the <paramref name="isDirty" /> parameter will be set to True if any items were added to
        /// the tree.</para>
        /// </remarks>
        /// <param name="xmlDoc">An <see cref="XmlDocument"/> to query.</param>
        /// <param name="xpath">A <see cref="System.String"/> xpath query.</param>
        /// <param name="isDirty">A <see cref="System.Boolean"/> value indicating if items were added to the tree.</param>
        /// <returns>An <see cref="XmlNode"/> corresponding to the xpath query.</returns>
        public static XmlNode GetXmlNode(this XmlDocument xmlDoc, string xpath, out bool isDirty)
        {
            return xmlDoc.DocumentElement.GetXmlNode(xpath, out isDirty);
        }

        /// <summary>Gets an XML node from given path, creating the entire path if it does not exist.</summary>
        /// <param name="parentNode">An <see cref="XmlNode"/> parent node to query.</param>
        /// <param name="xpath">A <see cref="System.String"/> xpath query.</param>
        /// <returns>An <see cref="XmlNode"/> corresponding to the xpath query.</returns>
        public static XmlNode GetXmlNode(this XmlNode parentNode, string xpath)
        {
            bool isDirty;
            return parentNode.GetXmlNode(xpath, out isDirty);
        }

        /// <summary>Gets an XML node from given path, creating the entire path if it does not exist.</summary>
        /// <remarks>Note that the <paramref name="isDirty" /> parameter will be set to True if any items were added to
        /// the tree.</remarks>
        /// <param name="parentNode">An <see cref="XmlNode"/> parent node to query.</param>
        /// <param name="xpath">A <see cref="System.String"/> xpath query.</param>
        /// <param name="isDirty">A <see cref="System.Boolean"/> value indicating if items were added to the tree.</param>
        /// <returns>An <see cref="XmlNode"/> corresponding to the xpath query.</returns>
        public static XmlNode GetXmlNode(this XmlNode parentNode, string xpath, out bool isDirty)
        {
            XmlNode node = null;
            string[] elements;

            isDirty = false;

            // Removes any slash prefixes.
            while (xpath[0] == '/')
            {
                xpath = xpath.Substring(1);
            }

            elements = xpath.Split('/');

            if (elements.Length == 1)
            {
                XmlNodeList nodes = parentNode.SelectNodes(xpath);

                if ((object)nodes != null)
                {
                    if (nodes.Count == 0)
                    {
                        if ((object)parentNode.OwnerDocument != null)
                        {
                            node = parentNode.OwnerDocument.CreateElement(xpath);
                            parentNode.AppendChild(node);
                            isDirty = true;
                        }
                    }
                    else
                    {
                        node = nodes[0];
                    }
                }
            }
            else
            {
                foreach (string element in elements)
                {
                    node = GetXmlNode(parentNode, element);
                    parentNode = node;
                }
            }

            return node;
        }

        /// <summary>Safely gets or sets an XML node's attribute.</summary>
        /// <remarks>If you get an attribute that does not exist, null will be returned.</remarks>
        /// <param name="name">A <see cref="System.String"/> name of the value to get.</param>
        /// <param name="node">A <see cref="XmlNode"/> to query.</param>
        /// <returns>A <see cref="System.String"/> value returned for the attribute's value.</returns>
        public static string GetAttributeValue(this XmlNode node, string name)
        {
            if ((object)node == null || (object)node.Attributes == null)
                return null;

            XmlAttribute attribute = node.Attributes[name];

            if ((object)attribute == null)
                return null;

            return attribute.Value;
        }

        /// <summary>Safely sets an XML node's attribute.</summary>
        /// <remarks>If you assign a value to an attribute that does not exist, the attribute will be created.</remarks>
        /// <param name="name">A <see cref="System.String"/> indicating the node name to use.</param>
        /// <param name="node">An <see cref="XmlNode"/> node to operate on.</param>
        /// <param name="value">A <see cref="System.String"/> value to set the node attribute's value to.</param>
        public static void SetAttributeValue(this XmlNode node, string name, string value)
        {
            if ((object)node == null || (object)node.Attributes == null)
                return;

            XmlAttribute attribute = node.Attributes[name];

            if ((object)attribute == null)
            {
                // Add the attribute.
                if ((object)node.OwnerDocument != null)
                {
                    attribute = node.OwnerDocument.CreateAttribute(name);
                    attribute.Value = value;
                    node.Attributes.Append(attribute);
                }
            }
            else
            {
                // Set the attribute value.
                attribute.Value = value;
                node.Attributes.SetNamedItem(attribute);
            }
        }

        /// <summary>
        /// Gets a data set object from an XML data set formatted as a String.
        /// </summary>
        /// <param name="xmlData">XML data string in standard DataSet format.</param>
        /// <returns>A <see cref="DataSet"/> object.</returns>
        public static DataSet GetDataSet(this string xmlData)
        {
            DataSet dataSet = new DataSet();

            using (XmlTextReader xmlReader = new XmlTextReader(xmlData, XmlNodeType.Document, null))
            {
                xmlReader.ReadOuterXml();

                // Reads the outer XML into the Dataset.
                dataSet.ReadXml(xmlReader);
            }

            return dataSet;
        }

        /// <summary>
        /// Transforms an XML document using the given XSL template.
        /// </summary>
        /// <param name="document">The document to be transformed.</param>
        /// <param name="transform">The template that defines how the data should be transformed.</param>
        /// <returns>The result of the transformation.</returns>
        public static string ApplyXSLTransform(this string document, string transform)
        {
            using (StringReader documentReader = new StringReader(document))
            using (StringReader transformReader = new StringReader(transform))
            using (XmlReader xmlDocumentReader = XmlReader.Create(documentReader))
            using (XmlReader xmlTransformReader = XmlReader.Create(transformReader))
            using (StringWriter resultWriter = new StringWriter())
            {
                XslCompiledTransform compiler = new XslCompiledTransform();
                compiler.Load(xmlTransformReader);
                compiler.Transform(xmlDocumentReader, null, resultWriter);
                return resultWriter.ToString();
            }
        }

        /// <summary>
        /// Transforms all elements in the given document matching the given
        /// <paramref name="name"/> using the given <paramref name="selector"/>
        /// function to perform the transformation.
        /// </summary>
        /// <param name="document">The document to be modified.</param>
        /// <param name="name">The name of the elements to be transformed.</param>
        /// <param name="selector">The function that defines the transformation.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is a targeted extension for an XML document")]
        public static void TransformAll(this XDocument document, XName name, Func<XElement, object> selector)
        {
            document
                .Descendants(name)
                .ToList()
                .ForEach(element => element.ReplaceWith(selector(element)));
        }

        /// <summary>
        /// Transforms all elements in the given document matching the given
        /// <paramref name="name"/> using the given <paramref name="selector"/>
        /// function to perform the transformation.
        /// </summary>
        /// <param name="document">The document to be modified.</param>
        /// <param name="name">The name of the elements to be transformed.</param>
        /// <param name="selector">The function that defines the transformation.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is a targeted extension for an XML document")]
        public static void TransformAll(this XDocument document, XName name, Func<XElement, int, object> selector)
        {
            document
                .Descendants(name)
                .Select((Element, Index) => new { Element, Index })
                .ToList()
                .ForEach(obj => obj.Element.ReplaceWith(selector(obj.Element, obj.Index)));
        }

        /// <summary>
        /// Formats the contents of an XML element and returns the result.
        /// </summary>
        /// <param name="element">The elements whose contents are to be formatted.</param>
        /// <returns>The formatted contents of the elements.</returns>
        /// <remarks>
        /// The format to be used is determined by the <c>type</c> and <c>spec</c>
        /// attributes of the element. The <c>type</c> attribute should be the fully
        /// qualified type name of an <see cref="IFormattable"/> type. The <c>spec</c>
        /// attribute defines the format string to be used when formatting the contents.
        /// If <c>type</c> or <c>spec</c> are not defined or if <c>type</c> resovles to
        /// a type that is not <see cref="IFormattable"/>, the contents are not formatted
        /// and are returned as-is.
        /// </remarks>
        public static object Format(this XElement element)
        {
            IFormattable formattable;

            Type elementType;
            string formatString;
            string value;

            elementType = Type.GetType((string)element.Attribute("type"), false);
            formatString = (string)element.Attribute("spec");

            if ((object)elementType == null || (object)formatString == null)
                return element.Nodes();

            if (!typeof(IFormattable).IsAssignableFrom(elementType))
                return element.Nodes();

            value = (string)element;
            formattable = (IFormattable)Convert.ChangeType(value, elementType);
            return new XText(formattable.ToString(formatString, null));
        }
    }
}