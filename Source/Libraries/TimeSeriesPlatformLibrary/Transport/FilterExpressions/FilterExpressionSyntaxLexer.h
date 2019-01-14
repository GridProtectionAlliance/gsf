
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

