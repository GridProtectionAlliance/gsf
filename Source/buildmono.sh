#!/bin/sh
#Build Grid Solutions Framework on Mono - to execute, chmod +x buildmono
xbuild /p:Configuration=Mono /p:PreBuildEvent="" /p:PostBuildEvent="" GridSolutionsFramework.sln