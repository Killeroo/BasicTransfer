using System;
using System.Threading;

namespace Basic_Transfer
{
    public class LoadingSpinner
    {
        private static Thread thread;
        private static char[] cursor = new char[] { '-', '\\', '|', '/' };
        private static int startX = 0;
        private static bool cleanedUp = true; // Flag to check if we have already cleaned spinner

        public static void Start()
        {
            Console.CursorVisible = false;
            cleanedUp = false;
            startX = Console.CursorLeft;

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
            // Stop drawing spinner
            try { thread.Abort(); }
            catch (ThreadStateException) { } // Sliently ignore errors if thread has already been aborted

            // cleanup console
            if (!cleanedUp)
            {
                Console.CursorTop--;
                Console.CursorLeft = startX;
                Console.WriteLine(" ");
                Console.CursorVisible = true;

                cleanedUp = true;
            }
        }
    }
}
