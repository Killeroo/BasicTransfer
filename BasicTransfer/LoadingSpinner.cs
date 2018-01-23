using System;
using System.Threading;

namespace Basic_Transfer
{
    public class LoadingSpinner
    {
        private static Thread thread;
        private static char[] cursor = new char[] { '-', '\\', '|', '/' };
        private static int startx = 0;
        public static void Start()
        {
            Console.CursorVisible = false;
            startx = Console.CursorLeft;

            thread = new Thread(() =>
            {
                int count = 0;
                while (true)
                {
                    if (count == cursor.Length)
                        count = 0;

                    // Draw spinner
                    Console.Write(cursor[count]);
                    Console.CursorLeft--;

                    count++;

                    Thread.Sleep(100);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void Stop()
        {
            // Stop drawing spinner and cleanup console
            thread.Abort();

            Console.CursorTop--;
            Console.CursorLeft = startx;
            Console.WriteLine(" ");
            Console.CursorVisible = true;
        }
    }
}
