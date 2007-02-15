// JRC 2004 - ZipFile compression class
#include "stdafx.h"
#include "ZipFile.h"

using namespace TVA::Compression;

//
// Compressed file implementation
//
int ZipFile::CompressedFile::get_CompressionMethod()
{
	return fileInfo.compression_method;
}

int ZipFile::CompressedFile::get_CRC()
{
	return fileInfo.crc;
}

int ZipFile::CompressedFile::get_CompressedSize()
{
	return fileInfo.compressed_size;
}

int ZipFile::CompressedFile::get_UncompressedSize()
{
	return fileInfo.uncompressed_size;
}

int ZipFile::CompressedFile::get_DiskNumberStart()
{
	return fileInfo.disk_num_start;
}

int ZipFile::CompressedFile::get_InternalFileAttributes()
{
	return fileInfo.internal_fa;
}

int ZipFile::CompressedFile::get_ExternalFileAttributes()
{
	return fileInfo.external_fa;
}

int ZipFile::CompressedFile::get_DOSDateTime()
{
	return fileInfo.dosDate;
}

DateTime ZipFile::CompressedFile::get_FileDateTime()
{
	if (fileInfo.dosDate)
		return DateTime(fileInfo.tmu_date.tm_year, fileInfo.tmu_date.tm_mon + 1, fileInfo.tmu_date.tm_mday, 
			fileInfo.tmu_date.tm_hour, fileInfo.tmu_date.tm_min, fileInfo.tmu_date.tm_sec);
	else
		return DateTime::MinValue;
}

String* ZipFile::CompressedFile::get_FileName()
{
	return fileName;
}

String* ZipFile::CompressedFile::get_FileComment()
{
	return fileComment;
}

System::Byte ZipFile::CompressedFile::get_ExtraData() []
{
	return extraData;
}

int ZipFile::CompressedFile::get_ZipVersionCreatedBy()
{
	return fileInfo.version;
}

int ZipFile::CompressedFile::get_ZipVersionNeededToExtract()
{
	return fileInfo.version_needed;
}

int ZipFile::CompressedFile::get_Flag()
{
	return fileInfo.flag;
}

ZipFile::CompressedFile::CompressedFile(unzFile file)
{
	// We pin our file info structure so it can be safely passed into unmanaged code...
	unz_file_info __pin * pinFileInfo = &fileInfo;

	// Get compressed file info data structure
	unzGetCurrentFileInfo(file, pinFileInfo, NULL, 0, NULL, 0, NULL, 0);

	// Allocate needed space for variable length data structures
	char* pszFileName = (char*)malloc(fileInfo.size_filename + 1);
	char* pszComment = (char*)malloc(fileInfo.size_file_comment + 1);
	extraData = new System::Byte[fileInfo.size_file_extra];

	// Handle possible unmanaged code errors
	if (pszFileName == NULL || pszComment == NULL) throw new OutOfMemoryException();

	// pin extra data byte array so it can be safely passed into unmanaged code...
	System::Byte __pin * extraDataBuff = NULL;
	if (fileInfo.size_file_extra) extraDataBuff = &extraData[0];

	// Get variable length data structures
	unzGetCurrentFileInfo(file, NULL, pszFileName, fileInfo.size_filename + 1, extraDataBuff, fileInfo.size_file_extra, pszComment, fileInfo.size_file_comment + 1);

	// Convert ASCII strings into .NET Unicode strings
	fileName = CharBufferToString(pszFileName);
	fileComment = CharBufferToString(pszComment);

	free(pszFileName);
	free(pszComment);
}

//
// CompressedFiles collection implementation
//
ZipFile::CompressedFiles::CompressedFiles(ZipFile* parent)
{
	this->parent = parent;
	colFiles = new ArrayList();
}

ZipFile::CompressedFile* ZipFile::CompressedFiles::get_Item(int Index)
{
	if (Index < 0 || Index >= colFiles->get_Count())
		return NULL;
	else
		return static_cast<CompressedFile*>(colFiles->get_Item(Index));
}

// Returns first item from compressed file collection matches file spec, or NULL if not found (does not recurse sub directories)
ZipFile::CompressedFile* ZipFile::CompressedFiles::get_Item(String* FileSpec)
{
	return get_Item(FileSpec, false);
}

// Returns first item from compressed file collection matches file spec, or NULL if not found
ZipFile::CompressedFile* ZipFile::CompressedFiles::get_Item(String* FileSpec, Boolean RecurseSubdirectories)
{
	if (colFiles->get_Count())
	{
		ArrayList* matchedIndices = new ArrayList();

		FindMatchingFiles(FileSpec, RecurseSubdirectories, matchedIndices, true);

		return (matchedIndices->get_Count() ? get_Item(*dynamic_cast<__box int*>(matchedIndices->get_Item(0))) : NULL);
	}
	else
		return NULL;
}

// Returns True if any item in compressed file collection matches file spec (does not recurse sub directories)
Boolean ZipFile::CompressedFiles::Find(String* FileSpec)
{
	return Find(FileSpec, false);
}

// Returns True if any item in compressed file collection matches file spec
Boolean ZipFile::CompressedFiles::Find(String* FileSpec, Boolean RecurseSubdirectories)
{
	return (get_Item(FileSpec, RecurseSubdirectories) == NULL ? false : true);
}

