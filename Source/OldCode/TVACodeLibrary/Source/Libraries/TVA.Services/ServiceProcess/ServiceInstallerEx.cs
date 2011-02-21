//*******************************************************************************************************
//  ServiceInstallerEx.cs - Gbtc
//
//  Tennessee Valley Authority, 2011
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/04/2011 - J. Ritchie Carroll
//       Initial version of source integrated into TVA Code Library.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

/////////////////////////////////////////////////////////////////////////////
//
// Original Name    : Verifide.ServiceUtils.dll
// Description		: Extension to System.ServiceProcess.ServiceInstallerEx
//					  to enable configuration of advanced options
// Date				: 1/14/04
//
//	Copyright (C) 2004 Narendra (Neil) Baliga
//
//	This library is free software; you can redistribute it and/or
//	modify it as you wish. It is distributed in the hope that it will 
//	be useful,but WITHOUT ANY WARRANTY; without even the implied 
//	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
//
/////////////////////////////////////////////////////////////////////////////

#endregion

using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using TVA.Interop;

namespace TVA.Services.ServiceProcess
{
    #region [ Enumerations ]

    /// <summary>
    /// Defines the recover action to be performed upon service failure.
    /// </summary>
    /// <remarks>
    /// Enum values correspond to Win32 equivalents.
    /// </remarks>
    public enum RecoverAction
    {
        /// <summary>
        /// No action.
        /// </summary>
        None = 0,
        /// <summary>
        /// Restart the service.
        /// </summary>
        Restart = 1,
        /// <summary>
        /// Reboot the computer.
        /// </summary>
        Reboot = 2,
        /// <summary>
        /// Run a command.
        /// </summary>
        RunCommand = 3
    }

    #endregion

    /// <summary>
    /// Defines an extended service installer class that can define service failure actions at install time.
    /// </summary>
    public class ServiceInstallerEx : ServiceInstaller
    {
        #region [ Members ]

        // Constants
        private const int SC_MANAGER_ALL_ACCESS = 0xF003F;
        private const int SERVICE_ALL_ACCESS = 0xF01FF;
        private const int SERVICE_CONFIG_FAILURE_ACTIONS = 0x2;
        private const int SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 0x4;

