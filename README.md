# EZImGui
Easy to use ImGui framework for .Net

## How To Use

Creating Panels/Menus

```
using ImGuiNet;

public class MyPanel : IPanel
{
    public void Render()
    {
        ImGui.ShowDemoWindow();
    }
}

public class MyMenu : IMenu
{
    public void RenderMenu()
    {
        if(ImGui.BeginMenu("File"))
        {
            if(ImGui.MenuItem("Some Item"))
                Logger.Debug("Some Item Clicked!");
            ImGui.EndMenu();
        }
    }
}

```

Setup your app
```
using EZImGui;
using ImGuiNet;

static class Program
{
    static void Main(string[] args)
    {
        // Create App
        App.CreateApp("My App Name", 800, 480);
        // Add Menus
        App.AddMenu(new MyMenu());
        // Add Panels
        App.AddPanel(new MyPanel());
        // Start the app
        App.Run();
    }
}

```

# Settings (In Development)

The framework includes a YAML based settings framework. If settings are enabled, they will be automatically loaded/saved on app start/end.

To implement your own settings, simply create a new class defining your settings, create a new AppSettings<T\> with your class

```
class MySettings
{
    public string SomeVar { get; set; }
    ...
}

```

Then in your main program:
```
AppSettings<MySettings> settings = new AppSettings<MySettings>();
```
Add any type converters (optional)
```
settings.AddTypeConverter(new MyTypeConverter());
```
Register your settings handler
settings.  (optional)
```
settings.HandleSettings = SettingsHandler;
```
Finally, add the settings class to your app:
```
App.AddSettings(settings);
```

You generally shouldn't need to add a SettingsHandler, but it may be useful for some special cases. Otherwise, you should be able to access your settings by calling the App.Settings.GetSettings() function, or by casting the App.Settings interface back to your type.

# Contributing

All contributions are welcome!