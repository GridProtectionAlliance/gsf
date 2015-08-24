# Generated automatically from Makefile.in by configure.
#========================================================================
#
# Goo library Makefile
#
# Copyright 1996 Derek B. Noonburg
#
#========================================================================

SHELL = /bin/sh

srcdir = .

CFLAGS += $(DEBUG) -O2 -DHAVE_CONFIG_H -I.. -I$(srcdir) 
#-g -DDEBUG_MEM
CXXFLAGS += $(DEBUG) -O2 -DHAVE_CONFIG_H -I.. -I$(srcdir) 
#-g -DDEBUG_MEM

CC ?= cc
CXX ?= c++
AR = ar rc
RANLIB = ranlib

LIBPREFIX = lib

#------------------------------------------------------------------------

.SUFFIXES: .cc

.cc.o:
	$(CXX) $(CXXFLAGS) -c $<

.SUFFIXES: .c

.c.o:
	$(CC) $(CFLAGS) -c $<


#------------------------------------------------------------------------

CXX_SRC = \
	$(srcdir)/GHash.cc \
	$(srcdir)/GList.cc \
	$(srcdir)/GString.cc \
	$(srcdir)/gmempp.cc \
	$(srcdir)/gfile.cc

C_SRC = \
	$(srcdir)/gmem.c \
	$(srcdir)/parseargs.c

#------------------------------------------------------------------------

GOO_CXX_OBJS = GHash.o GList.o GString.o gmempp.o gfile.o
GOO_C_OBJS = gmem.o parseargs.o
GOO_OBJS = $(GOO_CXX_OBJS) $(GOO_C_OBJS)

$(LIBPREFIX)Goo.a: $(GOO_OBJS) $(GOO_C_OBJS)
	rm -f $(LIBPREFIX)Goo.a
	$(AR) $(LIBPREFIX)Goo.a $(GOO_OBJS)
	$(RANLIB) $(LIBPREFIX)Goo.a

#------------------------------------------------------------------------

clean:
	rm -f $(GOO_OBJS) $(LIBPREFIX)Goo.a

#------------------------------------------------------------------------

depend:
	$(CXX) $(CXXFLAGS) -MM $(CXX_SRC) >Makefile.dep
	$(CC) $(CFLAGS) -MM $(C_SRC) >>Makefile.dep
