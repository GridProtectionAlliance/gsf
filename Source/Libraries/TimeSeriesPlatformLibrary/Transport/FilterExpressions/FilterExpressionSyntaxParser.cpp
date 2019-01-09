
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
    setState(50);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case FilterExpressionSyntaxParser::T__0:
      case FilterExpressionSyntaxParser::K_FILTER:
      case FilterExpressionSyntaxParser::GUID_LITERAL:
      case FilterExpressionSyntaxParser::MEASUREMENT_KEY_LITERAL:
      case FilterExpressionSyntaxParser::POINT_TAG_LITERAL: {
        setState(48);
        filterExpressionStatementList();
        break;
      }

      case FilterExpressionSyntaxParser::UNEXPECTED_CHAR: {
        setState(49);
        error();
        break;
      }

    default:
      throw NoViableAltException(this);
    }
    setState(52);
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
    setState(54);
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
    setState(63);
    filterExpressionStatement();
    setState(72);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 3, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        setState(65); 
        _errHandler->sync(this);
        _la = _input->LA(1);
        do {
          setState(64);
          match(FilterExpressionSyntaxParser::T__0);
          setState(67); 
          _errHandler->sync(this);
          _la = _input->LA(1);
        } while (_la == FilterExpressionSyntaxParser::T__0);
        setState(69);
        filterExpressionStatement(); 
      }
      setState(74);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 3, _ctx);
    }
    setState(78);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while (_la == FilterExpressionSyntaxParser::T__0) {
      setState(75);
      match(FilterExpressionSyntaxParser::T__0);
      setState(80);
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
    setState(83);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case FilterExpressionSyntaxParser::GUID_LITERAL:
      case FilterExpressionSyntaxParser::MEASUREMENT_KEY_LITERAL:
      case FilterExpressionSyntaxParser::POINT_TAG_LITERAL: {
        enterOuterAlt(_localctx, 1);
        setState(81);
        identifierStatement();
        break;
      }

      case FilterExpressionSyntaxParser::K_FILTER: {
        enterOuterAlt(_localctx, 2);
        setState(82);
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
    setState(85);
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

FilterExpressionSyntaxParser::TopLimitContext* FilterExpressionSyntaxParser::FilterStatementContext::topLimit() {
  return getRuleContext<FilterExpressionSyntaxParser::TopLimitContext>(0);
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
    setState(87);
    match(FilterExpressionSyntaxParser::K_FILTER);
    setState(90);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_TOP) {
      setState(88);
      match(FilterExpressionSyntaxParser::K_TOP);
      setState(89);
      topLimit();
    }
    setState(92);
    tableName();
    setState(93);
    match(FilterExpressionSyntaxParser::K_WHERE);
    setState(94);
    expression(0);
    setState(105);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_ORDER) {
      setState(95);
      match(FilterExpressionSyntaxParser::K_ORDER);
      setState(96);
      match(FilterExpressionSyntaxParser::K_BY);
      setState(97);
      orderingTerm();
      setState(102);
      _errHandler->sync(this);
      _la = _input->LA(1);
      while (_la == FilterExpressionSyntaxParser::T__1) {
        setState(98);
        match(FilterExpressionSyntaxParser::T__1);
        setState(99);
        orderingTerm();
        setState(104);
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

//----------------- TopLimitContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::TopLimitContext::TopLimitContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::TopLimitContext::INTEGER_LITERAL() {
  return getToken(FilterExpressionSyntaxParser::INTEGER_LITERAL, 0);
}


size_t FilterExpressionSyntaxParser::TopLimitContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleTopLimit;
}

void FilterExpressionSyntaxParser::TopLimitContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterTopLimit(this);
}

void FilterExpressionSyntaxParser::TopLimitContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitTopLimit(this);
}

FilterExpressionSyntaxParser::TopLimitContext* FilterExpressionSyntaxParser::topLimit() {
  TopLimitContext *_localctx = _tracker.createInstance<TopLimitContext>(_ctx, getState());
  enterRule(_localctx, 12, FilterExpressionSyntaxParser::RuleTopLimit);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(108);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::T__2

    || _la == FilterExpressionSyntaxParser::T__3) {
      setState(107);
      _la = _input->LA(1);
      if (!(_la == FilterExpressionSyntaxParser::T__2

      || _la == FilterExpressionSyntaxParser::T__3)) {
      _errHandler->recoverInline(this);
      }
      else {
        _errHandler->reportMatch(this);
        consume();
      }
    }
    setState(110);
    match(FilterExpressionSyntaxParser::INTEGER_LITERAL);
   
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
  enterRule(_localctx, 14, FilterExpressionSyntaxParser::RuleOrderingTerm);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(112);
    orderByColumnName();
    setState(114);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_ASC

    || _la == FilterExpressionSyntaxParser::K_DESC) {
      setState(113);
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

//----------------- ExpressionListContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::ExpressionListContext::ExpressionListContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

std::vector<FilterExpressionSyntaxParser::ExpressionContext *> FilterExpressionSyntaxParser::ExpressionListContext::expression() {
  return getRuleContexts<FilterExpressionSyntaxParser::ExpressionContext>();
}

FilterExpressionSyntaxParser::ExpressionContext* FilterExpressionSyntaxParser::ExpressionListContext::expression(size_t i) {
  return getRuleContext<FilterExpressionSyntaxParser::ExpressionContext>(i);
}


size_t FilterExpressionSyntaxParser::ExpressionListContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleExpressionList;
}

void FilterExpressionSyntaxParser::ExpressionListContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterExpressionList(this);
}

void FilterExpressionSyntaxParser::ExpressionListContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitExpressionList(this);
}

FilterExpressionSyntaxParser::ExpressionListContext* FilterExpressionSyntaxParser::expressionList() {
  ExpressionListContext *_localctx = _tracker.createInstance<ExpressionListContext>(_ctx, getState());
  enterRule(_localctx, 16, FilterExpressionSyntaxParser::RuleExpressionList);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(116);
    expression(0);
    setState(121);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while (_la == FilterExpressionSyntaxParser::T__1) {
      setState(117);
      match(FilterExpressionSyntaxParser::T__1);
      setState(118);
      expression(0);
      setState(123);
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

//----------------- ExpressionContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::ExpressionContext::ExpressionContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

FilterExpressionSyntaxParser::NotOperatorContext* FilterExpressionSyntaxParser::ExpressionContext::notOperator() {
  return getRuleContext<FilterExpressionSyntaxParser::NotOperatorContext>(0);
}

std::vector<FilterExpressionSyntaxParser::ExpressionContext *> FilterExpressionSyntaxParser::ExpressionContext::expression() {
  return getRuleContexts<FilterExpressionSyntaxParser::ExpressionContext>();
}

FilterExpressionSyntaxParser::ExpressionContext* FilterExpressionSyntaxParser::ExpressionContext::expression(size_t i) {
  return getRuleContext<FilterExpressionSyntaxParser::ExpressionContext>(i);
}

FilterExpressionSyntaxParser::PredicateExpressionContext* FilterExpressionSyntaxParser::ExpressionContext::predicateExpression() {
  return getRuleContext<FilterExpressionSyntaxParser::PredicateExpressionContext>(0);
}

FilterExpressionSyntaxParser::LogicalOperatorContext* FilterExpressionSyntaxParser::ExpressionContext::logicalOperator() {
  return getRuleContext<FilterExpressionSyntaxParser::LogicalOperatorContext>(0);
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
  size_t startState = 18;
  enterRecursionRule(_localctx, 18, FilterExpressionSyntaxParser::RuleExpression, precedence);

    

  auto onExit = finally([=] {
    unrollRecursionContexts(parentContext);
  });
  try {
    size_t alt;
    enterOuterAlt(_localctx, 1);
    setState(129);
    _errHandler->sync(this);
    switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 12, _ctx)) {
    case 1: {
      setState(125);
      notOperator();
      setState(126);
      expression(3);
      break;
    }

    case 2: {
      setState(128);
      predicateExpression(0);
      break;
    }

    }
    _ctx->stop = _input->LT(-1);
    setState(137);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 13, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        if (!_parseListeners.empty())
          triggerExitRuleEvent();
        previousContext = _localctx;
        _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
        pushNewRecursionContext(_localctx, startState, RuleExpression);
        setState(131);

        if (!(precpred(_ctx, 2))) throw FailedPredicateException(this, "precpred(_ctx, 2)");
        setState(132);
        logicalOperator();
        setState(133);
        expression(3); 
      }
      setState(139);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 13, _ctx);
    }
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }
  return _localctx;
}

