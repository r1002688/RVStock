using System.Windows;
using Velopack;

namespace RVStock
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            VelopackApp.Build().Run();

            base.OnStartup(e);

            
            _ = CheckForUpdatesAsync();
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
            catch
            {
                // Update server niet bereikbaar, gewoon verder
            }
        }
    }
}
