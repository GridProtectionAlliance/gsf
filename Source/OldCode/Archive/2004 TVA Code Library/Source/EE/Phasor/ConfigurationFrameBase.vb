'***********************************************************************
'  ConfigurationFrameBase.vb - Configuration frame base class
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

    ' This class represents the protocol independent common implementation of a configuration frame that can be sent or received from a PMU.
    Public MustInherit Class ConfigurationFrameBase

        Inherits ChannelFrameBase
        'Implements IDataFrame

        'Private m_idCode() As Int16
        'Private m_configurationCells As ConfigurationCellCollection

        'Protected Sub New()

        '    MyBase.New()

        '    m_configurationCells = New ConfigurationCellCollection

        'End Sub

        'Protected Sub New(ByVal timeTag As Unix.TimeTag, ByVal milliseconds As Double, ByVal synchronizationIsValid As Boolean, ByVal dataIsValid As Boolean, ByVal dataImage As Byte(), ByVal configurationFrame As IConfigurationFrame, ByVal configurationCells As ConfigurationCellCollection)

        '    MyBase.New(timeTag, milliseconds, synchronizationIsValid, dataIsValid, dataImage)

        '    m_configurationFrame = configurationFrame
        '    m_configurationCells = configurationCells

        'End Sub

        '' Dervied classes are expected to expose a Public Sub New(ByVal phasorDataFrame As IDataFrame)
        'Protected Sub New(ByVal phasorDataFrame As IDataFrame)

        '    Me.New(phasorDataFrame.TimeTag, phasorDataFrame.Milliseconds, phasorDataFrame.SynchronizationIsValid, phasorDataFrame.DataIsValid, _
        '            phasorDataFrame.DataImage, phasorDataFrame.ConfigurationFrame, phasorDataFrame.ConfigurationCells)

        'End Sub

        'Public Overridable Property ConfigurationFrame() As IConfigurationFrame Implements IDataFrame.ConfigurationFrame
        '    Get
        '        Return m_configurationFrame
        '    End Get
        '    Set(ByVal Value As IConfigurationFrame)
        '        m_configurationFrame = Value
        '    End Set
        'End Property

        'Public ReadOnly Property ConfigurationCells() As ConfigurationCellCollection Implements IConfigurationFrame.ConfigurationCells
        '    Get
        '        Return m_configurationCells
        '    End Get
        'End Property

        'Public Overrides ReadOnly Property Name() As String
        '    Get
        '        Return "TVA.EE.Phasor.DataFrameBase"
        '    End Get
        'End Property

        'Public Overrides ReadOnly Property DataLength() As Int16
        '    Get
        '        Dim length As Int16

        '        For x As Integer = 0 To m_dataCells.Count - 1
        '            length += m_dataCells(x).BinaryLength
        '        Next

        '        Return length
        '    End Get
        'End Property

        'Public Overrides Property DataImage() As Byte()
        '    Get
        '        Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), DataLength)
        '        Dim index As Integer

        '        For x As Integer = 0 To m_dataCells.Count - 1
        '            Array.Copy(m_dataCells(x).BinaryImage, 0, buffer, index, m_dataCells(x).BinaryLength)
        '            index += m_dataCells(x).BinaryLength
        '        Next

        '        Return buffer
        '    End Get
        '    Set(ByVal Value() As Byte)
        '        ' TODO: may be possible provide a generic cell creation implementation - especially if configuration frame is available...
        '        Throw New NotImplementedException
        '    End Set
        'End Property

    End Class

End Namespace