using RVStock.Data;
using System.Windows;
using Velopack;

namespace RVStock
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            VelopackApp.Build()
                .WithFirstRun(v =>
                {
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName;
                    CreateShortcut(
                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "RVStock.lnk"),
                        exePath, "RVStock Stockbeheer");
                    CreateShortcut(
                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "RVStock.lnk"),
                        exePath, "RVStock Stockbeheer");
                })
                .Run();

            // Database aanmaken bij eerste opstart
            using (var db = new StockContext())
            {
                db.Database.EnsureCreated();
            }

            base.OnStartup(e);
            _ = CheckForUpdatesAsync();
        }

        private static void CreateShortcut(string shortcutPath, string targetPath, string description)
        {
            try
            {
                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;
                dynamic shell = Activator.CreateInstance(shellType)!;
                var shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
                shortcut.Description = description;
                shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(targetPath);
                shortcut.Save();
            }
            catch { }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var updateUrl = "https://github.com/r1002688/RVStock/releases/latest/download";
                var mgr = new Velopack.UpdateManager(updateUrl);
                if (!mgr.IsInstalled) return;

                var update = await mgr.CheckForUpdatesAsync();
                if (update == null) return;

                var result = MessageBox.Show(
                    $"Er is een nieuwe versie beschikbaar ({update.TargetFullRelease.Version}).\nNu bijwerken?",
                    "Update beschikbaar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    await mgr.DownloadUpdatesAsync(update);
                    mgr.ApplyUpdatesAndRestart(update);
                }
            }
            catch { }
        }
    }
}
