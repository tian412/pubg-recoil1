using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Eventing.Reader;
using OpenCvSharp;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;
using Emgu.CV.Reg;
using System.Resources;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Threading;


namespace pubg_recoil1
{
    //预加载图片
    class Program
    {

        public class ImageLoader
        {
            public static List<System.Drawing.Image> LoadImagesFromFolder(string folderPath)
            {
                List<System.Drawing.Image> images = new List<System.Drawing.Image>();
                // 获取文件夹中所有文件的路径
                string[] filePaths = Directory.GetFiles(folderPath);

                // 遍历文件路径
                foreach (string filePath in filePaths)
                {
                    // 获取文件扩展名
                    string extension = System.IO.Path.GetExtension(filePath).ToLower();

                    // 判断文件扩展名是否为常见图片格式
                    if (extension == ".jpg" || extension == ".jpeg" ||
                        extension == ".png" || extension == ".bmp"
                      )
                    {
                        try
                        {
                            // 使用Image.FromFile方法加载图片
                            System.Drawing.Image image = System.Drawing.Image.FromFile(filePath);
                            images.Add(image);
                        }
                        catch (Exception ex)
                        {
                            // 处理加载图片时可能出现的异常
                            Console.WriteLine($"加载图片{filePath}时出错：{ex.Message}");
                        }
                    }
                }

                return images;
            }
        }

        // 使用示例
        /* string folderPath = @"Pack://application:,,,/Resources/grips/";
         string folderPath1 = @"Pack://application:,,,/Resources/muzzles/";
         string folderPath2 = @"Pack://application:,,,/Resources/poses/";
         string folderPath3 = @"Pack://application:,,,/Resources/scopes/";
         string folderPath4 = @"Pack://application:,,,/Resources/stocks/";
         string folderPath5 = @"Pack://application:,,,/Resources/weapons/";
         List<System.Drawing.Image> grips = ImageLoader.LoadImagesFromFolder(@"Pack://application:,,,/Resources/grips/");
         List<System.Drawing.Image> muzzles = ImageLoader.LoadImagesFromFolder(@"Pack://application:,,,/Resources/muzzles/");
         List<System.Drawing.Image> poses = ImageLoader.LoadImagesFromFolder(@"Pack://application:,,,/Resources/poses/");
         List<System.Drawing.Image> scopes = ImageLoader.LoadImagesFromFolder(@"Pack://application:,,,/Resources/scopes/");
         List<System.Drawing.Image> stocks = ImageLoader.LoadImagesFromFolder(@"Pack://application:,,,/Resources/stocks/");
         List<System.Drawing.Image> weapons = ImageLoader.LoadImagesFromFolder(@"Pack://application:,,,/Resources/weapons/");
        */
        // 此时images列表中包含了文件夹中的所有图片对象


        public class CompareImage
        {
            //获取屏幕分辨率
            string currentResolution = ResolutionSettings.GetScreenResolution();

            public CompareImage()
            {
                Console.WriteLine($"当前屏幕分辨率: {currentResolution}");
            }

            //从resolustion_setting中拿到截图坐标
            public List<System.Drawing.Rectangle> GetScreenAreas(string resolution)
            {
                var settings = ResolutionSettings.GetAllResolutionSettings(resolution);
                return settings.Values.Select(t => new System.Drawing.Rectangle(t.Item1, t.Item2, t.Item3, t.Item4)).ToList();
            }
            //将坐标传给截图方法，截图并转换为Mat
            // 截取屏幕指定区域并转换为灰度图像
            public  List<OpenCvSharp.Mat> CaptureScreenArea(List<System.Drawing.Rectangle> areas, out List<System.Windows.Media.Imaging.BitmapImage> resultImages)
            {
                List<OpenCvSharp.Mat> matList = new List<OpenCvSharp.Mat>();
                resultImages = new List<System.Windows.Media.Imaging.BitmapImage>();

                foreach (var area in areas)
                {
                    var img = new Bitmap(area.Width, area.Height);

                    using (var g = Graphics.FromImage(img))
                    {
                        g.CopyFromScreen(area.Location, System.Drawing.Point.Empty, area.Size);
                    }

                    // 转换为 Mat（OpenCV 格式）并转换为灰度图像
                    var mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(img);
                    Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY);
                    matList.Add(mat);

                    // 显示在窗口中
                    using (var memory = new MemoryStream())
                    {
                        img.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                        memory.Position = 0;
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        resultImages.Add(bitmapImage);
                    }
                }

                return matList;
            }

            // 添加 TestImageComparison 方法
            public async Task TestImageComparison(string gripsPath, string muzzlesPath, string posesPath, string scopesPath, string stocksPath, string weaponsPath)
            {
                // 示例方法体，可以根据实际需求进行修改
                List<System.Drawing.Image> grips = ImageLoader.LoadImagesFromFolder(gripsPath);
                List<System.Drawing.Image> muzzles = ImageLoader.LoadImagesFromFolder(muzzlesPath);
                List<System.Drawing.Image> poses = ImageLoader.LoadImagesFromFolder(posesPath);
                List<System.Drawing.Image> scopes = ImageLoader.LoadImagesFromFolder(scopesPath);
                List<System.Drawing.Image> stocks = ImageLoader.LoadImagesFromFolder(stocksPath);
                List<System.Drawing.Image> weapons = ImageLoader.LoadImagesFromFolder(weaponsPath);

                // 进行图像比较的逻辑
                await Task.Run(() =>
                {
                    // 示例：输出加载的图像数量
                    Console.WriteLine($"Grips: {grips.Count}, Muzzles: {muzzles.Count}, Poses: {poses.Count}, Scopes: {scopes.Count}, Stocks: {stocks.Count}, Weapons: {weapons.Count}");
                });
            }
        }


    }
}

