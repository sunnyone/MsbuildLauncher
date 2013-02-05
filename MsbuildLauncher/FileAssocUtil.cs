using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MsbuildLauncher
{
    public static class FileAssocUtil
    {
        [DllImport("shlwapi.dll", SetLastError = true)]
        static extern uint AssocQueryString(int flags, int str, string pszAssoc, string pszExtra,
                            [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);

        public static string GetAssocExecutable(string extension)
        {
            uint pcchOut = 0;

            int AssocFVerify = 0x40;
            int AssocStrExecutable = 2;

            AssocQueryString(AssocFVerify, AssocStrExecutable, extension, null, null, ref pcchOut);
            StringBuilder pszOut = new StringBuilder((int) pcchOut);
            AssocQueryString(AssocFVerify, AssocStrExecutable, extension, null, pszOut, ref pcchOut);
            return pszOut.ToString();
        }
    }
}