// Returns True if any item in compressed file collection matches file spec, if found index is returned in FirstMatchedIndex parameter otherwise
Boolean ZipFile::CompressedFiles::Find(String* FileSpec, Boolean RecurseSubdirectories, Int32* FirstMatchedIndex)
{
	*FirstMatchedIndex = -1;

	if (colFiles->get_Count())
	{
		ArrayList* matchedIndices = new ArrayList();

		FindMatchingFiles(FileSpec, RecurseSubdirectories, matchedIndices, true);

		if (matchedIndices->get_Count())
			*FirstMatchedIndex = *dynamic_cast<__box int*>(matchedIndices->get_Item(0));			

		return (matchedIndices->get_Count() ? true : false);
	}
	else
		return false;
}

Boolean ZipFile::CompressedFiles::Find(String* FileSpec, Boolean RecurseSubdirectories, IList* MatchedIndices)
{
	if (MatchedIndices == NULL) throw new ArgumentNullException(S"MatchedIndices list was null");
	if (MatchedIndices->get_IsFixedSize()) throw new ArgumentException(S"MatchedIndices list cannot be fixed size - try using an ArrayList");
	if (MatchedIndices->get_Count())
	{
		if (MatchedIndices->get_IsReadOnly()) throw new ArgumentException(S"MatchedIndices list cannot be read only - try using an ArrayList");
		MatchedIndices->Clear();
	}

	if (colFiles->get_Count())
	{
		FindMatchingFiles(FileSpec, RecurseSubdirectories, MatchedIndices, false);
		return (MatchedIndices->get_Count() ? true : false);
	}
	else
		return false;
}

void ZipFile::CompressedFiles::FindMatchingFiles(String* FileSpec, Boolean RecurseSubdirectories, IList* MatchedIndices, Boolean StopAtFirstMatch)
{
	Regex* filePattern = GetFilePatternRegularExpression(FileSpec, parent->get_CaseSensitive());
	int x;

	// Loop through compressed file collection by index
	for (x = 0; x < colFiles->get_Count(); x++)
	{
		if (filePattern->IsMatch(parent->GetSearchFileName(FileSpec, static_cast<CompressedFile*>(colFiles->get_Item(x))->get_FileName(), RecurseSubdirectories)))
		{
			MatchedIndices->Add(__box(x));
			if (StopAtFirstMatch) break;
		}
	}
}

int ZipFile::CompressedFiles::get_Count()
{
	return colFiles->get_Count();
}

IEnumerator* ZipFile::CompressedFiles::GetEnumerator()
{
	return colFiles->GetEnumerator();
}

void ZipFile::CompressedFiles::Add(CompressedFile* compressedFile)
{
	colFiles->Add(compressedFile);
}

void ZipFile::CompressedFiles::Clear()
{
	colFiles->Clear();
}

//
// ZipFile component implementation
//
ZipFile::ZipFile()
{
	fileName = S"";
	password = S"";
	comment = S"";
	tempPath = S"";
	autoRefresh = true;
	caseSensitive = false;
	strength = CompressLevel::DefaultCompression;
	files = new CompressedFiles(this);
	hZipFile = NULL;
	hUnzipFile = NULL;
}

ZipFile::~ZipFile()
{
	Close();
}

// Opens zip file, creating it if it doesn't exist
void ZipFile::Open()
{
	if (!FileName->get_Length()) throw new CompressionException(S"Cannot open Zip file: file name was not specified");

	if (File::Exists(FileName))
		OpenFileForUnzip();
	else
		OpenFileForZip();
}

// Reload zip file's compressed file list
void ZipFile::Refresh()
{
	// We just reopen an existing file to refresh it...
	Close();
	Open();
}

// Closes zip file if it was open
void ZipFile::Close()
{
	CloseFile();
}

// Creates new zip file object for given zip file name, then opens zip file
ZipFile* ZipFile::Open(String* FileName)
{
	ZipFile* zipFile = new ZipFile();

	zipFile->FileName = FileName;
	zipFile->Open();

	return zipFile;
}

// Creates new zip file object for given zip file name and password, then opens zip file
ZipFile* ZipFile::Open(String* FileName, String* Password)
{
	ZipFile* zipFile = new ZipFile();

	zipFile->FileName = FileName;
	zipFile->Password = Password;
	zipFile->Open();

	return zipFile;
}

// Zip file name property
String* ZipFile::get_FileName()
{
	return fileName;
}

void ZipFile::set_FileName(String* value)
{
	fileName = value;
}

// Zip file password property
String* ZipFile::get_Password()
{
	return password;
}

void ZipFile::set_Password(String* value)
{
	password = value;
}

// Zip file temp path property
String* ZipFile::get_TempPath()
{
	return tempPath;
}

void ZipFile::set_TempPath(String* value)
{
	tempPath = JustPath(value);
}

// AutoRefresh property
Boolean ZipFile::get_AutoRefresh()
{
	return autoRefresh;
}

void ZipFile::set_AutoRefresh(Boolean value)
{
	autoRefresh = value;
}

// CaseSensitive property
Boolean ZipFile::get_CaseSensitive()
{
	return caseSensitive;
}

void ZipFile::set_CaseSensitive(Boolean value)
{
	caseSensitive = value;
}

// Compression strength property
CompressLevel ZipFile::get_Strength()
{
	return strength;
}

void ZipFile::set_Strength(CompressLevel value)
{
	if (value == CompressLevel::MultiPass)
		strength = CompressLevel::DefaultCompression;
	else
		strength = value;
}

