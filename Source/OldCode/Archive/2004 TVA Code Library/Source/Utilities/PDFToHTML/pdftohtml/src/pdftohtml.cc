//========================================================================
//
// pdftohtml.cc
//
//
// Copyright 1999-2000 G. Ovtcharov
//========================================================================

#include <stdio.h>
#include <stdlib.h>
#include <stddef.h>
#include <string.h>
#include <aconf.h>
#include <time.h>
#include "parseargs.h"
#include "GString.h"
#include "gmem.h"
#include "Object.h"
#include "Stream.h"
#include "Array.h"
#include "Dict.h"
#include "XRef.h"
#include "Catalog.h"
#include "Page.h"
#include "PDFDoc.h"
#include "HtmlOutputDev.h"
#include "PSOutputDev.h"
#include "GlobalParams.h"
#include "Error.h"
#include "config.h"
#include "gfile.h"

static int firstPage = 1;
static int lastPage = 0;
static GBool rawOrder = gTrue;
GBool printCommands = gTrue;
static GBool printHelp = gFalse;
GBool printHtml = gFalse;
GBool complexMode=gFalse;
GBool ignore=gFalse;
//char extension[5]=".png";
double scale=1.5;
GBool noframes=gFalse;
GBool stout=gFalse;
GBool xml=gFalse;
GBool errQuiet=gFalse;

GBool showHidden = gFalse;
GBool noMerge = gFalse;
static char ownerPassword[33] = "";
static char userPassword[33] = "";
static char gsDevice[33] = "png16m";
static GBool printVersion = gFalse;

static GString* getInfoString(Dict *infoDict, char *key);
static GString* getInfoDate(Dict *infoDict, char *key);

static char textEncName[128] = "";

static ArgDesc argDesc[] = {
  {"-f",      argInt,      &firstPage,     0,
   "first page to convert"},
  {"-l",      argInt,      &lastPage,      0,
   "last page to convert"},
  /*{"-raw",    argFlag,     &rawOrder,      0,
    "keep strings in content stream order"},*/
  {"-q",      argFlag,     &errQuiet,      0,
   "don't print any messages or errors"},
  {"-h",      argFlag,     &printHelp,     0,
   "print usage information"},
  {"-help",   argFlag,     &printHelp,     0,
   "print usage information"},
  {"-p",      argFlag,     &printHtml,     0,
   "exchange .pdf links by .html"}, 
  {"-c",      argFlag,     &complexMode,          0,
   "generate complex document"},
  {"-i",      argFlag,     &ignore,        0,
   "ignore images"},
  {"-noframes", argFlag,   &noframes,      0,
   "generate no frames"},
  {"-stdout"  ,argFlag,    &stout,         0,
   "use standard output"},
  {"-zoom",   argFP,    &scale,         0,
   "zoom the pdf document (default 1.5)"},
  {"-xml",    argFlag,    &xml,         0,
   "output for XML post-processing"},
  {"-hidden", argFlag,   &showHidden,   0,
   "output hidden text"},
  {"-nomerge", argFlag, &noMerge, 0,
   "do not merge paragraphs"},   
  {"-enc",    argString,   textEncName,    sizeof(textEncName),
   "output text encoding name"},
  {"-dev",    argString,   gsDevice,       sizeof(gsDevice),
   "output device name for Ghostscript (png16m, jpeg etc)"},
  {"-v",      argFlag,     &printVersion,  0,
   "print copyright and version info"},
  {"-opw",    argString,   ownerPassword,  sizeof(ownerPassword),
   "owner password (for encrypted files)"},
  {"-upw",    argString,   userPassword,   sizeof(userPassword),
   "user password (for encrypted files)"},
  {NULL}
};

