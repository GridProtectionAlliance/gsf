#******************************************************************************************************
#  CMakeLists.txt - Gbtc
#
#  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
#
#  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
#  the NOTICE file distributed with this work for additional information regarding copyright ownership.
#  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
#  not use this file except in compliance with the License. You may obtain a copy of the License at:
#
#      http://opensource.org/licenses/MIT
#
#  Unless agreed to in writing, the subject software distributed under the License is distributed on an
#  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
#  License for the specific language governing permissions and limitations.
#
#  Code Modification History:
#  ----------------------------------------------------------------------------------------------------
#  04/09/2012 - Stephen C. Wills
#       Generated original version of source code.
#
#  02/19/2019 - J. Ritchie Carroll
#       Updated to include publisher code.
#
#******************************************************************************************************

cmake_minimum_required (VERSION 2.8)
project (TimeSeriesPlatformLibrary)

set (HEADER_OUTPUT_DIRECTORY Include/GSF)
set (CMAKE_RUNTIME_OUTPUT_DIRECTORY Output)
set (CMAKE_ARCHIVE_OUTPUT_DIRECTORY Libraries)
set (CMAKE_LIBRARY_OUTPUT_DIRECTORY Libraries)

set (GSF_HEADER_FILES
    "Common/CommonTypes.h"
    "Common/Convert.h"
    "Common/EndianConverter.h"
    "Common/Nullable.h"
    "Common/pugixml.hpp"
    "Common/ThreadSafeQueue.h"
    "Common/Timer.h"
    "Data/DataColumn.h"
    "Data/DataRow.h"
    "Data/DataSet.h"
    "Data/DataTable.h"
    "FilterExpressions/ExpressionTree.h"
    "FilterExpressions/FilterExpressionParser.h"
    "FilterExpressions/FilterExpressionSyntaxBaseListener.h"
    "FilterExpressions/FilterExpressionSyntaxLexer.h"
    "FilterExpressions/FilterExpressionSyntaxListener.h"
    "FilterExpressions/FilterExpressionSyntaxParser.h"
    "FilterExpressions/antlr4-runtime/ANTLRErrorListener.h"
    "FilterExpressions/antlr4-runtime/ANTLRErrorStrategy.h"
    "FilterExpressions/antlr4-runtime/ANTLRFileStream.h"
    "FilterExpressions/antlr4-runtime/ANTLRInputStream.h"
    "FilterExpressions/antlr4-runtime/BailErrorStrategy.h"
    "FilterExpressions/antlr4-runtime/BaseErrorListener.h"
    "FilterExpressions/antlr4-runtime/BufferedTokenStream.h"
    "FilterExpressions/antlr4-runtime/CharStream.h"
    "FilterExpressions/antlr4-runtime/CommonToken.h"
    "FilterExpressions/antlr4-runtime/CommonTokenFactory.h"
    "FilterExpressions/antlr4-runtime/CommonTokenStream.h"
    "FilterExpressions/antlr4-runtime/ConsoleErrorListener.h"
    "FilterExpressions/antlr4-runtime/DefaultErrorStrategy.h"
    "FilterExpressions/antlr4-runtime/DiagnosticErrorListener.h"
    "FilterExpressions/antlr4-runtime/Exceptions.h"
    "FilterExpressions/antlr4-runtime/FailedPredicateException.h"
    "FilterExpressions/antlr4-runtime/InputMismatchException.h"
    "FilterExpressions/antlr4-runtime/IntStream.h"
    "FilterExpressions/antlr4-runtime/InterpreterRuleContext.h"
    "FilterExpressions/antlr4-runtime/Lexer.h"
    "FilterExpressions/antlr4-runtime/LexerInterpreter.h"
    "FilterExpressions/antlr4-runtime/LexerNoViableAltException.h"
    "FilterExpressions/antlr4-runtime/ListTokenSource.h"
    "FilterExpressions/antlr4-runtime/NoViableAltException.h"
    "FilterExpressions/antlr4-runtime/Parser.h"
    "FilterExpressions/antlr4-runtime/ParserInterpreter.h"
    "FilterExpressions/antlr4-runtime/ParserRuleContext.h"
    "FilterExpressions/antlr4-runtime/ProxyErrorListener.h"
    "FilterExpressions/antlr4-runtime/RecognitionException.h"
    "FilterExpressions/antlr4-runtime/Recognizer.h"
    "FilterExpressions/antlr4-runtime/RuleContext.h"
    "FilterExpressions/antlr4-runtime/RuleContextWithAltNum.h"
    "FilterExpressions/antlr4-runtime/RuntimeMetaData.h"
    "FilterExpressions/antlr4-runtime/Token.h"
    "FilterExpressions/antlr4-runtime/TokenSource.h"
    "FilterExpressions/antlr4-runtime/TokenStream.h"
    "FilterExpressions/antlr4-runtime/TokenStreamRewriter.h"
    "FilterExpressions/antlr4-runtime/UnbufferedCharStream.h"
    "FilterExpressions/antlr4-runtime/UnbufferedTokenStream.h"
    "FilterExpressions/antlr4-runtime/Vocabulary.h"
    "FilterExpressions/antlr4-runtime/WritableToken.h"
    "FilterExpressions/antlr4-runtime/atn/ATN.h"
    "FilterExpressions/antlr4-runtime/atn/ATNConfig.h"
    "FilterExpressions/antlr4-runtime/atn/ATNConfigSet.h"
    "FilterExpressions/antlr4-runtime/atn/ATNDeserializationOptions.h"
    "FilterExpressions/antlr4-runtime/atn/ATNDeserializer.h"
    "FilterExpressions/antlr4-runtime/atn/ATNSerializer.h"
    "FilterExpressions/antlr4-runtime/atn/ATNSimulator.h"
    "FilterExpressions/antlr4-runtime/atn/ATNState.h"
    "FilterExpressions/antlr4-runtime/atn/AbstractPredicateTransition.h"
    "FilterExpressions/antlr4-runtime/atn/ActionTransition.h"
    "FilterExpressions/antlr4-runtime/atn/AmbiguityInfo.h"
    "FilterExpressions/antlr4-runtime/atn/ArrayPredictionContext.h"
    "FilterExpressions/antlr4-runtime/atn/AtomTransition.h"
    "FilterExpressions/antlr4-runtime/atn/BasicBlockStartState.h"
    "FilterExpressions/antlr4-runtime/atn/BasicState.h"
    "FilterExpressions/antlr4-runtime/atn/BlockEndState.h"
    "FilterExpressions/antlr4-runtime/atn/BlockStartState.h"
    "FilterExpressions/antlr4-runtime/atn/ContextSensitivityInfo.h"
    "FilterExpressions/antlr4-runtime/atn/DecisionEventInfo.h"
    "FilterExpressions/antlr4-runtime/atn/DecisionInfo.h"
    "FilterExpressions/antlr4-runtime/atn/DecisionState.h"
    "FilterExpressions/antlr4-runtime/atn/EmptyPredictionContext.h"
    "FilterExpressions/antlr4-runtime/atn/EpsilonTransition.h"
    "FilterExpressions/antlr4-runtime/atn/ErrorInfo.h"
    "FilterExpressions/antlr4-runtime/atn/LL1Analyzer.h"
    "FilterExpressions/antlr4-runtime/atn/LexerATNConfig.h"
    "FilterExpressions/antlr4-runtime/atn/LexerATNSimulator.h"
    "FilterExpressions/antlr4-runtime/atn/LexerAction.h"
    "FilterExpressions/antlr4-runtime/atn/LexerActionExecutor.h"
    "FilterExpressions/antlr4-runtime/atn/LexerChannelAction.h"
    "FilterExpressions/antlr4-runtime/atn/LexerCustomAction.h"
    "FilterExpressions/antlr4-runtime/atn/LexerIndexedCustomAction.h"
    "FilterExpressions/antlr4-runtime/atn/LexerModeAction.h"
    "FilterExpressions/antlr4-runtime/atn/LexerMoreAction.h"
    "FilterExpressions/antlr4-runtime/atn/LexerPopModeAction.h"
    "FilterExpressions/antlr4-runtime/atn/LexerPushModeAction.h"
    "FilterExpressions/antlr4-runtime/atn/LexerSkipAction.h"
    "FilterExpressions/antlr4-runtime/atn/LexerTypeAction.h"
    "FilterExpressions/antlr4-runtime/atn/LookaheadEventInfo.h"
    "FilterExpressions/antlr4-runtime/atn/LoopEndState.h"
    "FilterExpressions/antlr4-runtime/atn/NotSetTransition.h"
    "FilterExpressions/antlr4-runtime/atn/OrderedATNConfigSet.h"
    "FilterExpressions/antlr4-runtime/atn/ParseInfo.h"
    "FilterExpressions/antlr4-runtime/atn/ParserATNSimulator.h"
    "FilterExpressions/antlr4-runtime/atn/PlusBlockStartState.h"
    "FilterExpressions/antlr4-runtime/atn/PlusLoopbackState.h"
    "FilterExpressions/antlr4-runtime/atn/PrecedencePredicateTransition.h"
    "FilterExpressions/antlr4-runtime/atn/PredicateEvalInfo.h"
    "FilterExpressions/antlr4-runtime/atn/PredicateTransition.h"
    "FilterExpressions/antlr4-runtime/atn/PredictionContext.h"
    "FilterExpressions/antlr4-runtime/atn/PredictionMode.h"
    "FilterExpressions/antlr4-runtime/atn/ProfilingATNSimulator.h"
    "FilterExpressions/antlr4-runtime/atn/RangeTransition.h"
    "FilterExpressions/antlr4-runtime/atn/RuleStartState.h"
    "FilterExpressions/antlr4-runtime/atn/RuleStopState.h"
    "FilterExpressions/antlr4-runtime/atn/RuleTransition.h"
    "FilterExpressions/antlr4-runtime/atn/SemanticContext.h"
    "FilterExpressions/antlr4-runtime/atn/SetTransition.h"
    "FilterExpressions/antlr4-runtime/atn/SingletonPredictionContext.h"
    "FilterExpressions/antlr4-runtime/atn/StarBlockStartState.h"
    "FilterExpressions/antlr4-runtime/atn/StarLoopEntryState.h"
    "FilterExpressions/antlr4-runtime/atn/StarLoopbackState.h"
    "FilterExpressions/antlr4-runtime/atn/TokensStartState.h"
    "FilterExpressions/antlr4-runtime/atn/Transition.h"
    "FilterExpressions/antlr4-runtime/atn/WildcardTransition.h"
    "FilterExpressions/antlr4-runtime/dfa/DFA.h"
    "FilterExpressions/antlr4-runtime/dfa/DFASerializer.h"
    "FilterExpressions/antlr4-runtime/dfa/DFAState.h"
    "FilterExpressions/antlr4-runtime/dfa/LexerDFASerializer.h"
    "FilterExpressions/antlr4-runtime/misc/InterpreterDataReader.h"
    "FilterExpressions/antlr4-runtime/misc/Interval.h"
    "FilterExpressions/antlr4-runtime/misc/IntervalSet.h"
    "FilterExpressions/antlr4-runtime/misc/MurmurHash.h"
    "FilterExpressions/antlr4-runtime/misc/Predicate.h"
    "FilterExpressions/antlr4-runtime/support/Any.h"
    "FilterExpressions/antlr4-runtime/support/Arrays.h"
    "FilterExpressions/antlr4-runtime/support/CPPUtils.h"
    "FilterExpressions/antlr4-runtime/support/StringUtils.h"
    "FilterExpressions/antlr4-runtime/support/guid.h"
    "FilterExpressions/antlr4-runtime/tree/ErrorNode.h"
    "FilterExpressions/antlr4-runtime/tree/ErrorNodeImpl.h"
    "FilterExpressions/antlr4-runtime/tree/IterativeParseTreeWalker.h"
    "FilterExpressions/antlr4-runtime/tree/ParseTree.h"
    "FilterExpressions/antlr4-runtime/tree/ParseTreeListener.h"
    "FilterExpressions/antlr4-runtime/tree/ParseTreeVisitor.h"
    "FilterExpressions/antlr4-runtime/tree/ParseTreeWalker.h"
    "FilterExpressions/antlr4-runtime/tree/TerminalNode.h"
    "FilterExpressions/antlr4-runtime/tree/TerminalNodeImpl.h"
    "FilterExpressions/antlr4-runtime/tree/Trees.h"
    "FilterExpressions/antlr4-runtime/tree/pattern/Chunk.h"
    "FilterExpressions/antlr4-runtime/tree/pattern/ParseTreeMatch.h"
    "FilterExpressions/antlr4-runtime/tree/pattern/ParseTreePattern.h"
    "FilterExpressions/antlr4-runtime/tree/pattern/ParseTreePatternMatcher.h"
    "FilterExpressions/antlr4-runtime/tree/pattern/RuleTagToken.h"
    "FilterExpressions/antlr4-runtime/tree/pattern/TagChunk.h"
    "FilterExpressions/antlr4-runtime/tree/pattern/TextChunk.h"
    "FilterExpressions/antlr4-runtime/tree/pattern/TokenTagToken.h"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPath.h"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathElement.h"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathLexer.h"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathLexerErrorListener.h"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathRuleAnywhereElement.h"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathRuleElement.h"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathTokenAnywhereElement.h"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathTokenElement.h"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathWildcardAnywhereElement.h"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathWildcardElement.h"
    "Transport/ActiveMeasurementsSchema.h"
    "Transport/CompactMeasurement.h"
    "Transport/Constants.h"
    "Transport/DataPublisher.h"
    "Transport/DataSubscriber.h"
    "Transport/MetadataSchema.h"
    "Transport/PublisherInstance.h"
    "Transport/SignalIndexCache.h"
    "Transport/SubscriberConnection.h"
    "Transport/SubscriberInstance.h"
    "Transport/TransportTypes.h"
    "Transport/TSSCMeasurementParser.h"
    "Transport/Version.h"
)

