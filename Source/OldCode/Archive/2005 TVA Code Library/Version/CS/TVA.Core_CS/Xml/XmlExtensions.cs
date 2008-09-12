//*******************************************************************************************************
//  TVA.Xml.XmlExtensions.vb - Defines extension functions related to Xml elements
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/23/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  01/23/2006 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Common).
//  01/24/2007 - J. Ritchie Carroll
//       Added GetDataSet method to convert an XML string of data into a DataSet object.
//  12/13/2007 - Darrell Zuercher
//       Edited code comments.
//  09/12/2008 - J. Ritchie Carroll
//      Converted to C# extension functions.
//
//*******************************************************************************************************

using System;
using System.Xml;
using System.Data;

namespace TVA.Xml
{
    /// <summary>Defines common global functions related to XML data.</summary>
    public static class XmlExtensions
    {
        /// <summary>Gets an XML node from given path, creating the entire path if it does not exist.</summary>
        /// <remarks>This overload just allows the start of the given XML document by using its root element.</remarks>
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
        public static XmlNode GetXmlNode(this XmlDocument xmlDoc, string xpath, out bool isDirty)
        {
            return xmlDoc.DocumentElement.GetXmlNode(xpath, out isDirty);
        }

        /// <summary>Gets an XML node from given path, creating the entire path if it does not exist.</summary>
        public static XmlNode GetXmlNode(this XmlNode parentNode, string xpath)
        {
            bool isDirty;
            return parentNode.GetXmlNode(xpath, out isDirty);
        }

        /// <summary>Gets an XML node from given path, creating the entire path if it does not exist.</summary>
        /// <remarks>Note that the <paramref name="isDirty" /> parameter will be set to True if any items were added to
        /// the tree.</remarks>
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

                if (nodes.Count == 0)
                {
                    node = parentNode.OwnerDocument.CreateElement(xpath);
                    parentNode.AppendChild(node);
                    isDirty = true;
                }
                else
                {
                    node = nodes[0];
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
        public static string GetAttributeValue(this XmlNode node, string name)
        {
            XmlAttribute attr = node.Attributes[name];

            if (attr == null)
            {
                return null;
            }
            else
            {
                return attr.Value;
            }
        }

        /// <summary>Safely sets an XML node's attribute.</summary>
        /// <remarks>If you assign a value to an attribute that does not exist, the attribute will be created.</remarks>
        public static void SetAttributeValue(this XmlNode node, string name, string value)
        {
            XmlAttribute attr = node.Attributes[name];

            if (attr == null)
            {
                attr = node.OwnerDocument.CreateAttribute(name);
                node.Attributes.Append(attr);
            }
            else
            {
                attr.Value = value;
                node.Attributes.SetNamedItem(attr);
            }
        }

        /// <summary>
        /// Gets a data set object from an XML data set formatted as a String.
        /// </summary>
        /// <param name="xmlData">XML data string in standard DataSet format.</param>
        public static DataSet GetDataSet(this string xmlData)
        {
            DataSet dataSet = new DataSet();
            XmlTextReader xmlReader = new XmlTextReader(xmlData, XmlNodeType.Document, null);

            xmlReader.ReadOuterXml();

            // Reads the outer XML into the Dataset.
            dataSet.ReadXml(xmlReader);

            return dataSet;
        }
    }
}