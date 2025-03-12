using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Gma.System.MouseKeyHook;
using static pubg_recoil1.ImageLoader;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Controls.Primitives;


namespace pubg_recoil1
{
    public partial class MainWindow : System.Windows.Window
    {
        private List<BitmapImage> resultImages; // 用于存储截图结果
        private Dictionary<string, List<Mat>> preloadedImages; // 用于存储预加载的图片，按文件夹分类
        private Dictionary<string, List<string>> preloadedImageNames; // 用于存储预加载图片的文件名，按文件夹分类
        private IKeyboardMouseEvents m_GlobalHook;
        private TextBlock mousePositionText;
        private Dictionary<string, string> results = new Dictionary<string, string>();
        private List<string> allPreloadedImageNames; // 用于收集所有预加载的图片名字
        private List<string> allPreloadedFolderNames; // 用于收集所有预加载的文件夹名字
        public MainWindow()
        {
            InitializeComponent();
            resultImages = new List<BitmapImage>();
            preloadedImages = new Dictionary<string, List<Mat>>(); // 初始化字典
            preloadedImageNames = new Dictionary<string, List<string>>(); // 初始化字典
            allPreloadedImageNames = new List<string>();
            allPreloadedFolderNames = new List<string>();
            Subscribe(); // 必须显示调用初始化钩子,注册全局热键

            LoadPreloadedImages();

            //初始化鼠标位置显示控件
        }

        private void LoadPreloadedImages()
        {
            string rootFolderPath = @"H:\pubg recoil1\Resources\";
            Dictionary<string, List<Tuple<Bitmap, string>>> imagesDict = ImageLoader.LoadImagesFromFolders(rootFolderPath);

            foreach (var kvp in imagesDict)
            {
                List<Tuple<Mat, string>> mats = new List<Tuple<Mat, string>>();
                foreach (var imageTuple in kvp.Value)
                {
                    var mat = BitmapConverter.ToMat(imageTuple.Item1);
                    mats.Add(Tuple.Create(mat, imageTuple.Item2));
                }
                preloadedImages[kvp.Key.ToLower()] = mats.Select(t => t.Item1).ToList(); // 文件夹名称转换为小写
                preloadedImageNames[kvp.Key.ToLower()] = mats.Select(t => t.Item2).ToList(); // 存储文件名
            }
        }

        // 根据截图区域的索引获取对应的文件夹名称
        private string GetAreaNameFromScreenshot(int index)
        {
            string[] areaNames = new string[]
            {
                "Name_1", "Scope_1", "Muzzle_1", "Grip_1", "Stock_1", "poses",
                "Name_2", "Scope_2", "Muzzle_2", "Grip_2", "Stock_2", "poses",
            };

            return index >= 0 && index < areaNames.Length ? areaNames[index] : "unknown";
        }

        private async void PerformScreenshotAsync()
        {
            var compareImage = new CompareImage();
            var screenAreas = compareImage.GetScreenAreas();

            var (mats, bitmapImages) = await compareImage.CaptureScreenAreaAsync(screenAreas);
            if (mats == null || mats.Count == 0 || bitmapImages == null || bitmapImages.Count == 0)
            {
                Console.WriteLine("截图列表为空，跳过图像处理。");
                return;
            }
         

            // 进行图片比对
            await CompareImagesAsync(mats);
        }

        private async Task CompareImagesAsync(List<Mat> mats)
        {
            var tasks = new List<Task<double>>();
            List<Mat> allPreloadedMats = new List<Mat>(); // 用于收集所有预加载的图片
            List<string> allPreloadedImageNames = new List<string>(); // 用于收集所有预加载的图片名字
            List<string> allPreloadedFolderNames = new List<string>(); // 用于收集所有预加载的图片所在文件夹名称
            var processedFolders = new HashSet<string>(); // 用于记录已经处理过的文件夹
            var processedFileNames = new HashSet<string>(); // 用于记录已经处理过的文件名
            var matInfo = mats.Select((mat, index) => new
            {
                Mat = mat,
                AreaName = GetAreaNameFromScreenshot(index),
                FolderName = ResolutionSettings.GetAreaFolderName(GetAreaNameFromScreenshot(index))
            });

            foreach (var item in matInfo)
            {
                string folderName = item.FolderName;

                // 如果已经处理过该文件夹，则跳过
                if (processedFolders.Contains(folderName))
                {
                    continue;
                }

                if (preloadedImages.TryGetValue(folderName, out var preloadedMats))
                {
                    if (preloadedMats == null || preloadedMats.Count == 0)
                    {
                        Console.WriteLine($"预加载的图片列表为空：{folderName}。");
                        continue;
                    }

                    foreach (var preloadedMat in preloadedMats)
                    {
                        tasks.Add(CompareImage.MatchSiftAsync(item.Mat, preloadedMat));
                        allPreloadedMats.Add(preloadedMat);

                        // 获取当前文件夹的图片文件名
                        if (preloadedImageNames.TryGetValue(folderName, out List<string> imageNames))
                        {
                            foreach (var imageName in imageNames)
                            {
                                // 如果文件名已经处理过，则跳过
                                if (processedFileNames.Contains(imageName))
                                {
                                    continue;
                                }

                                // 添加文件名到列表
                                allPreloadedImageNames.Add(imageName);
                                allPreloadedFolderNames.Add(folderName);

                                // 标记文件名为已处理
                                processedFileNames.Add(imageName);
                            }
                        }
                    }

                    // 标记该文件夹为已处理
                    processedFolders.Add(folderName);
                }
            }

            // 等待所有任务完成
            var similarityResults = await Task.WhenAll(tasks);

            // 初始化 results 字典，确保每个文件夹的条目都存在
            results["weaponName"] = "none";
            results["grip"] = "none";
            results["muzzle"] = "none";
            results["scope"] = "none";
            results["stock"] = "none";
            results["pose"] = "none";

            // 处理匹配结果
            double similarityThreshold = 0.7; // 设置相似度阈值
            for (int i = 0; i < similarityResults.Length; i++)
            {
                if (i >= allPreloadedFolderNames.Count || i >= allPreloadedImageNames.Count)
                {
                    Console.WriteLine("索引超出范围，跳过当前结果。");
                    continue;
                }

                double score = similarityResults[i];
                string folderName = allPreloadedFolderNames[i];
                string fileName = allPreloadedImageNames[i];

                Console.WriteLine($"区域 {folderName} 的匹配分数: {score}");

                // 弹出窗口显示每个区域的匹配分数
               // System.Windows.MessageBox.Show($"区域 {folderName} 的匹配分数: {score}", "匹配分数");
                // 检查是否已经匹配过该文件夹
                if (results.ContainsKey(folderName) && results[folderName] != "none")
                {
                    continue;
                }

                if (score >= similarityThreshold)
                {
                    // 更新 results 字典
                    switch (folderName)
                    {
                        case "weapons":
                            results["weaponName"] = fileName;
                            break;
                        case "grips":
                            results["grip"] = fileName;
                            break;
                        case "muzzles":
                            results["muzzle"] = fileName;
                            break;
                        case "scopes":
                            results["scope"] = fileName;
                            break;
                        case "stocks":
                            results["stock"] = fileName;
                            break;
                        case "poses":
                            results["pose"] = fileName;
                            break;
                    }
                }
            }

            // 确保在调用 WriteResultsToLuaFile 时，results 字典已填充
            WriteResultsToLuaFile();
        }


