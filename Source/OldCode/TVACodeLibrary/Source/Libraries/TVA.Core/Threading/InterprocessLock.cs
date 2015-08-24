//*******************************************************************************************************
//  InterprocessLock.cs - Gbtc
//
//  Tennessee Valley Authority, 2011
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/21/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  06/30/2011 - Stephen Wills
//       Applying changes from Jian (Ryan) Zuo: updated to allow unauthorized users to attempt to grant
//       themselves lower than full access to existing mutexes and semaphores.
//  08/12/2011 - J. Ritchie Carroll
//       Modified creation methods such that locking natives are created in a synchronized fashion.
//  09/21/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
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

#endregion

using System;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using TVA.Security.Cryptography;

namespace TVA.Threading
{
    /// <summary>
    /// Defines helper methods related to interprocess locking.
    /// </summary>
    public static class InterprocessLock
    {
        private const int MutexHash = 0;
        private const int SemaphoreHash = 1;

        /// <summary>
        /// Gets a uniquely named interprocess <see cref="Mutex"/> associated with the specified <paramref name="name"/> that identifies a source object
        /// needing concurrency locking.
        /// </summary>
        /// <param name="name">Identifiying name of source object needing concurrency locking (e.g., a path and file name).</param>
        /// <returns>A uniquely named interprocess <see cref="Mutex"/> specific to <paramref name="name"/>; <see cref="Mutex"/> is created if it does not exist.</returns>
        /// <remarks>
        /// <para>
        /// This function uses a hash of the <paramref name="name"/> when creating the <see cref="Mutex"/>, not the actual <paramref name="name"/> - this way
        /// restrictions on the <paramref name="name"/> do not need to be a user concern. All processes needing an interprocess <see cref="Mutex"/> need to
        /// use this same function to ensure access to the same <see cref="Mutex"/>.
        /// </para>
        /// <para>
        /// The <paramref name="name"/> can be a string of any length (must not be empty, null or white space) and is not case-sensitive. All hashes of the
        /// <paramref name="name"/> used to create the global <see cref="Mutex"/> are first converted to lower case.
        /// </para>
        /// <para>
        /// The <see cref="Mutex"/> created is "Global" meaning that it will be accessible to all active application sessions including terminal service
        /// sessions. This is accomplished internally by prefixing the <see cref="Mutex"/> name with "Global\"; it is not necessary for the user to be
        /// concerned with the length or contents of the <paramref name="name"/> in this method as long as the same <paramref name="name"/> is used for
        /// each application. Do not use this helper function if you need to create a specifically named or non-global <see cref="Mutex"/>, such as when
        /// you need to interact with another application using a <see cref="Mutex"/> that does not use this function.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Argument <paramref name="name"/> cannot be empty, null or white space.</exception>
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static Mutex GetNamedMutex(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Argument cannot be empty, null or white space.");

            Mutex namedMutex = null;
            bool doesNotExist = false;
            bool unauthorized = false;

            // Create a mutex name that is specific to an object (e.g., a path and file name).
            // Prefix mutext name with "Global\" such that mutex will apply to all active
            // application sessions in case terminal services is running.
            string mutexName = "Global\\" + Cipher.GetPasswordHash(name.ToLower(), MutexHash).Replace('\\', '-');

            // Attempt to open the named mutex
            try
            {
                namedMutex = Mutex.OpenExisting(mutexName);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                doesNotExist = true;
            }
            catch (UnauthorizedAccessException)
            {
                unauthorized = true;
            }

            // Mono Mutex implementations do not include ability to change access rules
#if MONO
            // If mutex does not exist we create it
            if (doesNotExist || unauthorized)
            {
                try
                {
                    bool mutexWasCreated;

                    namedMutex = new Mutex(false, mutexName, out mutexWasCreated);

                    if (!mutexWasCreated)
                        throw new InvalidOperationException("Failed to create mutex.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new InvalidOperationException("Failed to create mutex: " + ex.Message);
                }
            }
#else
            // If mutex does not exist we create it
            if (doesNotExist)
            {
                try
                {
                    MutexSecurity security = new MutexSecurity();
                    bool mutexWasCreated;

                    security.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));
                    namedMutex = new Mutex(false, mutexName, out mutexWasCreated, security);

                    if (!mutexWasCreated)
                        throw new InvalidOperationException("Failed to create mutex.");
                }
                catch (UnauthorizedAccessException)
                {
                    MutexSecurity security = new MutexSecurity();
                    bool mutexWasCreated;

                    security.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.Synchronize | MutexRights.Modify, AccessControlType.Allow));
                    namedMutex = new Mutex(false, mutexName, out mutexWasCreated, security);

                    if (!mutexWasCreated)
                        throw new InvalidOperationException("Failed to create mutex.");
                }
            }

            if (unauthorized)
            {
                namedMutex = Mutex.OpenExisting(mutexName, MutexRights.ReadPermissions | MutexRights.ChangePermissions);

                // Get the current ACL. This requires MutexRights.ReadPermission
                MutexSecurity security = namedMutex.GetAccessControl();
                MutexAccessRule rule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
                security.RemoveAccessRule(rule);

                // Now grant specific user rights for less than full access
                rule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.Synchronize | MutexRights.Modify, AccessControlType.Allow);
                security.AddAccessRule(rule);

                // Update the ACL. This requires MutexRighs.ChangePermission.
                namedMutex.SetAccessControl(security);
                namedMutex = Mutex.OpenExisting(mutexName);
            }
