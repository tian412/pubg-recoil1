using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using static pubg_recoil1.Program;

namespace pubg_recoil1
{
    public partial class MainWindow : System.Windows.Window
    {
        private System.Windows.Controls.Image resultImage;

        public MainWindow()
        {
            resultImage = (System.Windows.Controls.Image)FindName("resultImage");
            resultImage1 = (System.Windows.Controls.Image)FindName("resultImage1");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 创建 CompareImage 类的实例
            var compareImage = new CompareImage();

            // 定义输出参数
            List<BitmapImage> resultImages;

           

            // 调用截图方法
            var mats = compareImage.CaptureScreenArea( out resultImages);

            // 显示截图结果
            if (resultImages.Count > 0)
            {
                resultImage.Source = resultImages[0];
                if (resultImages.Count > 1)
                {
                    resultImage1.Source = resultImages[1];
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // 执行设置
            // 设置截图间隔
            // 设置识别阈值
            // 示例：设置 TextBox 的内容
            var textBox = (TextBox)FindName("TextBoxName"); // 替换为实际的 TextBox 名称
            textBox.Text = "0.7"; // 示例值
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            // 执行识别
            // 识别截图
            // 显示识别结果
            // 保存识别结果
            // 示例：调用识别方法
            var compareImage = new CompareImage();
            var result = compareImage.PerformRecognition();
            // 显示识别结果
            MessageBox.Show($"识别结果: {result}");
            // 保存识别结果
            File.WriteAllText("recognition_result.txt", result);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            // 执行退出
            this.Close();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    var textBox = (TextBox)FindName("TextBoxName"); // 替换为实际的 TextBox 名称
                    textBox.Text = "0.7";
                    break;
            }
        }

        private BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}