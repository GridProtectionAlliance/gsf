//******************************************************************************************************
//  gsf.c - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/29/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// This will create a hosted Mono service that contains needed GSF functions

//  Compiling for Linux:
//
//	Get PAM development libraries, e.g., for Ubuntu: apt-get install libpam0g-dev
//
//	Compile GSF.POSIX.o object library first, then...
//
//	To compile as a mono hosted executable:
//		gcc -o gsf gsf.c `pkg-config --cflags --libs mono-2` GSF.POSIX.o -lm -lpam -lpam_misc -lcrypt -rdynamic
//
//	To export symbols for DllImport("__Internal") use gcc parameter -rdynamic
//

#include <mono/jit/jit.h>
#include <mono/metadata/environment.h>
#include <stdlib.h>

int main(int argc, char* argv[])
{
    MonoDomain *domain;
    MonoAssembly *assembly;
    const char *file;
    int retval;
    
    if (argc < 2)
    {
        fprintf(stderr, "Please provide name of mono assembly to load.\n");
        return 1;
    }
    
    file = argv[1];

    // Load the default Mono configuration file, this is needed
    // if you are planning on using the dllmaps defined on the
    // system configuration
    mono_config_parse(NULL);
    
    // Create mono AppDomain
    domain = mono_jit_init(file);

    // Open entry assembly in new AppDomain
    assembly = mono_domain_assembly_open(domain, file);

    if (assembly != NULL)
    {
        // mono_jit_exec runs the Main() method in the assembly
        mono_jit_exec(domain, assembly, argc - 1, argv + 1);

        // Get exit code from entry assembly
        retval = mono_environment_exitcode_get();
    }
    else
    {
        fprintf(stderr, "Failed to load mono assembly: %s\n", file);
        retval = 2;
    }
    
    // This clean up call tends to crash - since we are exiting anyway, we skip it...
    //mono_jit_cleanup(domain);

    return retval;
}
