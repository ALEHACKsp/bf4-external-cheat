using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace BF4_Private_By_Tejisav
{
    class WithOverlay
    {
        static string m_line = "Dark Overlay By Tejisav";

        [STAThread]
        public static void StartHack()
        {
            Init();

            ConsoleSpiner spin = new ConsoleSpiner();

            while (true)
            {
                Process process;
                if (GetProcessesByName("bf4", out process))
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ClearCurrentConsoleLine();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(new string('-', Console.WindowWidth - 1));
                    Console.WriteLine("Status: Loaded{0}", new string(' ', 15));
                    Console.WriteLine("Id: {0}", process.Id);

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(true);
                    Application.Run(new Overlay(process));
                    break;
                }
                spin.Turn();
                Thread.Sleep(100);
            }

            Console.ReadKey();
        }

        private static void Init()
        {
            Console.Title = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(m_line);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.Write("Waiting For Battlefield 4 ....");
        }

        public static bool GetProcessesByName(string pName, out Process process)
        {
            Process[] pList = Process.GetProcessesByName(pName);
            process = pList.Length > 0 ? pList[0] : null;
            return process != null;
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);

            for (int i = 0; i < Console.WindowWidth; i++)
                Console.Write("");

            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
