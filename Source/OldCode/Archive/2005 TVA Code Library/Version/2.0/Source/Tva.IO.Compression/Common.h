// JRC 2004 - Common.h
// Note 2005: Removed from project settings "__DllMainCRTStartup@12"

#pragma once
#pragma unmanaged
#include "zlib\\zlib.h"
#include "zlib\\zip.h"
#include "zlib\\unzip.h"
#pragma managed

using namespace System;
using namespace System::IO;
using namespace System::Text;
using namespace System::Runtime::InteropServices;

// A 256K buffer size produces very good compression, slightly better than WinZip (~2%) when using the
// CompressFile function with CompressLevel::BestCompression.  To achieve best results zlib needs a
// sizeable buffer to work with, however when these buffers are needed in the code they are created on
// the garbage collected heap and used as briefly as possible.  Even so, you may want to reduce this
// buffer size if you intend on running this code on an embedded device...
#define BufferSize 262144

// Needed version of this library to uncompress stream (1.0.0 stored as byte 100)
#define CompressionVersion 100

String* CharBufferToString(const char* str);
char* StringToCharBuffer(String* gcStr);

namespace Tva
{
	namespace IO
	{
		namespace Compression
		{
			public __value enum CompressLevel : int
			{
				DefaultCompression = Z_DEFAULT_COMPRESSION,
				NoCompression = Z_NO_COMPRESSION,
				BestSpeed = Z_BEST_SPEED,
				BestCompression = Z_BEST_COMPRESSION,
				MultiPass = 10
			};

			public __delegate void ProgressEventHandler(__int64 bytesCompleted, __int64 bytesTotal);

			// Common compression functions
			public __gc class Common
			{
				public: 
					static System::Byte Compress(System::Byte data[]) [];
					static System::Byte Compress(System::Byte data[], CompressLevel strength) [];			
					static Stream* Compress(Stream* inStream);
					static Stream* Compress(Stream* inStream, CompressLevel strength);
					static void Compress(Stream* inStream, Stream* outStream, CompressLevel strength, ProgressEventHandler* progressHandler);
					
					static System::Byte Uncompress(System::Byte data[], int uncompressedSize) [];
					static Stream* Uncompress(System::IO::Stream* inStream);
					static void Uncompress(Stream* inStream, Stream* outStream, ProgressEventHandler* progressHandler);
					
					static void CompressFile(String* sourceFileName, String* destFileName);
					static void CompressFile(String* sourceFileName, String* destFileName, CompressLevel strength);
					static void CompressFile(String* sourceFileName, String* destFileName, CompressLevel strength, ProgressEventHandler* progressHandler);
					
					static void UncompressFile(String* sourceFileName, String* destFileName);
					static void UncompressFile(String* sourceFileName, String* destFileName, ProgressEventHandler* progressHandler);
					
					static UInt32 CRC32(UInt32 crc, System::Byte data[], int offset, int count);
					static UInt32 CRC32(System::Byte data[]);
					static UInt32 CRC32(Stream* inStream);
					static UInt32 CRC32(String* fileName);
					
					static UInt16 CRC16(UInt16 crc, System::Byte data[], int offset, int count);
					static UInt16 CRC16(System::Byte data[]);
					static UInt16 CRC16(Stream* inStream);
					static UInt16 CRC16(String* fileName);
					
					static UInt16 CRC_CCITT(UInt16 crc, System::Byte data[], int offset, int count);
					static UInt16 CRC_CCITT(System::Byte data[]);
					static UInt16 CRC_CCITT(Stream* inStream);
					static UInt16 CRC_CCITT(String* fileName);
					
					static String* ZLibVersion();
				private:
					Common(){}
					static System::Byte Compress(System::Byte data[], CompressLevel strength, int compressionDepth) [];
			};

			// Custom compression exception class derived from System::Exception
			__gc class CompressionException : public Exception
			{
				public: 
					CompressionException(String* message) : Exception(message), error(Int32::MaxValue){}
					CompressionException(String* source, int err) : Exception(GetErrorMessage(source, err)), error(err){}
					__property int get_ZLibError();
				private:
					static String* GetErrorMessage(String* source, int err);
					int error;
			};			
		}
	}
}