        private void WriteResultsToLuaFile()
        {
            string luaFilePath = @"H:\pubg recoil1\results.lua";
            using (StreamWriter writer = new StreamWriter(luaFilePath, false))
            {
              
                foreach (var result in results)
                {
                    writer.WriteLine($"    {result.Key} = \"{result.Value}\"");
                }
              
            }

            System.Windows.MessageBox.Show($"匹配结果已写入 {luaFilePath}");
        }

        // 在 CompareImagesAsync 方法中调用



        // 热键订阅
        private void Subscribe()
        {
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyPress += GlobalHookKeyPress; // 监听 KeyDown 事件
        }

        private async void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            // 如果按下 Tab 键，触发截图
            if (e.KeyChar == (char)Keys.Tab)
            {
                await Task.Delay(10); // 等待 10 毫秒
                PerformScreenshotAsync();
            }
        }

        private void Unsubscribe()
        {
            m_GlobalHook.KeyPress -= GlobalHookKeyPress;
            m_GlobalHook.Dispose();
        }

       

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (System.Windows.Controls.ComboBox)sender;
            var parentStackPanel = (StackPanel)comboBox.Parent; // 获取包含 ComboBox 的 StackPanel

            // 在 StackPanel 中查找 TextBox
            var textBox = parentStackPanel.Children.OfType<System.Windows.Controls.TextBox>().FirstOrDefault();

            switch (comboBox.Name)
            {
                case "scope_combobox":
                    switch (comboBox.SelectedIndex)
                    {
                        case 0:
                            textBox.Text = "1.0";
                            break;
                        case 1:
                            textBox.Text = "1.0";
                            break;
                        case 2:
                            textBox.Text = "1.5";
                            break;
                        case 3:
                            textBox.Text = "2.0";
                            break;
                        case 4:
                            textBox.Text = "3.0";
                            break;
                        case 5:
                            textBox.Text = "4.0";
                            break;
                        case 6:
                            textBox.Text = "5.0";
                            break;
                        case 7:
                            textBox.Text = "6.0";
                            break;

                        default:
                            break;
                    }
                    break;
                case "stock_combobox":
                    switch (comboBox.SelectedIndex)
                    {
                        case 0:
                            textBox.Text = "1.0";
                            break;
                        case 1:
                            textBox.Text = "2.0";
                            break;
                        case 2:
                            textBox.Text = "3.0";
                            break;
                        case 3:
                            textBox.Text = "4.0";
                            break;
                    }
                    break;
                case "grip_combobox":
                    switch (comboBox.SelectedIndex)
                    {
                        case 0:
                            textBox.Text = "1.0";
                            break;
                        case 1:
                            textBox.Text = "2.0";
                            break;
                        case 2:
                            textBox.Text = "3.0";
                            break;
                        case 3:
                            textBox.Text = "4.0";
                            break;
                        case 4:
                            textBox.Text = "4.0";
                            break;
                        case 5:
                            textBox.Text = "4.0";
                            break;
                        case 6:
                            textBox.Text = "4.0";
                            break;
                        case 7:
                            textBox.Text = "4.0";
                            break;
                        case 8:
                            textBox.Text = "4.0";
                            break;

                    }
                    break;
                case "muzzle_combobox":
                    switch (comboBox.SelectedIndex)
                    {
                        case 0:
                            textBox.Text = "1.0";
                            break;
                        case 1:
                            textBox.Text = "2.0";
                            break;
                        case 2:
                            textBox.Text = "3.0";
                            break;
                        case 3:
                            textBox.Text = "4.0";
                            break;

                    }
                    break;
                case "pose_combobox":
                    switch (comboBox.SelectedIndex)
                    {
                        case 0:
                            textBox.Text = "1.0";
                            break;
                        case 1:
                            textBox.Text = "2.0";
                            break;
                        case 2:
                            textBox.Text = "3.0";
                            break;

                    }
                    break;
            }
        }
    }
}
    