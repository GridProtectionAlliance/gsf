// JRC 2004 - Common compression functions
#include "stdafx.h"
#include "Common.h"
#include "zlib\\zlib.h"
#include "zlib\\zip.h"
#include "zlib\\unzip.h"

using namespace TVA::Compression;

// Compress a byte array using default compression strength
System::Byte Common::Compress(System::Byte Data[]) []
{
	return Compress(Data, CompressLevel::DefaultCompression);
}

// Compress a byte array using specified compression strength
System::Byte Common::Compress(System::Byte Data[], CompressLevel Strength) []
{
	return Compress(Data, Strength, 0);
}

// When user requests multi-pass compression, we allow multiple compression passes on a buffer because
// this can often produce better compression results
System::Byte Common::Compress(System::Byte Data[], CompressLevel Strength, int CompressionDepth) []
{
	// zlib requests destination buffer to be 0.1% and 12 bytes larger than source stream...
	int err;
	unsigned long sourceBuffLen = Data->get_Count(), destBuffLen = sourceBuffLen + (unsigned long)(sourceBuffLen * 0.001) + 12;
	System::Byte Buffer[] = new System::Byte[destBuffLen];

	// pin byte arrays so they can be safely passed into unmanaged code...
	System::Byte __pin * sourceBuff = &Data[0];
	System::Byte __pin * destBuff = &Buffer[0];

	// Compress buffer using zlib
    err = compress2(destBuff, &destBuffLen, (const unsigned char *)sourceBuff, sourceBuffLen, __min(Strength, Z_BEST_COMPRESSION));
	if (err != Z_OK) throw new CompressionException(S"Compression", err);

	// Extract only used part of compressed buffer
	System::Byte outBuffer[] = new System::Byte[++destBuffLen];	
	outBuffer[0] = (System::Byte)CompressionDepth;
	for (unsigned long x = 1; x < destBuffLen; x++)
		outBuffer[x] = Buffer[x - 1];

	if (Strength == CompressLevel::MultiPass && outBuffer->get_Length() < Data->get_Length() && CompressionDepth < 255)
	{
		// See if another pass would help the compression...
		System::Byte testBuffer[] = Compress(outBuffer, Strength, CompressionDepth + 1);

		if (testBuffer->get_Length() < outBuffer->get_Length())
			return testBuffer;
		else
			return outBuffer;
	}
	else
		return outBuffer;
}

// Compress a stream using default compression strength
Stream* Common::Compress(Stream* InStream)
{
	return Compress(InStream, CompressLevel::DefaultCompression);
}

// Compress a stream using specified compression strength
Stream* Common::Compress(Stream* InStream, CompressLevel Strength)
{
	MemoryStream* outStream = new MemoryStream();
	Compress(InStream, outStream, Strength, NULL);
	return outStream;
}

// Compress a stream onto given output stream using specified compression strength
void Common::Compress(Stream* InStream, Stream* OutStream, CompressLevel Strength, ProgressEventHandler* ProgressHandler)
{
	System::Byte inBuffer[] = new System::Byte[BufferSize];
	System::Byte outBuffer[];
	System::Byte bufferLen[];
	int read;
	__int64 total = 0, len = -1;

	// Send initial progress event
	if (ProgressHandler)
	{
		try
		{
			if (InStream->get_CanSeek())
				len = InStream->get_Length();
		}
		catch (Exception*)
		{
			len = -1;
		}

		ProgressHandler(0, len);
	}

	// Read initial buffer
	read = InStream->Read(inBuffer, 0, BufferSize);

	// Write compression version into stream
	System::Byte version[] = new System::Byte[1];
	version[0] = CompressionVersion;
	OutStream->Write(version, 0, 1);

	while (read)
	{
		// Compress buffer - note that we are only going to compress used part of buffer,
		// we don't want any left over garbage to end up in compressed stream...
		if (read != BufferSize)
		{
			System::Byte linBuffer[] = new System::Byte[read];
			for (int x = 0; x < read; x++)
				linBuffer[x] = inBuffer[x];

			outBuffer = Compress(linBuffer, Strength);
		}
		else
			outBuffer = Compress(inBuffer, Strength);

		// The output stream is hopefully smaller than the input stream, so we prepend the final size of
		// each compressed buffer into the destination output stream so that we can safely uncompress
		// the stream in a "chunked" fashion later...
		bufferLen = BitConverter::GetBytes(outBuffer->get_Length());
		OutStream->Write(bufferLen, 0, bufferLen->get_Length());
		OutStream->Write(outBuffer, 0, outBuffer->get_Length());

		// Update compression progress
		if (ProgressHandler)
		{
			total += read;
			ProgressHandler(total, len);
		}

		// Read next buffer
		read = InStream->Read(inBuffer, 0, BufferSize);
	}
}