//----------------- PredicateExpressionContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::PredicateExpressionContext::PredicateExpressionContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

FilterExpressionSyntaxParser::ValueExpressionContext* FilterExpressionSyntaxParser::PredicateExpressionContext::valueExpression() {
  return getRuleContext<FilterExpressionSyntaxParser::ValueExpressionContext>(0);
}

std::vector<FilterExpressionSyntaxParser::PredicateExpressionContext *> FilterExpressionSyntaxParser::PredicateExpressionContext::predicateExpression() {
  return getRuleContexts<FilterExpressionSyntaxParser::PredicateExpressionContext>();
}

FilterExpressionSyntaxParser::PredicateExpressionContext* FilterExpressionSyntaxParser::PredicateExpressionContext::predicateExpression(size_t i) {
  return getRuleContext<FilterExpressionSyntaxParser::PredicateExpressionContext>(i);
}

FilterExpressionSyntaxParser::ComparisonOperatorContext* FilterExpressionSyntaxParser::PredicateExpressionContext::comparisonOperator() {
  return getRuleContext<FilterExpressionSyntaxParser::ComparisonOperatorContext>(0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::PredicateExpressionContext::K_LIKE() {
  return getToken(FilterExpressionSyntaxParser::K_LIKE, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::PredicateExpressionContext::K_NOT() {
  return getToken(FilterExpressionSyntaxParser::K_NOT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::PredicateExpressionContext::K_IN() {
  return getToken(FilterExpressionSyntaxParser::K_IN, 0);
}

FilterExpressionSyntaxParser::ExpressionListContext* FilterExpressionSyntaxParser::PredicateExpressionContext::expressionList() {
  return getRuleContext<FilterExpressionSyntaxParser::ExpressionListContext>(0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::PredicateExpressionContext::K_IS() {
  return getToken(FilterExpressionSyntaxParser::K_IS, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::PredicateExpressionContext::K_NULL() {
  return getToken(FilterExpressionSyntaxParser::K_NULL, 0);
}


size_t FilterExpressionSyntaxParser::PredicateExpressionContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RulePredicateExpression;
}

void FilterExpressionSyntaxParser::PredicateExpressionContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterPredicateExpression(this);
}

void FilterExpressionSyntaxParser::PredicateExpressionContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitPredicateExpression(this);
}


FilterExpressionSyntaxParser::PredicateExpressionContext* FilterExpressionSyntaxParser::predicateExpression() {
   return predicateExpression(0);
}

FilterExpressionSyntaxParser::PredicateExpressionContext* FilterExpressionSyntaxParser::predicateExpression(int precedence) {
  ParserRuleContext *parentContext = _ctx;
  size_t parentState = getState();
  FilterExpressionSyntaxParser::PredicateExpressionContext *_localctx = _tracker.createInstance<PredicateExpressionContext>(_ctx, parentState);
  FilterExpressionSyntaxParser::PredicateExpressionContext *previousContext = _localctx;
  size_t startState = 20;
  enterRecursionRule(_localctx, 20, FilterExpressionSyntaxParser::RulePredicateExpression, precedence);

    size_t _la = 0;

  auto onExit = finally([=] {
    unrollRecursionContexts(parentContext);
  });
  try {
    size_t alt;
    enterOuterAlt(_localctx, 1);
    setState(141);
    valueExpression(0);
    _ctx->stop = _input->LT(-1);
    setState(170);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 18, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        if (!_parseListeners.empty())
          triggerExitRuleEvent();
        previousContext = _localctx;
        setState(168);
        _errHandler->sync(this);
        switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 17, _ctx)) {
        case 1: {
          _localctx = _tracker.createInstance<PredicateExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RulePredicateExpression);
          setState(143);

          if (!(precpred(_ctx, 3))) throw FailedPredicateException(this, "precpred(_ctx, 3)");
          setState(144);
          comparisonOperator();
          setState(145);
          predicateExpression(4);
          break;
        }

        case 2: {
          _localctx = _tracker.createInstance<PredicateExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RulePredicateExpression);
          setState(147);

          if (!(precpred(_ctx, 2))) throw FailedPredicateException(this, "precpred(_ctx, 2)");
          setState(149);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::K_NOT) {
            setState(148);
            match(FilterExpressionSyntaxParser::K_NOT);
          }
          setState(151);
          match(FilterExpressionSyntaxParser::K_LIKE);
          setState(152);
          predicateExpression(3);
          break;
        }

        case 3: {
          _localctx = _tracker.createInstance<PredicateExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RulePredicateExpression);
          setState(153);

          if (!(precpred(_ctx, 5))) throw FailedPredicateException(this, "precpred(_ctx, 5)");
          setState(155);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::K_NOT) {
            setState(154);
            match(FilterExpressionSyntaxParser::K_NOT);
          }
          setState(157);
          match(FilterExpressionSyntaxParser::K_IN);
          setState(158);
          match(FilterExpressionSyntaxParser::T__4);
          setState(159);
          expressionList();
          setState(160);
          match(FilterExpressionSyntaxParser::T__5);
          break;
        }

        case 4: {
          _localctx = _tracker.createInstance<PredicateExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RulePredicateExpression);
          setState(162);

          if (!(precpred(_ctx, 4))) throw FailedPredicateException(this, "precpred(_ctx, 4)");
          setState(163);
          match(FilterExpressionSyntaxParser::K_IS);
          setState(165);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::K_NOT) {
            setState(164);
            match(FilterExpressionSyntaxParser::K_NOT);
          }
          setState(167);
          match(FilterExpressionSyntaxParser::K_NULL);
          break;
        }

        } 
      }
      setState(172);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 18, _ctx);
    }
  }
  catch (RecognitionException &e) {
    _errHandler->reportError(this, e);
    _localctx->exception = std::current_exception();
    _errHandler->recover(this, _localctx->exception);
  }
  return _localctx;
}

