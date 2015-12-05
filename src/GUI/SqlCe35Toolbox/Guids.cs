// Guids.cs
// MUST match guids.h
using System;

namespace ErikEJ.SqlCeToolbox
{
    static class GuidList
    {
        public const string guidSqlCeToolboxPkgString = "41521019-e4c7-480c-8ea8-fc4a2c6f50aa";
        public const string guidSqlCeToolboxCmdSetString = "97755b4a-7dce-4a6f-b60e-b9cffd7ffd8b";
        public const string guidToolWindowPersistanceString = "c5bb427c-36fe-45e9-ac41-f1895991c277";
        public const string GuidPageGeneral = "D6A11FF4-1079-42E9-A3D2-03E83276931A";
        public const string GuidPageAdvanced = "07227C89-31A1-4FCF-B6F6-7BBB0FACC1E3";

        public static readonly Guid guidSEPlusCmdSet = new Guid("dd722525-dfc4-4f55-b7ca-a29e0c857cc4");
        public static readonly Guid guidSqlCeToolboxCmdSet = new Guid(guidSqlCeToolboxCmdSetString);
    };
}