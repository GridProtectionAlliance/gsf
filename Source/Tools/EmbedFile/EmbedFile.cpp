//******************************************************************************************************
//  EmbedFile.cpp - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  02/02/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

/*
 * This application is used to create a portable embedded resource for a C++ application.
 * The program will take the file name specified as a command line argument and turn it
 * into a byte array where the array is declared in a .H file and defined in a .CPP file.
 */

#include "pch.h"

FILE* open_or_exit(const char* fname, const char* mode)
{
    FILE* f;
    
    if (fopen_s(&f, fname, mode) != 0) {
        perror(fname);
        exit(EXIT_FAILURE);
    }

    return f;
}

void write_byte(FILE* out, int32_t& chars, unsigned char value)
{
    if (chars++ % 10 == 0)
    {
        if (chars > 1)
            fprintf(out, ",");

        fprintf(out, "\n    ");
    }
    else
    {
        fprintf(out, ", ");
    }

    fprintf(out, "0x%02X", value);
}

bool starts_with(const char* str, const char* prefix)
{
    return _strnicmp(prefix, str, strlen(prefix)) == 0;
}

int main(int argc, char** argv)
{
    if (argc < 3) {
        fprintf(stderr, "\nUSAGE: EmbedFile {sym} {rsrc} [-namespace={ns1::ns2::}] [-addNull] [-useIncludeGuard]\n\n"
            "  Creates {sym}.cpp/.h from the contents of {rsrc}\n\n");
        return EXIT_FAILURE;
    }

    const char* sym = argv[1];
    FILE* in = open_or_exit(argv[2], "r");
    const char* namespaceArgument = "";
    const char* namespacePrefix = "";
    bool addNullByte = false;
    bool useIncludeGuard = false;

    // Parse optional argumments
    for (int i = 3; i < argc; i++)
    {
        //                                 01234567890
        if (starts_with(argv[i], "-namespace=") && strlen(argv[i]) > 11)
        {
            namespaceArgument = argv[i];
            namespacePrefix = &namespaceArgument[11];
        }
        else if (_strcmpi(argv[i], "-addNull") == 0)
        {
            addNullByte = true;
        }
        else if (_strcmpi(argv[i], "-useIncludeGuard") == 0)
        {
            useIncludeGuard = true;
        }
    }

    // Create .H resource file
    char symfile[256];
    snprintf(symfile, sizeof(symfile), "%s.h", sym);
    FILE* out = open_or_exit(symfile, "w");

    // Define #ifdef directive symbol used to include header file only once
    char symdirective[256];

    if (useIncludeGuard)
    {
        const char* s = sym;
        char* d = symdirective;

        while (*s) {
            if (isupper(*s))
                *d++ = '_';
            *d++ = toupper(*s++);
        }

        *d = '\0';
    }

    char now[256];
    time_t rawtime;
    struct tm timeinfo {};
    time(&rawtime);
    localtime_s(&timeinfo, &rawtime);
    asctime_s(now, 256, &timeinfo);

    // Write header for .H resource file
    fprintf(out, "// Auto-generated on %s", now);
    fprintf(out, "// EmbedFile.exe %s %s %s%s%s\n\n", sym, argv[2], namespaceArgument, addNullByte ? " -addNull" : "", useIncludeGuard ? " -useIncludeGuard" : "");

    if (useIncludeGuard)
    {
        fprintf(out, "#ifndef %s\n", symdirective);
        fprintf(out, "#define %s\n\n", symdirective);
    }
    else
    {
        fprintf(out, "#pragma once\n\n");
    }

    fprintf(out, "#include <cstdint>\n\n");

    if (strlen(namespacePrefix) > 0)
    {
        // When a C++ namespace prefix is provided, break each namespace
        // component out into separate namespace commands for the .H file
        char source[256];
        strcpy_s(source, 256, namespacePrefix);

        char* next_token;
        char* token = strtok_s(source, "::", &next_token);
        int32_t count = 0;

        while (token) {
            if (count++ > 0)
                fprintf(out, " {\n");

            fprintf(out, "namespace %s", token);
            token = strtok_s(nullptr, "::", &next_token);
        }

        fprintf(out, "\n{\n");
        fprintf(out, "    const extern uint8_t %s[];\n", sym);
        fprintf(out, "    const extern uint32_t %sLength;\n", sym);

        for (int32_t i = 0; i < count; i++)
            fprintf(out, "}");
    }
    else
    {
        fprintf(out, "const extern uint8_t %s[];\n", sym);
        fprintf(out, "const extern uint32_t %sLength;", sym);
    }

    if (useIncludeGuard)
        fprintf(out, "\n\n#endif\n");

    fclose(out);

    // Create .CPP resource file
    snprintf(symfile, sizeof(symfile), "%s.cpp", sym);
    out = open_or_exit(symfile, "w");

    // Write header for .CPP resource file
    fprintf(out, "// Auto-generated on %s", now);
    fprintf(out, "// EmbedFile.exe %s %s %s%s%s\n\n", sym, argv[2], namespaceArgument, addNullByte ? " -addNull" : "", useIncludeGuard ? " -useIncludeGuard" : "");
    fprintf(out, "#include \"%s.h\"\n\n", sym);
    fprintf(out, "const uint8_t %s%s[] = {", namespacePrefix, sym);

    unsigned char buf[256];
    size_t nread;
    int32_t chars = 0;

    // Write resource bytes
    do
    {
        nread = fread(buf, 1, sizeof(buf), in);
        size_t i;
        
        for (i = 0; i < nread; i++)
            write_byte(out, chars, buf[i]);
    }
    while (nread > 0);

    // Write NULL as last character, if requested
    if (addNullByte)
        write_byte(out, chars, '\0');
    
    fprintf(out, "\n};\n\n");
    fprintf(out, "const uint32_t %s%sLength = sizeof(%s%s);\n", namespacePrefix, sym, namespacePrefix, sym);

    fclose(in);
    fclose(out);

    return EXIT_SUCCESS;
}
