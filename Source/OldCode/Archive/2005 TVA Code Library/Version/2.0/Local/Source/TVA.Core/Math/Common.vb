'*******************************************************************************************************
'  TVA.Math.Common.vb - Math Functions
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
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
'       Generated original version of source code.
'  12/29/2005 - Pinal C. Patel
'       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Math).
'  01/04/2006 - J. Ritchie Carroll
'       Added crytographically strong random number generation functions
'  01/24/2006 - J. Ritchie Carroll
'       Added curve fit function (courtesy of Brian Fox from DatAWare client code)
'  11/08/2006 - J. Ritchie Carroll
'       Added standard devitaion and average functions
'  12/07/2006 - J. Ritchie Carroll
'       Added strongly-typed generic Not "comparator" functions (e.g., NotEqualTo)
'
'*******************************************************************************************************

Imports TVA.Common
Imports TVA.Interop

Namespace Math

    ''' <summary>Defines common math functions.</summary>
    Public NotInheritable Class Common

        Private Shared m_randomNumberGenerator As New System.Security.Cryptography.RNGCryptoServiceProvider

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated.

        End Sub

        ''' <summary>Ensures parameter passed to function is not zero. Returns -1
        ''' if <paramref name="testValue">testValue</paramref> is zero.</summary>
        ''' <param name="testValue">Value to test for zero.</param>
        ''' <returns>A non-zero value.</returns>
        Public Shared Function NotZero(ByVal testValue As Double) As Double

            Return NotZero(testValue, -1.0R)

        End Function

        ''' <summary>Ensures parameter passed to function is not zero.</summary>
        ''' <param name="testValue">Value to test for zero.</param>
        ''' <param name="nonZeroReturnValue">Value to return if <paramref name="testValue">testValue</paramref> is 
        ''' zero.</param>
        ''' <returns>A non-zero value.</returns>
        ''' <remarks>To optimize performance, this function does not validate that the notZeroReturnValue is not 
        ''' zero.</remarks>
        Public Shared Function NotZero(ByVal testValue As Double, ByVal nonZeroReturnValue As Double) As Double

            Return IIf(testValue = 0.0R, nonZeroReturnValue, testValue)

        End Function

        ''' <summary>Ensures test parameter passed to function is not equal to the specified value.</summary>
        ''' <param name="testValue">Value to test.</param>
        ''' <param name="notEqualToValue">Value that represents the undesired value (e.g., zero).</param>
        ''' <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is equal 
        ''' to the undesired value.</param>
        ''' <typeparam name="T">Structure or class that implements IEquatable(Of T) (e.g., Double, Single, 
        ''' Integer, etc.).</typeparam>
        ''' <returns>A value not equal to notEqualToValue.</returns>
        ''' <remarks>To optimize performance, this function does not validate that the notEqualToValue is not equal 
        ''' to the alternateValue.</remarks>
        Public Shared Function NotEqualTo(Of T As IEquatable(Of T))(ByVal testValue As T, ByVal notEqualToValue As T, ByVal alternateValue As T) As T

            Return IIf(Of T)(DirectCast(testValue, IEquatable(Of T)).Equals(notEqualToValue), alternateValue, testValue)

        End Function

        ''' <summary>Ensures test parameter passed to function is not less than the specified value.</summary>
        ''' <param name="testValue">Value to test.</param>
        ''' <param name="notLessThanValue">Value that represents the lower limit for the testValue. This value 
        ''' is returned if testValue is less than notLessThanValue.</param>
        ''' <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single, 
        ''' Integer, etc.).</typeparam>
        ''' <returns>A value not less than notLessThanValue.</returns>
        ''' <remarks>If testValue is less than notLessThanValue, then notLessThanValue is returned.</remarks>
        Public Shared Function NotLessThan(Of T As IComparable(Of T))(ByVal testValue As T, ByVal notLessThanValue As T) As T

            Return IIf(Of T)(DirectCast(testValue, IComparable(Of T)).CompareTo(notLessThanValue) < 0, notLessThanValue, testValue)

        End Function

        ''' <summary>Ensures test parameter passed to function is not less than the specified value.</summary>
        ''' <param name="testValue">Value to test.</param>
        ''' <param name="notLessThanValue">Value that represents the lower limit for the testValue.</param>
        ''' <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is  
        ''' less than <paramref name="notLessThanValue">notLessThanValue</paramref>.</param>
        ''' <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single, 
        ''' Integer, etc.).</typeparam>
        ''' <returns>A value not less than notLessThanValue.</returns>
        ''' <remarks>To optimize performance, this function does not validate that the notLessThanValue is not 
        ''' less than the alternateValue.</remarks>
        Public Shared Function NotLessThan(Of T As IComparable(Of T))(ByVal testValue As T, ByVal notLessThanValue As T, ByVal alternateValue As T) As T

            Return IIf(Of T)(DirectCast(testValue, IComparable(Of T)).CompareTo(notLessThanValue) < 0, alternateValue, testValue)

        End Function

        ''' <summary>Ensures test parameter passed to function is not less than or equal to the specified value.</summary>
        ''' <param name="testValue">Value to test.</param>
        ''' <param name="notLessThanOrEqualToValue">Value that represents the lower limit for the testValue.</param>
        ''' <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is 
        ''' less than or equal to <paramref name="notLessThanOrEqualToValue">notLessThanOrEqualToValue</paramref>.</param>
        ''' <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single, 
        ''' Integer, etc.).</typeparam>
        ''' <returns>A value not less than or equal to notLessThanOrEqualToValue.</returns>
        ''' <remarks>To optimize performance, this function does not validate that the notLessThanOrEqualToValue is 
        ''' not less than or equal to the alternateValue.</remarks>
        Public Shared Function NotLessThanOrEqualTo(Of T As IComparable(Of T))(ByVal testValue As T, ByVal notLessThanOrEqualToValue As T, ByVal alternateValue As T) As T

            Return IIf(Of T)(DirectCast(testValue, IComparable(Of T)).CompareTo(notLessThanOrEqualToValue) <= 0, alternateValue, testValue)

        End Function

        ''' <summary>Ensures test parameter passed to function is not greater than the specified value.</summary>
        ''' <param name="testValue">Value to test.</param>
        ''' <param name="notGreaterThanValue">Value that represents the upper limit for the testValue. This 
        ''' value is returned if testValue is greater than notGreaterThanValue.</param>
        ''' <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single, 
        ''' Integer, etc.).</typeparam>
        ''' <returns>A value not greater than notGreaterThanValue.</returns>
        ''' <remarks>If testValue is greater than notGreaterThanValue, then notGreaterThanValue is returned.</remarks>
        Public Shared Function NotGreaterThan(Of T As IComparable(Of T))(ByVal testValue As T, ByVal notGreaterThanValue As T) As T

            Return IIf(Of T)(DirectCast(testValue, IComparable(Of T)).CompareTo(notGreaterThanValue) > 0, notGreaterThanValue, testValue)

        End Function

        ''' <summary>Ensures test parameter passed to function is not greater than the specified value.</summary>
        ''' <param name="testValue">Value to test.</param>
        ''' <param name="notGreaterThanValue">Value that represents the upper limit for the testValue.</param>
        ''' <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is 
        ''' greater than <paramref name="notGreaterThanValue">notGreaterThanValue</paramref>.</param>
        ''' <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single, 
        ''' Integer, etc.).</typeparam>
        ''' <returns>A value not greater than notGreaterThanValue.</returns>
        ''' <remarks>To optimize performance, this function does not validate that the notGreaterThanValue is 
        ''' not greater than the alternateValue</remarks>
        Public Shared Function NotGreaterThan(Of T As IComparable(Of T))(ByVal testValue As T, ByVal notGreaterThanValue As T, ByVal alternateValue As T) As T

            Return IIf(Of T)(DirectCast(testValue, IComparable(Of T)).CompareTo(notGreaterThanValue) > 0, alternateValue, testValue)

        End Function

        ''' <summary>Ensures test parameter passed to function is not greater than or equal to the specified value.</summary>
        ''' <param name="testValue">Value to test.</param>
        ''' <param name="notGreaterThanOrEqualToValue">Value that represents the upper limit for the testValue.</param>
        ''' <param name="alternateValue">Value to return if <paramref name="testValue">testValue</paramref> is 
        ''' greater than or equal to <paramref name="notGreaterThanOrEqualToValue">notGreaterThanOrEqualToValue</paramref>.</param>
        ''' <typeparam name="T">Structure or class that implements IComparable(Of T) (e.g., Double, Single, 
        ''' Integer, etc.).</typeparam>
        ''' <returns>A value not greater than or equal to notGreaterThanOrEqualToValue.</returns>
        ''' <remarks>To optimize performance, this function does not validate that the notGreaterThanOrEqualToValue 
        ''' is not greater than or equal to the alternateValue.</remarks>
        Public Shared Function NotGreaterThanOrEqualTo(Of T As IComparable(Of T))(ByVal testValue As T, ByVal notGreaterThanOrEqualToValue As T, ByVal alternateValue As T) As T

            Return IIf(Of T)(DirectCast(testValue, IComparable(Of T)).CompareTo(notGreaterThanOrEqualToValue) >= 0, alternateValue, testValue)

        End Function

        ''' <summary>Calculates byte length (8-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        ''' <param name="data">Data buffer to perform XOR check-sum on.</param>
        ''' <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        ''' <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to 
        ''' perform XOR check-sum over.</param>
        ''' <returns>Byte length XOR check-sum.</returns>
        Public Shared Function Xor8BitCheckSum(ByVal data As Byte(), ByVal startIndex As Integer, ByVal length As Integer) As Byte

            Dim sum As Byte

            For x As Integer = 0 To length - 1
                sum = sum Xor data(startIndex + x)
            Next

            Return sum

        End Function

        ''' <summary>Calculates word length (16-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        ''' <param name="data">Data buffer to perform XOR check-sum on.</param>
        ''' <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        ''' <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to 
        ''' perform XOR check-sum overs</param>
        ''' <returns>Word length XOR check-sum.</returns>
        <CLSCompliant(False)> _
        Public Shared Function Xor16BitCheckSum(ByVal data As Byte(), ByVal startIndex As Integer, ByVal length As Integer) As UInt16

            Dim sum As UInt16

            For x As Integer = 0 To length - 1 Step 2
                sum = sum Xor BitConverter.ToUInt16(data, startIndex + x)
            Next

            Return sum

        End Function

        ''' <summary>Calculates double-word length (32-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        ''' <param name="data">Data buffer to perform XOR check-sum on.</param>
        ''' <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        ''' <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to 
        ''' perform XOR check-sum over.</param>
        ''' <returns>Double-word length XOR check-sum.</returns>
        <CLSCompliant(False)> _
        Public Shared Function Xor32BitCheckSum(ByVal data As Byte(), ByVal startIndex As Integer, ByVal length As Integer) As UInt32

            Dim sum As UInt32

            For x As Integer = 0 To length - 1 Step 4
                sum = sum Xor BitConverter.ToUInt32(data, startIndex + x)
            Next

            Return sum

        End Function

        ''' <summary>Calculates quad-word length (64-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        ''' <param name="data">Data buffer to perform XOR check-sum on.</param>
        ''' <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        ''' <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to 
        ''' perform XOR check-sum over.</param>
        ''' <returns>Quad-word length XOR check-sum.</returns>
        <CLSCompliant(False)> _
        Public Shared Function Xor64BitCheckSum(ByVal data As Byte(), ByVal startIndex As Integer, ByVal length As Integer) As UInt64

            Dim sum As UInt64

            For x As Integer = 0 To length - 1 Step 8
                sum = sum Xor BitConverter.ToUInt64(data, startIndex + x)
            Next

            Return sum

        End Function

        ''' <summary>Generates a cryptographically strong floating-point random number between zero and one.</summary>
        Public Shared ReadOnly Property RandomNumber() As Double
            Get
                Return BitwiseCast.ToUInt32(RandomInt32) / UInt32.MaxValue
            End Get
        End Property

        ''' <summary>Generates a cryptographically strong random decimal between zero and one.</summary>
        Public Shared ReadOnly Property RandomDecimal() As Decimal
            Get
                Return Convert.ToDecimal(BitwiseCast.ToUInt64(RandomInt64)) / Convert.ToDecimal(UInt64.MaxValue)
            End Get
        End Property

        ''' <summary>Generates a cryptographically strong random integer between specified values.</summary>
        Public Shared ReadOnly Property RandomBetween(ByVal startNumber As Double, ByVal stopNumber As Double) As Double
            Get
                If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
                Return RandomNumber * (stopNumber - startNumber) + startNumber
            End Get
        End Property

        ''' <summary>Generates a cryptographically strong random boolean (i.e., a coin toss).</summary>
        Public Shared ReadOnly Property RandomBoolean() As Boolean
            Get
                Dim value As Byte() = CreateArray(Of Byte)(1)

                m_randomNumberGenerator.GetBytes(value)

                Return IIf(value(0) Mod 2 = 0, True, False)
            End Get
        End Property

        ''' <summary>Generates a cryptographically strong 8-bit random integer.</summary>
        Public Shared ReadOnly Property RandomByte() As Byte
            Get
                Dim value As Byte() = CreateArray(Of Byte)(1)

                m_randomNumberGenerator.GetBytes(value)

                Return value(0)
            End Get
        End Property

        ''' <summary>Generates a cryptographically strong 8-bit random integer between specified values.</summary>
        Public Shared ReadOnly Property RandomByteBetween(ByVal startNumber As Byte, ByVal stopNumber As Byte) As Byte
            Get
                If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
                Return Convert.ToByte(RandomNumber * (stopNumber - startNumber) + startNumber)
            End Get
        End Property

        ''' <summary>Generates a cryptographically strong 16-bit random integer.</summary>
        Public Shared ReadOnly Property RandomInt16() As Int16
            Get
                Dim value As Byte() = CreateArray(Of Byte)(2)

                m_randomNumberGenerator.GetBytes(value)

                Return BitConverter.ToInt16(value, 0)
            End Get
        End Property

        ''' <summary>Generates a cryptographically strong 16-bit random integer between specified values.</summary>
        Public Shared ReadOnly Property RandomInt16Between(ByVal startNumber As Int16, ByVal stopNumber As Int16) As Int16
            Get
                If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
                Return Convert.ToInt16(RandomNumber * (stopNumber - startNumber) + startNumber)
            End Get
        End Property

        ''' <summary>Generates a cryptographically strong 32-bit random integer.</summary>
        Public Shared ReadOnly Property RandomInt32() As Int32
            Get
                Dim value As Byte() = CreateArray(Of Byte)(4)

                m_randomNumberGenerator.GetBytes(value)

                Return BitConverter.ToInt32(value, 0)
            End Get
        End Property

        ''' <summary>Generates a cryptographically strong 32-bit random integer between specified values.</summary>
        Public Shared ReadOnly Property RandomInt32Between(ByVal startNumber As Int32, ByVal stopNumber As Int32) As Int32
            Get
                If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
                Return Convert.ToInt32(RandomNumber * (stopNumber - startNumber) + startNumber)
            End Get
        End Property

        ''' <summary>Generates a cryptographically strong 64-bit random integer.</summary>
        Public Shared ReadOnly Property RandomInt64() As Int64
            Get
                Dim value As Byte() = CreateArray(Of Byte)(8)

                m_randomNumberGenerator.GetBytes(value)

                Return BitConverter.ToInt64(value, 0)
            End Get
        End Property

        ''' <summary>Generates a cryptographically strong 64-bit random integer between specified values.</summary>
        Public Shared ReadOnly Property RandomInt64Between(ByVal startNumber As Int64, ByVal stopNumber As Int64) As Int64
            Get
                If stopNumber < startNumber Then Throw New ArgumentException("stopNumber must be greater than startNumber")
                Return Convert.ToInt64(RandomNumber * (stopNumber - startNumber) + startNumber)
            End Get
        End Property

        ''' <summary>Linear regression algorithm.</summary>
        Public Shared Function CurveFit(ByVal polynomialOrder As Integer, ByVal pointCount As Integer, ByVal xValues As IList(Of Double), ByVal yValues As IList(Of Double)) As Double()

            Dim coeffs(7) As Double
            Dim sum(21) As Double
            Dim v(11) As Double
            Dim b(11, 12) As Double
            Dim p As Double
            Dim divB As Double
            Dim fMultB As Double
            Dim sigma As Double
            Dim ls As Integer
            Dim lb As Integer
            Dim lv As Integer
            Dim i1 As Integer
            Dim i As Integer
            Dim j As Integer
            Dim k As Integer
            Dim l As Integer

            If Not (pointCount >= polynomialOrder + 1) Then Throw New ArgumentException("Point count must be greater than requested polynomial order")
            If Not (polynomialOrder >= 1) And (polynomialOrder <= 7) Then Throw New ArgumentOutOfRangeException("polynomialOrder", "Polynomial order must be between 1 and 7")

            ls = polynomialOrder * 2
            lb = polynomialOrder + 1
            lv = polynomialOrder
            sum(0) = pointCount

            For i = 0 To pointCount - 1
                p = 1.0#
                v(0) = v(0) + yValues(i)
                For j = 1 To lv
                    p = xValues(i) * p
                    sum(j) = sum(j) + p
                    v(j) = v(j) + yValues(i) * p
                Next
                For j = lb To ls
                    p = xValues(i) * p
                    sum(j) = sum(j) + p
                Next
            Next

            For i = 0 To lv
                For k = 0 To lv
                    b(k, i) = sum(k + i)
                Next
            Next

            For k = 0 To lv
                b(k, lb) = v(k)
            Next

            For l = 0 To lv
                divB = b(0, 0)
                For j = l To lb
                    If divB = 0 Then divB = 1
                    b(l, j) = b(l, j) / divB
                Next

                i1 = l + 1

                If i1 - lb < 0 Then
                    For i = i1 To lv
                        fMultB = b(i, l)
                        For j = l To lb
                            b(i, j) = b(i, j) - b(l, j) * fMultB
                        Next
                    Next
                Else
                    Exit For
                End If
            Next

            coeffs(lv) = b(lv, lb)
            i = lv

            Do
                sigma = 0
                For j = i To lv
                    sigma = sigma + b(i - 1, j) * coeffs(j)
                Next j
                i = i - 1
                coeffs(i) = b(i, lb) - sigma
            Loop While i - 1 > 0

            '    For i = 1 To 7
            '        Debug.Print "Coeffs(" & i & ") = " & Coeffs(i)
            '    Next i

            'For i = 1 To 60
            '    '        CalcY(i).TTag = xValues(1) + ((i - 1) / (xValues(pointCount) - xValues(1)))

            '    CalcY(i).TTag = ((i - 1) / 59) * xValues(pointCount) - xValues(1)
            '    CalcY(i).Value = Coeffs(1)

            '    For j = 1 To polynomialOrder
            '        CalcY(i).Value = CalcY(i).Value + Coeffs(j + 1) * CalcY(i).TTag ^ j
            '    Next
            'Next

            '    SSERROR = 0
            '    For i = 1 To pointCount
            '        SSERROR = SSERROR + (yValues(i) - CalcY(i).Value) * (yValues(i) - CalcY(i).Value)
            '    Next i
            '    SSERROR = SSERROR / (pointCount - polynomialOrder)
            '    sError = SSERROR

            ' Return slopes...
            Return coeffs

        End Function

        Public Shared Function StandardDeviation(ByVal dataSample As IList(Of Double)) As Double

            If dataSample.Count = 0 Then Return 0.0R

            Dim sampleAverage As Double = Average(dataSample)
            Dim totalVariance As Double
            Dim dataPointDeviation As Double

            For x As Integer = 0 To dataSample.Count - 1
                dataPointDeviation = dataSample(x) - sampleAverage
                totalVariance += (dataPointDeviation * dataPointDeviation)
            Next

            Return System.Math.Sqrt(totalVariance / dataSample.Count)

        End Function

        Public Shared Function Average(ByVal dataSample As IList(Of Double)) As Double

            If dataSample.Count = 0 Then Return 0.0R

            Dim sampleTotal As Double

            For x As Integer = 0 To dataSample.Count - 1
                sampleTotal += dataSample(x)
            Next

            Return sampleTotal / dataSample.Count

        End Function

    End Class

End Namespace
