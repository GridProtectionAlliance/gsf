'***********************************************************************
'  DataFrame.vb - IEEE1344 Data Frame
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
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
Imports TVA.Shared.Bit
Imports TVA.Shared.DateTime
Imports TVA.Compression.Common

Namespace EE.Phasor.IEEE1344

    ' This class represents a data frame that will be sent from a PMU during its real time data transmission.
    Public Class DataFrame

        Public Const BinaryLength As Integer = 16

        Public Sub New()

        End Sub

        Public ReadOnly Property BinaryImage() As Byte()
            Get
            End Get
        End Property

    End Class

End Namespace