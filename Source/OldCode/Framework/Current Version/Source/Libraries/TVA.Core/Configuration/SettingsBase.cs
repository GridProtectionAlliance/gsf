//*******************************************************************************************************
//  SettingsBase.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/30/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  03/31/2009 - J. Ritchie Carroll
//       Added code to allow override of name used to serialize field or property to configuration file
//          by applying a SettingNameAttribute to the member.
//  04/01/2009 - J. Ritchie Carroll
//       Added code to optionally encrypt settings based on EncryptSettingAttribute and to pickup
//          DefaultValueAttribute value if provided and current value was uninitialized.
//  08/05/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using TVA.Reflection;
using TVA.Security.Cryptography;

namespace TVA.Configuration
{
    /// <summary>
    /// Represents the base class for application settings that are synchronized with its configuration file.
    /// </summary>
    /// <remarks>
    /// In order to make custom types serializable for the configuration file, implement a <see cref="TypeConverter"/> for the type.<br/>
    /// See <a href="http://msdn.microsoft.com/en-us/library/ayybcxe5.aspx">MSDN</a> for details.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class SettingsBase : IDisposable, IEnumerable<string>, IEnumerable
    {
        #region [ Members ]

        // Constants

        // IMPORTANT! Never change the following constant or you will break backwards compatibility
        private const string InternalKey = "§g¥J2&5:0xDr;£l8fL?C¡dF4?6c«u%ª±n¤9µ%î]a4@³,ÜaD*{©[1P.¢7_~`4rËd®!5:1j=)Al6¦)a#2¦Ï3E?¤(^\\dz$|¶TÁ+";

        // Fields
        private BindingFlags m_memberAccessBindingFlags;
        private bool m_requireSerializeSettingAttribute;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the configuration file.
        /// </param>
        protected SettingsBase(bool requireSerializeSettingAttribute)
        {
            m_requireSerializeSettingAttribute = requireSerializeSettingAttribute;
            m_memberAccessBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="CategorizedSettingsBase"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~SettingsBase()
        {
            // If user failed to dispose class, we make sure settings get saved...
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="BindingFlags"/> used to access fields and properties of dervied class.
        /// </summary>
        /// <remarks>
        /// Value defaults to <c><see cref="BindingFlags.Public"/> | <see cref="BindingFlags.Instance"/> | <see cref="BindingFlags.DeclaredOnly"/></c>.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual BindingFlags MemberAccessBindingFlags
        {
            get
            {
                return m_memberAccessBindingFlags;
            }
            set
            {
                m_memberAccessBindingFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="SerializeSettingAttribute"/> is
        /// required to exist before a field or property is serialized to the configuration
        /// file; defaults to False.
        /// </summary>
        public bool RequireSerializeSettingAttribute
        {
            get
            {
                return m_requireSerializeSettingAttribute;
            }
            set
            {
                m_requireSerializeSettingAttribute = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the specified field or property.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns>Value of setting.</returns>
        /// <remarks>This is the default member of this class.</remarks>
        public string this[string name]
        {
            get
            {
                return GetValue<string>(name);
            }
            set
            {
                SetValue(name, value);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="CategorizedSettingsBase"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="CategorizedSettingsBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // We'll make sure settings are saved when class is properly disposed...
                    if (disposing)
                        Save();
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Implementor should create setting in configuration file (or other location).
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="value">Setting value.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected abstract void CreateSetting(string name, string setting, string value);

        /// <summary>
        /// Implementor should retrieve setting from configuration file (or other location).
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <returns>Setting value.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected abstract string RetrieveSetting(string name, string setting);

        /// <summary>
        /// Implementor should store setting to configuration file (or other location).
        /// </summary>
        /// <param name="name">Field or property name, if useful (can be different from setting name).</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="value">Setting value.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected abstract void StoreSetting(string name, string setting, string value);

        /// <summary>
        /// Implementor should persist any pending changes to configuration file (or other location).
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected abstract void PersistSettings();

        /// <summary>
        /// Gets setting name to use for specified field or property. 
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns><see cref="SettingNameAttribute.Name"/> applied to specified field or property; or <paramref name="name"/> if attribute does not exist.</returns>
        /// <remarks>
        /// Field or property name will be used for setting name unless user applied a <see cref="SettingNameAttribute"/>
        /// on the field or property to override name used to serialize value in configuration file.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or empty.</exception>
        public string GetSettingName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty");

            return GetAttributeValue<SettingNameAttribute, string>(name, name, attribute => attribute.Name).NotEmpty(name);
        }

        /// <summary>
        /// Gets the default value specified by <see cref="DefaultValueAttribute"/>, if any, applied to the specified field or property. 
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns>Default value applied to specified field or property; or null if one does not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or empty.</exception>
        public object GetDefaultValue(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty");

            return GetAttributeValue<DefaultValueAttribute, object>(name, null, attribute => attribute.Value);
        }

        /// <summary>
        /// Gets the encryption status specified by <see cref="EncryptSettingAttribute"/>, if any, applied to the specified field or property. 
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns>Encryption status applied to specified field or property; or <c>false</c> if one does not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or empty.</exception>
        public bool GetEncryptStatus(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty");

            return GetAttributeValue<EncryptSettingAttribute, bool>(name, false, attribute => attribute.Encrypt);
        }

        /// <summary>
        /// Gets the optional private encryption key specified by <see cref="EncryptSettingAttribute"/>, if any, applied to the specified field or property. 
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <returns>Encryption private key applied to specified field or property; or <c>null</c> if one does not exist.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> cannot be null or empty.</exception>
        public string GetEncryptKey(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty");

            return GetAttributeValue<EncryptSettingAttribute, string>(name, null, attribute => attribute.PrivateKey);
        }

        /// <summary>
        /// Adds a setting to the application's configuration file, if it doesn't already exist.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <param name="value">Setting value.</param>
        /// <remarks>
        /// Use this function to ensure a setting exists, it will not override an existing value.
        /// </remarks>
        public void CreateValue(string name, object value)
        {
            string setting = GetSettingName(name);

            if (value == null)
                CreateSetting(name, setting, EncryptValue(name, setting, ""));
            else
                CreateSetting(name, setting, EncryptValue(name, setting, Common.TypeConvertToString(value)));
        }

        /// <summary>
        /// Copies the given value into the specified application setting.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <param name="value">Setting value.</param>
        public void SetValue(string name, object value)
        {
            string setting = GetSettingName(name);

            if (value == null)
                StoreSetting(name, setting, EncryptValue(name, setting, ""));
            else
                StoreSetting(name, setting, EncryptValue(name, setting, Common.TypeConvertToString(value)));
        }

        /// <summary>
        /// Gets the application's configuration file setting converted to the given type.
        /// </summary>
        /// <typeparam name="T">Type to use for setting conversion.</typeparam>
        /// <param name="name">Field or property name.</param>
        /// <returns>Value of specified configuration file setting converted to the given type.</returns>
        public T GetValue<T>(string name)
        {
            string setting = GetSettingName(name);

            return DecryptValue(name, setting, RetrieveSetting(name, setting)).ConvertToType<T>();
        }

        /// <summary>
        /// Gets the application's configuration file setting converted to the given type.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <param name="type">Setting type.</param>
        /// <returns>Value of specified configuration file setting converted to the given type.</returns>
        public object GetValue(string name, Type type)
        {
            string setting = GetSettingName(name);

            return DecryptValue(name, setting, RetrieveSetting(name, setting)).ConvertToType(type);
        }

        /// <summary>
        /// Copies the specified application setting into the given value.
        /// </summary>
        /// <typeparam name="T">Type to use for setting conversion.</typeparam>
        /// <param name="name">Field or property name.</param>
        /// <param name="value">Setting value.</param>
        public void GetValue<T>(string name, out T value)
        {
            string setting = GetSettingName(name);

            value = DecryptValue(name, setting, RetrieveSetting(name, setting)).ConvertToType<T>();
        }

        // Encrypt setting value and return a base64 encoded value
        private string EncryptValue(string name, string setting, string value)
        {
            // If encrypt attribute has been applied, encrypt value
            if (GetEncryptStatus(name))
                return value.Encrypt(name, CipherStrength.Aes256);

            return value;
        }

        // Decrypt setting value
        private string DecryptValue(string name, string setting, string value)
        {
            // If encrypt attribute has been applied, decrypt value
            if (GetEncryptStatus(name))
                return value.Decrypt(name, CipherStrength.Aes256);

            return value;
        }

        /// <summary>
        /// Initializes configuration settings from derived class fields or properties.
        /// </summary>
        protected virtual void Initialize()
        {
            // Make sure all desired settings exist initialized with default values. Settings are
            // assumed to be public fields or public properties in derived class - so we enumerate
            // through of these making sure a setting exists for each field and property

            // Verify a configuration setting exists for each field
            ExecuteActionForFields(field => CreateValue(field.Name, DeriveDefaultValue(field.Name, field.GetValue(this))));

            // Verify a configuration setting exists for each property
            ExecuteActionForProperties(property => CreateValue(property.Name, DeriveDefaultValue(property.Name, property.GetValue(this, null))), BindingFlags.GetProperty);

            // If any new values were encountered, make sure they are flushed to config file
            PersistSettings();

            // Load current settings
            Load();
        }

        /// <summary>
        /// Attempts to get best default value for given member.
        /// </summary>
        /// <param name="name">Field or property name.</param>
        /// <param name="value">Current field or property value.</param>
        /// <remarks>
        /// If <paramref name="value"/> is equal to its default(type) value, then any value derived from <see cref="DefaultValueAttribute"/> will be used instead.
        /// </remarks>
        /// <returns>The object that is the best default value.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected object DeriveDefaultValue(string name, object value)
        {
            // See if value is equal to its default value (i.e., uninitialized)
            if (Common.IsDefaultValue(value))
            {
                // See if any value exists in a DefaultValueAttribute
                object defaultValue = GetDefaultValue(name);
                if (defaultValue != null)
                    return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Returns an enumerator based on <see cref="String"/> elements that iterates over the field and property names of this class
        /// that are targeted for serialization to the configuration file.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        public IEnumerator<string> GetEnumerator()
        {
            List<string> members = new List<string>();

            // Get names of fields
            ExecuteActionForFields(field => members.Add(field.Name));

            // Get names of properties
            ExecuteActionForProperties(property => members.Add(property.Name), BindingFlags.GetProperty);

            return members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Loads configuration file into setting fields.
        /// </summary>
        public virtual void Load()
        {
            // Load configuration file settings into fields
            ExecuteActionForFields(field => field.SetValue(this, GetValue(field.Name, field.FieldType)));
            
            // Load configuration file settings into properties
            ExecuteActionForProperties(property => property.SetValue(this, GetValue(property.Name, property.PropertyType), null), BindingFlags.SetProperty);
        }

        /// <summary>
        /// Saves setting fields into configuration file.
        /// </summary>
        public virtual void Save()
        {
            // Saves setting fields into configuration file values
            ExecuteActionForFields(field => SetValue(field.Name, field.GetValue(this)));
            
            // Saves setting properties into configuration file values
            ExecuteActionForProperties(property => SetValue(property.Name, property.GetValue(this, null)), BindingFlags.GetProperty);

            // Make sure any changes are flushed to config file
            PersistSettings();
        }

        /// <summary>
        /// Executes specified action over all public dervied class member fields.
        /// </summary>
        /// <param name="fieldAction">Action to excute for all dervied class member fields.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void ExecuteActionForFields(Action<FieldInfo> fieldAction)
        {
            ExecuteActionForMembers(fieldAction, this.GetType().GetFields(m_memberAccessBindingFlags));
        }

        /// <summary>
        /// Executes specified action over all public dervied class properties.
        /// </summary>
        /// <param name="propertyAction">Action to execute for all properties.</param>
        /// <param name="isGetOrSet"><see cref="BindingFlags.GetProperty"/> or <see cref="BindingFlags.SetProperty"/>.</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected void ExecuteActionForProperties(Action<PropertyInfo> propertyAction, BindingFlags isGetOrSet)
        {
            // Make sure only non-indexer properties are used for settings
            ExecuteActionForMembers(property => { if (property.GetIndexParameters().Length == 0) propertyAction(property); }, this.GetType().GetProperties(m_memberAccessBindingFlags | isGetOrSet));
        }

        // Execute specified action over specified memembers
        private void ExecuteActionForMembers<T>(Action<T> memberAction, T[] members) where T : MemberInfo
        {
            SerializeSettingAttribute attribute;

            // Execute action for each member
            foreach (T member in members)
            {
                // See if serialize setting attribute exists
                if (member.TryGetAttribute(out attribute))
                {
                    // Found serialze setting attribute, perform action if setting is true
                    if (attribute.Serialize)
                        memberAction(member);
                }
                else if (!m_requireSerializeSettingAttribute)
                {
                    // Didn't find serialize setting attribute and it's not required, so we perform action
                    memberAction(member);
                }
            }
        }

        /// <summary>
        /// Attempts to find specified attribute and return specified value.
        /// </summary>
        /// <typeparam name="TAttribute">Type of <see cref="Attribute"/> to find.</typeparam>
        /// <typeparam name="TValue">Type of value attribute delegate returns.</typeparam>
        /// <param name="name">Name of field or property to search for attribute.</param>
        /// <param name="defaultValue">Default value to return if attribute doesn't exist.</param>
        /// <param name="attributeValue">Function delegate used to return desired attribute property.</param>
        /// <returns>Specified attribute value if it exists; otherwise default value.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected TValue GetAttributeValue<TAttribute, TValue>(string name, TValue defaultValue, Func<TAttribute, TValue> attributeValue) where TAttribute : Attribute
        {
            TAttribute attribute;

            // See if field exists with specified name
            FieldInfo field = this.GetType().GetField(name, m_memberAccessBindingFlags);

            if (field != null)
            {
                // See if attribute exists on field
                if (field.TryGetAttribute(out attribute))
                {
                    // Return value as specified by delegate
                    return attributeValue(attribute);
                }

                // Attribute wasn't found, return default value
                return defaultValue;
            }

            // See if property exists with specified name
            PropertyInfo property = this.GetType().GetProperty(name, m_memberAccessBindingFlags);

            if (property != null)
            {
                // See if attribute exists on property
                if (property.TryGetAttribute(out attribute))
                {
                    // Return value as specified by delegate
                    return attributeValue(attribute);
                }

                // Attribute wasn't found, return default value
                return defaultValue;
            }

            // Return default value
            return defaultValue;
        }

        #endregion
    }
}