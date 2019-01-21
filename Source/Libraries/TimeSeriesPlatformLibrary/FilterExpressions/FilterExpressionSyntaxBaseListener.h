
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

  virtual void enterTopLimit(FilterExpressionSyntaxParser::TopLimitContext * /*ctx*/) override { }
  virtual void exitTopLimit(FilterExpressionSyntaxParser::TopLimitContext * /*ctx*/) override { }

  virtual void enterOrderingTerm(FilterExpressionSyntaxParser::OrderingTermContext * /*ctx*/) override { }
  virtual void exitOrderingTerm(FilterExpressionSyntaxParser::OrderingTermContext * /*ctx*/) override { }

  virtual void enterExpressionList(FilterExpressionSyntaxParser::ExpressionListContext * /*ctx*/) override { }
  virtual void exitExpressionList(FilterExpressionSyntaxParser::ExpressionListContext * /*ctx*/) override { }

  virtual void enterExpression(FilterExpressionSyntaxParser::ExpressionContext * /*ctx*/) override { }
  virtual void exitExpression(FilterExpressionSyntaxParser::ExpressionContext * /*ctx*/) override { }

  virtual void enterPredicateExpression(FilterExpressionSyntaxParser::PredicateExpressionContext * /*ctx*/) override { }
  virtual void exitPredicateExpression(FilterExpressionSyntaxParser::PredicateExpressionContext * /*ctx*/) override { }

  virtual void enterValueExpression(FilterExpressionSyntaxParser::ValueExpressionContext * /*ctx*/) override { }
  virtual void exitValueExpression(FilterExpressionSyntaxParser::ValueExpressionContext * /*ctx*/) override { }

  virtual void enterNotOperator(FilterExpressionSyntaxParser::NotOperatorContext * /*ctx*/) override { }
  virtual void exitNotOperator(FilterExpressionSyntaxParser::NotOperatorContext * /*ctx*/) override { }

  virtual void enterUnaryOperator(FilterExpressionSyntaxParser::UnaryOperatorContext * /*ctx*/) override { }
  virtual void exitUnaryOperator(FilterExpressionSyntaxParser::UnaryOperatorContext * /*ctx*/) override { }

  virtual void enterExactMatchModifier(FilterExpressionSyntaxParser::ExactMatchModifierContext * /*ctx*/) override { }
  virtual void exitExactMatchModifier(FilterExpressionSyntaxParser::ExactMatchModifierContext * /*ctx*/) override { }

  virtual void enterComparisonOperator(FilterExpressionSyntaxParser::ComparisonOperatorContext * /*ctx*/) override { }
  virtual void exitComparisonOperator(FilterExpressionSyntaxParser::ComparisonOperatorContext * /*ctx*/) override { }

  virtual void enterLogicalOperator(FilterExpressionSyntaxParser::LogicalOperatorContext * /*ctx*/) override { }
  virtual void exitLogicalOperator(FilterExpressionSyntaxParser::LogicalOperatorContext * /*ctx*/) override { }

  virtual void enterBitwiseOperator(FilterExpressionSyntaxParser::BitwiseOperatorContext * /*ctx*/) override { }
  virtual void exitBitwiseOperator(FilterExpressionSyntaxParser::BitwiseOperatorContext * /*ctx*/) override { }

  virtual void enterMathOperator(FilterExpressionSyntaxParser::MathOperatorContext * /*ctx*/) override { }
  virtual void exitMathOperator(FilterExpressionSyntaxParser::MathOperatorContext * /*ctx*/) override { }

  virtual void enterFunctionName(FilterExpressionSyntaxParser::FunctionNameContext * /*ctx*/) override { }
  virtual void exitFunctionName(FilterExpressionSyntaxParser::FunctionNameContext * /*ctx*/) override { }

  virtual void enterFunctionExpression(FilterExpressionSyntaxParser::FunctionExpressionContext * /*ctx*/) override { }
  virtual void exitFunctionExpression(FilterExpressionSyntaxParser::FunctionExpressionContext * /*ctx*/) override { }

  virtual void enterLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext * /*ctx*/) override { }
  virtual void exitLiteralValue(FilterExpressionSyntaxParser::LiteralValueContext * /*ctx*/) override { }

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

