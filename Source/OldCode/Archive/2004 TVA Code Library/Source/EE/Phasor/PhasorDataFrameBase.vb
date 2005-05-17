'***********************************************************************
'  PhasorDataFrameBase.vb - Phasor data frame base class
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

    ' This class represents the protocol independent common implementation of a phasor data frame that can be sent or received from a PMU.
    Public MustInherit Class PhasorDataFrameBase

        Inherits ChannelFrameBase
        Implements IPhasorDataFrame

        Private m_phasorDataCells As PhasorDataCellCollection

        ' Create phasor data frame from other phasor data frame
        ' Note: This method is expected to be implemented as a public shared method in derived class automatically passing in phasorDataFrameType
        ' Dervied class must expose a Public Sub New(ByVal phasorDataFrame As IPhasorDataFrame)
        Protected Shared Shadows Function CreateFrom(ByVal phasorDataFrameType As Type, ByVal phasorDataFrame As IPhasorDataFrame) As IPhasorDataFrame

            Return CType(Activator.CreateInstance(phasorDataFrameType, New Object() {phasorDataFrame}), IPhasorDataFrame)

        End Function

        Protected Sub New(ByVal frequencyValue As IFrequencyValue)

            MyBase.New()

            m_phasorDataCells = New PhasorDataCellCollection

        End Sub

        Protected Sub New(ByVal timeTag As Unix.TimeTag, ByVal milliseconds As Double, ByVal synchronizationIsValid As Boolean, ByVal dataIsValid As Boolean, ByVal dataImage As Byte(), ByVal phasorDataCells As PhasorDataCellCollection)

            MyBase.New(timeTag, milliseconds, synchronizationIsValid, dataIsValid, dataImage)

            m_phasorDataCells = phasorDataCells

        End Sub

        Protected Sub New(ByVal phasorDataFrame As IPhasorDataFrame)

            Me.New(phasorDataFrame.TimeTag, phasorDataFrame.Milliseconds, phasorDataFrame.SynchronizationIsValid, phasorDataFrame.DataIsValid, _
                    phasorDataFrame.DataImage, phasorDataFrame.PhasorDataCells)

        End Sub

        Public ReadOnly Property PhasorDataCells() As PhasorDataCellCollection Implements IPhasorDataFrame.PhasorDataCells
            Get
                Return m_phasorDataCells
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "TVA.EE.Phasor.PhasorDataFrameBase"
            End Get
        End Property

        Public Overrides ReadOnly Property DataLength() As Int16
            Get
                Dim length As Int16

                For x As Integer = 0 To m_phasorDataCells.Count - 1
                    length += m_phasorDataCells(x).BinaryLength
                Next

                Return length
            End Get
        End Property

        Public Overrides Property DataImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)
                Dim index As Integer

                For x As Integer = 0 To m_phasorDataCells.Count - 1
                    Array.Copy(m_phasorDataCells(x).BinaryImage, 0, buffer, index, m_phasorDataCells(x).BinaryLength)
                    index += m_phasorDataCells(x).BinaryLength
                Next

                Return buffer
            End Get
            Set(ByVal Value() As Byte)
                Throw New NotImplementedException
            End Set
        End Property

    End Class

End Namespace