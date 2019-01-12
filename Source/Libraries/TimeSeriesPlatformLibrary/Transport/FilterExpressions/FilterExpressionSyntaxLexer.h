
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
    T__26 = 27, K_ABS = 28, K_AND = 29, K_ASC = 30, K_BINARY = 31, K_BY = 32, 
    K_CEILING = 33, K_COALESCE = 34, K_CONVERT = 35, K_CONTAINS = 36, K_DATEADD = 37, 
    K_DATEDIFF = 38, K_DATEPART = 39, K_DESC = 40, K_ENDSWITH = 41, K_FILTER = 42, 
    K_FLOOR = 43, K_IIF = 44, K_IN = 45, K_INDEXOF = 46, K_IS = 47, K_ISDATE = 48, 
    K_ISINTEGER = 49, K_ISGUID = 50, K_ISNULL = 51, K_ISNUMERIC = 52, K_LASTINDEXOF = 53, 
    K_LEN = 54, K_LIKE = 55, K_LOWER = 56, K_MAX = 57, K_MIN = 58, K_NOT = 59, 
    K_NOW = 60, K_NULL = 61, K_OR = 62, K_ORDER = 63, K_POWER = 64, K_REGEXMATCH = 65, 
    K_REGEXVAL = 66, K_REPLACE = 67, K_REVERSE = 68, K_ROUND = 69, K_SQRT = 70, 
    K_SPLIT = 71, K_STARTSWITH = 72, K_STRCOUNT = 73, K_STRCMP = 74, K_SUBSTR = 75, 
    K_TOP = 76, K_TRIM = 77, K_TRIMLEFT = 78, K_TRIMRIGHT = 79, K_UPPER = 80, 
    K_UTCNOW = 81, K_WHERE = 82, BOOLEAN_LITERAL = 83, IDENTIFIER = 84, 
    INTEGER_LITERAL = 85, NUMERIC_LITERAL = 86, GUID_LITERAL = 87, MEASUREMENT_KEY_LITERAL = 88, 
    POINT_TAG_LITERAL = 89, STRING_LITERAL = 90, DATETIME_LITERAL = 91, 
    SINGLE_LINE_COMMENT = 92, MULTILINE_COMMENT = 93, SPACES = 94, UNEXPECTED_CHAR = 95
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

