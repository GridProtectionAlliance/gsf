
// Generated from FilterExpressionSyntax.g4 by ANTLR 4.7.1

#pragma once


#include "antlr4-runtime.h"




class  FilterExpressionSyntaxLexer : public antlr4::Lexer {
public:
  enum {
    T__0 = 1, T__1 = 2, T__2 = 3, T__3 = 4, T__4 = 5, T__5 = 6, T__6 = 7, 
    T__7 = 8, T__8 = 9, T__9 = 10, T__10 = 11, T__11 = 12, T__12 = 13, T__13 = 14, 
    T__14 = 15, T__15 = 16, T__16 = 17, T__17 = 18, T__18 = 19, T__19 = 20, 
    T__20 = 21, T__21 = 22, T__22 = 23, T__23 = 24, T__24 = 25, T__25 = 26, 
    T__26 = 27, BOOLEAN_LITERAL = 28, IDENTIFIER = 29, INTEGER_LITERAL = 30, 
    NUMERIC_LITERAL = 31, GUID_LITERAL = 32, MEASUREMENT_KEY_LITERAL = 33, 
    POINT_TAG_LITERAL = 34, STRING_LITERAL = 35, DATETIME_LITERAL = 36, 
    SINGLE_LINE_COMMENT = 37, MULTILINE_COMMENT = 38, SPACES = 39, UNEXPECTED_CHAR = 40, 
    K_ABS = 41, K_AND = 42, K_ASC = 43, K_BINARY = 44, K_BY = 45, K_CEILING = 46, 
    K_COALESCE = 47, K_CONVERT = 48, K_CONTAINS = 49, K_DATEADD = 50, K_DATEDIFF = 51, 
    K_DATEPART = 52, K_DESC = 53, K_ENDSWITH = 54, K_FILTER = 55, K_FLOOR = 56, 
    K_IIF = 57, K_IN = 58, K_INDEXOF = 59, K_IS = 60, K_ISDATE = 61, K_ISINTEGER = 62, 
    K_ISGUID = 63, K_ISNULL = 64, K_ISNUMERIC = 65, K_LASTINDEXOF = 66, 
    K_LEN = 67, K_LIKE = 68, K_LOWER = 69, K_MAX = 70, K_MIN = 71, K_NOT = 72, 
    K_NOW = 73, K_NULL = 74, K_OR = 75, K_ORDER = 76, K_POWER = 77, K_REGEXMATCH = 78, 
    K_REGEXVAL = 79, K_REPLACE = 80, K_REVERSE = 81, K_ROUND = 82, K_SQRT = 83, 
    K_SPLIT = 84, K_STARTSWITH = 85, K_STRCOUNT = 86, K_STRCMP = 87, K_SUBSTR = 88, 
    K_TOP = 89, K_TRIM = 90, K_TRIMLEFT = 91, K_TRIMRIGHT = 92, K_UPPER = 93, 
    K_UTCNOW = 94, K_WHERE = 95
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

