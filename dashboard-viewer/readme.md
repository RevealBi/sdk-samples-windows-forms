# Adding Reveal Dashboards to Windows Forms Apps 

This guide provides step-by-step instructions to integrate Reveal SDK into a Windows Forms application for viewing, editing and saving dashboards. It covers the setup of necessary NuGet packages, loading existing dashboards, and saving dashboards once they are edited and changed.

This is part 1 of a 2 part tutorial.  Part 2 is a set-by-step tutorial where you will learn how to create new dashboards from scratch with data from a Microsoft SQL Server database.

The desktop version of the Reveal SDK ships as a WPF package, when implementing in a Windows Forms app, you will use the WPF interoperability control Element.  

With the Reveal WPF SDK, you will be building a rich client application that handles all of the data access, authentication and querying to your database.

## Prerequisites

Before you start, ensure you have Visual Studio installed with support for Windows Forms applications in C# in the latest version of the .NET Framework.

## Steps

### 1. Create a New Visual Studio Project

1 - In Visual Studio, create a new Windows Forms project using the latest .NET Framework version and name it RevealClient.

### 2. Add Reveal NuGet Packages

The first thing you need to do is light up this application with Reveal. You do that by adding the Reveal NuGet package for WPF. 

1 - Right click on your project Project, and select **Manage NuGet Packages** for Solution.

2 - In the package manager dialog, open the **Browse** tab, and install the **Reveal.Sdk.Wpf** NuGet package into the project.

Accept all the defaults on the install dialog for the NuGet package. At this point, the core WPF Reveal SDK is installed in your application.


### 3. Migrate Packages.config

When installing NuGet packages in a Windows Forms or WPF application, you may need to migrate the  `packages.config` file that is installed when you install the NuGet Package.  If there is a  `packages.config` file in the root of your project:

1. Right-click on the `packages.config` file and select Migrate packages.config to PackageReference.

2. Select the Top-Level checkbox in the Migration dialog, then click OK.

After a successful migration, a report is generated and displayed in your default browser.  You can close this assuming there are no errors in the migration.

3. Build your application to ensure there are no errors.

### 2. Add Your Trial License Key

Reveal requires a license key, either Trial or Paid, to run.  If you are in a Reveal trial, your license key is sent to you via email when you download the Reveal SDK here:

