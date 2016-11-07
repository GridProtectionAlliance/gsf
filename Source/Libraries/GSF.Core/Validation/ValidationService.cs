//******************************************************************************************************
//  ValidationService.cs - Gbtc
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
//  08/19/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace GSF.Validation
{
    /// <summary>
    /// A class that facilitates value validation using <see cref="IValidator"/> implementations.
    /// </summary>
    /// <example>
    /// This example shows how to use <see cref="ValidationService"/> for input validation:
    /// <code>
    /// using System;
    /// using System.Collections.Generic;
    /// using GSF.Validation;
    /// 
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         // Dictionary where captured user input is saved.
    ///         Dictionary&lt;string, string&gt; input = new Dictionary&lt;string, string&gt;();
    /// 
    ///         // Validation service that will validate user input.
    ///         ValidationService validation = new ValidationService(delegate(string source)
    ///             {
    ///                 string value;
    ///                 if (input.TryGetValue(source, out value))
    ///                     return input[source];
    ///                 else
    ///                     return string.Empty;
    ///             });
    /// 
    ///         // Add validation rules to the validation service.
    ///         validation.AddValidation("Name", new NonNullStringValidator());
    ///         validation.AddValidation("Email", new EmailAddressValidator());
    /// 
    ///         // Capture user input.
    ///         Console.Write("Enter name: ");
    ///         input["Name"] = Console.ReadLine();
    ///         Console.Write("Enter email: ");
    ///         input["Email"] = Console.ReadLine();
    ///         Console.WriteLine("");
    ///     
    ///         // Validate user input.
    ///         string errors;
    ///         if (!validation.Validate(out errors))
    ///             Console.WriteLine(errors);
    ///         else
    ///             Console.WriteLine("No validation errors were found!");
    /// 
    ///         Console.ReadLine();
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IValidator"/>
    public class ValidationService
    {
        #region [ Members ]

        // Fields
        private readonly Func<string, object> m_lookup;
        private readonly Dictionary<string, IValidator> m_validations;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationService"/> class.
        /// </summary>
        /// <param name="valueLookupHandler"><see cref="Delegate"/> that will lookup the value to be validated.</param>
        public ValidationService(Func<string, object> valueLookupHandler)
        {
            m_lookup = valueLookupHandler;
            m_validations = new Dictionary<string, IValidator>();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds a new validation.
        /// </summary>
        /// <param name="source">The source that will provide the value to be validated.</param>
        /// <param name="validator">The <see cref="IValidator"/> that will validate the value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="validator"/> is null.</exception>
        public void AddValidation(string source, IValidator validator)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException(nameof(source));

            if ((object)validator == null)
                throw new ArgumentNullException(nameof(validator));

            m_validations.Add(string.Format("{0}+{1}", source, validator.GetType().Name), validator);
        }

        /// <summary>
        /// Executes all validations.
        /// </summary>
        /// <param name="validationErrors">Errors messages returned by one or more of the validations.</param>
        /// <returns><strong>true</strong> if the validation completes without errors; otherwise <strong>false</strong>.</returns>
        public bool Validate(out string validationErrors)
        {
            string error;
            string source;
            StringBuilder result = new StringBuilder();
            foreach (string key in m_validations.Keys)
            {
                source = key.Split('+')[0];
                if (!m_validations[key].Validate(m_lookup(source), out error))
                {
                    if (result.Length == 0)
                        result.Append("One or more validation errors were encountered:\r\n");

                    result.AppendFormat("    {0} - {1}\r\n", source, error);
                }
            }

            validationErrors = result.ToString();
            if (string.IsNullOrEmpty(validationErrors))
                return true;
            else
                return false;
        }

        #endregion
    }
}
