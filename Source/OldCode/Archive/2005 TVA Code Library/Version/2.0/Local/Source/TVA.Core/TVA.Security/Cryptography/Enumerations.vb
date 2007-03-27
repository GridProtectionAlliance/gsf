'*******************************************************************************************************
'  Enumerations.vb - Global enumerations for this namespace
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
'  07/11/2007 - J. Ritchie Carroll
'       Moved all namespace level enumerations into "Enumerations.vb" file
'
'*******************************************************************************************************

Namespace Security.Cryptography

    ''' <summary>Cryptographic Strength Enumeration</summary>
    ''' <remarks>
    ''' <para>
    ''' Encryption algorithms are cumulative, the levels represent tradeoffs on speed vs. cipher strength - level 1
    ''' will have the fastest encryption speed with the simplest encryption strength - level 4 will have the
    ''' strongest cumulative encryption strength with the slowest encryption speed.
    ''' </para>
    ''' </remarks>
    Public Enum EncryptLevel
        ''' <summary>Use no encryption</summary>
        None
        ''' <summary>Adds simple multi-alogorithm XOR based encryption</summary>
        ''' <remarks>This is the fastest and weakest level of encyption</remarks>
        Level1
        ''' <summary>Adds TripleDES based encryption</summary>
        Level2
        ''' <summary>Adds RC2 based encryption</summary>
        Level3
        ''' <summary>Adds RijndaelManaged based enryption</summary>
        ''' <remarks>This is the slowest and strongest level of encyption</remarks>
        Level4
    End Enum

End Namespace