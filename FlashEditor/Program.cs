using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FlashEditor;

static class Program
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // 二重起動防止用Mutex（既に起動中なら終了）
        using var mutex = new Mutex(true, "FlashEditor_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            // 既に起動中のプロセスを探して最前面に表示する
            using var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);
            try
            {
                foreach (var process in processes)
                {
                    if (process.Id != currentProcess.Id && process.MainWindowHandle != IntPtr.Zero)
                    {
                        IntPtr hWnd = process.MainWindowHandle;
                        if (IsIconic(hWnd))
                        {
                            ShowWindow(hWnd, SW_RESTORE);
                        }
                        SetForegroundWindow(hWnd);
                        break;
                    }
                }
            }
            finally
            {
                // GetProcessesByNameで確保された全プロセスインスタンスのWin32ハンドルを確実に解放する
                foreach (var process in processes)
                {
                    process.Dispose();
                }
            }
            return;
        }

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // 言語ファイルのリストを初期化
        LocalizationManager.Initialize();

        Application.Run(new MainForm());
    }    
}