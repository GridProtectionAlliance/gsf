// JRC 2004 - PDFToHTML class
#include "stdafx.h"
#include "PDFToHTML.h"
#include "_vcclrit.h"

using namespace TVA::Utilities;

String* CharBufferToString(const char* str);
char* StringToCharBuffer(String* gcStr);
GString* StringToGString(String* gcStr);
static GString* getInfoString(Dict *infoDict, char *key);
static GString* getInfoDate(Dict *infoDict, char *key);
void CaptureError(char* errMsg);

//
// PDFToHTML component implementation
//
PDFToHTML::PDFToHTML()
{
	firstPage = 1;
	lastPage = 0;

	noFrames = true;
	outputAsXML = false;
	outputHiddenText = false;
	noMerge = false;
	simplifiedOutput = false;
	convertPDFLinks = false;
	disposed = false;
	scale = 1.5;

	pdfFileName = S"";
	outputFileName = S"";
	ownerPassword = S"";
	userPassword = S"";
	textEncodingName = S"";
}

// This makes sure statics get properly initialized
void PDFToHTML::BeginInit()
{
	try
	{
		__crt_dll_initialize();
	}
	catch (System::Exception* e)
	{
		Console::WriteLine(e->Message);
	}
}

void PDFToHTML::EndInit()
{
}

void PDFToHTML::Dispose(Boolean Disposing)
{
    if (!disposed)
    {
        try
        {
			__crt_dll_terminate();
			disposed = true;
        }
		catch (System::Exception* e)
		{
			Console::WriteLine(e->Message);
		}
        __finally
        {
			__super::Dispose(Disposing);
        }
    }
}

// PDFToHTML source PDF file name property
String* PDFToHTML::get_PDFFileName()
{
	return pdfFileName;
}

void PDFToHTML::set_PDFFileName(String* value)
{
	pdfFileName = value;
}

// PDFToHTML destination HTML/XML file name property
String* PDFToHTML::get_OutputFileName()
{
	return outputFileName;
}

void PDFToHTML::set_OutputFileName(String* value)
{
	outputFileName = value;
}

// PDFToHTML first page to convert property
int PDFToHTML::get_FirstPageToConvert()
{
	return firstPage;
}
void PDFToHTML::set_FirstPageToConvert(int value)
{
	firstPage = value;
}

// PDFToHTML last page to convert property
int PDFToHTML::get_LastPageToConvert()
{
	return lastPage;
}
void PDFToHTML::set_LastPageToConvert(int value)
{
	lastPage = value;
}

// PDFToHTML generate HTML with no frames property
Boolean PDFToHTML::get_NoFrames()
{
	return noFrames;
}
void PDFToHTML::set_NoFrames(Boolean value)
{
	noFrames = value;
}

// PDFToHTML generate output as XML property
Boolean PDFToHTML::get_OutputAsXML()
{
	return outputAsXML;
}

void PDFToHTML::set_OutputAsXML(Boolean value)
{
	outputAsXML = value;
}

// PDFToHTML include hidden text in generated output property
Boolean PDFToHTML::get_OutputHiddenText()
{
	return outputHiddenText;
}

void PDFToHTML::set_OutputHiddenText(Boolean value)
{
	outputHiddenText = value;
}

// PDFToHTML do not merge paragraphs property
Boolean PDFToHTML::get_NoMerge()
{
	return noMerge;
}

void PDFToHTML::set_NoMerge(Boolean value)
{
	noMerge = value;
}

// PDFToHTML generate simplified output property
Boolean PDFToHTML::get_SimplifiedOutput()
{
	return simplifiedOutput;
}

void PDFToHTML::set_SimplifiedOutput(Boolean value)
{
	simplifiedOutput = value;
}

// PDFToHTML convert PDF links to HTML links property
Boolean PDFToHTML::get_ConvertPDFLinks()
{
	return convertPDFLinks;
}

void PDFToHTML::set_ConvertPDFLinks(Boolean value)
{
	convertPDFLinks = value;
}

// PDFToHTML zoom factor for HTML output property
double PDFToHTML::get_ZoomFactor()
{
	return scale;
}

void PDFToHTML::set_ZoomFactor(double value)
{
	if (scale > 3.0) scale = 3.0;
	if (scale < 0.5) scale = 0.5;
	scale = value;
}

// PDFToHTML owner password property
String* PDFToHTML::get_OwnerPassword()
{
	return ownerPassword;
}

void PDFToHTML::set_OwnerPassword(String* value)
{
	ownerPassword = value;
}

// PDFToHTML user password property
String* PDFToHTML::get_UserPassword()
{
	return userPassword;
}

void PDFToHTML::set_UserPassword(String* value)
{
	userPassword = value;
}

