//******************************************************************************************************
//  EmailNotifier.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/16/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using GSF;
using GSF.Net.Smtp;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DynamicCalculator
{
    /// <summary>
    /// The EmailNotifier is an action adapter which takes multiple input measurements and defines
    /// a boolean expression such that when the expression is true an e-mail is triggered.
    /// </summary>
    [Description("E-Mail Notifier: Sends an e-mail based on a custom boolean expression")]
    public class EmailNotifier : DynamicCalculator
    {
        #region [ Members ]
        
        // Constants
        private const string DefaultExpressionText = "x > 0";
        private const int DefaultFramesPerSecond = 30;
        private const double DefaultLagTime = 5.0D;
        private const double DefaultLeadTime = 5.0D;

        // Fields
        private readonly Mail m_mailClient;
        private long m_expressionSuccesses;
        private long m_expressionFailures;
        private long m_totalEmailOperations;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="EmailNotifier"/>.
        /// </summary>
        public EmailNotifier()
        {
            m_mailClient = new Mail();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the textual representation of the boolean expression.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Define the boolean expression used to determine if an e-mail should be sent.")]
        [DefaultValue(DefaultExpressionText)]
        public new string ExpressionText // Redeclared to provide a more relevant description and example value for this adapter
        {
            get => base.ExpressionText;
            set => base.ExpressionText = value;
        }

        /// <summary>
        /// Gets or sets the e-mail address of the message sender.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value being assigned is a null or empty string.</exception>
        [ConnectionStringParameter,
        Description("Define the e-mail address of the message sender.")]
        public string From
        {
            get => m_mailClient.From;
            set => m_mailClient.From = value;
        }

        /// <summary>
        /// Gets or sets the comma-separated or semicolon-separated e-mail address list of the message recipients.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value being assigned is a null or empty string.</exception>
        [ConnectionStringParameter,
        Description("Define the comma-separated or semicolon-separated e-mail address list of the e-mail message recipients."),
        DefaultValue("")]
        public string ToRecipients
        {
            get => m_mailClient.ToRecipients;
            set => m_mailClient.ToRecipients = value;
        }

        /// <summary>
        /// Gets or sets the comma-separated or semicolon-separated e-mail address list of the message carbon copy (CC) recipients.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the comma-separated or semicolon-separated e-mail address list of the e-mail message carbon copy (CC) recipients."),
        DefaultValue("")]
        public string CcRecipients
        {
            get => m_mailClient.CcRecipients;
            set => m_mailClient.CcRecipients = value;
        }

        /// <summary>
        /// Gets or sets the comma-separated or semicolon-separated e-mail address list of the message blank carbon copy (BCC) recipients.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the comma-separated or semicolon-separated e-mail address list of the e-mail message blank carbon copy (BCC) recipients."),
        DefaultValue("")]
        public string BccRecipients
        {
            get => m_mailClient.BccRecipients;
            set => m_mailClient.BccRecipients = value;
        }

        /// <summary>
        /// Gets or sets the subject of the message.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the subject of the e-mail message.")]
        public string Subject
        {
            get => m_mailClient.Subject;
            set => m_mailClient.Subject = value;
        }

        /// <summary>
        /// Gets or sets the body of the message.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the body of the e-mail message.")]
        public string Body
        {
            get => m_mailClient.Body;
            set => m_mailClient.Body = value;
        }

        /// <summary>
        /// Gets or sets the name or IP address of the SMTP server to be used for sending the message.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value being assigned is a null or empty string.</exception>
        [ConnectionStringParameter,
        Description("Define the name or IP address of the SMTP server to be used for sending the e-mail message.")]
        public string SmtpServer
        {
            get => m_mailClient.SmtpServer;
            set => m_mailClient.SmtpServer = value;
        }

        /// <summary>
        /// Gets or sets a boolean value that indicating whether the message body is to be formatted as HTML.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the boolean value that indicating whether the message body is to be formatted as HTML."),
        DefaultValue(false)]
        public bool IsBodyHtml
        {
            get => m_mailClient.IsBodyHtml;
            set => m_mailClient.IsBodyHtml = value;
        }

        /// <summary>
        /// Gets or sets the username used to authenticate to the SMTP server.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the username used to authenticate to the SMTP server."),
        DefaultValue("")]
        public string Username
        {
            get => m_mailClient.Username;
            set => m_mailClient.Username = value;
        }

        /// <summary>
        /// Gets or sets the password used to authenticate to the SMTP server.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the password used to authenticate to the SMTP server."),
        DefaultValue("")]
        public string Password
        {
            get => m_mailClient.Password;
            set => m_mailClient.Password = value;
        }

        /// <summary>
        /// Gets or sets the flag that determines whether to use SSL when communicating with the SMTP server.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag that determines whether to use SSL when communicating with the SMTP server."),
        DefaultValue(false)]
        public bool EnableSSL
        {
            get => m_mailClient.EnableSSL;
            set => m_mailClient.EnableSSL = value;
        }

        /// <summary>
        /// Gets or sets the number of frames per second.
        /// </summary>
        /// <remarks>
        /// Valid frame rates for a <see cref="ConcentratorBase"/> are greater than 0 frames per second.
        /// </remarks>
        [ConnectionStringParameter]
        [Description("Defines the number of frames per second expected by the adapter.")]
        [DefaultValue(DefaultFramesPerSecond)]
        public new int FramesPerSecond // Redeclared to provide a default value since property is not commonly used
        {
            get => base.FramesPerSecond;
            set => base.FramesPerSecond = value;
        }

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to past measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// <para>This becomes the amount of delay introduced by the concentrator to allow time for data to flow into the system.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one.</exception>
        [ConnectionStringParameter]
        [Description("Defines the allowed past time deviation tolerance, in seconds (can be sub-second).")]
        [DefaultValue(DefaultLagTime)]
        public new double LagTime // Redeclared to provide a default value since property is not commonly used
        {
            get => base.LagTime;
            set => base.LagTime = value;
        }

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to future measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// <para>This becomes the tolerated +/- accuracy of the local clock to real-time.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one.</exception>
        [ConnectionStringParameter]
        [Description("Defines the allowed future time deviation tolerance, in seconds (can be sub-second).")]
        [DefaultValue(DefaultLeadTime)]
        public new double LeadTime // Redeclared to provide a default value since property is not commonly used
        {
            get => base.LeadTime;
            set => base.LeadTime = value;
        }

        /// <summary>
        /// Gets or sets output measurements that the action adapter will produce, if any.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IMeasurement[] OutputMeasurements // Redeclared to hide property - not relevant to this adapter
        {
            get => base.OutputMeasurements;
            set => base.OutputMeasurements = value;
        }

        /// <summary>
        /// Gets or sets the source of the timestamps of the calculated values.
        /// </summary>
        public new TimestampSource TimestampSource // Redeclared to hide property - not relevant to this adapter
        {
            get => base.TimestampSource;
            set => base.TimestampSource = value;
        }

        /// <summary>
        /// Gets flag that determines if the implementation of the <see cref="DynamicCalculator"/> requires an output measurement.
        /// </summary>
        protected override bool ExpectsOutputMeasurement => false;

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine();
                status.AppendFormat("      Expression Successes: {0:N0}", m_expressionSuccesses);
                status.AppendLine();
                status.AppendFormat("       Expression Failures: {0:N0}", m_expressionFailures);
                status.AppendLine();
                status.AppendFormat("   Total E-mail Operations: {0:N0}", m_totalEmailOperations);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="EmailNotifier"/>.
        /// </summary>
        public override void Initialize()
        {
            const string MissingRequiredMailSetting = "Missing required e-mail setting: \"{0}\"";
            Dictionary<string, string> settings = Settings;

            if (!settings.TryGetValue(nameof(ExpressionText), out _))
                settings[nameof(ExpressionText)] = DefaultExpressionText;

            if (!settings.TryGetValue(nameof(FramesPerSecond), out _))
                settings[nameof(FramesPerSecond)] = DefaultFramesPerSecond.ToString();

            if (!settings.TryGetValue(nameof(LagTime), out _))
                settings[nameof(LagTime)] = DefaultLagTime.ToString();

            if (!settings.TryGetValue(nameof(LeadTime), out _))
                settings[nameof(LeadTime)] = DefaultLeadTime.ToString();

            base.Initialize();

            // Load required mail settings
            if (settings.TryGetValue("from", out string setting) && !string.IsNullOrWhiteSpace(setting))
                From = setting;
            else
                throw new ArgumentException(string.Format(MissingRequiredMailSetting, "from"));

            if (settings.TryGetValue("subject", out setting) && !string.IsNullOrWhiteSpace(setting))
                Subject = setting;
            else
                throw new ArgumentException(string.Format(MissingRequiredMailSetting, "subject"));

            if (settings.TryGetValue("body", out setting) && !string.IsNullOrWhiteSpace(setting))
                Body = setting;
            else
                throw new ArgumentException(string.Format(MissingRequiredMailSetting, "body"));

            if (settings.TryGetValue("smtpServer", out setting) && !string.IsNullOrWhiteSpace(setting))
                SmtpServer = setting;
            else
                throw new ArgumentException(string.Format(MissingRequiredMailSetting, "smtpServer"));

            // Load optional mail settings
            if (settings.TryGetValue("toRecipients", out setting) && !string.IsNullOrWhiteSpace(setting))
                ToRecipients = setting;

            if (settings.TryGetValue("ccRecipients", out setting) && !string.IsNullOrWhiteSpace(setting))
                CcRecipients = setting;

            if (settings.TryGetValue("bccRecipients", out setting) && !string.IsNullOrWhiteSpace(setting))
                BccRecipients = setting;

            if (string.IsNullOrWhiteSpace(ToRecipients) && string.IsNullOrWhiteSpace(CcRecipients) && string.IsNullOrWhiteSpace(BccRecipients))
                throw new ArgumentException("At least one destination e-mail address for one of ToRecipients, CcRecipients or BccRecipients must be defined");

            if (settings.TryGetValue("isBodyHtml", out setting) && !string.IsNullOrWhiteSpace(setting))
                IsBodyHtml = setting.ParseBoolean();

            if (settings.TryGetValue("username", out setting) && !string.IsNullOrWhiteSpace(setting))
                Username = setting;

            if (settings.TryGetValue("password", out setting) && !string.IsNullOrWhiteSpace(setting))
                Password = setting;

            if (settings.TryGetValue("enableSSL", out setting) && !string.IsNullOrWhiteSpace(setting))
                EnableSSL = setting.ParseBoolean();
        }

        /// <summary>
        /// Handler for the values calculated by the <see cref="DynamicCalculator"/>.
        /// </summary>
        /// <param name="value">The value calculated by the <see cref="DynamicCalculator"/>.</param>
        protected override void HandleCalculatedValue(object value)
        {            
            if (value.ToString().ParseBoolean())
            {
                m_expressionSuccesses++;
                m_mailClient.Send();
                m_totalEmailOperations++;
            }
            else
            {
                m_expressionFailures++;
            }
        }

        #endregion
    }
}