'*******************************************************************************************************
'  ConfigurationFrameDraft6.vb - IEEE C37.118 Draft6 Configuration Frame
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

Namespace IeeeC37_118

    <CLSCompliant(False), Serializable()> _
    Public Class ConfigurationFrameDraft6

        Inherits ConfigurationFrame

        Protected Sub New()
        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

        Public Sub New(ByVal frameType As FrameType, ByVal timeBase As Int32, ByVal idCode As Int16, ByVal ticks As Long, ByVal frameRate As Int16, ByVal version As Byte)

            MyBase.New(frameType, timeBase, idCode, ticks, frameRate, version)

        End Sub

        Public Sub New(ByVal parsedFrameHeader As ICommonFrameHeader, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

            MyBase.New(parsedFrameHeader, binaryImage, startIndex)

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            MyBase.New(configurationFrame)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Overrides ReadOnly Property DraftRevision() As DraftRevision
            Get
                Return IeeeC37_118.DraftRevision.Draft6
            End Get
        End Property

    End Class

End Namespace
