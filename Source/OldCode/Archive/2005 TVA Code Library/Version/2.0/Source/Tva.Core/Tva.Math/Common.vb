'*******************************************************************************************************
'  Tva.Math.Common.vb - Math Functions
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
'  11/12/2004 - James R Carroll
'       Original version of source code generated
'  12/29/2005 - Pinal C Patel
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Math)
'  01/04/2006 - James R Carroll
'       Added crytographically strong random number generation functions
'
'*******************************************************************************************************

Namespace Math

    ''' <summary>
    ''' Defines common math functions
    ''' </summary>
    Public NotInheritable Class Common

        Private Shared m_randomNumberGenerator As New Security.Cryptography.RNGCryptoServiceProvider

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>
        ''' <para>Calculates word length XOR check-sum on specified portion of a buffer.</para>
        ''' </summary>
        ''' <param name="data">Data buffer to perform XOR check-sum on</param>
        ''' <param name="startIndex">Start index in data buffer to begin XOR check-sum</param>
        ''' <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to perform XOR check-sum over</param>
        ''' <returns>Word length XOR check-sum</returns>
        Public Shared Function XorCheckSum(ByVal data As Byte(), ByVal startIndex As Integer, ByVal length As Integer) As Int16

            Dim sum As Int16

            For x As Integer = 0 To length - 1 Step 2
                sum = sum Xor BitConverter.ToInt16(data, startIndex + x)
            Next

            Return sum

        End Function

        ''' <summary>
        ''' <para>Generates a cryptographically strong floating-point random number between zero and one</para>
        ''' </summary>
        Public Shared ReadOnly Property RandomNumber() As Double
            Get
                Return Convert.ToUInt64(RandomInt64) / UInt64.MaxValue
            End Get
        End Property

        ''' <summary>
        ''' <para>Generates a cryptographically strong random boolean (i.e., coin toss)</para>
        ''' </summary>
        Public Shared ReadOnly Property RandomBoolean() As Boolean
            Get
                Dim value As Byte() = Array.CreateInstance(GetType(Byte), 1)

                m_randomNumberGenerator.GetBytes(value)

                Return IIf(value(0) Mod 2 = 0, True, False)
            End Get
        End Property

        ''' <summary>
        ''' <para>Generates a cryptographically strong 8-bit random integer</para>
        ''' </summary>
        Public Shared ReadOnly Property RandomByte() As Byte
            Get
                Dim value As Byte() = Array.CreateInstance(GetType(Byte), 1)

                m_randomNumberGenerator.GetBytes(value)

                Return value(0)
            End Get
        End Property

        ''' <summary>
        ''' <para>Generates a cryptographically strong 8-bit random integer between specified values</para>
        ''' </summary>
        Public Shared ReadOnly Property RandomByteBetween(ByVal startNumber As Byte, ByVal stopNumber As Byte) As Byte
            Get
                If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
                Return Convert.ToByte(RandomNumber * (stopNumber - startNumber) + startNumber)
            End Get
        End Property

        ''' <summary>
        ''' <para>Generates a cryptographically strong 16-bit random integer</para>
        ''' </summary>
        Public Shared ReadOnly Property RandomInt16() As Int16
            Get
                Dim value As Byte() = Array.CreateInstance(GetType(Byte), 2)

                m_randomNumberGenerator.GetBytes(value)

                Return BitConverter.ToInt16(value, 0)
            End Get
        End Property

        ''' <summary>
        ''' <para>Generates a cryptographically strong 16-bit random integer between specified values</para>
        ''' </summary>
        Public Shared ReadOnly Property RandomInt16Between(ByVal startNumber As Int16, ByVal stopNumber As Int16) As Int16
            Get
                If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
                Return Convert.ToInt16(RandomNumber * (stopNumber - startNumber) + startNumber)
            End Get
        End Property

        ''' <summary>
        ''' <para>Generates a cryptographically strong 32-bit random integer</para>
        ''' </summary>
        Public Shared ReadOnly Property RandomInt32() As Int32
            Get
                Dim value As Byte() = Array.CreateInstance(GetType(Byte), 4)

                m_randomNumberGenerator.GetBytes(value)

                Return BitConverter.ToInt32(value, 0)
            End Get
        End Property

        ''' <summary>
        ''' <para>Generates a cryptographically strong 32-bit random integer between specified values</para>
        ''' </summary>
        Public Shared ReadOnly Property RandomInt32Between(ByVal startNumber As Int32, ByVal stopNumber As Int32) As Int32
            Get
                If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
                Return Convert.ToInt32(RandomNumber * (stopNumber - startNumber) + startNumber)
            End Get
        End Property

        ''' <summary>
        ''' <para>Generates a cryptographically strong 64-bit random integer</para>
        ''' </summary>
        Public Shared ReadOnly Property RandomInt64() As Int64
            Get
                Dim value As Byte() = Array.CreateInstance(GetType(Byte), 8)

                m_randomNumberGenerator.GetBytes(value)

                Return BitConverter.ToInt64(value, 0)
            End Get
        End Property

        ''' <summary>
        ''' <para>Generates a cryptographically strong 64-bit random integer between specified values</para>
        ''' </summary>
        Public Shared ReadOnly Property RandomInt32Between(ByVal startNumber As Int64, ByVal stopNumber As Int64) As Int64
            Get
                If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
                Return Convert.ToInt64(RandomNumber * (stopNumber - startNumber) + startNumber)
            End Get
        End Property

    End Class

End Namespace
