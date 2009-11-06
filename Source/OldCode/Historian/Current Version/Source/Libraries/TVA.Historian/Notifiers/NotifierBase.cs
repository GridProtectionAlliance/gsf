//*******************************************************************************************************
//  NotifierBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/26/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//  08/06/2009 - Pinal C. Patel
//       Made Initialize() virtual so inheriting classes can override the default behavior.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/05/2009 - Pinal C. Patel
//       Modified to abort notify operation during dispose.
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

using System;
using System.Threading;
using TVA.Configuration;

namespace TVA.Historian.Notifiers
{
    /// <summary>
    /// Base class for a notifier that can process notification messages.
    /// </summary>
    /// <see cref="NotificationTypes"/>
    public abstract class NotifierBase : INotifier
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when a notification is being sent.
        /// </summary>
        public event EventHandler NotificationSendStart;

        /// <summary>
        /// Occurs when a notification has been sent.
        /// </summary>
        public event EventHandler NotificationSendComplete;

        /// <summary>
        /// Occurs when a timeout is encountered while sending a notification.
        /// </summary>
        public event EventHandler NotificationSendTimeout;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while sending a notification.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception encountered while sending a notification.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> NotificationSendException;

        // Fields
        private int m_notifyTimeout;
        private NotificationTypes m_notifyOptions;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private Thread m_notifyThread;
        private bool m_enabled;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the notifier.
        /// </summary>
        /// <param name="notifyOptions"><see cref="NotificationTypes"/> that can be processed by the notifier.</param>
        public NotifierBase(NotificationTypes notifyOptions)
        {
            m_notifyOptions = notifyOptions;
            m_notifyTimeout = 30;
            m_persistSettings = true;
            m_settingsCategory = this.GetType().Name;
        }

        /// <summary>
        /// Releases the unmanaged resources before the notifier is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~NotifierBase()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of seconds to wait for <see cref="Notify"/> to complete.
        /// </summary>
        /// <remarks>
        /// Set <see cref="NotifyTimeout"/> to -1 to wait indefinitely on <see cref="Notify"/>.
        /// </remarks>
        public int NotifyTimeout
        {
            get
            {
                return m_notifyTimeout;
            }
            set
            {
                if (value < 1)
                    m_notifyTimeout = -1;
                else
                    m_notifyTimeout = value;
            }
        }

        /// <summary>
        /// Gets or set <see cref="NotificationTypes"/> that can be processed by the notifier.
        /// </summary>
        public NotificationTypes NotifyOptions
        {
            get
            {
                return m_notifyOptions;
            }
            set
            {
                m_notifyOptions = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the notifier is currently enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the notifier settings are to be saved to the config file.
        /// </summary>
        public bool PersistSettings
        {
            get
            {
                return m_persistSettings;
            }
            set
            {
                m_persistSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the category under which the notifier settings are to be saved to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                m_settingsCategory = value;
            }
        }

        #endregion

        #region [ Methods ]

        #region [ Abstract ]

        /// <summary>
        /// When overridden in a derived class, processes a <see cref="NotificationTypes.Alarm"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        protected abstract void NotifyAlarm(string subject, string message, string details);

        /// <summary>
        /// When overridden in a derived class, processes a <see cref="NotificationTypes.Warning"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        protected abstract void NotifyWarning(string subject, string message, string details);

        /// <summary>
        /// When overridden in a derived class, processes a <see cref="NotificationTypes.Information"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        protected abstract void NotifyInformation(string subject, string message, string details);

        /// <summary>
        /// When overridden in a derived class, processes a <see cref="NotificationTypes.Heartbeat"/> notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        protected abstract void NotifyHeartbeat(string subject, string message, string details);

        #endregion

        /// <summary>
        /// Releases all the resources used by the notifier.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the notifier.
        /// </summary>
        public virtual void Initialize()
        {
            if (!m_initialized)
            {
                LoadSettings();         // Load settings from the config file.
                m_initialized = true;   // Initialize only once.
            }
        }

        /// <summary>
        /// Saves notifier settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public virtual void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElement element = null;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                element = settings["Enabled", true];
                element.Update(m_enabled, element.Description, element.Encrypted);
                element = settings["NotifyTimeout", true];
                element.Update(m_notifyTimeout, element.Description, element.Encrypted);
                element = settings["NotifyOptions", true];
                element.Update(m_notifyOptions, element.Description, element.Encrypted);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved notifier settings from the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>        
        public virtual void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Add("Enabled", m_enabled, "True if this notifier is enabled; otherwise False.");
                settings.Add("NotifyTimeout", m_notifyTimeout, "Number of seconds to wait for notification processing to complete.");
                settings.Add("NotifyOptions", m_notifyOptions, "Types of notifications (Information; Warning; Alarm; Heartbeat) to be processed by this notifier.");
                Enabled = settings["Enabled"].ValueAs(m_enabled);
                NotifyTimeout = settings["NotifyTimeout"].ValueAs(m_notifyTimeout);
                NotifyOptions = settings["NotifyOptions"].ValueAs(m_notifyOptions);
            }
        }

