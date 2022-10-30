/**
 * Author:    George Key <1spb-org at github>
 * Created:   30.10.2022
 *  
 **/

namespace T2G
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new FMain());
        }
    }
}