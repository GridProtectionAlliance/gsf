//******************************************************************************************************
//  Schema.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
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
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using GSF.IO;

namespace GSF.Data
{
    // James Ritchie Carroll - 2003

    #region [ Enumerations ]

    /// <summary>
    /// Specifies the type of object in database
    /// </summary>
    [Flags]
    [Serializable]
    [SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
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
        /// Database object is Link
        /// </summary>
        Link = 256,
        /// <summary>
        /// Database object is not defined
        /// </summary>
        Undetermined = 512
    }

    /// <summary>
    /// Specified the type of referential action on database object/Tables
    /// </summary>
    [Serializable]
    public enum ReferentialAction
    {
        /// <summary>
        /// Action Type is cascade
        /// </summary>
        Cascade,
        /// <summary>
        /// Action Type is to set null
        /// </summary>
        SetNull,
        /// <summary>
        /// Action Type is to set default
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
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
    public class Field : IComparable
    {
        #region [ Members ]

        //Fields
        private Fields m_parent;
        private string m_name;
        private OleDbType m_dataType;

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

        private object m_value;
        private bool m_isPrimaryKey;
        private int m_primaryKeyOrdinal;
        private string m_primaryKeyName;

        private ForeignKeyFields m_foreignKeys;
        private Field m_referencedBy;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Field"/>.
        /// </summary>
        /// <param name="Name">Field name.</param>
        /// <param name="Type">OLEDB data type for field.</param>
        public Field(string Name, OleDbType Type)
        {
            // We only allow internal creation of this object
            m_name = Name;
            m_dataType = Type;
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
                return m_name;
            }
            set
            {
                m_parent?.FieldDictionary.Remove(m_name);
                m_parent?.FieldDictionary.Add(value, this);
                m_name = value;
            }
        }

        /// <summary>
        /// Get SQL escaped name of <see cref="Table"/>
        /// </summary>
        public string SQLEscapedName
        {
            get
            {
                return m_parent.Parent.Parent.Parent.SQLEscapeName(m_name);
            }
        }

        /// <summary>
        /// Get <see cref="OleDbType"/> Type
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public OleDbType Type
        {
            get
            {
                return m_dataType;
            }
            set
            {
                m_dataType = value;
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
            }
            set
            {
                m_ordinal = value;
            }
        }

        /// <summary>
        /// Get or set Allow Null flag
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
        /// Get or set Auto increment flag
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
        /// Get or set Auto increment seed
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
        /// Get or set Auto increment step
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
        /// Get or set has default value flag
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
        /// Get or set default value
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
        /// Get or set maximum length of field
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
        /// Get or set numeric precision
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
        /// Get or set Numeric scale
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
        /// Get or set date time precision
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
        /// Get or set read-only flag
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
        /// Get or set for unique
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
        /// Get or set description
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
        /// Get or set auto increment translation
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
        /// Get or set value of <see cref="Field"/>
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
        /// Get or set flag to check <see cref="Field"/> is primary key or not
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
        /// Get or set ordinal for Primary key field
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
        /// Get or set primary key name
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
        /// Get or set list of <see cref="ForeignKeyFields"/>
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
        /// Get or set - check <see cref="Field"/> is reference by
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
        /// Get or set foreign key flag. if <see cref="Field"/> is <see cref="ReferencedBy"/> then true else false
        /// </summary>
        public bool IsForeignKey
        {
            get
            {
                return ((object)m_referencedBy != null);
            }
        }

        /// <summary>
        /// Get or set <see cref="Fields"/> parent
        /// </summary>
        public Fields Parent
        {
            get
            {
                return m_parent;
            }
            internal set
            {
                m_parent = value;
            }
        }

