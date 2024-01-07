using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Reveal.Sdk;


namespace RevealClient
{
    public partial class DashboardViewer : UserControl
    {
        private RevealView _revealView;
        private const string DashboardDirectory = "Dashboards";
        private const string DashboardExtension = ".rdash";
        private static string _defaultDirectory;

        public DashboardViewer()
        {
            InitializeComponent();

            RevealSdkSettings.EnableActionsOnHoverTooltip = true;
            _defaultDirectory = Path.Combine(Environment.CurrentDirectory, DashboardDirectory);
            _revealView = new RevealView();
            elementHost1.Child = _revealView;

            LoadDashboard("Sales");
            _revealView.SaveDashboard += RevealView_SaveDashboard;
        }

public static Stream GetDashboardStreamFromFile(string dashboardId)
{
    var path = Path.Combine(_defaultDirectory, $"{dashboardId}{DashboardExtension}");
    return File.Exists(path) ? File.OpenRead(path) : null;
}

        private void LoadDashboard(string dashboardId)
        {
            try
            {
                using (var stream = GetDashboardStreamFromFile(dashboardId))
                {
                    if (stream != null)
                    {
                        _revealView.HoverTooltipsEnabled = true;
                        _revealView.Dashboard = new RVDashboard(stream);
                    }
                    else
                    {
                        MessageBox.Show("Dashboard Not Found", "Error Loading Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Loading Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void RevealView_SaveDashboard(object sender, DashboardSaveEventArgs e)
        {
            try
            {
                var path = Path.Combine(_defaultDirectory, $"{e.Name}{DashboardExtension}");
                var data = await e.Serialize();

                using (var output = File.Open(path, FileMode.Create))
                {
                    await output.WriteAsync(data, 0, data.Length);
                }
                e.SaveFinished();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Saving Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
