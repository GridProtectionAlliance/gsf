#!/bin/sh
#Build Grid Solutions Framework on Mono - to execute, chmod +x buildmono.sh
xbuild /p:Configuration=Mono /p:PreBuildEvent="" /p:PostBuildEvent="" GridSolutionsFramework.sln