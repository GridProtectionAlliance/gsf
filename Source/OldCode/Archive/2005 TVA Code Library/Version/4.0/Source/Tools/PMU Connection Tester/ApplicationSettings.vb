'*******************************************************************************************************
'  ApplicationSettings.vb - Dynamic Application Settings
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
'  02/13/2007 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports TVA.Configuration

Public Class ApplicationSettings

    Inherits CategorizedSettingsBase

#Region " Default Setting Values "

    ' Default application settings
    Private Const DefaultMaximumConnectionAttempts As Integer = 1
    Private Const DefaultAutoStartDataParsingSequence As Boolean = True
    Private Const DefaultExecuteParseOnSeparateThread As Boolean = False
    Private Const DefaultMaximumFrameDisplayBytes As Integer = 128
    Private Const DefaultRestoreLastConnectionSettings As Boolean = True

    ' Default attribute tree settings
    Private Const DefaultChannelNodeBackgroundColor As String = "Yellow"
    Private Const DefaultChannelNodeForegroundColor As String = "Black"
    Private Const DefaultInitialNodeState As String = "Collapsed"
    Private Const DefaultShowAttributesAsChildren As Boolean = True

    ' Default general chart settings
    Private Const DefaultChartRefreshRate As Single = 0.1
    Private Const DefaultBackgroundColor As String = "White"
    Private Const DefaultForegroundColor As String = "Navy"
    Private Const DefaultTrendLineWidth As Integer = 4
    Private Const DefaultShowDataPointsOnGraphs As Boolean = False

    ' Default phase angle graph settings
    Private Const DefaultPhaseAngleGraphStyle As String = "Relative"
    Private Const DefaultShowPhaseAngleLegend As Boolean = True
    Private Const DefaultPhaseAnglePointsToPlot As Integer = 30
    Private Const DefaultLegendBackgroundColor As String = "AliceBlue"
    Private Const DefaultLegendForegroundColor As String = "Navy"
    Private Const DefaultPhaseAngleColors As String = "Black;Red;Green;SteelBlue;DarkGoldenrod;Brown;Coral;Purple"

    ' Default frequency graph settings
    Private Const DefaultFrequencyPointsToPlot As Integer = 30
    Private Const DefaultFrequencyColor As String = "SteelBlue"

#End Region

#Region " Public Member Declarations "

    Public Event PhaseAngleColorsChanged()

    ' Configuration file categories
    Public Const ApplicationSettingsCategory As String = "Application Settings"
    Public Const AttributeTreeCategory As String = "Attribute Tree"
    Public Const ChartSettingsCategory As String = "Chart Settings"
    Public Const PhaseAngleGraphCategory As String = "Phase Angle Graph"
    Public Const FrequencyGraphCategory As String = "Frequency Graph"

    Public Enum AngleGraphStyle
        Raw
        Relative
    End Enum

    Public Enum NodeState
        Expanded
        Collapsed
    End Enum

#Region " Color List with Content Cleared Notification "

    Public Class ColorListTypeConverter

        Inherits TypeConverter

        Private m_colorParser As New ColorConverter

        Public Overrides Function CanConvertFrom(ByVal context As ITypeDescriptorContext, ByVal sourceType As Type) As Boolean

            If sourceType Is GetType(String) Then Return True
            Return MyBase.CanConvertFrom(context, sourceType)

        End Function

        Public Overrides Function ConvertFrom(ByVal context As ITypeDescriptorContext, ByVal culture As CultureInfo, ByVal value As Object) As Object

            If TypeOf value Is String Then
                Dim colors As New ColorList

                For Each colorItem As String In CStr(value).Split(";"c)
                    colors.Add(CType(m_colorParser.ConvertFromString(colorItem.Trim()), Color))
                Next

                Return colors
            End If

            Return MyBase.ConvertFrom(context, culture, value)

        End Function

        Public Overrides Function ConvertTo(ByVal context As ITypeDescriptorContext, ByVal culture As CultureInfo, ByVal value As Object, ByVal destinationType As Type) As Object

            If destinationType Is GetType(String) Then
                With New StringBuilder
                    For Each colorItem As Color In CType(value, ColorList)
                        If .Length > 0 Then .Append(";"c)
                        .Append(m_colorParser.ConvertToString(colorItem))
                    Next

                    Return .ToString()
                End With
            End If

            Return MyBase.ConvertTo(context, culture, value, destinationType)

        End Function

    End Class

    ' This class exposes an event notification of when then list is cleared - this is really
    ' our only signal to know when a collection has been modified in the property grid

    <TypeConverter(GetType(ColorListTypeConverter))> _
    Public Class ColorList

        Inherits Collection(Of Color)

        Public Event ListContentCleared()

        Public Sub New(ByVal ParamArray colors As Color())

            For Each newColor As Color In colors
                Add(newColor)
            Next

        End Sub

        Protected Overrides Sub ClearItems()

            MyBase.ClearItems()
            RaiseEvent ListContentCleared()

        End Sub

    End Class

