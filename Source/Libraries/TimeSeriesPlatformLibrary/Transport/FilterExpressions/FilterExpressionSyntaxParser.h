
// Generated from FilterExpressionSyntax.g4 by ANTLR 4.7.1

#pragma once


#include "antlr4-runtime.h"




class  FilterExpressionSyntaxParser : public antlr4::Parser {
public:
  enum {
    T__0 = 1, T__1 = 2, T__2 = 3, T__3 = 4, T__4 = 5, T__5 = 6, T__6 = 7, 
    T__7 = 8, T__8 = 9, T__9 = 10, T__10 = 11, T__11 = 12, T__12 = 13, T__13 = 14, 
    T__14 = 15, T__15 = 16, T__16 = 17, T__17 = 18, T__18 = 19, T__19 = 20, 
    T__20 = 21, T__21 = 22, K_AND = 23, K_ASC = 24, K_BY = 25, K_CONVERT = 26, 
    K_COALESCE = 27, K_DESC = 28, K_FALSE = 29, K_FILTER = 30, K_IIF = 31, 
    K_IN = 32, K_IS = 33, K_ISNULL = 34, K_ISREGEXMATCH = 35, K_LEN = 36, 
    K_LIKE = 37, K_NOT = 38, K_NULL = 39, K_OR = 40, K_ORDER = 41, K_REGEXVAL = 42, 
    K_SUBSTR = 43, K_SUBSTRING = 44, K_TOP = 45, K_TRIM = 46, K_TRUE = 47, 
    K_WHERE = 48, IDENTIFIER = 49, INTEGER_LITERAL = 50, NUMERIC_LITERAL = 51, 
    GUID_LITERAL = 52, STRING_LITERAL = 53, DATETIME_LITERAL = 54, BOOLEAN_LITERAL = 55, 
    MEASUREMENT_KEY_LITERAL = 56, POINT_TAG_LITERAL = 57, SINGLE_LINE_COMMENT = 58, 
    MULTILINE_COMMENT = 59, SPACES = 60, UNEXPECTED_CHAR = 61
  };

  enum {
    RuleParse = 0, RuleError = 1, RuleFilterExpressionStatementList = 2, 
    RuleFilterExpressionStatement = 3, RuleIdentifierStatement = 4, RuleFilterStatement = 5, 
    RuleOrderingTerm = 6, RuleExpression = 7, RuleLiteralValue = 8, RuleUnaryOperator = 9, 
    RuleKeyword = 10, RuleFunctionName = 11, RuleTableName = 12, RuleColumnName = 13
  };

  FilterExpressionSyntaxParser(antlr4::TokenStream *input);
  ~FilterExpressionSyntaxParser();

  virtual std::string getGrammarFileName() const override;
  virtual const antlr4::atn::ATN& getATN() const override { return _atn; };
  virtual const std::vector<std::string>& getTokenNames() const override { return _tokenNames; }; // deprecated: use vocabulary instead.
  virtual const std::vector<std::string>& getRuleNames() const override;
  virtual antlr4::dfa::Vocabulary& getVocabulary() const override;


  class ParseContext;
  class ErrorContext;
  class FilterExpressionStatementListContext;
  class FilterExpressionStatementContext;
  class IdentifierStatementContext;
  class FilterStatementContext;
  class OrderingTermContext;
  class ExpressionContext;
  class LiteralValueContext;
  class UnaryOperatorContext;
  class KeywordContext;
  class FunctionNameContext;
  class TableNameContext;
  class ColumnNameContext; 

  class  ParseContext : public antlr4::ParserRuleContext {
  public:
    ParseContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *EOF();
    FilterExpressionStatementListContext *filterExpressionStatementList();
    ErrorContext *error();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  ParseContext* parse();

  class  ErrorContext : public antlr4::ParserRuleContext {
  public:
    antlr4::Token *unexpected_charToken = nullptr;;
    ErrorContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *UNEXPECTED_CHAR();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  ErrorContext* error();

  class  FilterExpressionStatementListContext : public antlr4::ParserRuleContext {
  public:
    FilterExpressionStatementListContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    std::vector<FilterExpressionStatementContext *> filterExpressionStatement();
    FilterExpressionStatementContext* filterExpressionStatement(size_t i);

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  FilterExpressionStatementListContext* filterExpressionStatementList();

  class  FilterExpressionStatementContext : public antlr4::ParserRuleContext {
  public:
    FilterExpressionStatementContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    FilterStatementContext *filterStatement();
    IdentifierStatementContext *identifierStatement();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  FilterExpressionStatementContext* filterExpressionStatement();

  class  IdentifierStatementContext : public antlr4::ParserRuleContext {
  public:
    IdentifierStatementContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *GUID_LITERAL();
    antlr4::tree::TerminalNode *MEASUREMENT_KEY_LITERAL();
    antlr4::tree::TerminalNode *POINT_TAG_LITERAL();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  IdentifierStatementContext* identifierStatement();

  class  FilterStatementContext : public antlr4::ParserRuleContext {
  public:
    FilterStatementContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *K_FILTER();
    TableNameContext *tableName();
    antlr4::tree::TerminalNode *K_WHERE();
    ExpressionContext *expression();
    antlr4::tree::TerminalNode *K_TOP();
    antlr4::tree::TerminalNode *INTEGER_LITERAL();
    antlr4::tree::TerminalNode *K_ORDER();
    antlr4::tree::TerminalNode *K_BY();
    std::vector<OrderingTermContext *> orderingTerm();
    OrderingTermContext* orderingTerm(size_t i);

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  FilterStatementContext* filterStatement();

