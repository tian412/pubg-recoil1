using System.Configuration;
using System.Data;
using System.Windows;

namespace pubg_recoil1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //应用程序启动时执行的事件
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //设置窗体的标题
            MainWindow mainWindow = new MainWindow();
            mainWindow.Title = "PUBG Recoil";
            mainWindow.Show();
        }
        //应用程序退出时执行的事件
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //释放资源
            ConfigurationManager.AppSettings.Clear();
            DataTable dt = new DataTable();
            dt.Clear();
        }
    }

}
