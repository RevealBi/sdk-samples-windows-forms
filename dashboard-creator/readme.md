
# Creating New Reveal Dashboards with SQL Server in Windows Forms

This guide provides step-by-step instructions on adding the Reveal Microsoft SQL Server data connector an exiting Windows Forms application that enables creating and editing dashboards.

This is Part 2 of a 2 part tutorial.  Part 1 is a set-by-step tutorial where you will learn how to load exisitng dashboards, edit those dashboards and save your edits to disk.

You can watch this step-by-step in YouTube.

[![image](https://github.com/RevealBi/sdk-samples-windows-forms/assets/18453092/a0238f64-92ae-41ef-a938-a97b1558f9be)](https://www.youtube.com/watch?v=rVV2Fmr4Qrw)


The desktop version of the Reveal SDK ships as a WPF package, when implementing in a Windows Forms app, you will use the WPF interoperability control Element.  

With the Reveal WPF SDK, you will be building a rich client application that handles all of the data access, authentication and querying to your database.

## Prerequisites

Before you start, you must complete Part 1: Adding Reveal Dashboards to Windows Forms Apps.

https://github.com/RevealBi/sdk-samples-windows-forms/blob/main/dashboard-viewer/readme.md

## Steps

### 1. Add NuGet Packages

In the previous tutorial, you added the prerequisite `Reveal.Wpf.Sdk` NuGet package.  In this tutorial, you’ll be connecting to a Microsoft SQL Server database.  To accomplish this:

- Right-click on your project and select Manage NuGet Packages.
- Click Browse In the NuGet Package Manager, and search for `Reveal.Sdk.Data.Sql`.
- Select the `Reveal.Sdk.Data.SqlServer` package and install it to your application. 

<img width="600" alt="image" src="https://github.com/RevealBi/sdk-samples-windows-forms/assets/18453092/2fee6c71-d90a-4609-9008-41bb62000a0f">


### 2. Register the Datasource

When you add a new datasource to a Reveal project, you need to register the datasource to make it available for your application. 

First, in Program.cs, add the using statements required for Reveal references:

```csharp
using Reveal.Sdk;
using Reveal.Sdk.Data;
```

in the `Main()` constructor in the `Program.cs` file, add the following code to register your newly added Microsoft SQL Server data connector:

```csharp
 RevealSdkSettings.DataSources.RegisterMicrosoftSqlServer();
```

### 3. Add DataSource Authentication

To provide authentication credentials to your data source, you must first create a class that implements the `IRVAuthenticationProvider` interface and implement the `ResolveCredentialsAsync` method.

1. Create a folder called Reveal in the root of your project
2. Add a new class named `AuthenticationProvider`.

In the AuthenticationProvider class, Add the necessary using statements:

```csharp
using Reveal.Sdk.Data;
using Reveal.Sdk.Data.Microsoft.SqlServer;
```
 
Implement the `IRVAuthenticationProvider` interface and add the default `ResolveCredentialsAsync` member with the following code:  

```csharp
public class AuthenticationProvider : IRVAuthenticationProvider
{
    public Task<IRVDataSourceCredential> ResolveCredentialsAsync(RVDashboardDataSource dataSource)
    {
        IRVDataSourceCredential userCredential = null;
        if (dataSource is RVSqlServerDataSource)
        {
            userCredential = new RVUsernamePasswordDataSourceCredential("user", "password");
        }
        return Task.FromResult<IRVDataSourceCredential>(userCredential);
    }
}
```

Change the “user” and “password” with your SQL Server credentials. Each time a request is made to your data source, the credentials in the `AuthenticationProvider` are retrieved 

The final step is to register the `AuthenticationProvider` with your application.  To do this, add the highlighted line to the `Main()` constructor in the `Program.cs` file in your application:

```csharp
static void Main()
{
	...

    // register AuthenticationProvider
    RevealSdkSettings.AuthenticationProvider = new Reveal.AuthenticationProvider();

	...
}
```

### 4. Add the DashboardCreator User Control

Add a new user control to your project named `DashboardCreator.`

<img width="400" alt="image" src="https://github.com/RevealBi/sdk-samples-windows-forms/assets/18453092/9d19208d-1054-46c9-a93b-d6cac10c1d82">


### 5. Add ElementHost Control

Add an `ElementHost` control to the `DashboardCreator` by dragging it from the Visual Studio Toolbox onto the UserControl.

In the newly added `ElementHost` control click the Dock in Parent Control option.

<img width="869" alt="image" src="https://github.com/RevealBi/sdk-samples-windows-forms/assets/18453092/b138421f-b949-4e78-8d13-4338b2d6faa8">


### 6. Implement DashboardCreator Code-Behind

Next you will add the code to implement the code in `DashboardCreator.cs` that enables creating and saving new dashboards.

#### a. Add Using Statement

In `DashboardCreator.cs` add the required namespaces:

```csharp
using System.IO;
using Reveal.Sdk;
using Reveal.Sdk.Data;
using Reveal.Sdk.Data.Microsoft.SqlServer;
```

#### b. Define Variables

Add the following class variables:

```csharp
private RevealView _revealView;
private const string DashboardDirectory = "Dashboards";
private const string DashboardExtension = ".rdash";
private static string _defaultDirectory;
```

In the `DashboardViewer` constructor, after the `InitializeComponent` block of code, add the following.

```csharp
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
```

#### c. Add the SQL Server Data Source Code

In the `InitializeComponent`, add an event handler for the `RevealView.DataSourcesRequested` event.

```
_revealView.DataSourcesRequested += RevealView_DataSourcesRequested;
```

In the event handler, define two collections; one for the data sources, and one for the data source items. These two collections are used a parameters to the `RevealDataSources` object which is provided in the event handler callback.

```objc
private void RevealView_DataSourcesRequested(object sender, DataSourcesRequestedEventArgs e)
{
    var dataSources = new List<RVDashboardDataSource>();
    var dataSourceItems = new List<RVDataSourceItem>();

...

    e.Callback(new **RevealDataSources**(dataSources, dataSourceItems, false));
}
```

Next, in the same `RevealView.DataSourcesRequested` event handler, create a new instance of the `RVSqlServerDataSource` object. Set the `Host`, `Database`, `Port`, and `Title` properties to values that correspond to your Microsoft SQL Server.  In my demos, I will be using an on-premise Microsoft SQL Server installation with the Northwind database.

After you have created the `RVSqlServerDataSource` object, add it to the data sources collection.

```objc
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

    e.Callback(new RevealDataSources(dataSources, dataSourceItems, false));
}
```

At this point, all of the data access code required to connect to your Microsoft SQL Server and create dashboards is complete. In the next step, you’ll add the code to save the dashboard once you design your dashboards.

#### d. Implement Save Dashboard Event Handler

In the `InitializeComponent`, add an event handler for the `RevealView.SaveDashboard` event.

```
_revealView.SaveDashboard += RevealView_SaveDashboard;
```

During  the `Save` operation, you start by getting the path of the .rdash file we are creating. Since the name of the .rdash file should match the Title of the dashboard, you use the `e.Name` to build the path. Once we have the path, we can then use the `e.Serialize()` method to obtain the byte array of the current dashboard. Once we have the byte array of the dashboard, we can create a file stream and save it to disk.

```csharp

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
```

When adding a new dashboard in Reveal, the default filename is “New Dashboard”.  To force the user to give the dashboard a proper name add this code before the try block:

```csharp
if (e.Name == "New Dashboard")
{
    MessageBox.Show("Please Enter a New Dashboard Name", "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    return;
}
```

There are multiple approaches to saving dashboard files.  The best place to learn how to implement `Save` and `Save As` is in this help topic:

[https://help.revealbi.io/wpf/saving-dashboards](https://help.revealbi.io/wpf/saving-dashboards) 

### 9. Add the DashboardCreator UserControl to a Form

At this point in the tutorial, your UserControl is ready to test.  Start by building your project to make sure there are no errors. 

To add you DashboardCreator UserControl to a Form, do the following:

1. Right-Click on your project, click **Add**, the select **Form (Windows Form)**.
2. Leave the default name as **Form2.cs** and click the **Add** button.
3. From the Toolbox, search for DashboardCreator, and drag it onto the Form2.cs design surface.
4. Select the newly added control and set its `Dock` property to `Fill`.

Your form should look like this:

<img width="700" alt="image" src="https://github.com/RevealBi/sdk-samples-windows-forms/assets/18453092/a9bdcd05-efb9-4bdc-9aad-fd47ca3643f5">


### 10. Run Form2

Open the `Program.cs` and change the `Form1` to `Form2` in `Application.Run`:

```csharp
static void Main()
{
    RevealSdkSettings.DataSources.RegisterMicrosoftSqlServer();
    RevealSdkSettings.AuthenticationProvider = new Reveal.AuthenticationProvider();

    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new Form2());
}
```

Click the Start button or hit F5 to run the application.  When the application runs, create a new Visualization and you will see the newly created Microsoft SQL Server data source listed in the "Select a Data Source" dialog.

Your experience should match the same as in this video.

![Create New Dashboard](https://github.com/RevealBi/sdk-samples-windows-forms/assets/18453092/24392d01-58a8-42a2-acd3-4248fbad8a89)


### 11. Adding Curated DataSourceItems for Dashboard Creation

In the previous step, you set up the data source connection to your database and you were able to create a dashboard by accessing your entire database. However, most deployments require on a handful of curated data source items that you use to create dashboards or that you present to your end-user to create dashboards.

To do this, you implement  the `DataSourceProvider` interface with the `ChangeDataSourceItemAsync` function, and you create specific data source items by specifying one of the following:

- Table
- CustomQuery
- Stored Procedure

If you follow the approach in this help topic - [https://help.revealbi.io/wpf/adding-data-sources/ms-sql-server](https://help.revealbi.io/wpf/adding-data-sources/ms-sql-server) - you’ll see that you can simply add data source items directly in the `DataSourcesRequested` event.  However, as your application gets more complex, and you have multiple data source items, this can get verbose, and messy.  We’ll simply this in the next few steps.

#### Add & Register the DataSourceProvider

First you need to add a class that will implement the necessary interface to perform your data operations. 

1. In the Reveal folder, add a new class named `DataSourceProvider`.

In the DataSourceProvider class, Add the necessary using statements:

```csharp
using Reveal.Sdk.Data;
using Reveal.Sdk.Data.Microsoft.SqlServer;
```
 
Implement the `IRVDataSourceProvider` interface and add the default `ChangeDataSourceItemAsync` member with the following code:  

```csharp

namespace RevealDashboardApp.Reveal
{
    class DataSourceProvider : IRVDataSourceProvider
    {
        public Task<RVDataSourceItem> ChangeDataSourceItemAsync(RVDataSourceItem dataSourceItem)
        {
            return Task.FromResult(dataSourceItem);
        }
    }
}
```

Now that the default implementation is added, you’ll add a block of code that does 3 things:

1. Checks the the incoming request is from a Microsoft SQL Server data source
2. Checks the `Id` property of the incoming request and matches it to a data operation 
3. Returns a `null` if the `Id` is invalid

Update the `ChangeDataSourceItemAsync` with this code.

```csharp

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
```

Note, I am using the Northwind database in this example. Change the `Table` property to a valid table name in your database.  

The final step is to register the DataSourceProvider with your application.  To do this, add the highlighted line to the `Main` constructor in the `Program.cs` file in your application:

```csharp
static void Main()
{
    RevealSdkSettings.DataSources.RegisterMicrosoftSqlServer();
    RevealSdkSettings.AuthenticationProvider = new Reveal.AuthenticationProvider();
    RevealSdkSettings.DataSourceProvider = new Reveal.DataSourceProvider();

	...
}
```

The next step is to add the data operation request to the `DataSourcesRequsted` event in the DashboardCreator UserControl.

#### Add Data Source Items

In the DataSourcesRequested event, you’ll remember the callback that presents the DataSources and the DataSourceItems:

```csharp
e.Callback(new RevealDataSources(dataSources, dataSourceItems, false));
```

Up until now, the dataSourceItems was empty, which means when you add a new Visualization, you’ll only see what is available in the dataSources array.  To add curated data source items, you’ll populate the dataSourceItems array which represents the list of queries available to the dashboard designer.

First, we’ll add a function that populates the `RVDataSourceItem` list.  Add this to the `DashboardCreator.cs` code behind:

```csharp
void AddSqlServerItem(List<RVDataSourceItem> itemList, RVSqlServerDataSource dataSource, string id, string title)
{
    var sqlDsi = new RVSqlServerDataSourceItem(dataSource)
    { Id = id, Title = title };
    itemList.Add(sqlDsi);
}
```

Next we’ll update the `RevealView_DataSourcesRequested` event with the highlighted code:

```csharp
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
```

In this case, we are only adding a single dataSourceItem, however, we’ll add more in a minute.

Click the Start button or hit F5 to run the application.  When the application runs, create a new Visualization and you will see the newly created Microsoft SQL Server data source item called **All Customer Orders** listed in the "Select a Data Source" dialog.

![](DraggedImage-3.png)

#### Add Additional DataSourceItems

There is no limit to the number of data connections (DataSources) or `DataSourceItems` you can add to the `RevealDataSources` call back.

Here is updated code in the `ChangeDataSourceItemAsync` function that executes different data operations based on the incoming Id:

```csharp
public Task<RVDataSourceItem> ChangeDataSourceItemAsync(RVDataSourceItem dataSourceItem)
{
    if (dataSourceItem is RVSqlServerDataSourceItem sqlDsi)
    {
        if (sqlDsi.Id == "TenMostExpensiveProducts")
        {
            sqlDsi.Procedure = "Ten Most Expensive Products";
        }

        else if (sqlDsi.Id == "Customers")
        {
            sqlDsi.CustomQuery = "Select * from Customers";
        }

        else if (sqlDsi.Id == "OrdersQry")
        {
            sqlDsi.Table = "Orders Qry";
        }

        else if (dataSourceItem.Id == "OrdersByEmployee")
        {
            sqlDsi.Procedure = "OrdersByEmployee";
            sqlDsi.ProcedureParameters = new Dictionary<string, object>
                {
                    { "@EmployeeID", 2 }
                };
        }
        else dataSourceItem = null;
    }
    return Task.FromResult(dataSourceItem);
}
```

Next I updated the `RevealView_DataSourcesRequested` event with the additional data source items:

```csharp
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

	// Note I am commenting out the next line of code, 
	// so the dataSources array will remain empty
    //dataSources.Add(sqlDataSource);

	AddSqlServerItem(dataSourceItems, sqlDataSource, "TenMostExpensiveProducts", "Ten Most Expensive Products");
	AddSqlServerItem(dataSourceItems, sqlDataSource, "Customers", "Customers");
	AddSqlServerItem(dataSourceItems, sqlDataSource, "OrdersQry", "All Customer Orders");
	AddSqlServerItem(dataSourceItems, sqlDataSource, "OrdersByEmployee", "Orders by Employee");

    e.Callback(new RevealDataSources(dataSources, dataSourceItems, false));
}
```

When you run this, and click the New Visualization button, you should see the four curated data items, and the DataSource is not present.  You are not required to show a full data source, the information in the `RVSqlServerDataSource` is required to connect to the database, but it is not required to display and make available to your users.

![](DraggedImage-4.png)

## Conclusion

Following these steps, you will have a Windows Forms application capable of creating and saving new Reveal dashboards.  For more information on data sources, and the features of the `RevealView`, please review the product documentation located here:

[https://help.revealbi.io/wpf/getting-started](https://help.revealbi.io/wpf/getting-started)

