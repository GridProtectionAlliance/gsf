// JRC 2004 - Common compression functions
#include "stdafx.h"
#include "Common.h"
#include "zlib\\zlib.h"
#include "zlib\\zip.h"
#include "zlib\\unzip.h"

using namespace TVA::Compression;

static unsigned short crc16tab[256] =
{
	0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7,
	0x8108, 0x9129, 0xa14a, 0xb16b, 0xc18c, 0xd1ad, 0xe1ce, 0xf1ef,
	0x1231, 0x0210, 0x3273, 0x2252, 0x52b5, 0x4294, 0x72f7, 0x62d6,
	0x9339, 0x8318, 0xb37b, 0xa35a, 0xd3bd, 0xc39c, 0xf3ff, 0xe3de,
	0x2462, 0x3443, 0x0420, 0x1401, 0x64e6, 0x74c7, 0x44a4, 0x5485,
	0xa56a, 0xb54b, 0x8528, 0x9509, 0xe5ee, 0xf5cf, 0xc5ac, 0xd58d,
	0x3653, 0x2672, 0x1611, 0x0630, 0x76d7, 0x66f6, 0x5695, 0x46b4,
	0xb75b, 0xa77a, 0x9719, 0x8738, 0xf7df, 0xe7fe, 0xd79d, 0xc7bc,
	0x48c4, 0x58e5, 0x6886, 0x78a7, 0x0840, 0x1861, 0x2802, 0x3823,
	0xc9cc, 0xd9ed, 0xe98e, 0xf9af, 0x8948, 0x9969, 0xa90a, 0xb92b,
	0x5af5, 0x4ad4, 0x7ab7, 0x6a96, 0x1a71, 0x0a50, 0x3a33, 0x2a12, 
	0xdbfd, 0xcbdc, 0xfbbf, 0xeb9e, 0x9b79, 0x8b58, 0xbb3b, 0xab1a, 
	0x6ca6, 0x7c87, 0x4ce4, 0x5cc5, 0x2c22, 0x3c03, 0x0c60, 0x1c41, 
	0xedae, 0xfd8f, 0xcdec, 0xddcd, 0xad2a, 0xbd0b, 0x8d68, 0x9d49, 
	0x7e97, 0x6eb6, 0x5ed5, 0x4ef4, 0x3e13, 0x2e32, 0x1e51, 0x0e70,
	0xff9f, 0xefbe, 0xdfdd, 0xcffc, 0xbf1b, 0xaf3a, 0x9f59, 0x8f78, 
	0x9188, 0x81a9, 0xb1ca, 0xa1eb, 0xd10c, 0xc12d, 0xf14e, 0xe16f,
	0x1080, 0x00a1, 0x30c2, 0x20e3, 0x5004, 0x4025, 0x7046, 0x6067, 
	0x83b9, 0x9398, 0xa3fb, 0xb3da, 0xc33d, 0xd31c, 0xe37f, 0xf35e,
	0x02b1, 0x1290, 0x22f3, 0x32d2, 0x4235, 0x5214, 0x6277, 0x7256,
	0xb5ea, 0xa5cb, 0x95a8, 0x8589, 0xf56e, 0xe54f, 0xd52c, 0xc50d,
	0x34e2, 0x24c3, 0x14a0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
	0xa7db, 0xb7fa, 0x8799, 0x97b8, 0xe75f, 0xf77e, 0xc71d, 0xd73c,
	0x26d3, 0x36f2, 0x0691, 0x16b0, 0x6657, 0x7676, 0x4615, 0x5634,
	0xd94c, 0xc96d, 0xf90e, 0xe92f, 0x99c8, 0x89e9, 0xb98a, 0xa9ab,
	0x5844, 0x4865, 0x7806, 0x6827, 0x18c0, 0x08e1, 0x3882, 0x28a3,
	0xcb7d, 0xdb5c, 0xeb3f, 0xfb1e, 0x8bf9, 0x9bd8, 0xabbb, 0xbb9a,
	0x4a75, 0x5a54, 0x6a37, 0x7a16, 0x0af1, 0x1ad0, 0x2ab3, 0x3a92,
	0xfd2e, 0xed0f, 0xdd6c, 0xcd4d, 0xbdaa, 0xad8b, 0x9de8, 0x8dc9,
	0x7c26, 0x6c07, 0x5c64, 0x4c45, 0x3ca2, 0x2c83, 0x1ce0, 0x0cc1,
	0xef1f, 0xff3e, 0xcf5d, 0xdf7c, 0xaf9b, 0xbfba, 0x8fd9, 0x9ff8,
	0x6e17, 0x7e36, 0x4e55, 0x5e74, 0x2e93, 0x3eb2, 0x0ed1, 0x1ef0
};

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
Int32 Common::CRC32(Int32 CRC, System::Byte Data[], int Offset, int Count)
{
	// pin buffer so it can be safely passed into unmanaged code...
	System::Byte __pin * sourceBuff = &Data[Offset];

	// Calculate CRC32 checksum for given buffer...
	return (Int32)crc32((unsigned long)CRC, sourceBuff, Count);
}