// Compressed file collection
ZipFile::CompressedFiles* ZipFile::get_Files()
{
	return files;
}

// Zip file comment
String* ZipFile::get_Comment()
{
	return comment;
}

// Returns True if Zip file is open
Boolean ZipFile::get_IsOpen()
{
	return (hUnzipFile || hZipFile);
}

// Adds specified files to zip (does not recurse sub directories)
void ZipFile::Add(String* FileSpec)
{
	Add(FileSpec, false);
}

// Adds specified files to zip (adds relative paths if RecurseSubdirectories is True)
void ZipFile::Add(String* FileSpec, Boolean RecurseSubdirectories)
{
	Add(FileSpec, RecurseSubdirectories, PathInclusion::RelativePath);
}

// Adds specified files to zip
void ZipFile::Add(String* FileSpec, Boolean RecurseSubdirectories, PathInclusion AddPathMethod)
{	
	char* pszPassword = NULL;
	OpenFileForZip();

	// Just wrapping in a try/catch/finally to make sure unmanaged code allocations always get released
	try
	{
		// Get ANSI password, if one was provided
		if (password)
			if (password->get_Length())
				pszPassword = StringToCharBuffer(password);

		String* fileSpec = Path::GetFileName(FileSpec);
		String* rootPath = JustPath(FileSpec, fileSpec);

		AddFilesToZip(fileSpec, rootPath, rootPath->get_Length(), RecurseSubdirectories, AddPathMethod, pszPassword);
	}
	catch (CompressionException* ex)		
	{
		// We just rethrow any errors back to user
		throw ex;
	}
	catch (Exception* ex)
	{
		throw ex;
	}
	__finally
	{
		if (pszPassword)
			free(pszPassword);
	}

	// We'll reload the file list after add if requested...
	if (autoRefresh) Refresh();
}

// This internal function adds each file that matches file specification, then does this for each subdirectory if requested
void ZipFile::AddFilesToZip(String* FileSpec, String* CurrDirectory, int RootPathLength, Boolean RecurseSubdirectories, PathInclusion AddPathMethod, char* Password)
{
	IEnumerator* filesEnum = Directory::GetFiles(CurrDirectory, FileSpec)->GetEnumerator();
	String* fullFileName;
	String* adjustedFileName;

	while (filesEnum->MoveNext())
	{
		fullFileName = filesEnum->Current->ToString();

		adjustedFileName = GetAdjustedFileName(fullFileName, RootPathLength, AddPathMethod);

		if (files->Find(adjustedFileName, false)) throw new CompressionException(String::Concat(S"Failed to add file \"",  fullFileName, "\" to zip, compressed file with this same name already exists in zip file.  Try using \"Update\" instead."));

		AddFileToZip(this, NULL, fullFileName, adjustedFileName, Password, S"Add Zip File");
	}

	if (RecurseSubdirectories)
	{
		IEnumerator* dirsEnum = Directory::GetDirectories(CurrDirectory, "*")->GetEnumerator();

		while (dirsEnum->MoveNext())
			AddFilesToZip(FileSpec, dirsEnum->Current->ToString(), RootPathLength, RecurseSubdirectories, AddPathMethod, Password);
	}
}

// Extracts specified files from zip (creates relative path, does not recurse sub directories)
void ZipFile::Extract(String* FileSpec, String* DestPath, UpdateOption OverwriteWhen)
{
	Extract(FileSpec, DestPath, OverwriteWhen, false);
}

// Extracts specified files from zip (creates relative path)
void ZipFile::Extract(String* FileSpec, String* DestPath, UpdateOption OverwriteWhen, Boolean RecurseSubdirectories)
{
	Extract(FileSpec, DestPath, OverwriteWhen, RecurseSubdirectories, PathInclusion::RelativePath);
}

