
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
    setState(52);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case FilterExpressionSyntaxParser::T__0:
      case FilterExpressionSyntaxParser::T__2:
      case FilterExpressionSyntaxParser::T__3:
      case FilterExpressionSyntaxParser::T__4:
      case FilterExpressionSyntaxParser::T__6:
      case FilterExpressionSyntaxParser::T__7:
      case FilterExpressionSyntaxParser::K_ABS:
      case FilterExpressionSyntaxParser::K_CEILING:
      case FilterExpressionSyntaxParser::K_COALESCE:
      case FilterExpressionSyntaxParser::K_CONVERT:
      case FilterExpressionSyntaxParser::K_CONTAINS:
      case FilterExpressionSyntaxParser::K_DATEADD:
      case FilterExpressionSyntaxParser::K_DATEDIFF:
      case FilterExpressionSyntaxParser::K_DATEPART:
      case FilterExpressionSyntaxParser::K_ENDSWITH:
      case FilterExpressionSyntaxParser::K_FILTER:
      case FilterExpressionSyntaxParser::K_FLOOR:
      case FilterExpressionSyntaxParser::K_IIF:
      case FilterExpressionSyntaxParser::K_INDEXOF:
      case FilterExpressionSyntaxParser::K_ISDATE:
      case FilterExpressionSyntaxParser::K_ISINTEGER:
      case FilterExpressionSyntaxParser::K_ISGUID:
      case FilterExpressionSyntaxParser::K_ISNULL:
      case FilterExpressionSyntaxParser::K_ISNUMERIC:
      case FilterExpressionSyntaxParser::K_LASTINDEXOF:
      case FilterExpressionSyntaxParser::K_LEN:
      case FilterExpressionSyntaxParser::K_LOWER:
      case FilterExpressionSyntaxParser::K_MAXOF:
      case FilterExpressionSyntaxParser::K_MINOF:
      case FilterExpressionSyntaxParser::K_NOT:
      case FilterExpressionSyntaxParser::K_NOW:
      case FilterExpressionSyntaxParser::K_NULL:
      case FilterExpressionSyntaxParser::K_POWER:
      case FilterExpressionSyntaxParser::K_REGEXMATCH:
      case FilterExpressionSyntaxParser::K_REGEXVAL:
      case FilterExpressionSyntaxParser::K_REPLACE:
      case FilterExpressionSyntaxParser::K_REVERSE:
      case FilterExpressionSyntaxParser::K_ROUND:
      case FilterExpressionSyntaxParser::K_SQRT:
      case FilterExpressionSyntaxParser::K_SPLIT:
      case FilterExpressionSyntaxParser::K_STARTSWITH:
      case FilterExpressionSyntaxParser::K_STRCOUNT:
      case FilterExpressionSyntaxParser::K_STRCMP:
      case FilterExpressionSyntaxParser::K_SUBSTR:
      case FilterExpressionSyntaxParser::K_TRIM:
      case FilterExpressionSyntaxParser::K_TRIMLEFT:
      case FilterExpressionSyntaxParser::K_TRIMRIGHT:
      case FilterExpressionSyntaxParser::K_UPPER:
      case FilterExpressionSyntaxParser::K_UTCNOW:
      case FilterExpressionSyntaxParser::BOOLEAN_LITERAL:
      case FilterExpressionSyntaxParser::IDENTIFIER:
      case FilterExpressionSyntaxParser::INTEGER_LITERAL:
      case FilterExpressionSyntaxParser::NUMERIC_LITERAL:
      case FilterExpressionSyntaxParser::GUID_LITERAL:
      case FilterExpressionSyntaxParser::MEASUREMENT_KEY_LITERAL:
      case FilterExpressionSyntaxParser::POINT_TAG_LITERAL:
      case FilterExpressionSyntaxParser::STRING_LITERAL:
      case FilterExpressionSyntaxParser::DATETIME_LITERAL: {
        setState(50);
        filterExpressionStatementList();
        break;
      }

      case FilterExpressionSyntaxParser::UNEXPECTED_CHAR: {
        setState(51);
        error();
        break;
      }

    default:
      throw NoViableAltException(this);
    }
    setState(54);
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
    setState(56);
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
    setState(62);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while (_la == FilterExpressionSyntaxParser::T__0) {
      setState(59);
      match(FilterExpressionSyntaxParser::T__0);
      setState(64);
      _errHandler->sync(this);
      _la = _input->LA(1);
    }
    setState(65);
    filterExpressionStatement();
    setState(74);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 3, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        setState(67); 
        _errHandler->sync(this);
        _la = _input->LA(1);
        do {
          setState(66);
          match(FilterExpressionSyntaxParser::T__0);
          setState(69); 
          _errHandler->sync(this);
          _la = _input->LA(1);
        } while (_la == FilterExpressionSyntaxParser::T__0);
        setState(71);
        filterExpressionStatement(); 
      }
      setState(76);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 3, _ctx);
    }
    setState(80);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while (_la == FilterExpressionSyntaxParser::T__0) {
      setState(77);
      match(FilterExpressionSyntaxParser::T__0);
      setState(82);
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

FilterExpressionSyntaxParser::ExpressionContext* FilterExpressionSyntaxParser::FilterExpressionStatementContext::expression() {
  return getRuleContext<FilterExpressionSyntaxParser::ExpressionContext>(0);
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
    setState(86);
    _errHandler->sync(this);
    switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 5, _ctx)) {
    case 1: {
      enterOuterAlt(_localctx, 1);
      setState(83);
      identifierStatement();
      break;
    }

    case 2: {
      enterOuterAlt(_localctx, 2);
      setState(84);
      filterStatement();
      break;
    }

    case 3: {
      enterOuterAlt(_localctx, 3);
      setState(85);
      expression(0);
      break;
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
    setState(88);
    _la = _input->LA(1);
    if (!(((((_la - 87) & ~ 0x3fULL) == 0) &&
      ((1ULL << (_la - 87)) & ((1ULL << (FilterExpressionSyntaxParser::GUID_LITERAL - 87))
      | (1ULL << (FilterExpressionSyntaxParser::MEASUREMENT_KEY_LITERAL - 87))
      | (1ULL << (FilterExpressionSyntaxParser::POINT_TAG_LITERAL - 87)))) != 0))) {
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
    setState(90);
    match(FilterExpressionSyntaxParser::K_FILTER);
    setState(93);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_TOP) {
      setState(91);
      match(FilterExpressionSyntaxParser::K_TOP);
      setState(92);
      topLimit();
    }
    setState(95);
    tableName();
    setState(96);
    match(FilterExpressionSyntaxParser::K_WHERE);
    setState(97);
    expression(0);
    setState(108);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_ORDER) {
      setState(98);
      match(FilterExpressionSyntaxParser::K_ORDER);
      setState(99);
      match(FilterExpressionSyntaxParser::K_BY);
      setState(100);
      orderingTerm();
      setState(105);
      _errHandler->sync(this);
      _la = _input->LA(1);
      while (_la == FilterExpressionSyntaxParser::T__1) {
        setState(101);
        match(FilterExpressionSyntaxParser::T__1);
        setState(102);
        orderingTerm();
        setState(107);
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
    setState(111);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::T__2

    || _la == FilterExpressionSyntaxParser::T__3) {
      setState(110);
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
    setState(113);
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
    setState(115);
    orderByColumnName();
    setState(117);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if (_la == FilterExpressionSyntaxParser::K_ASC

    || _la == FilterExpressionSyntaxParser::K_DESC) {
      setState(116);
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
    setState(119);
    expression(0);
    setState(124);
    _errHandler->sync(this);
    _la = _input->LA(1);
    while (_la == FilterExpressionSyntaxParser::T__1) {
      setState(120);
      match(FilterExpressionSyntaxParser::T__1);
      setState(121);
      expression(0);
      setState(126);
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
    setState(132);
    _errHandler->sync(this);
    switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 12, _ctx)) {
    case 1: {
      setState(128);
      notOperator();
      setState(129);
      expression(3);
      break;
    }

    case 2: {
      setState(131);
      predicateExpression(0);
      break;
    }

    }
    _ctx->stop = _input->LT(-1);
    setState(140);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 13, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        if (!_parseListeners.empty())
          triggerExitRuleEvent();
        previousContext = _localctx;
        _localctx = _tracker.createInstance<ExpressionContext>(parentContext, parentState);
        pushNewRecursionContext(_localctx, startState, RuleExpression);
        setState(134);

        if (!(precpred(_ctx, 2))) throw FailedPredicateException(this, "precpred(_ctx, 2)");
        setState(135);
        logicalOperator();
        setState(136);
        expression(3); 
      }
      setState(142);
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

FilterExpressionSyntaxParser::ExactMatchOperatorContext* FilterExpressionSyntaxParser::PredicateExpressionContext::exactMatchOperator() {
  return getRuleContext<FilterExpressionSyntaxParser::ExactMatchOperatorContext>(0);
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
    setState(144);
    valueExpression(0);
    _ctx->stop = _input->LT(-1);
    setState(179);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 20, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        if (!_parseListeners.empty())
          triggerExitRuleEvent();
        previousContext = _localctx;
        setState(177);
        _errHandler->sync(this);
        switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 19, _ctx)) {
        case 1: {
          _localctx = _tracker.createInstance<PredicateExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RulePredicateExpression);
          setState(146);

          if (!(precpred(_ctx, 3))) throw FailedPredicateException(this, "precpred(_ctx, 3)");
          setState(147);
          comparisonOperator();
          setState(148);
          predicateExpression(4);
          break;
        }

        case 2: {
          _localctx = _tracker.createInstance<PredicateExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RulePredicateExpression);
          setState(150);

          if (!(precpred(_ctx, 2))) throw FailedPredicateException(this, "precpred(_ctx, 2)");
          setState(152);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::K_NOT) {
            setState(151);
            match(FilterExpressionSyntaxParser::K_NOT);
          }
          setState(154);
          match(FilterExpressionSyntaxParser::K_LIKE);
          setState(156);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::T__8

          || _la == FilterExpressionSyntaxParser::K_BINARY) {
            setState(155);
            exactMatchOperator();
          }
          setState(158);
          predicateExpression(3);
          break;
        }

        case 3: {
          _localctx = _tracker.createInstance<PredicateExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RulePredicateExpression);
          setState(159);

          if (!(precpred(_ctx, 5))) throw FailedPredicateException(this, "precpred(_ctx, 5)");
          setState(161);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::K_NOT) {
            setState(160);
            match(FilterExpressionSyntaxParser::K_NOT);
          }
          setState(163);
          match(FilterExpressionSyntaxParser::K_IN);
          setState(165);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::T__8

          || _la == FilterExpressionSyntaxParser::K_BINARY) {
            setState(164);
            exactMatchOperator();
          }
          setState(167);
          match(FilterExpressionSyntaxParser::T__4);
          setState(168);
          expressionList();
          setState(169);
          match(FilterExpressionSyntaxParser::T__5);
          break;
        }

        case 4: {
          _localctx = _tracker.createInstance<PredicateExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RulePredicateExpression);
          setState(171);

          if (!(precpred(_ctx, 4))) throw FailedPredicateException(this, "precpred(_ctx, 4)");
          setState(172);
          match(FilterExpressionSyntaxParser::K_IS);
          setState(174);
          _errHandler->sync(this);

          _la = _input->LA(1);
          if (_la == FilterExpressionSyntaxParser::K_NOT) {
            setState(173);
            match(FilterExpressionSyntaxParser::K_NOT);
          }
          setState(176);
          match(FilterExpressionSyntaxParser::K_NULL);
          break;
        }

        } 
      }
      setState(181);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 20, _ctx);
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
    setState(193);
    _errHandler->sync(this);
    switch (_input->LA(1)) {
      case FilterExpressionSyntaxParser::K_NULL:
      case FilterExpressionSyntaxParser::BOOLEAN_LITERAL:
      case FilterExpressionSyntaxParser::INTEGER_LITERAL:
      case FilterExpressionSyntaxParser::NUMERIC_LITERAL:
      case FilterExpressionSyntaxParser::GUID_LITERAL:
      case FilterExpressionSyntaxParser::STRING_LITERAL:
      case FilterExpressionSyntaxParser::DATETIME_LITERAL: {
        setState(183);
        literalValue();
        break;
      }

      case FilterExpressionSyntaxParser::IDENTIFIER: {
        setState(184);
        columnName();
        break;
      }

      case FilterExpressionSyntaxParser::K_ABS:
      case FilterExpressionSyntaxParser::K_CEILING:
      case FilterExpressionSyntaxParser::K_COALESCE:
      case FilterExpressionSyntaxParser::K_CONVERT:
      case FilterExpressionSyntaxParser::K_CONTAINS:
      case FilterExpressionSyntaxParser::K_DATEADD:
      case FilterExpressionSyntaxParser::K_DATEDIFF:
      case FilterExpressionSyntaxParser::K_DATEPART:
      case FilterExpressionSyntaxParser::K_ENDSWITH:
      case FilterExpressionSyntaxParser::K_FLOOR:
      case FilterExpressionSyntaxParser::K_IIF:
      case FilterExpressionSyntaxParser::K_INDEXOF:
      case FilterExpressionSyntaxParser::K_ISDATE:
      case FilterExpressionSyntaxParser::K_ISINTEGER:
      case FilterExpressionSyntaxParser::K_ISGUID:
      case FilterExpressionSyntaxParser::K_ISNULL:
      case FilterExpressionSyntaxParser::K_ISNUMERIC:
      case FilterExpressionSyntaxParser::K_LASTINDEXOF:
      case FilterExpressionSyntaxParser::K_LEN:
      case FilterExpressionSyntaxParser::K_LOWER:
      case FilterExpressionSyntaxParser::K_MAXOF:
      case FilterExpressionSyntaxParser::K_MINOF:
      case FilterExpressionSyntaxParser::K_NOW:
      case FilterExpressionSyntaxParser::K_POWER:
      case FilterExpressionSyntaxParser::K_REGEXMATCH:
      case FilterExpressionSyntaxParser::K_REGEXVAL:
      case FilterExpressionSyntaxParser::K_REPLACE:
      case FilterExpressionSyntaxParser::K_REVERSE:
      case FilterExpressionSyntaxParser::K_ROUND:
      case FilterExpressionSyntaxParser::K_SQRT:
      case FilterExpressionSyntaxParser::K_SPLIT:
      case FilterExpressionSyntaxParser::K_STARTSWITH:
      case FilterExpressionSyntaxParser::K_STRCOUNT:
      case FilterExpressionSyntaxParser::K_STRCMP:
      case FilterExpressionSyntaxParser::K_SUBSTR:
      case FilterExpressionSyntaxParser::K_TRIM:
      case FilterExpressionSyntaxParser::K_TRIMLEFT:
      case FilterExpressionSyntaxParser::K_TRIMRIGHT:
      case FilterExpressionSyntaxParser::K_UPPER:
      case FilterExpressionSyntaxParser::K_UTCNOW: {
        setState(185);
        functionExpression();
        break;
      }

      case FilterExpressionSyntaxParser::T__2:
      case FilterExpressionSyntaxParser::T__3:
      case FilterExpressionSyntaxParser::T__6:
      case FilterExpressionSyntaxParser::T__7:
      case FilterExpressionSyntaxParser::K_NOT: {
        setState(186);
        unaryOperator();
        setState(187);
        valueExpression(4);
        break;
      }

      case FilterExpressionSyntaxParser::T__4: {
        setState(189);
        match(FilterExpressionSyntaxParser::T__4);
        setState(190);
        expression(0);
        setState(191);
        match(FilterExpressionSyntaxParser::T__5);
        break;
      }

    default:
      throw NoViableAltException(this);
    }
    _ctx->stop = _input->LT(-1);
    setState(205);
    _errHandler->sync(this);
    alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 23, _ctx);
    while (alt != 2 && alt != atn::ATN::INVALID_ALT_NUMBER) {
      if (alt == 1) {
        if (!_parseListeners.empty())
          triggerExitRuleEvent();
        previousContext = _localctx;
        setState(203);
        _errHandler->sync(this);
        switch (getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 22, _ctx)) {
        case 1: {
          _localctx = _tracker.createInstance<ValueExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleValueExpression);
          setState(195);

          if (!(precpred(_ctx, 2))) throw FailedPredicateException(this, "precpred(_ctx, 2)");
          setState(196);
          mathOperator();
          setState(197);
          valueExpression(3);
          break;
        }

        case 2: {
          _localctx = _tracker.createInstance<ValueExpressionContext>(parentContext, parentState);
          pushNewRecursionContext(_localctx, startState, RuleValueExpression);
          setState(199);

          if (!(precpred(_ctx, 1))) throw FailedPredicateException(this, "precpred(_ctx, 1)");
          setState(200);
          bitwiseOperator();
          setState(201);
          valueExpression(2);
          break;
        }

        } 
      }
      setState(207);
      _errHandler->sync(this);
      alt = getInterpreter<atn::ParserATNSimulator>()->adaptivePredict(_input, 23, _ctx);
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
    setState(208);
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
    setState(210);
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

