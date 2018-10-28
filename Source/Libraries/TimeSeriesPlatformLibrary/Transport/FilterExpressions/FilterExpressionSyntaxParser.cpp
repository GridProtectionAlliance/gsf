
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
    setState(32);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case FilterExpressionSyntaxParser::T__0:
      case FilterExpressionSyntaxParser::K_FILTER:
      case FilterExpressionSyntaxParser::GUID_LITERAL:
      case FilterExpressionSyntaxParser::MEASUREMENT_KEY_LITERAL:
      case FilterExpressionSyntaxParser::POINT_TAG_LITERAL: {
        setState(30);
        filterExpressionStatementList();
        break;
      }

      case FilterExpressionSyntaxParser::UNEXPECTED_CHAR: {
        setState(31);
        error();
        break;
      }

    default:
      throw NoViableAltException(this);
    }
    setState(34);
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
    setState(36);
    dynamic_cast<ErrorContext *>(_localctx)->unexpected_charToken = match(FilterExpressionSyntaxParser::UNEXPECTED_CHAR);
     
         throw new RuntimeException("Unexpected character: " + (dynamic_cast<ErrorContext *>(_localctx)->unexpected_charToken != nullptr ? dynamic_cast<ErrorContext *>(_localctx)->unexpected_charToken->getText() : "")); 
       
   
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
    setState(42);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while (_la == FilterExpressionSyntaxParser::T__0) {
      setState(39);
      match(FilterExpressionSyntaxParser::T__0);
      setState(44);
      _errHandler->sync(this);
      _la = _input->LA(1);
    }
    setState(45);
    filterExpressionStatement();
    setState(54);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 3, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        setState(47); 
        _errHandler->sync(this);
        _la = _input->LA(1);
        do {
          setState(46);
          match(FilterExpressionSyntaxParser::T__0);
          setState(49); 
          _errHandler->sync(this);
          _la = _input->LA(1);
        } while (_la == FilterExpressionSyntaxParser::T__0);
        setState(51);
        filterExpressionStatement(); 
      }
      setState(56);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 3, _ctx);
    }
    setState(60);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while (_la == FilterExpressionSyntaxParser::T__0) {
      setState(57);
      match(FilterExpressionSyntaxParser::T__0);
      setState(62);
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

FilterExpressionSyntaxParser::FilterStatementContext* FilterExpressionSyntaxParser::FilterExpressionStatementContext::filterStatement() {
  return getRuleContext<FilterExpressionSyntaxParser::FilterStatementContext>(0);
}

FilterExpressionSyntaxParser::IdentifierStatementContext* FilterExpressionSyntaxParser::FilterExpressionStatementContext::identifierStatement() {
  return getRuleContext<FilterExpressionSyntaxParser::IdentifierStatementContext>(0);
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
    setState(65);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case FilterExpressionSyntaxParser::K_FILTER: {
        enterOuterAlt(_localctx, 1);
        setState(63);
        filterStatement();
        break;
      }

      case FilterExpressionSyntaxParser::GUID_LITERAL:
      case FilterExpressionSyntaxParser::MEASUREMENT_KEY_LITERAL:
      case FilterExpressionSyntaxParser::POINT_TAG_LITERAL: {
        enterOuterAlt(_localctx, 2);
        setState(64);
        identifierStatement();
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
    setState(67);
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
    setState(69);
    match(FilterExpressionSyntaxParser::K_FILTER);
    setState(72);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_TOP) {
      setState(70);
      match(FilterExpressionSyntaxParser::K_TOP);
      setState(71);
      match(FilterExpressionSyntaxParser::INTEGER_LITERAL);
    }
    setState(74);
    tableName();
    setState(75);
    match(FilterExpressionSyntaxParser::K_WHERE);
    setState(76);
    expression(0);
    setState(87);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_ORDER) {
      setState(77);
      match(FilterExpressionSyntaxParser::K_ORDER);
      setState(78);
      match(FilterExpressionSyntaxParser::K_BY);
      setState(79);
      orderingTerm();
      setState(84);
      _errHandler->sync(this);
      _la = _input->LA(1);
      while (_la == FilterExpressionSyntaxParser::T__1) {
        setState(80);
        match(FilterExpressionSyntaxParser::T__1);
        setState(81);
        orderingTerm();
        setState(86);
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

FilterExpressionSyntaxParser::ColumnNameContext* FilterExpressionSyntaxParser::OrderingTermContext::columnName() {
  return getRuleContext<FilterExpressionSyntaxParser::ColumnNameContext>(0);
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
    setState(89);
    columnName();
    setState(91);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_ASC

    || _la == FilterExpressionSyntaxParser::K_DESC) {
      setState(90);
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

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_IS() {
  return getToken(FilterExpressionSyntaxParser::K_IS, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_NOT() {
  return getToken(FilterExpressionSyntaxParser::K_NOT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_IN() {
  return getToken(FilterExpressionSyntaxParser::K_IN, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_LIKE() {
  return getToken(FilterExpressionSyntaxParser::K_LIKE, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_AND() {
  return getToken(FilterExpressionSyntaxParser::K_AND, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_OR() {
  return getToken(FilterExpressionSyntaxParser::K_OR, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_ISNULL() {
  return getToken(FilterExpressionSyntaxParser::K_ISNULL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_NOTNULL() {
  return getToken(FilterExpressionSyntaxParser::K_NOTNULL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExpressionContext::K_NULL() {
  return getToken(FilterExpressionSyntaxParser::K_NULL, 0);
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
    setState(118);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case FilterExpressionSyntaxParser::K_NULL:
      case FilterExpressionSyntaxParser::NUMERIC_LITERAL:
      case FilterExpressionSyntaxParser::STRING_LITERAL:
      case FilterExpressionSyntaxParser::DATETIME_LITERAL: {
        setState(94);
        literalValue();
        break;
      }

      case FilterExpressionSyntaxParser::IDENTIFIER: {
        setState(95);
        columnName();
        break;
      }

      case FilterExpressionSyntaxParser::T__6:
      case FilterExpressionSyntaxParser::T__7:
      case FilterExpressionSyntaxParser::T__22:
      case FilterExpressionSyntaxParser::K_NOT: {
        setState(96);
        unaryOperator();
        setState(97);
        expression(15);
        break;
      }

      case FilterExpressionSyntaxParser::K_CONVERT:
      case FilterExpressionSyntaxParser::K_IIF:
      case FilterExpressionSyntaxParser::K_LEN:
      case FilterExpressionSyntaxParser::K_REGEXP:
      case FilterExpressionSyntaxParser::K_SUBSTRING:
      case FilterExpressionSyntaxParser::K_TRIM: {
        setState(99);
        functionName();
        setState(100);
        match(FilterExpressionSyntaxParser::T__20);
        setState(110);
        _errHandler->sync(this);
        switch (_input->LA(1)) {
          case FilterExpressionSyntaxParser::T__6:
          case FilterExpressionSyntaxParser::T__7:
          case FilterExpressionSyntaxParser::T__20:
          case FilterExpressionSyntaxParser::T__22:
          case FilterExpressionSyntaxParser::K_CONVERT:
          case FilterExpressionSyntaxParser::K_IIF:
          case FilterExpressionSyntaxParser::K_LEN:
          case FilterExpressionSyntaxParser::K_NOT:
          case FilterExpressionSyntaxParser::K_NULL:
          case FilterExpressionSyntaxParser::K_REGEXP:
          case FilterExpressionSyntaxParser::K_SUBSTRING:
          case FilterExpressionSyntaxParser::K_TRIM:
          case FilterExpressionSyntaxParser::IDENTIFIER:
          case FilterExpressionSyntaxParser::NUMERIC_LITERAL:
          case FilterExpressionSyntaxParser::STRING_LITERAL:
          case FilterExpressionSyntaxParser::DATETIME_LITERAL: {
            setState(101);
            expression(0);
            setState(106);
            _errHandler->sync(this);
            _la = _input->LA(1);
            while (_la == FilterExpressionSyntaxParser::T__1) {
              setState(102);
              match(FilterExpressionSyntaxParser::T__1);
              setState(103);
              expression(0);
              setState(108);
              _errHandler->sync(this);
              _la = _input->LA(1);
            }
            break;
          }

          case FilterExpressionSyntaxParser::T__3: {
            setState(109);
            match(FilterExpressionSyntaxParser::T__3);
            break;
          }

          case FilterExpressionSyntaxParser::T__21: {
            break;
          }

        default:
          break;
        }
        setState(112);
        match(FilterExpressionSyntaxParser::T__21);
        break;
      }

      case FilterExpressionSyntaxParser::T__20: {
        setState(114);
        match(FilterExpressionSyntaxParser::T__20);
        setState(115);
        expression(0);
        setState(116);
        match(FilterExpressionSyntaxParser::T__21);
        break;
      }

    default:
      throw NoViableAltException(this);
    }
    _ctx->stop = _input->LT(-1);
    setState(192);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 21, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        if (!_parseListeners.empty())
          triggerExitRuleEvent();
        previousContext = _localctx;
        setState(190);
        _errHandler->sync(this);
        switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 20, _ctx)) {
        case 1: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(120);

          if (!(precpred(_ctx, 14))) throw FailedPredicateException(this, "precpred(_ctx, 14)");
          setState(121);
          match(FilterExpressionSyntaxParser::T__2);
          setState(122);
          expression(15);
          break;
        }

        case 2: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(123);

          if (!(precpred(_ctx, 13))) throw FailedPredicateException(this, "precpred(_ctx, 13)");
          setState(124);
          _la = _input->LA(1);
          if (!((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__3)
            | (1ULL << FilterExpressionSyntaxParser::T__4)
            | (1ULL << FilterExpressionSyntaxParser::T__5))) != 0))) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(125);
          expression(14);
          break;
        }

        case 3: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(126);

          if (!(precpred(_ctx, 12))) throw FailedPredicateException(this, "precpred(_ctx, 12)");
          setState(127);
          _la = _input->LA(1);
          if (!(_la == FilterExpressionSyntaxParser::T__6

          || _la == FilterExpressionSyntaxParser::T__7)) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(128);
          expression(13);
          break;
        }

        case 4: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(129);

          if (!(precpred(_ctx, 11))) throw FailedPredicateException(this, "precpred(_ctx, 11)");
          setState(130);
          _la = _input->LA(1);
          if (!((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__8)
            | (1ULL << FilterExpressionSyntaxParser::T__9)
            | (1ULL << FilterExpressionSyntaxParser::T__10)
            | (1ULL << FilterExpressionSyntaxParser::T__11))) != 0))) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(131);
          expression(12);
          break;
        }

        case 5: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(132);

          if (!(precpred(_ctx, 10))) throw FailedPredicateException(this, "precpred(_ctx, 10)");
          setState(133);
          _la = _input->LA(1);
          if (!((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__12)
            | (1ULL << FilterExpressionSyntaxParser::T__13)
            | (1ULL << FilterExpressionSyntaxParser::T__14)
            | (1ULL << FilterExpressionSyntaxParser::T__15))) != 0))) {
          _errHandler->recoverInline(this);
          }
          else {
            _errHandler->reportMatch(this);
            consume();
          }
          setState(134);
          expression(11);
          break;
        }

        case 6: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(135);

          if (!(precpred(_ctx, 9))) throw FailedPredicateException(this, "precpred(_ctx, 9)");
          setState(145);
          _errHandler->sync(this);
          switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 13, _ctx)) {
          case 1: {
            setState(136);
            match(FilterExpressionSyntaxParser::T__16);
            break;
          }

          case 2: {
            setState(137);
            match(FilterExpressionSyntaxParser::T__17);
            break;
          }

          case 3: {
            setState(138);
            match(FilterExpressionSyntaxParser::T__18);
            break;
          }

          case 4: {
            setState(139);
            match(FilterExpressionSyntaxParser::T__19);
            break;
          }

          case 5: {
            setState(140);
            match(FilterExpressionSyntaxParser::K_IS);
            break;
          }

          case 6: {
            setState(141);
            match(FilterExpressionSyntaxParser::K_IS);
            setState(142);
            match(FilterExpressionSyntaxParser::K_NOT);
            break;
          }

          case 7: {
            setState(143);
            match(FilterExpressionSyntaxParser::K_IN);
            break;
          }

          case 8: {
            setState(144);
            match(FilterExpressionSyntaxParser::K_LIKE);
            break;
          }

          }
          setState(147);
          expression(10);
          break;
        }

        case 7: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(148);

          if (!(precpred(_ctx, 8))) throw FailedPredicateException(this, "precpred(_ctx, 8)");
          setState(149);
          match(FilterExpressionSyntaxParser::K_AND);
          setState(150);
          expression(9);
          break;
        }

        case 8: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(151);

          if (!(precpred(_ctx, 7))) throw FailedPredicateException(this, "precpred(_ctx, 7)");
          setState(152);
          match(FilterExpressionSyntaxParser::K_OR);
          setState(153);
          expression(8);
          break;
        }

        case 9: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(154);

          if (!(precpred(_ctx, 4))) throw FailedPredicateException(this, "precpred(_ctx, 4)");
          setState(156);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::K_NOT) {
            setState(155);
            match(FilterExpressionSyntaxParser::K_NOT);
          }
          setState(158);
          match(FilterExpressionSyntaxParser::K_LIKE);
          setState(159);
          expression(5);
          break;
        }

        case 10: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(160);

          if (!(precpred(_ctx, 2))) throw FailedPredicateException(this, "precpred(_ctx, 2)");
          setState(161);
          match(FilterExpressionSyntaxParser::K_IS);
          setState(163);
          _errHandler->sync(this);

          switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 15, _ctx)) {
          case 1: {
            setState(162);
            match(FilterExpressionSyntaxParser::K_NOT);
            break;
          }

          }
          setState(165);
          expression(3);
          break;
        }

        case 11: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(166);

          if (!(precpred(_ctx, 3))) throw FailedPredicateException(this, "precpred(_ctx, 3)");
          setState(171);
          _errHandler->sync(this);
          switch (_input->LA(1)) {
            case FilterExpressionSyntaxParser::K_ISNULL: {
              setState(167);
              match(FilterExpressionSyntaxParser::K_ISNULL);
              break;
            }

            case FilterExpressionSyntaxParser::K_NOTNULL: {
              setState(168);
              match(FilterExpressionSyntaxParser::K_NOTNULL);
              break;
            }

            case FilterExpressionSyntaxParser::K_NOT: {
              setState(169);
              match(FilterExpressionSyntaxParser::K_NOT);
              setState(170);
              match(FilterExpressionSyntaxParser::K_NULL);
              break;
            }

          default:
            throw NoViableAltException(this);
          }
          break;
        }

        case 12: {
          _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleExpression);
          setState(173);

          if (!(precpred(_ctx, 1))) throw FailedPredicateException(this, "precpred(_ctx, 1)");
          setState(175);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::K_NOT) {
            setState(174);
            match(FilterExpressionSyntaxParser::K_NOT);
          }
          setState(177);
          match(FilterExpressionSyntaxParser::K_IN);

          setState(178);
          match(FilterExpressionSyntaxParser::T__20);
          setState(187);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if ((((_la & ~ 0x3fULL) == 0) &&
            ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__6)
            | (1ULL << FilterExpressionSyntaxParser::T__7)
            | (1ULL << FilterExpressionSyntaxParser::T__20)
            | (1ULL << FilterExpressionSyntaxParser::T__22)
            | (1ULL << FilterExpressionSyntaxParser::K_CONVERT)
            | (1ULL << FilterExpressionSyntaxParser::K_IIF)
            | (1ULL << FilterExpressionSyntaxParser::K_LEN)
            | (1ULL << FilterExpressionSyntaxParser::K_NOT)
            | (1ULL << FilterExpressionSyntaxParser::K_NULL)
            | (1ULL << FilterExpressionSyntaxParser::K_REGEXP)
            | (1ULL << FilterExpressionSyntaxParser::K_SUBSTRING)
            | (1ULL << FilterExpressionSyntaxParser::K_TRIM)
            | (1ULL << FilterExpressionSyntaxParser::IDENTIFIER)
            | (1ULL << FilterExpressionSyntaxParser::NUMERIC_LITERAL)
            | (1ULL << FilterExpressionSyntaxParser::STRING_LITERAL)
            | (1ULL << FilterExpressionSyntaxParser::DATETIME_LITERAL))) != 0)) {
            setState(179);
            expression(0);
            setState(184);
            _errHandler->sync(this);
            _la = _input->LA(1);
            while (_la == FilterExpressionSyntaxParser::T__1) {
              setState(180);
              match(FilterExpressionSyntaxParser::T__1);
              setState(181);
              expression(0);
              setState(186);
              _errHandler->sync(this);
              _la = _input->LA(1);
            }
          }
          setState(189);
          match(FilterExpressionSyntaxParser::T__21);
          break;
        }

        } 
      }
      setState(194);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 21, _ctx);
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