// Extracts specified files from zip
void ZipFile::Extract(String* FileSpec, String* DestPath, UpdateOption OverwriteWhen, Boolean RecurseSubdirectories, PathInclusion CreatePathMethod)
{
	// Any file spec in destination path will be ignored
	DestPath = JustPath(DestPath);

	char* pszPassword = NULL;
	char* pszFileName = NULL;
	OpenFileForUnzip();

	// Just wrapping in a try/catch/finally to make sure unmanaged code allocations always get released
	try
	{
		Regex* filePattern = GetFilePatternRegularExpression(FileSpec, caseSensitive);
		IEnumerator* filesEnum = files->GetEnumerator();
		CompressedFile* file;
		String* sourceFileName;
		String* destFileName;
		bool writeFile;
 		int err;

		// Get ANSI password, if one was provided
		if (password)
			if (password->get_Length())
				pszPassword = StringToCharBuffer(password);

		// Loop through compressed file collection
		while (filesEnum->MoveNext())
		{
			file = static_cast<CompressedFile*>(filesEnum->Current);
			sourceFileName = file->get_FileName();

			if (filePattern->IsMatch(GetSearchFileName(FileSpec, sourceFileName, RecurseSubdirectories)))
			{
				pszFileName = StringToCharBuffer(sourceFileName);
				err = unzLocateFile(hUnzipFile, pszFileName, (caseSensitive ? 1 : 2));
				free(pszFileName);
				pszFileName = NULL;

				// We should find file in zip file if it was in our compressed file collection
				if (err != Z_OK) throw new CompressionException(String::Concat(S"Extract Zip File Error: Compressed file \"", sourceFileName, "\" cannot be found in zip file!"));

				// Open compressed file for unzipping
				if (pszPassword)
					err = unzOpenCurrentFilePassword(hUnzipFile, pszPassword);
				else
					err = unzOpenCurrentFile(hUnzipFile);

				if (err != Z_OK) throw new CompressionException(S"Extract Zip File", err);

				// Get full destination file name
				switch (CreatePathMethod)
				{
					case PathInclusion::FullPath:
						destFileName = sourceFileName;
						break;
					case PathInclusion::NoPath:
						destFileName = String::Concat(DestPath, Path::GetFileName(sourceFileName));
						break;
					case PathInclusion::RelativePath:
						destFileName = String::Concat(DestPath, sourceFileName);
						break;
				}

				// Make sure destination directory exists
				Directory::CreateDirectory(JustPath(destFileName));

				// See if destination file already exists
				if (File::Exists(destFileName))
				{
					DateTime lastUpdate = File::GetLastWriteTime(destFileName);

					switch (OverwriteWhen)
					{
						case UpdateOption::Never:
							writeFile = false;
							break;
						case UpdateOption::Always:
							writeFile = true;
							break;
						case UpdateOption::ZipFileIsNewer:
							writeFile = (DateTime::Compare(file->get_FileDateTime(), lastUpdate) > 0);
							break;
						case UpdateOption::DiskFileIsNewer:
							writeFile = (DateTime::Compare(file->get_FileDateTime(), lastUpdate) < 0);
							break;
						default:
							writeFile = false;
							break;
					}
				}
				else
					writeFile = true;

				if (writeFile)
				{
					System::Byte buffer[] = new System::Byte[BufferSize];
					System::Byte __pin * destBuff = &buffer[0];	// pin buffer so it can be safely passed into unmanaged code...
					FileStream* fileStream = File::Create(destFileName);
					int read;
					__int64 total = 0, len = -1;

					// Send initial progress event
					len = file->get_UncompressedSize();
					CurrentFile(destFileName, sourceFileName);
					FileProgress(0, len);					

					// Read initial buffer
					read = unzReadCurrentFile(hUnzipFile, destBuff, buffer->get_Length());
					if (read < 0) throw new CompressionException(S"Extract Zip File", read);

					while (read)
					{
						// Write data to file stream,
						fileStream->Write(buffer, 0, read);

						// Raise progress event
						total += read;
						FileProgress(total, len);

						// Read next buffer from source file stream
						read = unzReadCurrentFile(hUnzipFile, destBuff, buffer->get_Length());
						if (read < 0) throw new CompressionException(S"Extract Zip File", read);
					}

					fileStream->Close();
				}

				// Close compressed file
				unzCloseCurrentFile(hUnzipFile);
			}
		}
	}
	catch (CompressionException* ex)		
	{
		// We just rethrow any errors back to user
		throw ex;
	}
	catch (Exception* ex)
	{
		throw ex;
	}
	__finally
	{
		if (pszPassword)
			free(pszPassword);

		if (pszFileName)
			free(pszFileName);
	}
}

// Updates specified files in zip (does not add new files, does not recurse sub directories)
void ZipFile::Update(String* FileSpec, UpdateOption UpdateWhen)
{
	Update(FileSpec, UpdateWhen, false);
}

// Updates specified files in zip (does not recurse sub directories)
void ZipFile::Update(String* FileSpec, UpdateOption UpdateWhen, Boolean AddNewFiles)
{
	Update(FileSpec, UpdateWhen, AddNewFiles, false);
}

// Updates specified files in zip (adds relative paths if RecurseSubdirectories is True)
void ZipFile::Update(String* FileSpec, UpdateOption UpdateWhen, Boolean AddNewFiles, Boolean RecurseSubdirectories)
{
	Update(FileSpec, UpdateWhen, AddNewFiles, RecurseSubdirectories, PathInclusion::RelativePath);
}

// Updates specified files in zip
void ZipFile::Update(String* FileSpec, UpdateOption UpdateWhen, Boolean AddNewFiles, Boolean RecurseSubdirectories, PathInclusion AddPathMethod)
{
	// If destination zip file doesn't exist, we just perform an Add
	if (!File::Exists(fileName))
	{
		if (AddNewFiles) Add(FileSpec, RecurseSubdirectories, AddPathMethod);
		return;
	}

	char* pszPassword = NULL;
	ZipFile* tempZipFile;
	OpenFileForUnzip();

	// Just wrapping in a try/catch/finally to make sure unmanaged code allocations always get released and temp zip file gets removed
	try
	{
		// Create temporary zip file to hold update results
		tempZipFile = CreateTempZipFile();

		// Get ANSI password, if one was provided
		if (password)
			if (password->get_Length())
				pszPassword = StringToCharBuffer(password);

		String* fileSpec = Path::GetFileName(FileSpec);
		String* rootPath = JustPath(FileSpec, fileSpec);

		UpdateFilesInZip(tempZipFile, fileSpec, rootPath, rootPath->get_Length(), UpdateWhen, AddNewFiles, RecurseSubdirectories, AddPathMethod, pszPassword);

		// Now make the temp file the new zip file
		Close();
		tempZipFile->Close();
		File::Delete(fileName);
		File::Move(tempZipFile->fileName, fileName);
	}
	catch (CompressionException* ex)		
	{
		// We just rethrow any errors back to user
		throw ex;
	}
	catch (Exception* ex)
	{
		throw ex;
	}
	__finally
	{
		if (pszPassword)
			free(pszPassword);

		// If everything went well, temp zip will no longer exist, but just in case we tidy up and delete the temp file...
		DeleteTempZipFile(tempZipFile);
	}

	// We'll reload the file list after update if requested...
	if (autoRefresh) Refresh();
}

