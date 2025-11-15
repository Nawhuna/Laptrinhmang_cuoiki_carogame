using System;
using System.Windows.Forms;

namespace Client
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Chạy FormLogin trước
            Application.Run(new Client.Forms.FormLogin());
        }
    }
}
