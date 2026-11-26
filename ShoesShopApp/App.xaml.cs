using System.Windows;
using ShoesShopApp.Services;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        MappingConfig.Configure();   // <-- добавить
    }
}