using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using KeyboardLocker.UI;

namespace KeyboardLocker
{
    static class Program
    {
        private static readonly Mutex mutex = new Mutex(false, "Global\\" + Assembly.GetExecutingAssembly().GetName().Name);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {            
            if (!mutex.WaitOne(0, false))
                return;

            AppDomain.CurrentDomain.UnhandledException += onUnhandledException;
            Application.Run(new TrayIcon());
        }

        private static void onUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
#if DEBUG
            throw e.ExceptionObject as Exception;
#endif
        }
    }
}
