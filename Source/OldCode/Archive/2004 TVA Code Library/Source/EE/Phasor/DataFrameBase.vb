'***********************************************************************
'  DataFrameBase.vb - Data frame base class
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports TVA.Interop

Namespace EE.Phasor

    ' This class represents the protocol independent common implementation of a data frame that can be sent or received from a PMU.
    Public MustInherit Class DataFrameBase

        Inherits ChannelFrameBase
        Implements IDataFrame

        Private m_configurationFrame As IConfigurationFrame

        Protected Sub New()

            MyBase.New()

        End Sub

        Protected Sub New(ByVal cells As ChannelCellCollection, ByVal timeTag As Unix.TimeTag, ByVal milliseconds As Double, ByVal synchronizationIsValid As Boolean, ByVal dataIsValid As Boolean, ByVal configurationFrame As IConfigurationFrame)

            MyBase.New(cells, timeTag, milliseconds, synchronizationIsValid, dataIsValid)

            m_configurationFrame = configurationFrame

        End Sub

        ' Dervied classes are expected to expose a Public Sub New(ByVal configurationFrame As IConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
        ' and automatically pass in type parameter
        Protected Sub New(ByVal configurationFrame As IConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer, ByVal dataCellType As Type)

            Me.New()

            m_configurationFrame = configurationFrame

            With m_configurationFrame
                For x As Integer = 0 To .Cells.Count - 1
                    Cells.Add(Activator.CreateInstance(dataCellType, New Object() {.Cells(x), binaryImage, startIndex}))
                    startIndex += Cells(x).BinaryLength
                Next
            End With

        End Sub

        ' Dervied classes are expected to expose a Public Sub New(ByVal dataFrame As IDataFrame)
        Protected Sub New(ByVal dataFrame As IDataFrame)

            Me.New(dataFrame.Cells, dataFrame.TimeTag, dataFrame.Milliseconds, dataFrame.SynchronizationIsValid, dataFrame.DataIsValid, dataFrame.ConfigurationFrame)

        End Sub

        Public Overridable Property ConfigurationFrame() As IConfigurationFrame Implements IDataFrame.ConfigurationFrame
            Get
                Return m_configurationFrame
            End Get
            Set(ByVal Value As IConfigurationFrame)
                m_configurationFrame = Value
            End Set
        End Property

        Public Overridable Shadows ReadOnly Property Cells() As DataCellCollection Implements IDataFrame.Cells
            Get
                Return MyBase.Cells
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "TVA.EE.Phasor.DataFrameBase"
            End Get
        End Property

    End Class

End Namespace