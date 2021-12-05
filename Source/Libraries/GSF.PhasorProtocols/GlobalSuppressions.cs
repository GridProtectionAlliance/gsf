// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Event method naming convention matches m_ member name pattern")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters required by delegate pattern")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Keeping method for completeness", Scope = "member", Target = "~M:GSF.PhasorProtocols.MultiProtocolFrameParser.SharedTcpServerReference.TerminateSharedClient")]
