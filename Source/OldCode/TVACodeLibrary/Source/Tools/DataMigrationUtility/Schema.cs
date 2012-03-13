//******************************************************************************************************
//  Schema.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/28/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/15/2008 - Mihir Brahmbhatt
//       Converted to C# extensions.
//  09/27/2010 - Mihir Brahmbhatt
//       Edited code comments.
//  12/07/2010 - Mihir Brahmbhatt
//       Changed SqlEncoded method to check proper numeric conversion value
//******************************************************************************************************

// James Ritchie Carroll - 2003
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Text;
using TVA.Data;

namespace Database
{
    #region [ Enumerations ]

    /// <summary>
    /// Specifies the type of Database
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// Database is sql server
        /// </summary>
        SqlServer,
        /// <summary>
        /// Database is oracle
        /// </summary>
        Oracle,
        /// <summary>
        /// Database is MySQL
        /// </summary>
        MySQL,
        /// <summary>
        /// Database is MS-Access
        /// </summary>
        Access,
        /// <summary>
        /// Database is not specified
        /// </summary>
        Unspecified
    }

    /// <summary>
    /// Specifies the type of object in database
    /// </summary>
    [Flags()]
    public enum TableType
    {
        /// <summary>
        /// Database object is DataTable
        /// </summary>
        Table = 1,
        /// <summary>
        /// Database object is View
        /// </summary>
        View = 2,
        /// <summary>
        /// Database object is System Defined Table
        /// </summary>
        SystemTable = 4,
        /// <summary>
        /// Database object is System Defined View
        /// </summary>
        SystemView = 8,
        /// <summary>
        /// Database object is Alias
        /// </summary>
        Alias = 16,
        /// <summary>
        /// Database object is Synonym
        /// </summary>
        Synonym = 32,
        /// <summary>
        /// Database object is Global Temp 
        /// </summary>
        GlobalTemp = 64,
        /// <summary>
        /// Database object is local Temp
        /// </summary>
        LocalTemp = 128,
        /// <summary>
        /// Dataobase object is Link
        /// </summary>
        Link = 256,
        /// <summary>
        /// Database object is not defined
        /// </summary>
        Undetermined = 512
    }

    /// <summary>
    /// Specified the type of Referentail Action on Database object/Tables
    /// </summary>
    public enum ReferentialAction
    {
        /// <summary>
        /// Action Type is Casecase
        /// </summary>
        Cascade,
        /// <summary>
        /// Action Type is to set null
        /// </summary>
        SetNull,
        /// <summary>
        /// Action Type is to set Default
        /// </summary>
        SetDefault,
        /// <summary>
        /// No Action
        /// </summary>
        NoAction
    }

    #endregion

    /// <summary>
    /// Represents a database field.
    /// </summary>
    public class Field : IComparable
    {
        #region [ Members ]

        //Fields
        private Fields m_parentField;
        private string m_fieldName;
        private OleDbType m_DataType;

        //internal int intOrdinal;
        //internal bool flgAllowsNulls;
        //internal bool flgAutoInc;
        //internal int intAutoIncSeed;
        //internal int intAutoIncStep;
        //internal bool flgHasDefault;
        //internal object objDefaultValue;
        //internal int intMaxLength;
        //internal int intNumericPrecision;
        //internal int intNumericScale;
        //internal int intDateTimePrecision;
        //internal bool flgReadOnly;
        //internal bool flgUnique;
        //internal string m_description;
        //internal Hashtable tblAutoIncTranslations;

        private int m_ordinal;
        private bool m_allowsNulls;
        private bool m_autoIncrement;
        private int m_autoIncrementSeed;
        private int m_autoIncrementStep;
        private bool m_hasDefault;
        private object m_defaultValue;
        private int m_maxLength;
        private int m_numericPrecision;
        private int m_numericScale;
        private int m_dateTimePrecision;
        private bool m_readOnly;
        private bool m_unique;
        private string m_description;
        private Hashtable m_autoIncrementTranslations;

        // This allows user to store a field value if desired
        //public object Value;
        //public bool IsPrimaryKey;
        //public int PrimaryKeyOrdinal;
        //public string PrimaryKeyName;
        //public ForeignKeyFields ForeignKeys;
        //public Field ReferencedBy;

        private object m_value;
        private bool m_isPrimaryKey;
        private int m_primaryKeyOrdinal;
        private string m_primaryKeyName;
        private ForeignKeyFields m_foreignKeys;
        private Field m_referencedBy;

        #endregion

        #region [ Constructors ]

        internal Field(Fields Parent, string Name, OleDbType Type)
        {
            // We only allow internal creation of this object
            m_parentField = Parent;
            m_fieldName = Name;
            m_DataType = Type;
            ForeignKeys = new ForeignKeyFields(this);

        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get Field Name 
        /// </summary>
        public string Name
        {
            get
            {
                return m_fieldName;
            }
        }

        /// <summary>
        /// Get <see cref="OleDbType"/> Type
        /// </summary>
        public OleDbType Type
        {
            get
            {
                return m_DataType;
            }
        }

        /// <summary>
        /// Get or set file ordinal
        /// </summary>
        public int Ordinal
        {
            get
            {
                return m_ordinal;
                //intOrdinal; 
            }
            internal set
            {
                m_ordinal = value;
            }
        }

        /// <summary>
        /// Get or Set Allow Null flag
        /// </summary>
        public bool AllowsNulls
        {
            get
            {
                return m_allowsNulls;
            }
            internal set
            {
                m_allowsNulls = value;
            }
        }

        /// <summary>
        /// Get or Set Auto increment flag
        /// </summary>
        public bool AutoIncrement
        {
            get
            {
                return m_autoIncrement;
            }
            internal set
            {
                m_autoIncrement = value;
            }
        }

        /// <summary>
        /// Get or Set Auto increment seed
        /// </summary>
        public int AutoIncrementSeed
        {
            get
            {
                return m_autoIncrementSeed;
            }
            internal set
            {
                m_autoIncrementSeed = value;
            }
        }

        /// <summary>
        /// Get or Set Auto increment step
        /// </summary>
        public int AutoIncrementStep
        {
            get
            {
                return m_autoIncrementStep;
            }
            internal set
            {
                m_autoIncrementStep = value;
            }
        }


        /// <summary>
        /// Get or Set has defult value flag
        /// </summary>
        public bool HasDefault
        {
            get
            {
                return m_hasDefault;
            }
            internal set
            {
                m_hasDefault = value;
            }
        }


        /// <summary>
        /// Get or Set default value
        /// </summary>
        public object DefaultValue
        {
            get
            {
                return m_defaultValue;
            }
            internal set
            {
                m_defaultValue = value;
            }
        }

        /// <summary>
        /// Get or Set maximum length of field
        /// </summary>
        public int MaxLength
        {
            get
            {
                return m_maxLength;
            }
            internal set
            {
                m_maxLength = value;
            }
        }


        /// <summary>
        /// Get or Set numeric precision
        /// </summary>
        public int NumericPrecision
        {
            get
            {
                return m_numericPrecision;
            }
            internal set
            {
                m_numericPrecision = value;
            }
        }

        /// <summary>
        /// Get or Set Numeric scale
        /// </summary>
        public int NumericScale
        {
            get
            {
                return m_numericScale;
            }
            internal set
            {
                m_numericScale = value;
            }
        }

        /// <summary>
        /// Get or Set date time precision
        /// </summary>
        public int DateTimePrecision
        {
            get
            {
                return m_dateTimePrecision;
            }
            internal set
            {
                m_dateTimePrecision = value;
            }
        }


        /// <summary>
        /// Get or Set readonly flag
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return m_readOnly;
            }
            internal set
            {
                m_readOnly = value;
            }
        }

        /// <summary>
        /// Get or Set for unique
        /// </summary>
        public bool Unique
        {
            get
            {
                return m_unique;
            }
            internal set
            {
                m_unique = value;
            }
        }

        /// <summary>
        /// Get or Set description
        /// </summary>
        public string Description
        {
            get
            {
                return m_description;
            }
            internal set
            {
                m_description = value;
            }
        }

        /// <summary>
        /// Get or Set auto increment translation
        /// </summary>
        internal Hashtable AutoIncrementTranslations
        {
            get
            {
                return m_autoIncrementTranslations;
            }
            set
            {
                m_autoIncrementTranslations = value;
            }
        }


        /// <summary>
        /// Get or Set value of <see cref="Field"/>
        /// </summary>
        public object Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        /// <summary>
        /// Get or Set flag to check <see cref="Field"/> is primary key or not
        /// </summary>
        public bool IsPrimaryKey
        {
            get
            {
                return m_isPrimaryKey;
            }
            set
            {
                m_isPrimaryKey = value;
            }
        }

        /// <summary>
        /// Get or Set ordinal for Primary key field
        /// </summary>
        public int PrimaryKeyOrdinal
        {
            get
            {
                return m_primaryKeyOrdinal;
            }
            set
            {
                m_primaryKeyOrdinal = value;
            }
        }

        /// <summary>
        /// Get or Set primary key name
        /// </summary>
        public string PrimaryKeyName
        {
            get
            {
                return m_primaryKeyName;
            }
            set
            {
                m_primaryKeyName = value;
            }
        }

        /// <summary>
        /// Get or Set list of <see cref="ForeignKeyFields"/>
        /// </summary>
        public ForeignKeyFields ForeignKeys
        {
            get
            {
                return m_foreignKeys;
            }
            set
            {
                m_foreignKeys = value;
            }
        }

        /// <summary>
        /// Get or Set - check <see cref="Field"/> is reference by
        /// </summary>
        public Field ReferencedBy
        {
            get
            {
                return m_referencedBy;
            }
            internal set
            {
                m_referencedBy = value;
            }
        }

        /// <summary>
        /// Get or Set foreign key flag. if <see cref="Field"/> is <see cref="ReferencedBy"/> then true else false
        /// </summary>
        public bool IsForeignKey
        {
            get
            {
                return (m_referencedBy != null);
            }
        }

        /// <summary>
        /// Get or Set <see cref="Fields"/> parent
        /// </summary>
        public Fields Parent
        {
            get
            {
                return m_parentField;
            }
        }

        /// <summary>
        /// Get or set <see cref="Field"/>'s parent <see cref="Table"/>
        /// </summary>
        public Table Table
        {
            get
            {
                return m_parentField.Parent;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Compare <paramref name="obj"/> ordinal to current field <see cref="Ordinal"/>
        /// </summary>
        /// <param name="obj">Check <paramref name="obj"/> type <see cref="Object"/>, if it is type of <see cref="Field"/> then compare to <see cref="Ordinal"/> of <paramref name="obj"/> else throw <see cref="ArgumentException"/></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            // Fields are sorted in ordinal position order
            if (obj is Field)
            {
                //return intOrdinal.CompareTo(((Field)obj).intOrdinal);
                return m_ordinal.CompareTo(((Field)obj).m_ordinal);
            }
            else
            {
                throw new ArgumentException("Field can only be compared to other Fields");
            }
        }

        /// <summary>
        /// Change <see cref="Field"/> value to encoded string. It will check <see cref="Type"/>  and <see cref="Parent"/> value before convert to <see cref="OleDbType"/> compatible value
        /// </summary>
        public string SqlEncodedValue
        {
            get
            {
                string strValue = "";
                //long tempValue;
                if (!Convert.IsDBNull(m_value))
                {
                    try
                    {
                        // Attempt to get string based source field value
                        strValue = m_value.ToString().Trim();

                        // Format field value based on field's data type
                        switch (m_DataType)
                        {
                            case OleDbType.BigInt:
                            case OleDbType.Integer:
                            case OleDbType.SmallInt:
                            case OleDbType.TinyInt:
                            case OleDbType.UnsignedBigInt:
                            case OleDbType.UnsignedInt:
                            case OleDbType.UnsignedSmallInt:
                            case OleDbType.UnsignedTinyInt:
                            case OleDbType.Error:
                                if (strValue.Length == 0)
                                {
                                    if (m_parentField.Parent.Parent.Parent.AllowNumericNulls)
                                    {
                                        strValue = "NULL";
                                    }
                                    else
                                    {
                                        strValue = "0";
                                    }
                                }
                                else
                                {
                                    Int64 tempValue = 0;
                                    if (Int64.TryParse(m_value.ToString(), out tempValue)) //(Information.IsNumeric(Value))
                                    {
                                        strValue = Convert.ToInt64(Value).ToString().Trim();
                                    }
                                    else
                                    {
                                        if (m_parentField.Parent.Parent.Parent.AllowNumericNulls)
                                        {
                                            strValue = "NULL";
                                        }
                                        else
                                        {
                                            strValue = "0";
                                        }
                                    }
                                }
                                break;
                            case OleDbType.Single:
                                if (strValue.Length == 0)
                                {
                                    if (m_parentField.Parent.Parent.Parent.AllowNumericNulls)
                                    {
                                        strValue = "NULL";
                                    }
                                    else
                                    {
                                        strValue = "0.0";
                                    }
                                }
                                else
                                {
                                    Single tempValue = 0;
                                    if (Single.TryParse(m_value.ToString(), out tempValue)) //if (Information.IsNumeric(Value))
                                    {
                                        strValue = Convert.ToSingle(Value).ToString().Trim();
                                    }
                                    else
                                    {
                                        if (m_parentField.Parent.Parent.Parent.AllowNumericNulls)
                                        {
                                            strValue = "NULL";
                                        }
                                        else
                                        {
                                            strValue = "0.0";
                                        }
                                    }
                                }
                                break;
                            case OleDbType.Double:
                                if (strValue.Length == 0)
                                {
                                    if (m_parentField.Parent.Parent.Parent.AllowNumericNulls)
                                    {
                                        strValue = "NULL";
                                    }
                                    else
                                    {
                                        strValue = "0.0";
                                    }
                                }
                                else
                                {
                                    Double tempValue = 0;
                                    if (Double.TryParse(m_value.ToString(), out tempValue)) //if (Information.IsNumeric(Value))
                                    {
                                        strValue = Convert.ToDouble(Value).ToString().Trim();
                                    }
                                    else
                                    {
                                        if (m_parentField.Parent.Parent.Parent.AllowNumericNulls)
                                        {
                                            strValue = "NULL";
                                        }
                                        else
                                        {
                                            strValue = "0.0";
                                        }
                                    }
                                }
                                break;
                            case OleDbType.Currency:
                            case OleDbType.Decimal:
                            case OleDbType.Numeric:
                            case OleDbType.VarNumeric:
                                if (strValue.Length == 0)
                                {
                                    if (m_parentField.Parent.Parent.Parent.AllowNumericNulls)
                                    {
                                        strValue = "NULL";
                                    }
                                    else
                                    {
                                        strValue = "0.00";
                                    }
                                }
                                else
                                {
                                    Decimal tempValue = 0;
                                    if (Decimal.TryParse(m_value.ToString(), out tempValue)) //if (Information.IsNumeric(Value))
                                    {
                                        strValue = Convert.ToDecimal(Value).ToString().Trim();
                                    }
                                    else
                                    {
                                        if (m_parentField.Parent.Parent.Parent.AllowNumericNulls)
                                        {
                                            strValue = "NULL";
                                        }
                                        else
                                        {
                                            strValue = "0.00";
                                        }
                                    }
                                }
                                break;
                            case OleDbType.Boolean:
                                if (strValue.Length == 0)
                                {
                                    if (m_parentField.Parent.Parent.Parent.AllowNumericNulls)
                                    {
                                        strValue = "NULL";
                                    }
                                    else
                                    {
                                        strValue = "0";
                                    }
                                }
                                else
                                {
                                    long tempValue = 0;
                                    if (long.TryParse(m_value.ToString(), out tempValue)) //if (Information.IsNumeric(strValue))
                                    {
                                        if (tempValue == 0)
                                        {
                                            strValue = "0";
                                        }
                                        else
                                        {
                                            strValue = "1";
                                        }
                                    }
                                    else
                                    {
                                        switch (char.ToUpper(strValue.Trim()[0]))
                                        {
                                            case 'Y':
                                            case 'T':
                                                strValue = "1";
                                                break;
                                            case 'N':
                                            case 'F':
                                                strValue = "0";
                                                break;
                                            default:
                                                if (m_parentField.Parent.Parent.Parent.AllowNumericNulls)
                                                {
                                                    strValue = "NULL";
                                                }
                                                else
                                                {
                                                    strValue = "0";
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;
                            case OleDbType.Char:
                            case OleDbType.WChar:
                            case OleDbType.VarChar:
                            case OleDbType.VarWChar:
                            case OleDbType.LongVarChar:
                            case OleDbType.LongVarWChar:
                            case OleDbType.BSTR:
                                if (strValue.Length == 0)
                                {
                                    if (m_parentField.Parent.Parent.Parent.AllowTextNulls)
                                    {
                                        strValue = "NULL";
                                    }
                                    else
                                    {
                                        strValue = "''";
                                    }
                                }
                                else
                                {
                                    strValue = "'" + strValue.SqlEncode() + "'";
                                }
                                break;
                            case OleDbType.DBTimeStamp:
                            case OleDbType.DBDate:
                            case OleDbType.Date:
                                if (strValue.Length > 0)
                                {
                                    DateTime tempDateTimeValue;
                                    if (DateTime.TryParse(m_value.ToString(), out tempDateTimeValue)) //if (Information.IsDate(strValue))
                                    {
                                        switch (m_parentField.Parent.Parent.Parent.DataSourceType)
                                        {
                                            case DatabaseType.SqlServer:
                                                strValue = "'" + tempDateTimeValue.ToString("MM/dd/yyyy HH:mm:ss") + "'"; // Strings.Format((DateTime)strValue, "MM/dd/yyyy HH:mm:ss") + "'";
                                                break;
                                            case DatabaseType.Access:
                                                strValue = "#" + tempDateTimeValue.ToString("MM/dd/yyyy HH:mm:ss") + "#"; //+ Strings.Format((DateTime)strValue, "MM/dd/yyyy HH:mm:ss") + "#";
                                                break;
                                            default:
                                                strValue = "'" + tempDateTimeValue.ToString("dd-MMM-yyyy HH:mm:ss") + "'"; //+ Strings.Format((DateTime)strValue, "dd-MMM-yyyy HH:mm:ss") + "'";
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        strValue = "NULL";
                                    }
                                }
                                else
                                {
                                    strValue = "NULL";
                                }
                                break;
                            case OleDbType.DBTime:
                                if (strValue.Length > 0)
                                {
                                    DateTime tempDateTimeValue;
                                    if (DateTime.TryParse(m_value.ToString(), out tempDateTimeValue)) //if (Information.IsDate(strValue))
                                    {
                                        strValue = "'" + tempDateTimeValue.ToString("HH:mm:ss") + "'"; // Strings.Format((DateTime)strValue, "HH:mm:ss") + "'";
                                    }
                                    else
                                    {
                                        strValue = "NULL";
                                    }
                                }
                                else
                                {
                                    strValue = "NULL";
                                }
                                break;
                            case OleDbType.Filetime:
                                if (strValue.Length > 0)
                                {
                                    strValue = "'" + strValue + "'";
                                }
                                else
                                {
                                    strValue = "NULL";
                                }
                                break;
                            case OleDbType.Guid:
                                if (strValue.Length == 0)
                                {
                                    strValue = (new Guid()).ToString();
                                }

                                if (m_parentField.Parent.Parent.Parent.DataSourceType == DatabaseType.Access)
                                {
                                    strValue = "{" + strValue + "}";
                                }
                                else
                                {
                                    strValue = "'" + strValue + "'";
                                }
                                break;
                        }
                    }
                    catch //(Exception ex)
                    {
                        // We'll default to NULL if we failed to evaluate field data
                        strValue = "NULL";
                    }
                }

                if (strValue.Length == 0)
                    strValue = "NULL";

                return strValue;
            }
        }

        /// <summary>
        /// Get information about referential action
        /// </summary>
        /// <param name="RefAction">check <paramref name="RefAction"/> and return to appropriate <see cref="ReferentialAction"/>.</param>
        /// <returns></returns>
        static internal ReferentialAction GetReferentialAction(string RefAction)
        {

            switch (RefAction.Trim().ToUpper())
            {
                case "CASCADE":
                    return ReferentialAction.Cascade;
                case "SET NULL":
                    return ReferentialAction.SetNull;
                case "SET DEFAULT":
                    return ReferentialAction.SetDefault;
                case "NO ACTION":
                    return ReferentialAction.NoAction;
                default:
                    return ReferentialAction.NoAction;
            }

        }

        #endregion
    }

    /// <summary>
    /// Represents a database foreign key field.
    /// </summary>
    public class ForeignKeyField
    {
        #region [ Members ]

        private ForeignKeyFields m_Parent;

        private Field m_primaryKey;
        private Field m_foreignKey;
        private int m_ordinal;
        private string m_keyName;
        private ReferentialAction m_updateRule = ReferentialAction.NoAction;
        private ReferentialAction m_deleteRule = ReferentialAction.NoAction;

        #endregion

        #region [ Constructors ]

        internal ForeignKeyField(ForeignKeyFields Parent)
        {
            // We only allow internal creation of this object
            m_Parent = Parent;

        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get foreign key parent information
        /// </summary>
        public ForeignKeyFields Parent
        {
            get
            {
                return m_Parent;
            }
        }

        /// <summary>
        /// Get or Set Primary key field
        /// </summary>
        public Field PrimaryKey
        {
            get
            {
                return m_primaryKey;
            }
            set
            {
                m_primaryKey = value;
            }
        }

        /// <summary>
        /// Get or Set Foreign key field
        /// </summary>
        public Field ForeignKey
        {
            get
            {
                return m_foreignKey;
            }
            set
            {
                m_foreignKey = value;
            }
        }

        /// <summary>
        /// Get or Set ordinal of <see cref="Field"/>
        /// </summary>
        public int Ordinal
        {
            get
            {
                return m_ordinal;
            }
            set
            {
                m_ordinal = value;
            }
        }

        /// <summary>
        /// Get or Set name of key
        /// </summary>
        public string KeyName
        {
            get
            {
                return m_keyName;
            }
            set
            {
                m_keyName = value;
            }
        }

        /// <summary>
        /// Get or Set update rule for <see cref="ReferentialAction"/> for <see cref="Field"/>
        /// </summary>
        public ReferentialAction UpdateRule
        {
            get
            {
                return m_updateRule;
            }
            set
            {
                m_updateRule = value;
            }
        }

        /// <summary>
        /// Get or Set delete rule for <see cref="ReferentialAction"/> for <see cref="Field"/>
        /// </summary>
        public ReferentialAction DeleteRule
        {
            get
            {
                return m_deleteRule;
            }
            set
            {
                m_deleteRule = value;
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a collection of <see cref="ForeignKeyField"/> values.
    /// </summary>
    public class ForeignKeyFields
    {
        #region [ Members ]

        private Field m_parent;
        // Used for field name lookups
        private Dictionary<string, ForeignKeyField> m_fields;
        // Used for field index lookups
        private List<ForeignKeyField> m_fieldList;

        #endregion

        #region [ Constructors ]

        internal ForeignKeyFields(Field Parent)
        {
            // We only allow internal creation of this object
            m_parent = Parent;
            m_fields = new Dictionary<string, ForeignKeyField>(StringComparer.OrdinalIgnoreCase);
            m_fieldList = new List<ForeignKeyField>();

        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get or set field parent information
        /// </summary>
        public Field Parent
        {
            get
            {
                return m_parent;
            }
            //internal set
            //{
            //    m_Parent = value;
            //}
        }

        /// <summary>
        /// Get of Set Fields names to lookups
        /// </summary>
        internal Dictionary<string, ForeignKeyField> FieldDictionary
        {
            get
            {
                return m_fields;
            }
        }

        /// <summary>
        /// Get or Set field indexs to lookups
        /// </summary>
        internal List<ForeignKeyField> FieldsList
        {
            get
            {
                return m_fieldList;
            }
        }

        /// <summary>
        /// Get the current index of foreignkey field information
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ForeignKeyField this[int index]
        {
            get
            {
                if (index < 0 | index >= m_fieldList.Count)
                {
                    return null;
                }
                else
                {
                    return m_fieldList[index];
                }
            }
        }

        /// <summary>
        /// Get the current <see cref="ForeignKeyField"/> information by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ForeignKeyField this[string name]
        {
            get
            {
                ForeignKeyField lookup;
                m_fields.TryGetValue(name, out lookup);
                return lookup;
            }
        }

        /// <summary>
        /// Get count of <see cref="ForeignKeyFields"/> list
        /// </summary>
        public int Count
        {
            get
            {
                return m_fieldList.Count;
            }
        }


        #endregion

        #region [ Methods ]

        /// <summary>
        /// Get <see cref="IEnumarator"/> of field lists
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_fieldList.GetEnumerator();
        }

        /// <summary>
        /// Add a <see cref="ForeignKeyField"/> to list object
        /// </summary>
        /// <param name="newField"><paramref name="newField"/> is type of <see cref="ForeignKeyField"/></param>
        internal void Add(ForeignKeyField newField)
        {
            m_fieldList.Add(newField);
            m_fields.Add((newField.KeyName.Length > 0 ? newField.KeyName : "FK" + m_fieldList.Count), newField);
        }

        /// <summary>
        /// Get comma seperated <see cref="string"/> of <see cref="ForeignKeyField"/>
        /// </summary>
        /// <returns></returns>
        public string GetList()
        {
            StringBuilder fieldList = new StringBuilder();

            foreach (ForeignKeyField fld in m_fieldList)
            {
                if (fieldList.Length > 0)
                    fieldList.Append(',');
                fieldList.Append('[');
                fieldList.Append(fld.ForeignKey.Name);
                fieldList.Append(']');
            }

            return fieldList.ToString();

        }

        #endregion
    }

    /// <summary>
    /// Represents a collection of <see cref="Field"/> values.
    /// </summary>
    public class Fields
    {
        #region [ Members ]

        private Table m_parent;
        // Used for field name lookups
        private Dictionary<string, Field> m_fields;
        // Used for field index lookups
        private List<Field> m_fieldList;

        #endregion

        #region [ Constructors ]

        internal Fields(Table Parent)
        {
            // We only allow internal creation of this object
            m_parent = Parent;
            m_fields = new Dictionary<string, Field>(StringComparer.OrdinalIgnoreCase);
            m_fieldList = new List<Field>();

        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get <see cref="Field"/>'s parent <see cref="Table"/>.
        /// </summary>
        public Table Parent
        {
            get
            {
                return m_parent;
            }
        }

        /// <summary>
        /// Get or Set to fields lookup 
        /// </summary>
        internal Dictionary<string, Field> FieldDictionary
        {
            get
            {
                return m_fields;
            }
        }

        /// <summary>
        /// Get or Set Fields index lookup
        /// </summary>
        internal List<Field> FieldList
        {
            get
            {
                return m_fieldList;
            }
        }

        /// <summary>
        /// Indexer property of Field
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Field this[int index]
        {
            get
            {
                if (index < 0 | index >= m_fieldList.Count)
                {
                    return null;
                }
                else
                {
                    return m_fieldList[index];
                }
            }
        }

        /// <summary>
        /// Indexer property of Field by Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Field this[string name]
        {
            get
            {
                Field lookup;
                m_fields.TryGetValue(name, out lookup);
                return lookup;
            }
        }

        /// <summary>
        /// Get count of collection of <see cref="Field"/>
        /// </summary>
        public int Count
        {
            get
            {
                return m_fieldList.Count;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Add new <see cref="Field"/> to this collection
        /// </summary>
        /// <param name="NewField"></param>
        internal void Add(Field NewField)
        {
            m_fields.Add(NewField.Name, NewField);
            m_fieldList.Add(NewField);
        }

        /// <summary>
        /// Get <see cref="IEnumerator"/> type of <see cref="Field"/> list.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_fieldList.GetEnumerator();
        }

        /// <summary>
        /// Get comma seperated list of <see cref="Field"/>
        /// </summary>
        /// <param name="ReturnAutoInc"></param>
        /// <returns></returns>
        public string GetList(bool ReturnAutoInc = true)
        {
            //var _with2 = new StringBuilder();
            StringBuilder fieldList = new StringBuilder();

            foreach (Field fld in m_fieldList)
            {
                if (!fld.AutoIncrement | ReturnAutoInc)
                {
                    if (fieldList.Length > 0)
                        fieldList.Append(',');
                    fieldList.Append('[');
                    fieldList.Append(fld.Name);
                    fieldList.Append(']');
                }
            }

            return fieldList.ToString();

        }

        #endregion
    }

    /// <summary>
    /// Get data table information for data processing
    /// </summary>
    public class Table : IComparable, IComparable<Table>
    {
        #region [ Members ]

        private Tables m_parent;
        private string m_catalog;
        private string m_schema;
        private string m_name;
        private TableType m_tableType;
        private string m_description;
        private int m_rows;

        private Fields m_fields;
        // This is the name that will be used during table mapping when using a data handler
        private string m_mapName;
        // This flag allows users to override whether or not table will be processed in a data operation
        private bool m_process;
        // This is the user-overridable I/O process priority for a table
        private int m_priority;
        // User definable Sql used to retrieve last auto-inc value from identity column
        private string m_identitySql = "SELECT @@IDENTITY";

        #endregion

        #region [ Consturctors ]

        internal Table(Tables Parent, string Catalog, string Schema, string Name, string Type, string Description, int Rows)
        {
            // We only allow internal creation of this object
            m_parent = Parent;
            m_fields = new Fields(this);

            m_catalog = Catalog;
            m_schema = Schema;
            m_name = Name;
            m_mapName = Name;
            m_description = Description;

            switch (Type.Trim().ToUpper())
            {
                case "TABLE":
                    m_tableType = TableType.Table;
                    break;
                case "VIEW":
                    m_tableType = TableType.View;
                    break;
                case "SYSTEM TABLE":
                    m_tableType = TableType.SystemTable;
                    break;
                case "SYSTEM VIEW":
                    m_tableType = TableType.SystemView;
                    break;
                case "ALIAS":
                    m_tableType = TableType.Alias;
                    break;
                case "SYNONYM":
                    m_tableType = TableType.Synonym;
                    break;
                case "GLOBAL TEMPORARY":
                    m_tableType = TableType.GlobalTemp;
                    break;
                case "LOCAL TEMPORARY":
                    m_tableType = TableType.LocalTemp;
                    break;
                case "LINK":
                    m_tableType = TableType.Link;
                    break;
                default:
                    m_tableType = TableType.Undetermined;
                    break;
            }

            if (Rows == 0 && m_tableType == TableType.Table)
            {
                try
                {
                    //m_rows = m_Parent.Parent.Connection.ExecuteScalar("SELECT COUNT(*) FROM " + FullName);
                    OleDbCommand oCMD = Parent.Parent.Connection.CreateCommand();
                    oCMD.CommandText = "SELECT COUNT(*) FROM " + FullName;
                    oCMD.CommandType = CommandType.Text;
                    m_rows = Convert.ToInt32(oCMD.ExecuteScalar());
                }
                catch
                {
                    m_rows = 0;
                }
            }
            else
            {
                m_rows = Rows;
            }

            if (m_parent.Parent.DataSourceType == DatabaseType.SqlServer)
                m_identitySql = "SELECT IDENT_CURRENT('" + Name + "')";

        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get or Set list of <see cref="Fields"/> for <see cref="Table"/>
        /// </summary>
        public Fields Fields
        {
            get
            {
                return m_fields;
            }
            set
            {
                m_fields = value;
            }
        }
        /// <summary>
        /// Get or Set name of <see cref="Table"/>
        /// </summary>
        public string MapName
        {
            get
            {
                return m_mapName;
            }
            set
            {
                m_mapName = value;
            }
        }

        /// <summary>
        /// Get or Set process flag
        /// </summary>
        public bool Process
        {
            get
            {
                return m_process;
            }
            set
            {
                m_process = value;
            }
        }

        /// <summary>
        /// Get or set priority 
        /// </summary>
        public int Priority
        {
            get
            {
                return m_priority;
            }
            set
            {
                m_priority = value;
            }
        }

        /// <summary>
        /// Get or Set identitysql for <see cref="Table"/>
        /// </summary>
        public string IdentitySql
        {
            get
            {
                return m_identitySql;
            }
            set
            {
                m_identitySql = value;
            }
        }

        /// <summary>
        /// Get or Set name of <see cref="Table"/>
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// Get or Set full name of <see cref="Table"/>
        /// </summary>
        public string FullName
        {
            get
            {
                string strFullName = "";

                if (m_catalog.Length > 0)
                    strFullName += "[" + m_catalog + "].";
                if (m_schema.Length > 0)
                    strFullName += "[" + m_schema + "].";
                strFullName += "[" + m_name + "]";

                return strFullName;
            }
        }

        /// <summary>
        /// Get or Set catalog information for <see cref="Table"/>
        /// </summary>
        public string Catalog
        {
            get
            {
                return m_catalog;
            }
        }

        /// <summary>
        /// Get or Set schema name
        /// </summary>
        public string Schema
        {
            get
            {
                return m_schema;
            }
        }

        /// <summary>
        /// Get <see cref="TableType"/>
        /// </summary>
        public TableType Type
        {
            get
            {
                return m_tableType;
            }
        }

        /// <summary>
        /// Get or Set description
        /// </summary>
        public string Description
        {
            get
            {
                return m_description;
            }
        }

        /// <summary>
        /// Get row count in <see cref="Table"/>
        /// </summary>
        public int RowCount
        {
            get
            {
                return m_rows;
            }
        }

        /// <summary>
        /// Get parent <see cref="Table"/> information
        /// </summary>
        public Tables Parent
        {
            get
            {
                return m_parent;
            }
        }

        /// <summary>
        /// Get <see cref="OleDbConnection"/> of object
        /// </summary>
        public OleDbConnection Connection
        {
            get
            {
                return m_parent.Parent.Connection;
            }
        }

        /// <summary>
        /// Check for object is view
        /// </summary>
        public bool IsView
        {
            get
            {
                return (m_tableType == TableType.View || m_tableType == TableType.SystemView);
            }
        }

        /// <summary>
        /// Check for system tables and system views
        /// </summary>
        public bool IsSystem
        {
            get
            {
                return (m_tableType == TableType.SystemTable || m_tableType == TableType.SystemView);
            }
        }

        /// <summary>
        /// Get flag for <see cref="TableType"/>  for temp
        /// </summary>
        public bool IsTemporary
        {
            get
            {
                return (m_tableType == TableType.GlobalTemp || m_tableType == TableType.LocalTemp);
            }
        }

        /// <summary>
        /// Get flag for <see cref="TableType"/> alias or link
        /// </summary>
        public bool IsLinked
        {
            get
            {
                return (m_tableType == TableType.Alias || m_tableType == TableType.Link);
            }
        }

        /// <summary>
        /// Get count for primay key <see cref="Field"/>
        /// </summary>
        public int PrimaryKeyFieldCount
        {
            get
            {
                int iCount = 0;

                foreach (Field fld in m_fields)
                {
                    if (fld.IsPrimaryKey)
                        iCount += 1;
                }

                return iCount;
            }
        }

        /// <summary>
        /// Get flag of any foreign key <see cref="Field"/>
        /// </summary>
        public bool IsForeignKeyTable
        {
            get
            {
                foreach (Field fld in m_fields)
                {
                    if (fld.IsForeignKey)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Get flag for auto incremented <see cref="Field"/>
        /// </summary>
        public bool HasAutoIncField
        {
            get
            {
                foreach (Field fld in m_fields)
                {
                    if (fld.AutoIncrement)
                        return true;
                }
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Get schema information flag based on <see cref="DatabaseType"/>
        /// </summary>
        /// <returns></returns>
        public bool UsesDefaultSchema()
        {
            if (Parent.Parent.DataSourceType == DatabaseType.SqlServer)
                return (string.Compare(m_schema, "dbo", true) == 0);
            else
                return (Schema.Length == 0);
        }

        /// <summary>
        /// Gets display name for table.
        /// </summary>
        public override string ToString()
        {
            return MapName;
        }

        /// <summary>
        /// Compare <see cref="Object"/> type of <paramref name="obj"/> with <see cref="Table"/> object <see cref="Priority"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            // Tables are sorted in priority order
            if (obj is Table)
                return CompareTo(obj as Table);
            else
                throw new ArgumentException("Table can only be compared to other Tables");
        }

        /// <summary>
        /// Compare <paramref name="Table"/> with other <see cref="Table"/> object <see cref="Priority"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Table other)
        {
            return m_priority.CompareTo(other.Priority);
        }

        /// <summary>
        /// Check for reference flag, whether table has reference in another table
        /// </summary>
        /// <param name="otherTable"></param>
        /// <param name="tableStack"></param>
        /// <returns></returns>
        internal bool IsReferencedBy(Table otherTable, List<Table> tableStack)
        {
            Table table = null;
            bool tableIsInStack = false;

            if (tableStack == null)
                tableStack = new List<Table>();

            tableStack.Add(this);

            foreach (Field field in m_fields)
            {
                foreach (ForeignKeyField foreignKey in field.ForeignKeys)
                {
                    // We don't want to circle back on ourselves
                    table = foreignKey.ForeignKey.Table;
                    tableIsInStack = tableStack.Exists(tbl => string.Compare(tbl.Name, table.Name, true) == 0);

                    if (tableIsInStack)
                    {
                        if (string.Compare(table.Name, otherTable.Name, true) == 0)
                            return true;
                    }
                    else
                    {
                        if (table.IsReferencedBy(otherTable, tableStack))
                            return true;
                        else if (string.Compare(table.Name, otherTable.Name, true) == 0)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check for table reference
        /// </summary>
        /// <param name="otherTable"></param>
        /// <returns></returns>
        public bool IsReferencedBy(Table otherTable)
        {
            return IsReferencedBy(otherTable, null);
        }

        public bool IsReferencedVia(Table otherTable)
        {
            Table table = null;

            foreach (Field field in otherTable.Fields)
            {
                foreach (ForeignKeyField foreignKey in field.ForeignKeys)
                {
                    table = foreignKey.ForeignKey.Table;

                    // Not a direct relation, but children are related
                    if (string.Compare(m_name, table.Name, true) != 0 && IsReferencedBy(table))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// check for primary key field in <see cref="Table"/>
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="PrimaryKeyOrdinal"></param>
        /// <param name="PrimaryKeyName"></param>
        /// <returns></returns>
        public bool DefinePrimaryKey(string FieldName, int PrimaryKeyOrdinal = -1, string PrimaryKeyName = "")
        {

            Field fldLookup;// = null;

            fldLookup = m_fields[FieldName];
            if ((fldLookup != null))
            {
                //var _with3 = fldLookup;
                fldLookup.IsPrimaryKey = true;
                fldLookup.PrimaryKeyOrdinal = (PrimaryKeyOrdinal == -1 ? PrimaryKeyFieldCount + 1 : PrimaryKeyOrdinal);
                fldLookup.PrimaryKeyName = PrimaryKeyName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check for <see cref="ForeignKeyField"/>
        /// </summary>
        /// <param name="PrimaryKeyFieldName"></param>
        /// <param name="ForeignKeyTableName"></param>
        /// <param name="ForeignKeyFieldName"></param>
        /// <param name="ForeignKeyOrdinal"></param>
        /// <param name="ForeignKeyName"></param>
        /// <param name="ForeignKeyUpdateRule"></param>
        /// <param name="ForeignKeyDeleteRule"></param>
        /// <returns></returns>
        public bool DefineForeignKey(string PrimaryKeyFieldName, string ForeignKeyTableName, string ForeignKeyFieldName, int ForeignKeyOrdinal = -1, string ForeignKeyName = "", ReferentialAction ForeignKeyUpdateRule = ReferentialAction.NoAction, ReferentialAction ForeignKeyDeleteRule = ReferentialAction.NoAction)
        {

            Field fldLookup = null;
            Table tblLookup = null;
            Field fldParentLookup = null;

            fldLookup = m_fields[PrimaryKeyFieldName];
            if ((fldLookup != null))
            {
                tblLookup = m_parent[ForeignKeyTableName];
                if ((tblLookup != null))
                {
                    fldParentLookup = tblLookup.Fields[ForeignKeyFieldName];
                    if ((fldParentLookup != null))
                    {
                        ForeignKeyField fkFld = new ForeignKeyField(fldLookup.ForeignKeys);
                        //var _with4 = fkFld;
                        fkFld.PrimaryKey = fldLookup;
                        fkFld.ForeignKey = fldParentLookup;
                        fkFld.ForeignKey.ReferencedBy = fkFld.PrimaryKey;
                        fkFld.Ordinal = (ForeignKeyOrdinal == -1 ? fldLookup.ForeignKeys.Count + 1 : ForeignKeyOrdinal);
                        fkFld.KeyName = ForeignKeyName;
                        fkFld.UpdateRule = ForeignKeyUpdateRule;
                        fkFld.DeleteRule = ForeignKeyDeleteRule;
                        fldLookup.ForeignKeys.Add(fkFld);
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }

    /// <summary>
    /// List of <see cref="Table"/> collection
    /// </summary>
    public class Tables
    {
        #region [ Memebers ]

        //Fields
        private Schema m_parent;
        // Used for table name lookups
        private Dictionary<string, Table> m_tables;
        // Used for table index lookups
        private List<Table> m_tableList;

        #endregion

        #region [ Constructor ]

        internal Tables(Schema Parent)
        {
            // We only allow internal creation of this object
            m_parent = Parent;
            m_tables = new Dictionary<string, Table>(StringComparer.OrdinalIgnoreCase);
            m_tableList = new List<Table>();
        }

        #endregion

        #region [ Properties ]

        internal Dictionary<string, Table> TableDictionary
        {
            get
            {
                return m_tables;
            }
        }

        internal List<Table> TableList
        {
            get
            {
                return m_tableList;
            }
        }

        public int Count
        {
            get
            {
                return m_tableList.Count;
            }
        }

        public Schema Parent
        {
            get
            {
                return m_parent;
            }
        }

        #endregion

        #region [ Methods ]

        internal void Add(Table table)
        {
            m_tables.Add(table.Name, table);
            m_tableList.Add(table);
        }

        public Table this[int index]
        {
            get
            {
                if (index < 0 | index >= m_tableList.Count)
                {
                    return null;
                }
                else
                {
                    return m_tableList[index];
                }
            }
        }

        public Table this[string name]
        {
            get
            {
                Table lookup;                
                m_tables.TryGetValue(name, out lookup);
                return lookup;
            }
        }

        public Table FindByMapName(string mapName)
        {

            foreach (Table table in m_tableList)
            {
                if (string.Compare(table.MapName, mapName, true) == 0)
                {
                    return table;
                }
            }

            return null;

        }

        public IEnumerator GetEnumerator()
        {

            return m_tableList.GetEnumerator();

        }

        public string GetList()
        {
            StringBuilder fieldList = new StringBuilder();

            foreach (Table tbl in m_tableList)
            {
                if (fieldList.Length > 0)
                    fieldList.Append(',');
                fieldList.Append(tbl.FullName);
            }

            return fieldList.ToString();

        }

        #endregion

        #region [ Inner Class ]

        /// <summary>
        /// Check for referentail order of <see cref="Table"/>
        /// </summary>
        public class ReferentialOrderComparer : IComparer<Table>
        {

            #region  [ Properties ]

            /// <summary>
            /// Default property of object
            /// </summary>
            public readonly static ReferentialOrderComparer Default = new ReferentialOrderComparer();

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Allows tables to be sorted in proper referential integrity process object
            /// </summary>
            /// <param name="item1"></param>
            /// <param name="item2"></param>
            /// <returns></returns>
            public int Compare(Table table1, Table table2)
            {
                // This function allows tables to be sorted in proper referential integrity process order
                int intCompare = 0;

                if (table1 == table2)
                    return 0;

                if (table1.IsReferencedBy(table2))// || table1.IsReferencedVia(table2))
                    intCompare = -1;
                else if (table2.IsReferencedBy(table1))// || table2.IsReferencedVia(table1))
                    intCompare = 1;

                // Sort by existence of foreign key fields, if defined
                if (intCompare == 0)
                    intCompare = ForeignKeyCompare(table1, table2);

                return intCompare;
            }

            /// <summary>
            /// Compare foreign key comparation of tables
            /// </summary>
            /// <param name="table1"></param>
            /// <param name="table2"></param>
            /// <returns></returns>
            private int ForeignKeyCompare(Table table1, Table table2)
            {
                if (table1.IsForeignKeyTable && table2.IsForeignKeyTable)
                {
                    // Both tables have foreign key fields, consider them equal
                    return 0;
                }
                else if (!table1.IsForeignKeyTable && !table2.IsForeignKeyTable)
                {
                    // Neither table has foreign key fields, consider them equal
                    return 0;
                }
                else if (table1.IsForeignKeyTable)
                {
                    // Table1 has foreign key fields and Table2 does not, sort it below
                    return 1;
                }
                else
                {
                    // Table2 has foreign key fields and Table1 does not, sort it below
                    return -1;
                }
            }

            ///// <summary>
            ///// We compare based on the existance of AutoInc fields as a secondary compare in case user
            ///// has no defined relational integrity - lastly we just sort by table name
            ///// </summary>
            ///// <param name="tbl1"></param>
            ///// <param name="tbl2"></param>
            ///// <returns></returns>
            //private int AutoIncCompare(Table tbl1, Table tbl2)
            //{
            //    return (tbl1.HasAutoIncField == tbl2.HasAutoIncField ? 0 : (tbl1.HasAutoIncField ? -1 : 1));
            //}

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// Get information about database schema
    /// </summary>
    [ToolboxBitmap(typeof(Schema), "Database.Schema.bmp"), DefaultProperty("SourceSchema")]
    public partial class Schema : Component
    {
        #region [ Members ]

        //Fields
        private Tables m_tables;

        public const TableType NoRestriction = TableType.Table | TableType.View | TableType.SystemTable | TableType.SystemView | TableType.Alias | TableType.Synonym | TableType.GlobalTemp | TableType.LocalTemp | TableType.Link | TableType.Undetermined;

        private OleDbConnection m_schemaConnection;
        private string m_connectString;
        private DatabaseType m_databaseType;
        private TableType m_restriction;
        private bool m_immediateClose;
        private bool m_allowTextNulls;
        private bool m_allowNumericNulls;

        #endregion

        #region [ Constructors ]

        public Schema()
        {
            InitializeComponent();
            m_connectString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\\SourceDB.mdb";
            m_databaseType = DatabaseType.Unspecified;
            m_restriction = NoRestriction;
            m_immediateClose = true;
            m_allowTextNulls = false;
            m_allowNumericNulls = false;
        }


        public Schema(string ConnectString, TableType TableTypeRestriction = NoRestriction, bool ImmediateClose = true, bool AnaylzeNow = true)
        {
            InitializeComponent();
            m_connectString = ConnectString;
            m_restriction = TableTypeRestriction;
            m_immediateClose = ImmediateClose;
            m_allowTextNulls = false;
            m_allowNumericNulls = false;
            if (AnaylzeNow)
                Analyze();


        }

        public Schema(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="Schema"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~Schema()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get or Set - information to process <see cref="Tables"/>
        /// </summary>
        [Browsable(false)]
        public Tables Tables
        {
            get
            {
                return m_tables;
            }
            set
            {
                m_tables = value;
            }
        }

        /// <summary>
        /// OLEDB connection string to datasource to analyze.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("OLEDB connection string to datasource to analyze.")]
        public string ConnectString
        {
            get
            {
                return m_connectString;
            }
            set
            {
                m_connectString = value;
            }
        }

        /// <summary>
        /// Set this value to restrict the types of tables returned in your schema.  Table types can be OR'd together to create this table type restriction.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Set this value to restrict the types of tables returned in your schema.  Table types can be OR'd together to create this table type restriction."), DefaultValue(NoRestriction)]
        public TableType TableTypeRestriction
        {
            get
            {
                return m_restriction;
            }
            set
            {
                m_restriction = value;
            }
        }

        /// <summary>
        /// Set this value to False to keep the schema connection used during analysis open after analysis is complete.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Set this value to False to keep the schema connection used during analysis open after analysis is complete."), DefaultValue(true)]
        public bool ImmediateClose
        {
            get
            {
                return m_immediateClose;
            }
            set
            {
                m_immediateClose = value;
            }
        }

        /// <summary>
        /// Type of database specified in connect string.
        /// </summary>
        [Browsable(true), Category("Sql Encoding"), Description("Type of database specified in connect string."), DefaultValue(DatabaseType.Unspecified)]
        public DatabaseType DataSourceType
        {
            get
            {
                return m_databaseType;
            }
            set
            {
                m_databaseType = value;
            }
        }

        /// <summary>
        /// Set this value to False to convert all Null values encountered in character fields to empty strings.
        /// </summary>
        [Browsable(true), Category("Sql Encoding"), Description("Set this value to False to convert all Null values encountered in character fields to empty strings."), DefaultValue(false)]
        public bool AllowTextNulls
        {
            get
            {
                return m_allowTextNulls;
            }
            set
            {
                m_allowTextNulls = true;
            }
        }

        /// <summary>
        /// Set this value to False to convert all Null values encountered in numeric fields to zeros.
        /// </summary>
        [Browsable(true), Category("Sql Encoding"), Description("Set this value to False to convert all Null values encountered in numeric fields to zeros."), DefaultValue(false)]
        public bool AllowNumericNulls
        {
            get
            {
                return m_allowNumericNulls;
            }
            set
            {
                m_allowNumericNulls = true;
            }
        }

        /// <summary>
        /// <see cref="OleDbConnection"/> to open a database connection
        /// </summary>
        [Browsable(false)]
        public OleDbConnection Connection
        {
            get
            {
                if (DesignMode)
                {
                    return null;
                }
                else
                {
                    return m_schemaConnection;
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Analyze data schema for processing data
        /// </summary>
        public void Analyze()
        {
            DataRow row;
            Table tbl;
            Field fld;
            int x = 0;
            int y = 0;

            // Check http://msdn.microsoft.com/library/default.asp?url=/library/en-us/oledb/htm/olprappendixb.asp
            // for detailed OLEDB schema rowset information
            m_tables = new Tables(this);
            m_schemaConnection = new OleDbConnection(m_connectString);
            m_schemaConnection.StateChange += SchemaConnection_StateChange;
            m_schemaConnection.Open();

            // Load all tables and views into the schema
            DataTable schemaTable = m_schemaConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables_Info, null);
            for (x = 0; x < schemaTable.Rows.Count; x++)
            {
                row = schemaTable.Rows[x];
                tbl = new Table(m_tables, Common.NotNull(row["TABLE_CATALOG"]),
                                Common.NotNull(row["TABLE_SCHEMA"]),
                                row["TABLE_NAME"].ToString(),
                                row["TABLE_TYPE"].ToString(),
                                Common.NotNull(row["DESCRIPTION"], ""), 0);

                if ((tbl.Type & m_restriction) == m_restriction)
                {
                    // Both the data adapter and the OleDB schema rowsets provide column properties
                    // that the other doesn't - so we use both to get a very complete schema                        
                    DataSet data = new DataSet();
                    OleDbDataAdapter adapter = new OleDbDataAdapter();

                    if (tbl.Name.IndexOf(' ') == -1 & tbl.UsesDefaultSchema())
                    {
                        try
                        {
                            // For standard table names we can use direct table commands for speed
                            adapter.SelectCommand = new OleDbCommand(tbl.Name, m_schemaConnection);
                            adapter.SelectCommand.CommandType = CommandType.TableDirect;
                            adapter.FillSchema(data, SchemaType.Mapped);
                        }
                        catch
                        {
                            // We'll fall back on the standard method (maybe provider doesn't support TableDirect)
                            adapter.SelectCommand = new OleDbCommand("SELECT TOP 1 * FROM " + tbl.FullName, m_schemaConnection);
                            adapter.FillSchema(data, SchemaType.Mapped);
                        }
                    }
                    else
                    {
                        // For schema based databases and non-standard table names we must use a regular select command
                        adapter.SelectCommand = new OleDbCommand("SELECT TOP 1 * FROM " + tbl.FullName, m_schemaConnection);
                        adapter.FillSchema(data, SchemaType.Mapped);
                    }

                    // Load all column data into the schema
                    DataTable currentTable = m_schemaConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] {
                        null,
                        null,
                        tbl.Name
                    });

                    //var _with7 = m_schemaConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] {
                    //    null,
                    //    null,
                    //    tbl.Name
                    //});
                    for (y = 0; y < currentTable.Rows.Count; y++)
                    {
                        row = currentTable.Rows[y];

                        // New field encountered, create new field
                        fld = new Field(tbl.Fields, row["COLUMN_NAME"].ToString(), (OleDbType)row["DATA_TYPE"]);
                        fld.HasDefault = Convert.ToBoolean(Common.NotNull(row["COLUMN_HASDEFAULT"], "0"));  ///Common.NotNull(row["COLUMN_HASDEFAULT"], false);
                        fld.NumericPrecision = Convert.ToInt32(Common.NotNull(row["NUMERIC_PRECISION"], "0"));
                        fld.NumericScale = Convert.ToInt32(Common.NotNull(row["NUMERIC_SCALE"], "0"));
                        fld.DateTimePrecision = Convert.ToInt32(Common.NotNull(row["DATETIME_PRECISION"], "0"));
                        fld.Description = Common.NotNull(row["DESCRIPTION"], "");

                        // We also use as many properties as we can from data adapter schema
                        //var _with8 = data.Tables[0].Columns[fld.Name];
                        //fld.intOrdinal = data.Tables[0].Columns[fld.Name].Ordinal;
                        fld.Ordinal = data.Tables[0].Columns[fld.Name].Ordinal;

                        fld.AllowsNulls = data.Tables[0].Columns[fld.Name].AllowDBNull;
                        fld.DefaultValue = data.Tables[0].Columns[fld.Name].DefaultValue;
                        fld.MaxLength = data.Tables[0].Columns[fld.Name].MaxLength;
                        fld.AutoIncrement = data.Tables[0].Columns[fld.Name].AutoIncrement;
                        fld.AutoIncrementSeed = Convert.ToInt32(data.Tables[0].Columns[fld.Name].AutoIncrementSeed);
                        fld.AutoIncrementStep = Convert.ToInt32(data.Tables[0].Columns[fld.Name].AutoIncrementStep);
                        fld.ReadOnly = data.Tables[0].Columns[fld.Name].ReadOnly;
                        fld.Unique = data.Tables[0].Columns[fld.Name].Unique;

                        // Add field to table's field collection
                        tbl.Fields.Add(fld);
                    }

                    // Sort all loaded fields in ordinal order
                    tbl.Fields.FieldList.Sort();

                    // Define primary keys
                    try
                    {
                        DataTable primaryKeyTable = m_schemaConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new object[] {
                            null,
                            null,
                            tbl.Name
                        });
                        for (y = 0; y <= primaryKeyTable.Rows.Count - 1; y++)
                        {
                            row = primaryKeyTable.Rows[y];
                            tbl.DefinePrimaryKey(row["COLUMN_NAME"].ToString(), Convert.ToInt32(Common.NotNull(row["ORDINAL"], "-1")), Common.NotNull(row["PK_NAME"], ""));
                        }
                    }
                    catch
                    {
                        // It's possible that the data source doesn't provide a primary keys rowset
                    }

                    // Add table to schema's table collection
                    m_tables.Add(tbl);
                }
            }

            // Define foreign keys (must be done after all tables are defined so relations can be properly established)
            //foreach (tbl in Tables)
            for (int i = 0; i < m_tables.Count; i++)
            {
                tbl = m_tables[i];
                try
                {
                    DataTable foreignKeyTable = m_schemaConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, new object[] {
                        null,
                        null,
                        tbl.Name
                    });
                    for (x = 0; x <= foreignKeyTable.Rows.Count - 1; x++)
                    {
                        row = foreignKeyTable.Rows[x];
                        tbl.DefineForeignKey(row["PK_COLUMN_NAME"].ToString(),
                                    row["FK_TABLE_NAME"].ToString(), row["FK_COLUMN_NAME"].ToString(),
                                    Convert.ToInt32(Common.NotNull(row["ORDINAL"], "-1")), Common.NotNull(row["FK_NAME"], ""),
                                    Field.GetReferentialAction(Common.NotNull(row["UPDATE_RULE"], "")),
                                    Field.GetReferentialAction(Common.NotNull(row["DELETE_RULE"], "")));
                    }
                }
                catch
                {
                    // It's possible that the data source doesn't provide a foreign keys rowset
                }
            }

            // Using a simple (i.e., stable) sorting algorithm here since not all relationships will
            // be considered mathematically congruent and the fast .NET sort algorithm depends on
            // comparisons based on perfect equality (i.e., if A > B and B > C then A > C - this may
            // not be true in terms of referential integrity)
            List<Table> sortedList = new List<Table>(m_tables.TableList);
            Table temp = null;

            for (x = 0; x < sortedList.Count; x++)
            {
                for (y = 0; y < sortedList.Count; y++)
                {
                    if (x != y && Tables.ReferentialOrderComparer.Default.Compare(sortedList[x], sortedList[y]) < 0)
                    {
                        temp = sortedList[y];
                        sortedList[y] = sortedList[x];
                        sortedList[x] = temp;
                    }
                }
            }

            // Set initial I/O processing priorties for tables based on this order.  Processing tables
            // based on the "Priority" field allows user to have final say in processing order
            for (x = 0; x < sortedList.Count; x++)
            {
                m_tables.TableList.Find(table => string.Compare(table.Name, sortedList[x].Name, true) == 0).Priority = x;
            }

            // Check to see if user requested to keep connection open, this is just for convience...
            if (m_immediateClose)
                Close();

        }

        /// <summary>
        /// <see cref="OleDbConnection"/> state change event will fire if it unexpectedbly close connection while processing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SchemaConnection_StateChange(object sender, StateChangeEventArgs e)
        {
            // The connection may have been closed prematurely so we reopen it.
            if (m_schemaConnection.State == ConnectionState.Closed)
                m_schemaConnection.Open();
        }

        /// <summary>
        /// Close <see cref="OleDbConnection"/> 
        /// </summary>
        public void Close()
        {
            if ((m_schemaConnection != null))
            {
                try
                {
                    m_schemaConnection.StateChange -= SchemaConnection_StateChange;
                    m_schemaConnection.Close();
                }
                catch
                {
                    // Keep on going here...
                }
            }
            m_schemaConnection = null;

        }

        #endregion
    }
}
