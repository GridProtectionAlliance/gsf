//******************************************************************************************************
//  BooleanExpression.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/12/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using GSF.Collections;

namespace GSF.Parsing
{
    /// <summary>
    /// Represents a boolean expression that can be parsed and executed at runtime.
    /// </summary>
    /// <remarks>
    /// Binary operators have the same level of precedence and are evaluated from right to left.
    /// </remarks>
    public class BooleanExpression
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Represents a variable that can be tweaked at runtime.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class Variable
        {
            /// <summary>
            /// The identifier used to refer to the variable.
            /// </summary>
            public readonly string Identifier;

            /// <summary>
            /// The value of the variable.
            /// </summary>
            public bool Value;

            /// <summary>
            /// Creates a new instance of the <see cref="Variable"/> class.
            /// </summary>
            /// <param name="identifier">The identifier used to refer to the variable.</param>
            public Variable(string identifier)
            {
                Identifier = identifier;
            }
        }

        // Fields
        private Dictionary<string, Variable> m_variables;
        private Func<bool> m_evaluate;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="BooleanExpression"/> class.
        /// </summary>
        /// <param name="expressionText">The expression text to be parsed as a boolean expression.</param>
        /// <remarks>
        /// The default comparer for identifiers is <see cref="StringComparer.OrdinalIgnoreCase"/>.
        /// </remarks>
        public BooleanExpression(string expressionText)
            : this(expressionText, StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BooleanExpression"/> class.
        /// </summary>
        /// <param name="expressionText">The expression text to be parsed as a boolean expression.</param>
        /// <param name="identifierComparer">Comparer used to compare identifiers.</param>
        public BooleanExpression(string expressionText, IEqualityComparer<string> identifierComparer)
        {
            StringBuilder builder;
            Expression expression;

            m_variables = new Dictionary<string, Variable>(StringComparer.OrdinalIgnoreCase);
            builder = new StringBuilder(expressionText);
            expression = ParseExpression(builder);

            if (builder.Length > 0)
                throw new FormatException($"Unexpected character '{builder[0]}' in expression. Expected end of expression.");

            m_evaluate = Expression.Lambda<Func<bool>>(expression).Compile();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the list of variables found while parsing the boolean expression.
        /// </summary>
        public List<Variable> Variables
        {
            get
            {
                return m_variables.Values.ToList();
            }
        }

        /// <summary>
        /// Gets the variable identified by the given identifier.
        /// </summary>
        /// <param name="identifier">The identifier used to refer to the variable.</param>
        /// <returns>The variable identified by the given identifier.</returns>
        public Variable this[string identifier]
        {
            get
            {
                return m_variables[identifier];
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Evaluates the expression using the
        /// current values of the variables.
        /// </summary>
        /// <returns>The result of the evalulation.</returns>
        public bool Evaluate()
        {
            return m_evaluate();
        }
        
        /// <summary>
        /// Attempts to get the variable identified by the given identifier.
        /// </summary>
        /// <param name="identifier">The identifier used to refer to the variable.</param>
        /// <param name="variable">The variable identified by the given identifier.</param>
        /// <returns>True if the variable is present in the expression; false otherwise.</returns>
        public bool TryGetVariable(string identifier, out Variable variable)
        {
            return m_variables.TryGetValue(identifier, out variable);
        }

        private Expression ParseExpression(StringBuilder builder)
        {
            Expression subexpression;
            char binaryOp;

            subexpression = ParseSubexpression(builder);
            ShedWhitespace(builder);

            if (builder.Length > 0)
            {
                binaryOp = builder[0];

                if (binaryOp == ')')
                    return subexpression;

                builder.Remove(0, 1);
                ShedWhitespace(builder);

                switch (binaryOp)
                {
                    case '&':
                        return Expression.And(subexpression, ParseExpression(builder));

                    case '|':
                        return Expression.Or(subexpression, ParseExpression(builder));

                    case '^':
                        return Expression.ExclusiveOr(subexpression, ParseExpression(builder));

                    default:
                        throw new FormatException($"Unexpected character '{binaryOp}' in expression. Expected: '&', '|', or '^'.");
                }
            }

            return subexpression;
        }

        private Expression ParseSubexpression(StringBuilder builder)
        {
            Expression expression;

            ShedWhitespace(builder);

            if (builder.Length == 0)
                throw new FormatException($"Unexpected end of expression. Expected: '(', '!', '~', or identifier.");

            switch (builder[0])
            {
                case '(':
                    builder.Remove(0, 1);
                    expression = ParseExpression(builder);

                    if (builder.Length == 0)
                        throw new FormatException($"Unexpected end of expression. Expected: ')'.");

                    if (builder[0] != ')')
                        throw new FormatException($"Unexpected character '{builder[0]}' in expression. Expected: ')'.");

                    builder.Remove(0, 1);

                    return expression;

                case '!':
                case '~':
                    builder.Remove(0, 1);
                    return Expression.Not(ParseSubexpression(builder));
            }

            return ParseIdentifier(builder);
        }

        private Expression ParseIdentifier(StringBuilder builder)
        {
            Variable identifier;
            StringBuilder nameBuilder = new StringBuilder();
            string name;

            while (builder.Length > 0 && char.IsLetterOrDigit(builder[0]))
            {
                nameBuilder.Append(builder[0]);
                builder.Remove(0, 1);
            }

            if (nameBuilder.Length == 0)
                throw new FormatException($"Unexpected character '{builder[0]}' in expression. Expected identifier.");

            name = nameBuilder.ToString();
            identifier = m_variables.GetOrAdd(name, key => new Variable(key));

            return ((Expression<Func<bool>>)(() => identifier.Value)).Body;
        }

        private void ShedWhitespace(StringBuilder builder)
        {
            while (builder.Length > 0 && char.IsWhiteSpace(builder[0]))
                builder.Remove(0, 1);
        }

        #endregion
    }
}
