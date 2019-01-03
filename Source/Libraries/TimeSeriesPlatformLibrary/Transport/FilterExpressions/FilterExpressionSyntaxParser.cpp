
// Generated from FilterExpressionSyntax.g4 by ANTLR 4.7.1


#include "FilterExpressionSyntaxListener.h"

#include "FilterExpressionSyntaxParser.h"


using namespace antlrcpp;
using namespace antlr4;

FilterExpressionSyntaxParser::FilterExpressionSyntaxParser(TokenStream *input) : Parser(input) {
  _interpreter = new atn::ParserATNSimulator(this, _atn, _decisionToDFA, _sharedContextCache);
}

FilterExpressionSyntaxParser::~FilterExpressionSyntaxParser() {
  delete _interpreter;
}

std::string FilterExpressionSyntaxParser::getGrammarFileName() const {
  return "FilterExpressionSyntax.g4";
}

const std::vector<std::string>& FilterExpressionSyntaxParser::getRuleNames() const {
  return _ruleNames;
}

dfa::Vocabulary& FilterExpressionSyntaxParser::getVocabulary() const {
  return _vocabulary;
}


//----------------- ParseContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::ParseContext::ParseContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::ParseContext::EOF() {
  return getToken(FilterExpressionSyntaxParser::EOF, 0);
}

FilterExpressionSyntaxParser::FilterExpressionStatementListContext* FilterExpressionSyntaxParser::ParseContext::filterExpressionStatementList() {
  return getRuleContext<FilterExpressionSyntaxParser::FilterExpressionStatementListContext>(0);
}

FilterExpressionSyntaxParser::ErrorContext* FilterExpressionSyntaxParser::ParseContext::error() {
  return getRuleContext<FilterExpressionSyntaxParser::ErrorContext>(0);
}


size_t FilterExpressionSyntaxParser::ParseContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleParse;
}

void FilterExpressionSyntaxParser::ParseContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterParse(this);
}

void FilterExpressionSyntaxParser::ParseContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitParse(this);
}

FilterExpressionSyntaxParser::ParseContext* FilterExpressionSyntaxParser::parse() {
  ParseContext *_localctx = _tracker.createInstance<ParseContext>(_ctx, getState());
  enterRule(_localctx, 0, FilterExpressionSyntaxParser::RuleParse);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(30);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case FilterExpressionSyntaxParser::T__0:
      case FilterExpressionSyntaxParser::K_FILTER:
      case FilterExpressionSyntaxParser::GUID_LITERAL:
      case FilterExpressionSyntaxParser::MEASUREMENT_KEY_LITERAL:
      case FilterExpressionSyntaxParser::POINT_TAG_LITERAL: {
        setState(28);
        filterExpressionStatementList();
        break;
      }

      case FilterExpressionSyntaxParser::UNEXPECTED_CHAR: {
        setState(29);
        error();
        break;
      }

    default:
      throw NoViableAltException(this);
    }
    setState(32);
    match(FilterExpressionSyntaxParser::EOF);
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- ErrorContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::ErrorContext::ErrorContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::ErrorContext::UNEXPECTED_CHAR() {
  return getToken(FilterExpressionSyntaxParser::UNEXPECTED_CHAR, 0);
}


size_t FilterExpressionSyntaxParser::ErrorContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleError;
}

void FilterExpressionSyntaxParser::ErrorContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterError(this);
}

void FilterExpressionSyntaxParser::ErrorContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitError(this);
}

FilterExpressionSyntaxParser::ErrorContext* FilterExpressionSyntaxParser::error() {
  ErrorContext *_localctx = _tracker.createInstance<ErrorContext>(_ctx, getState());
  enterRule(_localctx, 2, FilterExpressionSyntaxParser::RuleError);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(34);
    dynamic_cast<ErrorContext *>(_localctx)->unexpected_charToken = match(FilterExpressionSyntaxParser::UNEXPECTED_CHAR);
     
         throw RuntimeException("Unexpected character: " + (dynamic_cast<ErrorContext *>(_localctx)->unexpected_charToken != nullptr ? dynamic_cast<ErrorContext *>(_localctx)->unexpected_charToken->getText() : "")); 
       
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- FilterExpressionStatementListContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::FilterExpressionStatementListContext::FilterExpressionStatementListContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

std::vector<FilterExpressionSyntaxParser::FilterExpressionStatementContext *> FilterExpressionSyntaxParser::FilterExpressionStatementListContext::filterExpressionStatement() {
  return getRuleContexts<FilterExpressionSyntaxParser::FilterExpressionStatementContext>();
}

FilterExpressionSyntaxParser::FilterExpressionStatementContext* FilterExpressionSyntaxParser::FilterExpressionStatementListContext::filterExpressionStatement(size_t i) {
  return getRuleContext<FilterExpressionSyntaxParser::FilterExpressionStatementContext>(i);
}


size_t FilterExpressionSyntaxParser::FilterExpressionStatementListContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleFilterExpressionStatementList;
}

void FilterExpressionSyntaxParser::FilterExpressionStatementListContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterFilterExpressionStatementList(this);
}

void FilterExpressionSyntaxParser::FilterExpressionStatementListContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitFilterExpressionStatementList(this);
}

FilterExpressionSyntaxParser::FilterExpressionStatementListContext* FilterExpressionSyntaxParser::filterExpressionStatementList() {
  FilterExpressionStatementListContext *_localctx = _tracker.createInstance<FilterExpressionStatementListContext>(_ctx, getState());
  enterRule(_localctx, 4, FilterExpressionSyntaxParser::RuleFilterExpressionStatementList);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    size_t alt;
    enterOuterAlt(_localctx, 1);
    setState(40);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while (_la == FilterExpressionSyntaxParser::T__0) {
      setState(37);
      match(FilterExpressionSyntaxParser::T__0);
      setState(42);
      _errHandler->sync(this);
      _la = _input->LA(1);
    }
    setState(43);
    filterExpressionStatement();
    setState(52);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 3, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        setState(45); 
        _errHandler->sync(this);
        _la = _input->LA(1);
        do {
          setState(44);
          match(FilterExpressionSyntaxParser::T__0);
          setState(47); 
          _errHandler->sync(this);
          _la = _input->LA(1);
        } while (_la == FilterExpressionSyntaxParser::T__0);
        setState(49);
        filterExpressionStatement(); 
      }
      setState(54);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 3, _ctx);
    }
    setState(58);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while (_la == FilterExpressionSyntaxParser::T__0) {
      setState(55);
      match(FilterExpressionSyntaxParser::T__0);
      setState(60);
      _errHandler->sync(this);
      _la = _input->LA(1);
    }
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- FilterExpressionStatementContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::FilterExpressionStatementContext::FilterExpressionStatementContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

FilterExpressionSyntaxParser::IdentifierStatementContext* FilterExpressionSyntaxParser::FilterExpressionStatementContext::identifierStatement() {
  return getRuleContext<FilterExpressionSyntaxParser::IdentifierStatementContext>(0);
}

FilterExpressionSyntaxParser::FilterStatementContext* FilterExpressionSyntaxParser::FilterExpressionStatementContext::filterStatement() {
  return getRuleContext<FilterExpressionSyntaxParser::FilterStatementContext>(0);
}


size_t FilterExpressionSyntaxParser::FilterExpressionStatementContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleFilterExpressionStatement;
}

void FilterExpressionSyntaxParser::FilterExpressionStatementContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterFilterExpressionStatement(this);
}

void FilterExpressionSyntaxParser::FilterExpressionStatementContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitFilterExpressionStatement(this);
}