#End Region

#End Region

#Region " Private Member Declarations "

    ' Application settings
    Private m_maximumConnectionAttempts As Integer
    Private m_autoStartDataParsingSequence As Boolean
    Private m_executeParseOnSeparateThread As Boolean
    Private m_maximumFrameDisplayBytes As Integer
    Private m_restoreLastConnectionSettings As Boolean

    ' Attribute tree settings
    Private m_channelNodeBackgroundColor As Color
    Private m_channelNodeForegroundColor As Color
    Private m_initialNodeState As NodeState
    Private m_showAttributesAsChildren As Boolean

    ' General chart settings
    Private m_refreshRate As Single
    Private m_backgroundColor As Color
    Private m_foregroundColor As Color
    Private m_trendLineWidth As Integer
    Private m_showDataPointsOnGraphs As Boolean

    ' Phase angle graph settings
    Private m_phaseAngleGraphStyle As AngleGraphStyle
    Private m_showPhaseAngleLegend As Boolean
    Private m_phaseAnglePointsToPlot As Integer
    Private m_legendBackgroundColor As Color
    Private m_legendForegroundColor As Color
    Private WithEvents m_phaseAngleColors As ColorList

    ' Frequency graph settings
    Private m_frequencyPointsToPlot As Integer
    Private m_frequencyColor As Color

    ' Other members
    Private WithEvents m_eventDelayTimer As Timers.Timer

#End Region

#Region " Constructors "

    Public Sub New()

        ' Specifiy default category
        MyBase.New("General")

        m_eventDelayTimer = New Timers.Timer

        With m_eventDelayTimer
            .Interval = 250
            .AutoReset = False
            .Enabled = False
        End With

    End Sub

#End Region

#Region " Application Settings "

    <Category(ApplicationSettingsCategory), _
    Description("Maximum number of times to attempt connection before giving up; set to -1 to continue connection attempt indefinitely."), _
    DefaultValue(DefaultMaximumConnectionAttempts)> _
    Public Property MaximumConnectionAttempts() As Integer
        Get
            Return m_maximumConnectionAttempts
        End Get
        Set(ByVal value As Integer)
            If value < 0 Then
                m_maximumConnectionAttempts = -1
            ElseIf value > 0 Then
                m_maximumConnectionAttempts = value
            Else
                m_maximumConnectionAttempts = DefaultMaximumConnectionAttempts
            End If
        End Set
    End Property

    <Category(ApplicationSettingsCategory), _
    Description("Set to True to automatically send commands for ConfigFrame2 and EnableRealTimeData."), _
    DefaultValue(DefaultAutoStartDataParsingSequence)> _
    Public Property AutoStartDataParsingSequence() As Boolean
        Get
            Return m_autoStartDataParsingSequence
        End Get
        Set(ByVal value As Boolean)
            m_autoStartDataParsingSequence = value
        End Set
    End Property

    <Category(ApplicationSettingsCategory), _
    Description("Allows frame parsing to be executed on a separate thread (other than communications thread) - typically only needed when data frames are very large.  This change will happen dynamically, even if a connection is active."), _
    DefaultValue(DefaultExecuteParseOnSeparateThread)> _
    Public Property ExecuteParseOnSeparateThread() As Boolean
        Get
            Return m_executeParseOnSeparateThread
        End Get
        Set(ByVal value As Boolean)
            m_executeParseOnSeparateThread = value
        End Set
    End Property

    <Category(ApplicationSettingsCategory), _
    Description("Maximum encoded bytes to display for frames in the ""Real-time Frame Detail""."), _
    DefaultValue(DefaultMaximumFrameDisplayBytes)> _
    Public Property MaximumFrameDisplayBytes() As Integer
        Get
            Return m_maximumFrameDisplayBytes
        End Get
        Set(ByVal value As Integer)
            If value < 1 Then
                m_maximumFrameDisplayBytes = DefaultMaximumFrameDisplayBytes
            Else
                m_maximumFrameDisplayBytes = value
            End If
        End Set
    End Property

    <Category(ApplicationSettingsCategory), _
    Description("Set to True to load previous connection settings at startup."), _
    DefaultValue(DefaultRestoreLastConnectionSettings)> _
    Public Property RestoreLastConnectionSettings() As Boolean
        Get
            Return m_restoreLastConnectionSettings
        End Get
        Set(ByVal value As Boolean)
            m_restoreLastConnectionSettings = value
        End Set
    End Property

