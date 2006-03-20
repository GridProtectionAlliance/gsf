'*******************************************************************************************************
'  DigitalDefinition.vb - IEEE C37.118 Digital definition
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports Tva.Interop

Namespace IeeeC37_118

    <CLSCompliant(False)> _
    Public Class DigitalDefinition

        Inherits DigitalDefinitionBase

        Private m_normalStatus As Int16
        Private m_validInputs As Int16

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Integer, ByVal label As String)

            MyBase.New(parent, index, label)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(parent, binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal digitalDefinition As IDigitalDefinition)

            MyBase.New(digitalDefinition)

        End Sub

        Friend Shared Function CreateNewDigitalDefintion(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Integer) As IDigitalDefinition

            Return New DigitalDefinition(parent, binaryImage, startIndex)

        End Function

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

        Public ReadOnly Property LabelCount() As Integer
            Get
                If Parent.Parent.RevisionNumber = RevisionNumber.RevisionD6 Then
                    Return 1
                Else
                    Return 16
                End If
            End Get
        End Property

        ''' <summary>Accesses individual labels for each bit in the digital defintion</summary>
        ''' <param name="index">Desired bit label to access</param>
        ''' <remarks>In the final version of the protocol each digital bit can be labeled, but we read them out as one big string in the "Label" property so this property allows individual access to each label</remarks>
        Public Property Labels(ByVal index As Integer) As String
            Get
                If index < 0 Or index >= LabelCount Then Throw New IndexOutOfRangeException("Invalid label index specified.  Note that there are " & LabelCount & " labels per digital available in " & [Enum].GetName(GetType(RevisionNumber), Parent.Parent.RevisionNumber) & " of the IEEE C37.118 protocol")

                Return Label.PadRight(MaximumLabelLength).Substring(index * 16, MaximumLabelLength).Trim()
            End Get
            Set(ByVal value As String)
                If index < 0 Or index >= LabelCount Then Throw New IndexOutOfRangeException("Invalid label index specified.  Note that there are " & LabelCount & " labels per digital available in " & [Enum].GetName(GetType(RevisionNumber), Parent.Parent.RevisionNumber) & " of the IEEE C37.118 protocol")

                If value.Trim().Length > MyBase.MaximumLabelLength Then
                    Throw New OverflowException("Label length cannot exceed " & MyBase.MaximumLabelLength)
                Else
                    Dim current As String = Label.PadRight(MaximumLabelLength)
                    Dim left As String = ""
                    Dim right As String = ""

                    If index > 0 Then left = current.Substring(0, index * MaximumLabelLength)
                    If index < 15 Then right = current.Substring((index + 1) * 16)

                    Label = left & value.Replace(Chr(20), " "c).PadRight(MaximumLabelLength) & right
                End If
            End Set
        End Property

        Public Overrides ReadOnly Property MaximumLabelLength() As Integer
            Get
                Return LabelCount * MyBase.MaximumLabelLength
            End Get
        End Property

        Public Property NormalStatus() As Int16
            Get
                Return m_normalStatus
            End Get
            Set(ByVal value As Int16)
                m_normalStatus = value
            End Set
        End Property

        Public Property ValidInputs() As Int16
            Get
                Return m_validInputs
            End Get
            Set(ByVal value As Int16)
                m_validInputs = value
            End Set
        End Property

        Friend Shared ReadOnly Property ConversionFactorLength() As Integer
            Get
                Return 4
            End Get
        End Property

        Friend ReadOnly Property ConversionFactorImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), ConversionFactorLength)

                EndianOrder.BigEndian.CopyBytes(m_normalStatus, buffer, 0)
                EndianOrder.BigEndian.CopyBytes(m_validInputs, buffer, 2)

                Return buffer
            End Get
        End Property

        Friend Sub ParseConversionFactor(ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            m_normalStatus = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
            m_validInputs = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)

        End Sub

    End Class

End Namespace