[https://www.revealbi.io/download-sdk](https://www.revealbi.io/download-sdk)

Follow this help document to enable your application with your license key:  
  
[https://help.revealbi.io/wpf/adding-license-key](https://help.revealbi.io/wpf/adding-license-key)

### 3. Add Sample Dashboards

To complete this tutorial, you will need a sample dashboard to load into the `RevealView`.  You will not be creating dashboards from scratch, only loading local dashboards, editing and saving.  In part 2 of this tutorial, you will be creating dashboards from scratch.

To set up your app with sample dashboards:

1. Download this zip file [https://users.infragistics.com/Reveal/Dashboards/dashboard-samples.zip](https://users.infragistics.com/Reveal/Dashboards/dashboard-samples.zip)
2. Create a folder called `Dashboards` in the root of your project.
3. Unzip the dashboard-samples.zip file and copy the content to the newly created Dashboards folder.

You should have a Sales.rdash and a Marketing.rdash in the `.\Dashboards` folder in your project.  Note that dashboard file names are not case-sensitive.

The `.\Dashboards` folder is where you will load and save the Reveal dashboard files to.  Reveal dashboards are saved in a `JSON` format.  The `JSON` file is simply a `.zip` file renamed with the `.rdash` extension. 

4. Select the Marketing.rdash and Sales.rdash and change their **Build Action** and **Content to Output Directory** properties to `Content` and `Copy if Newer` as this screenshot demonstrates.

This ensures that when working with dashboard files in debug mode, they are loaded and saved from the correct folder in `./bin/debug/dashboards.`
 
### 5. Create a DashboardViewer User Control

The `RevealView` component  will be holder in a UserControl. 

1.  Right-click on your project and select New -\> User Control (Windows Forms) and change the name to `DashboardViewer` then click OK.

### 6. Add ElementHost Control

The `DashboardViewer` will host the `RevealView` component which is the core  Reveal dashboard component.  However, since this is a WPF component, and you are building a Windows Forms application, you will use the `ElementHost` control to host the `RevealView` WPF control in your Windows Forms app.

1. Open the Toolbox in Visual Studio and search for the ElementHost control
2. Drag or Double-click the `ElementHost` other add it to the `DashboardViewer` control. 
3. Click the `Dock in Parent Control` link to dock the control appropriately in the UserControl.

### 8. Implement DashboardViewer Code-Behind

All of the code to load and save dashboards will be in the `DashboardViewer.cs` code file.

The next steps guide you through the steps to complete the project.

#### a. Add Using Statement

Add the Reveal.Sdk and the System.IO namespaces to the top of your `DashboardViewer.cs` class file.

```csharp
using System.IO;
using Reveal.Sdk;
```

#### b. Define Variables

Add the following class-level variables:

```csharp
private RevealView _revealView;
private const string DashboardDirectory = "Dashboards";
private const string DashboardExtension = ".rdash";
private static string _defaultDirectory;
```

#### c. Initialize Variables and Setup the RevealView

In the `DashboardViewer` constructor, after the `InitializeComponent` block of code, add the following.  

```csharp
// use the new hover-tooltip feature
RevealSdkSettings.EnableActionsOnHoverTooltip = true;

// set the load/save directory
_defaultDirectory = Path.Combine(Environment.CurrentDirectory, DashboardDirectory);

// add and instantiate the _revealView variable
_revealView = new RevealView();

// set the Child property of the element host to contain
// you new _revealView dashboard
elementHost1.Child = _revealView;
```

#### d. Implement GetDashboardStreamFromFile

Add a method called `GetDashboardStreamFromFile` to read a dashboard file and return a stream.  This stream is returned to the calling function to load the dashboard into the `RevealView`.

```csharp
public static Stream GetDashboardStreamFromFile(string dashboardId)
{
    var path = Path.Combine(_defaultDirectory, $"{dashboardId}{DashboardExtension}");
    return File.Exists(path) ? File.OpenRead(path) : null;
}
```

#### e. Add the LoadDashboard Function

Loading a dashboard in Reveal only takes a single line of code.  You load the stream from the `.rdash` file into the `RevealView` component.  
  
_`revealView.Dashboard = new RVDashboard(stream);`
  
However, since you are dealing with files on the file system, you require a few more checks to ensure the file exists and that it loads properly.  Add the following `LoadDashboard` function to handle loading the dashboard file passed as the `dashboardId` property into the `RevealView`.

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

#### e. Add the SaveDashboard Event 

When a user clicks the check-box button in the upper-right of a dashboard when it is in edit more, the `SaveDashboard` event is fired.  There are various strategies you may employ to handle saving dashboards:  

- Overwrite the existing dashboard
- Save As a new filename
- Save to a different folder
- Etc. etc.

Refer to the Saving Dashboards topic to learn more about saving dashboard files: [https://help.revealbi.io/wpf/saving-dashboards](https://help.revealbi.io/wpf/saving-dashboards).

In this sample, you are overwriting the dashboard that is currently loaded in the `RevealView`.  Add the following `RevealView_SaveDashboard` code:  

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

In your constructor, after the `elementHost1.Child = _revealView;`line of code, add your call to the `LoadDashboard` function, and add code to handle the `SaveDashboard` event on the `RevealView`.
 
```csharp
// Load the dashbaord when the user control is initialized
LoadDashboard("Sales");

// handle the Save event
_revealView.SaveDashboard += RevealView_SaveDashboard;
```

At this point, all the necessary code is written to load, edit and save Reveal dashboards in your UserControl.

### 10. Add the DashboardViewer to Form1

To run your application, add the DashboardViewer UserControl to the default Form1 that was added to your project.  Follow these steps:

1. Build the project to ensure there are no errors. 
2. Open `Form1` in the designer by double-clicking on it in your Solution Explorer.
3. With `Form1` open in the designer, select it and set it’s **WindowState **property to `Maximized`.
4. Search for the `DashboardViewer` component in the Toolbox, and drag it onto `Form1`.
5. With the `DashboardViewer` selected, set its **Dock** property to `Fill`.

Note, you may get a **Dashboard Not Found Error** when you add `DashboardViewer`  to `Form1`.  This won’t happen after you run the application once, as the dashboards will be copied to the bindebug folder.

### Run the Project

Finally, hit F5 or click the Start button to run your project.  You should see the Sales dashboard loaded successfully!


### Conclusion
Next, follow the steps in Part 2 of this Tutorial, Creating New Reveal Dashboards with SQL Server in Windows Forms, to connect your Microsoft SQL Server data to Reveal and create new dashboards that can be used by this DashboardViewer UserControl you built in this tutorial.  For more information on data sources, and the features of the `RevealView`, please review the product documentation located here:

[https://help.revealbi.io/wpf/getting-started](https://help.revealbi.io/wpf/getting-started)