int main(int argc, char *argv[]) {
  PDFDoc *doc = NULL;
  GString *fileName = NULL;
  GString *docTitle = NULL;
  GString *author = NULL, *keywords = NULL, *subject = NULL, *date = NULL;
  GString *htmlFileName = NULL;
  GString *psFileName = NULL;
  HtmlOutputDev *htmlOut = NULL;
  PSOutputDev *psOut = NULL;
  GBool ok;
  char *p;
  char extension[16] = "png";
  GString *ownerPW, *userPW;
  Object info;
  char * extsList[] = {"png", "jpeg", "bmp", "pcx", "tiff", "pbm", NULL};

  // parse args
  ok = parseArgs(argDesc, &argc, argv);
  if (!ok || argc < 2 || argc > 3 || printHelp || printVersion) {
    fprintf(stderr, "pdftohtml version %s http://pdftohtml.sourceforge.net/, based on Xpdf version %s\n", "0.36", xpdfVersion);
    fprintf(stderr, "%s\n", "Copyright 1999-2003 Gueorgui Ovtcharov and Rainer Dorsch");
    fprintf(stderr, "%s\n\n", xpdfCopyright);
    if (!printVersion) {
      printUsage("pdftohtml", "<PDF-file> [<html-file> <xml-file>]", argDesc);
    }
    exit(1);
  }
 
  // init error file
  //errorInit();

  // read config file
  globalParams = new GlobalParams("");

  if (errQuiet) {
    globalParams->setErrQuiet(errQuiet);
    printCommands = gFalse; // I'm not 100% what is the differecne between them
  }

  if (textEncName[0]) {
    globalParams->setTextEncoding(textEncName);
    if( !globalParams->getTextEncoding() )  {
	goto error;    
    }
  }

  // open PDF file
  if (ownerPassword[0]) {
    ownerPW = new GString(ownerPassword);
  } else {
    ownerPW = NULL;
  }
  if (userPassword[0]) {
    userPW = new GString(userPassword);
  } else {
    userPW = NULL;
  }

  fileName = new GString(argv[1]);

  doc = new PDFDoc(fileName, ownerPW, userPW);
  if (userPW) {
    delete userPW;
  }
  if (ownerPW) {
    delete ownerPW;
  }
  if (!doc->isOk()) {
    goto error;
  }

  // check for copy permission
  if (!doc->okToCopy()) {
    error(-1, "Copying of text from this document is not allowed.");
    goto error;
  }

  // construct text file name
  if (argc == 3) {
    GString* tmp = new GString(argv[2]);
    p=tmp->getCString()+tmp->getLength()-5;
    if (!xml)
      if (!strcmp(p, ".html") || !strcmp(p, ".HTML"))
	htmlFileName = new GString(tmp->getCString(),
				   tmp->getLength() - 5);
      else htmlFileName =new GString(tmp);
    else   
      if (!strcmp(p, ".xml") || !strcmp(p, ".XML"))
	htmlFileName = new GString(tmp->getCString(),
				   tmp->getLength() - 5);
      else htmlFileName =new GString(tmp);
    
    delete tmp;
  } else {
    p = fileName->getCString() + fileName->getLength() - 4;
    if (!strcmp(p, ".pdf") || !strcmp(p, ".PDF"))
      htmlFileName = new GString(fileName->getCString(),
				 fileName->getLength() - 4);
    else
      htmlFileName = fileName->copy();
    //   htmlFileName->append(".html");
  }
  
   if (scale>3.0) scale=3.0;
   if (scale<0.5) scale=0.5;
   
   if (complexMode) {
     //noframes=gFalse;
     stout=gFalse;
   } 

   if (stout) {
     noframes=gTrue;
     complexMode=gFalse;
   }

   if (xml)
   { 
       complexMode = gTrue;
       noframes = gTrue;
       noMerge = gTrue;
   }

  // get page range
  if (firstPage < 1)
    firstPage = 1;
  if (lastPage < 1 || lastPage > doc->getNumPages())
    lastPage = doc->getNumPages();

  doc->getDocInfo(&info);
  if (info.isDict()) {
    docTitle = getInfoString(info.getDict(), "Title");
    author = getInfoString(info.getDict(), "Author");
    keywords = getInfoString(info.getDict(), "Keywords");
    subject = getInfoString(info.getDict(), "Subject");
    date = getInfoDate(info.getDict(), "ModDate");
    if( !date )
	date = getInfoDate(info.getDict(), "CreationDate");
  }
  info.free();
  if( !docTitle ) docTitle = new GString(htmlFileName);

  /* determine extensions of output backgroun images */
  {int i;
  for(i = 0; extsList[i]; i++)
  {
	  if( strstr(gsDevice, extsList[i]) != (char *) NULL )
	  {
		  strncpy(extension, extsList[i], sizeof(extension));
		  break;
	  }
  }}

  rawOrder = complexMode; // todo: figure out what exactly rawOrder do :)

  // write text file
  htmlOut = new HtmlOutputDev(htmlFileName->getCString(), 
	  docTitle->getCString(), 
	  author ? author->getCString() : NULL,
	  keywords ? keywords->getCString() : NULL, 
          subject ? subject->getCString() : NULL, 
	  date ? date->getCString() : NULL,
	  extension,
	  rawOrder, 
	  firstPage,
	  doc->getCatalog()->getOutline()->isDict());
  delete docTitle;
  if( author )
  {   
      delete author;
  }
  if( keywords )
  {
      delete keywords;
  }
  if( subject )
  {
      delete subject;
  }
  if( date )
  {
      delete date;
  }

  if (htmlOut->isOk())
  {
	doc->displayPages(htmlOut, firstPage, lastPage, static_cast<int>(72*scale), 0, gTrue);
  	if (!xml)
	{
		htmlOut->dumpDocOutline(doc->getCatalog());
	}
  }
  
  if( complexMode && !xml && !ignore ) {
    int h=xoutRound(htmlOut->getPageHeight()/scale);
    int w=xoutRound(htmlOut->getPageWidth()/scale);
    //int h=xoutRound(doc->getPageHeight(1)/scale);
    //int w=xoutRound(doc->getPageWidth(1)/scale);

    psFileName = new GString(htmlFileName->getCString());
    psFileName->append(".ps");

    globalParams->setPSPaperWidth(w);
    globalParams->setPSPaperHeight(h);
    globalParams->setPSNoText(gTrue);
    psOut = new PSOutputDev(psFileName->getCString(), doc->getXRef(),
			    doc->getCatalog(), firstPage, lastPage, psModePS);
    doc->displayPages(psOut, firstPage, lastPage, 
	    static_cast<int>(72*scale), 0, gFalse);
    delete psOut;

    /*sprintf(buf, "%s -sDEVICE=png16m -dBATCH -dNOPROMPT -dNOPAUSE -r72 -sOutputFile=%s%%03d.png -g%dx%d -q %s", GHOSTSCRIPT, htmlFileName->getCString(), w, h,
      psFileName->getCString());*/
    
    GString *gsCmd = new GString(GHOSTSCRIPT);
    GString *tw, *th, *sc;
    gsCmd->append(" -sDEVICE=");
	gsCmd->append(gsDevice);
	gsCmd->append(" -dBATCH -dNOPROMPT -dNOPAUSE -r");
    sc = GString::fromInt(static_cast<int>(72*scale));
    gsCmd->append(sc);
    gsCmd->append(" -sOutputFile=");
    gsCmd->append("\"");
    gsCmd->append(htmlFileName);
    gsCmd->append("%03d.");
	gsCmd->append(extension);
	gsCmd->append("\" -g");
    tw = GString::fromInt(static_cast<int>(scale*w));
    gsCmd->append(tw);
    gsCmd->append("x");
    th = GString::fromInt(static_cast<int>(scale*h));
    gsCmd->append(th);
    gsCmd->append(" -q \"");
    gsCmd->append(psFileName);
    gsCmd->append("\"");
//    printf("running: %s\n", gsCmd->getCString());
    if( !executeCommand(gsCmd->getCString()) && !errQuiet) {
      error(-1, "Failed to launch Ghostscript!\n");
    }
    unlink(psFileName->getCString());
    delete tw;
    delete th;
    delete sc;
    delete gsCmd;
    delete psFileName;
  }
  
  delete htmlOut;

  // clean up
 error:
  if(doc) delete doc;
  if(globalParams) delete globalParams;

  if(htmlFileName) delete htmlFileName;
  HtmlFont::clear();
  
  // check for memory leaks
  Object::memCheck(stderr);
  gMemReport(stderr);

  return 0;
}

