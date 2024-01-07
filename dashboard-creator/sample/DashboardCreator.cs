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
using Reveal.Sdk.Data;
using Reveal.Sdk.Data.Microsoft.SqlServer;


namespace RevealClient
{
    public partial class DashboardCreator : UserControl
    {
        private RevealView _revealView;
        private const string DashboardDirectory = "Dashboards";
        private const string DashboardExtension = ".rdash";
        private static string _defaultDirectory;

        public DashboardCreator()
        {
            InitializeComponent();
            // use the new hover-tooltip feature
            RevealSdkSettings.EnableActionsOnHoverTooltip = true;

            // set the load/save directory
            _defaultDirectory = Path.Combine(Environment.CurrentDirectory, DashboardDirectory);

            // add and instantiate the _revealView variable
            _revealView = new RevealView();

            // set the Child property of the element host to contain your new _revealView dashboard
            elementHost1.Child = _revealView;

            // force the RevealView to start in Edit mode
            _revealView.StartInEditMode = true;

            // set the Dashboard of the _revealView to a new instance of an RVDashboard
            _revealView.Dashboard = new RVDashboard();

            _revealView.DataSourcesRequested += RevealView_DataSourcesRequested;

            _revealView.SaveDashboard += RevealView_SaveDashboard;
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

        private void RevealView_DataSourcesRequested(object sender, DataSourcesRequestedEventArgs e)
        {
            var dataSources = new List<RVDashboardDataSource>();
            var dataSourceItems = new List<RVDataSourceItem>();

            var sqlDataSource = new RVSqlServerDataSource()
            {
                Host = "your-sql-server-name-or-IP",
                Database = "your-database",
                Title = "My SQL Server",
            };
            dataSources.Add(sqlDataSource);

            AddSqlServerItem(dataSourceItems, sqlDataSource, "OrdersQry", "All Customer Orders");

            e.Callback(new RevealDataSources(dataSources, dataSourceItems, false));
        }

        void AddSqlServerItem(List<RVDataSourceItem> itemList, RVSqlServerDataSource dataSource, string id, string title)
        {
            var sqlDsi = new RVSqlServerDataSourceItem(dataSource)
            { Id = id, Title = title };
            itemList.Add(sqlDsi);
        }
    }
}
