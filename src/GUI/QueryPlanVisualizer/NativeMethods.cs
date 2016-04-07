using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ExecutionPlanVisualizer
{
    class NativeMethods
    {
        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);

        public static string AssocQueryString(AssocStr association, string extension)
        {
            const int sOk = 0;
            const int sFalse = 1;

            uint length = 0;
            var result = AssocQueryString(AssocF.None, association, extension, null, null, ref length);
            if (result != sFalse)
            {
                return null;
            }

            var stringBuilder = new StringBuilder((int)length);
            result = AssocQueryString(AssocF.None, association, extension, null, stringBuilder, ref length);
            if (result != sOk)
            {
                return null;
            }

            return stringBuilder.ToString();
        }

        [Flags]
        enum AssocF : uint
        {
            None = 0,
        }

        internal enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DdeCommand,
            DdeIfExec,
            DdeApplication,
            DdeTopic,
            InfoTip,
            QuickTip,
            TileInfo,
            ContentType,
            DefaultIcon,
            ShellExtension,
            DropTarget,
            DelegateExecute,
            SupportedUriProtocols,
            Max,
        }
    }
}