// This internal function copies each file into a temporary zip that matches file specification, then does this for each subdirectory if requested
void ZipFile::UpdateFilesInZip(ZipFile* TempZipFile, String* FileSpec, String* CurrDirectory, int RootPathLength, UpdateOption UpdateWhen, Boolean AddNewFiles, Boolean RecurseSubdirectories, PathInclusion AddPathMethod, char* Password)
{
	IEnumerator* filesEnum = Directory::GetFiles(CurrDirectory, FileSpec)->GetEnumerator();
	String* fullFileName;
	String* adjustedFileName;
	CompressedFile* file;
	DateTime lastUpdate;
	bool updateFile;

	while (filesEnum->MoveNext())
	{
		fullFileName = filesEnum->Current->ToString();
		adjustedFileName = GetAdjustedFileName(fullFileName, RootPathLength, AddPathMethod);

		if (file = files->get_Item(adjustedFileName))
		{
			lastUpdate = File::GetLastWriteTime(fullFileName);

			switch (UpdateWhen)
			{
				case UpdateOption::Never:
					updateFile = false;
					break;
				case UpdateOption::Always:
					updateFile = true;
					break;
				case UpdateOption::ZipFileIsNewer:
					updateFile = (DateTime::Compare(file->get_FileDateTime(), lastUpdate) > 0);
					break;
				case UpdateOption::DiskFileIsNewer:
					updateFile = (DateTime::Compare(file->get_FileDateTime(), lastUpdate) < 0);
					break;
				default:
					updateFile = false;
					break;
			}

			if (updateFile)
			{
				// Need to update compressed file from disk, so we add the it the new temporary archive
				AddFileToZip(TempZipFile, this, fullFileName, adjustedFileName, Password, S"Update Zip File");
			}
			else
			{
				// Otherwise we just copy the compressed file from the original archive
				CopyFileInZip(file, this, TempZipFile, S"Update Zip File");
			}
		}
		else if (AddNewFiles)
		{
			// This was a new file so we just add it the new zip, if requested...
			AddFileToZip(TempZipFile, this, fullFileName, adjustedFileName, Password, S"Update Zip File");
		}
	}

	if (RecurseSubdirectories)
	{
		IEnumerator* dirsEnum = Directory::GetDirectories(CurrDirectory, "*")->GetEnumerator();

		while (dirsEnum->MoveNext())
			UpdateFilesInZip(TempZipFile, FileSpec, dirsEnum->Current->ToString(), RootPathLength, UpdateWhen, AddNewFiles, RecurseSubdirectories, AddPathMethod, Password);
	}
}

// Removes specified files from zip (does not recurse sub directories)
void ZipFile::Remove(String* FileSpec)
{
	Remove(FileSpec, false);
}

// Removes specified files from zip
void ZipFile::Remove(String* FileSpec, Boolean RecurseSubdirectories)
{
	ZipFile* tempZipFile;
	OpenFileForUnzip();

	// Just wrapping in a try/catch/finally to make sure temp zip file gets removed
	try
	{
		Regex* filePattern = GetFilePatternRegularExpression(FileSpec, caseSensitive);
		IEnumerator* filesEnum = files->GetEnumerator();
		CompressedFile* file;

		// Create temporary zip file to hold remove results
		tempZipFile = CreateTempZipFile();

		// Loop through compressed file collection
		while (filesEnum->MoveNext())
		{
			file = static_cast<CompressedFile*>(filesEnum->Current);

			// We copy all files into destination zip file except those user requested to be removed...
			if (!filePattern->IsMatch(GetSearchFileName(FileSpec, file->get_FileName(), RecurseSubdirectories)))
				CopyFileInZip(file, this, tempZipFile, S"Remove Zip File");
		}

		// Now make the temp file the new zip file
		Close();
		tempZipFile->Close();
		File::Delete(fileName);
		File::Move(tempZipFile->fileName, fileName);
	}
	catch (CompressionException* ex)		
	{
		// We just rethrow any errors back to user
		throw ex;
	}
	catch (Exception* ex)
	{
		throw ex;
	}
	__finally
	{
		// If everything went well, temp zip will no longer exist, but just in case we tidy up and delete the temp file...
		DeleteTempZipFile(tempZipFile);
	}

	// We'll reload the file list after remove if requested...
	if (autoRefresh) Refresh();
}

void ZipFile::CloseFile()
{
	// We clear the compressedfile collection any time we close the archive
	files->Clear();

	if (hUnzipFile) unzClose(hUnzipFile);
	hUnzipFile = NULL;

	if (hZipFile) zipClose(hZipFile, NULL);
	hZipFile = NULL;
}

