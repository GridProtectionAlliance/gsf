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

			public __delegate void ProgressEventHandler(__int64 BytesCompleted, __int64 BytesTotal);

			// Common compression functions
			public __gc class Common
			{
				public: 
					static System::Byte Compress(System::Byte Data[]) [];
					static System::Byte Compress(System::Byte Data[], CompressLevel Strength) [];			
					static Stream* Compress(Stream* InStream);
					static Stream* Compress(Stream* InStream, CompressLevel Strength);
					static void Compress(Stream* InStream, Stream* OutStream, CompressLevel Strength, ProgressEventHandler* ProgressHandler);
					
					static System::Byte Uncompress(System::Byte Data[], int UncompressedSize) [];
					static Stream* Uncompress(System::IO::Stream* InStream);
					static void Uncompress(Stream* InStream, Stream* OutStream, ProgressEventHandler* ProgressHandler);
					
					static void CompressFile(String* SourceFileName, String* DestFileName);
					static void CompressFile(String* SourceFileName, String* DestFileName, CompressLevel Strength);
					static void CompressFile(String* SourceFileName, String* DestFileName, CompressLevel Strength, ProgressEventHandler* ProgressHandler);
					
					static void UncompressFile(String* SourceFileName, String* DestFileName);
					static void UncompressFile(String* SourceFileName, String* DestFileName, ProgressEventHandler* ProgressHandler);
					
					static Int32 CRC32(Int32 CRC, System::Byte Data[], int Offset, int Count);
					static Int32 CRC32(System::Byte Data[]);
					static Int32 CRC32(Stream* InStream);
					static Int32 CRC32(String* FileName);
					
					static Int16 CRC16(Int16 CRC, System::Byte Data[], int Offset, int Count);
					static Int16 CRC16(System::Byte Data[]);
					static Int16 CRC16(Stream* InStream);
					static Int16 CRC16(String* FileName);
					
					static Int16 CRC_CCITT(Int16 CRC, System::Byte Data[], int Offset, int Count);
					static Int16 CRC_CCITT(System::Byte Data[]);
					static Int16 CRC_CCITT(Stream* InStream);
					static Int16 CRC_CCITT(String* FileName);
					
					static String* ZLibVersion();
				private:
					Common(){}
					static System::Byte Compress(System::Byte Data[], CompressLevel Strength, int CompressionDepth) [];
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