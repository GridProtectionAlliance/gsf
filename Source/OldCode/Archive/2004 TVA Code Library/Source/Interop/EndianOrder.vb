'***********************************************************************
'  EndianOrder.vb - Endian Byte Order Interoperability Classes
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Namespace Interop

    Public Class EndianOrder

        Private Sub New()

            ' This is a shared function class not meant for instantiation

        End Sub

        ' This function behaves just like Array.Copy but takes a little-endian source array and copies it in big-endian order,
        ' or if the source array is big-endian it will copy it in little-endian order
        Public Shared Sub SwapCopy(ByVal sourceArray As Array, ByVal sourceIndex As Integer, ByVal destinationArray As Array, ByVal destinationIndex As Integer, ByVal length As Integer)

            For x As Integer = sourceIndex To sourceIndex + length - 1
                destinationArray.SetValue(sourceArray.GetValue(x), destinationIndex + length - 1 - (x - sourceIndex))
            Next

        End Sub

    End Class

End Namespace