void ZipFile::OpenFileForZip()
{
	if (!hZipFile)
	{
		bool zipExists;
		char* pszFileName = NULL;
		
		// Just wrapping in a try/catch/finally to make sure unmanaged code allocations always get released
		try
		{						
			CloseFile();

			zipExists = File::Exists(fileName);
			pszFileName = StringToCharBuffer(fileName);
			hZipFile = zipOpen(pszFileName, (zipExists ? APPEND_STATUS_ADDINZIP : APPEND_STATUS_CREATE));
			
			free(pszFileName);
			pszFileName = NULL;

			if (!hZipFile)
			{
				if (zipExists)
					throw new CompressionException(S"Failed to open Zip file");
				else
					throw new CompressionException(S"Failed to create Zip file");
			}
		}
		catch (CompressionException* ex)		
		{
			// We just rethrow any errors back to user
			throw ex;
		}
		catch (Exception* ex)
		{
			throw ex;
		}
		__finally
		{
			if (pszFileName)
				free(pszFileName);
		}
	}
}

void ZipFile::OpenFileForUnzip()
{
	if (!hUnzipFile)
	{
		int err;
		char* pszFileName = NULL;
		char* pszComment = NULL;

		// Just wrapping in a try/catch/finally to make sure unmanaged code allocations always get released
		try
		{
			CloseFile();

			pszFileName = StringToCharBuffer(fileName);			
			hUnzipFile = unzOpen(pszFileName);
			
			free(pszFileName);
			pszFileName = NULL;

			if (hUnzipFile)
			{
				unz_global_info globalInfo;

				// Load global comment
				err = unzGetGlobalInfo(hUnzipFile, &globalInfo);
				if (err != Z_OK) throw new CompressionException(S"Zip File Read", err);

				// Load zip file comment if it exists
				if (globalInfo.size_comment)
				{
					pszComment = (char*)malloc(globalInfo.size_comment + 1);
					
					// Handle possible unmanaged code errors
					if (pszComment == NULL) throw new OutOfMemoryException();

					err = unzGetGlobalComment(hUnzipFile, pszComment, globalInfo.size_comment + 1);
					if (err != Z_OK) throw new CompressionException(S"Zip File Read", err);

					// Convert ASCII string into .NET Unicode string
					comment = CharBufferToString(pszComment);

					free(pszComment);
					pszComment = NULL;
				}

				// Load in compressed file list for existing zip file
				files->Clear();
				int result = unzGoToFirstFile(hUnzipFile);

				while (result == Z_OK)
				{
					files->Add(new CompressedFile(hUnzipFile));
					result = unzGoToNextFile(hUnzipFile);
				}
			}
			else
				throw new CompressionException(S"Failed to open Zip file");
		}
		catch (CompressionException* ex)		
		{
			// We just rethrow any errors back to user
			throw ex;
		}
		catch (Exception* ex)
		{
			throw ex;
		}
		__finally
		{
			if (pszFileName)
				free(pszFileName);

			if (pszComment)
				free(pszComment);
		}
	}
}

// This internal function returns the type of path the user wants stored in the zip file
String* ZipFile::GetAdjustedFileName(String* FullFileName, int RootPathLength, PathInclusion AddPathMethod)
{
	String* adjustedFileName;

	switch (AddPathMethod)
	{
		case PathInclusion::FullPath:
			adjustedFileName = FullFileName;
			break;
		case PathInclusion::NoPath:
			adjustedFileName = Path::GetFileName(FullFileName);
			break;
		case PathInclusion::RelativePath:
			// For relative paths, we just return the full path with the root path removed
			adjustedFileName = FullFileName->Substring(RootPathLength);
			break;
	}

	return adjustedFileName;
}

// This internal function returns the matchable file name to search for in an archive
String* ZipFile::GetSearchFileName(String* FileSpec, String* AdjustedFileName, Boolean RecurseSubdirectories)
{
	if (RecurseSubdirectories)
	{
		if (FileSpec->get_Length() == Path::GetFileName(FileSpec)->get_Length())
		{
			// No path in filespec and recursing subdirectories, pattern match against filename portion only
			return Path::GetFileName(AdjustedFileName);
		}
		else
		{
			// Path specified in filespec, so user wants to match from specified folder down - so if file
			// is rooted in that folder, pattern match against root path + filename portion only
			String* rootPath = JustPath(FileSpec);

			if (String::Compare(rootPath, 0, AdjustedFileName, 0, rootPath->get_Length(), !caseSensitive) == 0)
				return String::Concat(rootPath, Path::GetFileName(AdjustedFileName));
			else
				return AdjustedFileName;
		}
	}
	else
	{
		// If we're not recursing subdirectories, do an exact pattern match against filespec
		return AdjustedFileName;
	}
}

ZipFile* ZipFile::CreateTempZipFile()
{
	String* destPath;

	if (tempPath)
		if (tempPath->get_Length())
			if (Directory::Exists(tempPath))
				destPath = tempPath;

	if (!destPath) destPath = JustPath(fileName);

	return ZipFile::Open(String::Concat(destPath, Guid::NewGuid().ToString(), ".tmp"), password);
}

void ZipFile::DeleteTempZipFile(ZipFile* TempZipFile)
{
	if (TempZipFile)
	{
		TempZipFile->Close();
		if (File::Exists(TempZipFile->get_FileName()))
			File::Delete(TempZipFile->get_FileName());
	}
}