        // Fields
        private List<WindowsApi.SC_ACTION> m_failureActions;    // Service failure actions
        private int m_failResetPeriod = Timeout.Infinite;       // Service fail count reset time
        private string m_failRebootMessage = "";			    // Service reboot message
        private string m_failRunCommand = "";				    // Service fail run command
        private bool m_executeActionsOnNonCrashErrors = true;   // Windows 2008 check box to restart on exit with error
        private bool m_startOnInstall = false;
        private int m_startTimeout = 15000;
        private string m_logMessagePrefix;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ServiceInstallerEx"/>.
        /// </summary>
        public ServiceInstallerEx() : base()
        {
            // Initialize the failure actions and register for the Committed event
            m_failureActions = new List<WindowsApi.SC_ACTION>();

            // Register the event handlers for post install operations
            base.Committed += new InstallEventHandler(UpdateServiceConfig);
            base.Committed += new InstallEventHandler(StartIfNeeded);

            // Set the log message prefix
            m_logMessagePrefix = base.ServiceName + " - ServiceInstallerEx: ";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Sets the time after which to reset the failure count to zero if there are no failures, in seconds.
        /// Specify <see cref="Timeout.Infinite"/> to indicate that this value should never be reset.
        /// </summary>
        public int FailResetPeriod
        {
            set
            {
                if (value < 0)
                    m_failResetPeriod = Timeout.Infinite;
                else
                    m_failResetPeriod = value;
            }
        }

        /// <summary>
        /// Sets the service fail reboot message.
        /// </summary>
        public string FailRebootMessage
        {
            set
            {
                m_failRebootMessage = value;
            }
        }

        /// <summary>
        /// Sets the service fail run command.
        /// </summary>
        public string FailRunCommand
        {
            set
            {
                m_failRunCommand = value;
            }
        }

        /// <summary>
        /// Sets flag that determines when failure actions are to be executed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this member is <c>true</c> and the service has configured failure actions, the failure actions are queued if the service process
        /// terminates without reporting a status of SERVICE_STOPPED or if it enters the SERVICE_STOPPED state but the exit code returned in the
        /// SERVICE_STATUS structure is not ERROR_SUCCESS (0).
        /// </para>
        /// <para>
        /// If this member is <c>false</c> and the service has configured failure actions, the failure actions are queued only if the service
        /// terminates without reporting a status of SERVICE_STOPPED.
        /// </para>
        /// <para>
        /// This setting is ignored unless the service has configured failure actions.
        /// </para>
        /// </remarks>
        public bool ExecuteActionsOnNonCrashErrors
        {
            set
            {
                m_executeActionsOnNonCrashErrors = value;
            }

        }

        /// <summary>
        /// Sets the boolean value to configure the service to start after it is installed.
        /// </summary>
        public bool StartOnInstall
        {
            set
            {
                m_startOnInstall = value;
            }
        }

        /// <summary>
        /// Sets the service start timeout in milliseconds.
        /// </summary>
        public int StartTimeout
        {
            set
            {
                m_startTimeout = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Defines a new recover action to be performed upon service failure.
        /// </summary>
        /// <param name="recoverAction"><see cref="RecoverAction"/> to execute upon service failure.</param>
        /// <param name="delay">The time to wait before performing the specified <paramref name="recoverAction"/>, in milliseconds.</param>
        public void DefineRecoverAction(RecoverAction recoverAction, int delay)
        {
            m_failureActions.Add(new WindowsApi.SC_ACTION() { Type = (WindowsApi.SC_ACTION_TYPE)(uint)recoverAction, Delay = delay });
        }

        // The worker method to set all the extension properties for the service
        private void UpdateServiceConfig(object sender, InstallEventArgs e)
        {
            int actionCount = m_failureActions.Count;

            // Do we need to do any work that the base installer did not do already?
            if (actionCount == 0)
                return;

            // We've got work to do
            IntPtr serviceManagerHandle = IntPtr.Zero;
            IntPtr serviceHandle = IntPtr.Zero;
            IntPtr serviceLockHandle = IntPtr.Zero;
            IntPtr actionsPtr = IntPtr.Zero;
            IntPtr failureActionsPtr = IntPtr.Zero;

            // Err check var
            bool result = false;

            // Place all our code in a try block
            try
            {
                // Open the service control manager
                serviceManagerHandle = WindowsApi.OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);

                if (serviceManagerHandle.ToInt32() <= 0)
                {
                    LogInstallMessage(EventLogEntryType.Error, m_logMessagePrefix + "Failed to Open Service Control Manager");
                    return;
                }

                // Lock the Service Database
                serviceLockHandle = WindowsApi.LockServiceDatabase(serviceManagerHandle);

                if (serviceLockHandle.ToInt32() <= 0)
                {
                    LogInstallMessage(EventLogEntryType.Error, m_logMessagePrefix + "Failed to Lock Service Database for Write");
                    return;
                }

                // Open the service
                serviceHandle = WindowsApi.OpenService(serviceManagerHandle, base.ServiceName, SERVICE_ALL_ACCESS);

                if (serviceHandle.ToInt32() <= 0)
                {
                    LogInstallMessage(EventLogEntryType.Information, m_logMessagePrefix + "Failed to Open Service ");
                    return;
                }

                // Need to set service failure actions. Note that the API lets us set as many as
                // we want, yet the Service Control Manager GUI only lets us see the first 3.
                // Bill is aware of this and has promised no fixes. Also note that the API allows
                // granularity of seconds whereas GUI only shows days and minutes.

                // Calculate size of SC_ACTION structure
                int scActionSize = Marshal.SizeOf(typeof(WindowsApi.SC_ACTION));
                bool needShutdownPrivilege = false;

                // Allocate memory for the array of individual actions
                actionsPtr = Marshal.AllocHGlobal(scActionSize * actionCount);

                // Set up the restart actions array by copying in each failure action structure
                for (int i = 0; i < actionCount; i++)
                {
                    // Handle pointer math as 64-bit, cast will convert back to 32-bit if needed
                    Marshal.StructureToPtr(m_failureActions[i], (IntPtr)((Int64)actionsPtr + i * scActionSize), false);

                    if (m_failureActions[i].Type == WindowsApi.SC_ACTION_TYPE.SC_ACTION_REBOOT)
                        needShutdownPrivilege = true;
                }


                // If we need shutdown privilege, then grant it to this process
                if (needShutdownPrivilege)
                {
                    result = GrantShutdownPrivilege();

                    if (!result)
                        return;
                }

                // Set up the failure actions
                WindowsApi.SERVICE_FAILURE_ACTIONS failureActions = new WindowsApi.SERVICE_FAILURE_ACTIONS();
                failureActions.cActions = actionCount;
                failureActions.dwResetPeriod = m_failResetPeriod;
                failureActions.lpCommand = m_failRunCommand;
                failureActions.lpRebootMsg = m_failRebootMessage;
                failureActions.lpsaActions = actionsPtr;

                failureActionsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WindowsApi.SERVICE_FAILURE_ACTIONS)));
                Marshal.StructureToPtr(failureActions, failureActionsPtr, false);

                // Make the change
                result = WindowsApi.ChangeServiceConfig2(serviceHandle, SERVICE_CONFIG_FAILURE_ACTIONS, failureActionsPtr);
                    
                // Check the return
                if (!result)
                {
                    int err = WindowsApi.GetLastError();

                    if (err == WindowsApi.ERROR_ACCESS_DENIED)
                        throw new Exception(m_logMessagePrefix + "Access Denied while setting service failure actions");
                }

                LogInstallMessage(EventLogEntryType.Information, m_logMessagePrefix + "Successfully configured service failure actions");

                // Failure actions flag only applies on Vista / 2008 or better
                if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6)
                {
                    WindowsApi.SERVICE_FAILURE_ACTIONS_FLAG failureActionFlag = new WindowsApi.SERVICE_FAILURE_ACTIONS_FLAG();
                    failureActionFlag.bFailureAction = m_executeActionsOnNonCrashErrors;
                    result = WindowsApi.ChangeServiceConfig2(serviceHandle, SERVICE_CONFIG_FAILURE_ACTIONS_FLAG, ref failureActionFlag);

                    // Error setting description?
                    if (!result)
                        throw new Exception(m_logMessagePrefix + "Failed to set failure actions flag");
                }
            }
            // Catch all exceptions
            catch (Exception ex)
            {
                LogInstallMessage(EventLogEntryType.Error, ex.Message);
            }
            finally
            {
                if (serviceManagerHandle != IntPtr.Zero)
                {
                    // Unlock the service database
                    if (serviceLockHandle != IntPtr.Zero)
                        WindowsApi.UnlockServiceDatabase(serviceLockHandle);

                    // Close the service control manager handle
                    WindowsApi.CloseServiceHandle(serviceManagerHandle);
                }

                // Close the service handle
                if (serviceHandle != IntPtr.Zero)
                    WindowsApi.CloseServiceHandle(serviceHandle);

                // Free allocated memory
                if (failureActionsPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(failureActionsPtr);

                if (actionsPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(actionsPtr);
            }
        }

        // This code mimics the MSDN defined way to adjust privilege for shutdown
        // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/shutting_down.asp
        private bool GrantShutdownPrivilege()
        {
            bool grantSuccess = false;
            IntPtr processToken = IntPtr.Zero;
            IntPtr processHandle = IntPtr.Zero;
            WindowsApi.TOKEN_PRIVILEGES tokenPrivileges = new WindowsApi.TOKEN_PRIVILEGES();
            long luid = 0;
            int returnLen = 0;

            try
            {
                processHandle = WindowsApi.GetCurrentProcess();

                bool result = WindowsApi.OpenProcessToken(processHandle, WindowsApi.TOKEN_ADJUST_PRIVILEGES | WindowsApi.TOKEN_QUERY, ref processToken);

                if (!result)
                    return grantSuccess;

                WindowsApi.LookupPrivilegeValue(null, WindowsApi.SE_SHUTDOWN_NAME, ref luid);

                tokenPrivileges.PrivilegeCount = 1;
                tokenPrivileges.Privileges.Luid = luid;
                tokenPrivileges.Privileges.Attributes = WindowsApi.SE_PRIVILEGE_ENABLED;

                result = WindowsApi.AdjustTokenPrivileges(processToken, false, ref tokenPrivileges, 0, IntPtr.Zero, ref returnLen);

                if (WindowsApi.GetLastError() != 0)
                    throw new Exception("Failed to grant shutdown privilege");

                grantSuccess = true;

            }
            catch (Exception ex)
            {
                LogInstallMessage(EventLogEntryType.Error, m_logMessagePrefix + ex.Message);
            }
            finally
            {
                if (processToken != IntPtr.Zero)
                    WindowsApi.CloseHandle(processToken);
            }

            return grantSuccess;
        }

        // Method to start the service automatically after installation
        private void StartIfNeeded(object sender, InstallEventArgs e)
        {
            // Do we need to do any work?
            if (!m_startOnInstall)
                return;

            try
            {
                TimeSpan waitTo = new TimeSpan(0, 0, m_startTimeout);

                // Get a handle to the service
                ServiceController sc = new ServiceController(base.ServiceName);

                // Start the service and wait for it to start
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, waitTo);

                // Be good and release our handle
                sc.Close();

                LogInstallMessage(EventLogEntryType.Information, m_logMessagePrefix + " Service Started");

            }
            // Catch all exceptions
            catch (Exception ex)
            {
                LogInstallMessage(EventLogEntryType.Error, m_logMessagePrefix + ex.Message);
            }
        }

        // Method to log to console and event log
        private void LogInstallMessage(EventLogEntryType logLevel, string msg)
        {
            System.Console.WriteLine(msg);

            try
            {
                EventLog.WriteEntry(base.ServiceName, msg, logLevel);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
        }

        #endregion
    }
}