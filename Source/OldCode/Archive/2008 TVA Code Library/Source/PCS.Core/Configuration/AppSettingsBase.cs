using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCS.Configuration
{
    /// <summary>
    /// Represents the base class for application settings that are synchronized with the "appSettings" section in a configuration file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In order to make custom types serializable for the configuration file, implement a <see cref="System.ComponentModel.TypeConverter"/> for the type.<br/>
    /// See <a href="http://msdn.microsoft.com/en-us/library/ayybcxe5.aspx">MSDN</a> for details.
    /// </para>
    /// <example>
    /// Here is an example class derived from <see cref="AppSettingsBase"/> that automatically
    /// serializes its fields and properties to the configuration file.
    /// <code>
    ///    public enum MyEnum
    ///     {
    ///         One,
    ///         Two,
    ///         Three
    ///     }
    /// 
    ///     public class MySettings : AppSettingsBase
    ///     {
    ///         // Private property fields (private fields will not be serialized)
    ///         private double m_doubleVal = 1.159D;
    /// 
    ///         // Public settings fields
    ///         public bool BoolVal = true;
    ///         public int IntVal = 1;
    ///         public float FloatVal = 3.14F;
    ///         public string StrVal = "This is a test...";
    ///         public MyEnum EnumVal = MyEnum.Three;
    /// 
    ///         // Mark this field to not be serialized to configuration file...
    ///         [SerializeSetting(false)]
    ///         public decimal DecimalVal;
    /// 
    ///         public double DoubleVal
    ///         {
    ///             get
    ///             {
    ///                 return m_doubleVal;
    ///             }
    ///             set
    ///             {
    ///                 m_doubleVal = value;
    ///             }
    ///         }
    /// 
    ///         [SerializeSetting(false)]
    ///         public bool DontSerializeMe { get; set; }
    ///     }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class AppSettingsBase : SettingsBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AppSettingsBase"/> class for the application's configuration file.
        /// </summary>
        public AppSettingsBase()
            : this(ConfigurationFile.Current, false)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AppSettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="configFile">Configuration file used for accessing settings.</param>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the configuration file.
        /// </param>
        public AppSettingsBase(ConfigurationFile configFile, bool requireSerializeSettingAttribute)
            : base(requireSerializeSettingAttribute)
        {
            ConfigFile = configFile;

            // Define delegates used to access and create settings in configuration file
            Getter = setting => ConfigFile.AppSettings.Settings[setting].Value;
            Setter = (setting, value) => ConfigFile.AppSettings.Settings[setting].Value = value;
            Creator = (setting, value) => { if (Getter(setting) == null) Setter(setting, value); };

            // Make sure settings exist and load current values
            Initialize();
        }

        #endregion
    }
}
