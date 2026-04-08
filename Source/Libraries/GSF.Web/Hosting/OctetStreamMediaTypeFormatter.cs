//******************************************************************************************************
//  OctetStreamMediaTypeFormatter.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/08/2026 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSF.Web.Hosting;

/// <summary>
/// A <see cref="MediaTypeFormatter"/> for HTTP data as a byte array.
/// </summary>
public class OctetStreamMediaTypeFormatter : MediaTypeFormatter
{
    /// <summary>
    /// Creates a new instance of the <see cref="OctetStreamMediaTypeFormatter"/> class.
    /// </summary>
    public OctetStreamMediaTypeFormatter()
    {
        const string ApplicationOctetStream = System.Net.Mime.MediaTypeNames.Application.Octet;
        MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue(ApplicationOctetStream);
        SupportedMediaTypes.Add(mediaType);
    }

    /// <inheritdoc/>
    public override bool CanReadType(Type type)
    {
        return type == typeof(byte[]);
    }

    /// <inheritdoc/>
    public override bool CanWriteType(Type type)
    {
        return type == typeof(byte[]);
    }

    /// <inheritdoc/>
    public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
    {
        return ReadFromStreamAsync(type, readStream, content, formatterLogger, default);
    }

    /// <inheritdoc/>
    public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
    {
        long length = content.Headers.ContentLength.GetValueOrDefault();

        return (length > 0L && length <= int.MaxValue)
            ? await ReadFromAsync(readStream, (int)length, cancellationToken)
            : await ReadFromAsync(readStream, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
    {
        return WriteToStreamAsync(type, value, writeStream, content, transportContext, default);
    }

    /// <inheritdoc/>
    public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
    {
        if (!(value is byte[] bytes))
            throw new InvalidOperationException("Value is not a byte array");

        await writeStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
    }

    private async Task<byte[]> ReadFromAsync(Stream stream, int length, CancellationToken cancellationToken)
    {
        byte[] bytes = new byte[length];
        int offset = 0;
        int remaining = length;

        while (offset < length)
        {
            int bytesRead = await stream.ReadAsync(bytes, offset, remaining, cancellationToken);
            if (bytesRead == 0) throw new EndOfStreamException();
            offset += bytesRead;
            remaining -= bytesRead;
        }

        return bytes;
    }

    private async Task<byte[]> ReadFromAsync(Stream stream, CancellationToken cancellationToken)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            await stream.CopyToAsync(memoryStream, 81920, cancellationToken);
            return memoryStream.ToArray();
        }
    }
}
