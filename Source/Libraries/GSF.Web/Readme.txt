Many of the locally referenced NuGet packages, even though not directly required
by the GSF.Web project code, are managed as dependencies on behalf of downstream
applications that reference GSF.Web in order to support a self-hosted web server.

Having these packages referenced here helps to reduce the locally required NuGet
references in the GSF.Web dependent downstream applications and simplifies version
updates. Updates to NuGet references here will flow down to all dependent
applications during the nightly build process.

As a result, do not trust dependency walkers that may tell you that a certain
reference is not required as it may likely be required by a downstream application
in order to properly self-host a web site.

Also, be mindful that adding or updating NuGet packages may auto-add an app.config
to the GSF.Web project as well as any local projects that may depend on GSF.Web,
e.g., ModbusAdapters. Make sure these app.config files do not get checked-in as
they are typically not needed. However, the updated content in the app.config is
usually assembly binding updates that may be useful since multiple NuGet packages
could reference different versions of the same DLL. The assembly binding information
needs to be migrated into the local "GSF.Web.AssemblyBindings.xml" found in the
project root of the GSF.Web project. This file is an embedded resource that is
used by downstream applications to auto-update their local app.config files where
the assembly binding information is critical. Since GSF.Web does not directly depend
on many of the assemblies referenced, the auto-updates to app.config may not be
complete so manual updates and validation of each the referenced assembly versions
in the AssemblyBindings.xml could be required - the best way to know it simply to
run a downstream application with the updates and only add needed entries.