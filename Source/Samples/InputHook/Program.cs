using System;
using System.Windows.Forms;

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
            var inputBLocker = new InputBlocker(new KeyCombination(Keys.Pause));
            Application.Run();
        }
    }
}
