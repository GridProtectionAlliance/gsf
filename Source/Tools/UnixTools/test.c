//******************************************************************************************************
//  test.c - Gbtc
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

//  Compiling for Linux:
//
//	Compile GSF.POSIX.o object library first, then...
//
//	To compile as a mono hosted executable:
//		gcc -o test test.c GSF.POSIX.o -lpam -lpam_misc -lcrypt
//

#include <stdlib.h>
#include <stdio.h>
#include "GSF.POSIX.h"

int main(int argc, char* argv[])
{
    int retval;

    if (argc != 2)
    {
        fprintf(stderr, "Specify account name as an argument.\n");
        retval = 1;
    }
    else
    {
        char** members;

        if (GetLocalGroupMembers(argv[1], &members) == 0)
        {
            int i;

            fprintf(stdout, "Group \"%s\" members: ", argv[1]);

            for (i = 0; members[i] != NULL; i++)
            {
                fprintf(stdout, "%s%s", i > 0 ? "," : "", members[i]);
            }

            fprintf(stdout, "\n\n");

            FreeLocalGroupMembers(members);
        }

        struct UserPasswordInformation info;
        int status;
        int retval = GetLocalUserPasswordInformation(argv[1], &info, &status);

        fprintf(stdout, "GetLocalUserPasswordInformation(%s) = %d\n", argv[1], retval);

        if (retval == 0)
        {
            fprintf(stdout, "lastChangeDate = %ld\n", info.lastChangeDate);
            fprintf(stdout, "minDaysForChange = %ld\n", info.minDaysForChange);
            fprintf(stdout, "maxDaysForChange = %ld\n", info.maxDaysForChange);
            fprintf(stdout, "status = %d\n", status);
        }
    }

    return retval;
}