        /// <summary>
        /// Get or set <see cref="Field"/>'s parent <see cref="Table"/>
        /// </summary>
        public Table Table
        {
            get
            {
                return m_parent?.Parent;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Compare <paramref name="obj"/> ordinal to current field <see cref="Ordinal"/>
        /// </summary>
        /// <param name="obj">Check <paramref name="obj"/> type <see cref="object"/>, if it is type of <see cref="Field"/> then compare to <see cref="Ordinal"/> of <paramref name="obj"/> else throw <see cref="ArgumentException"/></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            // Fields are sorted in ordinal position order
            if (obj is Field field)
                return m_ordinal.CompareTo(field.m_ordinal);

            throw new ArgumentException("Field can only be compared to other Fields");
        }

        /// <summary>
        /// Change <see cref="Field"/> value to encoded string. It will check <see cref="Type"/>  and <see cref="Parent"/> value before convert to <see cref="OleDbType"/> compatible value
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public string SQLEncodedValue
        {
            get
            {
                string encodedValue = "";

                if (!Convert.IsDBNull(m_value))
                {
                    try
                    {
                        // Attempt to get string based source field value
                        encodedValue = m_value.ToString().Trim();

                        // Format field value based on field's data type
                        switch (m_dataType)
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
                                if (encodedValue.Length == 0)
                                {
                                    if (m_parent.Parent.Parent.Parent.AllowNumericNulls)
                                    {
                                        encodedValue = "NULL";
                                    }
                                    else
                                    {
                                        encodedValue = "0";
                                    }
                                }
                                else
                                {
                                    Int64 tempValue;

                                    if (Int64.TryParse(m_value.ToString(), out tempValue)) //(Information.IsNumeric(Value))
                                    {
                                        encodedValue = Convert.ToInt64(Value).ToString().Trim();
                                    }
                                    else
                                    {
                                        if (m_parent.Parent.Parent.Parent.AllowNumericNulls)
                                        {
                                            encodedValue = "NULL";
                                        }
                                        else
                                        {
                                            encodedValue = "0";
                                        }
                                    }
                                }
                                break;
                            case OleDbType.Single:
                                if (encodedValue.Length == 0)
                                {
                                    if (m_parent.Parent.Parent.Parent.AllowNumericNulls)
                                    {
                                        encodedValue = "NULL";
                                    }
                                    else
                                    {
                                        encodedValue = "0.0";
                                    }
                                }
                                else
                                {
                                    Single tempValue;

                                    if (Single.TryParse(m_value.ToString(), out tempValue)) //if (Information.IsNumeric(Value))
                                    {
                                        encodedValue = Convert.ToSingle(Value).ToString().Trim();
                                    }
                                    else
                                    {
                                        if (m_parent.Parent.Parent.Parent.AllowNumericNulls)
                                        {
                                            encodedValue = "NULL";
                                        }
                                        else
                                        {
                                            encodedValue = "0.0";
                                        }
                                    }
                                }
                                break;
                            case OleDbType.Double:
                                if (encodedValue.Length == 0)
                                {
                                    if (m_parent.Parent.Parent.Parent.AllowNumericNulls)
                                    {
                                        encodedValue = "NULL";
                                    }
                                    else
                                    {
                                        encodedValue = "0.0";
                                    }
                                }
                                else
                                {
                                    Double tempValue;

                                    if (Double.TryParse(m_value.ToString(), out tempValue)) //if (Information.IsNumeric(Value))
                                    {
                                        encodedValue = Convert.ToDouble(Value).ToString().Trim();
                                    }
                                    else
                                    {
                                        if (m_parent.Parent.Parent.Parent.AllowNumericNulls)
                                        {
                                            encodedValue = "NULL";
                                        }
                                        else
                                        {
                                            encodedValue = "0.0";
                                        }
                                    }
                                }
                                break;
                            case OleDbType.Currency:
                            case OleDbType.Decimal:
                            case OleDbType.Numeric:
                            case OleDbType.VarNumeric:
                                if (encodedValue.Length == 0)
                                {
                                    if (m_parent.Parent.Parent.Parent.AllowNumericNulls)
                                    {
                                        encodedValue = "NULL";
                                    }
                                    else
                                    {
                                        encodedValue = "0.00";
                                    }
                                }
                                else
                                {
                                    Decimal tempValue;

                                    if (Decimal.TryParse(m_value.ToString(), out tempValue)) //if (Information.IsNumeric(Value))
                                    {
                                        encodedValue = Convert.ToDecimal(Value).ToString().Trim();
                                    }
                                    else
                                    {
                                        if (m_parent.Parent.Parent.Parent.AllowNumericNulls)
                                        {
                                            encodedValue = "NULL";
                                        }
                                        else
                                        {
                                            encodedValue = "0.00";
                                        }
                                    }
                                }
                                break;
                            case OleDbType.Boolean:
                                if (encodedValue.Length == 0)
                                {
                                    if (m_parent.Parent.Parent.Parent.AllowNumericNulls)
                                    {
                                        encodedValue = "NULL";
                                    }
                                    else
                                    {
                                        encodedValue = "0";
                                    }
                                }
                                else
                                {
                                    long tempValue;

                                    if (long.TryParse(m_value.ToString(), out tempValue)) //if (Information.IsNumeric(strValue))
                                    {
                                        if (tempValue == 0)
                                        {
                                            encodedValue = "0";
                                        }
                                        else
                                        {
                                            encodedValue = "1";
                                        }
                                    }
                                    else
                                    {
                                        switch (char.ToUpper(encodedValue.Trim()[0]))
                                        {
                                            case 'Y':
                                            case 'T':
                                                encodedValue = "1";
                                                break;
                                            case 'N':
                                            case 'F':
                                                encodedValue = "0";
                                                break;
                                            default:
                                                if (m_parent.Parent.Parent.Parent.AllowNumericNulls)
                                                {
                                                    encodedValue = "NULL";
                                                }
                                                else
                                                {
                                                    encodedValue = "0";
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
                                if (encodedValue.Length == 0)
                                {
                                    if (m_parent.Parent.Parent.Parent.AllowTextNulls)
                                    {
                                        encodedValue = "NULL";
                                    }
                                    else
                                    {
                                        encodedValue = "''";
                                    }
                                }
                                else
                                {
                                    encodedValue = "'" + encodedValue.SQLEncode(m_parent.Parent.Parent.Parent.DataSourceType) + "'";
                                }
                                break;
                            case OleDbType.DBTimeStamp:
                            case OleDbType.DBDate:
                            case OleDbType.Date:
                                if (encodedValue.Length > 0)
                                {
                                    DateTime tempDateTimeValue;

                                    if (DateTime.TryParse(m_value.ToString(), out tempDateTimeValue)) //if (Information.IsDate(strValue))
                                    {
                                        switch (m_parent.Parent.Parent.Parent.DataSourceType)
                                        {
                                            case DatabaseType.Access:
                                                encodedValue = "#" + tempDateTimeValue.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) + "#";
                                                break;
                                            case DatabaseType.Oracle:
                                                encodedValue = "to_date('" + tempDateTimeValue.ToString("dd-MMM-yyyy HH:mm:ss") + "', 'DD-MON-YYYY HH24:MI:SS')";
                                                break;
                                            default:
                                                encodedValue = "'" + tempDateTimeValue.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        encodedValue = "NULL";
                                    }
                                }
                                else
                                {
                                    encodedValue = "NULL";
                                }
                                break;
                            case OleDbType.DBTime:
                                if (encodedValue.Length > 0)
                                {
                                    DateTime tempDateTimeValue;

                                    if (DateTime.TryParse(m_value.ToString(), out tempDateTimeValue)) //if (Information.IsDate(strValue))
                                    {
                                        encodedValue = "'" + tempDateTimeValue.ToString("HH:mm:ss") + "'"; // Strings.Format((DateTime)strValue, "HH:mm:ss") + "'";
                                    }
                                    else
                                    {
                                        encodedValue = "NULL";
                                    }
                                }
                                else
                                {
                                    encodedValue = "NULL";
                                }
                                break;
                            case OleDbType.Filetime:
                                if (encodedValue.Length > 0)
                                {
                                    encodedValue = "'" + encodedValue + "'";
                                }
                                else
                                {
                                    encodedValue = "NULL";
                                }
                                break;
                            case OleDbType.Guid:
                                if (encodedValue.Length == 0)
                                {
                                    encodedValue = (new Guid()).ToString().ToLower();
                                }

                                if (m_parent.Parent.Parent.Parent.DataSourceType == DatabaseType.Access)
                                {
                                    encodedValue = "{" + encodedValue.ToLower() + "}";
                                }
                                else
                                {
                                    encodedValue = "'" + encodedValue.ToLower() + "'";
                                }
                                break;
                        }
                    }
                    catch //(Exception ex)
                    {
                        // We'll default to NULL if we failed to evaluate field data
                        encodedValue = "NULL";
                    }
                }

                if (encodedValue.Length == 0)
                    encodedValue = "NULL";

                return encodedValue;
            }
        }

        /// <summary>
        /// Gets the native value for the field (SQL Encoded).
        /// </summary>
        public string NonNullNativeValue
        {
            get
            {
                string encodedValue = "";

                // Format field value based on field's data type
                switch (m_dataType)
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
                    case OleDbType.Boolean:
                        encodedValue = "0";
                        break;
                    case OleDbType.Single:
                    case OleDbType.Double:
                    case OleDbType.Currency:
                    case OleDbType.Decimal:
                    case OleDbType.Numeric:
                    case OleDbType.VarNumeric:
                        encodedValue = "0.00";
                        break;
                    case OleDbType.Char:
                    case OleDbType.WChar:
                    case OleDbType.VarChar:
                    case OleDbType.VarWChar:
                    case OleDbType.LongVarChar:
                    case OleDbType.LongVarWChar:
                    case OleDbType.BSTR:
                    case OleDbType.Filetime:
                        encodedValue = "''";
                        break;
                    case OleDbType.DBTimeStamp:
                    case OleDbType.DBDate:
                    case OleDbType.Date:
                        switch (m_parent.Parent.Parent.Parent.DataSourceType)
                        {
                            case DatabaseType.Access:
                                encodedValue = "#" + DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) + "#";
                                break;
                            case DatabaseType.Oracle:
                                encodedValue = "to_date('" + DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss") + "', 'DD-MON-YYYY HH24:MI:SS')";
                                break;
                            default:
                                encodedValue = "'" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                break;
                        }
                        break;
                    case OleDbType.DBTime:
                        encodedValue = "'" + DateTime.UtcNow.ToString("HH:mm:ss") + "'";
                        break;
                    case OleDbType.Guid:
                        encodedValue = (new Guid()).ToString().ToLower();
                        if (m_parent.Parent.Parent.Parent.DataSourceType == DatabaseType.Access)
                            encodedValue = "{" + encodedValue + "}";
                        else
                            encodedValue = "'" + encodedValue + "'";
                        break;
                }

                return encodedValue;
            }
        }

        /// <summary>
        /// Get information about referential action
        /// </summary>
        /// <param name="action">check <paramref name="action"/> and return to appropriate <see cref="ReferentialAction"/>.</param>
        /// <returns></returns>
        static internal ReferentialAction GetReferentialAction(string action)
        {
            switch (action.Trim().ToUpper())
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
    [Serializable]
    public class ForeignKeyField
    {
        #region [ Members ]

        private readonly ForeignKeyFields m_Parent;

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
        /// Get or set Primary key field
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
        /// Get or set Foreign key field
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
        /// Get or set ordinal of <see cref="Field"/>
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
        /// Get or set name of key
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
        /// Get or set update rule for <see cref="ReferentialAction"/> for <see cref="Field"/>
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
        /// Get or set delete rule for <see cref="ReferentialAction"/> for <see cref="Field"/>
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
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public class ForeignKeyFields : IEnumerable
    {
        #region [ Members ]

        private readonly Field m_parent;
        private readonly Dictionary<string, ForeignKeyField> m_fields;      // Used for field name lookups
        private readonly List<ForeignKeyField> m_fieldList;                 // Used for field index lookups

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
        /// Get or set field indexes to lookups
        /// </summary>
        internal List<ForeignKeyField> FieldsList
        {
            get
            {
                return m_fieldList;
            }
        }

        /// <summary>
        /// Get the current index of foreign key field information
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ForeignKeyField this[int index]
        {
            get
            {
                if (index < 0 || index >= m_fieldList.Count)
                    return null;

                return m_fieldList[index];
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
        /// Get <see cref="IEnumerator"/> of field lists
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
        /// Get comma separated <see cref="string"/> of <see cref="ForeignKeyField"/>
        /// </summary>
        /// <returns></returns>
        public string GetList()
        {
            StringBuilder fieldList = new StringBuilder();

            foreach (ForeignKeyField field in m_fieldList)
            {
                if (fieldList.Length > 0)
                    fieldList.Append(',');
                fieldList.Append('[');
                fieldList.Append(field.ForeignKey.Name);
                fieldList.Append(']');
            }

            return fieldList.ToString();

        }

        #endregion
    }

    /// <summary>
    /// Represents a collection of <see cref="Field"/> values.
    /// </summary>
    [Serializable]
    public class Fields : IEnumerable<Field>, IEnumerable
    {
        #region [ Members ]

        private readonly Table m_parent;

        // Used for field name lookups
        private readonly Dictionary<string, Field> m_fields;

        // Used for field index lookups
        private readonly List<Field> m_fieldList;

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
        /// Get or set to fields lookup 
        /// </summary>
        internal Dictionary<string, Field> FieldDictionary
        {
            get
            {
                return m_fields;
            }
        }

        /// <summary>
        /// Get or set Fields index lookup
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
                if (index < 0 || index >= m_fieldList.Count)
                    return null;

                return m_fieldList[index];
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
        /// Add new <see cref="Field"/> to this collection.
        /// </summary>
        /// <param name="newField">Field to add.</param>
        public void Add(Field newField)
        {
            if ((object)newField.Parent == null)
                newField.Parent = this;

            m_fields.Add(newField.Name, newField);
            m_fieldList.Add(newField);
        }

        /// <summary>
        /// Removes <see cref="Field"/> from the collection.
        /// </summary>
        /// <param name="field">Field to remove.</param>
        public void Remove(Field field)
        {
            if (field.Parent == this)
                field.Parent = null;

            m_fields.Remove(field.Name);
            m_fieldList.Remove(field);
        }

        /// <summary>
        /// Clears the field list.
        /// </summary>
        public void Clear()
        {
            foreach (Field field in m_fieldList)
            {
                if (field.Parent == this)
                    field.Parent = null;
            }

            m_fields.Clear();
            m_fieldList.Clear();
        }

        /// <summary>
        /// Get <see cref="IEnumerator{Field}"/> type of <see cref="Field"/> list.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Field> GetEnumerator()
        {
            return m_fieldList.GetEnumerator();
        }

        /// <summary>
        /// Get comma separated list of <see cref="Field"/>
        /// </summary>
        /// <param name="returnAutoInc"></param>
        /// <param name="sqlEscapeFunction"></param>
        /// <returns></returns>
        public string GetList(bool returnAutoInc = true, Func<string, string> sqlEscapeFunction = null)
        {
            if ((object)sqlEscapeFunction == null)
                sqlEscapeFunction = m_parent.Parent.Parent.SQLEscapeName;

            StringBuilder fieldList = new StringBuilder();

            foreach (Field field in m_fieldList)
            {
                if (!field.AutoIncrement || returnAutoInc)
                {
                    if (fieldList.Length > 0)
                        fieldList.Append(", ");

                    fieldList.Append(sqlEscapeFunction(field.Name));
                }
            }

            return fieldList.ToString();

        }

        /// <summary>
        /// Get <see cref="IEnumerator"/> type of <see cref="Field"/> list.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// Get data table information for data processing
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
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

        // User definable SQL used to retrieve last auto-inc value from identity column
        private string m_identitySQL = "SELECT @@IDENTITY";

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Table"/>.
        /// </summary>
        public Table()
            : this(null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Table"/>.
        /// </summary>
        public Table(string name)
            : this(null, null, name, TableType.Table.ToString(), null, 0)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Table"/>.
        /// </summary>
        public Table(string catalog, string schema, string name, string type, string description, int rows)
        {
            // We only allow internal creation of this object
            m_fields = new Fields(this);

            m_catalog = catalog;
            m_schema = schema;
            m_name = name;
            m_mapName = name;
            m_description = description;

            switch (type.Trim().ToUpper())
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

            m_rows = rows;
            ReevalulateIdentitySQL();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get or set list of <see cref="Fields"/> for <see cref="Table"/>
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
        /// Get or set name of <see cref="Table"/>
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
        /// Get or set process flag
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
        /// Get or set identity SQL for <see cref="Table"/>
        /// </summary>
        public string IdentitySQL
        {
            get
            {
                return m_identitySQL;
            }
            set
            {
                m_identitySQL = value;
            }
        }

        /// <summary>
        /// Get name of <see cref="Table"/>
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_parent?.TableDictionary.Remove(m_name);
                m_parent?.TableDictionary.Add(value, this);
                m_name = value;
            }
        }

        /// <summary>
        /// Get SQL escaped name of <see cref="Table"/>
        /// </summary>
        public string SQLEscapedName
        {
            get
            {
                return m_parent.Parent.SQLEscapeName(m_name);
            }
        }

        /// <summary>
        /// Get or set full name of <see cref="Table"/>
        /// </summary>
        public string FullName
        {
            get
            {
                Schema schema = m_parent.Parent;

                string strFullName = "";

                if (!string.IsNullOrWhiteSpace(m_catalog))
                    strFullName += schema.SQLEscapeName(m_catalog) + ".";

                if (!string.IsNullOrWhiteSpace(m_schema))
                    strFullName += schema.SQLEscapeName(m_schema) + ".";

                strFullName += schema.SQLEscapeName(m_name);

                return strFullName;
            }
        }

        /// <summary>
        /// Get or set catalog information for <see cref="Table"/>
        /// </summary>
        public string Catalog
        {
            get
            {
                return m_catalog;
            }
            set
            {
                m_catalog = value;
            }
        }

        /// <summary>
        /// Get or set schema name
        /// </summary>
        public string Schema
        {
            get
            {
                return m_schema;
            }
            set
            {
                m_schema = value;
            }
        }

        /// <summary>
        /// Get <see cref="TableType"/>
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public TableType Type
        {
            get
            {
                return m_tableType;
            }
            set
            {
                m_tableType = value;
            }
        }

        /// <summary>
        /// Get or set description
        /// </summary>
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
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
            internal set
            {
                m_parent = value;
                ReevalulateIdentitySQL();

                if (m_rows == 0)
                    CalculateRowCount();
            }
        }

        /// <summary>
        /// Get <see cref="IDbConnection"/> of object
        /// </summary>
        public IDbConnection Connection
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
        /// Get count for primary key <see cref="Field"/>
        /// </summary>
        public int PrimaryKeyFieldCount
        {
            get
            {
                int count = 0;

                foreach (Field field in m_fields)
                {
                    if (field.IsPrimaryKey)
                        count += 1;
                }

                return count;
            }
        }

        /// <summary>
        /// Get flag that determines if the table has any foreign keys.
        /// </summary>
        public bool ReferencedByForeignKeys
        {
            get
            {
                foreach (Field field in m_fields)
                {
                    if (field.IsPrimaryKey && field.ForeignKeys.Count > 0)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Get flag of any foreign key <see cref="Field"/>
        /// </summary>
        public bool IsForeignKeyTable
        {
            get
            {
                foreach (Field field in m_fields)
                {
                    if (field.IsForeignKey)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets flag that determines if the <see cref="Table"/> has an auto-increment field.
        /// </summary>
        public bool HasAutoIncField
        {
            get
            {
                return (object)AutoIncField != null;
            }
        }

        /// <summary>
        /// Gets auto-increment field for the <see cref="Table"/>, if any.
        /// </summary>
        public Field AutoIncField
        {
            get
            {
                foreach (Field field in m_fields)
                {
                    if (field.AutoIncrement)
                        return field;
                }

                return null;
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
            if (Parent.Parent.DataSourceType == DatabaseType.SQLServer)
                return (string.Compare(m_schema, "dbo", StringComparison.OrdinalIgnoreCase) == 0);

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
            if (obj is Table table)
                return CompareTo(table);

            throw new ArgumentException("Table can only be compared to other Tables");
        }

        /// <summary>
        /// Compare Table with other <see cref="Table"/> object <see cref="Priority"/>
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
            Table table;
            bool tableIsInStack;

            if ((object)tableStack == null)
                tableStack = new List<Table>();

            tableStack.Add(this);

            foreach (Field field in m_fields)
            {
                foreach (ForeignKeyField foreignKey in field.ForeignKeys)
                {
                    // We don't want to circle back on ourselves
                    table = foreignKey.ForeignKey.Table;
                    tableIsInStack = tableStack.Exists(tbl => string.Compare(tbl.Name, table.Name, StringComparison.OrdinalIgnoreCase) == 0);

                    if (tableIsInStack)
                    {
                        if (string.Compare(table.Name, otherTable.Name, StringComparison.OrdinalIgnoreCase) == 0)
                            return true;
                    }
                    else
                    {
                        if (table.IsReferencedBy(otherTable, tableStack))
                            return true;

                        if (string.Compare(table.Name, otherTable.Name, StringComparison.OrdinalIgnoreCase) == 0)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check for direct table reference by <paramref name="otherTable"/>.
        /// </summary>
        /// <param name="otherTable">Table to check for relation.</param>
        /// <returns><c>true</c> if directly referenced; otherwise, <c>false</c>.</returns>
        public bool IsReferencedBy(Table otherTable)
        {
            return IsReferencedBy(otherTable, null);
        }

        /// <summary>
        /// Checks for indirect table reference through <paramref name="otherTable"/>.
        /// </summary>
        /// <param name="otherTable">Table to check for relation.</param>
        /// <returns><c>true</c> if indirectly referenced; otherwise, <c>false</c>.</returns>
        public bool IsReferencedVia(Table otherTable)
        {
            Table table;

            foreach (Field field in otherTable.Fields)
            {
                foreach (ForeignKeyField foreignKey in field.ForeignKeys)
                {
                    table = foreignKey.ForeignKey.Table;

                    // Not a direct relation, but children are related
                    if (string.Compare(m_name, table.Name, StringComparison.OrdinalIgnoreCase) != 0 && IsReferencedBy(table))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// check for primary key field in <see cref="Table"/>
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="primaryKeyOrdinal"></param>
        /// <param name="primaryKeyName"></param>
        /// <returns></returns>
        public bool DefinePrimaryKey(string fieldName, int primaryKeyOrdinal = -1, string primaryKeyName = "")
        {
            Field lookupField;

            lookupField = m_fields[fieldName];

            if ((object)lookupField != null)
            {
                lookupField.IsPrimaryKey = true;
                lookupField.PrimaryKeyOrdinal = (primaryKeyOrdinal == -1 ? PrimaryKeyFieldCount + 1 : primaryKeyOrdinal);
                lookupField.PrimaryKeyName = primaryKeyName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check for <see cref="ForeignKeyField"/>
        /// </summary>
        /// <param name="primaryKeyFieldName"></param>
        /// <param name="foreignKeyTableName"></param>
        /// <param name="foreignKeyFieldName"></param>
        /// <param name="foreignKeyOrdinal"></param>
        /// <param name="foreignKeyName"></param>
        /// <param name="foreignKeyUpdateRule"></param>
        /// <param name="foreignKeyDeleteRule"></param>
        /// <returns></returns>
        public bool DefineForeignKey(string primaryKeyFieldName, string foreignKeyTableName, string foreignKeyFieldName, int foreignKeyOrdinal = -1, string foreignKeyName = "", ReferentialAction foreignKeyUpdateRule = ReferentialAction.NoAction, ReferentialAction foreignKeyDeleteRule = ReferentialAction.NoAction)
        {
            Field localPrimaryKeyField;
            Table remoteForeignKeyTable;
            Field remoteForeignKeyField;

            localPrimaryKeyField = m_fields[primaryKeyFieldName];

            if ((object)localPrimaryKeyField != null)
            {
                remoteForeignKeyTable = m_parent[foreignKeyTableName];

                if ((object)remoteForeignKeyTable != null)
                {
                    remoteForeignKeyField = remoteForeignKeyTable.Fields[foreignKeyFieldName];

                    if ((object)remoteForeignKeyField != null)
                    {
                        ForeignKeyField localForeignKeyField = new ForeignKeyField(localPrimaryKeyField.ForeignKeys);

                        localForeignKeyField.PrimaryKey = localPrimaryKeyField;
                        localForeignKeyField.ForeignKey = remoteForeignKeyField;
                        localForeignKeyField.ForeignKey.ReferencedBy = localForeignKeyField.PrimaryKey;
                        localForeignKeyField.Ordinal = (foreignKeyOrdinal == -1 ? localPrimaryKeyField.ForeignKeys.Count + 1 : foreignKeyOrdinal);
                        localForeignKeyField.KeyName = foreignKeyName;
                        localForeignKeyField.UpdateRule = foreignKeyUpdateRule;
                        localForeignKeyField.DeleteRule = foreignKeyDeleteRule;

                        localPrimaryKeyField.ForeignKeys.Add(localForeignKeyField);

                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Re-evaluates identity SQL for database type.
        /// </summary>
        public void ReevalulateIdentitySQL()
        {
            switch (m_parent?.Parent.DataSourceType)
            {
                case DatabaseType.SQLServer:
                    m_identitySQL = "SELECT IDENT_CURRENT('" + Name + "')";
                    break;

                case DatabaseType.Oracle:
                    m_identitySQL = "SELECT SEQ_" + Name + ".CURRVAL from dual";
                    break;

                case DatabaseType.SQLite:
                    m_identitySQL = "SELECT last_insert_rowid()";
                    break;

                case DatabaseType.PostgreSQL:
                    if ((object)AutoIncField != null)
                        m_identitySQL = "SELECT currval(pg_get_serial_sequence('" + Name.ToLower() + "', '" + AutoIncField.Name.ToLower() + "'))";
                    else
                        m_identitySQL = "SELECT lastval()";

                    break;

                default:
                    m_identitySQL = "SELECT @@IDENTITY";
                    break;
            }
        }

        /// <summary>
        /// Calculates row count.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void CalculateRowCount()
        {
            if (m_tableType == TableType.Table)
            {
                try
                {
                    IDbCommand command = m_parent.Parent.Connection.CreateCommand();

                    command.CommandText = "SELECT COUNT(*) FROM " + SQLEscapedName;
                    command.CommandType = CommandType.Text;

                    m_rows = Convert.ToInt32(command.ExecuteScalar());
                }
                catch
                {
                    m_rows = 0;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// List of <see cref="Table"/> collection
    /// </summary>
    [Serializable]
    public class Tables : IEnumerable<Table>, IEnumerable
    {
        #region [ Memebers ]

        //Fields
        private Schema m_parent;

        // Used for table name lookups
        private readonly Dictionary<string, Table> m_tables;

        // Used for table index lookups
        private readonly List<Table> m_tableList;

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

        /// <summary>
        /// Gets table count.
        /// </summary>
        public int Count
        {
            get
            {
                return m_tableList.Count;
            }
        }

        /// <summary>
        /// Gets or sets parent <see cref="Schema"/>.
        /// </summary>
        public Schema Parent
        {
            get
            {
                return m_parent;
            }
            internal set
            {
                m_parent = value;

                foreach (Table table in m_tableList)
                {
                    table.ReevalulateIdentitySQL();
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds new table.
        /// </summary>
        /// <param name="table">Table to add.</param>
        public void Add(Table table)
        {
            if ((object)table.Parent == null)
                table.Parent = this;

            m_tables.Add(table.Name, table);
            m_tableList.Add(table);
        }

        /// <summary>
        /// Removes table.
        /// </summary>
        /// <param name="table">Table to remove.</param>
        public void Remove(Table table)
        {
            if (table.Parent == this)
                table.Parent = null;

            m_tables.Remove(table.Name);
            m_tableList.Remove(table);
        }

        /// <summary>
        /// Clears all tables.
        /// </summary>
        public void Clear()
        {
            foreach (Table table in m_tableList)
            {
                if (table.Parent == this)
                    table.Parent = null;
            }

            m_tables.Clear();
            m_tableList.Clear();
        }

        /// <summary>
        /// Gets table at index.
        /// </summary>
        /// <param name="index">Index of table</param>
        /// <returns>Table at index.</returns>
        public Table this[int index]
        {
            get
            {
                if (index < 0 || index >= m_tableList.Count)
                    return null;

                return m_tableList[index];
            }
        }

        /// <summary>
        /// Gets table by name.
        /// </summary>
        /// <param name="name">Table name.</param>
        /// <returns>Table with specified name.</returns>
        public Table this[string name]
        {
            get
            {
                Table lookup;
                m_tables.TryGetValue(name, out lookup);
                return lookup;
            }
        }

        /// <summary>
        /// Finds table by mapped named.
        /// </summary>
        /// <param name="mapName">Mapped table name.</param>
        /// <returns>Table with mapped name.</returns>
        public Table FindByMapName(string mapName)
        {

            foreach (Table table in m_tableList)
            {
                if (string.Compare(table.MapName, mapName, StringComparison.OrdinalIgnoreCase) == 0)
                    return table;
            }

            return null;

        }

        /// <summary>
        /// Gets table enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Table> GetEnumerator()
        {
            return m_tableList.GetEnumerator();
        }

        /// <summary>
        /// Gets table field list.
        /// </summary>
        /// <returns>Comma separated field list.</returns>
        public string GetList()
        {
            StringBuilder fieldList = new StringBuilder();

            foreach (Table tbl in m_tableList)
            {
                if (fieldList.Length > 0)
                    fieldList.Append(',');

                fieldList.Append(tbl.SQLEscapedName);
            }

            return fieldList.ToString();

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region [ Inner Class ]

        /// <summary>
        /// Check for referential order of <see cref="Table"/>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class ReferentialOrderComparer : IComparer<Table>
        {

            #region  [ Properties ]

            /// <summary>
            /// Default property of object
            /// </summary>
            [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
            public readonly static ReferentialOrderComparer Default = new ReferentialOrderComparer();

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Allows tables to be sorted in proper referential integrity process object
            /// </summary>
            /// <param name="table1">First table to compare.</param>
            /// <param name="table2">Second table to compare.</param>
            /// <returns></returns>
            public int Compare(Table table1, Table table2)
            {
                // This function allows tables to be sorted in proper referential integrity process order
                int result = 0;

                if (table1 == table2)
                    return 0;

                if (table1.IsReferencedBy(table2))// || table1.IsReferencedVia(table2))
                    result = -1;
                else if (table2.IsReferencedBy(table1))// || table2.IsReferencedVia(table1))
                    result = 1;

                // Sort by existence of foreign key fields, if defined
                if (result == 0)
                    result = ForeignKeyCompare(table1, table2);

                return result;
            }

            /// <summary>
            /// Compare foreign key comparison of tables
            /// </summary>
            /// <param name="table1"></param>
            /// <param name="table2"></param>
            /// <returns></returns>
            private int ForeignKeyCompare(Table table1, Table table2)
            {
                if (table1.IsForeignKeyTable && table2.IsForeignKeyTable)
                    return 0;   // Both tables have foreign key fields, consider them equal

                if (!table1.IsForeignKeyTable && !table2.IsForeignKeyTable)
                    return 0;   // Neither table has foreign key fields, consider them equal

                if (table1.IsForeignKeyTable)
                    return 1;   // Table1 has foreign key fields and Table2 does not, sort it below

                return -1;      // Table2 has foreign key fields and Table1 does not, sort it below
            }

            ///// <summary>
            ///// We compare based on the existence of AutoInc fields as a secondary compare in case user
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
    [Serializable]
    public class Schema
    {
        #region [ Members ]

        // Fields
        private Tables m_tables;

        /// <summary>
        /// Defines a table filter that specifies no restrictions.
        /// </summary>
        public const TableType NoRestriction = TableType.Table | TableType.View | TableType.SystemTable | TableType.SystemView | TableType.Alias | TableType.Synonym | TableType.GlobalTemp | TableType.LocalTemp | TableType.Link | TableType.Undetermined;

        [NonSerialized]
        private IDbConnection m_schemaConnection;

        [NonSerialized]
        private string m_connectionString;

        private DatabaseType m_databaseType;
        private TableType m_restriction;
        private bool m_immediateClose;
        private bool m_allowTextNulls;
        private bool m_allowNumericNulls;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Schema"/>.
        /// </summary>
        public Schema()
        {
            m_connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\\SourceDB.mdb";
            m_databaseType = DatabaseType.Other;
            m_restriction = NoRestriction;
            m_immediateClose = true;
            m_allowTextNulls = false;
            m_allowNumericNulls = false;
            m_tables = new Tables(this);
        }

        /// <summary>
        /// Creates a new <see cref="Schema"/>.
        /// </summary>
        public Schema(string connectionString, TableType tableTypeRestriction = NoRestriction, bool immediateClose = true, bool analyzeNow = true)
        {
            m_connectionString = connectionString;
            m_restriction = tableTypeRestriction;
            m_immediateClose = immediateClose;
            m_allowTextNulls = false;
            m_allowNumericNulls = false;

            if (analyzeNow)
                Analyze();
            else
                m_tables = new Tables(this);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Get or set - information to process <see cref="Tables"/>
        /// </summary>
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
        /// OLEDB connection string to data source to analyze.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
            }
        }

        /// <summary>
        /// Set this value to restrict the types of tables returned in your schema.  Table types can be OR'd together to create this table type restriction.
        /// </summary>
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
        public bool AllowTextNulls
        {
            get
            {
                return m_allowTextNulls;
            }
            set
            {
                m_allowTextNulls = value;
            }
        }

        /// <summary>
        /// Set this value to False to convert all Null values encountered in numeric fields to zeros.
        /// </summary>
        public bool AllowNumericNulls
        {
            get
            {
                return m_allowNumericNulls;
            }
            set
            {
                m_allowNumericNulls = value;
            }
        }

        /// <summary>
        /// <see cref="IDbConnection"/> to open a database connection
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                return m_schemaConnection;
            }
            set
            {
                m_schemaConnection = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Escapes a field or table name.
        /// </summary>
        /// <param name="name">Name to escape.</param>
        /// <returns>Escaped <paramref name="name"/>.</returns>
        public string SQLEscapeName(string name)
        {
            switch (m_databaseType) //-V3002
            {
                case DatabaseType.Access:
                case DatabaseType.SQLServer:
                    return "[" + name + "]";
                case DatabaseType.Oracle:
                    return "\"" + name.ToUpper() + "\"";
                case DatabaseType.PostgreSQL:
                    return "\"" + name.ToLower() + "\"";
            }

            return "\"" + name + "\"";
        }

        /// <summary>
        /// Analyze data schema for processing data
        /// </summary>
        public void Analyze()
        {
            DatabaseType databaseType;
            Schema deserializedSchema;
            bool isAdoConnection;

            m_schemaConnection = OpenConnection(m_connectionString, out databaseType, out deserializedSchema, out isAdoConnection);

            if (isAdoConnection)
            {
                if ((object)deserializedSchema == null)
                    throw new InvalidOperationException("Cannot use an ADO style connection with out a serialized schema.\r\nValidate that the \"serializedSchema\" connection string parameter exists and refers to an existing file.");

                // Reference table collection from deserialized collection
                m_tables = deserializedSchema.Tables;

                // Update database type and force re-evaluation of SQL identity statements
                m_databaseType = databaseType;
                m_tables.Parent = this;

                // Set normal ANSI SQL quotes mode for MySQL
                if (databaseType == DatabaseType.MySQL)
                    m_schemaConnection.ExecuteNonQuery("SET sql_mode='ANSI_QUOTES'");

                // Validate table / field existence against open connection as defined in serialized schema
                List<Table> tablesToRemove = new List<Table>();

                foreach (Table table in m_tables)
                {
                    try
                    {
                        // Make sure table exists
                        m_schemaConnection.ExecuteScalar("SELECT COUNT(*) FROM " + table.SQLEscapedName);

                        List<Field> fieldsToRemove = new List<Field>();
                        string testFieldSQL;

                        try
                        {
                            // If table has an auto-inc field, this will typically be indexed and will allow for a faster field check than a count
                            if (table.HasAutoIncField)
                                testFieldSQL = "SELECT {0} FROM " + table.SQLEscapedName + " WHERE " + table.AutoIncField.SQLEscapedName + " < 0";
                            else
                                testFieldSQL = "SELECT COUNT({0}) FROM " + table.SQLEscapedName;
                        }
                        catch
                        {
                            testFieldSQL = "SELECT COUNT({0}) FROM " + table.SQLEscapedName;
                        }

                        foreach (Field field in table.Fields)
                        {
                            try
                            {
                                // Make sure field exists
                                m_schemaConnection.ExecuteScalar(string.Format(testFieldSQL, field.SQLEscapedName));
                            }
                            catch
                            {
                                fieldsToRemove.Add(field);
                            }
                        }

                        foreach (Field field in fieldsToRemove)
                        {
                            table.Fields.Remove(field);
                        }
                    }
                    catch
                    {
                        tablesToRemove.Add(table);
                    }
                }

                foreach (Table table in tablesToRemove)
                {
                    m_tables.Remove(table);
                }

                // Check to see if user requested to keep connection open, this is just for convience...
                if (m_immediateClose)
                    Close();

                return;
            }

            // If connection is OleDB, attempt to directly infer schema
            AnalyzeOleDbSchema();
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void AnalyzeOleDbSchema()
        {
            DataRow row;
            Table table;
            Field field;
            int x;
            int y;

            // See http://technet.microsoft.com/en-us/library/ms131488.aspx for detailed OLEDB schema row set information
            m_tables = new Tables(this);

            OleDbConnection oledbSchemaConnection = m_schemaConnection as OleDbConnection;

            if ((object)oledbSchemaConnection == null)
                throw new NullReferenceException("Current connection is an ADO style connection, OLE DB connection expected");

            // Attach to schema connection state change event
            oledbSchemaConnection.StateChange += SchemaConnection_StateChange;

            // Load all tables and views into the schema
            DataTable schemaTable = oledbSchemaConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables_Info, null);

            if ((object)schemaTable == null)
                throw new NullReferenceException("Failed to retrieve OLE DB schema table for OleDbSchemaGuid.Tables_Info");

            for (x = 0; x < schemaTable.Rows.Count; x++)
            {
                row = schemaTable.Rows[x];

                table = new Table(Common.ToNonNullString(row["TABLE_CATALOG"]),
                                Common.ToNonNullString(row["TABLE_SCHEMA"]),
                                row["TABLE_NAME"].ToString(),
                                row["TABLE_TYPE"].ToString(),
                                Common.ToNonNullString(row["DESCRIPTION"], ""), 0);

                table.Parent = m_tables;

                if ((table.Type & m_restriction) == m_restriction)
                {
                    // Both the data adapter and the OleDB schema row sets provide column properties
                    // that the other doesn't - so we use both to get a very complete schema                        
                    DataSet data = new DataSet();
                    OleDbDataAdapter adapter = new OleDbDataAdapter();

                    if (table.Name.IndexOf(' ') == -1 && table.UsesDefaultSchema())
                    {
                        try
                        {
                            // For standard table names we can use direct table commands for speed
                            adapter.SelectCommand = new OleDbCommand(table.Name, oledbSchemaConnection);
                            adapter.SelectCommand.CommandType = CommandType.TableDirect;
                            adapter.FillSchema(data, SchemaType.Mapped);
                        }
                        catch
                        {
                            // We'll fall back on the standard method (maybe provider doesn't support TableDirect)
                            adapter.SelectCommand = new OleDbCommand("SELECT TOP 1 * FROM " + table.FullName, oledbSchemaConnection);
                            adapter.FillSchema(data, SchemaType.Mapped);
                        }
                    }
                    else
                    {
                        // For schema based databases and non-standard table names we must use a regular select command
                        adapter.SelectCommand = new OleDbCommand("SELECT TOP 1 * FROM " + table.FullName, oledbSchemaConnection);
                        adapter.FillSchema(data, SchemaType.Mapped);
                    }

                    // Load all column data into the schema
                    DataTable currentTable = oledbSchemaConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] {
                        null,
                        null,
                        table.Name
                    });

                    if ((object)currentTable != null)
                    {
                        for (y = 0; y < currentTable.Rows.Count; y++)
                        {
                            row = currentTable.Rows[y];

                            // New field encountered, create new field
                            field = new Field(row["COLUMN_NAME"].ToString(), (OleDbType)row["DATA_TYPE"]);
                            field.HasDefault = Convert.ToBoolean(Common.NotNull(row["COLUMN_HASDEFAULT"], false));
                            field.NumericPrecision = Convert.ToInt32(Common.NotNull(row["NUMERIC_PRECISION"], false));
                            field.NumericScale = Convert.ToInt32(Common.NotNull(row["NUMERIC_SCALE"], false));
                            field.DateTimePrecision = Convert.ToInt32(Common.NotNull(row["DATETIME_PRECISION"], false));
                            field.Description = Common.ToNonNullString(row["DESCRIPTION"], "");

                            // We also use as many properties as we can from data adapter schema
                            field.Ordinal = data.Tables[0].Columns[field.Name].Ordinal;

                            field.AllowsNulls = data.Tables[0].Columns[field.Name].AllowDBNull;
                            field.DefaultValue = data.Tables[0].Columns[field.Name].DefaultValue;
                            field.MaxLength = data.Tables[0].Columns[field.Name].MaxLength;
                            field.AutoIncrement = data.Tables[0].Columns[field.Name].AutoIncrement;
                            field.AutoIncrementSeed = Convert.ToInt32(data.Tables[0].Columns[field.Name].AutoIncrementSeed);
                            field.AutoIncrementStep = Convert.ToInt32(data.Tables[0].Columns[field.Name].AutoIncrementStep);
                            field.ReadOnly = data.Tables[0].Columns[field.Name].ReadOnly;
                            field.Unique = data.Tables[0].Columns[field.Name].Unique;

                            // Add field to table's field collection
                            table.Fields.Add(field);
                        }
                    }

                    // Sort all loaded fields in ordinal order
                    table.Fields.FieldList.Sort();

                    // Define primary keys
                    try
                    {
                        DataTable primaryKeyTable = oledbSchemaConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new object[] {
                            null,
                            null,
                            table.Name
                        });

                        if ((object)primaryKeyTable != null)
                        {
                            for (y = 0; y <= primaryKeyTable.Rows.Count - 1; y++)
                            {
                                row = primaryKeyTable.Rows[y];
                                table.DefinePrimaryKey(row["COLUMN_NAME"].ToString(), Convert.ToInt32(Common.NotNull(row["ORDINAL"], -1)), Common.ToNonNullString(row["PK_NAME"], ""));
                            }
                        }
                    }
                    catch
                    {
                        // It's possible that the data source doesn't provide a primary keys row set
                    }

                    // Add table to schema's table collection
                    m_tables.Add(table);
                }
            }

            // Define foreign keys (must be done after all tables are defined so relations can be properly established)
            for (int i = 0; i < m_tables.Count; i++)
            {
                table = m_tables[i];

                try
                {
                    DataTable foreignKeyTable = oledbSchemaConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, new object[] {
                        null,
                        null,
                        table.Name
                    });

                    if ((object)foreignKeyTable != null)
                    {
                        for (x = 0; x <= foreignKeyTable.Rows.Count - 1; x++)
                        {
                            row = foreignKeyTable.Rows[x];
                            table.DefineForeignKey(row["PK_COLUMN_NAME"].ToString(),
                                        row["FK_TABLE_NAME"].ToString(), row["FK_COLUMN_NAME"].ToString(),
                                        Convert.ToInt32(Common.NotNull(row["ORDINAL"], -1)), Common.ToNonNullString(row["FK_NAME"], ""),
                                        Field.GetReferentialAction(Common.ToNonNullString(row["UPDATE_RULE"], "")),
                                        Field.GetReferentialAction(Common.ToNonNullString(row["DELETE_RULE"], "")));
                        }
                    }
                }
                catch
                {
                    // It's possible that the data source doesn't provide a foreign keys row set
                }
            }

            // Using a simple (i.e., stable) sorting algorithm here since not all relationships will
            // be considered mathematically congruent and the fast .NET sort algorithm depends on
            // comparisons based on perfect equality (i.e., if A > B and B > C then A > C - this may
            // not be true in terms of referential integrity)
            List<Table> sortedList = new List<Table>(m_tables.TableList);
            Table temp;

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

            // Set initial I/O processing priorities for tables based on this order.  Processing tables
            // based on the "Priority" field allows user to have final say in processing order
            for (x = 0; x < sortedList.Count; x++)
            {
                m_tables.TableList.Find(tbl => string.Compare(tbl.Name, sortedList[x].Name, StringComparison.OrdinalIgnoreCase) == 0).Priority = x;
            }

            // Detach from schema connection state change event
            oledbSchemaConnection.StateChange -= SchemaConnection_StateChange;

            // Check to see if user requested to keep connection open, this is just for convience...
            if (m_immediateClose)
                Close();
        }

        /// <summary>
        /// <see cref="IDbConnection"/> state change event will fire if it unexpectedly close connection while processing.
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
        /// Close <see cref="IDbConnection"/> 
        /// </summary>
        public void Close()
        {
            if ((object)m_schemaConnection != null)
            {
                try
                {
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

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Opens an ADO connection.
        /// </summary>
        /// <param name="connectionString">ADO connection string.</param>
        /// <returns>Opened connection.</returns>
        public static IDbConnection OpenConnection(string connectionString)
        {
            DatabaseType databaseType;
            Schema schema;
            bool isAdoConnection;
            return OpenConnection(connectionString, out databaseType, out schema, out isAdoConnection);
        }

        /// <summary>
        /// Opens an ADO connection.
        /// </summary>
        /// <param name="connectionString">ADO connection string.</param>
        /// <param name="databaseType">Database type.</param>
        /// <param name="deserializedSchema">The deserialized schema.</param>
        /// <param name="isAdoConnection">Flag that determines if connection is ADO.</param>
        /// <returns>Opened connection.</returns>
        public static IDbConnection OpenConnection(string connectionString, out DatabaseType databaseType, out Schema deserializedSchema, out bool isAdoConnection)
        {
            Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();

            deserializedSchema = null;
            databaseType = DatabaseType.Other;
            isAdoConnection = false;

            if (settings.ContainsKey("DataProviderString"))
            {
                // Assuming ADO connection
                string dataProviderString = settings["DataProviderString"];

                settings.Remove("DataProviderString");

                if (settings.ContainsKey("serializedSchema"))
                {
                    string serializedSchemaFileName = FilePath.GetAbsolutePath(settings["serializedSchema"]);

                    if (File.Exists(serializedSchemaFileName))
                    {
                        using (FileStream stream = new FileStream(serializedSchemaFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            deserializedSchema = Serialization.Deserialize<Schema>(stream, SerializationFormat.Binary);
                        }
                    }

                    settings.Remove("serializedSchema");
                }

                // Create updated connection string with removed settings
                connectionString = settings.JoinKeyValuePairs();

                AdoDataConnection database = new AdoDataConnection(connectionString, dataProviderString);

                databaseType = database.DatabaseType;
                isAdoConnection = true;

                return database.Connection;
            }

            // Assuming OLEDB connection
            OleDbConnection connection = new OleDbConnection(connectionString);
            connection.Open();
            return connection;
        }

        #endregion
    }
}