static GString* getInfoString(Dict *infoDict, char *key) {
  Object obj;
  GString *s1 = NULL;

  if (infoDict->lookup(key, &obj)->isString()) {
    s1 = new GString(obj.getString());
  }
  obj.free();
  return s1;
}

static GString* getInfoDate(Dict *infoDict, char *key) {
  Object obj;
  char *s;
  int year, mon, day, hour, min, sec;
  struct tm tmStruct;
  GString *result = NULL;
  char buf[256];

  if (infoDict->lookup(key, &obj)->isString()) {
    s = obj.getString()->getCString();
    if (s[0] == 'D' && s[1] == ':') {
      s += 2;
    }
    if (sscanf(s, "%4d%2d%2d%2d%2d%2d",
               &year, &mon, &day, &hour, &min, &sec) == 6) {
      tmStruct.tm_year = year - 1900;
      tmStruct.tm_mon = mon - 1;
      tmStruct.tm_mday = day;
      tmStruct.tm_hour = hour;
      tmStruct.tm_min = min;
      tmStruct.tm_sec = sec;
      tmStruct.tm_wday = -1;
      tmStruct.tm_yday = -1;
      tmStruct.tm_isdst = -1;
      mktime(&tmStruct); // compute the tm_wday and tm_yday fields
      if (strftime(buf, sizeof(buf), "%Y-%m-%dT%H:%M:%S+00:00", &tmStruct)) {
	result = new GString(buf);
      } else {
        result = new GString(s);
      }
    } else {
      result = new GString(s);
    }
  }
  obj.free();
  return result;
}

