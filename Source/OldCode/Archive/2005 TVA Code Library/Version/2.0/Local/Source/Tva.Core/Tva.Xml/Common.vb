'*******************************************************************************************************
'  Tva.Xml.Common.vb - Common XML Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/23/2003 - J. Ritchie Carroll
'       Original version of source code generated
'  01/23/2006 - J. Ritchie Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Common)
'
'*******************************************************************************************************

Imports System.Xml

Namespace Xml

    ''' <summary>Defines common global functions related to XML data.</summary>
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Gets an Xml node from given path, creating the entire path it if it doesn't exist.</summary>
        ''' <remarks>This overload just allows start given xml document by using its root element.</remarks>
        Public Shared Function GetXmlNode(ByVal xmlDoc As XmlDocument, ByVal xpath As String) As XmlNode

            Return GetXmlNode(xmlDoc.DocumentElement, xpath, False)

        End Function

        ''' <summary>Gets an Xml node from given path, creating the entire path it if it doesn't exist.</summary>
        ''' <remarks>
        ''' <para>This overload just allows start given xml document by using its root element.</para>
        ''' <para>Note that the <paramref name="isDirty" /> parameter will be set to True is any items were added to the tree</para>
        ''' </remarks>
        Public Shared Function GetXmlNode(ByVal xmlDoc As XmlDocument, ByVal xpath As String, ByRef isDirty As Boolean) As XmlNode

            Return GetXmlNode(xmlDoc.DocumentElement, xpath, isDirty)

        End Function

        ''' <summary>Gets an Xml node from given path, creating the entire path it if it doesn't exist.</summary>
        Public Shared Function GetXmlNode(ByVal parentNode As XmlNode, ByVal xpath As String) As XmlNode

            Return GetXmlNode(parentNode, xpath, False)

        End Function

        ''' <summary>Gets an Xml node from given path, creating the entire path it if it doesn't exist.</summary>
        ''' <remarks>Note that the <paramref name="isDirty" /> parameter will be set to True is any items were added to the tree</remarks>
        Public Shared Function GetXmlNode(ByVal parentNode As XmlNode, ByVal xpath As String, ByRef isDirty As Boolean) As XmlNode

            Dim node As XmlNode = Nothing
            Dim element As String
            Dim elements As String()

            ' Remove any slash prefixes
            Do While xpath.Chars(0) = "/"c
                xpath = xpath.Substring(1)
            Loop

            elements = xpath.Split("/"c)

            If elements.Length = 1 Then
                With parentNode.SelectNodes(xpath)
                    If .Count = 0 Then
                        node = parentNode.OwnerDocument.CreateElement(xpath)
                        parentNode.AppendChild(node)
                        isDirty = True
                    Else
                        node = .Item(0)
                    End If
                End With
            Else
                For Each element In elements
                    node = GetXmlNode(parentNode, element)
                    parentNode = node
                Next
            End If

            Return node

        End Function

        ''' <summary>Safely gets or sets an XML node's attribute.</summary>
        ''' <remarks>If you assign a value to an attribute that doesn't exist, it will be created</remarks>
        Public Shared Property Attribute(ByVal node As XmlNode, ByVal name As String) As String
            Get
                Dim attr As XmlAttribute = node.Attributes(name)
                If attr Is Nothing Then
                    Return Nothing
                Else
                    Return attr.Value
                End If
            End Get
            Set(ByVal value As String)
                Dim attr As XmlAttribute = node.Attributes(name)

                If attr Is Nothing Then
                    attr = node.OwnerDocument.CreateAttribute(name)
                    node.Attributes.Append(attr)
                End If

                If Not attr Is Nothing Then
                    attr.Value = value
                    node.Attributes.SetNamedItem(attr)
                End If
            End Set
        End Property

    End Class

End Namespace
