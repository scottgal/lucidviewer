using Avalonia;
using Avalonia.Data.Core.Plugins;

namespace MarkdownViewer;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        // Disable data validation to avoid IBinding errors from Markdown.Avalonia's StaticBinding
        // This removes the binding validation plugin that throws errors for custom IBinding implementations
        BindingPlugins.DataValidators.RemoveAt(0);

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}