tree::TerminalNode* FilterExpressionSyntaxParser::LiteralValueContext::NUMERIC_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::NUMERIC_LITERAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::LiteralValueContext::STRING_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::STRING_LITERAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::LiteralValueContext::DATETIME_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::DATETIME_LITERAL, 0);
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
    setState(195);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::K_NULL)
      | (1ULL << FilterExpressionSyntaxParser::NUMERIC_LITERAL)
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
    setState(197);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__6)
      | (1ULL << FilterExpressionSyntaxParser::T__7)
      | (1ULL << FilterExpressionSyntaxParser::T__22)
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

//----------------- KeywordContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::KeywordContext::KeywordContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_AND() {
  return getToken(FilterExpressionSyntaxParser::K_AND, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_ASC() {
  return getToken(FilterExpressionSyntaxParser::K_ASC, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_BY() {
  return getToken(FilterExpressionSyntaxParser::K_BY, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_CONVERT() {
  return getToken(FilterExpressionSyntaxParser::K_CONVERT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_DESC() {
  return getToken(FilterExpressionSyntaxParser::K_DESC, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_FILTER() {
  return getToken(FilterExpressionSyntaxParser::K_FILTER, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_IIF() {
  return getToken(FilterExpressionSyntaxParser::K_IIF, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_IN() {
  return getToken(FilterExpressionSyntaxParser::K_IN, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_IS() {
  return getToken(FilterExpressionSyntaxParser::K_IS, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_ISNULL() {
  return getToken(FilterExpressionSyntaxParser::K_ISNULL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_LEN() {
  return getToken(FilterExpressionSyntaxParser::K_LEN, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_LIKE() {
  return getToken(FilterExpressionSyntaxParser::K_LIKE, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_NOT() {
  return getToken(FilterExpressionSyntaxParser::K_NOT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_NOTNULL() {
  return getToken(FilterExpressionSyntaxParser::K_NOTNULL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_NULL() {
  return getToken(FilterExpressionSyntaxParser::K_NULL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_OR() {
  return getToken(FilterExpressionSyntaxParser::K_OR, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_ORDER() {
  return getToken(FilterExpressionSyntaxParser::K_ORDER, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_REGEXP() {
  return getToken(FilterExpressionSyntaxParser::K_REGEXP, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_SUBSTRING() {
  return getToken(FilterExpressionSyntaxParser::K_SUBSTRING, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_TOP() {
  return getToken(FilterExpressionSyntaxParser::K_TOP, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_TRIM() {
  return getToken(FilterExpressionSyntaxParser::K_TRIM, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::KeywordContext::K_WHERE() {
  return getToken(FilterExpressionSyntaxParser::K_WHERE, 0);
}


size_t FilterExpressionSyntaxParser::KeywordContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleKeyword;
}

void FilterExpressionSyntaxParser::KeywordContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterKeyword(this);
}

void FilterExpressionSyntaxParser::KeywordContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitKeyword(this);
}

FilterExpressionSyntaxParser::KeywordContext* FilterExpressionSyntaxParser::keyword() {
  KeywordContext *_localctx = _tracker.createInstance<KeywordContext>(_ctx, getState());
  enterRule(_localctx, 20, FilterExpressionSyntaxParser::RuleKeyword);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(199);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::K_AND)
      | (1ULL << FilterExpressionSyntaxParser::K_ASC)
      | (1ULL << FilterExpressionSyntaxParser::K_BY)
      | (1ULL << FilterExpressionSyntaxParser::K_CONVERT)
      | (1ULL << FilterExpressionSyntaxParser::K_DESC)
      | (1ULL << FilterExpressionSyntaxParser::K_FILTER)
      | (1ULL << FilterExpressionSyntaxParser::K_IIF)
      | (1ULL << FilterExpressionSyntaxParser::K_IN)
      | (1ULL << FilterExpressionSyntaxParser::K_IS)
      | (1ULL << FilterExpressionSyntaxParser::K_ISNULL)
      | (1ULL << FilterExpressionSyntaxParser::K_LEN)
      | (1ULL << FilterExpressionSyntaxParser::K_LIKE)
      | (1ULL << FilterExpressionSyntaxParser::K_NOT)
      | (1ULL << FilterExpressionSyntaxParser::K_NOTNULL)
      | (1ULL << FilterExpressionSyntaxParser::K_NULL)
      | (1ULL << FilterExpressionSyntaxParser::K_OR)
      | (1ULL << FilterExpressionSyntaxParser::K_ORDER)
      | (1ULL << FilterExpressionSyntaxParser::K_REGEXP)
      | (1ULL << FilterExpressionSyntaxParser::K_SUBSTRING)
      | (1ULL << FilterExpressionSyntaxParser::K_TOP)
      | (1ULL << FilterExpressionSyntaxParser::K_TRIM)
      | (1ULL << FilterExpressionSyntaxParser::K_WHERE))) != 0))) {
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

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_CONVERT() {
  return getToken(FilterExpressionSyntaxParser::K_CONVERT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_IIF() {
  return getToken(FilterExpressionSyntaxParser::K_IIF, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_LEN() {
  return getToken(FilterExpressionSyntaxParser::K_LEN, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_REGEXP() {
  return getToken(FilterExpressionSyntaxParser::K_REGEXP, 0);
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
  enterRule(_localctx, 22, FilterExpressionSyntaxParser::RuleFunctionName);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(201);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::K_CONVERT)
      | (1ULL << FilterExpressionSyntaxParser::K_IIF)
      | (1ULL << FilterExpressionSyntaxParser::K_LEN)
      | (1ULL << FilterExpressionSyntaxParser::K_REGEXP)
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

//----------------- DatabaseNameContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::DatabaseNameContext::DatabaseNameContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::DatabaseNameContext::IDENTIFIER() {
  return getToken(FilterExpressionSyntaxParser::IDENTIFIER, 0);
}


size_t FilterExpressionSyntaxParser::DatabaseNameContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleDatabaseName;
}

void FilterExpressionSyntaxParser::DatabaseNameContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterDatabaseName(this);
}

void FilterExpressionSyntaxParser::DatabaseNameContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitDatabaseName(this);
}

FilterExpressionSyntaxParser::DatabaseNameContext* FilterExpressionSyntaxParser::databaseName() {
  DatabaseNameContext *_localctx = _tracker.createInstance<DatabaseNameContext>(_ctx, getState());
  enterRule(_localctx, 24, FilterExpressionSyntaxParser::RuleDatabaseName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(203);
    match(FilterExpressionSyntaxParser::IDENTIFIER);
   
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
  enterRule(_localctx, 26, FilterExpressionSyntaxParser::RuleTableName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(205);
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
  enterRule(_localctx, 28, FilterExpressionSyntaxParser::RuleColumnName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(207);
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
    case 0: return precpred(_ctx, 14);
    case 1: return precpred(_ctx, 13);
    case 2: return precpred(_ctx, 12);
    case 3: return precpred(_ctx, 11);
    case 4: return precpred(_ctx, 10);
    case 5: return precpred(_ctx, 9);
    case 6: return precpred(_ctx, 8);
    case 7: return precpred(_ctx, 7);
    case 8: return precpred(_ctx, 4);
    case 9: return precpred(_ctx, 2);
    case 10: return precpred(_ctx, 3);
    case 11: return precpred(_ctx, 1);

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
  "literalValue", "unaryOperator", "keyword", "functionName", "databaseName", 
  "tableName", "columnName"
};

std::vector<std::string> FilterExpressionSyntaxParser::_literalNames = {
  "", "';'", "','", "'||'", "'*'", "'/'", "'%'", "'+'", "'-'", "'<<'", "'>>'", 
  "'&'", "'|'", "'<'", "'<='", "'>'", "'>='", "'='", "'=='", "'!='", "'<>'", 
  "'('", "')'", "'~'"
};

std::vector<std::string> FilterExpressionSyntaxParser::_symbolicNames = {
  "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
  "", "", "", "", "", "", "K_AND", "K_ASC", "K_BY", "K_CONVERT", "K_DESC", 
  "K_FILTER", "K_IIF", "K_IN", "K_IS", "K_ISNULL", "K_LEN", "K_LIKE", "K_NOT", 
  "K_NOTNULL", "K_NULL", "K_OR", "K_ORDER", "K_REGEXP", "K_SUBSTRING", "K_TOP", 
  "K_TRIM", "K_WHERE", "IDENTIFIER", "INTEGER_LITERAL", "NUMERIC_LITERAL", 
  "STRING_LITERAL", "DATETIME_LITERAL", "GUID_VALUE", "GUID_LITERAL", "MEASUREMENT_KEY_LITERAL", 
  "POINT_TAG_LITERAL", "SINGLE_LINE_COMMENT", "MULTILINE_COMMENT", "SPACES", 
  "UNEXPECTED_CHAR"
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
    0x3, 0x3c, 0xd4, 0x4, 0x2, 0x9, 0x2, 0x4, 0x3, 0x9, 0x3, 0x4, 0x4, 0x9, 
    0x4, 0x4, 0x5, 0x9, 0x5, 0x4, 0x6, 0x9, 0x6, 0x4, 0x7, 0x9, 0x7, 0x4, 
    0x8, 0x9, 0x8, 0x4, 0x9, 0x9, 0x9, 0x4, 0xa, 0x9, 0xa, 0x4, 0xb, 0x9, 
    0xb, 0x4, 0xc, 0x9, 0xc, 0x4, 0xd, 0x9, 0xd, 0x4, 0xe, 0x9, 0xe, 0x4, 
    0xf, 0x9, 0xf, 0x4, 0x10, 0x9, 0x10, 0x3, 0x2, 0x3, 0x2, 0x5, 0x2, 0x23, 
    0xa, 0x2, 0x3, 0x2, 0x3, 0x2, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x4, 
    0x7, 0x4, 0x2b, 0xa, 0x4, 0xc, 0x4, 0xe, 0x4, 0x2e, 0xb, 0x4, 0x3, 0x4, 
    0x3, 0x4, 0x6, 0x4, 0x32, 0xa, 0x4, 0xd, 0x4, 0xe, 0x4, 0x33, 0x3, 0x4, 
    0x7, 0x4, 0x37, 0xa, 0x4, 0xc, 0x4, 0xe, 0x4, 0x3a, 0xb, 0x4, 0x3, 0x4, 
    0x7, 0x4, 0x3d, 0xa, 0x4, 0xc, 0x4, 0xe, 0x4, 0x40, 0xb, 0x4, 0x3, 0x5, 
    0x3, 0x5, 0x5, 0x5, 0x44, 0xa, 0x5, 0x3, 0x6, 0x3, 0x6, 0x3, 0x7, 0x3, 
    0x7, 0x3, 0x7, 0x5, 0x7, 0x4b, 0xa, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 
    0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x7, 0x7, 0x55, 0xa, 
    0x7, 0xc, 0x7, 0xe, 0x7, 0x58, 0xb, 0x7, 0x5, 0x7, 0x5a, 0xa, 0x7, 0x3, 
    0x8, 0x3, 0x8, 0x5, 0x8, 0x5e, 0xa, 0x8, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 
    0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 
    0x3, 0x9, 0x7, 0x9, 0x6b, 0xa, 0x9, 0xc, 0x9, 0xe, 0x9, 0x6e, 0xb, 0x9, 
    0x3, 0x9, 0x5, 0x9, 0x71, 0xa, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 
    0x9, 0x3, 0x9, 0x3, 0x9, 0x5, 0x9, 0x79, 0xa, 0x9, 0x3, 0x9, 0x3, 0x9, 
    0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 
    0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 
    0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 
    0x3, 0x9, 0x3, 0x9, 0x5, 0x9, 0x94, 0xa, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 
    0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x5, 
    0x9, 0x9f, 0xa, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 
    0x5, 0x9, 0xa6, 0xa, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 
    0x9, 0x3, 0x9, 0x5, 0x9, 0xae, 0xa, 0x9, 0x3, 0x9, 0x3, 0x9, 0x5, 0x9, 
    0xb2, 0xa, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x3, 0x9, 0x7, 
    0x9, 0xb9, 0xa, 0x9, 0xc, 0x9, 0xe, 0x9, 0xbc, 0xb, 0x9, 0x5, 0x9, 0xbe, 
    0xa, 0x9, 0x3, 0x9, 0x7, 0x9, 0xc1, 0xa, 0x9, 0xc, 0x9, 0xe, 0x9, 0xc4, 
    0xb, 0x9, 0x3, 0xa, 0x3, 0xa, 0x3, 0xb, 0x3, 0xb, 0x3, 0xc, 0x3, 0xc, 
    0x3, 0xd, 0x3, 0xd, 0x3, 0xe, 0x3, 0xe, 0x3, 0xf, 0x3, 0xf, 0x3, 0x10, 
    0x3, 0x10, 0x3, 0x10, 0x2, 0x3, 0x10, 0x11, 0x2, 0x4, 0x6, 0x8, 0xa, 
    0xc, 0xe, 0x10, 0x12, 0x14, 0x16, 0x18, 0x1a, 0x1c, 0x1e, 0x2, 0xc, 
    0x3, 0x2, 0x36, 0x38, 0x4, 0x2, 0x1b, 0x1b, 0x1e, 0x1e, 0x3, 0x2, 0x6, 
    0x8, 0x3, 0x2, 0x9, 0xa, 0x3, 0x2, 0xb, 0xe, 0x3, 0x2, 0xf, 0x12, 0x4, 
    0x2, 0x28, 0x28, 0x32, 0x34, 0x5, 0x2, 0x9, 0xa, 0x19, 0x19, 0x26, 0x26, 
    0x3, 0x2, 0x1a, 0x2f, 0x7, 0x2, 0x1d, 0x1d, 0x20, 0x20, 0x24, 0x24, 
    0x2b, 0x2c, 0x2e, 0x2e, 0x2, 0xef, 0x2, 0x22, 0x3, 0x2, 0x2, 0x2, 0x4, 
    0x26, 0x3, 0x2, 0x2, 0x2, 0x6, 0x2c, 0x3, 0x2, 0x2, 0x2, 0x8, 0x43, 
    0x3, 0x2, 0x2, 0x2, 0xa, 0x45, 0x3, 0x2, 0x2, 0x2, 0xc, 0x47, 0x3, 0x2, 
    0x2, 0x2, 0xe, 0x5b, 0x3, 0x2, 0x2, 0x2, 0x10, 0x78, 0x3, 0x2, 0x2, 
    0x2, 0x12, 0xc5, 0x3, 0x2, 0x2, 0x2, 0x14, 0xc7, 0x3, 0x2, 0x2, 0x2, 
    0x16, 0xc9, 0x3, 0x2, 0x2, 0x2, 0x18, 0xcb, 0x3, 0x2, 0x2, 0x2, 0x1a, 
    0xcd, 0x3, 0x2, 0x2, 0x2, 0x1c, 0xcf, 0x3, 0x2, 0x2, 0x2, 0x1e, 0xd1, 
    0x3, 0x2, 0x2, 0x2, 0x20, 0x23, 0x5, 0x6, 0x4, 0x2, 0x21, 0x23, 0x5, 
    0x4, 0x3, 0x2, 0x22, 0x20, 0x3, 0x2, 0x2, 0x2, 0x22, 0x21, 0x3, 0x2, 
    0x2, 0x2, 0x23, 0x24, 0x3, 0x2, 0x2, 0x2, 0x24, 0x25, 0x7, 0x2, 0x2, 
    0x3, 0x25, 0x3, 0x3, 0x2, 0x2, 0x2, 0x26, 0x27, 0x7, 0x3c, 0x2, 0x2, 
    0x27, 0x28, 0x8, 0x3, 0x1, 0x2, 0x28, 0x5, 0x3, 0x2, 0x2, 0x2, 0x29, 
    0x2b, 0x7, 0x3, 0x2, 0x2, 0x2a, 0x29, 0x3, 0x2, 0x2, 0x2, 0x2b, 0x2e, 
    0x3, 0x2, 0x2, 0x2, 0x2c, 0x2a, 0x3, 0x2, 0x2, 0x2, 0x2c, 0x2d, 0x3, 
    0x2, 0x2, 0x2, 0x2d, 0x2f, 0x3, 0x2, 0x2, 0x2, 0x2e, 0x2c, 0x3, 0x2, 
    0x2, 0x2, 0x2f, 0x38, 0x5, 0x8, 0x5, 0x2, 0x30, 0x32, 0x7, 0x3, 0x2, 
    0x2, 0x31, 0x30, 0x3, 0x2, 0x2, 0x2, 0x32, 0x33, 0x3, 0x2, 0x2, 0x2, 
    0x33, 0x31, 0x3, 0x2, 0x2, 0x2, 0x33, 0x34, 0x3, 0x2, 0x2, 0x2, 0x34, 
    0x35, 0x3, 0x2, 0x2, 0x2, 0x35, 0x37, 0x5, 0x8, 0x5, 0x2, 0x36, 0x31, 
    0x3, 0x2, 0x2, 0x2, 0x37, 0x3a, 0x3, 0x2, 0x2, 0x2, 0x38, 0x36, 0x3, 
    0x2, 0x2, 0x2, 0x38, 0x39, 0x3, 0x2, 0x2, 0x2, 0x39, 0x3e, 0x3, 0x2, 
    0x2, 0x2, 0x3a, 0x38, 0x3, 0x2, 0x2, 0x2, 0x3b, 0x3d, 0x7, 0x3, 0x2, 
    0x2, 0x3c, 0x3b, 0x3, 0x2, 0x2, 0x2, 0x3d, 0x40, 0x3, 0x2, 0x2, 0x2, 
    0x3e, 0x3c, 0x3, 0x2, 0x2, 0x2, 0x3e, 0x3f, 0x3, 0x2, 0x2, 0x2, 0x3f, 
    0x7, 0x3, 0x2, 0x2, 0x2, 0x40, 0x3e, 0x3, 0x2, 0x2, 0x2, 0x41, 0x44, 
    0x5, 0xc, 0x7, 0x2, 0x42, 0x44, 0x5, 0xa, 0x6, 0x2, 0x43, 0x41, 0x3, 
    0x2, 0x2, 0x2, 0x43, 0x42, 0x3, 0x2, 0x2, 0x2, 0x44, 0x9, 0x3, 0x2, 
    0x2, 0x2, 0x45, 0x46, 0x9, 0x2, 0x2, 0x2, 0x46, 0xb, 0x3, 0x2, 0x2, 
    0x2, 0x47, 0x4a, 0x7, 0x1f, 0x2, 0x2, 0x48, 0x49, 0x7, 0x2d, 0x2, 0x2, 
    0x49, 0x4b, 0x7, 0x31, 0x2, 0x2, 0x4a, 0x48, 0x3, 0x2, 0x2, 0x2, 0x4a, 
    0x4b, 0x3, 0x2, 0x2, 0x2, 0x4b, 0x4c, 0x3, 0x2, 0x2, 0x2, 0x4c, 0x4d, 
    0x5, 0x1c, 0xf, 0x2, 0x4d, 0x4e, 0x7, 0x2f, 0x2, 0x2, 0x4e, 0x59, 0x5, 
    0x10, 0x9, 0x2, 0x4f, 0x50, 0x7, 0x2a, 0x2, 0x2, 0x50, 0x51, 0x7, 0x1c, 
    0x2, 0x2, 0x51, 0x56, 0x5, 0xe, 0x8, 0x2, 0x52, 0x53, 0x7, 0x4, 0x2, 
    0x2, 0x53, 0x55, 0x5, 0xe, 0x8, 0x2, 0x54, 0x52, 0x3, 0x2, 0x2, 0x2, 
    0x55, 0x58, 0x3, 0x2, 0x2, 0x2, 0x56, 0x54, 0x3, 0x2, 0x2, 0x2, 0x56, 
    0x57, 0x3, 0x2, 0x2, 0x2, 0x57, 0x5a, 0x3, 0x2, 0x2, 0x2, 0x58, 0x56, 
    0x3, 0x2, 0x2, 0x2, 0x59, 0x4f, 0x3, 0x2, 0x2, 0x2, 0x59, 0x5a, 0x3, 
    0x2, 0x2, 0x2, 0x5a, 0xd, 0x3, 0x2, 0x2, 0x2, 0x5b, 0x5d, 0x5, 0x1e, 
    0x10, 0x2, 0x5c, 0x5e, 0x9, 0x3, 0x2, 0x2, 0x5d, 0x5c, 0x3, 0x2, 0x2, 
    0x2, 0x5d, 0x5e, 0x3, 0x2, 0x2, 0x2, 0x5e, 0xf, 0x3, 0x2, 0x2, 0x2, 
    0x5f, 0x60, 0x8, 0x9, 0x1, 0x2, 0x60, 0x79, 0x5, 0x12, 0xa, 0x2, 0x61, 
    0x79, 0x5, 0x1e, 0x10, 0x2, 0x62, 0x63, 0x5, 0x14, 0xb, 0x2, 0x63, 0x64, 
    0x5, 0x10, 0x9, 0x11, 0x64, 0x79, 0x3, 0x2, 0x2, 0x2, 0x65, 0x66, 0x5, 
    0x18, 0xd, 0x2, 0x66, 0x70, 0x7, 0x17, 0x2, 0x2, 0x67, 0x6c, 0x5, 0x10, 
    0x9, 0x2, 0x68, 0x69, 0x7, 0x4, 0x2, 0x2, 0x69, 0x6b, 0x5, 0x10, 0x9, 
    0x2, 0x6a, 0x68, 0x3, 0x2, 0x2, 0x2, 0x6b, 0x6e, 0x3, 0x2, 0x2, 0x2, 
    0x6c, 0x6a, 0x3, 0x2, 0x2, 0x2, 0x6c, 0x6d, 0x3, 0x2, 0x2, 0x2, 0x6d, 
    0x71, 0x3, 0x2, 0x2, 0x2, 0x6e, 0x6c, 0x3, 0x2, 0x2, 0x2, 0x6f, 0x71, 
    0x7, 0x6, 0x2, 0x2, 0x70, 0x67, 0x3, 0x2, 0x2, 0x2, 0x70, 0x6f, 0x3, 
    0x2, 0x2, 0x2, 0x70, 0x71, 0x3, 0x2, 0x2, 0x2, 0x71, 0x72, 0x3, 0x2, 
    0x2, 0x2, 0x72, 0x73, 0x7, 0x18, 0x2, 0x2, 0x73, 0x79, 0x3, 0x2, 0x2, 
    0x2, 0x74, 0x75, 0x7, 0x17, 0x2, 0x2, 0x75, 0x76, 0x5, 0x10, 0x9, 0x2, 
    0x76, 0x77, 0x7, 0x18, 0x2, 0x2, 0x77, 0x79, 0x3, 0x2, 0x2, 0x2, 0x78, 
    0x5f, 0x3, 0x2, 0x2, 0x2, 0x78, 0x61, 0x3, 0x2, 0x2, 0x2, 0x78, 0x62, 
    0x3, 0x2, 0x2, 0x2, 0x78, 0x65, 0x3, 0x2, 0x2, 0x2, 0x78, 0x74, 0x3, 
    0x2, 0x2, 0x2, 0x79, 0xc2, 0x3, 0x2, 0x2, 0x2, 0x7a, 0x7b, 0xc, 0x10, 
    0x2, 0x2, 0x7b, 0x7c, 0x7, 0x5, 0x2, 0x2, 0x7c, 0xc1, 0x5, 0x10, 0x9, 
    0x11, 0x7d, 0x7e, 0xc, 0xf, 0x2, 0x2, 0x7e, 0x7f, 0x9, 0x4, 0x2, 0x2, 
    0x7f, 0xc1, 0x5, 0x10, 0x9, 0x10, 0x80, 0x81, 0xc, 0xe, 0x2, 0x2, 0x81, 
    0x82, 0x9, 0x5, 0x2, 0x2, 0x82, 0xc1, 0x5, 0x10, 0x9, 0xf, 0x83, 0x84, 
    0xc, 0xd, 0x2, 0x2, 0x84, 0x85, 0x9, 0x6, 0x2, 0x2, 0x85, 0xc1, 0x5, 
    0x10, 0x9, 0xe, 0x86, 0x87, 0xc, 0xc, 0x2, 0x2, 0x87, 0x88, 0x9, 0x7, 
    0x2, 0x2, 0x88, 0xc1, 0x5, 0x10, 0x9, 0xd, 0x89, 0x93, 0xc, 0xb, 0x2, 
    0x2, 0x8a, 0x94, 0x7, 0x13, 0x2, 0x2, 0x8b, 0x94, 0x7, 0x14, 0x2, 0x2, 
    0x8c, 0x94, 0x7, 0x15, 0x2, 0x2, 0x8d, 0x94, 0x7, 0x16, 0x2, 0x2, 0x8e, 
    0x94, 0x7, 0x22, 0x2, 0x2, 0x8f, 0x90, 0x7, 0x22, 0x2, 0x2, 0x90, 0x94, 
    0x7, 0x26, 0x2, 0x2, 0x91, 0x94, 0x7, 0x21, 0x2, 0x2, 0x92, 0x94, 0x7, 
    0x25, 0x2, 0x2, 0x93, 0x8a, 0x3, 0x2, 0x2, 0x2, 0x93, 0x8b, 0x3, 0x2, 
    0x2, 0x2, 0x93, 0x8c, 0x3, 0x2, 0x2, 0x2, 0x93, 0x8d, 0x3, 0x2, 0x2, 
    0x2, 0x93, 0x8e, 0x3, 0x2, 0x2, 0x2, 0x93, 0x8f, 0x3, 0x2, 0x2, 0x2, 
    0x93, 0x91, 0x3, 0x2, 0x2, 0x2, 0x93, 0x92, 0x3, 0x2, 0x2, 0x2, 0x94, 
    0x95, 0x3, 0x2, 0x2, 0x2, 0x95, 0xc1, 0x5, 0x10, 0x9, 0xc, 0x96, 0x97, 
    0xc, 0xa, 0x2, 0x2, 0x97, 0x98, 0x7, 0x1a, 0x2, 0x2, 0x98, 0xc1, 0x5, 
    0x10, 0x9, 0xb, 0x99, 0x9a, 0xc, 0x9, 0x2, 0x2, 0x9a, 0x9b, 0x7, 0x29, 
    0x2, 0x2, 0x9b, 0xc1, 0x5, 0x10, 0x9, 0xa, 0x9c, 0x9e, 0xc, 0x6, 0x2, 
    0x2, 0x9d, 0x9f, 0x7, 0x26, 0x2, 0x2, 0x9e, 0x9d, 0x3, 0x2, 0x2, 0x2, 
    0x9e, 0x9f, 0x3, 0x2, 0x2, 0x2, 0x9f, 0xa0, 0x3, 0x2, 0x2, 0x2, 0xa0, 
    0xa1, 0x7, 0x25, 0x2, 0x2, 0xa1, 0xc1, 0x5, 0x10, 0x9, 0x7, 0xa2, 0xa3, 
    0xc, 0x4, 0x2, 0x2, 0xa3, 0xa5, 0x7, 0x22, 0x2, 0x2, 0xa4, 0xa6, 0x7, 
    0x26, 0x2, 0x2, 0xa5, 0xa4, 0x3, 0x2, 0x2, 0x2, 0xa5, 0xa6, 0x3, 0x2, 
    0x2, 0x2, 0xa6, 0xa7, 0x3, 0x2, 0x2, 0x2, 0xa7, 0xc1, 0x5, 0x10, 0x9, 
    0x5, 0xa8, 0xad, 0xc, 0x5, 0x2, 0x2, 0xa9, 0xae, 0x7, 0x23, 0x2, 0x2, 
    0xaa, 0xae, 0x7, 0x27, 0x2, 0x2, 0xab, 0xac, 0x7, 0x26, 0x2, 0x2, 0xac, 
    0xae, 0x7, 0x28, 0x2, 0x2, 0xad, 0xa9, 0x3, 0x2, 0x2, 0x2, 0xad, 0xaa, 
    0x3, 0x2, 0x2, 0x2, 0xad, 0xab, 0x3, 0x2, 0x2, 0x2, 0xae, 0xc1, 0x3, 
    0x2, 0x2, 0x2, 0xaf, 0xb1, 0xc, 0x3, 0x2, 0x2, 0xb0, 0xb2, 0x7, 0x26, 
    0x2, 0x2, 0xb1, 0xb0, 0x3, 0x2, 0x2, 0x2, 0xb1, 0xb2, 0x3, 0x2, 0x2, 
    0x2, 0xb2, 0xb3, 0x3, 0x2, 0x2, 0x2, 0xb3, 0xb4, 0x7, 0x21, 0x2, 0x2, 
    0xb4, 0xbd, 0x7, 0x17, 0x2, 0x2, 0xb5, 0xba, 0x5, 0x10, 0x9, 0x2, 0xb6, 
    0xb7, 0x7, 0x4, 0x2, 0x2, 0xb7, 0xb9, 0x5, 0x10, 0x9, 0x2, 0xb8, 0xb6, 
    0x3, 0x2, 0x2, 0x2, 0xb9, 0xbc, 0x3, 0x2, 0x2, 0x2, 0xba, 0xb8, 0x3, 
    0x2, 0x2, 0x2, 0xba, 0xbb, 0x3, 0x2, 0x2, 0x2, 0xbb, 0xbe, 0x3, 0x2, 
    0x2, 0x2, 0xbc, 0xba, 0x3, 0x2, 0x2, 0x2, 0xbd, 0xb5, 0x3, 0x2, 0x2, 
    0x2, 0xbd, 0xbe, 0x3, 0x2, 0x2, 0x2, 0xbe, 0xbf, 0x3, 0x2, 0x2, 0x2, 
    0xbf, 0xc1, 0x7, 0x18, 0x2, 0x2, 0xc0, 0x7a, 0x3, 0x2, 0x2, 0x2, 0xc0, 
    0x7d, 0x3, 0x2, 0x2, 0x2, 0xc0, 0x80, 0x3, 0x2, 0x2, 0x2, 0xc0, 0x83, 
    0x3, 0x2, 0x2, 0x2, 0xc0, 0x86, 0x3, 0x2, 0x2, 0x2, 0xc0, 0x89, 0x3, 
    0x2, 0x2, 0x2, 0xc0, 0x96, 0x3, 0x2, 0x2, 0x2, 0xc0, 0x99, 0x3, 0x2, 
    0x2, 0x2, 0xc0, 0x9c, 0x3, 0x2, 0x2, 0x2, 0xc0, 0xa2, 0x3, 0x2, 0x2, 
    0x2, 0xc0, 0xa8, 0x3, 0x2, 0x2, 0x2, 0xc0, 0xaf, 0x3, 0x2, 0x2, 0x2, 
    0xc1, 0xc4, 0x3, 0x2, 0x2, 0x2, 0xc2, 0xc0, 0x3, 0x2, 0x2, 0x2, 0xc2, 
    0xc3, 0x3, 0x2, 0x2, 0x2, 0xc3, 0x11, 0x3, 0x2, 0x2, 0x2, 0xc4, 0xc2, 
    0x3, 0x2, 0x2, 0x2, 0xc5, 0xc6, 0x9, 0x8, 0x2, 0x2, 0xc6, 0x13, 0x3, 
    0x2, 0x2, 0x2, 0xc7, 0xc8, 0x9, 0x9, 0x2, 0x2, 0xc8, 0x15, 0x3, 0x2, 
    0x2, 0x2, 0xc9, 0xca, 0x9, 0xa, 0x2, 0x2, 0xca, 0x17, 0x3, 0x2, 0x2, 
    0x2, 0xcb, 0xcc, 0x9, 0xb, 0x2, 0x2, 0xcc, 0x19, 0x3, 0x2, 0x2, 0x2, 
    0xcd, 0xce, 0x7, 0x30, 0x2, 0x2, 0xce, 0x1b, 0x3, 0x2, 0x2, 0x2, 0xcf, 
    0xd0, 0x7, 0x30, 0x2, 0x2, 0xd0, 0x1d, 0x3, 0x2, 0x2, 0x2, 0xd1, 0xd2, 
    0x7, 0x30, 0x2, 0x2, 0xd2, 0x1f, 0x3, 0x2, 0x2, 0x2, 0x18, 0x22, 0x2c, 
    0x33, 0x38, 0x3e, 0x43, 0x4a, 0x56, 0x59, 0x5d, 0x6c, 0x70, 0x78, 0x93, 
    0x9e, 0xa5, 0xad, 0xb1, 0xba, 0xbd, 0xc0, 0xc2, 
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
