//******************************************************************************************************
//  TemplateExpression.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/13/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/26/2015 - J. Ritchie Carroll
//       Added advanced expression evaluation option, i.e., eval{expression}
//
//******************************************************************************************************

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#if DNF46
using ExpressionEvaluator;
#endif
using ParsedExpression = System.Tuple<string, bool, string>;
using ParsedEvaluation = System.Tuple<string, string>;

namespace GSF.Parsing
{
    /// <summary>
    /// Represents a template based token substitution parser that supports binary and advanced expressions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// As an example, this parser can use a templated expression of the form:
    /// <code>
    /// {CompanyAcronym}_{DeviceAcronym}[?{SignalType.Source}=Phasor[-{SignalType.Suffix}{SignalIndex}]]:{Signal.Acronymn}
    /// </code>
    /// then replace the tokens with actual values and properly evaluate the expressions.
    /// Example results could look like: GPA_SHELBY-PA1:IPHA and GPA_SHELBY:FREQ
    /// </para>
    /// <para>
    /// Parser also supports more complex C# style expressions using the "eval{}" function, e.g.:
    /// <code>
    /// eval{'{CompanyAcronym}'.Substring(0,3)}_{DeviceAcronym}eval{'[?{SignalType.Source}=Phasor[-{SignalType.Suffix}]]'.Length}
    /// </code>
    /// </para>
    /// </remarks>
    public class TemplatedExpressionParser
    {
        #region [ Members ]

        // Nested Types

        // Operator symbol indices (see s_operatorSymbols definition)
        private struct OperatorType
        {
            // Using struct instead of enum to avoid casts
            public const int Equality = 0;
            public const int Inequality = 1;
            public const int Inequality2 = 2;
            public const int LessThanOrEqual = 3;
            public const int LessThan = 4;
            public const int GreaterThanOrEqual = 5;
            public const int GreaterThan = 6;
            //public const int Equality2 = 7;
        }

        // Constants
        private const string ExpressionParser = @"^[^\{0}\{1}]*(((?<Expressions>(?'Open'\{0})[^\{0}\{1}]*))+((?'Close-Open'\{1})[^\{0}\{1}]*)+)*(?(Open)(?!))$";
        private const string EvaluationParser = @"eval\{0}([^\{0}]+)\{1}"; 

        // Fields
        private readonly Regex m_expressionParser;
        private readonly Regex m_evaluationParser;
        private readonly string[] m_escapedReservedSymbols;
        private readonly string[] m_encodedReservedSymbols;
        private string m_templatedExpression;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="TemplatedExpressionParser"/>.
        /// </summary>
        public TemplatedExpressionParser() : this('{', '}', '[', ']')
        {
        }

