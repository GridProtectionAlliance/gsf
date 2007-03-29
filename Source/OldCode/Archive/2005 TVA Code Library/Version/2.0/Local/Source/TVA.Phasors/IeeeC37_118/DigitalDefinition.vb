'*******************************************************************************************************
'  DigitalDefinition.vb - IEEE C37.118 Digital definition
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports System.ComponentModel
Imports System.Text
Imports TVA.Phasors.Common

Namespace Phasors.IeeeC37_118

    <CLSCompliant(False), Serializable()> _
    Public Class DigitalDefinition

        Inherits DigitalDefinitionBase

        Private m_normalStatus As Int16
        Private m_validInputs As Int16
        Private m_label As String
        Private m_parentAquired As Boolean
        Private m_draftRevision As DraftRevision

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize digital definition
            m_normalStatus = info.GetInt16("normalStatus")
            m_validInputs = info.GetInt16("validInputs")
            m_label = info.GetString("digitalLabels")

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell)

            MyBase.New(parent)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal index As Int32, ByVal label As String)

            MyBase.New(parent, index, label)

        End Sub

        Public Sub New(ByVal parent As ConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parent, binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal digitalDefinition As IDigitalDefinition)

            MyBase.New(digitalDefinition)

        End Sub

        Friend Shared Function CreateNewDigitalDefintion(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32) As IDigitalDefinition

            Return New DigitalDefinition(parent, binaryImage, startIndex)

        End Function

        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Parent() As ConfigurationCell
            Get
                Return MyBase.Parent
            End Get
        End Property

        Public ReadOnly Property LabelCount() As Int32
            Get
                If DraftRevision = DraftRevision.Draft6 Then
                    Return 1
                Else
                    Return 16
                End If
            End Get
        End Property

        ' We hide this from the editor just because this is a large combined string of all digital labels,
        ' and it will make more sense for consumers to use the "Labels" property
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Overrides Property Label() As String
            Get
                Return m_label
            End Get
            Set(ByVal value As String)
                If String.IsNullOrEmpty(value) Then value = "undefined"

                If value.Trim().Length > MaximumLabelLength Then
                    Throw New OverflowException("Label length cannot exceed " & MaximumLabelLength)
                Else
                    ' We override this function since base class automatically "fixes-up" labels
                    ' by removing duplicate white space characters - this can throw off the
                    ' label offsets which would break the "Labels" property (below)
                    m_label = value.Trim()
                End If
            End Set
        End Property

        ''' <summary>Accesses individual labels for each bit in the digital defintion</summary>
        ''' <param name="index">Desired bit label to access</param>
        ''' <remarks>
        ''' <para>In the final version of the protocol each digital bit can be labeled, but we read them out as one big string in the "Label" property so this property allows individual access to each label</para>
        ''' <para>Note that the draft 6 implementation of the protocol supports one label for all 16-bits, however draft 7 (i.e., version 1) supports a label for each of the 16 bits</para>
        ''' </remarks>
        Public Property Labels(ByVal index As Int32) As String
            Get
                If index < 0 Or index >= LabelCount Then Throw New IndexOutOfRangeException("Invalid label index specified.  Note that there are " & LabelCount & " labels per digital available in " & [Enum].GetName(GetType(DraftRevision), DraftRevision) & " of the IEEE C37.118 protocol")

                Return GetValidLabel(Label.PadRight(MaximumLabelLength).Substring(index * 16, MyBase.MaximumLabelLength))
            End Get
            Set(ByVal value As String)
                If index < 0 Or index >= LabelCount Then Throw New IndexOutOfRangeException("Invalid label index specified.  Note that there are " & LabelCount & " labels per digital available in " & [Enum].GetName(GetType(DraftRevision), DraftRevision) & " of the IEEE C37.118 protocol")

                If value.Trim().Length > MyBase.MaximumLabelLength Then
                    Throw New OverflowException("Label length cannot exceed " & MyBase.MaximumLabelLength)
                Else
                    Dim current As String = Label.PadRight(MaximumLabelLength)
                    Dim left As String = ""
                    Dim right As String = ""

                    If index > 0 Then left = current.Substring(0, index * MyBase.MaximumLabelLength)
                    If index < 15 Then right = current.Substring((index + 1) * MyBase.MaximumLabelLength)

                    Label = left & GetValidLabel(value).PadRight(MyBase.MaximumLabelLength) & right
                End If
            End Set
        End Property

        Public Overrides ReadOnly Property MaximumLabelLength() As Int32
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

        Public ReadOnly Property DraftRevision() As DraftRevision
            Get
                If m_parentAquired Then
                    Return m_draftRevision
                Else
                    ' We must assume version 1 until a parent reference is available
                    ' Note: parent class, being higher up in the chain, is not available during early
                    ' points of deserialization of this class - however, this method gets called
                    ' to determine proper number of maximum digital labels - hence the need for
                    ' this function - since we had to do this anyway, we took the opportunity to
                    ' cache this value locally for speed
                    If Parent IsNot Nothing AndAlso Parent.Parent IsNot Nothing Then
                        m_parentAquired = True
                        m_draftRevision = Parent.Parent.DraftRevision
                        Return m_draftRevision
                    Else
                        Return DraftRevision.Draft7
                    End If
                End If
            End Get
        End Property

        Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            If DraftRevision = DraftRevision.Draft6 Then
                ' Handle single label the standard way (parsing out null value)
                MyBase.ParseBodyImage(state, binaryImage, startIndex)
            Else
                ' For "multiple" labels - we just replace null's with spaces
                For x As Integer = startIndex To startIndex + MaximumLabelLength - 1
                    If binaryImage(x) = 0 Then binaryImage(x) = 32
                Next

                Label = Encoding.ASCII.GetString(binaryImage, startIndex, MaximumLabelLength)
            End If

        End Sub

        Friend Shared ReadOnly Property ConversionFactorLength() As Int32
            Get
                Return 4
            End Get
        End Property

        Friend ReadOnly Property ConversionFactorImage() As Byte()
            Get
                Dim buffer As Byte() = CreateArray(Of Byte)(ConversionFactorLength)

                EndianOrder.BigEndian.CopyBytes(m_normalStatus, buffer, 0)
                EndianOrder.BigEndian.CopyBytes(m_validInputs, buffer, 2)

                Return buffer
            End Get
        End Property

        Friend Sub ParseConversionFactor(ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            m_normalStatus = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
            m_validInputs = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)

        End Sub

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize digital definition
            info.AddValue("normalStatus", m_normalStatus)
            info.AddValue("validInputs", m_validInputs)
            info.AddValue("digitalLabels", m_label)

        End Sub

        Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
            Get
                Dim baseAttributes As Dictionary(Of String, String) = MyBase.Attributes
                Dim normalStatusBytes As Byte() = BitConverter.GetBytes(NormalStatus)
                Dim validInputsBytes As Byte() = BitConverter.GetBytes(ValidInputs)

                baseAttributes.Add("Normal Status", NormalStatus)
                baseAttributes.Add("Normal Status (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(normalStatusBytes))
                baseAttributes.Add("Normal Status (Hexadecimal)", "0x" & ByteEncoding.Hexadecimal.GetString(normalStatusBytes))

                baseAttributes.Add("Valid Inputs", ValidInputs)
                baseAttributes.Add("Valid Inputs (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(validInputsBytes))
                baseAttributes.Add("Valid Inputs (Hexadecimal)", "0x" & ByteEncoding.Hexadecimal.GetString(validInputsBytes))

                If DraftRevision > DraftRevision.Draft6 Then
                    baseAttributes.Add("Bit Label Count", LabelCount)
                    For x As Integer = 0 To LabelCount - 1
                        baseAttributes.Add("     Bit " & x & " Label", Labels(x))
                    Next
                End If

                Return baseAttributes
            End Get
        End Property

    End Class

End Namespace
