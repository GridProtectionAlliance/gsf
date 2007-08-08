'*******************************************************************************************************
'  ConfigurationFrame.vb - Protocol Independent Configuration Frame
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
Imports PhasorProtocols

<CLSCompliant(False), Serializable()> _
Public Class ConfigurationFrame

    Inherits ConfigurationFrameBase

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

    End Sub

    Public Sub New(ByVal idCode As UInt16, ByVal ticks As Long, ByVal frameRate As Int16)

        MyBase.New(idCode, New ConfigurationCellCollection(Integer.MaxValue, False), ticks, frameRate)

    End Sub

    Public Overrides ReadOnly Property DerivedType() As System.Type
        Get
            Return Me.GetType
        End Get
    End Property

    Public Shadows ReadOnly Property Cells() As ConfigurationCellCollection
        Get
            Return MyBase.Cells
        End Get
    End Property

End Class
