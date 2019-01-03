
// Generated from FilterExpressionSyntax.g4 by ANTLR 4.7.1

#pragma once


#include "antlr4-runtime.h"
#include "FilterExpressionSyntaxListener.h"


/**
 * This class provides an empty implementation of FilterExpressionSyntaxListener,
 * which can be extended to create a listener which only needs to handle a subset
 * of the available methods.
 */
class  FilterExpressionSyntaxBaseListener : public FilterExpressionSyntaxListener {
public:

  virtual void enterParse(FilterExpressionSyntaxParser::ParseContext * /*ctx*/) override { }
  virtual void exitParse(FilterExpressionSyntaxParser::ParseContext * /*ctx*/) override { }

  virtual void enterError(FilterExpressionSyntaxParser::ErrorContext * /*ctx*/) override { }
  virtual void exitError(FilterExpressionSyntaxParser::ErrorContext * /*ctx*/) override { }

  virtual void enterFilterExpressionStatementList(FilterExpressionSyntaxParser::FilterExpressionStatementListContext * /*ctx*/) override { }
  virtual void exitFilterExpressionStatementList(FilterExpressionSyntaxParser::FilterExpressionStatementListContext * /*ctx*/) override { }

  virtual void enterFilterExpressionStatement(FilterExpressionSyntaxParser::FilterExpressionStatementContext * /*ctx*/) override { }
  virtual void exitFilterExpressionStatement(FilterExpressionSyntaxParser::FilterExpressionStatementContext * /*ctx*/) override { }

  virtual void enterIdentifierStatement(FilterExpressionSyntaxParser::IdentifierStatementContext * /*ctx*/) override { }
  virtual void exitIdentifierStatement(FilterExpressionSyntaxParser::IdentifierStatementContext * /*ctx*/) override { }

  virtual void enterFilterStatement(FilterExpressionSyntaxParser::FilterStatementContext * /*ctx*/) override { }
  virtual void exitFilterStatement(FilterExpressionSyntaxParser::FilterStatementContext * /*ctx*/) override { }

  virtual void enterOrderingTerm(FilterExpressionSyntaxParser::OrderingTermContext * /*ctx*/) override { }
  virtual void exitOrderingTerm(FilterExpressionSyntaxParser::OrderingTermContext * /*ctx*/) override { }

  virtual void enterExpression(FilterExpressionSyntaxParser::ExpressionContext * /*ctx*/) override { }
  virtual void exitExpression(FilterExpressionSyntaxParser::ExpressionContext * /*ctx*/) override { }

  virtual void enterLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext * /*ctx*/) override { }
  virtual void exitLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext * /*ctx*/) override { }

  virtual void enterUnaryOperator(FilterExpressionSyntaxParser::UnaryOperatorContext * /*ctx*/) override { }
  virtual void exitUnaryOperator(FilterExpressionSyntaxParser::UnaryOperatorContext * /*ctx*/) override { }

  virtual void enterFunctionName(FilterExpressionSyntaxParser::FunctionNameContext * /*ctx*/) override { }
  virtual void exitFunctionName(FilterExpressionSyntaxParser::FunctionNameContext * /*ctx*/) override { }

  virtual void enterTableName(FilterExpressionSyntaxParser::TableNameContext * /*ctx*/) override { }
  virtual void exitTableName(FilterExpressionSyntaxParser::TableNameContext * /*ctx*/) override { }

  virtual void enterColumnName(FilterExpressionSyntaxParser::ColumnNameContext * /*ctx*/) override { }
  virtual void exitColumnName(FilterExpressionSyntaxParser::ColumnNameContext * /*ctx*/) override { }

  virtual void enterOrderByColumnName(FilterExpressionSyntaxParser::OrderByColumnNameContext * /*ctx*/) override { }
  virtual void exitOrderByColumnName(FilterExpressionSyntaxParser::OrderByColumnNameContext * /*ctx*/) override { }


  virtual void enterEveryRule(antlr4::ParserRuleContext * /*ctx*/) override { }
  virtual void exitEveryRule(antlr4::ParserRuleContext * /*ctx*/) override { }
  virtual void visitTerminal(antlr4::tree::TerminalNode * /*node*/) override { }
  virtual void visitErrorNode(antlr4::tree::ErrorNode * /*node*/) override { }

};