        /// <summary>
        /// Creates a new <see cref="TemplatedExpressionParser"/> with desired delimiters.
        /// </summary>
        /// <param name="startTokenDelimiter">Character that identifies the beginning of a token.</param>
        /// <param name="endTokenDelimiter">Character that identifies the end of a token.</param>
        /// <param name="startExpressionDelimiter">Character that identifies the beginning of an expression.</param>
        /// <param name="endExpressionDelimiter">Character that identifies the beginning of an expression.</param>
        public TemplatedExpressionParser(char startTokenDelimiter, char endTokenDelimiter, char startExpressionDelimiter, char endExpressionDelimiter)
        {
            if (startTokenDelimiter == endTokenDelimiter ||
                startTokenDelimiter == startExpressionDelimiter ||
                startTokenDelimiter == endExpressionDelimiter ||
                endTokenDelimiter == startExpressionDelimiter ||
                endTokenDelimiter == endExpressionDelimiter ||
                startExpressionDelimiter == endExpressionDelimiter)
                throw new ArgumentException("All delimiters must be unique");

            // Define a regular expression that can parse nested binary expressions
            m_expressionParser = new Regex(string.Format(ExpressionParser, startExpressionDelimiter, endExpressionDelimiter), RegexOptions.Compiled | RegexOptions.CultureInvariant);

            // Define a regular expression that will find each "eval[]" expression
            m_evaluationParser = new Regex(string.Format(EvaluationParser, startTokenDelimiter, endTokenDelimiter), RegexOptions.Compiled);

            // Reserved symbols include all expression operators and delimiters (hashset keeps symbol list unique)
            HashSet<string> escapedReservedSymbols = new HashSet<string>(new[] { "\\\\", "\\<", "\\>", "\\=", "\\!", "\\" + startTokenDelimiter, "\\" + endTokenDelimiter, "\\" + startExpressionDelimiter, "\\" + endExpressionDelimiter });
            m_escapedReservedSymbols = escapedReservedSymbols.ToArray();
            m_encodedReservedSymbols = new string[m_escapedReservedSymbols.Length];

            for (int i = 0; i < m_encodedReservedSymbols.Length; i++)
                m_encodedReservedSymbols[i] = m_escapedReservedSymbols[i][1].RegexEncode();

            StartTokenDelimiter = startTokenDelimiter;
            EndTokenDelimiter = endTokenDelimiter;
            StartExpressionDelimiter = startExpressionDelimiter;
            EndExpressionDelimiter = endExpressionDelimiter;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the templated expression to use.
        /// </summary>
        public string TemplatedExpression
        {
            get
            {
                return m_templatedExpression;
            }
            set
            {
                m_templatedExpression = value;

                // Encode any escaped reserved symbols in template expression
                if (!string.IsNullOrEmpty(m_templatedExpression))
                {
                    for (int i = 0; i < m_escapedReservedSymbols.Length; i++)
                        m_templatedExpression = m_templatedExpression.Replace(m_escapedReservedSymbols[i], m_encodedReservedSymbols[i]);
                }
            }
        }

        /// <summary>
        /// Gets the reserved symbols - this includes all delimiters and expression operators.
        /// </summary>
        /// <remarks>
        /// The default reserved symbol list is: \, &lt;, &gt;, =, !, {, }, [ and ]
        /// </remarks>
        public char[] ReservedSymbols => m_escapedReservedSymbols.Select(symbol => symbol[1]).ToArray(); // Return unescaped reserved symbols

        /// <summary>
        /// Gets the character that identifies the beginning of a token.
        /// </summary>
        public char StartTokenDelimiter { get; }

        /// <summary>
        /// Gets the character that identifies the end of a token.
        /// </summary>
        public char EndTokenDelimiter { get; }

        /// <summary>
        /// Gets the character that identifies the start of an expression.
        /// </summary>
        public char StartExpressionDelimiter { get; }

        /// <summary>
        /// Gets the character that identifies the end of an expression.
        /// </summary>
        public char EndExpressionDelimiter { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Executes replacements using provided <paramref name="substitutions"/> dictionary on the currently defined
        /// <see cref="TemplatedExpression"/> value and optionally evaluates any expressions.
        /// </summary>
        /// <param name="substitutions">Dictionary of substitutions. Dictionary keys are tokens to be replaced by the values.</param>
        /// <param name="ignoreCase">Determines if substitutions should be case insensitive. Defaults to <c>true</c>.</param>
        /// <param name="evaluateExpressions">Determines if expressions should be evaluated. Defaults to <c>true</c>.</param>
        /// <returns>A string that was based on <see cref="TemplatedExpression"/> with tokens replaced and expressions evaluated.</returns>
        /// <remarks>
        /// <para>
        /// The default token start and end delimiters are { and } but can be overridden in <see cref="TemplatedExpressionParser"/> constructor.
        /// All substitution tokens surrounded by <see cref="StartTokenDelimiter"/> and <see cref="EndTokenDelimiter"/> (e.g., {token})
        /// will be immediately replaced with their string equivalents before further expression evaluation. As a result of the expression
        /// syntax &lt;, &gt;, =, and ! are reserved expressions symbols; <see cref="StartTokenDelimiter"/>, <see cref="EndTokenDelimiter"/>,
        /// <see cref="StartExpressionDelimiter"/> and <see cref="EndExpressionDelimiter"/> are reserved delimiter symbols. To embed any reserved
        /// symbol into the <see cref="TemplatedExpression"/> so that it appears in the evaluated result, escape the symbol by prefixing it
        /// with a backslash, e.g., \{.
        /// </para>
        /// <para>
        /// The default expression start and end delimiters are [ and ] but can be overridden in <see cref="TemplatedExpressionParser"/> constructor.
        /// Expressions are represented in the form of [?expression[result]] and can be nested, e.g. ([?expression1[?expression2[result]]]).
        /// Expressions should not contain extraneous white space for proper evaluation. Only simple boolean comparison operations are allowed
        /// in expressions, e.g., A=B (or A==B), A!=B (or A&lt;&gt;B), A&gt;B, A&gt;=B, A&lt;B and A&lt;=B - nothing more. Any expression that
        /// fails to evaluate will be evaluated as FALSE. Note that if both left (A) and right (B) operands can be parsed as a numeric value then
        /// the expression will be numerically evaluated otherwise expression will be a culture-invariant string comparison. Nested expressions
        /// are evaluated as cumulative AND operators. There is no defined nesting limit.
        /// </para>
        /// <para>
        /// Advanced expressions can be parsed using the eval function, e.g., eval{1 + 2}. Eval function expression is delimited by the currently
        /// defined token delimiters, { and } by default. The evaluation engine deals with numbers and strings. The typing of numeric literals
        /// follow C# standards, suffixes such as d, f and m may be used to explicitly specify the numeric type. If no numeric suffix is provided,
        /// the default type of a numeric literal is assumed to be <see cref="int"/>. String literals should be specified using single quotes
        /// instead of double quotes, this includes substitution parameters, e.g., <c>eval{'{DeviceAcronym}'.Length + 1}</c>. Advanced evaluation
        /// expressions using the eval function are always parsed after common expressions. Eval functions cannot be nested.
        /// </para>
        /// </remarks>
        public string Execute(IDictionary<string, string> substitutions, bool ignoreCase = true, bool evaluateExpressions = true)
        {
            if (string.IsNullOrEmpty(m_templatedExpression))
                return "";

            string result = m_templatedExpression;

            // Execute substitutions
            foreach (KeyValuePair<string, string> substitution in substitutions)
            {
                if (ignoreCase)
                    result = result.ReplaceCaseInsensitive(substitution.Key, substitution.Value);
                else
                    result = result.Replace(substitution.Key, substitution.Value);
            }

            if (evaluateExpressions)
            {
                // Parse common expressions, i.e., [?expression[result]]
                List<ParsedExpression> parsedExpressions = ParseExpressions(result, ignoreCase);

                // Execute common expression replacements
                foreach (ParsedExpression parsedExpression in parsedExpressions)
                    result = result.Replace(parsedExpression.Item1, parsedExpression.Item2 ? parsedExpression.Item3 : "");

                // Parse evaluation expressions, i.e., eval{expression}
                List<ParsedEvaluation> parsedEvaluations = ParseEvaluations(result);

                // Execute evaluation expression replacements
                foreach (ParsedEvaluation parsedEvaluation in parsedEvaluations)
                    result = result.Replace(parsedEvaluation.Item1, parsedEvaluation.Item2);
            }

            // Decode any reserved symbols that were escaped in original templated expression
            for (int i = 0; i < m_escapedReservedSymbols.Length; i++)
                result = result.Replace(m_encodedReservedSymbols[i], m_escapedReservedSymbols[i].Substring(1));

            return result;
        }

        // Parses expressions of the form "[?expression[result]]". Expressions can be nested, e.g., "[?expression1[?expression2[result]]]".
        // Returns list of complete expressions (used as base replacement text), cumulative boolean expression evaluations and expression results
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private List<ParsedExpression> ParseExpressions(string fieldReplacedTemplatedExpression, bool ignoreCase)
        {
            List<ParsedExpression> parsedExpressions = new List<ParsedExpression>();

            // Find all expressions using regular expression
            Match match = m_expressionParser.Match(fieldReplacedTemplatedExpression);

            if (match.Success)
            {
                Group capturedExpressions = match.Groups["Expressions"];
                StringBuilder completeExpression = new StringBuilder();
                List<bool> evaluations = new List<bool>();
                int depth = 0, lastDepth = 0;

                foreach (Capture capture in capturedExpressions.Captures)
                {
                    if (capture.Value.StartsWith(StartExpressionDelimiter + "?", StringComparison.Ordinal))
                    {
                        int result;
                        bool evaluation;

                        // Found binary operation expression
                        depth++;
                        completeExpression.Append(capture.Value);
                        
                        // Only apply cumulative AND logic for items at continued depth
                        if (depth == lastDepth && evaluations.Count > 0)
                            evaluations.RemoveAt(evaluations.Count - 1);

                        lastDepth = depth;

                        // Parse binary expression
                        CodeBinaryOperatorExpression expression = ParseBinaryOperatorExpression(capture.Value.Substring(2), out TypeCode expressionType);

                        // Evaluate binary expression
                        if ((object)expression != null)
                        {
                            IComparer comparer;

                            // Select comparer based on expression type
                            switch (expressionType)
                            {
                                case TypeCode.Int32:
                                    comparer = Comparer<int>.Default;
                                    break;
                                case TypeCode.Double:
                                    comparer = Comparer<double>.Default;
                                    break;
                                case TypeCode.String:
                                    comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            // Compare operands
                            result = comparer.Compare(((CodePrimitiveExpression)expression.Left).Value, ((CodePrimitiveExpression)expression.Right).Value);

                            // Evaluate comparison result
                            switch (expression.Operator)
                            {
                                case CodeBinaryOperatorType.IdentityEquality:
                                    evaluation = result == 0;
                                    break;
                                case CodeBinaryOperatorType.IdentityInequality:
                                    evaluation = result != 0;
                                    break;
                                case CodeBinaryOperatorType.LessThan:
                                    evaluation = result < 0;
                                    break;
                                case CodeBinaryOperatorType.LessThanOrEqual:
                                    evaluation = result <= 0;
                                    break;
                                case CodeBinaryOperatorType.GreaterThan:
                                    evaluation = result > 0;
                                    break;
                                case CodeBinaryOperatorType.GreaterThanOrEqual:
                                    evaluation = result >= 0;
                                    break;
                                default:
                                    evaluation = false;
                                    break;
                            }

                            evaluations.Add(evaluation);
                        }
                        else
                        {
                            // Expression evaluation fails if there is not an expression
                            evaluations.Add(false);
                        }
                    }
                    else if (capture.Value.StartsWith(StartExpressionDelimiter.ToString(), StringComparison.Ordinal))
                    {
                        if (depth > 0)
                        {
                            depth++;

                            // Found expression result
                            completeExpression.Append(capture.Value);

                            // Complete closed expression
                            int index = capture.Index + capture.Length;

                            while (index < fieldReplacedTemplatedExpression.Length && fieldReplacedTemplatedExpression[index] == EndExpressionDelimiter)
                            {
                                completeExpression.Append(EndExpressionDelimiter);
                                index++;
                                depth--;
                            }

                            // Add complete expression, cumulative boolean expression evaluation and expression result to parsed expression list
                            parsedExpressions.Add(new ParsedExpression(completeExpression.ToString(), evaluations.All(item => item), capture.Value.Substring(1)));

                            // Reset for next expression
                            completeExpression.Clear();
                            
                            if (depth <= 0)
                            {
                                evaluations.Clear();
                                depth = 0;
                            }
                        }
                        else
                        {
                            // Unbalanced expression - exception not expected since regex should already catch this
                            throw new InvalidOperationException($"Unbalanced delimiters detected in field replaced templated expression \"{fieldReplacedTemplatedExpression}\"");
                        }
                    }
                }
            }

            return parsedExpressions;
        }

        private CodeBinaryOperatorExpression ParseBinaryOperatorExpression(string expression, out TypeCode expressionType)
        {
            if (!string.IsNullOrEmpty(expression))
            {
                string[] operands = expression.Split(s_operatorSymbols, StringSplitOptions.None);

                // Expression is only valid if there are exactly two operands
                if (operands.Length == 2)
                {
                    CodePrimitiveExpression leftOperand, rightOperand;
                    CodeBinaryOperatorType operatorType;

                    if (int.TryParse(operands[0], out int left) && int.TryParse(operands[1], out int right))
                    {
                        // Both operands can be compared as integers
                        leftOperand = new CodePrimitiveExpression(left);
                        rightOperand = new CodePrimitiveExpression(right);
                        expressionType = TypeCode.Int32;
                    }
                    else if (double.TryParse(operands[0], out double leftF) && double.TryParse(operands[1], out double rightF))
                    {
                        // Both operands can be compared as doubles
                        leftOperand = new CodePrimitiveExpression(leftF);
                        rightOperand = new CodePrimitiveExpression(rightF);
                        expressionType = TypeCode.Double;
                    }
                    else
                    {
                        // Default to string comparison of operands
                        leftOperand = new CodePrimitiveExpression(operands[0]);
                        rightOperand = new CodePrimitiveExpression(operands[1]);
                        expressionType = TypeCode.String;
                    }

                    // Determine operator type - order of detection is critical                         Symbol:
                    if (expression.Contains(s_operatorSymbols[OperatorType.Equality]))
                        operatorType = CodeBinaryOperatorType.IdentityEquality;                         // ==
                    else if (expression.Contains(s_operatorSymbols[OperatorType.Inequality]))
                        operatorType = CodeBinaryOperatorType.IdentityInequality;                       // !=
                    else if (expression.Contains(s_operatorSymbols[OperatorType.Inequality2]))
                        operatorType = CodeBinaryOperatorType.IdentityInequality;                       // <>
                    else if (expression.Contains(s_operatorSymbols[OperatorType.LessThanOrEqual]))
                        operatorType = CodeBinaryOperatorType.LessThanOrEqual;                          // <=
                    else if (expression.Contains(s_operatorSymbols[OperatorType.LessThan]))
                        operatorType = CodeBinaryOperatorType.LessThan;                                 // <
                    else if (expression.Contains(s_operatorSymbols[OperatorType.GreaterThanOrEqual]))
                        operatorType = CodeBinaryOperatorType.GreaterThanOrEqual;                       // >=
                    else if (expression.Contains(s_operatorSymbols[OperatorType.GreaterThan]))
                        operatorType = CodeBinaryOperatorType.GreaterThan;                              // >
                    else
                        operatorType = CodeBinaryOperatorType.IdentityEquality;                         // =

                    return new CodeBinaryOperatorExpression(leftOperand, operatorType, rightOperand);
                }
            }

            expressionType = TypeCode.Object;
            return null;
        }

        // Parses expressions of the form "eval{expression}". Expressions cannot be nested.
        // Returns list of complete expressions (used as base replacement text) and evaluation results
        private List<ParsedEvaluation> ParseEvaluations(string fieldReplacedTemplatedExpression)
        {
            List<ParsedEvaluation> parsedEvaluations = new List<ParsedEvaluation>();

            #if DNF46

            MatchCollection matches = m_evaluationParser.Matches(fieldReplacedTemplatedExpression);

            foreach (Match match in matches)
            {
                string source = match.Groups[0].Value;
                string result = new CompiledExpression(match.Groups[1].Value).Eval().ToString();
                parsedEvaluations.Add(new ParsedEvaluation(source, result));
            }

            #endif

            return parsedEvaluations;
        }

        #endregion

        #region [ Static ]

        // Static Fields

        // Operator symbol array - note that order is critical for proper split
        private static readonly string[] s_operatorSymbols = { "==", "!=", "<>", "<=", "<", ">=", ">", "=" };

        #endregion
    }
}
