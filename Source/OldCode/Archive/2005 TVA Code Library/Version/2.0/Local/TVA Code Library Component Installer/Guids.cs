// Guids.cs
// MUST match guids.h
using System;

namespace TVA
{
    static class GuidList
    {
        public const string guidTVAComponentInstallerPkgString = "a70fa6ea-f478-450b-9c39-cbaa042a5935";
        public const string guidTVAComponentInstallerCmdSetString = "c27609b9-29ec-4af3-be7c-b9d7328ebe94";

        public static readonly Guid guidTVAComponentInstallerPkg = new Guid(guidTVAComponentInstallerPkgString);
        public static readonly Guid guidTVAComponentInstallerCmdSet = new Guid(guidTVAComponentInstallerCmdSetString);
    };
}