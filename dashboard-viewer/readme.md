
# Adding Reveal Dashboards to Windows Forms Apps 

This guide provides step-by-step instructions to integrate Reveal SDK into a Windows Forms application for handling dashboards. It covers the setup of necessary NuGet packages, dashboard management, and implementation of a user control for dashboard viewing.

The Reveal SDK ships as a WPF package, when implementing in a Windows Forms app, you will use the WPF interoperability control Element

## Prerequisites

Before you start, ensure you have Visual Studio 2019 installed with support for Windows Forms applications in C#.

## Steps

### 1. Add NuGet Packages

First, add the following NuGet packages to your project:
- Reveal WPF NuGet Package

If you're migrating from `packages.config`, update your package management format accordingly.

### 2. Prepare Dashboard Folder

Create a folder named `Dashboards` in your project directory. This folder will store the `.rdash` dashboard files.

### 3. Download and Add Sample Dashboards

Download sample dashboards and add them to the `Dashboards` folder.
[https://users.infragistics.com/Reveal/Dashboards/dashboard-samples.zip](https://users.infragistics.com/Reveal/Dashboards/dashboard-samples.zip)

You should have a Sales.rdash and a Marketing.rdash in the .Dashboards folder in your project.  Note that dashboard file names are not case-sensitive.

Once you add the sample dashboards to your project, make sure to set their properties to:

Build Action: Content
Copy to Output Directory: Copy if Newer

![](DraggedImage.png)
### 4. Register Data Sources and Providers

In this example, you are only viewing sample dashboards that ship with Reveal. These samples are built using data stored in the cloud in a Microsoft Excel spreadsheet, so there is no specific data source or authentication  code required to complete this tutorial.

In your code, register Microsoft SQL Server as a data source and set authentication and data source providers:

```csharp
RevealSdkSettings.DataSources.RegisterMicrosoftSqlServer();
RevealSdkSettings.AuthenticationProvider = new Reveal.AuthenticationProvider();
RevealSdkSettings.DataSourceProvider = new Reveal.DataSourceProvider();
```

### 5. Create a DashboardViewer User Control

Add a new user control to your project named `DashboardViewer`.

### 6. Add ElementHost Control

In the `DashboardViewer` control, add an `ElementHost` control to host the WPF Reveal view.

### 7. Test Run

Run the project to ensure everything is set up correctly so far.

### 8. Implement DashboardViewer Code-Behind

In `DashboardViewer.cs`:

#### a. Add Using Statement

Include the Reveal SDK namespace:

```csharp
using Reveal.Sdk;
```

#### b. Define Variables

Add the following class variables:

```
private RevealView _revealView;
private const string DashboardDirectory = "Dashboards";
private const string DashboardExtension = ".rdash";
private readonly string _defaultDirectory;
```


And in the DashboardViewer constructor, add the code to initialize these new variables:

```
RevealSdkSettings.EnableActionsOnHoverTooltip = true;
_defaultDirectory = Path.Combine(Environment.CurrentDirectory, DashboardDirectory);
_revealView = new RevealView();
elementHost1.Child = _revealView;
```

#### c. Implement GetDashboardStreamFromFile

Add a method to read a dashboard file and return a stream:

```csharp
public static Stream GetDashboardStreamFromFile(string dashboardId)
{
    var path = Path.Combine(Environment.CurrentDirectory, DashboardDirectory, $"{dashboardId}{DashboardExtension}");
    return File.Exists(path) ? File.OpenRead(path) : null;
}
```

#### d. Implement Load and Save Dashboard Functions

Add methods to load and save dashboards:

```csharp

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
```

Next add the SaveDashboard function:

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

### 9. Load Dashboard and Handle Save Event

In your main form's constructor, load a default dashboard and set up the save event:

```csharp
LoadDashboard("Campaigns");
_revealView.SaveDashboard += RevealView_SaveDashboard;
```

### 10. Final Steps

Build the project to ensure there are no errors. Then, in your main form (`Form1`), set it’s WindowState property to Maximized, add the `DashboardViewer` component, set it to dock in the form, and run the project. You should see the loaded dashboard.

## Conclusion

Following these steps, you will have a Windows Forms application capable of loading and saving Reveal dashboards. The application will start by displaying a default dashboard and allows saving any modifications back to the file.