#endif

            return namedMutex;
        }

        /// <summary>
        /// Gets a uniquely named interprocess <see cref="Semaphore"/> associated with the specified <paramref name="name"/> that identifies a source object
        /// needing concurrency locking.
        /// </summary>
        /// <param name="name">Identifiying name of source object needing concurrency locking (e.g., a path and file name).</param>
        /// <param name="maximumCount">The maximum number of requests for the semaphore that can be granted concurrently.</param>
        /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently, or -1 to default to <paramref name="maximumCount"/>.</param>
        /// <returns>A uniquely named interprocess <see cref="Semaphore"/> specific to <paramref name="name"/>; <see cref="Semaphore"/> is created if it does not exist.</returns>
        /// <remarks>
        /// <para>
        /// This function uses a hash of the <paramref name="name"/> when creating the <see cref="Semaphore"/>, not the actual <paramref name="name"/> - this way
        /// restrictions on the <paramref name="name"/> do not need to be a user concern. All processes needing an interprocess <see cref="Semaphore"/> need to
        /// use this same function to ensure access to the same <see cref="Semaphore"/>.
        /// </para>
        /// <para>
        /// The <paramref name="name"/> can be a string of any length (must not be empty, null or white space) and is not case-sensitive. All hashes of the
        /// <paramref name="name"/> used to create the global <see cref="Semaphore"/> are first converted to lower case.
        /// </para>
        /// <para>
        /// It is not necessary for the user to be concerned with the length or contents of the <paramref name="name"/> in this method as long as the same
        /// <paramref name="name"/> is used for each application. Do not use this helper function if you need to create a specifically named <see cref="Semaphore"/>,
        /// such as when you need to interact with another application using a <see cref="Semaphore"/> that does not use this function.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Argument <paramref name="name"/> cannot be empty, null or white space.</exception>
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static Semaphore GetNamedSemaphore(string name, int maximumCount = 10, int initialCount = -1)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name", "Argument cannot be empty, null or white space.");

            Semaphore namedSemaphore = null;
            bool doesNotExist = false;
            bool unauthorized = false;

            if (initialCount < 0)
                initialCount = maximumCount;

            // Create a semaphore name that is specific to an object (e.g., a path and file name).
            string semaphoreName = Cipher.GetPasswordHash(name.ToLower(), SemaphoreHash).Replace('\\', '-');

            // Attempt to open the named semaphore
            try
            {
                namedSemaphore = Semaphore.OpenExisting(semaphoreName);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                doesNotExist = true;
            }
            catch (UnauthorizedAccessException)
            {
                unauthorized = true;
            }

            // Mono Semaphore implementations do not include ability to change access rules
#if MONO
            // If semaphore does not exist we create it
            if (doesNotExist || unauthorized)
            {
                try
                {
                    bool semaphoreWasCreated;

                    namedSemaphore = new Semaphore(initialCount, maximumCount, semaphoreName, out semaphoreWasCreated);

                    if (!semaphoreWasCreated)
                        throw new InvalidOperationException("Failed to create semaphore.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new InvalidOperationException("Failed to create semaphore: " + ex.Message);
                }
            }
#else
            // If semaphore does not exist we create it
            if (doesNotExist)
            {
                try
                {
                    SemaphoreSecurity security = new SemaphoreSecurity();
                    bool semaphoreWasCreated;

                    security.AddAccessRule(new SemaphoreAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), SemaphoreRights.FullControl, AccessControlType.Allow));
                    namedSemaphore = new Semaphore(initialCount, maximumCount, semaphoreName, out semaphoreWasCreated, security);

                    if (!semaphoreWasCreated)
                        throw new InvalidOperationException("Failed to create semaphore.");
                }
                catch (UnauthorizedAccessException)
                {
                    SemaphoreSecurity security = new SemaphoreSecurity();
                    bool semaphoreWasCreated;

                    security.AddAccessRule(new SemaphoreAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), SemaphoreRights.Synchronize | SemaphoreRights.Modify, AccessControlType.Allow));
                    namedSemaphore = new Semaphore(initialCount, maximumCount, semaphoreName, out semaphoreWasCreated, security);

                    if (!semaphoreWasCreated)
                        throw new InvalidOperationException("Failed to create semaphore.");
                }
            }
            else if (unauthorized)
            {
                namedSemaphore = Semaphore.OpenExisting(semaphoreName, SemaphoreRights.ReadPermissions | SemaphoreRights.ChangePermissions);

                // Get the current ACL. This requires MutexRights.ReadPermission
                SemaphoreSecurity security = new SemaphoreSecurity();
                SemaphoreAccessRule rule = new SemaphoreAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), SemaphoreRights.FullControl, AccessControlType.Allow);
                security.RemoveAccessRule(rule);

                // Now grant specific user rights for less than full access
                rule = new SemaphoreAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), SemaphoreRights.Synchronize | SemaphoreRights.Modify, AccessControlType.Allow);
                security.AddAccessRule(rule);

                // Update the ACL. This requires MutexRighs.ChangePermission.
                namedSemaphore.SetAccessControl(security);
                namedSemaphore = Semaphore.OpenExisting(semaphoreName);
            }
#endif

            return namedSemaphore;
        }
    }
}