// Uncompress a byte array
System::Byte Common::Uncompress(System::Byte Data[], int UncompressedSize) []
{
	// Uncompressed buffer size is requested because we must allocate a buffer large enough to hold resultant uncompressed
	// data and user will have a better idea of what this will be since they compressed the original data
	int err;
	unsigned long sourceBuffLen = Data->get_Length(), destBuffLen = (unsigned long)UncompressedSize;
	System::Byte Buffer[] = new System::Byte[UncompressedSize];

	// pin byte arrays so they can be safely passed into unmanaged code...
	System::Byte __pin * sourceBuff = &Data[1];
	System::Byte __pin * destBuff = &Buffer[0];

	// Uncompress buffer using zlib
	err = uncompress(destBuff, &destBuffLen, (const unsigned char *)sourceBuff, sourceBuffLen);
 	if (err != Z_OK) throw new CompressionException(S"Uncompression", err);

	// Extract only used part of compressed buffer
	System::Byte outBuffer[] = new System::Byte[destBuffLen];
	for (unsigned long x = 0; x < destBuffLen; x++)
		outBuffer[x] = Buffer[x];

	// When user requests muli-pass compression, there may be multiple compression passes on a buffer,
	// so we cycle through the needed uncompressions to get back to the original data
	if (Data[0])
		return Uncompress(outBuffer, UncompressedSize);
	else
		return outBuffer;
}

// Uncompress a stream
Stream* Common::Uncompress(Stream* InStream)
{
	MemoryStream* outStream = new MemoryStream();
	Uncompress(InStream, outStream, NULL);
	return outStream;
}

// Uncompress a stream onto given output stream
void Common::Uncompress(Stream* InStream, Stream* OutStream, ProgressEventHandler* ProgressHandler)
{
	System::Byte inBuffer[];
	System::Byte outBuffer[];
	System::Byte bufferLen[] = BitConverter::GetBytes((int)0);
	int read, size;
	__int64 total = 0, len = -1;

	// Send initial progress event
	if (ProgressHandler)
	{
		try
		{
			if (InStream->get_CanSeek())
				len = InStream->get_Length();
		}
		catch (Exception*)
		{
			len = -1;
		}

		ProgressHandler(0, len);
	}

	// Read compression version from stream
	System::Byte version[] = new System::Byte[1];

	if (InStream->Read(version, 0, 1))
	{
		if (version[0] != CompressionVersion)
			throw new CompressionException(S"Invalid compression version encountered in compressed stream - decompression aborted.");

		// Read initial buffer
		read = InStream->Read(bufferLen, 0, bufferLen->get_Length());

		while (read)
		{
			// Convert the byte array containing the buffer size into an integer
			size = BitConverter::ToInt32(bufferLen, 0);

			if (size)
			{
				// Create and read the next buffer
				inBuffer = new System::Byte[size];
				read = InStream->Read(inBuffer, 0, size);

				if (read)
				{
					// Uncompress buffer
					outBuffer = Uncompress(inBuffer, BufferSize);
					OutStream->Write(outBuffer, 0, outBuffer->get_Length());
				}

				// Update decompression progress
				if (ProgressHandler)
				{
					total += (read + bufferLen->get_Length());
					ProgressHandler(total, len);
				}
			}

			// Read the size of the next buffer from the stream
			read = InStream->Read(bufferLen, 0, bufferLen->get_Length());
		}
	}
}

// Compress a file using default compression strength (not PKZip compatible...)
void Common::CompressFile(String* SourceFileName, String* DestFileName)
{
	CompressFile(SourceFileName, DestFileName, CompressLevel::DefaultCompression);
}

// Compress a file using specified compression strength (not PKZip compatible...)
void Common::CompressFile(String* SourceFileName, String* DestFileName, CompressLevel Strength)
{
	CompressFile(SourceFileName, DestFileName, Strength, NULL);
}

