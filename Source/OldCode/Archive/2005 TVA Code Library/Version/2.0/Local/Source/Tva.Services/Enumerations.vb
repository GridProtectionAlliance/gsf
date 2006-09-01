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

''' <summary>Standard service related notifications that can be sent from a remote client monitor that are to be handled by the service.</summary>
''' <remarks>Note that there is no "Start Service" message defined since service must already be started in order for there to be any clients</remarks>
<Serializable()> _
Public Enum ServiceRequestType
    Undetermined
    PauseService
    ResumeService
    StopService
    RestartService
    ListProcesses
    StartProcess
    AbortProcess
    UnscheduleProcess
    RescheduleProcess
    PingService
    PingAllClients
    ListClients
    GetServiceStatus
    GetProcessStatus
    GetCommandHistory
    GetDirectoryListing
    ListSettings
    UpdateSetting
    SaveSettings
End Enum

''' <summary>Standard notifications that will be sent from the service that can be handled by remote monitoring clients</summary>
<Serializable()> _
Public Enum ServiceResponseType
    Undetermined
    UnicastMessage
    MulticastMessage
    BroadcastMessage
    ServiceStateChanged
    ProcessStateChanged
    ProcessProgressUpdate
End Enum