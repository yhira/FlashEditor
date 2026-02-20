namespace FlashEditor;

static class Program
{
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
            return;
        }

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }    
}