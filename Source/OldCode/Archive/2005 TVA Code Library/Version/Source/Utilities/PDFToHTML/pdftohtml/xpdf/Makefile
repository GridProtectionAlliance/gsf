# Generated automatically from Makefile.in by configure.
#========================================================================
#
# Xpdf Makefile
#
# Copyright 1996 Derek B. Noonburg
#
#========================================================================

SHELL = /bin/sh

srcdir = .

GOOSRCDIR = $(srcdir)/../goo
GOOLIBDIR = ../goo

CXXFLAGS += $(DEBUG) -O2 -DHAVE_CONFIG_H -I.. -I$(GOOSRCDIR) -I$(srcdir) 

CC ?= cc
CXX ?= c++
AR = ar rc
RANLIB = ranlib

LIBPREFIX = lib

#------------------------------------------------------------------------

.SUFFIXES: .cc

.cc.o:
	$(CXX) $(CXXFLAGS) -c $<

#------------------------------------------------------------------------

CXX_SRC = \
	$(srcdir)/Array.cc \
	$(srcdir)/BuiltinFont.cc \
	$(srcdir)/BuiltinFontTables.cc \
	$(srcdir)/CMap.cc \
	$(srcdir)/Catalog.cc \
	$(srcdir)/CharCodeToUnicode.cc \
	$(srcdir)/Decrypt.cc \
	$(srcdir)/Dict.cc \
	$(srcdir)/Error.cc \
	$(srcdir)/FontEncodingTables.cc \
	$(srcdir)/FontFile.cc \
	$(srcdir)/Function.cc \
	$(srcdir)/Gfx.cc \
	$(srcdir)/GfxFont.cc \
	$(srcdir)/GfxState.cc \
	$(srcdir)/GlobalParams.cc \
	$(srcdir)/ImageOutputDev.cc \
	$(srcdir)/JBIG2Stream.cc \
	$(srcdir)/Lexer.cc \
	$(srcdir)/Link.cc \
	$(srcdir)/NameToCharCode.cc \
	$(srcdir)/Object.cc \
	$(srcdir)/Outline.cc \
	$(srcdir)/OutputDev.cc \
	$(srcdir)/PBMOutputDev.cc \
	$(srcdir)/PDFDoc.cc \
	$(srcdir)/PDFDocEncoding.cc \
	$(srcdir)/PSOutputDev.cc \
	$(srcdir)/Page.cc \
	$(srcdir)/Parser.cc \
	$(srcdir)/SFont.cc \
	$(srcdir)/Stream.cc \
	$(srcdir)/T1Font.cc \
	$(srcdir)/TTFont.cc \
	$(srcdir)/TextOutputDev.cc \
	$(srcdir)/UnicodeMap.cc \
	$(srcdir)/XOutputDev.cc \
	$(srcdir)/Annot.cc \
        $(srcdir)/PSTokenizer.cc \
	$(srcdir)/XRef.cc

#------------------------------------------------------------------------

XPDF_OBJS = Array.o BuiltinFont.o BuiltinFontTables.o Catalog.o \
	CharCodeToUnicode.o CMap.o Decrypt.o Dict.o Error.o \
	FontEncodingTables.o FontFile.o Function.o Gfx.o \
	GfxFont.o GfxState.o GlobalParams.o Lexer.o Link.o \
	NameToCharCode.o Object.o Outline.o OutputDev.o Page.o Parser.o \
	PDFDoc.o PDFDocEncoding.o JBIG2Stream.o \
	PSOutputDev.o Stream.o UnicodeMap.o XRef.o PSTokenizer.o Annot.o 

$(LIBPREFIX)Xpdf.a: $(XPDF_OBJS)
	rm -f $(LIBPREFIX)Xpdf.a
	$(AR) $(LIBPREFIX)Xpdf.a $(XPDF_OBJS)
	$(RANLIB) $(LIBPREFIX)Xpdf.a

#------------------------------------------------------------------------

clean:
	rm -f $(XPDF_OBJS) $(LIBPREFIX)Xpdf.a

#------------------------------------------------------------------------

depend:
	$(CXX) $(CXXFLAGS) -MM $(CXX_SRC) >Makefile.dep