        /// <summary>
        /// Process a notification.
        /// </summary>
        /// <param name="subject">Subject matter for the notification.</param>
        /// <param name="message">Brief message for the notification.</param>
        /// <param name="details">Detailed message for the notification.</param>
        /// <param name="notificationType">One of the <see cref="NotificationTypes"/> values.</param>
        /// <returns>true if notification is processed successfully; otherwise false.</returns>
        public bool Notify(string subject, string message, string details, NotificationTypes notificationType)
        {
            if (!m_enabled || (m_notifyThread != null && m_notifyThread.IsAlive))
                return false;

            // Start notification thread with appropriate parameters.
            m_notifyThread = new Thread(NotifyInternal);
            if ((notificationType & NotificationTypes.Alarm) == NotificationTypes.Alarm &&
                (m_notifyOptions & NotificationTypes.Alarm) == NotificationTypes.Alarm)
                // Alarm notifications are supported.
                m_notifyThread.Start(new object[] { new Action<string, string, string>(NotifyAlarm) , subject, message, details});
            else if ((notificationType & NotificationTypes.Warning) == NotificationTypes.Warning && 
                     (m_notifyOptions & NotificationTypes.Warning) == NotificationTypes.Warning)
                // Warning notifications are supported.
                m_notifyThread.Start(new object[] { new Action<string, string, string>(NotifyWarning), subject, message, details });
            else if ((notificationType & NotificationTypes.Information) == NotificationTypes.Information && 
                     (m_notifyOptions & NotificationTypes.Information) == NotificationTypes.Information)
                // Information notifications are supported.
                m_notifyThread.Start(new object[] { new Action<string, string, string>(NotifyInformation), subject, message, details });
            else if ((notificationType & NotificationTypes.Heartbeat) == NotificationTypes.Heartbeat && 
                     (m_notifyOptions & NotificationTypes.Heartbeat) == NotificationTypes.Heartbeat)
                // Heartbeat notifications are supported.
                m_notifyThread.Start(new object[] { new Action<string, string, string>(NotifyHeartbeat), subject, message, details });
            else
                // Specified notification type is not supported.
                return false;

            if (m_notifyTimeout < 1)
            {
                // Wait indefinetely on the refresh.
                m_notifyThread.Join(Timeout.Infinite);
            }
            else
            {
                // Wait for the specified time on refresh.
                if (!m_notifyThread.Join(m_notifyTimeout * 1000))
                {
                    m_notifyThread.Abort();

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the notifier and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.

                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        SaveSettings();

                        if (m_notifyThread != null)
                            m_notifyThread.Abort();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="NotificationSendStart"/> event.
        /// </summary>
        protected virtual void OnNotificationSendStart()
        {
            if (NotificationSendStart != null)
                NotificationSendStart(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="NotificationSendComplete"/> event.
        /// </summary>
        protected virtual void OnNotificationSendComplete()
        {
            if (NotificationSendComplete != null)
                NotificationSendComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="NotificationSendTimeout"/> event.
        /// </summary>
        protected virtual void OnNotificationSendTimeout()
        {
            if (NotificationSendTimeout != null)
                NotificationSendTimeout(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="NotificationSendException"/> event.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> to send to <see cref="NotificationSendException"/> event.</param>
        protected virtual void OnNotificationSendException(Exception exception)
        {
            if (NotificationSendException != null)
                NotificationSendException(this, new EventArgs<Exception>(exception));
        }

        private void NotifyInternal(object state)
        {
            try
            {
                // Unpackage the parameters.
                object[] args = (object[])state;
                string subject = args[1].ToString();
                string message = args[2].ToString();
                string details = args[3].ToString();
                Action<string, string, string> target = (Action<string, string, string>)args[0];

                OnNotificationSendStart();
                target(subject, message, details);
                OnNotificationSendComplete();
            }
            catch (ThreadAbortException)
            {
                OnNotificationSendTimeout();
            }
            catch (Exception ex)
            {
                OnNotificationSendException(ex);
            }
        }

        #endregion
    }
}