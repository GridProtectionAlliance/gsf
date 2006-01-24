'*******************************************************************************************************
'  Tva.Text.Common.vb - Common Text Functions
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/23/2003 - James R Carroll
'       Original version of source code generated
'  01/24/2006 - James R Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.String)
'
'*******************************************************************************************************

Imports System.Text

Namespace Text

    ''' <summary>
    ''' Defines common global functions related to string manipulation
    ''' </summary>
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>
        ''' <para>Return the Unicode number for a character in proper Regular Expression format</para>
        ''' </summary>
        Public Shared Function EncodeRegexChar(ByVal item As Char) As String

            Return "\u" & Convert.ToInt32(item).ToString("x"c).PadLeft(4, "0"c)

        End Function

    End Class

End Namespace