// Return CRC32 checksum of entire given buffer
Int32 Common::CRC32(System::Byte Data[])
{
	return CRC32(0, Data, 0, Data->get_Length());
}

// Return CRC32 checksum of entire given stream
Int32 Common::CRC32(Stream* InStream)
{
	System::Byte inBuffer[] = new System::Byte[BufferSize];
	Int32 crc = 0, read;

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
Int32 Common::CRC32(String* FileName)
{
	FileStream* fileStream = File::Open(FileName, FileMode::Open, FileAccess::Read, FileShare::Read);

	Int32 crc = CRC32(fileStream);

	fileStream->Close();

	return crc;
}

// Return CRC16 checksum for specified portion of given buffer
Int16 Common::CRC16(Int16 CRC, System::Byte Data[], int Offset, int Count)
{
	/*
		2-byte (16-bit) CRC: The generating polynomial is
		    16   12   5    0
	    G(X) = X  + X  + X  + X
	*/
	UInt16 crc = (UInt16)CRC;

	for (int x = 0; x < Count; x++)
	{
		crc = (crc << 8) ^ crc16tab[((crc >> 8) ^ Data[x + Offset]) & 0xff];
	}

	return (Int16)crc;
}

// Return CRC16 checksum of entire given buffer
Int16 Common::CRC16(System::Byte Data[])
{
	return CRC16(-1, Data, 0, Data->get_Length());
}

// Return CRC16 checksum of entire given stream
Int16 Common::CRC16(Stream* InStream)
{
	System::Byte inBuffer[] = new System::Byte[BufferSize];
	Int16 crc = -1, read;

	// Calculate CRC16 checksum for stream
	read = InStream->Read(inBuffer, 0, BufferSize);

	while (read)
	{
		// Calculate CRC16 checksum for given buffer...
		crc = CRC16(crc, inBuffer, 0, read);

		// Read next buffer from stream
		read = InStream->Read(inBuffer, 0, BufferSize);
	}

	return crc;
}

// Return CRC16 checksum of given file
Int16 Common::CRC16(String* FileName)
{
	FileStream* fileStream = File::Open(FileName, FileMode::Open, FileAccess::Read, FileShare::Read);

	Int16 crc = CRC16(fileStream);

	fileStream->Close();

	return crc;
}

// Return CRC16 checksum for specified portion of given buffer
Int16 Common::QuickCRC16(Int16 CRC, System::Byte Data[], int Offset, int Count)
{
	UInt16 crc = (UInt16)CRC, temp, quick;

	for (int x = 0; x < Count; x++)
	{
		temp = (crc >> 8) ^ Data[x + Offset];
		crc <<= 8;
		quick = temp ^ (temp >> 4);
		crc ^= quick;
		quick <<= 5;
		crc ^= quick;
		quick <<= 7;
		crc ^= quick;
	}

	return (Int16)crc;
}

// Return CRC16 checksum of entire given buffer
Int16 Common::QuickCRC16(System::Byte Data[])
{
	return QuickCRC16(-1, Data, 0, Data->get_Length());
}

// Return CRC16 checksum of entire given stream
Int16 Common::QuickCRC16(Stream* InStream)
{
	System::Byte inBuffer[] = new System::Byte[BufferSize];
	Int16 crc = -1, read;

	// Calculate CRC16 checksum for stream
	read = InStream->Read(inBuffer, 0, BufferSize);

	while (read)
	{
		// Calculate CRC16 checksum for given buffer...
		crc = QuickCRC16(crc, inBuffer, 0, read);

		// Read next buffer from stream
		read = InStream->Read(inBuffer, 0, BufferSize);
	}

	return crc;
}

// Return CRC16 checksum of given file
Int16 Common::QuickCRC16(String* FileName)
{
	FileStream* fileStream = File::Open(FileName, FileMode::Open, FileAccess::Read, FileShare::Read);

	Int16 crc = QuickCRC16(fileStream);

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