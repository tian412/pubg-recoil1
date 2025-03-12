using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Windows.Forms; // 添加对 Windows Forms 的引用

namespace pubg_recoil1
{
    // 加载图片
    public class ImageLoader
    {
        public static Dictionary<string, List<Tuple<Bitmap, string>>> LoadImagesFromFolders(string rootFolderPath)
        {
            Dictionary<string, List<Tuple<Bitmap, string>>> imagesDict = new Dictionary<string, List<Tuple<Bitmap, string>>>();
            List<string> failedFiles = new List<string>(); // 记录失败的图片路径

            string[] folderPaths = Directory.GetDirectories(rootFolderPath);

            foreach (string folderPath in folderPaths)
            {
                string folderName = Path.GetFileName(folderPath);
                List<Tuple<Bitmap, string>> images = new List<Tuple<Bitmap, string>>();
                string[] filePaths = Directory.GetFiles(folderPath);

                foreach (string filePath in filePaths)
                {
                    string extension = Path.GetExtension(filePath).ToLower();
                    if (extension == ".png" || extension == ".bmp")
                    {
                        try
                        {
                            using (Bitmap image = new Bitmap(filePath))
                            {
                                images.Add(Tuple.Create(new Bitmap(image), Path.GetFileNameWithoutExtension(filePath))); // 添加副本和文件名

                                // 显示加载的图像
                              /*  using (Mat mat = BitmapConverter.ToMat(image))
                                {
                                    Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY);
                                    Cv2.ImShow($"Loaded Image - {Path.GetFileName(filePath)}", mat);
                                    Cv2.WaitKey(10); // 按下任意键关闭窗口
                                    Cv2.DestroyWindow($"Loaded Image - {Path.GetFileName(filePath)}");
                                }*/
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"加载图片 {filePath} 时出错：{ex.Message}");
                            failedFiles.Add(filePath); // 记录失败的文件路径
                        }
                    }
                }

                if (images.Count > 0)
                {
                    imagesDict[folderName] = images;
                }
            }

            // 重新加载失败的文件
            ReLoadFailedImages(failedFiles, imagesDict);

            return imagesDict;
        }