#End Region

#Region " Attribute Tree Settings "

    <Category(AttributeTreeCategory), _
    Description("Defines the highlight background color for channel node entries on the attribute tree."), _
    DefaultValue(GetType(Color), DefaultChannelNodeBackgroundColor)> _
    Public Property ChannelNodeBackgroundColor() As Color
        Get
            Return m_channelNodeBackgroundColor
        End Get
        Set(ByVal value As Color)
            m_channelNodeBackgroundColor = value
        End Set
    End Property

    <Category(AttributeTreeCategory), _
    Description("Defines the highlight foreground color for channel node entries on the attribute tree."), _
    DefaultValue(GetType(Color), DefaultChannelNodeForegroundColor)> _
    Public Property ChannelNodeForegroundColor() As Color
        Get
            Return m_channelNodeForegroundColor
        End Get
        Set(ByVal value As Color)
            m_channelNodeForegroundColor = value
        End Set
    End Property

    <Category(AttributeTreeCategory), _
    Description("Defines the initial state for nodes when added to the attribute tree.  Note that a fully expanded tree will take much longer to initialize."), _
    DefaultValue(GetType(NodeState), DefaultInitialNodeState)> _
    Public Property InitialNodeState() As NodeState
        Get
            Return m_initialNodeState
        End Get
        Set(ByVal value As NodeState)
            m_initialNodeState = value
        End Set
    End Property

    <Category(AttributeTreeCategory), _
    Description("Set to True to show attributes as children of their channel entries."), _
    DefaultValue(DefaultShowAttributesAsChildren)> _
    Public Property ShowAttributesAsChildren() As Boolean
        Get
            Return m_showAttributesAsChildren
        End Get
        Set(ByVal value As Boolean)
            m_showAttributesAsChildren = value
        End Set
    End Property

#End Region

#Region " General Chart Settings "

    <Category(ChartSettingsCategory), _
    Description("Chart refresh rate in seconds. Typical setting is 0.1, increase this number if app runs slow."), _
    DefaultValue(DefaultChartRefreshRate)> _
    Public Property RefreshRate() As Single
        Get
            Return m_refreshRate
        End Get
        Set(ByVal value As Single)
            If value <= 0 Then
                m_refreshRate = DefaultChartRefreshRate
            Else
                m_refreshRate = value
            End If
        End Set
    End Property

    <Category(ChartSettingsCategory), _
    Description("Background color for graph region."), _
    DefaultValue(GetType(Color), DefaultBackgroundColor)> _
    Public Property BackgroundColor() As Color
        Get
            Return m_backgroundColor
        End Get
        Set(ByVal value As Color)
            m_backgroundColor = value
        End Set
    End Property

    <Category(ChartSettingsCategory), _
    Description("Foreground color for graph region (axes, legend border, text, etc.)"), _
    DefaultValue(GetType(Color), DefaultForegroundColor)> _
    Public Property ForegroundColor() As Color
        Get
            Return m_foregroundColor
        End Get
        Set(ByVal value As Color)
            m_foregroundColor = value
        End Set
    End Property

    <Category(ChartSettingsCategory), _
    Description("Trend line graphing width (in pixels)."), _
    DefaultValue(DefaultTrendLineWidth)> _
    Public Property TrendLineWidth() As Integer
        Get
            Return m_trendLineWidth
        End Get
        Set(ByVal value As Integer)
            If value <= 0 Then
                m_trendLineWidth = DefaultTrendLineWidth
            Else
                m_trendLineWidth = value
            End If
        End Set
    End Property

    <Category(ChartSettingsCategory), _
    Description("Set to True to show data points on graphs."), _
    DefaultValue(DefaultShowDataPointsOnGraphs)> _
    Public Property ShowDataPointsOnGraphs() As Boolean
        Get
            Return m_showDataPointsOnGraphs
        End Get
        Set(ByVal value As Boolean)
            m_showDataPointsOnGraphs = value
        End Set
    End Property

