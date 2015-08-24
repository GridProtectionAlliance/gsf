// JRC 2004 - PDFToHTML.h
#pragma once

#pragma unmanaged
#include <stdio.h>
#include <stdlib.h>
#include <stddef.h>
#include <string.h>
#include <aconf.h>
#include <time.h>
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
#pragma managed

#using <System.dll>
#using <System.Drawing.dll>
using namespace System;
using namespace System::IO;
using namespace System::Text;
using namespace System::Collections;
using namespace System::Drawing;
using namespace System::Reflection;
using namespace System::ComponentModel;
using namespace System::Runtime::InteropServices;

namespace TVA
{
	namespace Utilities
	{
		// Important:  If you intend on using this component directly as a class (i.e., not as designable component), you
		// will need to manually call the the "BeginInit" method after you have instantiated your class to make sure statics
		// get properly initialized...

		// Because this assembly contains unmanaged code (xpdf etc.), use of this class as a designable component forces
		// the following limitations *:
		//
		//		1) You must reference the assembly in your project before dragging it onto a design surface
		//		2) You must change the "Copy Local" property of the reference to False
		//		3) Manually copy the assembly into your project's bin folder so it can find the assembly at run time
		//		4) Now you can safely drag the component onto a design surface
		//
		// * Note: This is only a limitation that is incurred under Visual Studio 2002 - this is not an issue when
		// using Visual Studio 2003.  Visual Studio 2002 apparently only loads assemblies as a byte stream and
		// this is not possible if an assembly contains unmanaged code...
		[ToolboxBitmap(__typeof(System::Object), S"TVA.Utilities.PDFToHTML.bmp"), DefaultProperty(S"FileName"), DefaultMember(S"Files")]
		public __gc class PDFToHTML :  public Component, public ISupportInitialize
		{
			public:
				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"Source PDF file name."), DefaultValue(S"")]
				__property String* get_PDFFileName();
				__property void set_PDFFileName(String* value);

				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"Destination HTML/XML file name without \".html\" or \".xml\" suffix."), DefaultValue(S"")]
				__property String* get_OutputFileName();
				__property void set_OutputFileName(String* value);

				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"First page to convert."), DefaultValue(1)]
				__property int get_FirstPageToConvert();
				__property void set_FirstPageToConvert(int value);

				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"Last page to convert.  Set to zero to convert all pages."), DefaultValue(0)]
				__property int get_LastPageToConvert();
				__property void set_LastPageToConvert(int value);

				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"Generate HTML with no frames."), DefaultValue(true)]
				__property Boolean get_NoFrames();
				__property void set_NoFrames(Boolean value);

				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"Generate output as XML."), DefaultValue(false)]
				__property Boolean get_OutputAsXML();
				__property void set_OutputAsXML(Boolean value);

				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"Include hidden text in generated output."), DefaultValue(false)]
				__property Boolean get_OutputHiddenText();
				__property void set_OutputHiddenText(Boolean value);

				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"Do not merge paragraphs."), DefaultValue(false)]
				__property Boolean get_NoMerge();
				__property void set_NoMerge(Boolean value);

				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"Generate simplified output."), DefaultValue(false)]
				__property Boolean get_SimplifiedOutput();
				__property void set_SimplifiedOutput(Boolean value);

				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"Convert PDF links to HTML links."), DefaultValue(false)]
				__property Boolean get_ConvertPDFLinks();
				__property void set_ConvertPDFLinks(Boolean value);

				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"Zoom factor for HTML output.  Range: 0.5 to 3.0, defaults to 1.5."), DefaultValue(1.5)]
				__property double get_ZoomFactor();
				__property void set_ZoomFactor(double value);

				[Browsable(true), Category(S"PDF Conversion Settings"), Description(S"Output text encoding name."), DefaultValue(S"")]
				__property String* get_TextEncodingName();
				__property void set_TextEncodingName(String* value);

				[Browsable(true), Category(S"PDF Encryption Settings"), Description(S"Owner password."), DefaultValue(S"")]
				__property String* get_OwnerPassword();
				__property void set_OwnerPassword(String* value);

				[Browsable(true), Category(S"PDF Encryption Settings"), Description(S"User password."), DefaultValue(S"")]
				__property String* get_UserPassword();
				__property void set_UserPassword(String* value);

				[Browsable(false)]
				__property String* get_ErrorMessages();

				PDFToHTML();
				void Convert();
				virtual void BeginInit();
				virtual void EndInit();

				static String* VersionInfo();

			protected:
				virtual void Dispose(Boolean Disposing);

			private public:
				static void CaptureError(String* errMsg);

			private:
				static StringBuilder* errorMessages = new StringBuilder();

				int firstPage;
				int lastPage;

				Boolean noFrames;
				Boolean outputAsXML;
				Boolean outputHiddenText;
				Boolean noMerge;
				Boolean simplifiedOutput;
				Boolean convertPDFLinks;
				Boolean disposed;
				double scale;

				String* pdfFileName;
				String* outputFileName;
				String* ownerPassword;
				String* userPassword;
				String* textEncodingName;
		};
	}
}
