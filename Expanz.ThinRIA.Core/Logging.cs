using System;
using System.Diagnostics;

namespace Expanz.ThinRIA
{
    internal static class Logging
    {
        /// <summary>
        /// function to show exception
        /// </summary>
        /// <param name="e"></param>
        public static void LogException(Exception e)
        {
            Debug.WriteLine("Logged exception: " + ExceptionMessage(e));
            //MessageBox.Show(ExceptionMessage(e) + System.Environment.NewLine + e.StackTrace, e.Message, MessageBoxButton.OK);
        }

        /// <summary>
        /// function to show exception
        /// </summary>
        /// <param name="e"></param>
        /// <param name="s"></param>
        public static void DebugException(Exception e, string s)
        {
            Debug.WriteLine("Debug exception: " + ExceptionMessage(e));
            //MessageBox.Show(ExceptionMessage(e) + System.Environment.NewLine + e.StackTrace, s, MessageBoxButton.OK);
        }

        /// <summary>
        /// function to get exception messages
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string ExceptionMessage(Exception e)
        {
            string ret = e.Message;
            if (e.InnerException != null)
            {
                ret += System.Environment.NewLine + "Inner Exception:" + System.Environment.NewLine + e.InnerException.Message;
            }
            return ret;
        }
    }
}