FilterExpressionSyntaxParser::FilterExpressionStatementContext* FilterExpressionSyntaxParser::filterExpressionStatement() {
  FilterExpressionStatementContext *_localctx = _tracker.createInstance<FilterExpressionStatementContext>(_ctx, getState());
  enterRule(_localctx, 6, FilterExpressionSyntaxParser::RuleFilterExpressionStatement);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    setState(63);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case FilterExpressionSyntaxParser::GUID_LITERAL:
      case FilterExpressionSyntaxParser::MEASUREMENT_KEY_LITERAL:
      case FilterExpressionSyntaxParser::POINT_TAG_LITERAL: {
        enterOuterAlt(_localctx, 1);
        setState(61);
        identifierStatement();
        break;
      }

      case FilterExpressionSyntaxParser::K_FILTER: {
        enterOuterAlt(_localctx, 2);
        setState(62);
        filterStatement();
        break;
      }

    default:
      throw NoViableAltException(this);
    }
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- IdentifierStatementContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::IdentifierStatementContext::IdentifierStatementContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::IdentifierStatementContext::GUID_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::GUID_LITERAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::IdentifierStatementContext::MEASUREMENT_KEY_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::MEASUREMENT_KEY_LITERAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::IdentifierStatementContext::POINT_TAG_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::POINT_TAG_LITERAL, 0);
}


size_t FilterExpressionSyntaxParser::IdentifierStatementContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleIdentifierStatement;
}

void FilterExpressionSyntaxParser::IdentifierStatementContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterIdentifierStatement(this);
}

void FilterExpressionSyntaxParser::IdentifierStatementContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitIdentifierStatement(this);
}

FilterExpressionSyntaxParser::IdentifierStatementContext* FilterExpressionSyntaxParser::identifierStatement() {
  IdentifierStatementContext *_localctx = _tracker.createInstance<IdentifierStatementContext>(_ctx, getState());
  enterRule(_localctx, 8, FilterExpressionSyntaxParser::RuleIdentifierStatement);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(65);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::GUID_LITERAL)
      | (1ULL << FilterExpressionSyntaxParser::MEASUREMENT_KEY_LITERAL)
      | (1ULL << FilterExpressionSyntaxParser::POINT_TAG_LITERAL))) != 0))) {
    _errHandler->recoverInline(this);
    }
    else {
      _errHandler->reportMatch(this);
      consume();
    }
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- FilterStatementContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::FilterStatementContext::FilterStatementContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::FilterStatementContext::K_FILTER() {
  return getToken(FilterExpressionSyntaxParser::K_FILTER, 0);
}

