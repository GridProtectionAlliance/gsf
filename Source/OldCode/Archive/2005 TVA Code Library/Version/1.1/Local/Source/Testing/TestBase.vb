' Steven Lowe - 2003
' 06/10/2004 JRC - Integrated into shared code library namespace TVA.Testing - renamed TvaTestClass to TestBase
' 06/10/2004 JRC - Made method name lookup beginning with "test" case insensitive
' 06/14/2004 JRC - Added a "test progress" method to allow logger to handle long tests (progress bar, DoEvents, etc.)
' 06/14/2004 JRC - Added a "test path" property for test components that need to test with files
' 06/15/2004 JRC - Added optional "RandomOrder" parameter to RunTests method to allow completely random test runs
' 09/02/2004 SAL - checked for and expose InnerException in test-case execution
Option Explicit On 

'	TestBase
'
'	This class provides a basic testing framework for inheritance by other test
'	classes. Its purpose is to provide an easy way to write tests for code
'	libraries independent of other projects, and to support automated regression
'	and stress testing.
'
'	To use this class, inherit from it and implement instance-level (not Shared)
'	public subs with no arguments whose names begin with "test". Each of these
'	subs should test one thing, feature, function, scenario, et al. Results of the
'	test are reported generically using the Assert() function to test a result
'	condition. Any number of conditions may be tested. For example:
'
'		Public Class MyTestClass
'			Inherits TestBase
'			Public Sub testSomething()
'				...	'do something
'				Assert(someCondition)	'test some result(s)
'				...	'do something else
'				Assert(someOtherCondition)	'test some more result(s)
'			End Sub
'		End Class
'
Imports System.Reflection
Imports TVA.Shared.Common