  class  OrderingTermContext : public antlr4::ParserRuleContext {
  public:
    OrderingTermContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    ColumnNameContext *columnName();
    antlr4::tree::TerminalNode *K_ASC();
    antlr4::tree::TerminalNode *K_DESC();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  OrderingTermContext* orderingTerm();

  class  ExpressionContext : public antlr4::ParserRuleContext {
  public:
    ExpressionContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    LiteralValueContext *literalValue();
    ColumnNameContext *columnName();
    UnaryOperatorContext *unaryOperator();
    std::vector<ExpressionContext *> expression();
    ExpressionContext* expression(size_t i);
    FunctionNameContext *functionName();
    antlr4::tree::TerminalNode *K_LIKE();
    antlr4::tree::TerminalNode *K_NOT();
    antlr4::tree::TerminalNode *K_AND();
    antlr4::tree::TerminalNode *K_OR();
    antlr4::tree::TerminalNode *K_IS();
    antlr4::tree::TerminalNode *K_NULL();
    antlr4::tree::TerminalNode *K_IN();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  ExpressionContext* expression();
  ExpressionContext* expression(int precedence);
  class  LiteralValueContext : public antlr4::ParserRuleContext {
  public:
    LiteralValueContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *INTEGER_LITERAL();
    antlr4::tree::TerminalNode *NUMERIC_LITERAL();
    antlr4::tree::TerminalNode *STRING_LITERAL();
    antlr4::tree::TerminalNode *DATETIME_LITERAL();
    antlr4::tree::TerminalNode *GUID_LITERAL();
    antlr4::tree::TerminalNode *BOOLEAN_LITERAL();
    antlr4::tree::TerminalNode *K_NULL();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  LiteralValueContext* literalValue();

  class  UnaryOperatorContext : public antlr4::ParserRuleContext {
  public:
    UnaryOperatorContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *K_NOT();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  UnaryOperatorContext* unaryOperator();

  class  KeywordContext : public antlr4::ParserRuleContext {
  public:
    KeywordContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *K_AND();
    antlr4::tree::TerminalNode *K_ASC();
    antlr4::tree::TerminalNode *K_BY();
    antlr4::tree::TerminalNode *K_COALESCE();
    antlr4::tree::TerminalNode *K_CONVERT();
    antlr4::tree::TerminalNode *K_DESC();
    antlr4::tree::TerminalNode *K_FALSE();
    antlr4::tree::TerminalNode *K_FILTER();
    antlr4::tree::TerminalNode *K_IIF();
    antlr4::tree::TerminalNode *K_IN();
    antlr4::tree::TerminalNode *K_IS();
    antlr4::tree::TerminalNode *K_ISNULL();
    antlr4::tree::TerminalNode *K_ISREGEXMATCH();
    antlr4::tree::TerminalNode *K_LEN();
    antlr4::tree::TerminalNode *K_LIKE();
    antlr4::tree::TerminalNode *K_NOT();
    antlr4::tree::TerminalNode *K_NULL();
    antlr4::tree::TerminalNode *K_OR();
    antlr4::tree::TerminalNode *K_ORDER();
    antlr4::tree::TerminalNode *K_REGEXVAL();
    antlr4::tree::TerminalNode *K_SUBSTR();
    antlr4::tree::TerminalNode *K_SUBSTRING();
    antlr4::tree::TerminalNode *K_TOP();
    antlr4::tree::TerminalNode *K_TRIM();
    antlr4::tree::TerminalNode *K_TRUE();
    antlr4::tree::TerminalNode *K_WHERE();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  KeywordContext* keyword();

  class  FunctionNameContext : public antlr4::ParserRuleContext {
  public:
    FunctionNameContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *K_COALESCE();
    antlr4::tree::TerminalNode *K_CONVERT();
    antlr4::tree::TerminalNode *K_IIF();
    antlr4::tree::TerminalNode *K_ISNULL();
    antlr4::tree::TerminalNode *K_ISREGEXMATCH();
    antlr4::tree::TerminalNode *K_LEN();
    antlr4::tree::TerminalNode *K_REGEXVAL();
    antlr4::tree::TerminalNode *K_SUBSTR();
    antlr4::tree::TerminalNode *K_SUBSTRING();
    antlr4::tree::TerminalNode *K_TRIM();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  FunctionNameContext* functionName();

  class  TableNameContext : public antlr4::ParserRuleContext {
  public:
    TableNameContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *IDENTIFIER();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  TableNameContext* tableName();

  class  ColumnNameContext : public antlr4::ParserRuleContext {
  public:
    ColumnNameContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *IDENTIFIER();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  ColumnNameContext* columnName();


  virtual bool sempred(antlr4::RuleContext *_localctx, size_t ruleIndex, size_t predicateIndex) override;
  bool expressionSempred(ExpressionContext *_localctx, size_t predicateIndex);

private:
  static std::vector<antlr4::dfa::DFA> _decisionToDFA;
  static antlr4::atn::PredictionContextCache _sharedContextCache;
  static std::vector<std::string> _ruleNames;
  static std::vector<std::string> _tokenNames;

  static std::vector<std::string> _literalNames;
  static std::vector<std::string> _symbolicNames;
  static antlr4::dfa::Vocabulary _vocabulary;
  static antlr4::atn::ATN _atn;
  static std::vector<uint16_t> _serializedATN;


  struct Initializer {
    Initializer();
  };
  static Initializer _init;
};

