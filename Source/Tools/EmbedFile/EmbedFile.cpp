// EmbedFile.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"

FILE* open_or_exit(const char* fname, const char* mode)
{
    FILE* f;
    errno_t err;
    
    if ((err = fopen_s(&f, fname, mode)) != 0) {
        perror(fname);
        exit(EXIT_FAILURE);
    }

    return f;
}

void write_byte(FILE* out, int32_t& chars, bool& first, unsigned char value)
{
    if (chars++ % 10 == 0)
    {
        if (!first)
            fprintf(out, ",");

        fprintf(out, "\n    ");
    }
    else
    {
        fprintf(out, ", ");
    }

    fprintf(out, "0x%02X", value);
    first = false;
}

int main(int argc, char** argv)
{
    if (argc < 3) {
        fprintf(stderr, "USAGE: %s {sym} {rsrc} [{namespace}] [addnull]\n\n"
            "  Creates {sym}.cpp from the contents of {rsrc}\n",
            argv[0]);
        return EXIT_FAILURE;
    }

    const char* sym = argv[1];
    FILE* in = open_or_exit(argv[2], "r");
    char empty[1] = "";
    const char* prefix = "";
    bool addNullByte = false;

    if (argc > 3)
        prefix = argv[3];

    if (argc > 4)
        addNullByte = _strcmpi(argv[4], "addnull") == 0;

    char symfile[256];
    snprintf(symfile, sizeof(symfile), "%s.h", sym);
    FILE* out = open_or_exit(symfile,"w");

    char symdirective[256];
    const char* s = sym;
    char* d = symdirective;

    while (*s) {
        if (isupper(*s))
            *d++ = '_';
        *d++ = toupper(*s++);
    }

    *d = '\0';

    char now[256];
    time_t rawtime;
    struct tm timeinfo {};
    time(&rawtime);
    localtime_s(&timeinfo, &rawtime);
    asctime_s(now, 256, &timeinfo);

    fprintf(out, "// Auto-generated on %s", now);
    fprintf(out, "// EmbedFile.exe %s %s %s%s\n\n", sym, argv[2], prefix, addNullByte ? " addnull" : "");
    fprintf(out, "#ifndef %s\n", symdirective);
    fprintf(out, "#define %s\n\n", symdirective);
    fprintf(out, "#include <cstdint>\n\n");

    if (strlen(prefix) > 0)
    {
        char source[256];
        strcpy_s(source, 256, prefix);

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

    fprintf(out, "\n\n#endif\n");
    fclose(out);

    snprintf(symfile, sizeof(symfile), "%s.cpp", sym);
    out = open_or_exit(symfile,"w");

    fprintf(out, "// Auto-generated on %s", now);
    fprintf(out, "// EmbedFile.exe %s %s %s%s\n\n", sym, argv[2], prefix, addNullByte ? " addnull" : "");
    fprintf(out, "#include \"%s.h\"\n\n", sym);
    fprintf(out, "const uint8_t %s%s[] = {", prefix, sym);

    unsigned char buf[256];
    size_t nread = 0;
    int32_t chars = 0;
    bool first = true;

    do
    {
        nread = fread(buf, 1, sizeof(buf), in);
        size_t i;
        
        for (i = 0; i < nread; i++)
            write_byte(out, chars, first, buf[i]);
    }
    while (nread > 0);

    // Write NULL as last character, if requested
    if (addNullByte)
        write_byte(out, chars, first, '\0');
    
    fprintf(out, "\n};\n\n");
    fprintf(out, "const uint32_t %s%sLength = sizeof(%s%s);\n", prefix, sym, prefix, sym);

    fclose(in);
    fclose(out);

    return EXIT_SUCCESS;
}
