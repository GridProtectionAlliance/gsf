'*******************************************************************************************************
'  Tva.Measurements.ISample.vb - Abstract sample interface
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  This class represents a collection of frames over a specified time interval
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  12/8/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Namespace Measurements

    Public Interface ISample

        Inherits IComparable

        ''' <summary>Handy instance reference to self</summary>
        ReadOnly Property This() As ISample

        ''' <summary>Array of frames in this sample</summary>
        ReadOnly Property Frames() As IFrame()

        ''' <summary>Time interval represented by this frame in seconds</summary>
        Property Interval() As Double

        ''' <summary>Closest date representation of ticks of this frame</summary>
        Property FramesPerInterval() As Integer

        ''' <summary>Exact timestamp of the beginning of data sample</summary>
        ''' <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
        Property Ticks() As Long

        ''' <summary>Closest date representation of ticks of data sample</summary>
        Property Timestamp() As Date

    End Interface

End Namespace
