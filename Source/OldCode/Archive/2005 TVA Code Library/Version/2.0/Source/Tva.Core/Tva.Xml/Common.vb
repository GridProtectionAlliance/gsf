'*******************************************************************************************************
'  Tva.Xml.Common.vb - Common XML Functions
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/23/2003 - James R Carroll
'       Original version of source code generated
'  01/23/2006 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Common)
'
'*******************************************************************************************************

Imports System.Xml

Namespace Xml

    ''' <summary>
    ''' Defines common global functions related to XML data
    ''' </summary>
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>
        ''' <para>Gets an Xml node from given path, creating the entire path it if it doesn't exist.</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>This overload just allows start given xml document by using its root element.</para>
        ''' </remarks>
        Public Shared Function GetXmlNode(ByVal xmlDoc As XmlDocument, ByVal xpath As String) As XmlNode

            Return GetXmlNode(xmlDoc.DocumentElement, xpath, False)

        End Function

        ''' <summary>
        ''' <para>Gets an Xml node from given path, creating the entire path it if it doesn't exist.</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>This overload just allows start given xml document by using its root element.</para>
        ''' </remarks>
        Public Shared Function GetXmlNode(ByVal xmlDoc As XmlDocument, ByVal xpath As String, ByRef isDirty As Boolean) As XmlNode

            Return GetXmlNode(xmlDoc.DocumentElement, xpath, isDirty)

        End Function

        ''' <summary>
        ''' <para>Gets an Xml node from given path, creating the entire path it if it doesn't exist.</para>
        ''' </summary>
        Public Shared Function GetXmlNode(ByVal parentNode As XmlNode, ByVal xpath As String) As XmlNode

            Return GetXmlNode(parentNode, xpath, False)

        End Function

        ''' <summary>
        ''' <para>Gets an Xml node from given path, creating the entire path it if it doesn't exist.</para>
        ''' </summary>
        Public Shared Function GetXmlNode(ByVal parentNode As XmlNode, ByVal xpath As String, ByRef isDirty As Boolean) As XmlNode

            Dim node As XmlNode
            Dim element As String
            Dim elements As String()

            ' Remove any slash prefixes
            While Left(xpath, 1) = "/"
                xpath = Mid(xpath, 2)
            End While

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

        ''' <summary>
        ''' <para>Safely gets or sets an XML node's attribute, creating it if needed.</para>
        ''' </summary>
        ''' <param name="node"> Required.xml node. </param>
        ''' <param name="name"> Required.String. </param>
        Public Shared Property Attribute(ByVal node As XmlNode, ByVal name As String) As String
            Get
                Dim attr As XmlAttribute = node.Attributes(name)
                If attr Is Nothing Then
                    Return Nothing
                Else
                    Return attr.Value
                End If
            End Get
            Set(ByVal Value As String)
                Dim attr As XmlAttribute = node.Attributes(name)

                If attr Is Nothing Then
                    attr = node.OwnerDocument.CreateAttribute(name)
                    node.Attributes.Append(attr)
                End If

                If Not attr Is Nothing Then
                    attr.Value = Value
                    node.Attributes.SetNamedItem(attr)
                End If
            End Set
        End Property

    End Class

End Namespace
