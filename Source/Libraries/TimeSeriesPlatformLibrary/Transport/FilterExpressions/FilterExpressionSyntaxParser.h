
// Generated from FilterExpressionSyntax.g4 by ANTLR 4.7.1

#pragma once


#include "antlr4-runtime.h"




class  FilterExpressionSyntaxParser : public antlr4::Parser {
public:
  enum {
    T__0 = 1, T__1 = 2, T__2 = 3, T__3 = 4, T__4 = 5, T__5 = 6, T__6 = 7, 
    T__7 = 8, T__8 = 9, T__9 = 10, T__10 = 11, T__11 = 12, T__12 = 13, T__13 = 14, 
    T__14 = 15, T__15 = 16, T__16 = 17, T__17 = 18, T__18 = 19, T__19 = 20, 
    T__20 = 21, T__21 = 22, T__22 = 23, T__23 = 24, T__24 = 25, T__25 = 26, 
    T__26 = 27, K_ABS = 28, K_AND = 29, K_ASC = 30, K_BINARY = 31, K_BY = 32, 
    K_CEILING = 33, K_COALESCE = 34, K_CONVERT = 35, K_CONTAINS = 36, K_DATEADD = 37, 
    K_DATEDIFF = 38, K_DATEPART = 39, K_DESC = 40, K_ENDSWITH = 41, K_FILTER = 42, 
    K_FLOOR = 43, K_IIF = 44, K_IN = 45, K_INDEXOF = 46, K_IS = 47, K_ISDATE = 48, 
    K_ISINTEGER = 49, K_ISGUID = 50, K_ISNULL = 51, K_ISNUMERIC = 52, K_LASTINDEXOF = 53, 
    K_LEN = 54, K_LIKE = 55, K_LOWER = 56, K_MAXOF = 57, K_MINOF = 58, K_NOT = 59, 
    K_NOW = 60, K_NTHINDEXOF = 61, K_NULL = 62, K_OR = 63, K_ORDER = 64, 
    K_POWER = 65, K_REGEXMATCH = 66, K_REGEXVAL = 67, K_REPLACE = 68, K_REVERSE = 69, 
    K_ROUND = 70, K_SQRT = 71, K_SPLIT = 72, K_STARTSWITH = 73, K_STRCOUNT = 74, 
    K_STRCMP = 75, K_SUBSTR = 76, K_TOP = 77, K_TRIM = 78, K_TRIMLEFT = 79, 
    K_TRIMRIGHT = 80, K_UPPER = 81, K_UTCNOW = 82, K_WHERE = 83, BOOLEAN_LITERAL = 84, 
    IDENTIFIER = 85, INTEGER_LITERAL = 86, NUMERIC_LITERAL = 87, GUID_LITERAL = 88, 
    MEASUREMENT_KEY_LITERAL = 89, POINT_TAG_LITERAL = 90, STRING_LITERAL = 91, 
    DATETIME_LITERAL = 92, SINGLE_LINE_COMMENT = 93, MULTILINE_COMMENT = 94, 
    SPACES = 95, UNEXPECTED_CHAR = 96
  };

  enum {
    RuleParse = 0, RuleError = 1, RuleFilterExpressionStatementList = 2, 
    RuleFilterExpressionStatement = 3, RuleIdentifierStatement = 4, RuleFilterStatement = 5, 
    RuleTopLimit = 6, RuleOrderingTerm = 7, RuleExpressionList = 8, RuleExpression = 9, 
    RulePredicateExpression = 10, RuleValueExpression = 11, RuleNotOperator = 12, 
    RuleUnaryOperator = 13, RuleExactMatchModifier = 14, RuleComparisonOperator = 15, 
    RuleLogicalOperator = 16, RuleBitwiseOperator = 17, RuleMathOperator = 18, 
    RuleFunctionName = 19, RuleFunctionExpression = 20, RuleLiteralValue = 21, 
    RuleTableName = 22, RuleColumnName = 23, RuleOrderByColumnName = 24
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
  class TopLimitContext;
  class OrderingTermContext;
  class ExpressionListContext;
  class ExpressionContext;
  class PredicateExpressionContext;
  class ValueExpressionContext;
  class NotOperatorContext;
  class UnaryOperatorContext;
  class ExactMatchModifierContext;
  class ComparisonOperatorContext;
  class LogicalOperatorContext;
  class BitwiseOperatorContext;
  class MathOperatorContext;
  class FunctionNameContext;
  class FunctionExpressionContext;
  class LiteralValueContext;
  class TableNameContext;
  class ColumnNameContext;
  class OrderByColumnNameContext; 

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
    IdentifierStatementContext *identifierStatement();
    FilterStatementContext *filterStatement();
    ExpressionContext *expression();

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
    TopLimitContext *topLimit();
    antlr4::tree::TerminalNode *K_ORDER();
    antlr4::tree::TerminalNode *K_BY();
    std::vector<OrderingTermContext *> orderingTerm();
    OrderingTermContext* orderingTerm(size_t i);

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  FilterStatementContext* filterStatement();

