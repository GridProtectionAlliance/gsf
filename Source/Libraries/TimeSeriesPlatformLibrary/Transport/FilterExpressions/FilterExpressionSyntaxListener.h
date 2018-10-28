
// Generated from FilterExpressionSyntax.g4 by ANTLR 4.7.1

#pragma once


#include "antlr4-runtime.h"
#include "FilterExpressionSyntaxParser.h"


/**
 * This interface defines an abstract listener for a parse tree produced by FilterExpressionSyntaxParser.
 */
class  FilterExpressionSyntaxListener : public antlr4::tree::ParseTreeListener {
public:

  virtual void enterParse(FilterExpressionSyntaxParser::ParseContext *ctx) = 0;
  virtual void exitParse(FilterExpressionSyntaxParser::ParseContext *ctx) = 0;

  virtual void enterError(FilterExpressionSyntaxParser::ErrorContext *ctx) = 0;
  virtual void exitError(FilterExpressionSyntaxParser::ErrorContext *ctx) = 0;

  virtual void enterFilterExpressionStatementList(FilterExpressionSyntaxParser::FilterExpressionStatementListContext *ctx) = 0;
  virtual void exitFilterExpressionStatementList(FilterExpressionSyntaxParser::FilterExpressionStatementListContext *ctx) = 0;

  virtual void enterFilterExpressionStatement(FilterExpressionSyntaxParser::FilterExpressionStatementContext *ctx) = 0;
  virtual void exitFilterExpressionStatement(FilterExpressionSyntaxParser::FilterExpressionStatementContext *ctx) = 0;

  virtual void enterIdentifierStatement(FilterExpressionSyntaxParser::IdentifierStatementContext *ctx) = 0;
  virtual void exitIdentifierStatement(FilterExpressionSyntaxParser::IdentifierStatementContext *ctx) = 0;

  virtual void enterFilterStatement(FilterExpressionSyntaxParser::FilterStatementContext *ctx) = 0;
  virtual void exitFilterStatement(FilterExpressionSyntaxParser::FilterStatementContext *ctx) = 0;

  virtual void enterOrderingTerm(FilterExpressionSyntaxParser::OrderingTermContext *ctx) = 0;
  virtual void exitOrderingTerm(FilterExpressionSyntaxParser::OrderingTermContext *ctx) = 0;

  virtual void enterExpression(FilterExpressionSyntaxParser::ExpressionContext *ctx) = 0;
  virtual void exitExpression(FilterExpressionSyntaxParser::ExpressionContext *ctx) = 0;

  virtual void enterLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext *ctx) = 0;
  virtual void exitLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext *ctx) = 0;

  virtual void enterUnaryOperator(FilterExpressionSyntaxParser::UnaryOperatorContext *ctx) = 0;
  virtual void exitUnaryOperator(FilterExpressionSyntaxParser::UnaryOperatorContext *ctx) = 0;

  virtual void enterKeyword(FilterExpressionSyntaxParser::KeywordContext *ctx) = 0;
  virtual void exitKeyword(FilterExpressionSyntaxParser::KeywordContext *ctx) = 0;

  virtual void enterFunctionName(FilterExpressionSyntaxParser::FunctionNameContext *ctx) = 0;
  virtual void exitFunctionName(FilterExpressionSyntaxParser::FunctionNameContext *ctx) = 0;

  virtual void enterDatabaseName(FilterExpressionSyntaxParser::DatabaseNameContext *ctx) = 0;
  virtual void exitDatabaseName(FilterExpressionSyntaxParser::DatabaseNameContext *ctx) = 0;

  virtual void enterTableName(FilterExpressionSyntaxParser::TableNameContext *ctx) = 0;
  virtual void exitTableName(FilterExpressionSyntaxParser::TableNameContext *ctx) = 0;

  virtual void enterColumnName(FilterExpressionSyntaxParser::ColumnNameContext *ctx) = 0;
  virtual void exitColumnName(FilterExpressionSyntaxParser::ColumnNameContext *ctx) = 0;


};

