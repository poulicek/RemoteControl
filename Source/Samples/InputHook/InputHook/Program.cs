using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputHook
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press a key to disable input...");
            var key1 = Console.ReadKey();
            disableInput();
            
            while (true)
            {
                Console.WriteLine("Press the same key to enable input...");
                var key2 = Console.ReadKey();
                if (key1 == key2)
                {
                    enableInput();
                    break;
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }


        private static void disableInput()
        {
            HooksManager.SetHooks();
        }

        private static void enableInput()
        {
            HooksManager.UnHook();
        }
    }
}