//----------------- ValueExpressionContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::ValueExpressionContext::ValueExpressionContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

FilterExpressionSyntaxParser::LiteralValueContext* FilterExpressionSyntaxParser::ValueExpressionContext::literalValue() {
  return getRuleContext<FilterExpressionSyntaxParser::LiteralValueContext>(0);
}

FilterExpressionSyntaxParser::ColumnNameContext* FilterExpressionSyntaxParser::ValueExpressionContext::columnName() {
  return getRuleContext<FilterExpressionSyntaxParser::ColumnNameContext>(0);
}

FilterExpressionSyntaxParser::FunctionExpressionContext* FilterExpressionSyntaxParser::ValueExpressionContext::functionExpression() {
  return getRuleContext<FilterExpressionSyntaxParser::FunctionExpressionContext>(0);
}

FilterExpressionSyntaxParser::UnaryOperatorContext* FilterExpressionSyntaxParser::ValueExpressionContext::unaryOperator() {
  return getRuleContext<FilterExpressionSyntaxParser::UnaryOperatorContext>(0);
}

std::vector<FilterExpressionSyntaxParser::ValueExpressionContext *> FilterExpressionSyntaxParser::ValueExpressionContext::valueExpression() {
  return getRuleContexts<FilterExpressionSyntaxParser::ValueExpressionContext>();
}

FilterExpressionSyntaxParser::ValueExpressionContext* FilterExpressionSyntaxParser::ValueExpressionContext::valueExpression(size_t i) {
  return getRuleContext<FilterExpressionSyntaxParser::ValueExpressionContext>(i);
}

FilterExpressionSyntaxParser::ExpressionContext* FilterExpressionSyntaxParser::ValueExpressionContext::expression() {
  return getRuleContext<FilterExpressionSyntaxParser::ExpressionContext>(0);
}

FilterExpressionSyntaxParser::MathOperatorContext* FilterExpressionSyntaxParser::ValueExpressionContext::mathOperator() {
  return getRuleContext<FilterExpressionSyntaxParser::MathOperatorContext>(0);
}

FilterExpressionSyntaxParser::BitwiseOperatorContext* FilterExpressionSyntaxParser::ValueExpressionContext::bitwiseOperator() {
  return getRuleContext<FilterExpressionSyntaxParser::BitwiseOperatorContext>(0);
}


size_t FilterExpressionSyntaxParser::ValueExpressionContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleValueExpression;
}

void FilterExpressionSyntaxParser::ValueExpressionContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterValueExpression(this);
}

void FilterExpressionSyntaxParser::ValueExpressionContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitValueExpression(this);
}


FilterExpressionSyntaxParser::ValueExpressionContext* FilterExpressionSyntaxParser::valueExpression() {
   return valueExpression(0);
}

FilterExpressionSyntaxParser::ValueExpressionContext* FilterExpressionSyntaxParser::valueExpression(int precedence) {
  ParserRuleContext *parentContext = _ctx;
  size_t parentState = getState();
  FilterExpressionSyntaxParser::ValueExpressionContext *_localctx = _tracker.createInstance<ValueExpressionContext>(_ctx, parentState);
  FilterExpressionSyntaxParser::ValueExpressionContext *previousContext = _localctx;
  size_t startState = 22;
  enterRecursionRule(_localctx, 22, FilterExpressionSyntaxParser::RuleValueExpression, precedence);

    

  auto onExit = finally([=] {
    unrollRecursionContexts(parentContext);
  });
  try {
    size_t alt;
    enterOuterAlt(_localctx, 1);
    setState(184);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case FilterExpressionSyntaxParser::K_NULL:
      case FilterExpressionSyntaxParser::BOOLEAN_LITERAL:
      case FilterExpressionSyntaxParser::INTEGER_LITERAL:
      case FilterExpressionSyntaxParser::NUMERIC_LITERAL:
      case FilterExpressionSyntaxParser::GUID_LITERAL:
      case FilterExpressionSyntaxParser::STRING_LITERAL:
      case FilterExpressionSyntaxParser::DATETIME_LITERAL: {
        setState(174);
        literalValue();
        break;
      }

      case FilterExpressionSyntaxParser::IDENTIFIER: {
        setState(175);
        columnName();
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
        setState(176);
        functionExpression();
        break;
      }

      case FilterExpressionSyntaxParser::T__2:
      case FilterExpressionSyntaxParser::T__3:
      case FilterExpressionSyntaxParser::T__6:
      case FilterExpressionSyntaxParser::T__7:
      case FilterExpressionSyntaxParser::K_NOT: {
        setState(177);
        unaryOperator();
        setState(178);
        valueExpression(4);
        break;
      }

      case FilterExpressionSyntaxParser::T__4: {
        setState(180);
        match(FilterExpressionSyntaxParser::T__4);
        setState(181);
        expression(0);
        setState(182);
        match(FilterExpressionSyntaxParser::T__5);
        break;
      }

    default:
      throw NoViableAltException(this);
    }
    _ctx->stop = _input->LT(-1);
    setState(196);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 21, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        if (!_parseListeners.empty())
          triggerExitRuleEvent();
        previousContext = _localctx;
        setState(194);
        _errHandler->sync(this);
        switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 20, _ctx)) {
        case 1: {
          _localctx = _tracker.createInstance<ValueExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleValueExpression);
          setState(186);

          if (!(precpred(_ctx, 2))) throw FailedPredicateException(this, "precpred(_ctx, 2)");
          setState(187);
          mathOperator();
          setState(188);
          valueExpression(3);
          break;
        }

        case 2: {
          _localctx = _tracker.createInstance<ValueExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleValueExpression);
          setState(190);

          if (!(precpred(_ctx, 1))) throw FailedPredicateException(this, "precpred(_ctx, 1)");
          setState(191);
          bitwiseOperator();
          setState(192);
          valueExpression(2);
          break;
        }

        } 
      }
      setState(198);
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

//----------------- NotOperatorContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::NotOperatorContext::NotOperatorContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::NotOperatorContext::K_NOT() {
  return getToken(FilterExpressionSyntaxParser::K_NOT, 0);
}


size_t FilterExpressionSyntaxParser::NotOperatorContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleNotOperator;
}

void FilterExpressionSyntaxParser::NotOperatorContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterNotOperator(this);
}

void FilterExpressionSyntaxParser::NotOperatorContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitNotOperator(this);
}

