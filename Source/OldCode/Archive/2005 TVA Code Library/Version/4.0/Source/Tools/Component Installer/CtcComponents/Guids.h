// guids.h: definitions of GUIDs/IIDs/CLSIDs used in this VsPackage

/*
Do not use #pragma once, as this file needs to be included twice.  Once to declare the externs
for the GUIDs, and again right after including initguid.h to actually define the GUIDs.
*/



// package guid
// { a70fa6ea-f478-450b-9c39-cbaa042a5935 }
#define guidTVAComponentInstallerPkg { 0xA70FA6EA, 0xF478, 0x450B, { 0x9C, 0x39, 0xCB, 0xAA, 0x04, 0x2A, 0x59, 0x35 } }
#ifdef DEFINE_GUID
DEFINE_GUID(CLSID_TVAComponentInstaller,
0xA70FA6EA, 0xF478, 0x450B, 0x9C, 0x39, 0xCB, 0xAA, 0x04, 0x2A, 0x59, 0x35 );
#endif

// Command set guid for our commands (used with IOleCommandTarget)
// { c27609b9-29ec-4af3-be7c-b9d7328ebe94 }
#define guidTVAComponentInstallerCmdSet { 0xC27609B9, 0x29EC, 0x4AF3, { 0xBE, 0x7C, 0xB9, 0xD7, 0x32, 0x8E, 0xBE, 0x94 } }
#ifdef DEFINE_GUID
DEFINE_GUID(CLSID_TVAComponentInstallerCmdSet, 
0xC27609B9, 0x29EC, 0x4AF3, 0xBE, 0x7C, 0xB9, 0xD7, 0x32, 0x8E, 0xBE, 0x94 );
#endif