FilterExpressionSyntaxParser::TableNameContext* FilterExpressionSyntaxParser::FilterStatementContext::tableName() {
  return getRuleContext<FilterExpressionSyntaxParser::TableNameContext>(0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FilterStatementContext::K_WHERE() {
  return getToken(FilterExpressionSyntaxParser::K_WHERE, 0);
}

FilterExpressionSyntaxParser::ExpressionContext* FilterExpressionSyntaxParser::FilterStatementContext::expression() {
  return getRuleContext<FilterExpressionSyntaxParser::ExpressionContext>(0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FilterStatementContext::K_TOP() {
  return getToken(FilterExpressionSyntaxParser::K_TOP, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FilterStatementContext::INTEGER_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::INTEGER_LITERAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FilterStatementContext::K_ORDER() {
  return getToken(FilterExpressionSyntaxParser::K_ORDER, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FilterStatementContext::K_BY() {
  return getToken(FilterExpressionSyntaxParser::K_BY, 0);
}

std::vector<FilterExpressionSyntaxParser::OrderingTermContext *> FilterExpressionSyntaxParser::FilterStatementContext::orderingTerm() {
  return getRuleContexts<FilterExpressionSyntaxParser::OrderingTermContext>();
}

FilterExpressionSyntaxParser::OrderingTermContext* FilterExpressionSyntaxParser::FilterStatementContext::orderingTerm(size_t i) {
  return getRuleContext<FilterExpressionSyntaxParser::OrderingTermContext>(i);
}


size_t FilterExpressionSyntaxParser::FilterStatementContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleFilterStatement;
}

void FilterExpressionSyntaxParser::FilterStatementContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterFilterStatement(this);
}

void FilterExpressionSyntaxParser::FilterStatementContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitFilterStatement(this);
}

FilterExpressionSyntaxParser::FilterStatementContext* FilterExpressionSyntaxParser::filterStatement() {
  FilterStatementContext *_localctx = _tracker.createInstance<FilterStatementContext>(_ctx, getState());
  enterRule(_localctx, 10, FilterExpressionSyntaxParser::RuleFilterStatement);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(67);
    match(FilterExpressionSyntaxParser::K_FILTER);
    setState(70);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_TOP) {
      setState(68);
      match(FilterExpressionSyntaxParser::K_TOP);
      setState(69);
      match(FilterExpressionSyntaxParser::INTEGER_LITERAL);
    }
    setState(72);
    tableName();
    setState(73);
    match(FilterExpressionSyntaxParser::K_WHERE);
    setState(74);
    expression(0);
    setState(85);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_ORDER) {
      setState(75);
      match(FilterExpressionSyntaxParser::K_ORDER);
      setState(76);
      match(FilterExpressionSyntaxParser::K_BY);
      setState(77);
      orderingTerm();
      setState(82);
      _errHandler->sync(this);
      _la = _input->LA(1);
      while (_la == FilterExpressionSyntaxParser::T__1) {
        setState(78);
        match(FilterExpressionSyntaxParser::T__1);
        setState(79);
        orderingTerm();
        setState(84);
        _errHandler->sync(this);
        _la = _input->LA(1);
      }
    }
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- OrderingTermContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::OrderingTermContext::OrderingTermContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

FilterExpressionSyntaxParser::OrderByColumnNameContext* FilterExpressionSyntaxParser::OrderingTermContext::orderByColumnName() {
  return getRuleContext<FilterExpressionSyntaxParser::OrderByColumnNameContext>(0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::OrderingTermContext::K_ASC() {
  return getToken(FilterExpressionSyntaxParser::K_ASC, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::OrderingTermContext::K_DESC() {
  return getToken(FilterExpressionSyntaxParser::K_DESC, 0);
}


size_t FilterExpressionSyntaxParser::OrderingTermContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleOrderingTerm;
}

void FilterExpressionSyntaxParser::OrderingTermContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterOrderingTerm(this);
}

void FilterExpressionSyntaxParser::OrderingTermContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitOrderingTerm(this);
}

FilterExpressionSyntaxParser::OrderingTermContext* FilterExpressionSyntaxParser::orderingTerm() {
  OrderingTermContext *_localctx = _tracker.createInstance<OrderingTermContext>(_ctx, getState());
  enterRule(_localctx, 12, FilterExpressionSyntaxParser::RuleOrderingTerm);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(87);
    orderByColumnName();
    setState(89);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_ASC

    || _la == FilterExpressionSyntaxParser::K_DESC) {
      setState(88);
      _la = _input->LA(1);
      if (!(_la == FilterExpressionSyntaxParser::K_ASC

      || _la == FilterExpressionSyntaxParser::K_DESC)) {
      _errHandler->recoverInline(this);
      }
      else {
        _errHandler->reportMatch(this);
        consume();
      }
    }
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- ExpressionContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::ExpressionContext::ExpressionContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

FilterExpressionSyntaxParser::LiteralValueContext* FilterExpressionSyntaxParser::ExpressionContext::literalValue() {
  return getRuleContext<FilterExpressionSyntaxParser::LiteralValueContext>(0);
}

FilterExpressionSyntaxParser::ColumnNameContext* FilterExpressionSyntaxParser::ExpressionContext::columnName() {
  return getRuleContext<FilterExpressionSyntaxParser::ColumnNameContext>(0);
}

FilterExpressionSyntaxParser::UnaryOperatorContext* FilterExpressionSyntaxParser::ExpressionContext::unaryOperator() {
  return getRuleContext<FilterExpressionSyntaxParser::UnaryOperatorContext>(0);
}

std::vector<FilterExpressionSyntaxParser::ExpressionContext *> FilterExpressionSyntaxParser::ExpressionContext::expression() {
  return getRuleContexts<FilterExpressionSyntaxParser::ExpressionContext>();
}

FilterExpressionSyntaxParser::ExpressionContext* FilterExpressionSyntaxParser::ExpressionContext::expression(size_t i) {
  return getRuleContext<FilterExpressionSyntaxParser::ExpressionContext>(i);
}

FilterExpressionSyntaxParser::FunctionNameContext* FilterExpressionSyntaxParser::ExpressionContext::functionName() {
  return getRuleContext<FilterExpressionSyntaxParser::FunctionNameContext>(0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_LIKE() {
  return getToken(FilterExpressionSyntaxParser::K_LIKE, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_NOT() {
  return getToken(FilterExpressionSyntaxParser::K_NOT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_AND() {
  return getToken(FilterExpressionSyntaxParser::K_AND, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_OR() {
  return getToken(FilterExpressionSyntaxParser::K_OR, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_IS() {
  return getToken(FilterExpressionSyntaxParser::K_IS, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_NULL() {
  return getToken(FilterExpressionSyntaxParser::K_NULL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_IN() {
  return getToken(FilterExpressionSyntaxParser::K_IN, 0);
}


size_t FilterExpressionSyntaxParser::ExpressionContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleExpression;
}

void FilterExpressionSyntaxParser::ExpressionContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterExpression(this);
}

void FilterExpressionSyntaxParser::ExpressionContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitExpression(this);
}


FilterExpressionSyntaxParser::ExpressionContext* FilterExpressionSyntaxParser::expression() {
   return expression(0);
}

FilterExpressionSyntaxParser::ExpressionContext* FilterExpressionSyntaxParser::expression(int precedence) {
  ParserRuleContext *parentContext = _ctx;
  size_t parentState = getState();
  FilterExpressionSyntaxParser::ExpressionContext *_localctx = _tracker.createInstance<ExpressionContext>(_ctx, parentState);
  FilterExpressionSyntaxParser::ExpressionContext *previousContext = _localctx;
  size_t startState = 14;
  enterRecursionRule(_localctx, 14, FilterExpressionSyntaxParser::RuleExpression, precedence);

    size_t _la = 0;

  auto onExit = finally([=] {
    unrollRecursionContexts(parentContext);
  });
  try {
    size_t alt;
    enterOuterAlt(_localctx, 1);
    setState(116);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case FilterExpressionSyntaxParser::K_NULL:
      case FilterExpressionSyntaxParser::BOOLEAN_LITERAL:
      case FilterExpressionSyntaxParser::INTEGER_LITERAL:
      case FilterExpressionSyntaxParser::NUMERIC_LITERAL:
      case FilterExpressionSyntaxParser::GUID_LITERAL:
      case FilterExpressionSyntaxParser::STRING_LITERAL:
      case FilterExpressionSyntaxParser::DATETIME_LITERAL: {
        setState(92);
        literalValue();
        break;
      }

      case FilterExpressionSyntaxParser::IDENTIFIER: {
        setState(93);
        columnName();
        break;
      }

      case FilterExpressionSyntaxParser::T__5:
      case FilterExpressionSyntaxParser::T__6:
      case FilterExpressionSyntaxParser::T__21:
      case FilterExpressionSyntaxParser::K_NOT: {
        setState(94);
        unaryOperator();
        setState(95);
        expression(13);
        break;
      }

      case FilterExpressionSyntaxParser::K_CONVERT:
      case FilterExpressionSyntaxParser::K_COALESCE:
      case FilterExpressionSyntaxParser::K_IIF:
      case FilterExpressionSyntaxParser::K_ISNULL:
      case FilterExpressionSyntaxParser::K_ISREGEXMATCH:
      case FilterExpressionSyntaxParser::K_LEN:
      case FilterExpressionSyntaxParser::K_REGEXVAL:
      case FilterExpressionSyntaxParser::K_SUBSTR:
      case FilterExpressionSyntaxParser::K_SUBSTRING:
      case FilterExpressionSyntaxParser::K_TRIM: {
        setState(97);
        functionName();
        setState(98);
        match(FilterExpressionSyntaxParser::T__19);
        setState(108);
        _errHandler->sync(this);
        switch (_input->LA(1)) {
          case FilterExpressionSyntaxParser::T__5:
          case FilterExpressionSyntaxParser::T__6:
          case FilterExpressionSyntaxParser::T__19:
          case FilterExpressionSyntaxParser::T__21:
          case FilterExpressionSyntaxParser::K_CONVERT:
          case FilterExpressionSyntaxParser::K_COALESCE:
          case FilterExpressionSyntaxParser::K_IIF:
          case FilterExpressionSyntaxParser::K_ISNULL:
          case FilterExpressionSyntaxParser::K_ISREGEXMATCH:
          case FilterExpressionSyntaxParser::K_LEN:
          case FilterExpressionSyntaxParser::K_NOT:
          case FilterExpressionSyntaxParser::K_NULL:
          case FilterExpressionSyntaxParser::K_REGEXVAL:
          case FilterExpressionSyntaxParser::K_SUBSTR:
          case FilterExpressionSyntaxParser::K_SUBSTRING:
          case FilterExpressionSyntaxParser::K_TRIM:
          case FilterExpressionSyntaxParser::BOOLEAN_LITERAL:
          case FilterExpressionSyntaxParser::IDENTIFIER:
          case FilterExpressionSyntaxParser::INTEGER_LITERAL:
          case FilterExpressionSyntaxParser::NUMERIC_LITERAL:
          case FilterExpressionSyntaxParser::GUID_LITERAL:
          case FilterExpressionSyntaxParser::STRING_LITERAL:
          case FilterExpressionSyntaxParser::DATETIME_LITERAL: {
            setState(99);
            expression(0);
            setState(104);
            _errHandler->sync(this);
            _la = _input->LA(1);
            while (_la == FilterExpressionSyntaxParser::T__1) {
              setState(100);
              match(FilterExpressionSyntaxParser::T__1);
              setState(101);
              expression(0);
              setState(106);
              _errHandler->sync(this);
              _la = _input->LA(1);
            }
            break;
          }

          case FilterExpressionSyntaxParser::T__2: {
            setState(107);
            match(FilterExpressionSyntaxParser::T__2);
            break;
          }

          case FilterExpressionSyntaxParser::T__20: {
            break;
          }

        default:
          break;
        }
        setState(110);
        match(FilterExpressionSyntaxParser::T__20);
        break;
      }

      case FilterExpressionSyntaxParser::T__19: {
        setState(112);
        match(FilterExpressionSyntaxParser::T__19);
        setState(113);
        expression(0);
        setState(114);
        match(FilterExpressionSyntaxParser::T__20);
        break;
      }

    default:
      throw NoViableAltException(this);
    }
    _ctx->stop = _input->LT(-1);
    setState(170);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 19, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        if (!_parseListeners.empty())
          triggerExitRuleEvent();
        previousContext = _localctx;
        setState(168);
        _errHandler->sync(this);
        switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 18, _ctx)) {
        case 1: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(118);

          if (!(precpred(_ctx, 12))) throw FailedPredicateException(this, "precpred(_ctx, 12)");
          setState(119);
          _la = _input->LA(1);
          if (!((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__2)
            | (1ULL << FilterExpressionSyntaxParser::T__3)
            | (1ULL << FilterExpressionSyntaxParser::T__4))) != 0))) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(120);
          expression(13);
          break;
        }

        case 2: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(121);

          if (!(precpred(_ctx, 11))) throw FailedPredicateException(this, "precpred(_ctx, 11)");
          setState(122);
          _la = _input->LA(1);
          if (!(_la == FilterExpressionSyntaxParser::T__5

          || _la == FilterExpressionSyntaxParser::T__6)) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(123);
          expression(12);
          break;
        }

        case 3: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(124);

          if (!(precpred(_ctx, 10))) throw FailedPredicateException(this, "precpred(_ctx, 10)");
          setState(125);
          _la = _input->LA(1);
          if (!((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__7)
            | (1ULL << FilterExpressionSyntaxParser::T__8)
            | (1ULL << FilterExpressionSyntaxParser::T__9)
            | (1ULL << FilterExpressionSyntaxParser::T__10))) != 0))) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(126);
          expression(11);
          break;
        }

        case 4: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(127);

          if (!(precpred(_ctx, 9))) throw FailedPredicateException(this, "precpred(_ctx, 9)");
          setState(128);
          _la = _input->LA(1);
          if (!((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__11)
            | (1ULL << FilterExpressionSyntaxParser::T__12)
            | (1ULL << FilterExpressionSyntaxParser::T__13)
            | (1ULL << FilterExpressionSyntaxParser::T__14))) != 0))) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(129);
          expression(10);
          break;
        }

        case 5: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(130);

          if (!(precpred(_ctx, 8))) throw FailedPredicateException(this, "precpred(_ctx, 8)");
          setState(131);
          _la = _input->LA(1);
          if (!((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__15)
            | (1ULL << FilterExpressionSyntaxParser::T__16)
            | (1ULL << FilterExpressionSyntaxParser::T__17)
            | (1ULL << FilterExpressionSyntaxParser::T__18))) != 0))) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(132);
          expression(9);
          break;
        }

        case 6: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(133);

          if (!(precpred(_ctx, 5))) throw FailedPredicateException(this, "precpred(_ctx, 5)");
          setState(135);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::K_NOT) {
            setState(134);
            match(FilterExpressionSyntaxParser::K_NOT);
          }
          setState(137);
          match(FilterExpressionSyntaxParser::K_LIKE);
          setState(138);
          expression(6);
          break;
        }

        case 7: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(139);

          if (!(precpred(_ctx, 4))) throw FailedPredicateException(this, "precpred(_ctx, 4)");
          setState(140);
          match(FilterExpressionSyntaxParser::K_AND);
          setState(141);
          expression(5);
          break;
        }

        case 8: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(142);

          if (!(precpred(_ctx, 3))) throw FailedPredicateException(this, "precpred(_ctx, 3)");
          setState(143);
          match(FilterExpressionSyntaxParser::K_OR);
          setState(144);
          expression(4);
          break;
        }

        case 9: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(145);

          if (!(precpred(_ctx, 7))) throw FailedPredicateException(this, "precpred(_ctx, 7)");
          setState(146);
          match(FilterExpressionSyntaxParser::K_IS);
          setState(148);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::K_NOT) {
            setState(147);
            match(FilterExpressionSyntaxParser::K_NOT);
          }
          setState(150);
          match(FilterExpressionSyntaxParser::K_NULL);
          break;
        }

        case 10: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(151);

          if (!(precpred(_ctx, 6))) throw FailedPredicateException(this, "precpred(_ctx, 6)");
          setState(153);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::K_NOT) {
            setState(152);
            match(FilterExpressionSyntaxParser::K_NOT);
          }
          setState(155);
          match(FilterExpressionSyntaxParser::K_IN);

          setState(156);
          match(FilterExpressionSyntaxParser::T__19);
          setState(165);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if ((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__5)
            | (1ULL << FilterExpressionSyntaxParser::T__6)
            | (1ULL << FilterExpressionSyntaxParser::T__19)
            | (1ULL << FilterExpressionSyntaxParser::T__21)
            | (1ULL << FilterExpressionSyntaxParser::K_CONVERT)
            | (1ULL << FilterExpressionSyntaxParser::K_COALESCE)
            | (1ULL << FilterExpressionSyntaxParser::K_IIF)
            | (1ULL << FilterExpressionSyntaxParser::K_ISNULL)
            | (1ULL << FilterExpressionSyntaxParser::K_ISREGEXMATCH)
            | (1ULL << FilterExpressionSyntaxParser::K_LEN)
            | (1ULL << FilterExpressionSyntaxParser::K_NOT)
            | (1ULL << FilterExpressionSyntaxParser::K_NULL)
            | (1ULL << FilterExpressionSyntaxParser::K_REGEXVAL)
            | (1ULL << FilterExpressionSyntaxParser::K_SUBSTR)
            | (1ULL << FilterExpressionSyntaxParser::K_SUBSTRING)
            | (1ULL << FilterExpressionSyntaxParser::K_TRIM)
            | (1ULL << FilterExpressionSyntaxParser::BOOLEAN_LITERAL)
            | (1ULL << FilterExpressionSyntaxParser::IDENTIFIER)
            | (1ULL << FilterExpressionSyntaxParser::INTEGER_LITERAL)
            | (1ULL << FilterExpressionSyntaxParser::NUMERIC_LITERAL)
            | (1ULL << FilterExpressionSyntaxParser::GUID_LITERAL)
            | (1ULL << FilterExpressionSyntaxParser::STRING_LITERAL)
            | (1ULL << FilterExpressionSyntaxParser::DATETIME_LITERAL))) != 0)) {
            setState(157);
            expression(0);
            setState(162);
            _errHandler->sync(this);
            _la = _input->LA(1);
            while (_la == FilterExpressionSyntaxParser::T__1) {
              setState(158);
              match(FilterExpressionSyntaxParser::T__1);
              setState(159);
              expression(0);
              setState(164);
              _errHandler->sync(this);
              _la = _input->LA(1);
            }
          }
          setState(167);
          match(FilterExpressionSyntaxParser::T__20);
          break;
        }

        } 
      }
      setState(172);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 19, _ctx);
    }
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }
  return _localctx;
}