FilterExpressionSyntaxParser::NotOperatorContext* FilterExpressionSyntaxParser::notOperator() {
  NotOperatorContext *_localctx = _tracker.createInstance<NotOperatorContext>(_ctx, getState());
  enterRule(_localctx, 24, FilterExpressionSyntaxParser::RuleNotOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(199);
    _la = _input->LA(1);
    if (!(_la == FilterExpressionSyntaxParser::T__6

    || _la == FilterExpressionSyntaxParser::K_NOT)) {
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
  enterRule(_localctx, 26, FilterExpressionSyntaxParser::RuleUnaryOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(201);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__2)
      | (1ULL << FilterExpressionSyntaxParser::T__3)
      | (1ULL << FilterExpressionSyntaxParser::T__6)
      | (1ULL << FilterExpressionSyntaxParser::T__7)
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

//----------------- ComparisonOperatorContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::ComparisonOperatorContext::ComparisonOperatorContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}


size_t FilterExpressionSyntaxParser::ComparisonOperatorContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleComparisonOperator;
}

void FilterExpressionSyntaxParser::ComparisonOperatorContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterComparisonOperator(this);
}

void FilterExpressionSyntaxParser::ComparisonOperatorContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitComparisonOperator(this);
}

FilterExpressionSyntaxParser::ComparisonOperatorContext* FilterExpressionSyntaxParser::comparisonOperator() {
  ComparisonOperatorContext *_localctx = _tracker.createInstance<ComparisonOperatorContext>(_ctx, getState());
  enterRule(_localctx, 28, FilterExpressionSyntaxParser::RuleComparisonOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(203);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__8)
      | (1ULL << FilterExpressionSyntaxParser::T__9)
      | (1ULL << FilterExpressionSyntaxParser::T__10)
      | (1ULL << FilterExpressionSyntaxParser::T__11)
      | (1ULL << FilterExpressionSyntaxParser::T__12)
      | (1ULL << FilterExpressionSyntaxParser::T__13)
      | (1ULL << FilterExpressionSyntaxParser::T__14)
      | (1ULL << FilterExpressionSyntaxParser::T__15))) != 0))) {
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

//----------------- LogicalOperatorContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::LogicalOperatorContext::LogicalOperatorContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::LogicalOperatorContext::K_AND() {
  return getToken(FilterExpressionSyntaxParser::K_AND, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::LogicalOperatorContext::K_OR() {
  return getToken(FilterExpressionSyntaxParser::K_OR, 0);
}


size_t FilterExpressionSyntaxParser::LogicalOperatorContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleLogicalOperator;
}

void FilterExpressionSyntaxParser::LogicalOperatorContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterLogicalOperator(this);
}

void FilterExpressionSyntaxParser::LogicalOperatorContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitLogicalOperator(this);
}

FilterExpressionSyntaxParser::LogicalOperatorContext* FilterExpressionSyntaxParser::logicalOperator() {
  LogicalOperatorContext *_localctx = _tracker.createInstance<LogicalOperatorContext>(_ctx, getState());
  enterRule(_localctx, 30, FilterExpressionSyntaxParser::RuleLogicalOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(205);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__16)
      | (1ULL << FilterExpressionSyntaxParser::T__17)
      | (1ULL << FilterExpressionSyntaxParser::K_AND)
      | (1ULL << FilterExpressionSyntaxParser::K_OR))) != 0))) {
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

//----------------- BitwiseOperatorContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::BitwiseOperatorContext::BitwiseOperatorContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}


size_t FilterExpressionSyntaxParser::BitwiseOperatorContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleBitwiseOperator;
}

void FilterExpressionSyntaxParser::BitwiseOperatorContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterBitwiseOperator(this);
}

void FilterExpressionSyntaxParser::BitwiseOperatorContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitBitwiseOperator(this);
}

FilterExpressionSyntaxParser::BitwiseOperatorContext* FilterExpressionSyntaxParser::bitwiseOperator() {
  BitwiseOperatorContext *_localctx = _tracker.createInstance<BitwiseOperatorContext>(_ctx, getState());
  enterRule(_localctx, 32, FilterExpressionSyntaxParser::RuleBitwiseOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(207);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__18)
      | (1ULL << FilterExpressionSyntaxParser::T__19)
      | (1ULL << FilterExpressionSyntaxParser::T__20)
      | (1ULL << FilterExpressionSyntaxParser::T__21))) != 0))) {
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

//----------------- MathOperatorContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::MathOperatorContext::MathOperatorContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}


size_t FilterExpressionSyntaxParser::MathOperatorContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleMathOperator;
}

void FilterExpressionSyntaxParser::MathOperatorContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterMathOperator(this);
}

void FilterExpressionSyntaxParser::MathOperatorContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitMathOperator(this);
}

FilterExpressionSyntaxParser::MathOperatorContext* FilterExpressionSyntaxParser::mathOperator() {
  MathOperatorContext *_localctx = _tracker.createInstance<MathOperatorContext>(_ctx, getState());
  enterRule(_localctx, 34, FilterExpressionSyntaxParser::RuleMathOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(209);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__2)
      | (1ULL << FilterExpressionSyntaxParser::T__3)
      | (1ULL << FilterExpressionSyntaxParser::T__22)
      | (1ULL << FilterExpressionSyntaxParser::T__23)
      | (1ULL << FilterExpressionSyntaxParser::T__24))) != 0))) {
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
  enterRule(_localctx, 36, FilterExpressionSyntaxParser::RuleFunctionName);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(211);
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

//----------------- FunctionExpressionContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::FunctionExpressionContext::FunctionExpressionContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

FilterExpressionSyntaxParser::FunctionNameContext* FilterExpressionSyntaxParser::FunctionExpressionContext::functionName() {
  return getRuleContext<FilterExpressionSyntaxParser::FunctionNameContext>(0);
}

FilterExpressionSyntaxParser::ExpressionListContext* FilterExpressionSyntaxParser::FunctionExpressionContext::expressionList() {
  return getRuleContext<FilterExpressionSyntaxParser::ExpressionListContext>(0);
}


size_t FilterExpressionSyntaxParser::FunctionExpressionContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleFunctionExpression;
}

void FilterExpressionSyntaxParser::FunctionExpressionContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterFunctionExpression(this);
}

void FilterExpressionSyntaxParser::FunctionExpressionContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitFunctionExpression(this);
}

