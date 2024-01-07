using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reveal.Sdk.Data;
using Reveal.Sdk.Data.Microsoft.SqlServer;

namespace RevealClient.Reveal
{
    class DataSourceProvider : IRVDataSourceProvider
    {
        public Task<RVDataSourceItem> ChangeDataSourceItemAsync(RVDataSourceItem dataSourceItem)
        {
            if (dataSourceItem is RVSqlServerDataSourceItem sqlDsi)
            {
                if (sqlDsi.Id == "OrdersQry")
                {
                    sqlDsi.Table = "Orders Qry";
                }
                else dataSourceItem = null;
            }
            return Task.FromResult(dataSourceItem);
        }
    }
}
