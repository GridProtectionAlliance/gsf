

SHELL = /bin/sh

SRCDIR = .
XPDFSRCDIR = ../xpdf
XPDFLIBDIR = ../xpdf

GOOSRCDIR = ../goo
GOOLIBDIR = ../goo


CXXFLAGS += $(DEBUG) -DHAVE_DIRENT_H=1  -I.. -DHAVE_REWINDDIR=1 -DHAVE_POPEN=1   -I$(GOOSRCDIR) -I$(SRCDIR) -I$(XPDFSRCDIR) 

LDFLAGS = 

OTHERLIBS = 

CXX ?= c++

LIBPREFIX = lib
EXE = 

#------------------------------------------------------------------------

.SUFFIXES: .cc

.cc.o:
	$(CXX) $(CXXFLAGS) -c $<

#------------------------------------------------------------------------

CXX_SRC = \
	$(SRCDIR)/pdftohtml.cc \
	$(SRCDIR)/HtmlOutputDev.cc \
	$(SRCDIR)/HtmlFonts.cc \
	$(SRCDIR)/HtmlLinks.cc 

#------------------------------------------------------------------------

all: pdftohtml$(EXE)

#-------------------------------------------------------------------------

PDFTOHTML_OBJS = HtmlOutputDev.o HtmlFonts.o HtmlLinks.o \
    pdftohtml.o
PDFTOHTML_LIBS = -L$(GOOLIBDIR) -L$(XPDFLIBDIR) $(OTHERLIBS) -lXpdf -lGoo -lm

pdftohtml$(EXE): $(PDFTOHTML_OBJS) $(GOOLIBDIR)/$(LIBPREFIX)Goo.a \
	$(XPDFLIBDIR)/$(LIBPREFIX)Xpdf.a
	$(CXX) $(CXXFLAGS) $(LDFLAGS) -o pdftohtml$(EXE) $(PDFTOHTML_OBJS) \
		$(PDFTOHTML_LIBS)


#-------------------------------------------------------------------------
clean:
	rm -f $(PDFTOHTML_OBJS) pdftohtml$(EXE)

#------------------------------------------------------------------------

distdepend:
	cp Makefile.in Makefile.in.bak
	sed '/^#----- dependences -----/q' Makefile.in.bak >Makefile.in
	$(CXX) $(CXXFLAGS) -MM $(CXX_SRC) >>Makefile.in

