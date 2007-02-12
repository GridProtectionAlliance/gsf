'*******************************************************************************************************
'  DataFrame.vb - FNet Data Frame
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
'  02/08/2007 - J. Ritchie Carroll & Jian Zuo (Ryan)
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports Tva.DateTime
Imports Tva.IO.Compression.Common

Namespace FNet

    ' This is essentially a "row" of PMU data at a given timestamp
    <CLSCompliant(False), Serializable()> _
    Public Class DataFrame

        Inherits DataFrameBase

        Private m_sampleIndex As Int16

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

            ' Deserialize data frame
            m_sampleIndex = info.GetInt16("sampleIndex")

        End Sub

        Public Sub New(ByVal ticks As Long, ByVal configurationFrame As ConfigurationFrame)

            MyBase.New(New DataCellCollection, ticks, configurationFrame)

        End Sub

        Public Sub New(ByVal configurationFrame As ConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(New DataFrameParsingState(New DataCellCollection, 0, configurationFrame, _
                AddressOf FNet.DataCell.CreateNewDataCell), binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal dataFrame As IDataFrame)

            MyBase.New(dataFrame)

        End Sub

        ''' <summary>
        ''' Return the type
        ''' </summary>
      
        Public Overrides ReadOnly Property DerivedType() As System.Type
            Get
                Return Me.GetType()
            End Get
        End Property

        Public Shadows ReadOnly Property Cells() As DataCellCollection
            Get
                Return MyBase.Cells
            End Get
        End Property

        Public Shadows Property ConfigurationFrame() As ConfigurationFrame
            Get
                Return MyBase.ConfigurationFrame
            End Get
            Set(ByVal value As ConfigurationFrame)
                MyBase.ConfigurationFrame = value
            End Set
        End Property

        ''' <summary>
        ''' Set and Return the SampleIndex
        ''' </summary>
                Public Property SampleIndex() As Int16
            Get
                Return m_sampleIndex
            End Get
            Set(ByVal value As Int16)
                m_sampleIndex = value
            End Set
        End Property

        ''' <summary>
        ''' Return the checksum verification result. Since FNET data has no checksum, it is always valid.
        ''' </summary>
        Protected Overrides Function ChecksumIsValid(ByVal buffer() As Byte, ByVal startIndex As Integer) As Boolean

            ' FNet uses no checksum, we assume data packet is valid
            Return True

        End Function

        Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

            MyBase.GetObjectData(info, context)

            ' Serialize data frame
            info.AddValue("sampleIndex", m_sampleIndex)

        End Sub

        ''' <summary>
        ''' Set and return the attributes of the FNET protocol
        ''' </summary>
        ''' <remarks>Need to add Longitude, Lattitude and Satellite number to the attributes</remarks>
        Public Overrides ReadOnly Property Attributes() As Dictionary(Of String, String)
            Get
                With MyBase.Attributes
                    .Add("Sample Index", SampleIndex)
                End With

                Return MyBase.Attributes
            End Get
        End Property

    End Class

End Namespace