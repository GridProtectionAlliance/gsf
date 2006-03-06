'*******************************************************************************************************
'  DigitalDefinition.vb - IEEE C37.118 Digital definition
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public Class DigitalDefinition

        Inherits DigitalDefinitionBase

        Public Labels As String() = Array.CreateInstance(GetType(String), 16)

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Integer, ByVal label As String)

            MyBase.New(parent, index, label)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(parent)

            If parent.Parent.RevisionNumber = RevisionNumber.RevisionV1 Then
                ' We only parse digital labels in draft 7
                For x As Integer = 0 To 15
                    Labels(x) = Encoding.ASCII.GetString(binaryImage, startIndex + x * MaximumLabelLength, MaximumLabelLength)
                Next
            End If

        End Sub

        Public Sub New(ByVal digitalDefinition As IDigitalDefinition)

            MyBase.New(digitalDefinition)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        ' TODO: May want to shadow all parents in final derived classes...
        Public Shadows ReadOnly Property Parent() As ConfigurationCell
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public Overrides ReadOnly Property MaximumLabelLength() As Integer
            Get
                If Parent.Parent.RevisionNumber = RevisionNumber.RevisionD6 Then
                    Return MyBase.MaximumLabelLength
                Else
                    ' In the final version, each digital bit can be labeled - 
                    Return 16 * MyBase.MaximumLabelLength
                End If
            End Get
        End Property

    End Class

End Namespace