  class  TopLimitContext : public antlr4::ParserRuleContext {
  public:
    TopLimitContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *INTEGER_LITERAL();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  TopLimitContext* topLimit();

  class  OrderingTermContext : public antlr4::ParserRuleContext {
  public:
    OrderingTermContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    OrderByColumnNameContext *orderByColumnName();
    antlr4::tree::TerminalNode *K_ASC();
    antlr4::tree::TerminalNode *K_DESC();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  OrderingTermContext* orderingTerm();

  class  ExpressionListContext : public antlr4::ParserRuleContext {
  public:
    ExpressionListContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    std::vector<ExpressionContext *> expression();
    ExpressionContext* expression(size_t i);

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  ExpressionListContext* expressionList();

  class  ExpressionContext : public antlr4::ParserRuleContext {
  public:
    ExpressionContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    NotOperatorContext *notOperator();
    std::vector<ExpressionContext *> expression();
    ExpressionContext* expression(size_t i);
    PredicateExpressionContext *predicateExpression();
    LogicalOperatorContext *logicalOperator();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  ExpressionContext* expression();
  ExpressionContext* expression(int precedence);
  class  PredicateExpressionContext : public antlr4::ParserRuleContext {
  public:
    PredicateExpressionContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    ValueExpressionContext *valueExpression();
    std::vector<PredicateExpressionContext *> predicateExpression();
    PredicateExpressionContext* predicateExpression(size_t i);
    ComparisonOperatorContext *comparisonOperator();
    antlr4::tree::TerminalNode *K_LIKE();
    NotOperatorContext *notOperator();
    ExactMatchModifierContext *exactMatchModifier();
    antlr4::tree::TerminalNode *K_IN();
    ExpressionListContext *expressionList();
    antlr4::tree::TerminalNode *K_IS();
    antlr4::tree::TerminalNode *K_NULL();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  PredicateExpressionContext* predicateExpression();
  PredicateExpressionContext* predicateExpression(int precedence);
  class  ValueExpressionContext : public antlr4::ParserRuleContext {
  public:
    ValueExpressionContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    LiteralValueContext *literalValue();
    ColumnNameContext *columnName();
    FunctionExpressionContext *functionExpression();
    UnaryOperatorContext *unaryOperator();
    std::vector<ValueExpressionContext *> valueExpression();
    ValueExpressionContext* valueExpression(size_t i);
    ExpressionContext *expression();
    MathOperatorContext *mathOperator();
    BitwiseOperatorContext *bitwiseOperator();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  ValueExpressionContext* valueExpression();
  ValueExpressionContext* valueExpression(int precedence);
  class  NotOperatorContext : public antlr4::ParserRuleContext {
  public:
    NotOperatorContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *K_NOT();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  NotOperatorContext* notOperator();

  class  UnaryOperatorContext : public antlr4::ParserRuleContext {
  public:
    UnaryOperatorContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *K_NOT();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  UnaryOperatorContext* unaryOperator();

  class  ExactMatchModifierContext : public antlr4::ParserRuleContext {
  public:
    ExactMatchModifierContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *K_BINARY();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  ExactMatchModifierContext* exactMatchModifier();

  class  ComparisonOperatorContext : public antlr4::ParserRuleContext {
  public:
    ComparisonOperatorContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  ComparisonOperatorContext* comparisonOperator();

  class  LogicalOperatorContext : public antlr4::ParserRuleContext {
  public:
    LogicalOperatorContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *K_AND();
    antlr4::tree::TerminalNode *K_OR();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  LogicalOperatorContext* logicalOperator();

