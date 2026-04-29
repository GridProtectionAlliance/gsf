//******************************************************************************************************
//  TemplatedExpressionParserTest.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
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
//  04/28/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable ObjectCreationAsStatement

using System;
using System.Collections.Generic;
using GSF.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests.GSF.Parsing
{
    [TestClass]
    public class TemplatedExpressionParserTest
    {
        // Real-world point-tag templates pulled from production adapter code; preserved verbatim
        // so the tests fail loudly if grammar handling regresses against in-the-field templates.
        private const string PointTagTemplate1 =
            "{CompanyAcronym}_{DeviceAcronym}[?{SignalType.Source}=Phasor[-eval{'{PhasorLabel}'.Trim().ToUpper().Replace(' ','_')}_eval{'{SignalType.Abbreviation}'.Substring(0,1)}eval{'{Phase}'=='+' ? '1' : ('{Phase}'=='-' ? '2' : '{Phase}')}[?{BaseKV}>0[_{BaseKV}]][?{SignalType.Suffix}=PA[:ANG]][?{SignalType.Suffix}=PM[:MAG]]]][?{SignalType.Source}!=Phasor[:{SignalType.Acronym}[?{SignalIndex}!=-1[{SignalIndex}]]]]";

        private const string PointTagTemplate2 =
            "{DeviceAcronym}[?{SignalType.Source}=Phasor[:eval{'{PhasorLabel}'.Trim().ToUpper().Replace(' ','_')}[?{SignalType.Suffix}=PA[.ANG]][?{SignalType.Suffix}=PM[.MAG]]]][?{SignalType.Acronym}=ALOG[:eval{('{Label}'.Trim().Length>0?'{Label}'.Trim().ToUpper().Replace(' ','_'):'ALOG'+((int){SignalIndex}).ToString().PadLeft(2,(char)48))}]][?{SignalType.Source}!=Phasor[?{SignalType.Acronym}!=ALOG[:{SignalType.Acronym}[?{SignalIndex}!=-1[eval{((int){SignalIndex}).ToString().PadLeft(2,(char)48)}]]]]]";

        // Same as Template 1 but with XML-escaped > and slightly different separator/output formatting
        // and an added ALOG branch. The &gt; will not work as a comparison operator without prior
        // XML decoding -- this template models how the value appears when read directly from XML.
        private const string PointTagTemplate3 =
            "{CompanyAcronym}_{DeviceAcronym}[?{SignalType.Source}=Phasor[:eval{'{PhasorLabel}'.Trim().ToUpper().Replace(' ','_')}_eval{'{SignalType.Abbreviation}'.Substring(0,1)}eval{'{Phase}'=='+' ? '1' : ('{Phase}'=='-' ? '2' : '{Phase}')}[?{BaseKV}&gt;0[_{BaseKV}]][?{SignalType.Suffix}=PA[.ANG]][?{SignalType.Suffix}=PM[.MAG]]]][?{SignalType.Acronym}=ALOG[:eval{('{Label}'.Trim().Length > 0 ? '{Label}'.Trim().ToUpper().Replace(' ','_') : 'ALOG'+((int){SignalIndex}).ToString().PadLeft(2,(char)48))}]][?{SignalType.Source}!=Phasor[?{SignalType.Acronym}!=ALOG[:{SignalType.Acronym}[?{SignalIndex}!=-1[eval{((int){SignalIndex}).ToString().PadLeft(2,(char)48)}]]]]]";

        // Heavy-eval template with a leading eval that dispatches on a DEGTOAL! prefix substring
        private const string PointTagTemplate4 =
            "eval{'{DeviceAcronym}'.StartsWith('DEGTOAL!') ? '{DeviceAcronym}'.Substring(8, 14) + (int.Parse('{DeviceAcronym}'.Substring('{DeviceAcronym}'.Length - 2)) % 2 == 0 ? (int.Parse('{DeviceAcronym}'.Substring('{DeviceAcronym}'.Length - 2)) - 1).ToString().PadLeft(2, (char)48) : '{DeviceAcronym}'.Substring('{DeviceAcronym}'.Length - 2)) : '{DeviceAcronym}'}[?{SignalType.Source}=Phasor[eval{'.{PhasorLabel}'.Trim().ToUpper().Replace(' ','')}[?{SignalType.Suffix}=PA[.Aeval{'{DeviceAcronym}'.StartsWith('DEGTOAL!') ? 'R' : ''}]][?{SignalType.Suffix}=PM[.M]]]][?{SignalType.Suffix}=AV[.eval{'{Label}'.Trim().Equals('ToA Latency') ? 'A{BaseKV}TOAL' : '{Label}'.Trim().ToUpper().Replace(' ','')}]][?{SignalType.Suffix}=FQ[.A{BaseKV}FREQ_____1F]][?{SignalType.Suffix}=DF[.A{BaseKV}FREQ_____1R_]][?{SignalType.Suffix}=QF[-QF]][?{SignalType.Suffix}=DV[-{SignalType.Acronym}[?{SignalIndex}!=-1[{SignalIndex}]]]]eval{'{DeviceAcronym}'.StartsWith('DEGTOAL!') ? (int.Parse('{DeviceAcronym}'.Substring('{DeviceAcronym}'.Length - 2)) % 2 == 0 ? '.B' : '.A') : ''}";

        private static string Execute(string template, IDictionary<string, string> substitutions, bool ignoreCase = true, bool evaluateExpressions = true, bool escapeSubstitutionValues = true)
        {
            TemplatedExpressionParser parser = new TemplatedExpressionParser
            {
                TemplatedExpression = template
            };

            return parser.Execute(substitutions ?? new Dictionary<string, string>(), ignoreCase, evaluateExpressions, escapeSubstitutionValues);
        }

        #region [ Sanity / Plumbing ]

        [TestMethod]
        public void Execute_NullOrEmptyTemplate_ReturnsEmpty()
        {
            TemplatedExpressionParser parser = new TemplatedExpressionParser();
            Assert.AreEqual("", parser.Execute(new Dictionary<string, string>()));

            parser.TemplatedExpression = "";
            Assert.AreEqual("", parser.Execute(new Dictionary<string, string>()));
        }

        [TestMethod]
        public void Execute_PlainTextWithoutTokensOrExpressions_PassesThrough()
        {
            Assert.AreEqual("Hello, World!", Execute("Hello, World!", null));
        }

        [TestMethod]
        public void Execute_TokenSubstitution_CaseInsensitiveByDefault()
        {
            Dictionary<string, string> subs = new Dictionary<string, string> { { "{name}", "Alice" } };
            Assert.AreEqual("Hello Alice", Execute("Hello {Name}", subs));
        }

        [TestMethod]
        public void Execute_TokenSubstitution_CaseSensitiveWhenRequested()
        {
            Dictionary<string, string> subs = new Dictionary<string, string> { { "{name}", "Alice" } };
            Assert.AreEqual("Hello {Name}", Execute("Hello {Name}", subs, ignoreCase: false));
        }

        [TestMethod]
        public void Execute_DefaultDelimitersExposed()
        {
            TemplatedExpressionParser parser = new TemplatedExpressionParser();
            Assert.AreEqual('{', parser.StartTokenDelimiter);
            Assert.AreEqual('}', parser.EndTokenDelimiter);
            Assert.AreEqual('[', parser.StartExpressionDelimiter);
            Assert.AreEqual(']', parser.EndExpressionDelimiter);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_NonUniqueDelimiters_Throws()
        {
            new TemplatedExpressionParser('{', '{', '[', ']');
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_ReservedEncodingSymbol_Throws()
        {
            new TemplatedExpressionParser('u', '}', '[', ']');
        }

        #endregion

        #region [ Common Expressions: Operators ]

        [TestMethod]
        public void Common_Equality_SingleEquals_True()
        {
            Assert.AreEqual("YES", Execute("[?A=A[YES]]", null));
        }

        [TestMethod]
        public void Common_Equality_DoubleEquals_True()
        {
            Assert.AreEqual("YES", Execute("[?A==A[YES]]", null));
        }

        [TestMethod]
        public void Common_Equality_FalseProducesEmpty()
        {
            Assert.AreEqual("", Execute("[?A=B[YES]]", null));
        }

        [TestMethod]
        public void Common_Inequality_BangEquals()
        {
            Assert.AreEqual("YES", Execute("[?A!=B[YES]]", null));
            Assert.AreEqual("", Execute("[?A!=A[YES]]", null));
        }

        [TestMethod]
        public void Common_Inequality_AngleBrackets()
        {
            Assert.AreEqual("YES", Execute("[?A<>B[YES]]", null));
            Assert.AreEqual("", Execute("[?A<>A[YES]]", null));
        }

        [TestMethod]
        public void Common_NumericComparisons()
        {
            Assert.AreEqual("LT", Execute("[?5<10[LT]]", null));
            Assert.AreEqual("LE", Execute("[?5<=5[LE]]", null));
            Assert.AreEqual("GT", Execute("[?10>5[GT]]", null));
            Assert.AreEqual("GE", Execute("[?5>=5[GE]]", null));
            Assert.AreEqual("",   Execute("[?5>10[GT]]", null));
        }

        [TestMethod]
        public void Common_DoubleComparison()
        {
            Assert.AreEqual("YES", Execute("[?2.5<3.1[YES]]", null));
        }

        [TestMethod]
        public void Common_StringComparison_IgnoresCaseByDefault()
        {
            // String compare with ignoreCase=true (default)
            Assert.AreEqual("YES", Execute("[?abc=ABC[YES]]", null));

            // ...and is case-sensitive when requested
            Assert.AreEqual("", Execute("[?abc=ABC[YES]]", null, ignoreCase: false));
        }

        [TestMethod]
        public void Common_BadOperandCount_EvaluatesFalse()
        {
            // No operator at all -- ParseBinaryOperatorExpression returns null -> empty result
            Assert.AreEqual("", Execute("[?ABC[YES]]", null));
        }

        #endregion

        #region [ Common Expressions: Nesting / Siblings ]

        [TestMethod]
        public void Common_NestedExpressions_BothTrue()
        {
            Assert.AreEqual("YES", Execute("[?A=A[?B=B[YES]]]", null));
        }

        [TestMethod]
        public void Common_NestedExpressions_OuterFalseShortCircuits()
        {
            Assert.AreEqual("", Execute("[?A=B[?B=B[YES]]]", null));
        }

        [TestMethod]
        public void Common_NestedExpressions_InnerFalseSuppresses()
        {
            Assert.AreEqual("", Execute("[?A=A[?B=C[YES]]]", null));
        }

        [TestMethod]
        public void Common_SiblingExpressions_AtTopLevel()
        {
            // Two independent conditionals at the same level
            Assert.AreEqual("ONETHREE", Execute("[?A=A[ONE]][?A=B[TWO]][?C=C[THREE]]", null));
        }

        [TestMethod]
        public void Common_SiblingExpressions_InsideResult()
        {
            // Outer wraps a result that itself contains siblings
            Assert.AreEqual("X-ONE-THREE", Execute("[?A=A[X[?B=B[-ONE]][?B=C[-TWO]][?D=D[-THREE]]]]", null));
        }

        [TestMethod]
        public void Common_DeepNesting()
        {
            Assert.AreEqual("DEEP", Execute("[?A=A[?B=B[?C=C[?D=D[DEEP]]]]]", null));
        }

        #endregion

        #region [ Reserved Symbols / Substitutions ]

        [TestMethod]
        public void Reserved_BackslashEscapeProducesLiteralReservedSymbol()
        {
            // \[ should appear literally in the output
            Assert.AreEqual("[literal]", Execute(@"\[literal\]", null));
        }

        [TestMethod]
        public void Reserved_SubstitutionValueWithReservedCharsIsEscaped()
        {
            // '!' in the value is reserved; encoded internally and decoded on the way out
            Dictionary<string, string> subs = new Dictionary<string, string> { { "{X}", "A!B" } };
            Assert.AreEqual("A!B", Execute("{X}", subs));
        }

        [TestMethod]
        public void Reserved_SubstitutionValueWithBracketsDoesNotConfuseExpressionParser()
        {
            // '[' and ']' in substitution values must not be parsed as expression delimiters
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{Sig}", "Phasor" },
                { "{Tag}", "[OK]" }
            };
            Assert.AreEqual("[OK]", Execute("[?{Sig}=Phasor[{Tag}]]", subs));
        }

        #endregion

        #region [ PR #521 Regression -- bug repro from author ]

        [TestMethod]
        public void Regression_PR521_EvalIndexerInsideConditionalResult()
        {
            // Verbatim repro from the PR description; the old algorithm threw:
            //   ExpressionEvaluator.Parser.ExpressionParseException: 'Error parsing token '7' at line 1 char 26'
            // because the [7] indexer inside eval{...} got accounted as an expression bracket.
            const string template = "[?{Output}=ModeShapeMagnitude[DAMP.MODE{ModeNum}.eval{'{Input.PointTag}'[7]}.MAG]]";

            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{Output}", "ModeShapeMagnitude" },
                { "{ModeNum}", "50" },
                { "{Input.PointTag}", "GPA_PDC!SHELBY:FREQ" }
            };

            Assert.AreEqual("DAMP.MODE50.!.MAG", Execute(template, subs));
        }

        [TestMethod]
        public void Regression_PR521_EvalIndexerInsideConditionalResult_FalseBranch()
        {
            // Same template, conditional false -> empty result
            const string template = "[?{Output}=ModeShapeMagnitude[DAMP.MODE{ModeNum}.eval{'{Input.PointTag}'[7]}.MAG]]";

            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{Output}", "Other" },
                { "{ModeNum}", "50" },
                { "{Input.PointTag}", "GPA_PDC!SHELBY:FREQ" }
            };

            Assert.AreEqual("", Execute(template, subs));
        }

        #endregion

        #region [ Eval Expressions ]

        [TestMethod]
        public void Eval_SimpleArithmetic()
        {
            Assert.AreEqual("3", Execute("eval{1 + 2}", null));
        }

        [TestMethod]
        public void Eval_StringOperationsAfterSubstitution()
        {
            Dictionary<string, string> subs = new Dictionary<string, string> { { "{Label}", "  va " } };
            Assert.AreEqual("VA", Execute("eval{'{Label}'.Trim().ToUpper()}", subs));
        }

        [TestMethod]
        public void Eval_DisabledWhenEvaluateExpressionsFalse()
        {
            // When expression evaluation is disabled, eval{} should be left intact
            string result = Execute("eval{1+2}", null, evaluateExpressions: false);
            Assert.AreEqual("eval{1+2}", result);
        }

        #endregion

        #region [ Real-world point-tag templates supplied by request ]

        // ---- Template 1: PA / PM / non-Phasor branches ----

        [TestMethod]
        public void PointTagTemplate1_PhasorAngle_PositiveSequence()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{CompanyAcronym}",          "GPA"    },
                { "{DeviceAcronym}",           "SHELBY" },
                { "{SignalType.Source}",       "Phasor" },
                { "{PhasorLabel}",             "VA"     },
                { "{SignalType.Abbreviation}", "Volts"  },
                { "{Phase}",                   "+"      },
                { "{BaseKV}",                  "500"    },
                { "{SignalType.Suffix}",       "PA"     },
                { "{SignalType.Acronym}",      "VPHA"   },
                { "{SignalIndex}",             "0"      }
            };

            Assert.AreEqual("GPA_SHELBY-VA_V1_500:ANG", Execute(PointTagTemplate1, subs));
        }

        [TestMethod]
        public void PointTagTemplate1_PhasorMagnitude_NegativeSequence_NoBaseKV()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{CompanyAcronym}",          "GPA"     },
                { "{DeviceAcronym}",           "SHELBY"  },
                { "{SignalType.Source}",       "Phasor"  },
                { "{PhasorLabel}",             "I A"     },
                { "{SignalType.Abbreviation}", "Amps"    },
                { "{Phase}",                   "-"       },
                { "{BaseKV}",                  "0"       },
                { "{SignalType.Suffix}",       "PM"      },
                { "{SignalType.Acronym}",      "IPHM"    },
                { "{SignalIndex}",             "1"       }
            };

            // Phase '-' -> '2', BaseKV '0' -> branch FALSE so no _BASEKV, suffix PM -> :MAG
            // PhasorLabel "I A" trims to "I A" then upper "I A" then space->_ "I_A"
            Assert.AreEqual("GPA_SHELBY-I_A_A2:MAG", Execute(PointTagTemplate1, subs));
        }

        [TestMethod]
        public void PointTagTemplate1_NonPhasor_WithSignalIndex()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{CompanyAcronym}",          "GPA"        },
                { "{DeviceAcronym}",           "SHELBY"     },
                { "{SignalType.Source}",       "Calculated" },
                { "{PhasorLabel}",             ""           },
                { "{SignalType.Abbreviation}", "F"          },
                { "{Phase}",                   ""           },
                { "{BaseKV}",                  "0"          },
                { "{SignalType.Suffix}",       ""           },
                { "{SignalType.Acronym}",      "FREQ"       },
                { "{SignalIndex}",             "3"          }
            };

            Assert.AreEqual("GPA_SHELBY:FREQ3", Execute(PointTagTemplate1, subs));
        }

        [TestMethod]
        public void PointTagTemplate1_NonPhasor_NegativeSignalIndexHidden()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{CompanyAcronym}",          "GPA"        },
                { "{DeviceAcronym}",           "SHELBY"     },
                { "{SignalType.Source}",       "Frequency"  },
                { "{PhasorLabel}",             ""           },
                { "{SignalType.Abbreviation}", "F"          },
                { "{Phase}",                   ""           },
                { "{BaseKV}",                  "0"          },
                { "{SignalType.Suffix}",       ""           },
                { "{SignalType.Acronym}",      "FREQ"       },
                { "{SignalIndex}",             "-1"         }
            };

            Assert.AreEqual("GPA_SHELBY:FREQ", Execute(PointTagTemplate1, subs));
        }

        // ---- Template 2: Phasor / ALOG / default branches ----

        [TestMethod]
        public void PointTagTemplate2_PhasorAngle()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{DeviceAcronym}",      "SHELBY"  },
                { "{SignalType.Source}",  "Phasor"  },
                { "{SignalType.Suffix}",  "PA"      },
                { "{PhasorLabel}",        "VA Bus1" },
                { "{SignalType.Acronym}", "VPHA"    },
                { "{Label}",              ""        },
                { "{SignalIndex}",        "0"       }
            };

            Assert.AreEqual("SHELBY:VA_BUS1.ANG", Execute(PointTagTemplate2, subs));
        }

        [TestMethod]
        public void PointTagTemplate2_AnalogWithLabel()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{DeviceAcronym}",      "SHELBY"        },
                { "{SignalType.Source}",  "Analog"        },
                { "{SignalType.Suffix}",  ""              },
                { "{PhasorLabel}",        ""              },
                { "{SignalType.Acronym}", "ALOG"          },
                { "{Label}",              " Active Power "},
                { "{SignalIndex}",        "5"             }
            };

            Assert.AreEqual("SHELBY:ACTIVE_POWER", Execute(PointTagTemplate2, subs));
        }

        [TestMethod]
        public void PointTagTemplate2_AnalogWithEmptyLabel_FallsBackToIndex()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{DeviceAcronym}",      "SHELBY" },
                { "{SignalType.Source}",  "Analog" },
                { "{SignalType.Suffix}",  ""       },
                { "{PhasorLabel}",        ""       },
                { "{SignalType.Acronym}", "ALOG"   },
                { "{Label}",              ""       },
                { "{SignalIndex}",        "5"      }
            };

            Assert.AreEqual("SHELBY:ALOG05", Execute(PointTagTemplate2, subs));
        }

        [TestMethod]
        public void PointTagTemplate2_NonPhasorNonAnalog_PaddedIndex()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{DeviceAcronym}",      "SHELBY"     },
                { "{SignalType.Source}",  "Calculated" },
                { "{SignalType.Suffix}",  ""           },
                { "{PhasorLabel}",        ""           },
                { "{SignalType.Acronym}", "STAT"       },
                { "{Label}",              ""           },
                { "{SignalIndex}",        "7"          }
            };

            Assert.AreEqual("SHELBY:STAT07", Execute(PointTagTemplate2, subs));
        }

        // ---- Template 3: same shape as #1 but with XML-escaped > ----
        // Caveat: &gt; is the literal four characters "&gt;" once it reaches the parser; the
        // operator scan looks for '>' and won't find it. The BaseKV branch therefore evaluates
        // false for any value, even positive ones. We assert the templates that *don't* depend
        // on that branch and document the limitation with an explicit test.

        [TestMethod]
        public void PointTagTemplate3_PhasorAngle_BaseKVBranchSkipped()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{CompanyAcronym}",          "GPA"    },
                { "{DeviceAcronym}",           "SHELBY" },
                { "{SignalType.Source}",       "Phasor" },
                { "{PhasorLabel}",             "VA"     },
                { "{SignalType.Abbreviation}", "Volts"  },
                { "{Phase}",                   "+"      },
                { "{BaseKV}",                  "500"    },
                { "{SignalType.Suffix}",       "PA"     },
                { "{SignalType.Acronym}",      "VPHA"   },
                { "{Label}",                   ""       },
                { "{SignalIndex}",             "0"      }
            };

            // BaseKV branch suppressed because '&gt;' isn't recognized as the '>' operator
            Assert.AreEqual("GPA_SHELBY:VA_V1.ANG", Execute(PointTagTemplate3, subs));
        }

        [TestMethod]
        public void PointTagTemplate3_AnalogWithLabel()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{CompanyAcronym}",          "GPA"            },
                { "{DeviceAcronym}",           "SHELBY"         },
                { "{SignalType.Source}",       "Analog"         },
                { "{PhasorLabel}",             ""               },
                { "{SignalType.Abbreviation}", "A"              },
                { "{Phase}",                   ""               },
                { "{BaseKV}",                  "0"              },
                { "{SignalType.Suffix}",       ""               },
                { "{SignalType.Acronym}",      "ALOG"           },
                { "{Label}",                   " Active Power " },
                { "{SignalIndex}",             "2"              }
            };

            Assert.AreEqual("GPA_SHELBY:ACTIVE_POWER", Execute(PointTagTemplate3, subs));
        }

        [TestMethod]
        public void PointTagTemplate3_NonPhasorNonAnalog_PaddedIndex()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{CompanyAcronym}",          "GPA"        },
                { "{DeviceAcronym}",           "SHELBY"     },
                { "{SignalType.Source}",       "Calculated" },
                { "{PhasorLabel}",             ""           },
                { "{SignalType.Abbreviation}", "S"          },
                { "{Phase}",                   ""           },
                { "{BaseKV}",                  "0"          },
                { "{SignalType.Suffix}",       ""           },
                { "{SignalType.Acronym}",      "STAT"       },
                { "{Label}",                   ""           },
                { "{SignalIndex}",             "9"          }
            };

            Assert.AreEqual("GPA_SHELBY:STAT09", Execute(PointTagTemplate3, subs));
        }

        // ---- Template 4: heavy-eval template with DEGTOAL! prefix dispatch ----

        [TestMethod]
        public void PointTagTemplate4_NonDegtoal_PhasorAngle()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{DeviceAcronym}",      "SHELBY"  },
                { "{SignalType.Source}",  "Phasor"  },
                { "{SignalType.Suffix}",  "PA"      },
                { "{SignalType.Acronym}", "VPHA"    },
                { "{PhasorLabel}",        "VA Bus1" },
                { "{Label}",              ""        },
                { "{BaseKV}",             "500"     },
                { "{SignalIndex}",        "0"       }
            };

            // Without DEGTOAL! prefix: device passes through, no R/B/A suffix
            Assert.AreEqual("SHELBY.VABUS1.A", Execute(PointTagTemplate4, subs));
        }

        [TestMethod]
        public void PointTagTemplate4_NonDegtoal_PhasorMagnitude()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{DeviceAcronym}",      "SHELBY"  },
                { "{SignalType.Source}",  "Phasor"  },
                { "{SignalType.Suffix}",  "PM"      },
                { "{SignalType.Acronym}", "VPHM"    },
                { "{PhasorLabel}",        "VA Bus1" },
                { "{Label}",              ""        },
                { "{BaseKV}",             "500"     },
                { "{SignalIndex}",        "0"       }
            };

            Assert.AreEqual("SHELBY.VABUS1.M", Execute(PointTagTemplate4, subs));
        }

        [TestMethod]
        public void PointTagTemplate4_FrequencySuffix()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{DeviceAcronym}",      "SHELBY"     },
                { "{SignalType.Source}",  "Frequency"  },
                { "{SignalType.Suffix}",  "FQ"         },
                { "{SignalType.Acronym}", "FREQ"       },
                { "{PhasorLabel}",        ""           },
                { "{Label}",              ""           },
                { "{BaseKV}",             "500"        },
                { "{SignalIndex}",        "-1"         }
            };

            Assert.AreEqual("SHELBY.A500FREQ_____1F", Execute(PointTagTemplate4, subs));
        }

        [TestMethod]
        public void PointTagTemplate4_DigitalValueSuffixWithIndex()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{DeviceAcronym}",      "SHELBY"     },
                { "{SignalType.Source}",  "Digital"    },
                { "{SignalType.Suffix}",  "DV"         },
                { "{SignalType.Acronym}", "DIGI"       },
                { "{PhasorLabel}",        ""           },
                { "{Label}",              ""           },
                { "{BaseKV}",             "0"          },
                { "{SignalIndex}",        "4"          }
            };

            Assert.AreEqual("SHELBY-DIGI4", Execute(PointTagTemplate4, subs));
        }

        [TestMethod]
        public void PointTagTemplate4_DegtoalPrefix_EvenIndex()
        {
            // DEGTOAL! prefix triggers Substring(8,14) and parity adjustment of trailing 2 chars.
            // DeviceAcronym layout:  "DEGTOAL!" (8 chars) + 14-char body + 2-digit suffix
            // For an even suffix we expect (suffix - 1) padded to 2 chars.
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{DeviceAcronym}",      "DEGTOAL!ABCDEFGHIJKLMN04" }, // 8 + 14 + 2 = 24, body=ABCDEFGHIJKLMN, suffix=04
                { "{SignalType.Source}",  "Phasor"                   },
                { "{SignalType.Suffix}",  "PA"                       },
                { "{SignalType.Acronym}", "VPHA"                     },
                { "{PhasorLabel}",        "VA"                       },
                { "{Label}",              ""                         },
                { "{BaseKV}",             "500"                      },
                { "{SignalIndex}",        "0"                        }
            };

            // body = "ABCDEFGHIJKLMN", suffix=04 (even) -> 03; .Aeval{...DEGTOAL? 'R':''}=AR; trailing '.B'
            Assert.AreEqual("ABCDEFGHIJKLMN03.VA.AR.B", Execute(PointTagTemplate4, subs));
        }

        [TestMethod]
        public void PointTagTemplate4_DegtoalPrefix_OddIndex()
        {
            Dictionary<string, string> subs = new Dictionary<string, string>
            {
                { "{DeviceAcronym}",      "DEGTOAL!ABCDEFGHIJKLMN05" },
                { "{SignalType.Source}",  "Phasor"                   },
                { "{SignalType.Suffix}",  "PA"                       },
                { "{SignalType.Acronym}", "VPHA"                     },
                { "{PhasorLabel}",        "VA"                       },
                { "{Label}",              ""                         },
                { "{BaseKV}",             "500"                      },
                { "{SignalIndex}",        "0"                        }
            };

            // suffix=05 (odd) -> kept as 05; trailing '.A'
            Assert.AreEqual("ABCDEFGHIJKLMN05.VA.AR.A", Execute(PointTagTemplate4, subs));
        }

        #endregion
    }
}