// PDFToHTML output text encoding name property
String* PDFToHTML::get_TextEncodingName()
{
	return textEncodingName;
}

void PDFToHTML::set_TextEncodingName(String* value)
{
	textEncodingName = value;
}

// PDFToHTML error messages property
String* PDFToHTML::get_ErrorMessages()
{
	return errorMessages->ToString();
}

// PDFToHTML version information property
String* PDFToHTML::VersionInfo()
{
	return S"Shared .NET PDF Conversion Code Library, Copyright © 2004, TVA - All rights reserved.  This .NET library uses code from the \"pdftohtml\" project (version 0.36) developed by Gueorgui Ovtcharov and Rainer Dorsch.  The \"pdftohtml\" project uses PDF reading code from Derek Noonburg's \"xpdf\" package (version 2.02), Copyright © 1996-2003.  PDF data structures, operators, and specification Copyright © 1995 Adobe Systems Inc.";
}

// This is used to capture errors messages from XPDF code...
void PDFToHTML::CaptureError(String* errMsg)
{
	errorMessages->Append(errMsg);
}

// Converstion routine
void PDFToHTML::Convert()
{
	PDFDoc *doc = NULL;
	GString *fileName = NULL;
	GString *docTitle = NULL;
	GString *author = NULL, *keywords = NULL, *subject = NULL, *date = NULL;
	GString *htmlFileName = NULL;
	HtmlOutputDev *htmlOut = NULL;
	GBool ok;
	GString *ownerPW, *userPW;
	::Object info;
	char *textEncName = NULL;
	char extension[16] = "png";
	//GString *psFileName = NULL;
	//PSOutputDev *psOut = NULL;
	//char *extsList[] = {"png", "jpeg", "bmp", "pcx", "tiff", "pbm", NULL};
	//char *ansiFileName;

	// Just wrapping in a try/catch/finally to make sure unmanaged code allocations always get released
	try
	{
		// Clear any existing error messages
		errorMessages = new StringBuilder();

		// Make sure source and destination file names have been specified
		if (pdfFileName->get_Length() == 0) throw new ArgumentNullException(S"Source PDF file name was not specified - check PDFFileName property");
		if (outputFileName->get_Length() == 0) throw new ArgumentNullException(S"Destination file name was not specified - check OutputFileName property");

		// read config file
		globalParams = new GlobalParams("");
		globalParams->setErrQuiet(false);

		if (textEncodingName->get_Length())
		{
			textEncName = StringToCharBuffer(textEncodingName);
			globalParams->setTextEncoding(textEncName);
			if(!globalParams->getTextEncoding())
				 throw new OutOfMemoryException(S"Unable to define text encoding parameters");
		}

		// open PDF file
		if (ownerPassword->get_Length())
			ownerPW = StringToGString(ownerPassword);
		else
			ownerPW = NULL;

		if (userPassword->get_Length())
			userPW = StringToGString(userPassword);
		else
			userPW = NULL;

		fileName = StringToGString(pdfFileName);
		doc = new PDFDoc(fileName, ownerPW, userPW);

		if (userPW) delete userPW;
		if (ownerPW) delete ownerPW;

		if (!doc->isOk()) throw new Exception("Error loading source PSD document");

		// check for copy permission
		if (!doc->okToCopy()) throw new Exception("Copy permission error - copying of text from this document is not allowed");

		// construct text file name
		htmlFileName = StringToGString(outputFileName);

		/*
		// JRC - for the .NET component, stout is always false...
		if (complexMode) {
			//noframes=gFalse;
			stout=gFalse;
		}

		if (stout) {
			noframes=gTrue;
			complexMode=gFalse;
		}
		*/

		if (outputAsXML)
		{
			simplifiedOutput = false;
			noFrames = true;
			noMerge = true;
		}

		// get page range
		if (firstPage < 1) firstPage = 1;

		if (lastPage < 1 || lastPage > doc->getNumPages())
			lastPage = doc->getNumPages();

		doc->getDocInfo(&info);
		if (info.isDict())
		{
			docTitle = getInfoString(info.getDict(), "Title");
			author = getInfoString(info.getDict(), "Author");
			keywords = getInfoString(info.getDict(), "Keywords");
			subject = getInfoString(info.getDict(), "Subject");
			date = getInfoDate(info.getDict(), "ModDate");
			if (!date) date = getInfoDate(info.getDict(), "CreationDate");
		}
		info.free();

		if (!docTitle) docTitle = new GString(htmlFileName);

		/*
		// JRC - for the .NET component, we don't support images for now since this code
		//		 just mades a command-line call to the the Ghostscript converter

		// determine extensions of output background images
		for(int i = 0; extsList[i]; i++)
		{
			if( strstr(gsDevice, extsList[i]) != (char *) NULL )
			{
				strncpy(extension, extsList[i], sizeof(extension));
				break;
			}
		}
		*/

		//rawOrder = complexMode; // todo: figure out what exactly rawOrder do :)

		// write text file
		htmlOut = new HtmlOutputDev(
			(GBool)!simplifiedOutput,
			(GBool)noFrames,
			(GBool)outputAsXML,
			(GBool)outputHiddenText,
			(GBool)noMerge,
			(GBool)convertPDFLinks,
			htmlFileName->getCString(),
			docTitle->getCString(),
			author ? author->getCString() : NULL,
			keywords ? keywords->getCString() : NULL,
			subject ? subject->getCString() : NULL,
			date ? date->getCString() : NULL,
			extension,
			(GBool)!simplifiedOutput,
			firstPage,
			doc->getCatalog()->getOutline()->isDict());

		delete docTitle;

		if (author) delete author;
		if (keywords) delete keywords;
		if (subject) delete subject;
		if (date) delete date;

		if (htmlOut->isOk())
		{
			doc->displayPages(htmlOut, firstPage, lastPage, static_cast<int>(72*scale), 0, gTrue);
  			if (!outputAsXML) htmlOut->dumpDocOutline(doc->getCatalog());
		}

		// Again, since pdftohtml conversion code just makes a call to another command line component (Ghostscript) for
		// image conversion, I opted not to include this feature in the .NET PDF convertet component.  I suppose I could
		// go download the open source Ghostscript component and plug that in - but I won't until I really need to...
		/*
		if( complexMode && !xml && !ignore )
		{
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

			//sprintf(buf, "%s -sDEVICE=png16m -dBATCH -dNOPROMPT -dNOPAUSE -r72 -sOutputFile=%s%%03d.png -g%dx%d -q %s", GHOSTSCRIPT, htmlFileName->getCString(), w, h, psFileName->getCString());

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
		*/

		delete htmlOut;
	}
	catch (Exception* ex)
	{
		// Just rethrow any exceptions back to consumer, only in try-catch to release non-managed code allocations in finally clause anyway...
		throw ex;
	}
	__finally
	{
		// clean up
		if (textEncName)
			free(textEncName);

		if (doc) delete doc;
		if (globalParams) delete globalParams;
		if (htmlFileName) delete htmlFileName;

		HtmlFont::clear();
	}
}