void Common::CompressFile(String* SourceFileName, String* DestFileName, CompressLevel Strength, ProgressEventHandler* ProgressHandler)
{
	FileStream* sourceFileStream = File::Open(SourceFileName, FileMode::Open, FileAccess::Read, FileShare::Read);
	FileStream* destFileStream = File::Create(DestFileName);

	Compress(sourceFileStream, destFileStream, Strength, ProgressHandler);

	destFileStream->Flush();
	destFileStream->Close();
	sourceFileStream->Close();
}

// Uncompress a file compressed with CompressFile (not PKZip compatible...)
void Common::UncompressFile(String* SourceFileName, String* DestFileName)
{
	UncompressFile(SourceFileName, DestFileName, NULL);
}

// Uncompress a file compressed with CompressFile given progress event handler (not PKZip compatible...)
void Common::UncompressFile(String* SourceFileName, String* DestFileName, ProgressEventHandler* ProgressHandler)
{
	FileStream* sourceFileStream = File::Open(SourceFileName, FileMode::Open, FileAccess::Read, FileShare::Read);
	FileStream* destFileStream = File::Create(DestFileName);

	Uncompress(sourceFileStream, destFileStream, ProgressHandler);

	destFileStream->Flush();
	destFileStream->Close();
	sourceFileStream->Close();
}

// Return CRC32 checksum for specified portion of given buffer
int Common::CRC32(int CRC, System::Byte Data[], int Offset, int Count)
{
	// pin buffer so it can be safely passed into unmanaged code...
	System::Byte __pin * sourceBuff = &Data[Offset];

	// Calculate CRC32 checksum for given buffer...
	return (int)crc32((unsigned long)CRC, sourceBuff, Count);
}

// Return CRC32 checksum of entire given buffer
int Common::CRC32(System::Byte Data[])
{
	return CRC32(0, Data, 0, Data->get_Length());
}

// Return CRC32 checksum of entire given stream
int Common::CRC32(Stream* InStream)
{
	System::Byte inBuffer[] = new System::Byte[BufferSize];
	int crc = 0, read;

	// Calculate CRC32 checksum for stream
	read = InStream->Read(inBuffer, 0, BufferSize);

	while (read)
	{
		// Calculate CRC32 checksum for given buffer...
		crc = CRC32(crc, inBuffer, 0, read);

		// Read next buffer from stream
		read = InStream->Read(inBuffer, 0, BufferSize);
	}

	return crc;
}

// Return CRC32 checksum of given file
int Common::CRC32(String* FileName)
{
	FileStream* fileStream = File::Open(FileName, FileMode::Open, FileAccess::Read, FileShare::Read);

	int crc = CRC32(fileStream);

	fileStream->Close();

	return crc;
}

// Error message generator (used by alternate constructor)
String* CompressionException::GetErrorMessage(String* source, int err)
{
	StringBuilder& errMsg = *(new StringBuilder());

	errMsg.Append(source);
	errMsg.Append(S" Error ");
	errMsg.Append(err);
	
	switch (err)
	{
		case Z_MEM_ERROR:
			errMsg.Append(S": Not enough memory to complete operation.");
			break;
		case Z_BUF_ERROR:
			errMsg.Append(S": Not enough room in the output buffer.");
			break;
		case Z_STREAM_ERROR:
			errMsg.Append(S": Compression level parameter is invalid.");
			break;
		case Z_DATA_ERROR:
			errMsg.Append(S": Input data was corrupt.");
			break;
		case ZIP_PARAMERROR:
			errMsg.Append(S": Invalid function parameter.");
			break;
		case ZIP_BADZIPFILE:
			errMsg.Append(S": Zip file is corrupt.");
			break;
		case ZIP_INTERNALERROR:
			errMsg.Append(S": Internal error encountered during zip file process.");
			break;
		case UNZ_CRCERROR:
			errMsg.Append(S": Invalid CRC32 detected during file unzip process - zip file is likely corrupt.");
			break;
	}

	return errMsg.ToString();
}

// We allow user to access zlib error number if one was generated
int CompressionException::get_ZLibError()
{
	return error;
}

String* Common::ZLibVersion()
{
	return CharBufferToString(zlibVersion());
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