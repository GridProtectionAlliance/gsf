'*******************************************************************************************************
'  Tva.Math.vb - Common math functions / classes
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
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.Bit)
'
'*******************************************************************************************************

''' <summary>
''' Defines global Math functions.
''' </summary>
''' <remarks></remarks>
Public Class Math

    Private Sub New()
        ' This class contains only global functions and is not meant to be instantiated
    End Sub

    ''' <summary>
    ''' Calculates word length XOR check-sum on specified portion of a buffer.
    ''' </summary>
    ''' <param name="data">A parameter value of Byte datatype.</param>
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
    ''' Converts a double value and properly convert it back into a signed 16-bit integer.
    ''' </summary>
    ''' <param name="source">The double value to be converted.</param>
    ''' <returns>A 16-bit signed integer.</returns>
    ''' <remarks></remarks>
    Public Shared Function ParseInt16(ByVal source As Double) As Int16

        Try
            Return BitConverter.ToInt16(BitConverter.GetBytes(Convert.ToUInt16(source)), 0)
        Catch
            Return 0
        End Try

    End Function



    ''''</summary>
    ''' <summary>
    ''' Class to temporarily cache composite values until all values been received so that a compound value can be created.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CompositeValues

        Private Structure CompositeValue

            Public Value As Double
            Public Received As Boolean

        End Structure

        Private m_compositeValues As CompositeValue()
        Private m_allReceived As Boolean

        ''''<summary> 
        '''' This Constructor creates an instance of an array to store all the composite values.
        ''''</summary>
        Public Sub New(ByVal count As Integer)

            m_compositeValues = Array.CreateInstance(GetType(CompositeValue), count)

        End Sub

        ''''<summary> 
        ''''Gets or sets the Composite Values to create a Compound value 
        ''''</summary>
        '''' <value>Composite Value</value>
        '''' <remarks> The value must be double.
        '''' </remarks>
        Default Public Property Value(ByVal index As Integer) As Double
            Get
                Return m_compositeValues(index).Value
            End Get
            Set(ByVal Value As Double)
                With m_compositeValues(index)
                    .Value = Value
                    .Received = True
                End With
            End Set
        End Property

        ''''<summary> 
        ''''Checks to see if Composite Value is received 
        ''''</summary>
        '''' <value>Readonly value</value>
        Public ReadOnly Property Received(ByVal index As Integer) As Boolean
            Get
                Return m_compositeValues(index).Received
            End Get
        End Property

        Public ReadOnly Property Count() As Integer
            Get
                Return m_compositeValues.Length
            End Get
        End Property

        ''''<summary> 
        ''''Checks to see if all Composite values are received 
        ''''</summary>
        '''' <value>Readonly value</value>
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
