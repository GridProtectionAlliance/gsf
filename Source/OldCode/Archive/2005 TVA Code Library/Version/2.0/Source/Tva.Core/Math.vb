'*******************************************************************************************************
'  Tva.Math.vb - Math Functions
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

''' <summary>
''' Defines static math functions.
''' </summary>
''' <remarks></remarks>
Public NotInheritable Class Math

    Private Shared m_randomNumberGenerator As New Security.Cryptography.RNGCryptoServiceProvider

    Private Sub New()

        ' This class contains only global functions and is not meant to be instantiated

    End Sub

    ''' <summary>
    ''' Calculates word length XOR check-sum on specified portion of a buffer.
    ''' </summary>
    ''' <param name="data">To be provided.</param>
    ''' <param name="startIndex">To be provided.</param>
    ''' <param name="length">To be provided.</param>
    ''' <returns>To be provided.</returns>
    ''' <remarks></remarks>
    Public Shared Function XorCheckSum(ByVal data As Byte(), ByVal startIndex As Integer, ByVal length As Integer) As Int16

        Dim sum As Int16

        For x As Integer = 0 To length - 1 Step 2
            sum = sum Xor BitConverter.ToInt16(data, startIndex + x)
        Next

        Return sum

    End Function

    ''' <summary>
    ''' <para>Generates a cryptographically strong random number between zero and one</para>
    ''' </summary>
    Public Shared ReadOnly Property RandomNumber() As Double
        Get
            Return Bit.ToUInt64(RandomInt64) / UInt64.MaxValue
        End Get
    End Property

    ''' <summary>
    ''' <para>Generates a cryptographically strong random boolean (e.g., coin toss)</para>
    ''' </summary>
    Public Shared ReadOnly Property RandomBoolean() As Boolean
        Get
            Dim value As Byte() = Array.CreateInstance(GetType(Byte), 1)

            m_randomNumberGenerator.GetBytes(value)

            Return IIf(value(0) Mod 2 = 0, True, False)
        End Get
    End Property

    ''' <summary>
    ''' <para>Generates a cryptographically strong 8-bit random number</para>
    ''' </summary>
    Public Shared ReadOnly Property RandomByte() As Byte
        Get
            Dim value As Byte() = Array.CreateInstance(GetType(Byte), 1)

            m_randomNumberGenerator.GetBytes(value)

            Return value(0)
        End Get
    End Property

    ''' <summary>
    ''' <para>Generates a cryptographically strong 8-bit random number between specified values</para>
    ''' </summary>
    Public Shared ReadOnly Property RandomByteBetween(ByVal startNumber As Byte, ByVal stopNumber As Byte) As Byte
        Get
            If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
            Return Convert.ToByte(RandomNumber * (stopNumber - startNumber) + startNumber)
        End Get
    End Property

    ''' <summary>
    ''' <para>Generates a cryptographically strong 16-bit random number</para>
    ''' </summary>
    Public Shared ReadOnly Property RandomInt16() As Int16
        Get
            Dim value As Byte() = Array.CreateInstance(GetType(Byte), 2)

            m_randomNumberGenerator.GetBytes(value)

            Return BitConverter.ToInt16(value, 0)
        End Get
    End Property

    ''' <summary>
    ''' <para>Generates a cryptographically strong 16-bit random number between specified values</para>
    ''' </summary>
    Public Shared ReadOnly Property RandomInt16Between(ByVal startNumber As Int16, ByVal stopNumber As Int16) As Int16
        Get
            If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
            Return Convert.ToInt16(RandomNumber * (stopNumber - startNumber) + startNumber)
        End Get
    End Property

    ''' <summary>
    ''' <para>Generates a cryptographically strong 32-bit random number</para>
    ''' </summary>
    Public Shared ReadOnly Property RandomInt32() As Int32
        Get
            Dim value As Byte() = Array.CreateInstance(GetType(Byte), 4)

            m_randomNumberGenerator.GetBytes(value)

            Return BitConverter.ToInt32(value, 0)
        End Get
    End Property

    ''' <summary>
    ''' <para>Generates a cryptographically strong 32-bit random number between specified values</para>
    ''' </summary>
    Public Shared ReadOnly Property RandomInt32Between(ByVal startNumber As Int32, ByVal stopNumber As Int32) As Int32
        Get
            If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
            Return Convert.ToInt32(RandomNumber * (stopNumber - startNumber) + startNumber)
        End Get
    End Property

    ''' <summary>
    ''' <para>Generates a cryptographically strong 64-bit random number</para>
    ''' </summary>
    Public Shared ReadOnly Property RandomInt64() As Int64
        Get
            Dim value As Byte() = Array.CreateInstance(GetType(Byte), 8)

            m_randomNumberGenerator.GetBytes(value)

            Return BitConverter.ToInt64(value, 0)
        End Get
    End Property

    ''' <summary>
    ''' <para>Generates a cryptographically strong 64-bit random number between specified values</para>
    ''' </summary>
    Public Shared ReadOnly Property RandomInt32Between(ByVal startNumber As Int64, ByVal stopNumber As Int64) As Int64
        Get
            If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
            Return Convert.ToInt64(RandomNumber * (stopNumber - startNumber) + startNumber)
        End Get
    End Property

    ''''</summary>
    ''' <summary>
    ''' Class to temporarily cache composite values until all values been received so that a compound value can be created.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CompositeValues

        ''' <summary>
        ''' To be provided.
        ''' </summary>
        ''' <remarks></remarks>
        Private Structure CompositeValue

            Public Value As Double
            Public Received As Boolean

        End Structure

        Private m_compositeValues As CompositeValue()
        Private m_allReceived As Boolean

        ''' <summary>
        ''' This Constructor creates an instance of an array to store all the composite values.
        ''' </summary>
        ''' <param name="count">To be provided.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal count As Integer)

            m_compositeValues = Array.CreateInstance(GetType(CompositeValue), count)

        End Sub

        ''' <summary>
        ''' Gets or sets the composite value at the specified index in composite value collection.
        ''' </summary>
        ''' <param name="index">The zero-based index of the composite value to get.</param>
        ''' <value></value>
        ''' <returns>The composite value at the specified index in composite value collection.</returns>
        ''' <remarks></remarks>
        Default Public Property Value(ByVal index As Integer) As Double
            Get
                Return m_compositeValues(index).Value
            End Get
            Set(ByVal value As Double)
                With m_compositeValues(index)
                    .Value = value
                    .Received = True
                End With
            End Set
        End Property

        ''' <summary>
        ''' Gets a boolean value indicating if composite value at the specified index is received.
        ''' </summary>
        ''' <param name="index">The zero-based index of the composite value.</param>
        ''' <value></value>
        ''' <returns>True if composite value at the specified index is received; otherwise, False.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Received(ByVal index As Integer) As Boolean
            Get
                Return m_compositeValues(index).Received
            End Get
        End Property

        ''' <summary>
        ''' Gets the number of compisite values in the composite value collection.
        ''' </summary>
        ''' <value></value>
        ''' <returns>To be provided.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count() As Integer
            Get
                Return m_compositeValues.Length
            End Get
        End Property

        ''' <summary>
        ''' Gets a boolean value indicating if all composite values are received.
        ''' </summary>
        ''' <value></value>
        ''' <returns>True if all composite values are received; otherwise, False.</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property AllReceived() As Boolean
            Get
                If m_allReceived Then
                    Return True
                Else
                    Dim allValuesReceived As Boolean = True

                    For x As Integer = 0 To m_compositeValues.Length - 1
                        If Not m_compositeValues(x).Received Then
                            allValuesReceived = False
                            Exit For
                        End If
                    Next

                    If allValuesReceived Then m_allReceived = True
                    Return allValuesReceived
                End If
            End Get
        End Property

    End Class

End Class
