//******************************************************************************************************
//  DataModelBase.cs - Gbtc
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
//  03/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace TimeSeriesFramework.UI
{
    /// <summary>
    /// Represents an abstract base class for all Model objects.
    /// </summary>
    public abstract class DataModelBase : INotifyPropertyChanged, IDataErrorInfo
    {
        #region [ Members ]

        private readonly Dictionary<string, object> m_values = new Dictionary<string, object>();
        private bool m_isValid = true;

        #region [ INotifyPropertyChanged Members ]
        
        // Events
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Indicates if the values associated with this object are valid.
        /// </summary>
        public bool IsValid
        {
            get { return m_isValid; }
            set
            {
                m_isValid = value;
                NotifyPropertyChanged("IsValid");
            }
        }

        #region [ IDataErrorInfo Properties ]
        
        /// <summary>
        /// Represents any errors associated with this object.
        /// </summary>
        public string Error
        {
            get
            {
                string error = string.Empty;                
                foreach (KeyValuePair<string, object> item in m_values)
                {
                    if (item.Key != "IsValid")
                    {
                        string result = OnValidate(item.Key);
                        if (!string.IsNullOrEmpty(result))
                            error += result + Environment.NewLine;
                    }
                }
                return error;
            }
        }

        /// <summary>
        /// Validates value assigned to the property.
        /// </summary>
        /// <param name="propertyName">Name of the property of this object whose value needs to be validated.</param>
        /// <returns></returns>
        public string this[string propertyName]
        {
            get
            {
                return OnValidate(propertyName);
            }
        }

        #endregion

        #endregion

        #region [ Methods ]

        #region [ INotifyPropertyChanged Implementation ]

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);

            }
        }

        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertySelector)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                string propertyName = GetPropertyName(propertySelector);
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region [ Protected Methods ]

        /// <summary>
        /// Sets the value of a property.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="propertySelector">Expression tree contains the property definition.</param>
        /// <param name="value">The property value.</param>
        protected void SetValue<T>(Expression<Func<T>> propertySelector, T value)
        {
            string propertyName = GetPropertyName(propertySelector);

            SetValue<T>(propertyName, value);
        }

        /// <summary>
        /// Sets the value of a property.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The property value.</param>
        protected void SetValue<T>(string propertyName, T value)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Invalid property name", propertyName);
            }

            m_values[propertyName] = value;
            NotifyPropertyChanged(propertyName);
            IsValid = string.IsNullOrEmpty((this as IDataErrorInfo).Error);
        }

        /// <summary>
        /// Gets the value of a property.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="propertySelector">Expression tree contains the property definition.</param>
        /// <returns>The value of the property or default value if not exist.</returns>
        protected T GetValue<T>(Expression<Func<T>> propertySelector)
        {
            string propertyName = GetPropertyName(propertySelector);

            return GetValue<T>(propertyName);
        }

        /// <summary>
        /// Gets the value of a property.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property or default value if not exist.</returns>
        protected T GetValue<T>(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Invalid property name", propertyName);
            }

            object value;
            if (!m_values.TryGetValue(propertyName, out value))
            {
                value = default(T);
                m_values.Add(propertyName, value);
            }

            return (T)value;
        }

        /// <summary>
        /// Validates current instance properties using Data Annotations.
        /// </summary>
        /// <param name="propertyName">This instance property to validate.</param>
        /// <returns>Relevant error string on validation failure or <see cref="System.String.Empty"/> on validation success.</returns>
        protected virtual string OnValidate(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Invalid property name", propertyName);
            }

            string error = string.Empty;
            var value = GetValue(propertyName);
            var results = new List<ValidationResult>(1);
            var result = Validator.TryValidateProperty(
                value,
                new ValidationContext(this, null, null)
                {
                    MemberName = propertyName
                },
                results);

            if (!result)
            {
                var validationResult = results.First();
                error = validationResult.ErrorMessage;
            }

            return error;
        }

        #endregion

        #region [ Private Methods ]

        private string GetPropertyName(LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new InvalidOperationException();
            }

            return memberExpression.Member.Name;
        }

        private object GetValue(string propertyName)
        {
            object value;
            if (!m_values.TryGetValue(propertyName, out value))
            {
                var propertyDescriptor = TypeDescriptor.GetProperties(GetType()).Find(propertyName, false);
                if (propertyDescriptor == null)
                {
                    throw new ArgumentException("Invalid property name", propertyName);
                }

                value = propertyDescriptor.GetValue(this);
                m_values.Add(propertyName, value);
            }

            return value;
        }

        #endregion

        #region Debugging

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides        

        #endregion
              
    }
}