  class  BitwiseOperatorContext : public antlr4::ParserRuleContext {
  public:
    BitwiseOperatorContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  BitwiseOperatorContext* bitwiseOperator();

  class  MathOperatorContext : public antlr4::ParserRuleContext {
  public:
    MathOperatorContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  MathOperatorContext* mathOperator();

  class  FunctionNameContext : public antlr4::ParserRuleContext {
  public:
    FunctionNameContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *K_ABS();
    antlr4::tree::TerminalNode *K_CEILING();
    antlr4::tree::TerminalNode *K_COALESCE();
    antlr4::tree::TerminalNode *K_CONVERT();
    antlr4::tree::TerminalNode *K_CONTAINS();
    antlr4::tree::TerminalNode *K_DATEADD();
    antlr4::tree::TerminalNode *K_DATEDIFF();
    antlr4::tree::TerminalNode *K_DATEPART();
    antlr4::tree::TerminalNode *K_ENDSWITH();
    antlr4::tree::TerminalNode *K_FLOOR();
    antlr4::tree::TerminalNode *K_IIF();
    antlr4::tree::TerminalNode *K_INDEXOF();
    antlr4::tree::TerminalNode *K_ISDATE();
    antlr4::tree::TerminalNode *K_ISINTEGER();
    antlr4::tree::TerminalNode *K_ISGUID();
    antlr4::tree::TerminalNode *K_ISNULL();
    antlr4::tree::TerminalNode *K_ISNUMERIC();
    antlr4::tree::TerminalNode *K_LASTINDEXOF();
    antlr4::tree::TerminalNode *K_LEN();
    antlr4::tree::TerminalNode *K_LOWER();
    antlr4::tree::TerminalNode *K_MAXOF();
    antlr4::tree::TerminalNode *K_MINOF();
    antlr4::tree::TerminalNode *K_NOW();
    antlr4::tree::TerminalNode *K_NTHINDEXOF();
    antlr4::tree::TerminalNode *K_POWER();
    antlr4::tree::TerminalNode *K_REGEXMATCH();
    antlr4::tree::TerminalNode *K_REGEXVAL();
    antlr4::tree::TerminalNode *K_REPLACE();
    antlr4::tree::TerminalNode *K_REVERSE();
    antlr4::tree::TerminalNode *K_ROUND();
    antlr4::tree::TerminalNode *K_SPLIT();
    antlr4::tree::TerminalNode *K_SQRT();
    antlr4::tree::TerminalNode *K_STARTSWITH();
    antlr4::tree::TerminalNode *K_STRCOUNT();
    antlr4::tree::TerminalNode *K_STRCMP();
    antlr4::tree::TerminalNode *K_SUBSTR();
    antlr4::tree::TerminalNode *K_TRIM();
    antlr4::tree::TerminalNode *K_TRIMLEFT();
    antlr4::tree::TerminalNode *K_TRIMRIGHT();
    antlr4::tree::TerminalNode *K_UPPER();
    antlr4::tree::TerminalNode *K_UTCNOW();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  FunctionNameContext* functionName();

  class  FunctionExpressionContext : public antlr4::ParserRuleContext {
  public:
    FunctionExpressionContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    FunctionNameContext *functionName();
    ExpressionListContext *expressionList();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  FunctionExpressionContext* functionExpression();

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

  class  OrderByColumnNameContext : public antlr4::ParserRuleContext {
  public:
    OrderByColumnNameContext(antlr4::ParserRuleContext *parent, size_t invokingState);
    virtual size_t getRuleIndex() const override;
    antlr4::tree::TerminalNode *IDENTIFIER();

    virtual void enterRule(antlr4::tree::ParseTreeListener *listener) override;
    virtual void exitRule(antlr4::tree::ParseTreeListener *listener) override;
   
  };

  OrderByColumnNameContext* orderByColumnName();


  virtual bool sempred(antlr4::RuleContext *_localctx, size_t ruleIndex, size_t predicateIndex) override;
  bool expressionSempred(ExpressionContext *_localctx, size_t predicateIndex);
  bool predicateExpressionSempred(PredicateExpressionContext *_localctx, size_t predicateIndex);
  bool valueExpressionSempred(ValueExpressionContext *_localctx, size_t predicateIndex);

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