        private static void ReLoadFailedImages(List<string> failedFiles, Dictionary<string, List<Tuple<Bitmap, string>>> imagesDict)
        {
            if (failedFiles.Count == 0)
            {
                Console.WriteLine("所有图片加载成功！");
                return;
            }

            Console.WriteLine("重新加载失败的图片...");
            foreach (string filePath in failedFiles)
            {
                try
                {
                    string folderPath = Path.GetDirectoryName(filePath); // 获取文件夹路径
                    string folderName = Path.GetFileName(folderPath);
                    using (Bitmap image = new Bitmap(filePath))
                    {
                        List<Tuple<Bitmap, string>> images;
                        if (imagesDict.TryGetValue(folderName, out images))
                        {
                            images.Add(Tuple.Create(new Bitmap(image), Path.GetFileNameWithoutExtension(filePath))); // 添加副本和文件名
                        }
                        else
                        {
                            imagesDict[folderName] = new List<Tuple<Bitmap, string>> { Tuple.Create(new Bitmap(image), Path.GetFileNameWithoutExtension(filePath)) };
                        }

                        Console.WriteLine($"成功重新加载：{filePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"重新加载图片 {filePath} 时出错：{ex.Message}");
                }
            }
        }


        // 截屏
        public class CompareImage
        {
            public string currentResolution; // 添加此字段

            public CompareImage()
            {
                currentResolution = ResolutionSettings.GetScreenResolution(); // 初始化
            }

            // 获取屏幕区域方法
            public List<Rectangle> GetScreenAreas()
            {
                var resolution = ResolutionSettings.GetScreenResolution();
                var settings = ResolutionSettings.GetResolutionSettings(resolution);

                if (settings == null)
                {
                    return new List<Rectangle>();
                }

                // 将字典转换为 List<Rectangle>
                return settings.Values.Select(t => new Rectangle(t.Item1, t.Item2, t.Item3, t.Item4)).ToList();
            }

            // 截图方法
            public async Task<(List<Mat> mats, List<BitmapImage> bitmapImages)> CaptureScreenAreaAsync(List<Rectangle> screenAreas)
            {
                List<Mat> mats = new List<Mat>();
                List<BitmapImage> bitmapImages = new List<BitmapImage>();

                foreach (var area in screenAreas)
                {
                    using (var img = new Bitmap(area.Width, area.Height))
                    {
                        using (var g = Graphics.FromImage(img))
                        {
                            g.CopyFromScreen(area.Location, System.Drawing.Point.Empty, area.Size);
                        }


                        // 显示截图图像
                        /* using (var mat = BitmapConverter.ToMat(img))
                         {
                             Cv2.ImShow("Screenshot", mat);
                             Cv2.WaitKey(0); // 按下任意键关闭窗口
                             Cv2.DestroyWindow("Screenshot");
                         } */
                        using (var mat = BitmapConverter.ToMat(img))
                        {
                            mats.Add((Mat)mat.Clone()); // 克隆 Mat 对象以避免引用问题
                            Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY); // 转换为灰度图像
                        }
                      
                        using (var memory = new MemoryStream())
                        {
                            img.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                            memory.Position = 0;
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = memory;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                            bitmapImages.Add(bitmapImage);
                        }
                    }
                }

                return (mats, bitmapImages);
            }

            public static async Task<double> MatchSiftAsync(Mat img1, Mat img2)
            {
                return await Task.Run(() =>
                {
                    // 检查图像是否为空
                    if (img1 == null || img2 == null)
                    {
                        Console.WriteLine("Image arguments cannot be null.");
                    }

                    if (img1.Empty() || img2.Empty())
                    {
                        Console.WriteLine("Image data is empty.");
                    }

                    // 确保输入图像是灰度图像
                    if (img1.Channels() != 1)
                    {
                        using (Mat grayImg1 = new Mat())
                        {
                            Cv2.CvtColor(img1, grayImg1, ColorConversionCodes.BGR2GRAY);
                            img1 = grayImg1.Clone();
                        }
                    }

                    if (img2.Channels() != 1)
                    {
                        using (Mat grayImg2 = new Mat())
                        {
                            Cv2.CvtColor(img2, grayImg2, ColorConversionCodes.BGR2GRAY);
                            img2 = grayImg2.Clone();
                        }
                    }

                    // 初始化变量
                    double siftSimilarity = 0.0;


                    // 创建 SIFT 检测器
                    using (var sift = OpenCvSharp.Features2D.SIFT.Create())
                    {
                        // 初始化关键点和描述子
                        var kp1 = new KeyPoint[0];
                        var kp2 = new KeyPoint[0];
                        Mat des1 = new Mat();
                        Mat des2 = new Mat();


                        // 检测关键点和计算描述子

                        sift.DetectAndCompute(img1, null, out kp1, des1, false);
                        sift.DetectAndCompute(img2, null, out kp2, des2, false);

                        // 检查关键点和描述子是否有效
                        if (kp1 == null || kp2 == null || des1.Empty() || des2.Empty())
                        {
                            Console.WriteLine("Failed to detect keypoints or compute descriptors.");
                            return 0.0;
                        }

                        // 输出关键点数量
                        Console.WriteLine("关键点数量1：" + kp1.Length);
                        Console.WriteLine("关键点数量2：" + kp2.Length);



                        // 使用 Flann 基于匹配器
                        using (var flann = new FlannBasedMatcher())
                        {
                            // 调用 KnnMatch 方法
                            var matches = flann.KnnMatch(des1, des2, 2);

                            int matchedPoints = 0;

                            // 遍历匹配结果
                            foreach (var match in matches)
                            {

                                if (match.Length >= 2 && match[0].Distance < 0.7 * match[1].Distance)
                                {
                                    matchedPoints++;
                                }
                            }

                            // 计算 SIFT 匹配度
                            siftSimilarity = (double)matchedPoints / (kp1.Length + 0.1);
                            Console.WriteLine($"SIFT 匹配度：{siftSimilarity}");
                        }
                    }

                    // Canny 边缘检测
                    using (var edges = new Mat())
                    {
                        using (var diff = new Mat())
                        {

                            using (Mat img2Resized = new Mat())
                            {
                                // 将 img2 调整为与 img1 相同的大小
                                Cv2.Resize(img2, img2Resized, img1.Size());

                                double cannySimilarity = 0.0;
                                Cv2.Canny(img1, edges, 100, 200);
                                Cv2.Canny(img2, edges, 100, 200);

                                Cv2.Absdiff(img1, img2Resized, diff);// 进行差异计算
                                cannySimilarity = Cv2.CountNonZero(diff) / (double)(diff.Rows * diff.Cols);

                                // 综合两种方法的结果
                                double combinedSimilarity = siftSimilarity * 0.4 + cannySimilarity * 0.6;
                                Console.WriteLine($"综合相似度：{combinedSimilarity}");
                                return combinedSimilarity;
                            }
                        }
                    }
                });
            }


        }
        
    }
}




