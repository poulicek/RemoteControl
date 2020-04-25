using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using InputHook;

namespace InputHookWin
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            HooksManager.SetHooks((IntPtr)HooksManager.MouseMessages.WM_LBUTTONDOWN);
            Application.Run();
        }
    }
}