set (GSF_SOURCE_FILES
    "Common/CommonTypes.cpp"
    "Common/Convert.cpp"
    "Common/EndianConverter.cpp"
    "Common/pugixml.cpp"
    "Data/DataColumn.cpp"
    "Data/DataRow.cpp"
    "Data/DataSet.cpp"
    "Data/DataTable.cpp"
    "FilterExpressions/ExpressionTree.cpp"
    "FilterExpressions/FilterExpressionParser.cpp"
    "FilterExpressions/FilterExpressionSyntaxBaseListener.cpp"
    "FilterExpressions/FilterExpressionSyntaxLexer.cpp"
    "FilterExpressions/FilterExpressionSyntaxListener.cpp"
    "FilterExpressions/FilterExpressionSyntaxParser.cpp"
    "FilterExpressions/antlr4-runtime/ANTLRErrorListener.cpp"
    "FilterExpressions/antlr4-runtime/ANTLRErrorStrategy.cpp"
    "FilterExpressions/antlr4-runtime/ANTLRFileStream.cpp"
    "FilterExpressions/antlr4-runtime/ANTLRInputStream.cpp"
    "FilterExpressions/antlr4-runtime/BailErrorStrategy.cpp"
    "FilterExpressions/antlr4-runtime/BaseErrorListener.cpp"
    "FilterExpressions/antlr4-runtime/BufferedTokenStream.cpp"
    "FilterExpressions/antlr4-runtime/CharStream.cpp"
    "FilterExpressions/antlr4-runtime/CommonToken.cpp"
    "FilterExpressions/antlr4-runtime/CommonTokenFactory.cpp"
    "FilterExpressions/antlr4-runtime/CommonTokenStream.cpp"
    "FilterExpressions/antlr4-runtime/ConsoleErrorListener.cpp"
    "FilterExpressions/antlr4-runtime/DefaultErrorStrategy.cpp"
    "FilterExpressions/antlr4-runtime/DiagnosticErrorListener.cpp"
    "FilterExpressions/antlr4-runtime/Exceptions.cpp"
    "FilterExpressions/antlr4-runtime/FailedPredicateException.cpp"
    "FilterExpressions/antlr4-runtime/InputMismatchException.cpp"
    "FilterExpressions/antlr4-runtime/IntStream.cpp"
    "FilterExpressions/antlr4-runtime/InterpreterRuleContext.cpp"
    "FilterExpressions/antlr4-runtime/Lexer.cpp"
    "FilterExpressions/antlr4-runtime/LexerInterpreter.cpp"
    "FilterExpressions/antlr4-runtime/LexerNoViableAltException.cpp"
    "FilterExpressions/antlr4-runtime/ListTokenSource.cpp"
    "FilterExpressions/antlr4-runtime/NoViableAltException.cpp"
    "FilterExpressions/antlr4-runtime/Parser.cpp"
    "FilterExpressions/antlr4-runtime/ParserInterpreter.cpp"
    "FilterExpressions/antlr4-runtime/ParserRuleContext.cpp"
    "FilterExpressions/antlr4-runtime/ProxyErrorListener.cpp"
    "FilterExpressions/antlr4-runtime/RecognitionException.cpp"
    "FilterExpressions/antlr4-runtime/Recognizer.cpp"
    "FilterExpressions/antlr4-runtime/RuleContext.cpp"
    "FilterExpressions/antlr4-runtime/RuleContextWithAltNum.cpp"
    "FilterExpressions/antlr4-runtime/RuntimeMetaData.cpp"
    "FilterExpressions/antlr4-runtime/Token.cpp"
    "FilterExpressions/antlr4-runtime/TokenSource.cpp"
    "FilterExpressions/antlr4-runtime/TokenStream.cpp"
    "FilterExpressions/antlr4-runtime/TokenStreamRewriter.cpp"
    "FilterExpressions/antlr4-runtime/UnbufferedCharStream.cpp"
    "FilterExpressions/antlr4-runtime/UnbufferedTokenStream.cpp"
    "FilterExpressions/antlr4-runtime/Vocabulary.cpp"
    "FilterExpressions/antlr4-runtime/WritableToken.cpp"
    "FilterExpressions/antlr4-runtime/atn/ATN.cpp"
    "FilterExpressions/antlr4-runtime/atn/ATNConfig.cpp"
    "FilterExpressions/antlr4-runtime/atn/ATNConfigSet.cpp"
    "FilterExpressions/antlr4-runtime/atn/ATNDeserializationOptions.cpp"
    "FilterExpressions/antlr4-runtime/atn/ATNDeserializer.cpp"
    "FilterExpressions/antlr4-runtime/atn/ATNSerializer.cpp"
    "FilterExpressions/antlr4-runtime/atn/ATNSimulator.cpp"
    "FilterExpressions/antlr4-runtime/atn/ATNState.cpp"
    "FilterExpressions/antlr4-runtime/atn/AbstractPredicateTransition.cpp"
    "FilterExpressions/antlr4-runtime/atn/ActionTransition.cpp"
    "FilterExpressions/antlr4-runtime/atn/AmbiguityInfo.cpp"
    "FilterExpressions/antlr4-runtime/atn/ArrayPredictionContext.cpp"
    "FilterExpressions/antlr4-runtime/atn/AtomTransition.cpp"
    "FilterExpressions/antlr4-runtime/atn/BasicBlockStartState.cpp"
    "FilterExpressions/antlr4-runtime/atn/BasicState.cpp"
    "FilterExpressions/antlr4-runtime/atn/BlockEndState.cpp"
    "FilterExpressions/antlr4-runtime/atn/BlockStartState.cpp"
    "FilterExpressions/antlr4-runtime/atn/ContextSensitivityInfo.cpp"
    "FilterExpressions/antlr4-runtime/atn/DecisionEventInfo.cpp"
    "FilterExpressions/antlr4-runtime/atn/DecisionInfo.cpp"
    "FilterExpressions/antlr4-runtime/atn/DecisionState.cpp"
    "FilterExpressions/antlr4-runtime/atn/EmptyPredictionContext.cpp"
    "FilterExpressions/antlr4-runtime/atn/EpsilonTransition.cpp"
    "FilterExpressions/antlr4-runtime/atn/ErrorInfo.cpp"
    "FilterExpressions/antlr4-runtime/atn/LL1Analyzer.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerATNConfig.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerATNSimulator.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerAction.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerActionExecutor.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerChannelAction.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerCustomAction.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerIndexedCustomAction.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerModeAction.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerMoreAction.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerPopModeAction.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerPushModeAction.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerSkipAction.cpp"
    "FilterExpressions/antlr4-runtime/atn/LexerTypeAction.cpp"
    "FilterExpressions/antlr4-runtime/atn/LookaheadEventInfo.cpp"
    "FilterExpressions/antlr4-runtime/atn/LoopEndState.cpp"
    "FilterExpressions/antlr4-runtime/atn/NotSetTransition.cpp"
    "FilterExpressions/antlr4-runtime/atn/OrderedATNConfigSet.cpp"
    "FilterExpressions/antlr4-runtime/atn/ParseInfo.cpp"
    "FilterExpressions/antlr4-runtime/atn/ParserATNSimulator.cpp"
    "FilterExpressions/antlr4-runtime/atn/PlusBlockStartState.cpp"
    "FilterExpressions/antlr4-runtime/atn/PlusLoopbackState.cpp"
    "FilterExpressions/antlr4-runtime/atn/PrecedencePredicateTransition.cpp"
    "FilterExpressions/antlr4-runtime/atn/PredicateEvalInfo.cpp"
    "FilterExpressions/antlr4-runtime/atn/PredicateTransition.cpp"
    "FilterExpressions/antlr4-runtime/atn/PredictionContext.cpp"
    "FilterExpressions/antlr4-runtime/atn/PredictionMode.cpp"
    "FilterExpressions/antlr4-runtime/atn/ProfilingATNSimulator.cpp"
    "FilterExpressions/antlr4-runtime/atn/RangeTransition.cpp"
    "FilterExpressions/antlr4-runtime/atn/RuleStartState.cpp"
    "FilterExpressions/antlr4-runtime/atn/RuleStopState.cpp"
    "FilterExpressions/antlr4-runtime/atn/RuleTransition.cpp"
    "FilterExpressions/antlr4-runtime/atn/SemanticContext.cpp"
    "FilterExpressions/antlr4-runtime/atn/SetTransition.cpp"
    "FilterExpressions/antlr4-runtime/atn/SingletonPredictionContext.cpp"
    "FilterExpressions/antlr4-runtime/atn/StarBlockStartState.cpp"
    "FilterExpressions/antlr4-runtime/atn/StarLoopEntryState.cpp"
    "FilterExpressions/antlr4-runtime/atn/StarLoopbackState.cpp"
    "FilterExpressions/antlr4-runtime/atn/TokensStartState.cpp"
    "FilterExpressions/antlr4-runtime/atn/Transition.cpp"
    "FilterExpressions/antlr4-runtime/atn/WildcardTransition.cpp"
    "FilterExpressions/antlr4-runtime/dfa/DFA.cpp"
    "FilterExpressions/antlr4-runtime/dfa/DFASerializer.cpp"
    "FilterExpressions/antlr4-runtime/dfa/DFAState.cpp"
    "FilterExpressions/antlr4-runtime/dfa/LexerDFASerializer.cpp"
    "FilterExpressions/antlr4-runtime/misc/InterpreterDataReader.cpp"
    "FilterExpressions/antlr4-runtime/misc/Interval.cpp"
    "FilterExpressions/antlr4-runtime/misc/IntervalSet.cpp"
    "FilterExpressions/antlr4-runtime/misc/MurmurHash.cpp"
    "FilterExpressions/antlr4-runtime/misc/Predicate.cpp"
    "FilterExpressions/antlr4-runtime/support/Any.cpp"
    "FilterExpressions/antlr4-runtime/support/Arrays.cpp"
    "FilterExpressions/antlr4-runtime/support/CPPUtils.cpp"
    "FilterExpressions/antlr4-runtime/support/StringUtils.cpp"
    "FilterExpressions/antlr4-runtime/support/guid.cpp"
    "FilterExpressions/antlr4-runtime/tree/ErrorNode.cpp"
    "FilterExpressions/antlr4-runtime/tree/ErrorNodeImpl.cpp"
    "FilterExpressions/antlr4-runtime/tree/IterativeParseTreeWalker.cpp"
    "FilterExpressions/antlr4-runtime/tree/ParseTree.cpp"
    "FilterExpressions/antlr4-runtime/tree/ParseTreeListener.cpp"
    "FilterExpressions/antlr4-runtime/tree/ParseTreeVisitor.cpp"
    "FilterExpressions/antlr4-runtime/tree/ParseTreeWalker.cpp"
    "FilterExpressions/antlr4-runtime/tree/TerminalNode.cpp"
    "FilterExpressions/antlr4-runtime/tree/TerminalNodeImpl.cpp"
    "FilterExpressions/antlr4-runtime/tree/Trees.cpp"
    "FilterExpressions/antlr4-runtime/tree/pattern/Chunk.cpp"
    "FilterExpressions/antlr4-runtime/tree/pattern/ParseTreeMatch.cpp"
    "FilterExpressions/antlr4-runtime/tree/pattern/ParseTreePattern.cpp"
    "FilterExpressions/antlr4-runtime/tree/pattern/ParseTreePatternMatcher.cpp"
    "FilterExpressions/antlr4-runtime/tree/pattern/RuleTagToken.cpp"
    "FilterExpressions/antlr4-runtime/tree/pattern/TagChunk.cpp"
    "FilterExpressions/antlr4-runtime/tree/pattern/TextChunk.cpp"
    "FilterExpressions/antlr4-runtime/tree/pattern/TokenTagToken.cpp"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPath.cpp"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathElement.cpp"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathLexer.cpp"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathLexerErrorListener.cpp"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathRuleAnywhereElement.cpp"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathRuleElement.cpp"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathTokenAnywhereElement.cpp"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathTokenElement.cpp"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathWildcardAnywhereElement.cpp"
    "FilterExpressions/antlr4-runtime/tree/xpath/XPathWildcardElement.cpp"
    "Transport/ActiveMeasurementsSchema.cpp"
    "Transport/CompactMeasurement.cpp"
    "Transport/Constants.cpp"
    "Transport/DataPublisher.cpp"
    "Transport/DataSubscriber.cpp"
    "Transport/MetadataSchema.cpp"
    "Transport/PublisherInstance.cpp"
    "Transport/SignalIndexCache.cpp"
    "Transport/SubscriberConnection.cpp"
    "Transport/SubscriberInstance.cpp"
    "Transport/TransportTypes.cpp"
    "Transport/TSSCMeasurementParser.cpp"
)

