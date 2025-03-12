using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace pubg_recoil1
{
    public class ResolutionSettings
    {
        public static readonly Dictionary<string, Dictionary<string, Tuple<int, int, int, int>>> RESOLUTION_SETTINGS = new()
    {
        { "3840x2160", new Dictionary<string, Tuple<int, int, int, int>>()
            {
                { "Name_1", Tuple.Create(2751, 198, 82, 31) },  // 枪名所有后面两个宽高不能动不然识别不了
                { "Scope_1", Tuple.Create(3225, 252, 46, 58) },  // 倍镜
                { "Muzzle_1", Tuple.Create(2700, 486, 52, 57) },  // 枪口
                { "Grip_1", Tuple.Create(2881, 492, 75, 66) },  // 握把
                { "Stock_1", Tuple.Create(3529, 516, 69, 87) },  // 托把
                { "poses_1", Tuple.Create(1417, 1984, 60, 60) },  // 姿势

                { "Name_2", Tuple.Create(2745, 640, 260, 50) },
                { "Scope_2", Tuple.Create(3225, 703, 46, 58) },
                { "Muzzle_2", Tuple.Create(2700, 937, 52, 57) },
                { "Grip_2", Tuple.Create(2881, 943, 75, 66) },
                { "Stock_2", Tuple.Create(3529, 967, 69, 87) },
                { "poses_2", Tuple.Create(1417, 1984, 60, 60) },  // 姿势
             
            }
        },
        { "3440x1440", new Dictionary<string, Tuple<int, int, int, int>>()
            {
                 { "Name_1", Tuple.Create(86, 19, 30, 160) },  // 枪名所有后面两个宽高不能动不然识别不了
                { "Scope_1", Tuple.Create(556, 56, 66, 66) },  // 倍镜
                { "Muzzle_1", Tuple.Create(16, 310, 75, 77) },  // 枪口
                { "Grip_1", Tuple.Create(220, 305, 66, 67) },  // 握把
                { "Stock_1", Tuple.Create(860, 302, 67, 66) },  // 托把
                { "poses", Tuple.Create(860, 302, 75, 90) },  // 姿势

                { "Name_2", Tuple.Create(90, 460, 350, 530) },
                { "Scope_2", Tuple.Create(556, 518, 66, 66) },
                { "Muzzle_2", Tuple.Create(11, 760, 75, 77) },
                { "Grip_2", Tuple.Create(230, 750, 67, 66) },
                { "Stock_2", Tuple.Create(860, 756, 66, 67) },
            }
        }
        };
        // 其他分辨率数据略，同样方法继续添加
        public static string GetAreaFolderName(string areaName)
        {
            // 提取文件夹名称
            switch (areaName)
            {
                case "Name_1":
                case "Name_2":
                    return "weapons";
                case "Scope_1":
                case "Scope_2":
                    return "scopes";
                case "Muzzle_1":
                case "Muzzle_2":
                    return "muzzles";
                case "Grip_1":
                case "Grip_2":
                    return "grips";
                case "Stock_1":
                case "Stock_2":
                    return "stocks";
                case "poses_1":
                case "poses_2":
                    return "poses";
                default:
                    return "weapons";
            }
        }



        public static string GetScreenResolution()
        {
            Screen screen = Screen.PrimaryScreen;
            return $"{screen.Bounds.Width}x{screen.Bounds.Height}";
        }

        public static Dictionary<string, Tuple<int, int, int, int>> GetResolutionSettings(string resolution)
        {
            if (RESOLUTION_SETTINGS.TryGetValue(resolution, out var settings))
            {
                if (settings.Count != 12) // 假设每个分辨率应有10个区域
                {
                    Console.WriteLine($"分辨率 {resolution} 的设置不完整。");
                }
                return settings;
            }
            else
            {
                Console.WriteLine($"未找到分辨率 {resolution} 的设置。使用默认分辨率 3840x2160。");
                return RESOLUTION_SETTINGS["3840x2160"];
            }
        }
    }
}
    