static GString* getInfoString(Dict *infoDict, char *key)
{
	::Object obj;
	GString *s1 = NULL;

	if (infoDict->lookup(key, &obj)->isString())
		s1 = new GString(obj.getString());
	obj.free();

	return s1;
}

static GString* getInfoDate(Dict *infoDict, char *key)
{
	::Object obj;
	char *s;
	int year, mon, day, hour, min, sec;
	struct tm tmStruct;
	GString *result = NULL;
	char buf[256];

	if (infoDict->lookup(key, &obj)->isString())
	{
		s = obj.getString()->getCString();

		if (s[0] == 'D' && s[1] == ':')
			s += 2;

		if (sscanf(s, "%4d%2d%2d%2d%2d%2d", &year, &mon, &day, &hour, &min, &sec) == 6)
		{
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

			if (strftime(buf, sizeof(buf), "%Y-%m-%dT%H:%M:%S+00:00", &tmStruct))
				result = new GString(buf);
			else
				result = new GString(s);
		}
		else
			result = new GString(s);
	}
	obj.free();

	return result;
}

String* CharBufferToString(const char* str)
{
	int len = strlen(str);
	System::Byte gcBytes[] = new System::Byte[len];
	Marshal::Copy((IntPtr)(char *)str, gcBytes, 0, len);
	return ASCIIEncoding::ASCII->GetString(gcBytes);
}

// Caller responsible for calling "free()" on returned buffer...
char* StringToCharBuffer(String* gcStr)
{
	System::Byte gcBytes[] = ASCIIEncoding::ASCII->GetBytes(gcStr);
	char* str = (char*)malloc(gcStr->get_Length() + 1);
	if (str == NULL) throw new OutOfMemoryException();
	Marshal::Copy(gcBytes, 0, (IntPtr)str, gcStr->get_Length());
	str[gcStr->get_Length()] = '\0';
	return str;
}

// pdftohtml code uses a "GString" object, this function handles the conversion...
GString* StringToGString(String* gcStr)
{
	char* str = StringToCharBuffer(gcStr);
	GString* gStr = new GString(str);
	free(str);
	if (gStr == NULL) throw new OutOfMemoryException();
	return gStr;
}

void CaptureError(char* errMsg)
{
	PDFToHTML::CaptureError(CharBufferToString(errMsg));
}