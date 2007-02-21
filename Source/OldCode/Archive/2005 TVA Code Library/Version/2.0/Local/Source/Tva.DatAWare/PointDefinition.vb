'*******************************************************************************************************
'  PointDefinition.vb - Standard DatAWare database structure element
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
'  05/03/2006 - J. Ritchie Carroll
'       Initial version of source imported from 1.1 code library
'
'*******************************************************************************************************

Imports System.Text
Imports Tva.Text.Common

' Standard database structure element
<Serializable()> _
Public Class PointDefinition
    Implements IComparable

#Region " Member Declaration "

    Private m_index As Integer
    Private m_description As String = ""        ' 40
    Private m_unit As Short                     ' 2
    Private m_securityLevel As Short            ' 2
    Private m_hardwareInfo As String = ""       ' 64
    Private m_spares As Byte()                  ' 64
    Private m_flagWord As MetadataFlagWord      ' 4
    Private m_transitionFlag As Integer         ' 4
    Private m_scanRate As Single                ' 4
    Private m_name As String = ""               ' 20
    Private m_synonym1 As String = ""           ' 20
    Private m_synonym2 As String = ""           ' 20
    Private m_siteName As String = ""           ' 2
    Private m_sourceID As Short                 ' 2
    Private m_compressionMinimumTime As Integer ' 4
    Private m_compressionMaximumTime As Integer ' 4
    Private m_system As String = ""             ' 4
    Private m_email As String = ""              ' 50
    Private m_pager As String = ""              ' 30
    Private m_phone As String = ""              ' 30
    Private m_remarks As String = ""            ' 128
    'Private m_binaryInfo As Byte()              ' 256
    Private m_analogFields As MetadataAnalogFields
    Private m_digitalFields As MetadataDigitalFields
    Private m_composedFields As MetadataComposedFields
    Private m_constantFields As MetadataConstantFields
    Private m_textEncoding As Encoding

#End Region