# Option to choose whether to build static or shared libraries
option (BUILD_SHARED_LIBS "Build gsf using shared libraries" OFF)

# Copy header files
foreach (headerFile ${GSF_HEADER_FILES})
    string (REGEX MATCH "(.*)[/\\]" DIR ${headerFile})
    file (COPY ${headerFile} DESTINATION ${HEADER_OUTPUT_DIRECTORY}/${DIR})
endforeach (headerFile)

# Define include directories
include_directories(
    "FilterExpressions/antlr4-runtime"
)

# Define macros
add_definitions(
   -DBOOST_CONFIG_SUPPRESS_OUTDATED_MESSAGE 
   -DBOOST_ASIO_ENABLE_CANCELIO 
   -DANTLR4CPP_STATIC 
)

# Define compile flags
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++17")
if ("${CMAKE_CXX_COMPILER_ID}" STREQUAL "Clang")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -stdlib=libc++")
endif()

set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} -Wall")

# Build gsf library
add_library (gsf ${GSF_SOURCE_FILES})
target_link_libraries (gsf pthread m z bz2 boost_system boost_thread boost_date_time boost_iostreams)

# Install headers and library
install (DIRECTORY ${PROJECT_BINARY_DIR}/${HEADER_OUTPUT_DIRECTORY} DESTINATION include)
install (TARGETS gsf DESTINATION lib)

