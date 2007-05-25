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

namespace TVA
{
	namespace IO
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

			public __delegate void CurrentFileEventHandler(String* fullFileName, String* relativeFileName);

			[ToolboxBitmap(__typeof(System::Object), S"TVA.IO.Compression.ZipFile.bmp"), DisplayName(S"ZipFile"), DefaultProperty(S"FileName"), DefaultMember(S"Files"), Description(S"Allows manipulation of compressed files in Zip format")]
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
							__property CompressedFile* get_Item(int index);
							__property CompressedFile* get_Item(String* fileSpec);
							__property CompressedFile* get_Item(String* fileSpec, Boolean recurseSubdirectories);
							__property int get_Count();
							Boolean Find(String* fileSpec);
							Boolean Find(String* fileSpec, Boolean recurseSubdirectories);
							Boolean Find(String* fileSpec, Boolean recurseSubdirectories, Int32* firstMatchedIndex);
							Boolean Find(String* fileSpec, Boolean recurseSubdirectories, IList* matchedIndices);
							IEnumerator* GetEnumerator();

						public private:	// essentially friend access: internally public, externally private
							CompressedFiles(ZipFile* parent);
							void Add(CompressedFile* compressedFile);
							void Clear();
							void FindMatchingFiles(String* fileSpec, Boolean recurseSubdirectories, IList* matchedIndices, Boolean stopAtFirstMatch);
							
							ZipFile* parent;
							ArrayList* colFiles;
					};

					__event CurrentFileEventHandler* CurrentFile;
					__event ProgressEventHandler* FileProgress;
					
					ZipFile();
					void Open();
					void Refresh();
					void Close();

					static ZipFile* Open(String* fileName);
					static ZipFile* Open(String* fileName, String* password);

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

					void Add(String* fileSpec);
					void Add(String* fileSpec, Boolean recurseSubdirectories);
					void Add(String* fileSpec, Boolean recurseSubdirectories, PathInclusion addPathMethod);

					void Extract(String* fileSpec, String* destPath, UpdateOption overwriteWhen);
					void Extract(String* fileSpec, String* destPath, UpdateOption overwriteWhen, Boolean recurseSubdirectories);
					void Extract(String* fileSpec, String* destPath, UpdateOption overwriteWhen, Boolean recurseSubdirectories, PathInclusion createPathMethod);

					void Update(String* fileSpec, UpdateOption updateWhen);
					void Update(String* fileSpec, UpdateOption updateWhen, Boolean addNewFiles);
					void Update(String* fileSpec, UpdateOption updateWhen, Boolean addNewFiles, Boolean recurseSubdirectories);
					void Update(String* fileSpec, UpdateOption updateWhen, Boolean addNewFiles, Boolean recurseSubdirectories, PathInclusion addPathMethod);

					void Remove(String* fileSpec);
					void Remove(String* fileSpec, Boolean recurseSubdirectories);

				protected:
					~ZipFile();
												
				public private:	// essentially friend access: internally public, externally private
					void OpenFileForZip();
					void OpenFileForUnzip();
					void CloseFile();
					void AddFilesToZip(String* fileSpec, String* currDirectory, int rootPathLength, Boolean recurseSubdirectories, PathInclusion addPathMethod, char* password);
					void UpdateFilesInZip(ZipFile* tempZipFile, String* fileSpec, String* currDirectory, int rootPathLength, UpdateOption updateWhen, Boolean addNewFiles, Boolean recurseSubdirectories, PathInclusion addPathMethod, char* password);
					String* GetAdjustedFileName(String* fullFileName, int rootPathLength, PathInclusion addPathMethod);
					String* GetSearchFileName(String* fileSpec, String* adjustedFileName, Boolean recurseSubdirectories);
					ZipFile* CreateTempZipFile();
					void DeleteTempZipFile(ZipFile* tempZipFile);

					static void AddFileToZip(ZipFile* destZip, ZipFile* eventSource, String* fullFileName, String* adjustedFileName, char* password, String* functionTitle);
					static void CopyFileInZip(CompressedFile* sourceFile, ZipFile* sourceZip, ZipFile* destZip, String* functionTitle);
					static String* JustPath(String* path);
					static String* JustPath(String* path, String* justFileName);
					static String* AddPathSuffix(String* path);
					static String* FileNameCharPattern;
					static Regex* GetFilePatternRegularExpression(String* fileSpec, Boolean caseSensitive);
					static String* GetRegexUnicodeChar(__wchar_t item);

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
}