Namespace Testing

    Public Enum EventType As Integer
        Failure     ' report failure of a test method assertion
        Success     ' report success of a test method assertion
        Starting    ' report starting of a test class object
        Ending      ' report completion of a test class object
        Status      ' report descriptive progress
    End Enum

    '	ILogger
    '
    '	This interface defines a message for the logging of testing events;
    '	Any object that implements this interface may receive testing events
    Public Interface ILogger

        Sub LogEvent(ByVal TestName As String, ByVal Type As EventType, ByVal NumTests As Integer, ByVal NumFailures As Integer, ByVal ErrMsg As String)
        Sub TestProgress(ByVal Completed As Long, ByVal Total As Long)
        ReadOnly Property TestPath() As String

    End Interface

    Public MustInherit Class TestBase

        ' This class compares two "System.Reflection.MethodInfo" class instances by name (used for sorting)
        Private Class MethodInfoComparer

            Implements IComparer

            Private Shared DefaultComparer As MethodInfoComparer

            Public Shared ReadOnly Property [Default]() As MethodInfoComparer
                Get
                    If DefaultComparer Is Nothing Then DefaultComparer = New MethodInfoComparer
                    Return DefaultComparer
                End Get
            End Property

            Public Function Compare(ByVal MethodInfoA As MethodInfo, ByVal MethodInfoB As MethodInfo) As Integer

                Return String.Compare(MethodInfoA.Name, MethodInfoB.Name, True)

            End Function

            Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare

                If TypeOf x Is MethodInfo And TypeOf y Is MethodInfo Then
                    Return Compare(DirectCast(x, MethodInfo), DirectCast(y, MethodInfo))
                Else
                    Throw New ArgumentException("MethodInfoComparer can only compare instances of ""System.Reflection.MethodInfo""")
                End If

            End Function

        End Class

        Protected iNumberOfTests As Integer = 0
        Protected iNumberOfSuccesses As Integer = 0
        Protected sCurrentTestMethodName As String
        Protected iAssertionIndex As Integer = 0
        Protected oLogger As ILogger 'receives test-event log results
        Protected bLogOnlyFailures As Boolean = False 'by default, successes and failures are sent to logger

        Public ReadOnly Property TestCount() As Integer
            Get
                Return iNumberOfTests
            End Get
        End Property

        Public ReadOnly Property SuccessfulTests() As Integer
            Get
                Return iNumberOfSuccesses
            End Get
        End Property

        Public ReadOnly Property FailedTests() As Integer
            Get
                Return (iNumberOfTests - iNumberOfSuccesses)
            End Get
        End Property

        'return number between 0 and 1 inclusive
        Public ReadOnly Property PercentSuccessful() As Double
            Get
                If iNumberOfTests > 0 Then
                    Return (CDbl(iNumberOfSuccesses) / CDbl(iNumberOfTests))
                End If
                Return 0.0
            End Get
        End Property

        'return number between 0 and 1 inclusive
        Public ReadOnly Property PercentFailed() As Double
            Get
                If iNumberOfTests > 0 Then
                    Return (CDbl(iNumberOfTests - iNumberOfSuccesses) / CDbl(iNumberOfTests))
                End If
                Return 0.0
            End Get
        End Property

        'return current test method name
        Public ReadOnly Property CurrentTestName() As String
            Get
                Return sCurrentTestMethodName
            End Get
        End Property

        'set to false to see all events, not just failure events
        Public Property LogOnlyFailures() As Boolean
            Get
                Return bLogOnlyFailures
            End Get
            Set(ByVal Value As Boolean)
                bLogOnlyFailures = Value
            End Set
        End Property

        Public Property Logger() As ILogger
            Get
                Return oLogger
            End Get
            Set(ByVal Value As ILogger)
                oLogger = Value
            End Set
        End Property

        'this function logs a test assertion success or failure to an external ILogger object
        Protected Sub LogEvent(ByVal Type As EventType, Optional ByVal Tag As String = Nothing, Optional ByVal ErrMsg As String = Nothing)

            If Not oLogger Is Nothing Then
                Dim str As String = CurrentTestName
                If Type = EventType.Success Or Type = EventType.Failure Then
                    str &= "." & CStr(iAssertionIndex)
                    If Not Tag Is Nothing Then
                        str &= "(" & Tag & ")"
                    End If
                ElseIf Not Tag Is Nothing Then
                    str &= "." & Tag
                End If
                oLogger.LogEvent(str, Type, TestCount, FailedTests, ErrMsg)
            End If

        End Sub

        Protected Sub UpdateTestProgress(ByVal Completed As Long, ByVal Total As Long)

            If Not oLogger Is Nothing Then oLogger.TestProgress(Completed, Total)

        End Sub

        'call this function to test a condition, e.g. assert(iID > 0)
        Protected Function Assert(ByVal Cond As Boolean, Optional ByVal Tag As String = Nothing, Optional ByVal ErrMsg As String = Nothing) As Boolean

            iAssertionIndex += 1
            iNumberOfTests += 1

            If Cond Then
                iNumberOfSuccesses += 1
                If Not LogOnlyFailures Then
                    LogEvent(EventType.Success, Tag, ErrMsg)
                End If
            Else
                LogEvent(EventType.Failure, Tag, ErrMsg)
            End If

            ' JRC: Updated to return condition value so Assert can be used as pass-through function, was always False before
            Return Cond

        End Function

        'call this function to run all of the public instance-level
        'test functions named testXXX with no parameters;
        'returns percent successful 0 - 100
        Public Overridable Function RunTests(Optional ByVal RandomOrder As Boolean = False) As Double

            Dim methodInfo As MethodInfo
            Dim oType As Type = Me.GetType()
            Dim methods As New ArrayList

            sCurrentTestMethodName = oType.Name
            LogEvent(EventType.Starting)    'log start event for test class

            'execute every public instance method in the class that has no parameters
            'and whose name begins with "test"
            For Each methodInfo In oType.GetMethods(BindingFlags.Public Or BindingFlags.Instance)
                If methodInfo.GetParameters.GetLength(0) = 0 And String.Compare(Left(methodInfo.Name, 4), "test", True) = 0 Then
                    ' JRC - changed such that test methods will fire in desired order
                    methods.Add(methodInfo)
                End If
            Next

            If RandomOrder Then
                ScrambleList(methods)
            Else
                methods.Sort(MethodInfoComparer.Default)
            End If

            For x As Integer = 0 To methods.Count - 1
                methodInfo = methods(x)
                sCurrentTestMethodName = methodInfo.Name    'set current test method name
                iAssertionIndex = 0    'reset assertion counter for each test method
                LogEvent(EventType.Starting)    'log start event for method
                Try
                    oType.InvokeMember(methodInfo.Name, Reflection.BindingFlags.Default Or Reflection.BindingFlags.InvokeMethod, Nothing, Me, Nothing)
                    LogEvent(EventType.Ending)       'log finish event for method
                Catch ex As Exception
                	If not ex.InnerException is Nothing Then
	                    Assert(False, "Unexpected Exception in " & methodInfo.Name, ex.InnerException.Message)      'generic unexpected-exception failure
	                Else
                    	Assert(False, "Unexpected Exception in " & methodInfo.Name, ex.Message)      'generic unexpected-exception failure
                    End If
                End Try
                sCurrentTestMethodName = oType.Name    'reset current test method name to class name
            Next

            LogEvent(EventType.Ending)    'log finish event for test class

            Return PercentSuccessful

        End Function

    End Class

End Namespace