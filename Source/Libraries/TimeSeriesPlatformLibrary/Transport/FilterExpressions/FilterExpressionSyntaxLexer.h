
// Generated from FilterExpressionSyntax.g4 by ANTLR 4.7.1

#pragma once


#include "antlr4-runtime.h"




class  FilterExpressionSyntaxLexer : public antlr4::Lexer {
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
    STRING_LITERAL = 52, DATETIME_LITERAL = 53, GUID_LITERAL = 54, BOOLEAN_LITERAL = 55, 
    MEASUREMENT_KEY_LITERAL = 56, POINT_TAG_LITERAL = 57, SINGLE_LINE_COMMENT = 58, 
    MULTILINE_COMMENT = 59, SPACES = 60, UNEXPECTED_CHAR = 61
  };

  FilterExpressionSyntaxLexer(antlr4::CharStream *input);
  ~FilterExpressionSyntaxLexer();

  virtual std::string getGrammarFileName() const override;
  virtual const std::vector<std::string>& getRuleNames() const override;

  virtual const std::vector<std::string>& getChannelNames() const override;
  virtual const std::vector<std::string>& getModeNames() const override;
  virtual const std::vector<std::string>& getTokenNames() const override; // deprecated, use vocabulary instead
  virtual antlr4::dfa::Vocabulary& getVocabulary() const override;

  virtual const std::vector<uint16_t> getSerializedATN() const override;
  virtual const antlr4::atn::ATN& getATN() const override;

private:
  static std::vector<antlr4::dfa::DFA> _decisionToDFA;
  static antlr4::atn::PredictionContextCache _sharedContextCache;
  static std::vector<std::string> _ruleNames;
  static std::vector<std::string> _tokenNames;
  static std::vector<std::string> _channelNames;
  static std::vector<std::string> _modeNames;

  static std::vector<std::string> _literalNames;
  static std::vector<std::string> _symbolicNames;
  static antlr4::dfa::Vocabulary _vocabulary;
  static antlr4::atn::ATN _atn;
  static std::vector<uint16_t> _serializedATN;


  // Individual action functions triggered by action() above.

  // Individual semantic predicate functions triggered by sempred() above.

  struct Initializer {
    Initializer();
  };
  static Initializer _init;
};

