// JRC 2004 - ZipFile.h
#pragma once
#using <System.dll>
#using <System.Drawing.dll>
#include "Common.h"

using namespace System::Collections;
using namespace System::Drawing;
using namespace System::Reflection;
using namespace System::ComponentModel;
using namespace System::Runtime::InteropServices;
using namespace System::Text::RegularExpressions;

// IMPORTANT: When compiling this code, do NOT turn on global optimizations - this breaks the ZipFile Extract
// functionality (just stops working - doesn't throw errors)...
namespace TVA
{
	namespace Compression
	{
		public __value enum PathInclusion : int
		{
			FullPath,
			RelativePath,
			NoPath
		};

		public __value enum UpdateOption : int
		{
			Never,
			Always,
			ZipFileIsNewer,
			DiskFileIsNewer
		};

		public __delegate void CurrentFileEventHandler(String* FullFileName, String* RelativeFileName);

		// Because this assembly contains unmanaged code (zlib etc.), use of this class as a designable component forces
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
		[ToolboxBitmap(__typeof(System::Object), S"TVA.Shared.Compression.ZipFile.bmp"), DefaultProperty(S"FileName"), DefaultMember(S"Files")]
		public __gc class ZipFile :  public Component
		{
			public:
				__gc class CompressedFile
				{
					public:
						__property int get_CompressionMethod();
						__property int get_CRC();
						__property int get_CompressedSize();
						__property int get_UncompressedSize();
						__property int get_DiskNumberStart();
						__property int get_InternalFileAttributes();
						__property int get_ExternalFileAttributes();
						__property int get_DOSDateTime();
						__property DateTime get_FileDateTime();
						__property String* get_FileName();
						__property String* get_FileComment();
						__property System::Byte get_ExtraData() [];
						__property int get_ZipVersionCreatedBy();
						__property int get_ZipVersionNeededToExtract();
						__property int get_Flag();

					public private:	// essentially friend access: internally public, externally private
						CompressedFile(unzFile file);

						unz_file_info fileInfo;
						String* fileName;
						String* fileComment;
						System::Byte extraData[];
				};

				[DefaultMember(S"Item")]
				__gc class CompressedFiles : public IEnumerable
				{
					public:
						__property CompressedFile* get_Item(int Index);
						__property CompressedFile* get_Item(String* FileSpec);
						__property CompressedFile* get_Item(String* FileSpec, Boolean RecurseSubdirectories);
						__property int get_Count();
						Boolean Find(String* FileSpec);
						Boolean Find(String* FileSpec, Boolean RecurseSubdirectories);
						Boolean Find(String* FileSpec, Boolean RecurseSubdirectories, Int32* FirstMatchedIndex);
						Boolean Find(String* FileSpec, Boolean RecurseSubdirectories, IList* MatchedIndices);
						IEnumerator* GetEnumerator();

					public private:	// essentially friend access: internally public, externally private
						CompressedFiles(ZipFile* parent);
						void Add(CompressedFile* compressedFile);
						void Clear();
						void FindMatchingFiles(String* FileSpec, Boolean RecurseSubdirectories, IList* MatchedIndices, Boolean StopAtFirstMatch);
						
						ZipFile* parent;
						ArrayList* colFiles;
				};

				__event CurrentFileEventHandler* CurrentFile;
				__event ProgressEventHandler* FileProgress;
				
				ZipFile();
				void Open();
				void Refresh();
				void Close();

				static ZipFile* Open(String* FileName);
				static ZipFile* Open(String* FileName, String* Password);

				[Browsable(true), Category(S"Zip File Settings"), Description(S"Compressed zip file name."), DefaultValue(S"")]
				__property String* get_FileName();
				__property void set_FileName(String* value);

				[Browsable(true), Category(S"Zip File Settings"), Description(S"Compressed zip file password, if any."), DefaultValue(S"")]
				__property String* get_Password();
				__property void set_Password(String* value);

				[Browsable(true), Category(S"Zip File Settings"), Description(S"Specifies the path used to create temporary zip files when needed.  Leave blank to use source file path."), DefaultValue(S"")]
				__property String* get_TempPath();
				__property void set_TempPath(String* value);

				[Browsable(true), Category(S"Zip File Settings"), Description(S"Automatically refresh compressed file list after each \"Add\", \"Update\" or \"Remove\"."), DefaultValue(true)]
				__property Boolean get_AutoRefresh();
				__property void set_AutoRefresh(Boolean value);

				[Browsable(true), Category(S"Zip File Settings"), Description(S"Set to True to make file name lookups during extractions be case sensitive."), DefaultValue(false)]
				__property Boolean get_CaseSensitive();
				__property void set_CaseSensitive(Boolean value);

				[Browsable(true), Category(S"Zip File Settings"), Description(S"Specifies the compression strength used when adding new compressed files to a zip file.  Note that zip file format doesn't support the Multipass compression setting, use \"CompressFile\" function for this functionality."), DefaultValue(CompressLevel::DefaultCompression)]
				__property CompressLevel get_Strength();
				__property void set_Strength(CompressLevel value);

				[Browsable(false)]
				__property String* get_Comment();

				[Browsable(false)]
				__property Boolean get_IsOpen();

				[Browsable(false)]
				__property CompressedFiles* get_Files();

				void Add(String* FileSpec);
				void Add(String* FileSpec, Boolean RecurseSubdirectories);
				void Add(String* FileSpec, Boolean RecurseSubdirectories, PathInclusion AddPathMethod);

				void Extract(String* FileSpec, String* DestPath, UpdateOption OverwriteWhen);
				void Extract(String* FileSpec, String* DestPath, UpdateOption OverwriteWhen, Boolean RecurseSubdirectories);
				void Extract(String* FileSpec, String* DestPath, UpdateOption OverwriteWhen, Boolean RecurseSubdirectories, PathInclusion CreatePathMethod);

				void Update(String* FileSpec, UpdateOption UpdateWhen);
				void Update(String* FileSpec, UpdateOption UpdateWhen, Boolean AddNewFiles);
				void Update(String* FileSpec, UpdateOption UpdateWhen, Boolean AddNewFiles, Boolean RecurseSubdirectories);
				void Update(String* FileSpec, UpdateOption UpdateWhen, Boolean AddNewFiles, Boolean RecurseSubdirectories, PathInclusion AddPathMethod);

				void Remove(String* FileSpec);
				void Remove(String* FileSpec, Boolean RecurseSubdirectories);

			protected:
				~ZipFile();
											
			public private:	// essentially friend access: internally public, externally private
				void OpenFileForZip();
				void OpenFileForUnzip();
				void CloseFile();
				void AddFilesToZip(String* FileSpec, String* CurrDirectory, int RootPathLength, Boolean RecurseSubdirectories, PathInclusion AddPathMethod, char* Password);
				void UpdateFilesInZip(ZipFile* TempZipFile, String* FileSpec, String* CurrDirectory, int RootPathLength, UpdateOption UpdateWhen, Boolean AddNewFiles, Boolean RecurseSubdirectories, PathInclusion AddPathMethod, char* Password);
				String* GetAdjustedFileName(String* FullFileName, int RootPathLength, PathInclusion AddPathMethod);
				String* GetSearchFileName(String* FileSpec, String* AdjustedFileName, Boolean RecurseSubdirectories);
				ZipFile* CreateTempZipFile();
				void DeleteTempZipFile(ZipFile* TempZipFile);

				static void AddFileToZip(ZipFile* DestZip, ZipFile* EventSource, String* FullFileName, String* AdjustedFileName, char* Password, String* FunctionTitle);
				static void CopyFileInZip(CompressedFile* SourceFile, ZipFile* SourceZip, ZipFile* DestZip, String* FunctionTitle);
				static String* JustPath(String* path);
				static String* JustPath(String* path, String* justFileName);
				static String* AddPathSuffix(String* path);
				static String* FileNameCharPattern;
				static Regex* GetFilePatternRegularExpression(String* FileSpec, Boolean CaseSensitive);
				static String* GetRegexUnicodeChar(__wchar_t Item);

				String* fileName;
				String* password;
				String* comment;
				String* tempPath;
				bool autoRefresh;
				bool caseSensitive;
				CompressLevel strength;
				CompressedFiles* files;
				zipFile hZipFile;
				unzFile hUnzipFile;
		};
	}
}