// This internal function handles getting a file into the zip file
void ZipFile::AddFileToZip(ZipFile* DestZip, ZipFile* EventSource, String* FullFileName, String* AdjustedFileName, char* Password, String* FunctionTitle)
{
	int err;
	unsigned long crc = 0;
	char* adjustedFileName = NULL;

	// Just wrapping in a try/catch/finally to make sure unmanaged code allocations always get released
	try
	{	
		if (Password) crc = (unsigned long)Common::CRC32(FullFileName);
		if (!EventSource) EventSource = DestZip;

		// Define zip file date/time to be stored
		DateTime fileDate = File::GetLastWriteTime(FullFileName);
		
		// Create the compressed file information to be stored in zip
		zip_fileinfo fileInfo;
		fileInfo.dosDate = 0;
		fileInfo.tmz_date.tm_year = fileDate.Year;
		fileInfo.tmz_date.tm_mon = fileDate.Month - 1;
		fileInfo.tmz_date.tm_mday = fileDate.Day;
		fileInfo.tmz_date.tm_hour = fileDate.Hour;
		fileInfo.tmz_date.tm_min = fileDate.Minute;
		fileInfo.tmz_date.tm_sec = fileDate.Second;
		fileInfo.internal_fa = 0;
		fileInfo.external_fa = 0;

		// Get ANSI file name to store in zip file
		adjustedFileName = StringToCharBuffer(AdjustedFileName);

		// Open a new compressed file stream in zip file
		err = zipOpenNewFileInZip3(DestZip->hZipFile, adjustedFileName, &fileInfo, NULL, 0, NULL, 0, NULL, (DestZip->strength == CompressLevel::NoCompression ? 0 : Z_DEFLATED), DestZip->strength, 0, -MAX_WBITS, DEF_MEM_LEVEL, Z_DEFAULT_STRATEGY, Password, crc);
		if (err != Z_OK) throw new CompressionException(FunctionTitle, err);
		
		free(adjustedFileName);
		adjustedFileName = NULL;

		// Stream file data into zip file
		System::Byte buffer[] = new System::Byte[BufferSize];		
		System::Byte __pin * sourceBuff = &buffer[0];	// pin buffer so it can be safely passed into unmanaged code...
		FileStream* fileStream = File::Open(FullFileName, FileMode::Open, FileAccess::Read, FileShare::Read);
		int read;
		__int64 total = 0, len = -1;

		// Send initial progress event
		try
		{
			if (fileStream->get_CanSeek())
				len = fileStream->get_Length();
		}
		catch (Exception*)
		{
			len = -1;
		}

		EventSource->CurrentFile(FullFileName, AdjustedFileName);
		EventSource->FileProgress(0, len);

		// Read initial buffer
		read = fileStream->Read(buffer, 0, BufferSize);

		while (read)
		{
			// Add buffer to zip file
			err = zipWriteInFileInZip(DestZip->hZipFile, sourceBuff, read);
			if (err != Z_OK) throw new CompressionException(FunctionTitle, err);

			// Raise progress event
			total += read;
			EventSource->FileProgress(total, len);

			// Read next buffer from source file stream
			read = fileStream->Read(buffer, 0, BufferSize);
		}

		fileStream->Close();

		err = zipCloseFileInZip(DestZip->hZipFile);
		if (err != Z_OK) throw new CompressionException(FunctionTitle, err);
	}
	catch (CompressionException* ex)		
	{
		// We just rethrow any errors back to user
		throw ex;
	}
	catch (Exception* ex)
	{
		throw ex;
	}
	__finally
	{
		if (adjustedFileName)
			free(adjustedFileName);
	}
}