#Region " Public Code "

    Public Const BinaryLength As Integer = 754

    Public Sub New(ByVal index As Integer)

        MyBase.New()
        m_index = index
        m_textEncoding = Encoding.Default ' By default we decode strings using encoding for the system's current ANSI code page
        m_flagWord = New MetadataFlagWord(0)
        m_spares = CreateArray(Of Byte)(64)
        'm_binaryInfo = CreateArray(Of Byte)(256)

    End Sub

    Public Sub New(ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

        MyClass.New(index, binaryImage, startIndex, Nothing)

    End Sub

    Public Sub New(ByVal index As Integer, ByVal binaryImage As Byte(), ByVal startIndex As Integer, ByVal encoding As Encoding)

        Me.New(index)

        If encoding IsNot Nothing Then m_textEncoding = encoding

        If binaryImage IsNot Nothing Then
            If binaryImage.Length - startIndex >= BinaryLength Then
                Dim binaryInfo As Byte() = CreateArray(Of Byte)(256)
                m_description = m_textEncoding.GetString(binaryImage, startIndex, 40).Trim()
                m_unit = BitConverter.ToInt16(binaryImage, startIndex + 40)
                m_securityLevel = BitConverter.ToInt16(binaryImage, startIndex + 42)
                m_hardwareInfo = m_textEncoding.GetString(binaryImage, startIndex + 44, 64).Trim()
                Array.Copy(binaryImage, startIndex + 108, m_spares, 0, 64)
                m_flagWord = New MetadataFlagWord(BitConverter.ToInt32(binaryImage, startIndex + 172))
                m_transitionFlag = BitConverter.ToInt32(binaryImage, startIndex + 176)
                m_scanRate = BitConverter.ToSingle(binaryImage, startIndex + 180)
                m_name = m_textEncoding.GetString(binaryImage, startIndex + 184, 20).Trim()
                m_synonym1 = m_textEncoding.GetString(binaryImage, startIndex + 204, 20).Trim()
                m_synonym2 = m_textEncoding.GetString(binaryImage, startIndex + 224, 20).Trim()
                m_siteName = m_textEncoding.GetString(binaryImage, startIndex + 244, 2).Trim()
                m_sourceID = BitConverter.ToInt16(binaryImage, startIndex + 246)
                m_compressionMinimumTime = BitConverter.ToInt32(binaryImage, startIndex + 248)
                m_compressionMaximumTime = BitConverter.ToInt32(binaryImage, startIndex + 252)
                m_system = m_textEncoding.GetString(binaryImage, startIndex + 256, 4).Trim()
                m_email = m_textEncoding.GetString(binaryImage, startIndex + 260, 50).Trim()
                m_pager = m_textEncoding.GetString(binaryImage, startIndex + 310, 30).Trim()
                m_phone = m_textEncoding.GetString(binaryImage, startIndex + 340, 30).Trim()
                m_remarks = m_textEncoding.GetString(binaryImage, startIndex + 370, 128).Trim()
                Array.Copy(binaryImage, startIndex + 498, binaryInfo, 0, 256)
                Select Case m_flagWord.PointType
                    Case PointType.Analog
                        m_analogFields = New MetadataAnalogFields(binaryInfo)
                    Case PointType.Digital
                        m_digitalFields = New MetadataDigitalFields(binaryInfo)
                    Case PointType.Composed
                        m_composedFields = New MetadataComposedFields(binaryInfo)
                    Case PointType.Constant
                        m_constantFields = New MetadataConstantFields(binaryInfo)
                End Select
            Else
                Throw New ArgumentException("Binary image size from startIndex is too small.")
            End If
        Else
            Throw New ArgumentNullException("binaryImage")
        End If

    End Sub

    Public Shared Function Clone(ByVal pointDefinition As PointDefinition, ByVal newIndex As Integer) As PointDefinition

        Dim newPointDefinition As New PointDefinition(newIndex)

        With newPointDefinition
            .Description = pointDefinition.Description
            .Unit = pointDefinition.Unit
            .SecurityLevel = pointDefinition.SecurityLevel
            .HardwareInfo = pointDefinition.HardwareInfo
            Array.Copy(pointDefinition.Spares, .Spares, 64)
            .FlagWord = pointDefinition.FlagWord
            .TransitionFlag = pointDefinition.TransitionFlag
            .ScanRate = pointDefinition.ScanRate
            .Name = pointDefinition.Name
            .Synonym1 = pointDefinition.Synonym1
            .Synonym2 = pointDefinition.Synonym2
            .SiteName = pointDefinition.SiteName
            .SourceID = pointDefinition.SourceID
            .CompressionMinimumTime = pointDefinition.CompressionMinimumTime
            .CompressionMaximumTime = pointDefinition.CompressionMaximumTime
            .System = pointDefinition.System
            .Email = pointDefinition.Email
            .Pager = pointDefinition.Pager
            .Phone = pointDefinition.Phone
            .Remarks = pointDefinition.Remarks
            'Array.Copy(pointDefinition.BinaryInfo, .BinaryInfo, 256)
            .AnalogFields = pointDefinition.AnalogFields
            .DigitalFields = pointDefinition.DigitalFields
            .ComposedFields = pointDefinition.ComposedFields
            .ConstantFields = pointDefinition.ConstantFields
            .TextEncoding = pointDefinition.TextEncoding
        End With

        Return newPointDefinition

    End Function

    Public ReadOnly Property Index() As Integer
        Get
            Return m_index
        End Get
    End Property

    Public Property Description() As String
        Get
            Return m_description
        End Get
        Set(ByVal value As String)
            m_description = TruncateString(value, 40)
        End Set
    End Property

    Public Property Unit() As Short
        Get
            Return m_unit
        End Get
        Set(ByVal value As Short)
            m_unit = value
        End Set
    End Property

    Public Property SecurityLevel() As Short
        Get
            Return m_securityLevel
        End Get
        Set(ByVal value As Short)
            m_securityLevel = value
        End Set
    End Property

    Public Property HardwareInfo() As String
        Get
            Return m_hardwareInfo
        End Get
        Set(ByVal value As String)
            m_hardwareInfo = TruncateString(value, 64)
        End Set
    End Property

    Public Property Spares() As Byte()
        Get
            Return m_spares
        End Get
        Set(ByVal value As Byte())
            m_spares = value
        End Set
    End Property

    Public Property FlagWord() As MetadataFlagWord
        Get
            Return m_flagWord
        End Get
        Set(ByVal value As MetadataFlagWord)
            m_flagWord = value
        End Set
    End Property

    Public Property TransitionFlag() As Integer
        Get
            Return m_transitionFlag
        End Get
        Set(ByVal value As Integer)
            m_transitionFlag = value
        End Set
    End Property

    Public Property ScanRate() As Single
        Get
            Return m_scanRate
        End Get
        Set(ByVal value As Single)
            m_scanRate = value
        End Set
    End Property

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Set(ByVal value As String)
            m_name = TruncateString(value, 20)
        End Set
    End Property

    Public Property Synonym1() As String
        Get
            Return m_synonym1
        End Get
        Set(ByVal value As String)
            m_synonym1 = TruncateString(value, 20)
        End Set
    End Property

    Public Property Synonym2() As String
        Get
            Return m_synonym2
        End Get
        Set(ByVal value As String)
            m_synonym2 = TruncateString(value, 20)
        End Set
    End Property

    Public Property SiteName() As String
        Get
            Return m_siteName
        End Get
        Set(ByVal value As String)
            m_siteName = TruncateString(value, 2)
        End Set
    End Property

    Public Property SourceID() As Short
        Get
            Return m_sourceID
        End Get
        Set(ByVal value As Short)
            m_sourceID = value
        End Set
    End Property

    Public Property CompressionMinimumTime() As Integer
        Get
            Return m_compressionMinimumTime
        End Get
        Set(ByVal value As Integer)
            m_compressionMinimumTime = value
        End Set
    End Property

    Public Property CompressionMaximumTime() As Integer
        Get
            Return m_compressionMaximumTime
        End Get
        Set(ByVal value As Integer)
            m_compressionMaximumTime = value
        End Set
    End Property

    Public Property System() As String
        Get
            Return m_system
        End Get
        Set(ByVal value As String)
            m_system = TruncateString(value, 4)
        End Set
    End Property

    Public Property Email() As String
        Get
            Return m_email
        End Get
        Set(ByVal value As String)
            m_email = TruncateString(value, 50)
        End Set
    End Property

    Public Property Pager() As String
        Get
            Return m_pager
        End Get
        Set(ByVal value As String)
            m_pager = TruncateString(value, 30)
        End Set
    End Property

    Public Property Phone() As String
        Get
            Return m_phone
        End Get
        Set(ByVal value As String)
            m_phone = TruncateString(value, 30)
        End Set
    End Property

    Public Property Remarks() As String
        Get
            Return m_remarks
        End Get
        Set(ByVal value As String)
            m_remarks = TruncateString(value, 128)
        End Set
    End Property

    'Public Property BinaryInfo() As Byte()
    '    Get
    '        Return m_binaryInfo
    '    End Get
    '    Set(ByVal value As Byte())
    '        m_binaryInfo = value
    '    End Set
    'End Property

    Public Property AnalogFields() As MetadataAnalogFields
        Get
            Return m_analogFields
        End Get
        Set(ByVal value As MetadataAnalogFields)
            m_analogFields = value
        End Set
    End Property

    Public Property DigitalFields() As MetadataDigitalFields
        Get
            Return m_digitalFields
        End Get
        Set(ByVal value As MetadataDigitalFields)
            m_digitalFields = value
        End Set
    End Property

    Public Property ComposedFields() As MetadataComposedFields
        Get
            Return m_composedFields
        End Get
        Set(ByVal value As MetadataComposedFields)
            m_composedFields = value
        End Set
    End Property

    Public Property ConstantFields() As MetadataConstantFields
        Get
            Return m_constantFields
        End Get
        Set(ByVal value As MetadataConstantFields)
            m_constantFields = value
        End Set
    End Property

    Public Property TextEncoding() As Encoding
        Get
            Return m_textEncoding
        End Get
        Set(ByVal value As Encoding)
            m_textEncoding = value
        End Set
    End Property

    Public ReadOnly Property BinaryImage() As Byte()
        Get
            Dim image As Byte() = CreateArray(Of Byte)(BinaryLength)
            Dim binaryInfo As Byte() = CreateArray(Of Byte)(256)

            ' Construct the binary IP buffer for this event
            Array.Copy(m_textEncoding.GetBytes(m_description.PadRight(40)), 0, image, 0, 40)
            Array.Copy(BitConverter.GetBytes(m_unit), 0, image, 40, 2)
            Array.Copy(BitConverter.GetBytes(m_securityLevel), 0, image, 42, 2)
            Array.Copy(m_textEncoding.GetBytes(m_hardwareInfo.PadRight(64)), 0, image, 44, 64)
            Array.Copy(m_spares, 0, image, 108, 64)
            Array.Copy(BitConverter.GetBytes(m_flagWord.Value), 0, image, 172, 4)
            Array.Copy(BitConverter.GetBytes(m_transitionFlag), 0, image, 176, 4)
            Array.Copy(BitConverter.GetBytes(m_scanRate), 0, image, 180, 4)
            Array.Copy(m_textEncoding.GetBytes(m_name.PadRight(20)), 0, image, 184, 20)
            Array.Copy(m_textEncoding.GetBytes(m_synonym1.PadRight(20)), 0, image, 204, 20)
            Array.Copy(m_textEncoding.GetBytes(m_synonym2.PadRight(20)), 0, image, 224, 20)
            Array.Copy(m_textEncoding.GetBytes(m_siteName.PadRight(2)), 0, image, 244, 2)
            Array.Copy(BitConverter.GetBytes(m_sourceID), 0, image, 246, 2)
            Array.Copy(BitConverter.GetBytes(m_compressionMinimumTime), 0, image, 248, 4)
            Array.Copy(BitConverter.GetBytes(m_compressionMaximumTime), 0, image, 252, 4)
            Array.Copy(m_textEncoding.GetBytes(m_system.PadRight(4)), 0, image, 256, 4)
            Array.Copy(m_textEncoding.GetBytes(m_email.PadRight(50)), 0, image, 260, 50)
            Array.Copy(m_textEncoding.GetBytes(m_pager.PadRight(30)), 0, image, 310, 30)
            Array.Copy(m_textEncoding.GetBytes(m_phone.PadRight(30)), 0, image, 340, 30)
            Array.Copy(m_textEncoding.GetBytes(m_remarks.PadRight(128)), 0, image, 370, 128)
            Select Case m_flagWord.PointType
                Case PointType.Analog
                    Array.Copy(m_analogFields.BinaryImage, binaryInfo, MetadataAnalogFields.BinaryLength)
                Case PointType.Digital
                    Array.Copy(m_digitalFields.BinaryImage, binaryInfo, MetadataDigitalFields.BinaryLength)
                Case PointType.Composed
                    Array.Copy(m_composedFields.BinaryImage, binaryInfo, MetadataComposedFields.BinaryLength)
                Case PointType.Constant
                    Array.Copy(m_constantFields.BinaryImage, binaryInfo, MetadataConstantFields.BinaryLength)
            End Select
            Array.Copy(binaryInfo, 0, image, 498, 256)

            Return image
        End Get
    End Property

#Region " IComparable Implementation "

    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo

        Dim other As PointDefinition = TryCast(obj, PointDefinition)
        If other IsNot Nothing Then
            Return m_index.CompareTo(other.Index)
        Else
            Throw New ArgumentException(String.Format("Cannot compare {0} with {1}.", Me.GetType().Name, other.GetType().Name))
        End If

    End Function

#End Region

#End Region

End Class