######################
# SAMPLE EXECUTABLES #
######################

set (CMAKE_RUNTIME_OUTPUT_DIRECTORY Samples)

# Copy sample content files
file (COPY "Samples/History.xml" DESTINATION ${CMAKE_RUNTIME_OUTPUT_DIRECTORY})
file (COPY "Samples/Metadata.xml" DESTINATION ${CMAKE_RUNTIME_OUTPUT_DIRECTORY})
file (COPY "Samples/MetadataSample1.xml" DESTINATION ${CMAKE_RUNTIME_OUTPUT_DIRECTORY})
file (COPY "Samples/MetadataSample2.xml" DESTINATION ${CMAKE_RUNTIME_OUTPUT_DIRECTORY})

# SimpleSubscribe sample
add_executable (SimpleSubscribe EXCLUDE_FROM_ALL Samples/SimpleSubscribe.cpp)
target_link_libraries (SimpleSubscribe gsf)

# AdvancedSubscribe sample
add_executable (AdvancedSubscribe EXCLUDE_FROM_ALL Samples/AdvancedSubscribe.cpp)
target_link_libraries (AdvancedSubscribe gsf)

# AverageFrequencyCalculator sample
add_executable (AverageFrequencyCalculator EXCLUDE_FROM_ALL Samples/AverageFrequencyCalculator.cpp)
target_link_libraries (AverageFrequencyCalculator gsf)

# InstanceSubscribe sample
add_executable (InstanceSubscribe EXCLUDE_FROM_ALL Samples/InstanceSubscribe.cpp Samples/SubscriberHandler.cpp)
target_link_libraries (InstanceSubscribe gsf)

# FilterExpressionTests sample
add_executable (FilterExpressionTests EXCLUDE_FROM_ALL Samples/FilterExpressionTests.cpp)
target_link_libraries (FilterExpressionTests gsf boost_filesystem)

# SimplePublish sample
add_executable (SimplePublish EXCLUDE_FROM_ALL Samples/SimplePublish.cpp)
target_link_libraries (SimplePublish gsf)

# AdvancedPublish sample
add_executable (AdvancedPublish EXCLUDE_FROM_ALL Samples/AdvancedPublish.cpp Samples/GenHistory.cpp Samples/TemporalSubscriber.cpp)
target_link_libraries (AdvancedPublish gsf)

# Build with 'make samples'
add_custom_target (samples DEPENDS 
    SimpleSubscribe
    AdvancedSubscribe
    AverageFrequencyCalculator
    InstanceSubscribe
    FilterExpressionTests
    SimplePublish
    AdvancedPublish
)