// This internal function streams a compressed file out of one zip file and into another
void ZipFile::CopyFileInZip(CompressedFile* SourceFile, ZipFile* SourceZip, ZipFile* DestZip, String* FunctionTitle)
{
	char* pszPassword = NULL;
	char* pszFileName = NULL;

	// Just wrapping in a try/catch/finally to make sure unmanaged code allocations always get released
	try
	{
 		int err;

		// Get ANSI password, if one was provided
		if (SourceZip->password)
			if (SourceZip->password->get_Length())
				pszPassword = StringToCharBuffer(SourceZip->password);

		pszFileName = StringToCharBuffer(SourceFile->get_FileName());
		err = unzLocateFile(SourceZip->hUnzipFile, pszFileName, (SourceZip->caseSensitive ? 1 : 2));		

		// We should find file in zip file if it was in our compressed file collection
		if (err != Z_OK) throw new CompressionException(String::Concat(FunctionTitle, S" Error: Compressed file \"", SourceFile->get_FileName(), "\" cannot be found in zip file!"));

		// Open compressed file for unzipping
		if (pszPassword)
			err = unzOpenCurrentFilePassword(SourceZip->hUnzipFile, pszPassword);
		else
			err = unzOpenCurrentFile(SourceZip->hUnzipFile);

		if (err != Z_OK) throw new CompressionException(FunctionTitle, err);

		System::Byte buffer[] = new System::Byte[BufferSize];
		System::Byte __pin * destBuff = &buffer[0];	// pin buffer so it can be safely passed into unmanaged code...
		zip_fileinfo fileInfo;
		int read;
		__int64 total = 0, len = -1;

		// Create the compressed file information to be stored in zip
		fileInfo.dosDate = 0;
		fileInfo.tmz_date.tm_year = SourceFile->fileInfo.tmu_date.tm_year;
		fileInfo.tmz_date.tm_mon = SourceFile->fileInfo.tmu_date.tm_mon;
		fileInfo.tmz_date.tm_mday = SourceFile->fileInfo.tmu_date.tm_mday;
		fileInfo.tmz_date.tm_hour = SourceFile->fileInfo.tmu_date.tm_hour;
		fileInfo.tmz_date.tm_min = SourceFile->fileInfo.tmu_date.tm_min;
		fileInfo.tmz_date.tm_sec = SourceFile->fileInfo.tmu_date.tm_sec;
		fileInfo.internal_fa = SourceFile->fileInfo.internal_fa;
		fileInfo.external_fa = SourceFile->fileInfo.external_fa;

		// Create new compressed file in destination zip file
		err = zipOpenNewFileInZip3(DestZip->hZipFile, pszFileName, &fileInfo, NULL, 0, NULL, 0, NULL, (SourceZip->strength == CompressLevel::NoCompression ? 0 : Z_DEFLATED), SourceZip->strength, 0, -MAX_WBITS, DEF_MEM_LEVEL, Z_DEFAULT_STRATEGY, pszPassword, SourceFile->get_CRC());
		if (err != Z_OK) throw new CompressionException(FunctionTitle, err);

		free(pszFileName);
		pszFileName = NULL;

		// Send initial progress event
		SourceZip->CurrentFile(SourceFile->get_FileName(), SourceFile->get_FileName());
		SourceZip->FileProgress(0, SourceFile->get_UncompressedSize());

		// Read initial buffer
		read = unzReadCurrentFile(SourceZip->hUnzipFile, destBuff, buffer->get_Length());
		if (read < 0) throw new CompressionException(FunctionTitle, read);

		while (read)
		{
			// Write data to destination zip file
			err = zipWriteInFileInZip(DestZip->hZipFile, destBuff, read);
			if (err != Z_OK) throw new CompressionException(FunctionTitle, err);

			// Raise progress event
			total += read;
			SourceZip->FileProgress(total, SourceFile->get_UncompressedSize());

			// Read next buffer from source file stream
			read = unzReadCurrentFile(SourceZip->hUnzipFile, destBuff, buffer->get_Length());
			if (read < 0) throw new CompressionException(FunctionTitle, read);
		}

		// Close compressed file
		unzCloseCurrentFile(SourceZip->hUnzipFile);

		err = zipCloseFileInZip(DestZip->hZipFile);
		if (err != Z_OK) throw new CompressionException(FunctionTitle, err);
	}
	catch (CompressionException* ex)		
	{
		// We just rethrow any errors back to user
		throw ex;
	}
	catch (Exception* ex)
	{
		throw ex;
	}
	__finally
	{
		if (pszPassword)
			free(pszPassword);

		if (pszFileName)
			free(pszFileName);
	}
}

// Returns just the path without a filename from a path
String* ZipFile::JustPath(String* path)
{
	return JustPath(path, Path::GetFileName(path));
}

// Returns just the path without a filename from a path when you already know
// the just file name in that path - this just saves cycles if you've
// already called Path::GetFileName function...
String* ZipFile::JustPath(String* path, String* justFileName)
{
	// Find the file portion of the path and return what's left
	if (path->get_Length() > 0 && path->get_Length() > justFileName->get_Length())
		return AddPathSuffix(path->Substring(0, path->get_Length() - justFileName->get_Length()));
	else
		return S"";
}

// Makes sure path is suffixed with standard directory separator
String* ZipFile::AddPathSuffix(String* path)
{
	if (path->get_Length())
	{
		__wchar_t suffix = path->Chars[path->get_Length() - 1];
		if (suffix != Path::DirectorySeparatorChar && suffix != Path::AltDirectorySeparatorChar)
			path = String::Concat(path, Path::DirectorySeparatorChar.ToString());
	}
	else
		path = Path::DirectorySeparatorChar.ToString();

	return path;
}

Regex* ZipFile::GetFilePatternRegularExpression(String* FileSpec, Boolean CaseSensitive)
{
	String* FilePattern;
	StringBuilder* sb = new StringBuilder();

	// FileNameCharPattern exists as a static class member so it only has to be defined once
	if (!FileNameCharPattern)
	{
		// Define a regular expression pattern for a valid file name character, we do this by
		// allowing any characters except those that would not be valid as part of a filename,
		// this essentially builds the "?" wildcard pattern match
		sb->Append(S"[^");
		sb->Append(GetRegexUnicodeChar(Path::DirectorySeparatorChar));
		sb->Append(GetRegexUnicodeChar(Path::AltDirectorySeparatorChar));
		sb->Append(GetRegexUnicodeChar(Path::PathSeparator));
		sb->Append(GetRegexUnicodeChar(Path::VolumeSeparatorChar));

		for (int x = 0; x < Path::InvalidPathChars->get_Length(); x++)
			sb->Append(GetRegexUnicodeChar(Path::InvalidPathChars[x]));

		sb->Append(S"]");
		FileNameCharPattern = sb->ToString();
	}

	FilePattern = FileSpec->Replace(S"\\", S"\\u005C");		// Backslash in Regex means special sequence, but here we really want a backslash
	FilePattern = FilePattern->Replace(S".", S"\\u002E");	// Dot in Regex means any character, but here we really want a dot
	FilePattern = FilePattern->Replace(S"?", FileNameCharPattern);
	FilePattern = String::Concat(S"^", FilePattern->Replace(S"*", String::Concat(S"(", FileNameCharPattern, S")*")), S"$");

	return new Regex(FilePattern, (CaseSensitive ? RegexOptions::None : RegexOptions::IgnoreCase));
}

String* ZipFile::GetRegexUnicodeChar(__wchar_t Item)
{
	return String::Concat(S"\\u", ((Int32)(Item)).ToString(S"x")->PadLeft(4, '0'));
}