FilterExpressionSyntaxParser::FunctionExpressionContext* FilterExpressionSyntaxParser::functionExpression() {
  FunctionExpressionContext *_localctx = _tracker.createInstance<FunctionExpressionContext>(_ctx, getState());
  enterRule(_localctx, 38, FilterExpressionSyntaxParser::RuleFunctionExpression);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(213);
    functionName();
    setState(214);
    match(FilterExpressionSyntaxParser::T__4);
    setState(216);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if ((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__2)
      | (1ULL << FilterExpressionSyntaxParser::T__3)
      | (1ULL << FilterExpressionSyntaxParser::T__4)
      | (1ULL << FilterExpressionSyntaxParser::T__6)
      | (1ULL << FilterExpressionSyntaxParser::T__7)
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
      setState(215);
      expressionList();
    }
    setState(218);
    match(FilterExpressionSyntaxParser::T__5);
   
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
  enterRule(_localctx, 40, FilterExpressionSyntaxParser::RuleLiteralValue);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(220);
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
  enterRule(_localctx, 42, FilterExpressionSyntaxParser::RuleTableName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(222);
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
  enterRule(_localctx, 44, FilterExpressionSyntaxParser::RuleColumnName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(224);
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
  enterRule(_localctx, 46, FilterExpressionSyntaxParser::RuleOrderByColumnName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(226);
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
    case 9: return expressionSempred(dynamic_cast<ExpressionContext *>(context), predicateIndex);
    case 10: return predicateExpressionSempred(dynamic_cast<PredicateExpressionContext *>(context), predicateIndex);
    case 11: return valueExpressionSempred(dynamic_cast<ValueExpressionContext *>(context), predicateIndex);

  default:
    break;
  }
  return true;
}

bool FilterExpressionSyntaxParser::expressionSempred(ExpressionContext *_localctx, size_t predicateIndex) {
  switch (predicateIndex) {
    case 0: return precpred(_ctx, 2);

  default:
    break;
  }
  return true;
}

bool FilterExpressionSyntaxParser::predicateExpressionSempred(PredicateExpressionContext *_localctx, size_t predicateIndex) {
  switch (predicateIndex) {
    case 1: return precpred(_ctx, 3);
    case 2: return precpred(_ctx, 2);
    case 3: return precpred(_ctx, 5);
    case 4: return precpred(_ctx, 4);

  default:
    break;
  }
  return true;
}

bool FilterExpressionSyntaxParser::valueExpressionSempred(ValueExpressionContext *_localctx, size_t predicateIndex) {
  switch (predicateIndex) {
    case 5: return precpred(_ctx, 2);
    case 6: return precpred(_ctx, 1);

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
  "identifierStatement", "filterStatement", "topLimit", "orderingTerm", 
  "expressionList", "expression", "predicateExpression", "valueExpression", 
  "notOperator", "unaryOperator", "comparisonOperator", "logicalOperator", 
  "bitwiseOperator", "mathOperator", "functionName", "functionExpression", 
  "literalValue", "tableName", "columnName", "orderByColumnName"
};

std::vector<std::string> FilterExpressionSyntaxParser::_literalNames = {
  "", "';'", "','", "'-'", "'+'", "'('", "')'", "'!'", "'~'", "'<'", "'<='", 
  "'>'", "'>='", "'='", "'=='", "'!='", "'<>'", "'&&'", "'||'", "'<<'", 
  "'>>'", "'&'", "'|'", "'*'", "'/'", "'%'"
};

std::vector<std::string> FilterExpressionSyntaxParser::_symbolicNames = {
  "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
  "", "", "", "", "", "", "", "", "K_AND", "K_ASC", "K_BY", "K_CONVERT", 
  "K_COALESCE", "K_DESC", "K_FILTER", "K_IIF", "K_IN", "K_IS", "K_ISNULL", 
  "K_ISREGEXMATCH", "K_LEN", "K_LIKE", "K_NOT", "K_NULL", "K_OR", "K_ORDER", 
  "K_REGEXVAL", "K_SUBSTR", "K_SUBSTRING", "K_TOP", "K_TRIM", "K_WHERE", 
  "BOOLEAN_LITERAL", "IDENTIFIER", "INTEGER_LITERAL", "NUMERIC_LITERAL", 
  "GUID_LITERAL", "MEASUREMENT_KEY_LITERAL", "POINT_TAG_LITERAL", "STRING_LITERAL", 
  "DATETIME_LITERAL", "SINGLE_LINE_COMMENT", "MULTILINE_COMMENT", "SPACES", 
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
    0x3, 0x40, 0xe7, 0x4, 0x2, 0x9, 0x2, 0x4, 0x3, 0x9, 0x3, 0x4, 0x4, 0x9, 
    0x4, 0x4, 0x5, 0x9, 0x5, 0x4, 0x6, 0x9, 0x6, 0x4, 0x7, 0x9, 0x7, 0x4, 
    0x8, 0x9, 0x8, 0x4, 0x9, 0x9, 0x9, 0x4, 0xa, 0x9, 0xa, 0x4, 0xb, 0x9, 
    0xb, 0x4, 0xc, 0x9, 0xc, 0x4, 0xd, 0x9, 0xd, 0x4, 0xe, 0x9, 0xe, 0x4, 
    0xf, 0x9, 0xf, 0x4, 0x10, 0x9, 0x10, 0x4, 0x11, 0x9, 0x11, 0x4, 0x12, 
    0x9, 0x12, 0x4, 0x13, 0x9, 0x13, 0x4, 0x14, 0x9, 0x14, 0x4, 0x15, 0x9, 
    0x15, 0x4, 0x16, 0x9, 0x16, 0x4, 0x17, 0x9, 0x17, 0x4, 0x18, 0x9, 0x18, 
    0x4, 0x19, 0x9, 0x19, 0x3, 0x2, 0x3, 0x2, 0x5, 0x2, 0x35, 0xa, 0x2, 
    0x3, 0x2, 0x3, 0x2, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 0x4, 0x7, 0x4, 
    0x3d, 0xa, 0x4, 0xc, 0x4, 0xe, 0x4, 0x40, 0xb, 0x4, 0x3, 0x4, 0x3, 0x4, 
    0x6, 0x4, 0x44, 0xa, 0x4, 0xd, 0x4, 0xe, 0x4, 0x45, 0x3, 0x4, 0x7, 0x4, 
    0x49, 0xa, 0x4, 0xc, 0x4, 0xe, 0x4, 0x4c, 0xb, 0x4, 0x3, 0x4, 0x7, 0x4, 
    0x4f, 0xa, 0x4, 0xc, 0x4, 0xe, 0x4, 0x52, 0xb, 0x4, 0x3, 0x5, 0x3, 0x5, 
    0x5, 0x5, 0x56, 0xa, 0x5, 0x3, 0x6, 0x3, 0x6, 0x3, 0x7, 0x3, 0x7, 0x3, 
    0x7, 0x5, 0x7, 0x5d, 0xa, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 
    0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x7, 0x7, 0x67, 0xa, 0x7, 0xc, 
    0x7, 0xe, 0x7, 0x6a, 0xb, 0x7, 0x5, 0x7, 0x6c, 0xa, 0x7, 0x3, 0x8, 0x5, 
    0x8, 0x6f, 0xa, 0x8, 0x3, 0x8, 0x3, 0x8, 0x3, 0x9, 0x3, 0x9, 0x5, 0x9, 
    0x75, 0xa, 0x9, 0x3, 0xa, 0x3, 0xa, 0x3, 0xa, 0x7, 0xa, 0x7a, 0xa, 0xa, 
    0xc, 0xa, 0xe, 0xa, 0x7d, 0xb, 0xa, 0x3, 0xb, 0x3, 0xb, 0x3, 0xb, 0x3, 
    0xb, 0x3, 0xb, 0x5, 0xb, 0x84, 0xa, 0xb, 0x3, 0xb, 0x3, 0xb, 0x3, 0xb, 
    0x3, 0xb, 0x7, 0xb, 0x8a, 0xa, 0xb, 0xc, 0xb, 0xe, 0xb, 0x8d, 0xb, 0xb, 
    0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 
    0x3, 0xc, 0x3, 0xc, 0x5, 0xc, 0x98, 0xa, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 
    0xc, 0x3, 0xc, 0x5, 0xc, 0x9e, 0xa, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 
    0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x5, 0xc, 0xa8, 0xa, 
    0xc, 0x3, 0xc, 0x7, 0xc, 0xab, 0xa, 0xc, 0xc, 0xc, 0xe, 0xc, 0xae, 0xb, 
    0xc, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 
    0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x5, 0xd, 0xbb, 0xa, 0xd, 
    0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 
    0x3, 0xd, 0x7, 0xd, 0xc5, 0xa, 0xd, 0xc, 0xd, 0xe, 0xd, 0xc8, 0xb, 0xd, 
    0x3, 0xe, 0x3, 0xe, 0x3, 0xf, 0x3, 0xf, 0x3, 0x10, 0x3, 0x10, 0x3, 0x11, 
    0x3, 0x11, 0x3, 0x12, 0x3, 0x12, 0x3, 0x13, 0x3, 0x13, 0x3, 0x14, 0x3, 
    0x14, 0x3, 0x15, 0x3, 0x15, 0x3, 0x15, 0x5, 0x15, 0xdb, 0xa, 0x15, 0x3, 
    0x15, 0x3, 0x15, 0x3, 0x16, 0x3, 0x16, 0x3, 0x17, 0x3, 0x17, 0x3, 0x18, 
    0x3, 0x18, 0x3, 0x19, 0x3, 0x19, 0x3, 0x19, 0x2, 0x5, 0x14, 0x16, 0x18, 
    0x1a, 0x2, 0x4, 0x6, 0x8, 0xa, 0xc, 0xe, 0x10, 0x12, 0x14, 0x16, 0x18, 
    0x1a, 0x1c, 0x1e, 0x20, 0x22, 0x24, 0x26, 0x28, 0x2a, 0x2c, 0x2e, 0x30, 
    0x2, 0xd, 0x3, 0x2, 0x38, 0x3a, 0x3, 0x2, 0x5, 0x6, 0x4, 0x2, 0x1d, 
    0x1d, 0x21, 0x21, 0x4, 0x2, 0x9, 0x9, 0x2a, 0x2a, 0x5, 0x2, 0x5, 0x6, 
    0x9, 0xa, 0x2a, 0x2a, 0x3, 0x2, 0xb, 0x12, 0x5, 0x2, 0x13, 0x14, 0x1c, 
    0x1c, 0x2c, 0x2c, 0x3, 0x2, 0x15, 0x18, 0x4, 0x2, 0x5, 0x6, 0x19, 0x1b, 
    0x7, 0x2, 0x1f, 0x20, 0x23, 0x23, 0x26, 0x28, 0x2e, 0x30, 0x32, 0x32, 
    0x6, 0x2, 0x2b, 0x2b, 0x34, 0x34, 0x36, 0x38, 0x3b, 0x3c, 0x2, 0xea, 
    0x2, 0x34, 0x3, 0x2, 0x2, 0x2, 0x4, 0x38, 0x3, 0x2, 0x2, 0x2, 0x6, 0x3e, 
    0x3, 0x2, 0x2, 0x2, 0x8, 0x55, 0x3, 0x2, 0x2, 0x2, 0xa, 0x57, 0x3, 0x2, 
    0x2, 0x2, 0xc, 0x59, 0x3, 0x2, 0x2, 0x2, 0xe, 0x6e, 0x3, 0x2, 0x2, 0x2, 
    0x10, 0x72, 0x3, 0x2, 0x2, 0x2, 0x12, 0x76, 0x3, 0x2, 0x2, 0x2, 0x14, 
    0x83, 0x3, 0x2, 0x2, 0x2, 0x16, 0x8e, 0x3, 0x2, 0x2, 0x2, 0x18, 0xba, 
    0x3, 0x2, 0x2, 0x2, 0x1a, 0xc9, 0x3, 0x2, 0x2, 0x2, 0x1c, 0xcb, 0x3, 
    0x2, 0x2, 0x2, 0x1e, 0xcd, 0x3, 0x2, 0x2, 0x2, 0x20, 0xcf, 0x3, 0x2, 
    0x2, 0x2, 0x22, 0xd1, 0x3, 0x2, 0x2, 0x2, 0x24, 0xd3, 0x3, 0x2, 0x2, 
    0x2, 0x26, 0xd5, 0x3, 0x2, 0x2, 0x2, 0x28, 0xd7, 0x3, 0x2, 0x2, 0x2, 
    0x2a, 0xde, 0x3, 0x2, 0x2, 0x2, 0x2c, 0xe0, 0x3, 0x2, 0x2, 0x2, 0x2e, 
    0xe2, 0x3, 0x2, 0x2, 0x2, 0x30, 0xe4, 0x3, 0x2, 0x2, 0x2, 0x32, 0x35, 
    0x5, 0x6, 0x4, 0x2, 0x33, 0x35, 0x5, 0x4, 0x3, 0x2, 0x34, 0x32, 0x3, 
    0x2, 0x2, 0x2, 0x34, 0x33, 0x3, 0x2, 0x2, 0x2, 0x35, 0x36, 0x3, 0x2, 
    0x2, 0x2, 0x36, 0x37, 0x7, 0x2, 0x2, 0x3, 0x37, 0x3, 0x3, 0x2, 0x2, 
    0x2, 0x38, 0x39, 0x7, 0x40, 0x2, 0x2, 0x39, 0x3a, 0x8, 0x3, 0x1, 0x2, 
    0x3a, 0x5, 0x3, 0x2, 0x2, 0x2, 0x3b, 0x3d, 0x7, 0x3, 0x2, 0x2, 0x3c, 
    0x3b, 0x3, 0x2, 0x2, 0x2, 0x3d, 0x40, 0x3, 0x2, 0x2, 0x2, 0x3e, 0x3c, 
    0x3, 0x2, 0x2, 0x2, 0x3e, 0x3f, 0x3, 0x2, 0x2, 0x2, 0x3f, 0x41, 0x3, 
    0x2, 0x2, 0x2, 0x40, 0x3e, 0x3, 0x2, 0x2, 0x2, 0x41, 0x4a, 0x5, 0x8, 
    0x5, 0x2, 0x42, 0x44, 0x7, 0x3, 0x2, 0x2, 0x43, 0x42, 0x3, 0x2, 0x2, 
    0x2, 0x44, 0x45, 0x3, 0x2, 0x2, 0x2, 0x45, 0x43, 0x3, 0x2, 0x2, 0x2, 
    0x45, 0x46, 0x3, 0x2, 0x2, 0x2, 0x46, 0x47, 0x3, 0x2, 0x2, 0x2, 0x47, 
    0x49, 0x5, 0x8, 0x5, 0x2, 0x48, 0x43, 0x3, 0x2, 0x2, 0x2, 0x49, 0x4c, 
    0x3, 0x2, 0x2, 0x2, 0x4a, 0x48, 0x3, 0x2, 0x2, 0x2, 0x4a, 0x4b, 0x3, 
    0x2, 0x2, 0x2, 0x4b, 0x50, 0x3, 0x2, 0x2, 0x2, 0x4c, 0x4a, 0x3, 0x2, 
    0x2, 0x2, 0x4d, 0x4f, 0x7, 0x3, 0x2, 0x2, 0x4e, 0x4d, 0x3, 0x2, 0x2, 
    0x2, 0x4f, 0x52, 0x3, 0x2, 0x2, 0x2, 0x50, 0x4e, 0x3, 0x2, 0x2, 0x2, 
    0x50, 0x51, 0x3, 0x2, 0x2, 0x2, 0x51, 0x7, 0x3, 0x2, 0x2, 0x2, 0x52, 
    0x50, 0x3, 0x2, 0x2, 0x2, 0x53, 0x56, 0x5, 0xa, 0x6, 0x2, 0x54, 0x56, 
    0x5, 0xc, 0x7, 0x2, 0x55, 0x53, 0x3, 0x2, 0x2, 0x2, 0x55, 0x54, 0x3, 
    0x2, 0x2, 0x2, 0x56, 0x9, 0x3, 0x2, 0x2, 0x2, 0x57, 0x58, 0x9, 0x2, 
    0x2, 0x2, 0x58, 0xb, 0x3, 0x2, 0x2, 0x2, 0x59, 0x5c, 0x7, 0x22, 0x2, 
    0x2, 0x5a, 0x5b, 0x7, 0x31, 0x2, 0x2, 0x5b, 0x5d, 0x5, 0xe, 0x8, 0x2, 
    0x5c, 0x5a, 0x3, 0x2, 0x2, 0x2, 0x5c, 0x5d, 0x3, 0x2, 0x2, 0x2, 0x5d, 
    0x5e, 0x3, 0x2, 0x2, 0x2, 0x5e, 0x5f, 0x5, 0x2c, 0x17, 0x2, 0x5f, 0x60, 
    0x7, 0x33, 0x2, 0x2, 0x60, 0x6b, 0x5, 0x14, 0xb, 0x2, 0x61, 0x62, 0x7, 
    0x2d, 0x2, 0x2, 0x62, 0x63, 0x7, 0x1e, 0x2, 0x2, 0x63, 0x68, 0x5, 0x10, 
    0x9, 0x2, 0x64, 0x65, 0x7, 0x4, 0x2, 0x2, 0x65, 0x67, 0x5, 0x10, 0x9, 
    0x2, 0x66, 0x64, 0x3, 0x2, 0x2, 0x2, 0x67, 0x6a, 0x3, 0x2, 0x2, 0x2, 
    0x68, 0x66, 0x3, 0x2, 0x2, 0x2, 0x68, 0x69, 0x3, 0x2, 0x2, 0x2, 0x69, 
    0x6c, 0x3, 0x2, 0x2, 0x2, 0x6a, 0x68, 0x3, 0x2, 0x2, 0x2, 0x6b, 0x61, 
    0x3, 0x2, 0x2, 0x2, 0x6b, 0x6c, 0x3, 0x2, 0x2, 0x2, 0x6c, 0xd, 0x3, 
    0x2, 0x2, 0x2, 0x6d, 0x6f, 0x9, 0x3, 0x2, 0x2, 0x6e, 0x6d, 0x3, 0x2, 
    0x2, 0x2, 0x6e, 0x6f, 0x3, 0x2, 0x2, 0x2, 0x6f, 0x70, 0x3, 0x2, 0x2, 
    0x2, 0x70, 0x71, 0x7, 0x36, 0x2, 0x2, 0x71, 0xf, 0x3, 0x2, 0x2, 0x2, 
    0x72, 0x74, 0x5, 0x30, 0x19, 0x2, 0x73, 0x75, 0x9, 0x4, 0x2, 0x2, 0x74, 
    0x73, 0x3, 0x2, 0x2, 0x2, 0x74, 0x75, 0x3, 0x2, 0x2, 0x2, 0x75, 0x11, 
    0x3, 0x2, 0x2, 0x2, 0x76, 0x7b, 0x5, 0x14, 0xb, 0x2, 0x77, 0x78, 0x7, 
    0x4, 0x2, 0x2, 0x78, 0x7a, 0x5, 0x14, 0xb, 0x2, 0x79, 0x77, 0x3, 0x2, 
    0x2, 0x2, 0x7a, 0x7d, 0x3, 0x2, 0x2, 0x2, 0x7b, 0x79, 0x3, 0x2, 0x2, 
    0x2, 0x7b, 0x7c, 0x3, 0x2, 0x2, 0x2, 0x7c, 0x13, 0x3, 0x2, 0x2, 0x2, 
    0x7d, 0x7b, 0x3, 0x2, 0x2, 0x2, 0x7e, 0x7f, 0x8, 0xb, 0x1, 0x2, 0x7f, 
    0x80, 0x5, 0x1a, 0xe, 0x2, 0x80, 0x81, 0x5, 0x14, 0xb, 0x5, 0x81, 0x84, 
    0x3, 0x2, 0x2, 0x2, 0x82, 0x84, 0x5, 0x16, 0xc, 0x2, 0x83, 0x7e, 0x3, 
    0x2, 0x2, 0x2, 0x83, 0x82, 0x3, 0x2, 0x2, 0x2, 0x84, 0x8b, 0x3, 0x2, 
    0x2, 0x2, 0x85, 0x86, 0xc, 0x4, 0x2, 0x2, 0x86, 0x87, 0x5, 0x20, 0x11, 
    0x2, 0x87, 0x88, 0x5, 0x14, 0xb, 0x5, 0x88, 0x8a, 0x3, 0x2, 0x2, 0x2, 
    0x89, 0x85, 0x3, 0x2, 0x2, 0x2, 0x8a, 0x8d, 0x3, 0x2, 0x2, 0x2, 0x8b, 
    0x89, 0x3, 0x2, 0x2, 0x2, 0x8b, 0x8c, 0x3, 0x2, 0x2, 0x2, 0x8c, 0x15, 
    0x3, 0x2, 0x2, 0x2, 0x8d, 0x8b, 0x3, 0x2, 0x2, 0x2, 0x8e, 0x8f, 0x8, 
    0xc, 0x1, 0x2, 0x8f, 0x90, 0x5, 0x18, 0xd, 0x2, 0x90, 0xac, 0x3, 0x2, 
    0x2, 0x2, 0x91, 0x92, 0xc, 0x5, 0x2, 0x2, 0x92, 0x93, 0x5, 0x1e, 0x10, 
    0x2, 0x93, 0x94, 0x5, 0x16, 0xc, 0x6, 0x94, 0xab, 0x3, 0x2, 0x2, 0x2, 
    0x95, 0x97, 0xc, 0x4, 0x2, 0x2, 0x96, 0x98, 0x7, 0x2a, 0x2, 0x2, 0x97, 
    0x96, 0x3, 0x2, 0x2, 0x2, 0x97, 0x98, 0x3, 0x2, 0x2, 0x2, 0x98, 0x99, 
    0x3, 0x2, 0x2, 0x2, 0x99, 0x9a, 0x7, 0x29, 0x2, 0x2, 0x9a, 0xab, 0x5, 
    0x16, 0xc, 0x5, 0x9b, 0x9d, 0xc, 0x7, 0x2, 0x2, 0x9c, 0x9e, 0x7, 0x2a, 
    0x2, 0x2, 0x9d, 0x9c, 0x3, 0x2, 0x2, 0x2, 0x9d, 0x9e, 0x3, 0x2, 0x2, 
    0x2, 0x9e, 0x9f, 0x3, 0x2, 0x2, 0x2, 0x9f, 0xa0, 0x7, 0x24, 0x2, 0x2, 
    0xa0, 0xa1, 0x7, 0x7, 0x2, 0x2, 0xa1, 0xa2, 0x5, 0x12, 0xa, 0x2, 0xa2, 
    0xa3, 0x7, 0x8, 0x2, 0x2, 0xa3, 0xab, 0x3, 0x2, 0x2, 0x2, 0xa4, 0xa5, 
    0xc, 0x6, 0x2, 0x2, 0xa5, 0xa7, 0x7, 0x25, 0x2, 0x2, 0xa6, 0xa8, 0x7, 
    0x2a, 0x2, 0x2, 0xa7, 0xa6, 0x3, 0x2, 0x2, 0x2, 0xa7, 0xa8, 0x3, 0x2, 
    0x2, 0x2, 0xa8, 0xa9, 0x3, 0x2, 0x2, 0x2, 0xa9, 0xab, 0x7, 0x2b, 0x2, 
    0x2, 0xaa, 0x91, 0x3, 0x2, 0x2, 0x2, 0xaa, 0x95, 0x3, 0x2, 0x2, 0x2, 
    0xaa, 0x9b, 0x3, 0x2, 0x2, 0x2, 0xaa, 0xa4, 0x3, 0x2, 0x2, 0x2, 0xab, 
    0xae, 0x3, 0x2, 0x2, 0x2, 0xac, 0xaa, 0x3, 0x2, 0x2, 0x2, 0xac, 0xad, 
    0x3, 0x2, 0x2, 0x2, 0xad, 0x17, 0x3, 0x2, 0x2, 0x2, 0xae, 0xac, 0x3, 
    0x2, 0x2, 0x2, 0xaf, 0xb0, 0x8, 0xd, 0x1, 0x2, 0xb0, 0xbb, 0x5, 0x2a, 
    0x16, 0x2, 0xb1, 0xbb, 0x5, 0x2e, 0x18, 0x2, 0xb2, 0xbb, 0x5, 0x28, 
    0x15, 0x2, 0xb3, 0xb4, 0x5, 0x1c, 0xf, 0x2, 0xb4, 0xb5, 0x5, 0x18, 0xd, 
    0x6, 0xb5, 0xbb, 0x3, 0x2, 0x2, 0x2, 0xb6, 0xb7, 0x7, 0x7, 0x2, 0x2, 
    0xb7, 0xb8, 0x5, 0x14, 0xb, 0x2, 0xb8, 0xb9, 0x7, 0x8, 0x2, 0x2, 0xb9, 
    0xbb, 0x3, 0x2, 0x2, 0x2, 0xba, 0xaf, 0x3, 0x2, 0x2, 0x2, 0xba, 0xb1, 
    0x3, 0x2, 0x2, 0x2, 0xba, 0xb2, 0x3, 0x2, 0x2, 0x2, 0xba, 0xb3, 0x3, 
    0x2, 0x2, 0x2, 0xba, 0xb6, 0x3, 0x2, 0x2, 0x2, 0xbb, 0xc6, 0x3, 0x2, 
    0x2, 0x2, 0xbc, 0xbd, 0xc, 0x4, 0x2, 0x2, 0xbd, 0xbe, 0x5, 0x24, 0x13, 
    0x2, 0xbe, 0xbf, 0x5, 0x18, 0xd, 0x5, 0xbf, 0xc5, 0x3, 0x2, 0x2, 0x2, 
    0xc0, 0xc1, 0xc, 0x3, 0x2, 0x2, 0xc1, 0xc2, 0x5, 0x22, 0x12, 0x2, 0xc2, 
    0xc3, 0x5, 0x18, 0xd, 0x4, 0xc3, 0xc5, 0x3, 0x2, 0x2, 0x2, 0xc4, 0xbc, 
    0x3, 0x2, 0x2, 0x2, 0xc4, 0xc0, 0x3, 0x2, 0x2, 0x2, 0xc5, 0xc8, 0x3, 
    0x2, 0x2, 0x2, 0xc6, 0xc4, 0x3, 0x2, 0x2, 0x2, 0xc6, 0xc7, 0x3, 0x2, 
    0x2, 0x2, 0xc7, 0x19, 0x3, 0x2, 0x2, 0x2, 0xc8, 0xc6, 0x3, 0x2, 0x2, 
    0x2, 0xc9, 0xca, 0x9, 0x5, 0x2, 0x2, 0xca, 0x1b, 0x3, 0x2, 0x2, 0x2, 
    0xcb, 0xcc, 0x9, 0x6, 0x2, 0x2, 0xcc, 0x1d, 0x3, 0x2, 0x2, 0x2, 0xcd, 
    0xce, 0x9, 0x7, 0x2, 0x2, 0xce, 0x1f, 0x3, 0x2, 0x2, 0x2, 0xcf, 0xd0, 
    0x9, 0x8, 0x2, 0x2, 0xd0, 0x21, 0x3, 0x2, 0x2, 0x2, 0xd1, 0xd2, 0x9, 
    0x9, 0x2, 0x2, 0xd2, 0x23, 0x3, 0x2, 0x2, 0x2, 0xd3, 0xd4, 0x9, 0xa, 
    0x2, 0x2, 0xd4, 0x25, 0x3, 0x2, 0x2, 0x2, 0xd5, 0xd6, 0x9, 0xb, 0x2, 
    0x2, 0xd6, 0x27, 0x3, 0x2, 0x2, 0x2, 0xd7, 0xd8, 0x5, 0x26, 0x14, 0x2, 
    0xd8, 0xda, 0x7, 0x7, 0x2, 0x2, 0xd9, 0xdb, 0x5, 0x12, 0xa, 0x2, 0xda, 
    0xd9, 0x3, 0x2, 0x2, 0x2, 0xda, 0xdb, 0x3, 0x2, 0x2, 0x2, 0xdb, 0xdc, 
    0x3, 0x2, 0x2, 0x2, 0xdc, 0xdd, 0x7, 0x8, 0x2, 0x2, 0xdd, 0x29, 0x3, 
    0x2, 0x2, 0x2, 0xde, 0xdf, 0x9, 0xc, 0x2, 0x2, 0xdf, 0x2b, 0x3, 0x2, 
    0x2, 0x2, 0xe0, 0xe1, 0x7, 0x35, 0x2, 0x2, 0xe1, 0x2d, 0x3, 0x2, 0x2, 
    0x2, 0xe2, 0xe3, 0x7, 0x35, 0x2, 0x2, 0xe3, 0x2f, 0x3, 0x2, 0x2, 0x2, 
    0xe4, 0xe5, 0x7, 0x35, 0x2, 0x2, 0xe5, 0x31, 0x3, 0x2, 0x2, 0x2, 0x19, 
    0x34, 0x3e, 0x45, 0x4a, 0x50, 0x55, 0x5c, 0x68, 0x6b, 0x6e, 0x74, 0x7b, 
    0x83, 0x8b, 0x97, 0x9d, 0xa7, 0xaa, 0xac, 0xba, 0xc4, 0xc6, 0xda, 
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