//----------------- ExactMatchOperatorContext ------------------------------------------------------------------

FilterExpressionSyntaxParser::ExactMatchOperatorContext::ExactMatchOperatorContext(ParserRuleContext *parent, size_t invokingState)
  : ParserRuleContext(parent, invokingState) {
}

tree::TerminalNode* FilterExpressionSyntaxParser::ExactMatchOperatorContext::K_BINARY() {
  return getToken(FilterExpressionSyntaxParser::K_BINARY, 0);
}


size_t FilterExpressionSyntaxParser::ExactMatchOperatorContext::getRuleIndex() const {
  return FilterExpressionSyntaxParser::RuleExactMatchOperator;
}

void FilterExpressionSyntaxParser::ExactMatchOperatorContext::enterRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->enterExactMatchOperator(this);
}

void FilterExpressionSyntaxParser::ExactMatchOperatorContext::exitRule(tree::ParseTreeListener *listener) {
  auto parserListener = dynamic_cast<FilterExpressionSyntaxListener *>(listener);
  if (parserListener != nullptr)
    parserListener->exitExactMatchOperator(this);
}

FilterExpressionSyntaxParser::ExactMatchOperatorContext* FilterExpressionSyntaxParser::exactMatchOperator() {
  ExactMatchOperatorContext *_localctx = _tracker.createInstance<ExactMatchOperatorContext>(_ctx, getState());
  enterRule(_localctx, 28, FilterExpressionSyntaxParser::RuleExactMatchOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(212);
    _la = _input->LA(1);
    if (!(_la == FilterExpressionSyntaxParser::T__8

    || _la == FilterExpressionSyntaxParser::K_BINARY)) {
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
  enterRule(_localctx, 30, FilterExpressionSyntaxParser::RuleComparisonOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(214);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__8)
      | (1ULL << FilterExpressionSyntaxParser::T__9)
      | (1ULL << FilterExpressionSyntaxParser::T__10)
      | (1ULL << FilterExpressionSyntaxParser::T__11)
      | (1ULL << FilterExpressionSyntaxParser::T__12)
      | (1ULL << FilterExpressionSyntaxParser::T__13)
      | (1ULL << FilterExpressionSyntaxParser::T__14)
      | (1ULL << FilterExpressionSyntaxParser::T__15)
      | (1ULL << FilterExpressionSyntaxParser::T__16)
      | (1ULL << FilterExpressionSyntaxParser::T__17))) != 0))) {
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
  enterRule(_localctx, 32, FilterExpressionSyntaxParser::RuleLogicalOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(216);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__18)
      | (1ULL << FilterExpressionSyntaxParser::T__19)
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
  enterRule(_localctx, 34, FilterExpressionSyntaxParser::RuleBitwiseOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(218);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__20)
      | (1ULL << FilterExpressionSyntaxParser::T__21)
      | (1ULL << FilterExpressionSyntaxParser::T__22)
      | (1ULL << FilterExpressionSyntaxParser::T__23))) != 0))) {
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
  enterRule(_localctx, 36, FilterExpressionSyntaxParser::RuleMathOperator);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(220);
    _la = _input->LA(1);
    if (!((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__2)
      | (1ULL << FilterExpressionSyntaxParser::T__3)
      | (1ULL << FilterExpressionSyntaxParser::T__24)
      | (1ULL << FilterExpressionSyntaxParser::T__25)
      | (1ULL << FilterExpressionSyntaxParser::T__26))) != 0))) {
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

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_ABS() {
  return getToken(FilterExpressionSyntaxParser::K_ABS, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_CEILING() {
  return getToken(FilterExpressionSyntaxParser::K_CEILING, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_COALESCE() {
  return getToken(FilterExpressionSyntaxParser::K_COALESCE, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_CONVERT() {
  return getToken(FilterExpressionSyntaxParser::K_CONVERT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_CONTAINS() {
  return getToken(FilterExpressionSyntaxParser::K_CONTAINS, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_DATEADD() {
  return getToken(FilterExpressionSyntaxParser::K_DATEADD, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_DATEDIFF() {
  return getToken(FilterExpressionSyntaxParser::K_DATEDIFF, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_DATEPART() {
  return getToken(FilterExpressionSyntaxParser::K_DATEPART, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_ENDSWITH() {
  return getToken(FilterExpressionSyntaxParser::K_ENDSWITH, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_FLOOR() {
  return getToken(FilterExpressionSyntaxParser::K_FLOOR, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_IIF() {
  return getToken(FilterExpressionSyntaxParser::K_IIF, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_INDEXOF() {
  return getToken(FilterExpressionSyntaxParser::K_INDEXOF, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_ISDATE() {
  return getToken(FilterExpressionSyntaxParser::K_ISDATE, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_ISINTEGER() {
  return getToken(FilterExpressionSyntaxParser::K_ISINTEGER, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_ISGUID() {
  return getToken(FilterExpressionSyntaxParser::K_ISGUID, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_ISNULL() {
  return getToken(FilterExpressionSyntaxParser::K_ISNULL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_ISNUMERIC() {
  return getToken(FilterExpressionSyntaxParser::K_ISNUMERIC, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_LASTINDEXOF() {
  return getToken(FilterExpressionSyntaxParser::K_LASTINDEXOF, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_LEN() {
  return getToken(FilterExpressionSyntaxParser::K_LEN, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_LOWER() {
  return getToken(FilterExpressionSyntaxParser::K_LOWER, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_MAXOF() {
  return getToken(FilterExpressionSyntaxParser::K_MAXOF, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_MINOF() {
  return getToken(FilterExpressionSyntaxParser::K_MINOF, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_NOW() {
  return getToken(FilterExpressionSyntaxParser::K_NOW, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_POWER() {
  return getToken(FilterExpressionSyntaxParser::K_POWER, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_REGEXMATCH() {
  return getToken(FilterExpressionSyntaxParser::K_REGEXMATCH, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_REGEXVAL() {
  return getToken(FilterExpressionSyntaxParser::K_REGEXVAL, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_REPLACE() {
  return getToken(FilterExpressionSyntaxParser::K_REPLACE, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_REVERSE() {
  return getToken(FilterExpressionSyntaxParser::K_REVERSE, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_ROUND() {
  return getToken(FilterExpressionSyntaxParser::K_ROUND, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_SPLIT() {
  return getToken(FilterExpressionSyntaxParser::K_SPLIT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_SQRT() {
  return getToken(FilterExpressionSyntaxParser::K_SQRT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_STARTSWITH() {
  return getToken(FilterExpressionSyntaxParser::K_STARTSWITH, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_STRCOUNT() {
  return getToken(FilterExpressionSyntaxParser::K_STRCOUNT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_STRCMP() {
  return getToken(FilterExpressionSyntaxParser::K_STRCMP, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_SUBSTR() {
  return getToken(FilterExpressionSyntaxParser::K_SUBSTR, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_TRIM() {
  return getToken(FilterExpressionSyntaxParser::K_TRIM, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_TRIMLEFT() {
  return getToken(FilterExpressionSyntaxParser::K_TRIMLEFT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_TRIMRIGHT() {
  return getToken(FilterExpressionSyntaxParser::K_TRIMRIGHT, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_UPPER() {
  return getToken(FilterExpressionSyntaxParser::K_UPPER, 0);
}

tree::TerminalNode* FilterExpressionSyntaxParser::FunctionNameContext::K_UTCNOW() {
  return getToken(FilterExpressionSyntaxParser::K_UTCNOW, 0);
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
  enterRule(_localctx, 38, FilterExpressionSyntaxParser::RuleFunctionName);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(222);
    _la = _input->LA(1);
    if (!(((((_la - 28) & ~ 0x3fULL) == 0) &&
      ((1ULL << (_la - 28)) & ((1ULL << (FilterExpressionSyntaxParser::K_ABS - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_CEILING - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_COALESCE - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_CONVERT - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_CONTAINS - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_DATEADD - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_DATEDIFF - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_DATEPART - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_ENDSWITH - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_FLOOR - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_IIF - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_INDEXOF - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_ISDATE - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_ISINTEGER - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_ISGUID - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_ISNULL - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_ISNUMERIC - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_LASTINDEXOF - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_LEN - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_LOWER - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_MAXOF - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_MINOF - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_NOW - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_POWER - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_REGEXMATCH - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_REGEXVAL - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_REPLACE - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_REVERSE - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_ROUND - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_SQRT - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_SPLIT - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_STARTSWITH - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_STRCOUNT - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_STRCMP - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_SUBSTR - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_TRIM - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_TRIMLEFT - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_TRIMRIGHT - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_UPPER - 28))
      | (1ULL << (FilterExpressionSyntaxParser::K_UTCNOW - 28)))) != 0))) {
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
  enterRule(_localctx, 40, FilterExpressionSyntaxParser::RuleFunctionExpression);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(224);
    functionName();
    setState(225);
    match(FilterExpressionSyntaxParser::T__4);
    setState(227);
    _errHandler->sync(this);

    _la = _input->LA(1);
    if ((((_la & ~ 0x3fULL) == 0) &&
      ((1ULL << _la) & ((1ULL << FilterExpressionSyntaxParser::T__2)
      | (1ULL << FilterExpressionSyntaxParser::T__3)
      | (1ULL << FilterExpressionSyntaxParser::T__4)
      | (1ULL << FilterExpressionSyntaxParser::T__6)
      | (1ULL << FilterExpressionSyntaxParser::T__7)
      | (1ULL << FilterExpressionSyntaxParser::K_ABS)
      | (1ULL << FilterExpressionSyntaxParser::K_CEILING)
      | (1ULL << FilterExpressionSyntaxParser::K_COALESCE)
      | (1ULL << FilterExpressionSyntaxParser::K_CONVERT)
      | (1ULL << FilterExpressionSyntaxParser::K_CONTAINS)
      | (1ULL << FilterExpressionSyntaxParser::K_DATEADD)
      | (1ULL << FilterExpressionSyntaxParser::K_DATEDIFF)
      | (1ULL << FilterExpressionSyntaxParser::K_DATEPART)
      | (1ULL << FilterExpressionSyntaxParser::K_ENDSWITH)
      | (1ULL << FilterExpressionSyntaxParser::K_FLOOR)
      | (1ULL << FilterExpressionSyntaxParser::K_IIF)
      | (1ULL << FilterExpressionSyntaxParser::K_INDEXOF)
      | (1ULL << FilterExpressionSyntaxParser::K_ISDATE)
      | (1ULL << FilterExpressionSyntaxParser::K_ISINTEGER)
      | (1ULL << FilterExpressionSyntaxParser::K_ISGUID)
      | (1ULL << FilterExpressionSyntaxParser::K_ISNULL)
      | (1ULL << FilterExpressionSyntaxParser::K_ISNUMERIC)
      | (1ULL << FilterExpressionSyntaxParser::K_LASTINDEXOF)
      | (1ULL << FilterExpressionSyntaxParser::K_LEN)
      | (1ULL << FilterExpressionSyntaxParser::K_LOWER)
      | (1ULL << FilterExpressionSyntaxParser::K_MAXOF)
      | (1ULL << FilterExpressionSyntaxParser::K_MINOF)
      | (1ULL << FilterExpressionSyntaxParser::K_NOT)
      | (1ULL << FilterExpressionSyntaxParser::K_NOW)
      | (1ULL << FilterExpressionSyntaxParser::K_NULL))) != 0) || ((((_la - 64) & ~ 0x3fULL) == 0) &&
      ((1ULL << (_la - 64)) & ((1ULL << (FilterExpressionSyntaxParser::K_POWER - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_REGEXMATCH - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_REGEXVAL - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_REPLACE - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_REVERSE - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_ROUND - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_SQRT - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_SPLIT - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_STARTSWITH - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_STRCOUNT - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_STRCMP - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_SUBSTR - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_TRIM - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_TRIMLEFT - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_TRIMRIGHT - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_UPPER - 64))
      | (1ULL << (FilterExpressionSyntaxParser::K_UTCNOW - 64))
      | (1ULL << (FilterExpressionSyntaxParser::BOOLEAN_LITERAL - 64))
      | (1ULL << (FilterExpressionSyntaxParser::IDENTIFIER - 64))
      | (1ULL << (FilterExpressionSyntaxParser::INTEGER_LITERAL - 64))
      | (1ULL << (FilterExpressionSyntaxParser::NUMERIC_LITERAL - 64))
      | (1ULL << (FilterExpressionSyntaxParser::GUID_LITERAL - 64))
      | (1ULL << (FilterExpressionSyntaxParser::STRING_LITERAL - 64))
      | (1ULL << (FilterExpressionSyntaxParser::DATETIME_LITERAL - 64)))) != 0)) {
      setState(226);
      expressionList();
    }
    setState(229);
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
  enterRule(_localctx, 42, FilterExpressionSyntaxParser::RuleLiteralValue);
  size_t _la = 0;

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(231);
    _la = _input->LA(1);
    if (!(((((_la - 61) & ~ 0x3fULL) == 0) &&
      ((1ULL << (_la - 61)) & ((1ULL << (FilterExpressionSyntaxParser::K_NULL - 61))
      | (1ULL << (FilterExpressionSyntaxParser::BOOLEAN_LITERAL - 61))
      | (1ULL << (FilterExpressionSyntaxParser::INTEGER_LITERAL - 61))
      | (1ULL << (FilterExpressionSyntaxParser::NUMERIC_LITERAL - 61))
      | (1ULL << (FilterExpressionSyntaxParser::GUID_LITERAL - 61))
      | (1ULL << (FilterExpressionSyntaxParser::STRING_LITERAL - 61))
      | (1ULL << (FilterExpressionSyntaxParser::DATETIME_LITERAL - 61)))) != 0))) {
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
  enterRule(_localctx, 44, FilterExpressionSyntaxParser::RuleTableName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(233);
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
  enterRule(_localctx, 46, FilterExpressionSyntaxParser::RuleColumnName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(235);
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
  enterRule(_localctx, 48, FilterExpressionSyntaxParser::RuleOrderByColumnName);

  auto onExit = finally([=] {
    exitRule();
  });
  try {
    enterOuterAlt(_localctx, 1);
    setState(237);
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
  "notOperator", "unaryOperator", "exactMatchOperator", "comparisonOperator", 
  "logicalOperator", "bitwiseOperator", "mathOperator", "functionName", 
  "functionExpression", "literalValue", "tableName", "columnName", "orderByColumnName"
};

std::vector<std::string> FilterExpressionSyntaxParser::_literalNames = {
  "", "';'", "','", "'-'", "'+'", "'('", "')'", "'!'", "'~'", "'==='", "'<'", 
  "'<='", "'>'", "'>='", "'='", "'=='", "'!='", "'!=='", "'<>'", "'&&'", 
  "'||'", "'<<'", "'>>'", "'&'", "'|'", "'*'", "'/'", "'%'"
};

std::vector<std::string> FilterExpressionSyntaxParser::_symbolicNames = {
  "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 
  "", "", "", "", "", "", "", "", "", "", "K_ABS", "K_AND", "K_ASC", "K_BINARY", 
  "K_BY", "K_CEILING", "K_COALESCE", "K_CONVERT", "K_CONTAINS", "K_DATEADD", 
  "K_DATEDIFF", "K_DATEPART", "K_DESC", "K_ENDSWITH", "K_FILTER", "K_FLOOR", 
  "K_IIF", "K_IN", "K_INDEXOF", "K_IS", "K_ISDATE", "K_ISINTEGER", "K_ISGUID", 
  "K_ISNULL", "K_ISNUMERIC", "K_LASTINDEXOF", "K_LEN", "K_LIKE", "K_LOWER", 
  "K_MAXOF", "K_MINOF", "K_NOT", "K_NOW", "K_NULL", "K_OR", "K_ORDER", "K_POWER", 
  "K_REGEXMATCH", "K_REGEXVAL", "K_REPLACE", "K_REVERSE", "K_ROUND", "K_SQRT", 
  "K_SPLIT", "K_STARTSWITH", "K_STRCOUNT", "K_STRCMP", "K_SUBSTR", "K_TOP", 
  "K_TRIM", "K_TRIMLEFT", "K_TRIMRIGHT", "K_UPPER", "K_UTCNOW", "K_WHERE", 
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
    0x3, 0x61, 0xf2, 0x4, 0x2, 0x9, 0x2, 0x4, 0x3, 0x9, 0x3, 0x4, 0x4, 0x9, 
    0x4, 0x4, 0x5, 0x9, 0x5, 0x4, 0x6, 0x9, 0x6, 0x4, 0x7, 0x9, 0x7, 0x4, 
    0x8, 0x9, 0x8, 0x4, 0x9, 0x9, 0x9, 0x4, 0xa, 0x9, 0xa, 0x4, 0xb, 0x9, 
    0xb, 0x4, 0xc, 0x9, 0xc, 0x4, 0xd, 0x9, 0xd, 0x4, 0xe, 0x9, 0xe, 0x4, 
    0xf, 0x9, 0xf, 0x4, 0x10, 0x9, 0x10, 0x4, 0x11, 0x9, 0x11, 0x4, 0x12, 
    0x9, 0x12, 0x4, 0x13, 0x9, 0x13, 0x4, 0x14, 0x9, 0x14, 0x4, 0x15, 0x9, 
    0x15, 0x4, 0x16, 0x9, 0x16, 0x4, 0x17, 0x9, 0x17, 0x4, 0x18, 0x9, 0x18, 
    0x4, 0x19, 0x9, 0x19, 0x4, 0x1a, 0x9, 0x1a, 0x3, 0x2, 0x3, 0x2, 0x5, 
    0x2, 0x37, 0xa, 0x2, 0x3, 0x2, 0x3, 0x2, 0x3, 0x3, 0x3, 0x3, 0x3, 0x3, 
    0x3, 0x4, 0x7, 0x4, 0x3f, 0xa, 0x4, 0xc, 0x4, 0xe, 0x4, 0x42, 0xb, 0x4, 
    0x3, 0x4, 0x3, 0x4, 0x6, 0x4, 0x46, 0xa, 0x4, 0xd, 0x4, 0xe, 0x4, 0x47, 
    0x3, 0x4, 0x7, 0x4, 0x4b, 0xa, 0x4, 0xc, 0x4, 0xe, 0x4, 0x4e, 0xb, 0x4, 
    0x3, 0x4, 0x7, 0x4, 0x51, 0xa, 0x4, 0xc, 0x4, 0xe, 0x4, 0x54, 0xb, 0x4, 
    0x3, 0x5, 0x3, 0x5, 0x3, 0x5, 0x5, 0x5, 0x59, 0xa, 0x5, 0x3, 0x6, 0x3, 
    0x6, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x5, 0x7, 0x60, 0xa, 0x7, 0x3, 0x7, 
    0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 0x3, 0x7, 
    0x7, 0x7, 0x6a, 0xa, 0x7, 0xc, 0x7, 0xe, 0x7, 0x6d, 0xb, 0x7, 0x5, 0x7, 
    0x6f, 0xa, 0x7, 0x3, 0x8, 0x5, 0x8, 0x72, 0xa, 0x8, 0x3, 0x8, 0x3, 0x8, 
    0x3, 0x9, 0x3, 0x9, 0x5, 0x9, 0x78, 0xa, 0x9, 0x3, 0xa, 0x3, 0xa, 0x3, 
    0xa, 0x7, 0xa, 0x7d, 0xa, 0xa, 0xc, 0xa, 0xe, 0xa, 0x80, 0xb, 0xa, 0x3, 
    0xb, 0x3, 0xb, 0x3, 0xb, 0x3, 0xb, 0x3, 0xb, 0x5, 0xb, 0x87, 0xa, 0xb, 
    0x3, 0xb, 0x3, 0xb, 0x3, 0xb, 0x3, 0xb, 0x7, 0xb, 0x8d, 0xa, 0xb, 0xc, 
    0xb, 0xe, 0xb, 0x90, 0xb, 0xb, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 
    0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x5, 0xc, 0x9b, 0xa, 
    0xc, 0x3, 0xc, 0x3, 0xc, 0x5, 0xc, 0x9f, 0xa, 0xc, 0x3, 0xc, 0x3, 0xc, 
    0x3, 0xc, 0x5, 0xc, 0xa4, 0xa, 0xc, 0x3, 0xc, 0x3, 0xc, 0x5, 0xc, 0xa8, 
    0xa, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 0x3, 0xc, 
    0x3, 0xc, 0x5, 0xc, 0xb1, 0xa, 0xc, 0x3, 0xc, 0x7, 0xc, 0xb4, 0xa, 0xc, 
    0xc, 0xc, 0xe, 0xc, 0xb7, 0xb, 0xc, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 
    0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 
    0xd, 0x5, 0xd, 0xc4, 0xa, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 
    0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x3, 0xd, 0x7, 0xd, 0xce, 0xa, 0xd, 0xc, 
    0xd, 0xe, 0xd, 0xd1, 0xb, 0xd, 0x3, 0xe, 0x3, 0xe, 0x3, 0xf, 0x3, 0xf, 
    0x3, 0x10, 0x3, 0x10, 0x3, 0x11, 0x3, 0x11, 0x3, 0x12, 0x3, 0x12, 0x3, 
    0x13, 0x3, 0x13, 0x3, 0x14, 0x3, 0x14, 0x3, 0x15, 0x3, 0x15, 0x3, 0x16, 
    0x3, 0x16, 0x3, 0x16, 0x5, 0x16, 0xe6, 0xa, 0x16, 0x3, 0x16, 0x3, 0x16, 
    0x3, 0x17, 0x3, 0x17, 0x3, 0x18, 0x3, 0x18, 0x3, 0x19, 0x3, 0x19, 0x3, 
    0x1a, 0x3, 0x1a, 0x3, 0x1a, 0x2, 0x5, 0x14, 0x16, 0x18, 0x1b, 0x2, 0x4, 
    0x6, 0x8, 0xa, 0xc, 0xe, 0x10, 0x12, 0x14, 0x16, 0x18, 0x1a, 0x1c, 0x1e, 
    0x20, 0x22, 0x24, 0x26, 0x28, 0x2a, 0x2c, 0x2e, 0x30, 0x32, 0x2, 0xe, 
    0x3, 0x2, 0x59, 0x5b, 0x3, 0x2, 0x5, 0x6, 0x4, 0x2, 0x20, 0x20, 0x2a, 
    0x2a, 0x4, 0x2, 0x9, 0x9, 0x3d, 0x3d, 0x5, 0x2, 0x5, 0x6, 0x9, 0xa, 
    0x3d, 0x3d, 0x4, 0x2, 0xb, 0xb, 0x21, 0x21, 0x3, 0x2, 0xb, 0x14, 0x5, 
    0x2, 0x15, 0x16, 0x1f, 0x1f, 0x40, 0x40, 0x3, 0x2, 0x17, 0x1a, 0x4, 
    0x2, 0x5, 0x6, 0x1b, 0x1d, 0xc, 0x2, 0x1e, 0x1e, 0x23, 0x29, 0x2b, 0x2b, 
    0x2d, 0x2e, 0x30, 0x30, 0x32, 0x38, 0x3a, 0x3c, 0x3e, 0x3e, 0x42, 0x4d, 
    0x4f, 0x53, 0x6, 0x2, 0x3f, 0x3f, 0x55, 0x55, 0x57, 0x59, 0x5c, 0x5d, 
    0x2, 0xf7, 0x2, 0x36, 0x3, 0x2, 0x2, 0x2, 0x4, 0x3a, 0x3, 0x2, 0x2, 
    0x2, 0x6, 0x40, 0x3, 0x2, 0x2, 0x2, 0x8, 0x58, 0x3, 0x2, 0x2, 0x2, 0xa, 
    0x5a, 0x3, 0x2, 0x2, 0x2, 0xc, 0x5c, 0x3, 0x2, 0x2, 0x2, 0xe, 0x71, 
    0x3, 0x2, 0x2, 0x2, 0x10, 0x75, 0x3, 0x2, 0x2, 0x2, 0x12, 0x79, 0x3, 
    0x2, 0x2, 0x2, 0x14, 0x86, 0x3, 0x2, 0x2, 0x2, 0x16, 0x91, 0x3, 0x2, 
    0x2, 0x2, 0x18, 0xc3, 0x3, 0x2, 0x2, 0x2, 0x1a, 0xd2, 0x3, 0x2, 0x2, 
    0x2, 0x1c, 0xd4, 0x3, 0x2, 0x2, 0x2, 0x1e, 0xd6, 0x3, 0x2, 0x2, 0x2, 
    0x20, 0xd8, 0x3, 0x2, 0x2, 0x2, 0x22, 0xda, 0x3, 0x2, 0x2, 0x2, 0x24, 
    0xdc, 0x3, 0x2, 0x2, 0x2, 0x26, 0xde, 0x3, 0x2, 0x2, 0x2, 0x28, 0xe0, 
    0x3, 0x2, 0x2, 0x2, 0x2a, 0xe2, 0x3, 0x2, 0x2, 0x2, 0x2c, 0xe9, 0x3, 
    0x2, 0x2, 0x2, 0x2e, 0xeb, 0x3, 0x2, 0x2, 0x2, 0x30, 0xed, 0x3, 0x2, 
    0x2, 0x2, 0x32, 0xef, 0x3, 0x2, 0x2, 0x2, 0x34, 0x37, 0x5, 0x6, 0x4, 
    0x2, 0x35, 0x37, 0x5, 0x4, 0x3, 0x2, 0x36, 0x34, 0x3, 0x2, 0x2, 0x2, 
    0x36, 0x35, 0x3, 0x2, 0x2, 0x2, 0x37, 0x38, 0x3, 0x2, 0x2, 0x2, 0x38, 
    0x39, 0x7, 0x2, 0x2, 0x3, 0x39, 0x3, 0x3, 0x2, 0x2, 0x2, 0x3a, 0x3b, 
    0x7, 0x61, 0x2, 0x2, 0x3b, 0x3c, 0x8, 0x3, 0x1, 0x2, 0x3c, 0x5, 0x3, 
    0x2, 0x2, 0x2, 0x3d, 0x3f, 0x7, 0x3, 0x2, 0x2, 0x3e, 0x3d, 0x3, 0x2, 
    0x2, 0x2, 0x3f, 0x42, 0x3, 0x2, 0x2, 0x2, 0x40, 0x3e, 0x3, 0x2, 0x2, 
    0x2, 0x40, 0x41, 0x3, 0x2, 0x2, 0x2, 0x41, 0x43, 0x3, 0x2, 0x2, 0x2, 
    0x42, 0x40, 0x3, 0x2, 0x2, 0x2, 0x43, 0x4c, 0x5, 0x8, 0x5, 0x2, 0x44, 
    0x46, 0x7, 0x3, 0x2, 0x2, 0x45, 0x44, 0x3, 0x2, 0x2, 0x2, 0x46, 0x47, 
    0x3, 0x2, 0x2, 0x2, 0x47, 0x45, 0x3, 0x2, 0x2, 0x2, 0x47, 0x48, 0x3, 
    0x2, 0x2, 0x2, 0x48, 0x49, 0x3, 0x2, 0x2, 0x2, 0x49, 0x4b, 0x5, 0x8, 
    0x5, 0x2, 0x4a, 0x45, 0x3, 0x2, 0x2, 0x2, 0x4b, 0x4e, 0x3, 0x2, 0x2, 
    0x2, 0x4c, 0x4a, 0x3, 0x2, 0x2, 0x2, 0x4c, 0x4d, 0x3, 0x2, 0x2, 0x2, 
    0x4d, 0x52, 0x3, 0x2, 0x2, 0x2, 0x4e, 0x4c, 0x3, 0x2, 0x2, 0x2, 0x4f, 
    0x51, 0x7, 0x3, 0x2, 0x2, 0x50, 0x4f, 0x3, 0x2, 0x2, 0x2, 0x51, 0x54, 
    0x3, 0x2, 0x2, 0x2, 0x52, 0x50, 0x3, 0x2, 0x2, 0x2, 0x52, 0x53, 0x3, 
    0x2, 0x2, 0x2, 0x53, 0x7, 0x3, 0x2, 0x2, 0x2, 0x54, 0x52, 0x3, 0x2, 
    0x2, 0x2, 0x55, 0x59, 0x5, 0xa, 0x6, 0x2, 0x56, 0x59, 0x5, 0xc, 0x7, 
    0x2, 0x57, 0x59, 0x5, 0x14, 0xb, 0x2, 0x58, 0x55, 0x3, 0x2, 0x2, 0x2, 
    0x58, 0x56, 0x3, 0x2, 0x2, 0x2, 0x58, 0x57, 0x3, 0x2, 0x2, 0x2, 0x59, 
    0x9, 0x3, 0x2, 0x2, 0x2, 0x5a, 0x5b, 0x9, 0x2, 0x2, 0x2, 0x5b, 0xb, 
    0x3, 0x2, 0x2, 0x2, 0x5c, 0x5f, 0x7, 0x2c, 0x2, 0x2, 0x5d, 0x5e, 0x7, 
    0x4e, 0x2, 0x2, 0x5e, 0x60, 0x5, 0xe, 0x8, 0x2, 0x5f, 0x5d, 0x3, 0x2, 
    0x2, 0x2, 0x5f, 0x60, 0x3, 0x2, 0x2, 0x2, 0x60, 0x61, 0x3, 0x2, 0x2, 
    0x2, 0x61, 0x62, 0x5, 0x2e, 0x18, 0x2, 0x62, 0x63, 0x7, 0x54, 0x2, 0x2, 
    0x63, 0x6e, 0x5, 0x14, 0xb, 0x2, 0x64, 0x65, 0x7, 0x41, 0x2, 0x2, 0x65, 
    0x66, 0x7, 0x22, 0x2, 0x2, 0x66, 0x6b, 0x5, 0x10, 0x9, 0x2, 0x67, 0x68, 
    0x7, 0x4, 0x2, 0x2, 0x68, 0x6a, 0x5, 0x10, 0x9, 0x2, 0x69, 0x67, 0x3, 
    0x2, 0x2, 0x2, 0x6a, 0x6d, 0x3, 0x2, 0x2, 0x2, 0x6b, 0x69, 0x3, 0x2, 
    0x2, 0x2, 0x6b, 0x6c, 0x3, 0x2, 0x2, 0x2, 0x6c, 0x6f, 0x3, 0x2, 0x2, 
    0x2, 0x6d, 0x6b, 0x3, 0x2, 0x2, 0x2, 0x6e, 0x64, 0x3, 0x2, 0x2, 0x2, 
    0x6e, 0x6f, 0x3, 0x2, 0x2, 0x2, 0x6f, 0xd, 0x3, 0x2, 0x2, 0x2, 0x70, 
    0x72, 0x9, 0x3, 0x2, 0x2, 0x71, 0x70, 0x3, 0x2, 0x2, 0x2, 0x71, 0x72, 
    0x3, 0x2, 0x2, 0x2, 0x72, 0x73, 0x3, 0x2, 0x2, 0x2, 0x73, 0x74, 0x7, 
    0x57, 0x2, 0x2, 0x74, 0xf, 0x3, 0x2, 0x2, 0x2, 0x75, 0x77, 0x5, 0x32, 
    0x1a, 0x2, 0x76, 0x78, 0x9, 0x4, 0x2, 0x2, 0x77, 0x76, 0x3, 0x2, 0x2, 
    0x2, 0x77, 0x78, 0x3, 0x2, 0x2, 0x2, 0x78, 0x11, 0x3, 0x2, 0x2, 0x2, 
    0x79, 0x7e, 0x5, 0x14, 0xb, 0x2, 0x7a, 0x7b, 0x7, 0x4, 0x2, 0x2, 0x7b, 
    0x7d, 0x5, 0x14, 0xb, 0x2, 0x7c, 0x7a, 0x3, 0x2, 0x2, 0x2, 0x7d, 0x80, 
    0x3, 0x2, 0x2, 0x2, 0x7e, 0x7c, 0x3, 0x2, 0x2, 0x2, 0x7e, 0x7f, 0x3, 
    0x2, 0x2, 0x2, 0x7f, 0x13, 0x3, 0x2, 0x2, 0x2, 0x80, 0x7e, 0x3, 0x2, 
    0x2, 0x2, 0x81, 0x82, 0x8, 0xb, 0x1, 0x2, 0x82, 0x83, 0x5, 0x1a, 0xe, 
    0x2, 0x83, 0x84, 0x5, 0x14, 0xb, 0x5, 0x84, 0x87, 0x3, 0x2, 0x2, 0x2, 
    0x85, 0x87, 0x5, 0x16, 0xc, 0x2, 0x86, 0x81, 0x3, 0x2, 0x2, 0x2, 0x86, 
    0x85, 0x3, 0x2, 0x2, 0x2, 0x87, 0x8e, 0x3, 0x2, 0x2, 0x2, 0x88, 0x89, 
    0xc, 0x4, 0x2, 0x2, 0x89, 0x8a, 0x5, 0x22, 0x12, 0x2, 0x8a, 0x8b, 0x5, 
    0x14, 0xb, 0x5, 0x8b, 0x8d, 0x3, 0x2, 0x2, 0x2, 0x8c, 0x88, 0x3, 0x2, 
    0x2, 0x2, 0x8d, 0x90, 0x3, 0x2, 0x2, 0x2, 0x8e, 0x8c, 0x3, 0x2, 0x2, 
    0x2, 0x8e, 0x8f, 0x3, 0x2, 0x2, 0x2, 0x8f, 0x15, 0x3, 0x2, 0x2, 0x2, 
    0x90, 0x8e, 0x3, 0x2, 0x2, 0x2, 0x91, 0x92, 0x8, 0xc, 0x1, 0x2, 0x92, 
    0x93, 0x5, 0x18, 0xd, 0x2, 0x93, 0xb5, 0x3, 0x2, 0x2, 0x2, 0x94, 0x95, 
    0xc, 0x5, 0x2, 0x2, 0x95, 0x96, 0x5, 0x20, 0x11, 0x2, 0x96, 0x97, 0x5, 
    0x16, 0xc, 0x6, 0x97, 0xb4, 0x3, 0x2, 0x2, 0x2, 0x98, 0x9a, 0xc, 0x4, 
    0x2, 0x2, 0x99, 0x9b, 0x7, 0x3d, 0x2, 0x2, 0x9a, 0x99, 0x3, 0x2, 0x2, 
    0x2, 0x9a, 0x9b, 0x3, 0x2, 0x2, 0x2, 0x9b, 0x9c, 0x3, 0x2, 0x2, 0x2, 
    0x9c, 0x9e, 0x7, 0x39, 0x2, 0x2, 0x9d, 0x9f, 0x5, 0x1e, 0x10, 0x2, 0x9e, 
    0x9d, 0x3, 0x2, 0x2, 0x2, 0x9e, 0x9f, 0x3, 0x2, 0x2, 0x2, 0x9f, 0xa0, 
    0x3, 0x2, 0x2, 0x2, 0xa0, 0xb4, 0x5, 0x16, 0xc, 0x5, 0xa1, 0xa3, 0xc, 
    0x7, 0x2, 0x2, 0xa2, 0xa4, 0x7, 0x3d, 0x2, 0x2, 0xa3, 0xa2, 0x3, 0x2, 
    0x2, 0x2, 0xa3, 0xa4, 0x3, 0x2, 0x2, 0x2, 0xa4, 0xa5, 0x3, 0x2, 0x2, 
    0x2, 0xa5, 0xa7, 0x7, 0x2f, 0x2, 0x2, 0xa6, 0xa8, 0x5, 0x1e, 0x10, 0x2, 
    0xa7, 0xa6, 0x3, 0x2, 0x2, 0x2, 0xa7, 0xa8, 0x3, 0x2, 0x2, 0x2, 0xa8, 
    0xa9, 0x3, 0x2, 0x2, 0x2, 0xa9, 0xaa, 0x7, 0x7, 0x2, 0x2, 0xaa, 0xab, 
    0x5, 0x12, 0xa, 0x2, 0xab, 0xac, 0x7, 0x8, 0x2, 0x2, 0xac, 0xb4, 0x3, 
    0x2, 0x2, 0x2, 0xad, 0xae, 0xc, 0x6, 0x2, 0x2, 0xae, 0xb0, 0x7, 0x31, 
    0x2, 0x2, 0xaf, 0xb1, 0x7, 0x3d, 0x2, 0x2, 0xb0, 0xaf, 0x3, 0x2, 0x2, 
    0x2, 0xb0, 0xb1, 0x3, 0x2, 0x2, 0x2, 0xb1, 0xb2, 0x3, 0x2, 0x2, 0x2, 
    0xb2, 0xb4, 0x7, 0x3f, 0x2, 0x2, 0xb3, 0x94, 0x3, 0x2, 0x2, 0x2, 0xb3, 
    0x98, 0x3, 0x2, 0x2, 0x2, 0xb3, 0xa1, 0x3, 0x2, 0x2, 0x2, 0xb3, 0xad, 
    0x3, 0x2, 0x2, 0x2, 0xb4, 0xb7, 0x3, 0x2, 0x2, 0x2, 0xb5, 0xb3, 0x3, 
    0x2, 0x2, 0x2, 0xb5, 0xb6, 0x3, 0x2, 0x2, 0x2, 0xb6, 0x17, 0x3, 0x2, 
    0x2, 0x2, 0xb7, 0xb5, 0x3, 0x2, 0x2, 0x2, 0xb8, 0xb9, 0x8, 0xd, 0x1, 
    0x2, 0xb9, 0xc4, 0x5, 0x2c, 0x17, 0x2, 0xba, 0xc4, 0x5, 0x30, 0x19, 
    0x2, 0xbb, 0xc4, 0x5, 0x2a, 0x16, 0x2, 0xbc, 0xbd, 0x5, 0x1c, 0xf, 0x2, 
    0xbd, 0xbe, 0x5, 0x18, 0xd, 0x6, 0xbe, 0xc4, 0x3, 0x2, 0x2, 0x2, 0xbf, 
    0xc0, 0x7, 0x7, 0x2, 0x2, 0xc0, 0xc1, 0x5, 0x14, 0xb, 0x2, 0xc1, 0xc2, 
    0x7, 0x8, 0x2, 0x2, 0xc2, 0xc4, 0x3, 0x2, 0x2, 0x2, 0xc3, 0xb8, 0x3, 
    0x2, 0x2, 0x2, 0xc3, 0xba, 0x3, 0x2, 0x2, 0x2, 0xc3, 0xbb, 0x3, 0x2, 
    0x2, 0x2, 0xc3, 0xbc, 0x3, 0x2, 0x2, 0x2, 0xc3, 0xbf, 0x3, 0x2, 0x2, 
    0x2, 0xc4, 0xcf, 0x3, 0x2, 0x2, 0x2, 0xc5, 0xc6, 0xc, 0x4, 0x2, 0x2, 
    0xc6, 0xc7, 0x5, 0x26, 0x14, 0x2, 0xc7, 0xc8, 0x5, 0x18, 0xd, 0x5, 0xc8, 
    0xce, 0x3, 0x2, 0x2, 0x2, 0xc9, 0xca, 0xc, 0x3, 0x2, 0x2, 0xca, 0xcb, 
    0x5, 0x24, 0x13, 0x2, 0xcb, 0xcc, 0x5, 0x18, 0xd, 0x4, 0xcc, 0xce, 0x3, 
    0x2, 0x2, 0x2, 0xcd, 0xc5, 0x3, 0x2, 0x2, 0x2, 0xcd, 0xc9, 0x3, 0x2, 
    0x2, 0x2, 0xce, 0xd1, 0x3, 0x2, 0x2, 0x2, 0xcf, 0xcd, 0x3, 0x2, 0x2, 
    0x2, 0xcf, 0xd0, 0x3, 0x2, 0x2, 0x2, 0xd0, 0x19, 0x3, 0x2, 0x2, 0x2, 
    0xd1, 0xcf, 0x3, 0x2, 0x2, 0x2, 0xd2, 0xd3, 0x9, 0x5, 0x2, 0x2, 0xd3, 
    0x1b, 0x3, 0x2, 0x2, 0x2, 0xd4, 0xd5, 0x9, 0x6, 0x2, 0x2, 0xd5, 0x1d, 
    0x3, 0x2, 0x2, 0x2, 0xd6, 0xd7, 0x9, 0x7, 0x2, 0x2, 0xd7, 0x1f, 0x3, 
    0x2, 0x2, 0x2, 0xd8, 0xd9, 0x9, 0x8, 0x2, 0x2, 0xd9, 0x21, 0x3, 0x2, 
    0x2, 0x2, 0xda, 0xdb, 0x9, 0x9, 0x2, 0x2, 0xdb, 0x23, 0x3, 0x2, 0x2, 
    0x2, 0xdc, 0xdd, 0x9, 0xa, 0x2, 0x2, 0xdd, 0x25, 0x3, 0x2, 0x2, 0x2, 
    0xde, 0xdf, 0x9, 0xb, 0x2, 0x2, 0xdf, 0x27, 0x3, 0x2, 0x2, 0x2, 0xe0, 
    0xe1, 0x9, 0xc, 0x2, 0x2, 0xe1, 0x29, 0x3, 0x2, 0x2, 0x2, 0xe2, 0xe3, 
    0x5, 0x28, 0x15, 0x2, 0xe3, 0xe5, 0x7, 0x7, 0x2, 0x2, 0xe4, 0xe6, 0x5, 
    0x12, 0xa, 0x2, 0xe5, 0xe4, 0x3, 0x2, 0x2, 0x2, 0xe5, 0xe6, 0x3, 0x2, 
    0x2, 0x2, 0xe6, 0xe7, 0x3, 0x2, 0x2, 0x2, 0xe7, 0xe8, 0x7, 0x8, 0x2, 
    0x2, 0xe8, 0x2b, 0x3, 0x2, 0x2, 0x2, 0xe9, 0xea, 0x9, 0xd, 0x2, 0x2, 
    0xea, 0x2d, 0x3, 0x2, 0x2, 0x2, 0xeb, 0xec, 0x7, 0x56, 0x2, 0x2, 0xec, 
    0x2f, 0x3, 0x2, 0x2, 0x2, 0xed, 0xee, 0x7, 0x56, 0x2, 0x2, 0xee, 0x31, 
    0x3, 0x2, 0x2, 0x2, 0xef, 0xf0, 0x7, 0x56, 0x2, 0x2, 0xf0, 0x33, 0x3, 
    0x2, 0x2, 0x2, 0x1b, 0x36, 0x40, 0x47, 0x4c, 0x52, 0x58, 0x5f, 0x6b, 
    0x6e, 0x71, 0x77, 0x7e, 0x86, 0x8e, 0x9a, 0x9e, 0xa3, 0xa7, 0xb0, 0xb3, 
    0xb5, 0xc3, 0xcd, 0xcf, 0xe5, 
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
