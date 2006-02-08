'*******************************************************************************************************
'  Tva.Interop.BitwiseCast.vb - Bitwise Data Type Conversion Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/05/2006 - James R Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

Namespace Interop

    ''' <summary>
    ''' <para>Defines specialized bitwise data type conversion functions</para>
    ''' </summary>
    Public NotInheritable Class BitwiseCast

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>
        ''' <para>Performs proper bitwise conversion between unsigned and signed value</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>This function is useful because Convert.ToInt16 will throw an OverflowException for values greater than Int16.MaxValue.</para>
        ''' <para>For example, this function correctly converts unsigned 16-bit integer 65535 (i.e., UInt16.MaxValue) to signed 16-bit integer 32767 (i.e., Int16.MaxValue).</para>
        ''' </remarks>
        <CLSCompliant(False)> _
        Public Shared ReadOnly Property ToInt16(ByVal unsignedInt As UInt16) As Int16
            Get
                Return BitConverter.ToInt16(BitConverter.GetBytes(unsignedInt), 0)
            End Get
        End Property

        ''' <summary>
        ''' <para>Performs proper bitwise conversion between unsigned and signed value</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>This function is useful because Convert.ToInt32 will throw an OverflowException for values greater than Int32.MaxValue.</para>
        ''' <para>For example, this function correctly converts unsigned 32-bit integer 4294967295 (i.e., UInt16.MaxValue) to signed 32-bit integer 2147483647 (i.e., Int32.MaxValue).</para>
        ''' </remarks>
        <CLSCompliant(False)> _
        Public Shared ReadOnly Property ToInt32(ByVal unsignedInt As UInt32) As Int32
            Get
                Return BitConverter.ToInt32(BitConverter.GetBytes(unsignedInt), 0)
            End Get
        End Property

        ''' <summary>
        ''' <para>Performs proper bitwise conversion between unsigned and signed value</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>This function is useful because Convert.ToInt64 will throw an OverflowException for values greater than Int64.MaxValue.</para>
        ''' <para>For example, this function correctly converts unsigned 64-bit integer 18446744073709551615 (i.e., UInt64.MaxValue) to signed 64-bit integer 9223372036854775807 (i.e., Int64.MaxValue).</para>
        ''' </remarks>
        <CLSCompliant(False)> _
        Public Shared ReadOnly Property ToInt64(ByVal unsignedInt As UInt64) As Int64
            Get
                Return BitConverter.ToInt64(BitConverter.GetBytes(unsignedInt), 0)
            End Get
        End Property

        ''' <summary>
        ''' <para>Performs proper bitwise conversion between signed and unsigned value</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>This function is useful because Convert.ToUInt16 will throw an OverflowException for values less than zero.</para>
        ''' <para>For example, this function correctly converts signed 16-bit integer -32768 (i.e., Int16.MinValue) to unsigned 16-bit integer 0 (i.e., UInt16.MinValue).</para>
        ''' </remarks>
        <CLSCompliant(False)> _
        Public Shared ReadOnly Property ToUInt16(ByVal signedInt As Int16) As UInt16
            Get
                Return BitConverter.ToUInt16(BitConverter.GetBytes(signedInt), 0)
            End Get
        End Property

        ''' <summary>
        ''' <para>Performs proper bitwise conversion between signed and unsigned value</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>This function is useful because Convert.ToUInt32 will throw an OverflowException for values less than zero.</para>
        ''' <para>For example, this function correctly converts signed 32-bit integer -2147483648 (i.e., Int32.MinValue) to unsigned 32-bit integer 0 (i.e., UInt32.MinValue).</para>
        ''' </remarks>
        <CLSCompliant(False)> _
        Public Shared ReadOnly Property ToUInt32(ByVal signedInt As Int32) As UInt32
            Get
                Return BitConverter.ToUInt32(BitConverter.GetBytes(signedInt), 0)
            End Get
        End Property

        ''' <summary>
        ''' <para>Performs proper bitwise conversion between signed and unsigned value</para>
        ''' </summary>
        ''' <remarks>
        ''' <para>This function is useful because Convert.ToUInt64 will throw an OverflowException for values less than zero.</para>
        ''' <para>For example, this function correctly converts signed 64-bit integer -9223372036854775808 (i.e., Int64.MinValue) to unsigned 64-bit integer 0 (i.e., UInt64.MinValue).</para>
        ''' </remarks>
        <CLSCompliant(False)> _
        Public Shared ReadOnly Property ToUInt64(ByVal signedInt As Int64) As UInt64
            Get
                Return BitConverter.ToUInt64(BitConverter.GetBytes(signedInt), 0)
            End Get
        End Property

    End Class

End Namespace