//----------------- LiteralValueContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::LiteralValueContext::LiteralValueContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::LiteralValueContext::INTEGER_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::INTEGER_LITERAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::LiteralValueContext::NUMERIC_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::NUMERIC_LITERAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::LiteralValueContext::STRING_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::STRING_LITERAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::LiteralValueContext::DATETIME_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::DATETIME_LITERAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::LiteralValueContext::GUID_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::GUID_LITERAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::LiteralValueContext::BOOLEAN_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::BOOLEAN_LITERAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::LiteralValueContext::K_NULL() {
  return getToken(FilterExpressionSyntaxParser::K_NULL, 0);
}


size_t FilterExpressionSyntaxParser::LiteralValueContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleLiteralValue;
}

void FilterExpressionSyntaxParser::LiteralValueContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterLiteralValue(this);
}

void FilterExpressionSyntaxParser::LiteralValueContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitLiteralValue(this);
}

FilterExpressionSyntaxParser::LiteralValueContext* FilterExpressionSyntaxParser::literalValue() {
  LiteralValueContext *_localctx = _tracker.createInstance<LiteralValueContext>(_ctx, getState());
  enterRule(_localctx, 16, FilterExpressionSyntaxParser::RuleLiteralValue);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(173);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::K_NULL)
      | (1ULL << FilterExpressionSyntaxParser::BOOLEAN_LITERAL)
      | (1ULL << FilterExpressionSyntaxParser::INTEGER_LITERAL)
      | (1ULL << FilterExpressionSyntaxParser::NUMERIC_LITERAL)
      | (1ULL << FilterExpressionSyntaxParser::GUID_LITERAL)
      | (1ULL << FilterExpressionSyntaxParser::STRING_LITERAL)
      | (1ULL << FilterExpressionSyntaxParser::DATETIME_LITERAL))) != 0))) {
    _errHandler->recoverInline(this);
    }
    else {
      _errHandler->reportMatch(this);
      consume();
    }
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- UnaryOperatorContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::UnaryOperatorContext::UnaryOperatorContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::UnaryOperatorContext::K_NOT() {
  return getToken(FilterExpressionSyntaxParser::K_NOT, 0);
}


size_t FilterExpressionSyntaxParser::UnaryOperatorContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleUnaryOperator;
}

