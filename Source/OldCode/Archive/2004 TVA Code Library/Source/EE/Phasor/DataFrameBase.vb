'*******************************************************************************************************
'  DataFrameBase.vb - Data frame base class
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/14/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports TVA.Interop

Namespace EE.Phasor

    ' TODO: Move class into its own file...
    Public Class DataFrameParsingState

        Inherits ChannelFrameParsingState

        Public ConfigurationFrame As IConfigurationFrame

        Public Sub New(ByVal cells As DataCellCollection, ByVal cellType As Type, ByVal configurationFrame As IConfigurationFrame)

            MyBase.New(cells, configurationFrame.Cells.Count, cellType)

            Me.ConfigurationFrame = configurationFrame

        End Sub

    End Class

    ' This class represents the protocol independent common implementation of a data frame that can be sent or received from a PMU.
    Public MustInherit Class DataFrameBase

        Inherits ChannelFrameBase
        Implements IDataFrame

        Private m_configurationFrame As IConfigurationFrame

        Protected Sub New(ByVal cells As DataCellCollection)

            MyBase.New(cells)

        End Sub

        Protected Sub New(ByVal cells As DataCellCollection, ByVal timeTag As Unix.TimeTag, ByVal milliseconds As Double, ByVal synchronizationIsValid As Boolean, ByVal dataIsValid As Boolean, ByVal configurationFrame As IConfigurationFrame)

            MyBase.New(cells, timeTag, milliseconds, synchronizationIsValid, dataIsValid)

            m_configurationFrame = configurationFrame

        End Sub

        ' Derived classes are expected to expose a Public Sub New(ByVal configurationFrame As IConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer)
        ' and automatically pass in cells collection and type parameter
        Protected Sub New(ByVal state As DataFrameParsingState, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(state, binaryImage, startIndex)

            m_configurationFrame = ConfigurationFrame

        End Sub

        ' Derived classes are expected to expose a Public Sub New(ByVal dataFrame As IDataFrame)
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

    End Class

End Namespace