#End Region

#Region " Phase Angle Graph Settings "

    <Category(PhaseAngleGraphCategory), _
    Description("Sets the phase angle graph to plot either raw or relative phase angles."), _
    DefaultValue(GetType(AngleGraphStyle), DefaultPhaseAngleGraphStyle)> _
    Public Property PhaseAngleGraphStyle() As AngleGraphStyle
        Get
            Return m_phaseAngleGraphStyle
        End Get
        Set(ByVal value As AngleGraphStyle)
            m_phaseAngleGraphStyle = value
        End Set
    End Property

    <Category(PhaseAngleGraphCategory), _
    Description("Set to True to show phase angle graph legend."), _
    DefaultValue(DefaultShowPhaseAngleLegend)> _
    Public Property ShowPhaseAngleLegend() As Boolean
        Get
            Return m_showPhaseAngleLegend
        End Get
        Set(ByVal value As Boolean)
            m_showPhaseAngleLegend = value
        End Set
    End Property

    <Category(PhaseAngleGraphCategory), _
    Description("Sets the total number of phase angle points to display."), _
    DefaultValue(DefaultPhaseAnglePointsToPlot)> _
    Public Property PhaseAnglePointsToPlot() As Integer
        Get
            Return m_phaseAnglePointsToPlot
        End Get
        Set(ByVal value As Integer)
            If value < 2 Then
                m_phaseAnglePointsToPlot = DefaultPhaseAnglePointsToPlot
            Else
                m_phaseAnglePointsToPlot = value
            End If
        End Set
    End Property

    <Category(PhaseAngleGraphCategory), _
    Description("Background color for phase angle legend."), _
    DefaultValue(GetType(Color), DefaultLegendBackgroundColor)> _
    Public Property LegendBackgroundColor() As Color
        Get
            Return m_legendBackgroundColor
        End Get
        Set(ByVal value As Color)
            m_legendBackgroundColor = value
        End Set
    End Property

    <Category(PhaseAngleGraphCategory), _
    Description("Foreground color for phase angle legend text."), _
    DefaultValue(GetType(Color), DefaultLegendForegroundColor)> _
    Public Property LegendForegroundColor() As Color
        Get
            Return m_legendForegroundColor
        End Get
        Set(ByVal value As Color)
            m_legendForegroundColor = value
        End Set
    End Property

    <Category(PhaseAngleGraphCategory), _
    Description("Possible foreground colors for phase angle trends."), _
    DefaultValue(GetType(ColorList), DefaultPhaseAngleColors)> _
    Public Property PhaseAngleColors() As ColorList
        Get
            Return m_phaseAngleColors
        End Get
        Set(ByVal value As ColorList)
            m_phaseAngleColors = value
        End Set
    End Property

#End Region

#Region " Frequency Graph Settings "

    <Category(FrequencyGraphCategory), _
    Description("Sets the total number of frequency points to display."), _
    DefaultValue(DefaultFrequencyPointsToPlot)> _
    Public Property FrequencyPointsToPlot() As Integer
        Get
            Return m_frequencyPointsToPlot
        End Get
        Set(ByVal value As Integer)
            If value < 2 Then
                m_frequencyPointsToPlot = DefaultFrequencyPointsToPlot
            Else
                m_frequencyPointsToPlot = value
            End If
        End Set
    End Property

    <Category(FrequencyGraphCategory), _
    Description("Foreground color for frequency trend."), _
    DefaultValue(GetType(Color), DefaultFrequencyColor)> _
    Public Property FrequencyColor() As Color
        Get
            Return m_frequencyColor
        End Get
        Set(ByVal value As Color)
            m_frequencyColor = value
        End Set
    End Property

#End Region

#Region " Private Method Implementation "

    Private Sub m_phaseAngleColors_ListContentCleared() Handles m_phaseAngleColors.ListContentCleared

        ' Updates to a collection from a PropertyGrid don't get a normal "PropertyValueChanged" notification,
        ' so you're stuck with detecting a call to "Clear" in your personal collection.  However, the update
        ' is not complete until a call to "Add" for each updated item, so we need to wait for a moment to
        ' allow all of the adds to finish - this isn't exact science - someone didn't think through this one.
        m_eventDelayTimer.Enabled = True

    End Sub

    Private Sub m_eventDelayTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_eventDelayTimer.Elapsed

        RaiseEvent PhaseAngleColorsChanged()

    End Sub

#End Region

End Class