void FilterExpressionSyntaxParser::UnaryOperatorContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterUnaryOperator(this);
}

void FilterExpressionSyntaxParser::UnaryOperatorContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitUnaryOperator(this);
}

FilterExpressionSyntaxParser::UnaryOperatorContext* FilterExpressionSyntaxParser::unaryOperator() {
  UnaryOperatorContext *_localctx = _tracker.createInstance<UnaryOperatorContext>(_ctx, getState());
  enterRule(_localctx, 18, FilterExpressionSyntaxParser::RuleUnaryOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(175);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__5)
      | (1ULL << FilterExpressionSyntaxParser::T__6)
      | (1ULL << FilterExpressionSyntaxParser::T__21)
      | (1ULL << FilterExpressionSyntaxParser::K_NOT))) != 0))) {
    _errHandler->recoverInline(this);
    }
    else {
      _errHandler->reportMatch(this);
      consume();
    }
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- FunctionNameContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::FunctionNameContext::FunctionNameContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_COALESCE() {
  return getToken(FilterExpressionSyntaxParser::K_COALESCE, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_CONVERT() {
  return getToken(FilterExpressionSyntaxParser::K_CONVERT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_IIF() {
  return getToken(FilterExpressionSyntaxParser::K_IIF, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_ISNULL() {
  return getToken(FilterExpressionSyntaxParser::K_ISNULL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_ISREGEXMATCH() {
  return getToken(FilterExpressionSyntaxParser::K_ISREGEXMATCH, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_LEN() {
  return getToken(FilterExpressionSyntaxParser::K_LEN, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_REGEXVAL() {
  return getToken(FilterExpressionSyntaxParser::K_REGEXVAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_SUBSTR() {
  return getToken(FilterExpressionSyntaxParser::K_SUBSTR, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_SUBSTRING() {
  return getToken(FilterExpressionSyntaxParser::K_SUBSTRING, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_TRIM() {
  return getToken(FilterExpressionSyntaxParser::K_TRIM, 0);
}


size_t FilterExpressionSyntaxParser::FunctionNameContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleFunctionName;
}

void FilterExpressionSyntaxParser::FunctionNameContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterFunctionName(this);
}

void FilterExpressionSyntaxParser::FunctionNameContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitFunctionName(this);
}

FilterExpressionSyntaxParser::FunctionNameContext* FilterExpressionSyntaxParser::functionName() {
  FunctionNameContext *_localctx = _tracker.createInstance<FunctionNameContext>(_ctx, getState());
  enterRule(_localctx, 20, FilterExpressionSyntaxParser::RuleFunctionName);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(177);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::K_CONVERT)
      | (1ULL << FilterExpressionSyntaxParser::K_COALESCE)
      | (1ULL << FilterExpressionSyntaxParser::K_IIF)
      | (1ULL << FilterExpressionSyntaxParser::K_ISNULL)
      | (1ULL << FilterExpressionSyntaxParser::K_ISREGEXMATCH)
      | (1ULL << FilterExpressionSyntaxParser::K_LEN)
      | (1ULL << FilterExpressionSyntaxParser::K_REGEXVAL)
      | (1ULL << FilterExpressionSyntaxParser::K_SUBSTR)
      | (1ULL << FilterExpressionSyntaxParser::K_SUBSTRING)
      | (1ULL << FilterExpressionSyntaxParser::K_TRIM))) != 0))) {
    _errHandler->recoverInline(this);
    }
    else {
      _errHandler->reportMatch(this);
      consume();
    }
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- TableNameContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::TableNameContext::TableNameContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::TableNameContext::IDENTIFIER() {
  return getToken(FilterExpressionSyntaxParser::IDENTIFIER, 0);
}


size_t FilterExpressionSyntaxParser::TableNameContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleTableName;
}

void FilterExpressionSyntaxParser::TableNameContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterTableName(this);
}

void FilterExpressionSyntaxParser::TableNameContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitTableName(this);
}

FilterExpressionSyntaxParser::TableNameContext* FilterExpressionSyntaxParser::tableName() {
  TableNameContext *_localctx = _tracker.createInstance<TableNameContext>(_ctx, getState());
  enterRule(_localctx, 22, FilterExpressionSyntaxParser::RuleTableName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(179);
    match(FilterExpressionSyntaxParser::IDENTIFIER);
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- ColumnNameContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::ColumnNameContext::ColumnNameContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::ColumnNameContext::IDENTIFIER() {
  return getToken(FilterExpressionSyntaxParser::IDENTIFIER, 0);
}


size_t FilterExpressionSyntaxParser::ColumnNameContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleColumnName;
}

void FilterExpressionSyntaxParser::ColumnNameContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterColumnName(this);
}

void FilterExpressionSyntaxParser::ColumnNameContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitColumnName(this);
}

FilterExpressionSyntaxParser::ColumnNameContext* FilterExpressionSyntaxParser::columnName() {
  ColumnNameContext *_localctx = _tracker.createInstance<ColumnNameContext>(_ctx, getState());
  enterRule(_localctx, 24, FilterExpressionSyntaxParser::RuleColumnName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(181);
    match(FilterExpressionSyntaxParser::IDENTIFIER);
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

//----------------- OrderByColumnNameContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::OrderByColumnNameContext::OrderByColumnNameContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::OrderByColumnNameContext::IDENTIFIER() {
  return getToken(FilterExpressionSyntaxParser::IDENTIFIER, 0);
}


size_t FilterExpressionSyntaxParser::OrderByColumnNameContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleOrderByColumnName;
}

void FilterExpressionSyntaxParser::OrderByColumnNameContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterOrderByColumnName(this);
}

void FilterExpressionSyntaxParser::OrderByColumnNameContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitOrderByColumnName(this);
}

FilterExpressionSyntaxParser::OrderByColumnNameContext* FilterExpressionSyntaxParser::orderByColumnName() {
  OrderByColumnNameContext *_localctx = _tracker.createInstance<OrderByColumnNameContext>(_ctx, getState());
  enterRule(_localctx, 26, FilterExpressionSyntaxParser::RuleOrderByColumnName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(183);
    match(FilterExpressionSyntaxParser::IDENTIFIER);
   
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }

  return _localctx;
}

bool FilterExpressionSyntaxParser::sempred(RuleContext *context, size_t ruleIndex, size_t predicateIndex) {
  switch (ruleIndex) {
    case 7: return expressionSempred(dynamic_cast<ExpressionContext *>(context), predicateIndex);

  default:
    break;
  }
  return true;
}

bool FilterExpressionSyntaxParser::expressionSempred(ExpressionContext *_localctx, size_t predicateIndex) {
  switch (predicateIndex) {
    case 0: return precpred(_ctx, 12);
    case 1: return precpred(_ctx, 11);
    case 2: return precpred(_ctx, 10);
    case 3: return precpred(_ctx, 9);
    case 4: return precpred(_ctx, 8);
    case 5: return precpred(_ctx, 5);
    case 6: return precpred(_ctx, 4);
    case 7: return precpred(_ctx, 3);
    case 8: return precpred(_ctx, 7);
    case 9: return precpred(_ctx, 6);

  default:
    break;
  }
  return true;
}

// Static vars and initialization.
std::vector<dfa::DFA> FilterExpressionSyntaxParser::_decisionToDFA;
atn::PredictionContextCache FilterExpressionSyntaxParser::_sharedContextCache;

// We own the ATN which in turn owns the ATN states.
atn::ATN FilterExpressionSyntaxParser::_atn;
std::vector<uint16_t> FilterExpressionSyntaxParser::_serializedATN;

std::vector<std::string> FilterExpressionSyntaxParser::_ruleNames = {
  "parse", "error", "filterExpressionStatementList", "filterExpressionStatement", 
  "identifierStatement", "filterStatement", "orderingTerm", "expression", 
  "literalValue", "unaryOperator", "functionName", "tableName", "columnName", 
  "orderByColumnName"
};

std::vector<std::string> FilterExpressionSyntaxParser::_literalNames = {
  "", "';'", "','", "'*'", "'/'", "'%'", "'+'", "'-'", "'<<'", "'>>'", "'&'", 
  "'|'", "'<'", "'<='", "'>'", "'>='", "'='", "'=='", "'!='", "'<>'", "'('", 
  "')'", "'~'"
};

std::vector<std::string> FilterExpressionSyntaxParser::_symbolicNames = {
  "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
  "", "", "", "", "", "K_AND", "K_ASC", "K_BY", "K_CONVERT", "K_COALESCE", 
  "K_DESC", "K_FILTER", "K_IIF", "K_IN", "K_IS", "K_ISNULL", "K_ISREGEXMATCH", 
  "K_LEN", "K_LIKE", "K_NOT", "K_NULL", "K_OR", "K_ORDER", "K_REGEXVAL", 
  "K_SUBSTR", "K_SUBSTRING", "K_TOP", "K_TRIM", "K_WHERE", "BOOLEAN_LITERAL", 
  "IDENTIFIER", "INTEGER_LITERAL", "NUMERIC_LITERAL", "GUID_LITERAL", "MEASUREMENT_KEY_LITERAL", 
  "POINT_TAG_LITERAL", "STRING_LITERAL", "DATETIME_LITERAL", "SINGLE_LINE_COMMENT", 
  "MULTILINE_COMMENT", "SPACES", "UNEXPECTED_CHAR"
};

dfa::Vocabulary FilterExpressionSyntaxParser::_vocabulary(_literalNames, _symbolicNames);

std::vector<std::string> FilterExpressionSyntaxParser::_tokenNames;

FilterExpressionSyntaxParser::Initializer::Initializer() {
	for (size_t i = 0; i < _symbolicNames.size(); ++i) {
		std::string name = _vocabulary.getLiteralName(i);
		if (name.empty()) {
			name = _vocabulary.getSymbolicName(i);
		}

		if (name.empty()) {
			_tokenNames.push_back("<INVALID>");
		} else {
      _tokenNames.push_back(name);
    }
	}

  _serializedATN = {
    0x3, 0x608b, 0xa72a, 0x8133, 0xb9ed, 0x417c, 0x3be7, 0x7786, 0x5964, 
    0x3, 0x3d, 0xbc, 0x4, 0x2, 0x9, 0x2, 0x4, 0x3, 0x9, 0x3, 0x4, 0x4, 0x9, 
    0x4, 0x4, 0x5, 0x9, 0x5, 0x4, 0x6, 0x9, 0x6, 0x4, 0x7, 0x9, 0x7, 0x4, 
    0x8, 0x9, 0x8, 0x4, 0x9, 0x9, 0x9, 0x4, 0xa, 0x9, 0xa, 0x4, 0xb, 0x9, 
    0xb, 0x4, 0xc, 0x9, 0xc, 0x4, 0xd, 0x9, 0xd, 0x4, 0xe, 0x9, 0xe, 0x4, 
    0xf, 0x9, 0xf, 0x3, 0x2, 0x3, 0x2, 0x5, 0x2, 0x21, 0xa, 0x2, 0x3, 0x2, 
    0x3, 0x2, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x4, 0x7, 0x4, 0x29, 0xa, 
    0x4, 0xc, 0x4, 0xe, 0x4, 0x2c, 0xb, 0x4, 0x3, 0x4, 0x3, 0x4, 0x6, 0x4, 
    0x30, 0xa, 0x4, 0xd, 0x4, 0xe, 0x4, 0x31, 0x3, 0x4, 0x7, 0x4, 0x35, 
    0xa, 0x4, 0xc, 0x4, 0xe, 0x4, 0x38, 0xb, 0x4, 0x3, 0x4, 0x7, 0x4, 0x3b, 
    0xa, 0x4, 0xc, 0x4, 0xe, 0x4, 0x3e, 0xb, 0x4, 0x3, 0x5, 0x3, 0x5, 0x5, 
    0x5, 0x42, 0xa, 0x5, 0x3, 0x6, 0x3, 0x6, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 
    0x5, 0x7, 0x49, 0xa, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 
    0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x7, 0x7, 0x53, 0xa, 0x7, 0xc, 0x7, 
    0xe, 0x7, 0x56, 0xb, 0x7, 0x5, 0x7, 0x58, 0xa, 0x7, 0x3, 0x8, 0x3, 0x8, 
    0x5, 0x8, 0x5c, 0xa, 0x8, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 
    0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x7, 
    0x9, 0x69, 0xa, 0x9, 0xc, 0x9, 0xe, 0x9, 0x6c, 0xb, 0x9, 0x3, 0x9, 0x5, 
    0x9, 0x6f, 0xa, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 
    0x3, 0x9, 0x5, 0x9, 0x77, 0xa, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 
    0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 
    0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x5, 
    0x9, 0x8a, 0xa, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 
    0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x5, 0x9, 
    0x97, 0xa, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x5, 0x9, 0x9c, 0xa, 0x9, 
    0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x7, 0x9, 0xa3, 0xa, 
    0x9, 0xc, 0x9, 0xe, 0x9, 0xa6, 0xb, 0x9, 0x5, 0x9, 0xa8, 0xa, 0x9, 0x3, 
    0x9, 0x7, 0x9, 0xab, 0xa, 0x9, 0xc, 0x9, 0xe, 0x9, 0xae, 0xb, 0x9, 0x3, 
    0xa, 0x3, 0xa, 0x3, 0xb, 0x3, 0xb, 0x3, 0xc, 0x3, 0xc, 0x3, 0xd, 0x3, 
    0xd, 0x3, 0xe, 0x3, 0xe, 0x3, 0xf, 0x3, 0xf, 0x3, 0xf, 0x2, 0x3, 0x10, 
    0x10, 0x2, 0x4, 0x6, 0x8, 0xa, 0xc, 0xe, 0x10, 0x12, 0x14, 0x16, 0x18, 
    0x1a, 0x1c, 0x2, 0xc, 0x3, 0x2, 0x35, 0x37, 0x4, 0x2, 0x1a, 0x1a, 0x1e, 
    0x1e, 0x3, 0x2, 0x5, 0x7, 0x3, 0x2, 0x8, 0x9, 0x3, 0x2, 0xa, 0xd, 0x3, 
    0x2, 0xe, 0x11, 0x3, 0x2, 0x12, 0x15, 0x6, 0x2, 0x28, 0x28, 0x31, 0x31, 
    0x33, 0x35, 0x38, 0x39, 0x5, 0x2, 0x8, 0x9, 0x18, 0x18, 0x27, 0x27, 
    0x7, 0x2, 0x1c, 0x1d, 0x20, 0x20, 0x23, 0x25, 0x2b, 0x2d, 0x2f, 0x2f, 
    0x2, 0xcd, 0x2, 0x20, 0x3, 0x2, 0x2, 0x2, 0x4, 0x24, 0x3, 0x2, 0x2, 
    0x2, 0x6, 0x2a, 0x3, 0x2, 0x2, 0x2, 0x8, 0x41, 0x3, 0x2, 0x2, 0x2, 0xa, 
    0x43, 0x3, 0x2, 0x2, 0x2, 0xc, 0x45, 0x3, 0x2, 0x2, 0x2, 0xe, 0x59, 
    0x3, 0x2, 0x2, 0x2, 0x10, 0x76, 0x3, 0x2, 0x2, 0x2, 0x12, 0xaf, 0x3, 
    0x2, 0x2, 0x2, 0x14, 0xb1, 0x3, 0x2, 0x2, 0x2, 0x16, 0xb3, 0x3, 0x2, 
    0x2, 0x2, 0x18, 0xb5, 0x3, 0x2, 0x2, 0x2, 0x1a, 0xb7, 0x3, 0x2, 0x2, 
    0x2, 0x1c, 0xb9, 0x3, 0x2, 0x2, 0x2, 0x1e, 0x21, 0x5, 0x6, 0x4, 0x2, 
    0x1f, 0x21, 0x5, 0x4, 0x3, 0x2, 0x20, 0x1e, 0x3, 0x2, 0x2, 0x2, 0x20, 
    0x1f, 0x3, 0x2, 0x2, 0x2, 0x21, 0x22, 0x3, 0x2, 0x2, 0x2, 0x22, 0x23, 
    0x7, 0x2, 0x2, 0x3, 0x23, 0x3, 0x3, 0x2, 0x2, 0x2, 0x24, 0x25, 0x7, 
    0x3d, 0x2, 0x2, 0x25, 0x26, 0x8, 0x3, 0x1, 0x2, 0x26, 0x5, 0x3, 0x2, 
    0x2, 0x2, 0x27, 0x29, 0x7, 0x3, 0x2, 0x2, 0x28, 0x27, 0x3, 0x2, 0x2, 
    0x2, 0x29, 0x2c, 0x3, 0x2, 0x2, 0x2, 0x2a, 0x28, 0x3, 0x2, 0x2, 0x2, 
    0x2a, 0x2b, 0x3, 0x2, 0x2, 0x2, 0x2b, 0x2d, 0x3, 0x2, 0x2, 0x2, 0x2c, 
    0x2a, 0x3, 0x2, 0x2, 0x2, 0x2d, 0x36, 0x5, 0x8, 0x5, 0x2, 0x2e, 0x30, 
    0x7, 0x3, 0x2, 0x2, 0x2f, 0x2e, 0x3, 0x2, 0x2, 0x2, 0x30, 0x31, 0x3, 
    0x2, 0x2, 0x2, 0x31, 0x2f, 0x3, 0x2, 0x2, 0x2, 0x31, 0x32, 0x3, 0x2, 
    0x2, 0x2, 0x32, 0x33, 0x3, 0x2, 0x2, 0x2, 0x33, 0x35, 0x5, 0x8, 0x5, 
    0x2, 0x34, 0x2f, 0x3, 0x2, 0x2, 0x2, 0x35, 0x38, 0x3, 0x2, 0x2, 0x2, 
    0x36, 0x34, 0x3, 0x2, 0x2, 0x2, 0x36, 0x37, 0x3, 0x2, 0x2, 0x2, 0x37, 
    0x3c, 0x3, 0x2, 0x2, 0x2, 0x38, 0x36, 0x3, 0x2, 0x2, 0x2, 0x39, 0x3b, 
    0x7, 0x3, 0x2, 0x2, 0x3a, 0x39, 0x3, 0x2, 0x2, 0x2, 0x3b, 0x3e, 0x3, 
    0x2, 0x2, 0x2, 0x3c, 0x3a, 0x3, 0x2, 0x2, 0x2, 0x3c, 0x3d, 0x3, 0x2, 
    0x2, 0x2, 0x3d, 0x7, 0x3, 0x2, 0x2, 0x2, 0x3e, 0x3c, 0x3, 0x2, 0x2, 
    0x2, 0x3f, 0x42, 0x5, 0xa, 0x6, 0x2, 0x40, 0x42, 0x5, 0xc, 0x7, 0x2, 
    0x41, 0x3f, 0x3, 0x2, 0x2, 0x2, 0x41, 0x40, 0x3, 0x2, 0x2, 0x2, 0x42, 
    0x9, 0x3, 0x2, 0x2, 0x2, 0x43, 0x44, 0x9, 0x2, 0x2, 0x2, 0x44, 0xb, 
    0x3, 0x2, 0x2, 0x2, 0x45, 0x48, 0x7, 0x1f, 0x2, 0x2, 0x46, 0x47, 0x7, 
    0x2e, 0x2, 0x2, 0x47, 0x49, 0x7, 0x33, 0x2, 0x2, 0x48, 0x46, 0x3, 0x2, 
    0x2, 0x2, 0x48, 0x49, 0x3, 0x2, 0x2, 0x2, 0x49, 0x4a, 0x3, 0x2, 0x2, 
    0x2, 0x4a, 0x4b, 0x5, 0x18, 0xd, 0x2, 0x4b, 0x4c, 0x7, 0x30, 0x2, 0x2, 
    0x4c, 0x57, 0x5, 0x10, 0x9, 0x2, 0x4d, 0x4e, 0x7, 0x2a, 0x2, 0x2, 0x4e, 
    0x4f, 0x7, 0x1b, 0x2, 0x2, 0x4f, 0x54, 0x5, 0xe, 0x8, 0x2, 0x50, 0x51, 
    0x7, 0x4, 0x2, 0x2, 0x51, 0x53, 0x5, 0xe, 0x8, 0x2, 0x52, 0x50, 0x3, 
    0x2, 0x2, 0x2, 0x53, 0x56, 0x3, 0x2, 0x2, 0x2, 0x54, 0x52, 0x3, 0x2, 
    0x2, 0x2, 0x54, 0x55, 0x3, 0x2, 0x2, 0x2, 0x55, 0x58, 0x3, 0x2, 0x2, 
    0x2, 0x56, 0x54, 0x3, 0x2, 0x2, 0x2, 0x57, 0x4d, 0x3, 0x2, 0x2, 0x2, 
    0x57, 0x58, 0x3, 0x2, 0x2, 0x2, 0x58, 0xd, 0x3, 0x2, 0x2, 0x2, 0x59, 
    0x5b, 0x5, 0x1c, 0xf, 0x2, 0x5a, 0x5c, 0x9, 0x3, 0x2, 0x2, 0x5b, 0x5a, 
    0x3, 0x2, 0x2, 0x2, 0x5b, 0x5c, 0x3, 0x2, 0x2, 0x2, 0x5c, 0xf, 0x3, 
    0x2, 0x2, 0x2, 0x5d, 0x5e, 0x8, 0x9, 0x1, 0x2, 0x5e, 0x77, 0x5, 0x12, 
    0xa, 0x2, 0x5f, 0x77, 0x5, 0x1a, 0xe, 0x2, 0x60, 0x61, 0x5, 0x14, 0xb, 
    0x2, 0x61, 0x62, 0x5, 0x10, 0x9, 0xf, 0x62, 0x77, 0x3, 0x2, 0x2, 0x2, 
    0x63, 0x64, 0x5, 0x16, 0xc, 0x2, 0x64, 0x6e, 0x7, 0x16, 0x2, 0x2, 0x65, 
    0x6a, 0x5, 0x10, 0x9, 0x2, 0x66, 0x67, 0x7, 0x4, 0x2, 0x2, 0x67, 0x69, 
    0x5, 0x10, 0x9, 0x2, 0x68, 0x66, 0x3, 0x2, 0x2, 0x2, 0x69, 0x6c, 0x3, 
    0x2, 0x2, 0x2, 0x6a, 0x68, 0x3, 0x2, 0x2, 0x2, 0x6a, 0x6b, 0x3, 0x2, 
    0x2, 0x2, 0x6b, 0x6f, 0x3, 0x2, 0x2, 0x2, 0x6c, 0x6a, 0x3, 0x2, 0x2, 
    0x2, 0x6d, 0x6f, 0x7, 0x5, 0x2, 0x2, 0x6e, 0x65, 0x3, 0x2, 0x2, 0x2, 
    0x6e, 0x6d, 0x3, 0x2, 0x2, 0x2, 0x6e, 0x6f, 0x3, 0x2, 0x2, 0x2, 0x6f, 
    0x70, 0x3, 0x2, 0x2, 0x2, 0x70, 0x71, 0x7, 0x17, 0x2, 0x2, 0x71, 0x77, 
    0x3, 0x2, 0x2, 0x2, 0x72, 0x73, 0x7, 0x16, 0x2, 0x2, 0x73, 0x74, 0x5, 
    0x10, 0x9, 0x2, 0x74, 0x75, 0x7, 0x17, 0x2, 0x2, 0x75, 0x77, 0x3, 0x2, 
    0x2, 0x2, 0x76, 0x5d, 0x3, 0x2, 0x2, 0x2, 0x76, 0x5f, 0x3, 0x2, 0x2, 
    0x2, 0x76, 0x60, 0x3, 0x2, 0x2, 0x2, 0x76, 0x63, 0x3, 0x2, 0x2, 0x2, 
    0x76, 0x72, 0x3, 0x2, 0x2, 0x2, 0x77, 0xac, 0x3, 0x2, 0x2, 0x2, 0x78, 
    0x79, 0xc, 0xe, 0x2, 0x2, 0x79, 0x7a, 0x9, 0x4, 0x2, 0x2, 0x7a, 0xab, 
    0x5, 0x10, 0x9, 0xf, 0x7b, 0x7c, 0xc, 0xd, 0x2, 0x2, 0x7c, 0x7d, 0x9, 
    0x5, 0x2, 0x2, 0x7d, 0xab, 0x5, 0x10, 0x9, 0xe, 0x7e, 0x7f, 0xc, 0xc, 
    0x2, 0x2, 0x7f, 0x80, 0x9, 0x6, 0x2, 0x2, 0x80, 0xab, 0x5, 0x10, 0x9, 
    0xd, 0x81, 0x82, 0xc, 0xb, 0x2, 0x2, 0x82, 0x83, 0x9, 0x7, 0x2, 0x2, 
    0x83, 0xab, 0x5, 0x10, 0x9, 0xc, 0x84, 0x85, 0xc, 0xa, 0x2, 0x2, 0x85, 
    0x86, 0x9, 0x8, 0x2, 0x2, 0x86, 0xab, 0x5, 0x10, 0x9, 0xb, 0x87, 0x89, 
    0xc, 0x7, 0x2, 0x2, 0x88, 0x8a, 0x7, 0x27, 0x2, 0x2, 0x89, 0x88, 0x3, 
    0x2, 0x2, 0x2, 0x89, 0x8a, 0x3, 0x2, 0x2, 0x2, 0x8a, 0x8b, 0x3, 0x2, 
    0x2, 0x2, 0x8b, 0x8c, 0x7, 0x26, 0x2, 0x2, 0x8c, 0xab, 0x5, 0x10, 0x9, 
    0x8, 0x8d, 0x8e, 0xc, 0x6, 0x2, 0x2, 0x8e, 0x8f, 0x7, 0x19, 0x2, 0x2, 
    0x8f, 0xab, 0x5, 0x10, 0x9, 0x7, 0x90, 0x91, 0xc, 0x5, 0x2, 0x2, 0x91, 
    0x92, 0x7, 0x29, 0x2, 0x2, 0x92, 0xab, 0x5, 0x10, 0x9, 0x6, 0x93, 0x94, 
    0xc, 0x9, 0x2, 0x2, 0x94, 0x96, 0x7, 0x22, 0x2, 0x2, 0x95, 0x97, 0x7, 
    0x27, 0x2, 0x2, 0x96, 0x95, 0x3, 0x2, 0x2, 0x2, 0x96, 0x97, 0x3, 0x2, 
    0x2, 0x2, 0x97, 0x98, 0x3, 0x2, 0x2, 0x2, 0x98, 0xab, 0x7, 0x28, 0x2, 
    0x2, 0x99, 0x9b, 0xc, 0x8, 0x2, 0x2, 0x9a, 0x9c, 0x7, 0x27, 0x2, 0x2, 
    0x9b, 0x9a, 0x3, 0x2, 0x2, 0x2, 0x9b, 0x9c, 0x3, 0x2, 0x2, 0x2, 0x9c, 
    0x9d, 0x3, 0x2, 0x2, 0x2, 0x9d, 0x9e, 0x7, 0x21, 0x2, 0x2, 0x9e, 0xa7, 
    0x7, 0x16, 0x2, 0x2, 0x9f, 0xa4, 0x5, 0x10, 0x9, 0x2, 0xa0, 0xa1, 0x7, 
    0x4, 0x2, 0x2, 0xa1, 0xa3, 0x5, 0x10, 0x9, 0x2, 0xa2, 0xa0, 0x3, 0x2, 
    0x2, 0x2, 0xa3, 0xa6, 0x3, 0x2, 0x2, 0x2, 0xa4, 0xa2, 0x3, 0x2, 0x2, 
    0x2, 0xa4, 0xa5, 0x3, 0x2, 0x2, 0x2, 0xa5, 0xa8, 0x3, 0x2, 0x2, 0x2, 
    0xa6, 0xa4, 0x3, 0x2, 0x2, 0x2, 0xa7, 0x9f, 0x3, 0x2, 0x2, 0x2, 0xa7, 
    0xa8, 0x3, 0x2, 0x2, 0x2, 0xa8, 0xa9, 0x3, 0x2, 0x2, 0x2, 0xa9, 0xab, 
    0x7, 0x17, 0x2, 0x2, 0xaa, 0x78, 0x3, 0x2, 0x2, 0x2, 0xaa, 0x7b, 0x3, 
    0x2, 0x2, 0x2, 0xaa, 0x7e, 0x3, 0x2, 0x2, 0x2, 0xaa, 0x81, 0x3, 0x2, 
    0x2, 0x2, 0xaa, 0x84, 0x3, 0x2, 0x2, 0x2, 0xaa, 0x87, 0x3, 0x2, 0x2, 
    0x2, 0xaa, 0x8d, 0x3, 0x2, 0x2, 0x2, 0xaa, 0x90, 0x3, 0x2, 0x2, 0x2, 
    0xaa, 0x93, 0x3, 0x2, 0x2, 0x2, 0xaa, 0x99, 0x3, 0x2, 0x2, 0x2, 0xab, 
    0xae, 0x3, 0x2, 0x2, 0x2, 0xac, 0xaa, 0x3, 0x2, 0x2, 0x2, 0xac, 0xad, 
    0x3, 0x2, 0x2, 0x2, 0xad, 0x11, 0x3, 0x2, 0x2, 0x2, 0xae, 0xac, 0x3, 
    0x2, 0x2, 0x2, 0xaf, 0xb0, 0x9, 0x9, 0x2, 0x2, 0xb0, 0x13, 0x3, 0x2, 
    0x2, 0x2, 0xb1, 0xb2, 0x9, 0xa, 0x2, 0x2, 0xb2, 0x15, 0x3, 0x2, 0x2, 
    0x2, 0xb3, 0xb4, 0x9, 0xb, 0x2, 0x2, 0xb4, 0x17, 0x3, 0x2, 0x2, 0x2, 
    0xb5, 0xb6, 0x7, 0x32, 0x2, 0x2, 0xb6, 0x19, 0x3, 0x2, 0x2, 0x2, 0xb7, 
    0xb8, 0x7, 0x32, 0x2, 0x2, 0xb8, 0x1b, 0x3, 0x2, 0x2, 0x2, 0xb9, 0xba, 
    0x7, 0x32, 0x2, 0x2, 0xba, 0x1d, 0x3, 0x2, 0x2, 0x2, 0x16, 0x20, 0x2a, 
    0x31, 0x36, 0x3c, 0x41, 0x48, 0x54, 0x57, 0x5b, 0x6a, 0x6e, 0x76, 0x89, 
    0x96, 0x9b, 0xa4, 0xa7, 0xaa, 0xac, 
  };

  atn::ATNDeserializer deserializer;
  _atn = deserializer.deserialize(_serializedATN);

  size_t count = _atn.getNumberOfDecisions();
  _decisionToDFA.reserve(count);
  for (size_t i = 0; i < count; i++) { 
    _decisionToDFA.emplace_back(_atn.getDecisionState(i), i);
  }
}

FilterExpressionSyntaxParser::Initializer FilterExpressionSyntaxParser::_init;
