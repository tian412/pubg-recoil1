using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace pubg_recoil1
{
    public class ResolutionSettings
    {
        // 分辨率配置
        public static readonly Dictionary<string, Dictionary<string, Tuple<int, int, int, int>>> RESOLUTION_SETTINGS = new()
        {
            { "3840x2160", new Dictionary<string, Tuple<int, int, int, int>>()
                {
                    { "Name_1", Tuple.Create(86, 19, 350, 74) },  // 枪名
                    { "Scope_1", Tuple.Create(556, 56, 652, 146) },  // 倍镜
                    { "Muzzle_1", Tuple.Create(16, 310, 109, 390) },  // 枪口
                    { "Grip_1", Tuple.Create(220, 305, 312, 395) },  // 握把
                    { "Stock_1", Tuple.Create(860, 302, 955, 385) },  // 托把

                    { "Name_2", Tuple.Create(90, 460, 350, 530) },
                    { "Scope_2", Tuple.Create(556, 518, 652, 593) },
                    { "Muzzle_2", Tuple.Create(11, 760, 109, 855) },
                    { "Grip_2", Tuple.Create(230, 750, 310, 970) },
                    { "Stock_2", Tuple.Create(860, 756, 955, 855) },
                }
            },
            { "3440x1440", new Dictionary<string, Tuple<int, int, int, int>>()
                {
                    { "Name_1", Tuple.Create(65, 12, 290, 49) },
                    { "Scope_1", Tuple.Create(377, 37, 440, 95) },
                    { "Muzzle_1", Tuple.Create(13, 190, 78, 252) },
                    { "Grip_1", Tuple.Create(149, 189, 213, 254) },
                    { "Stock_1", Tuple.Create(579, 189, 643, 254) },

                    { "Name_2", Tuple.Create(65, 288, 290, 325) },
                    { "Scope_2", Tuple.Create(376, 313, 441, 377) },
                    { "Muzzle_2", Tuple.Create(13, 463, 78, 526) },
                    { "Grip_2", Tuple.Create(148, 462, 214, 527) },
                    { "Stock_2", Tuple.Create(579, 463, 644, 526) },
                }
            },
            // 其他分辨率数据略，同样方法继续添加
        };

        // 分辨率对应的坐标设置
        public static readonly Dictionary<string, Tuple<int, int, int, int>> GUNS_RESOLUTION_SETTINGS = new()
        {
            { "3840x2160", Tuple.Create(173, 2647, 975, 808) },
            { "3440x1440", Tuple.Create(115, 2198, 681, 539) },
            { "2560x1600", Tuple.Create(132, 1820, 738, 598) },
            { "2560x1440", Tuple.Create(118, 1761, 660, 536) },
            { "2560x1080", Tuple.Create(88, 1642, 509, 432) },
            { "1920x1080", Tuple.Create(89, 1324, 517, 411) },
            { "1728x1080", Tuple.Create(86, 1223, 505, 404) }
        };

        // 右键单击开镜坐标位置
        public static readonly Dictionary<string, Tuple<int, int>> CLICK_SETTINGS = new()
        {
            { "3840x2160", Tuple.Create(1841, 2004) },
            { "3440x1440", Tuple.Create(1667, 1336) },
            { "2560x1600", Tuple.Create(1222, 1484) },
            { "2560x1440", Tuple.Create(1228, 1335) },
            { "2560x1080", Tuple.Create(1242, 1002) },
            { "1920x1080", Tuple.Create(920, 1001) },
            { "1728x1080", Tuple.Create(824, 1001) }
        };

        // 判断用户屏幕分辨率
        public static string GetScreenResolution()
        {
            var width = (int)SystemParameters.PrimaryScreenWidth;
            var height = (int)SystemParameters.PrimaryScreenHeight;
            return $"{width}x{height}";
        }

        // 获取特定区域的坐标
        public static Tuple<int, int, int, int> GetResolutionSetting(string resolution, string area)
        {
            if (RESOLUTION_SETTINGS.TryGetValue(resolution, out var settings) && settings.TryGetValue(area, out var coordinates))
            {
                return coordinates;
            }
            return Tuple.Create(0, 0, 0, 0); // 返回默认值
        }

        // 获取所有区域的坐标
        public static Dictionary<string, Tuple<int, int, int, int>> GetAllResolutionSettings(string resolution)
        {
            if (RESOLUTION_SETTINGS.TryGetValue(resolution, out var settings))
            {
                return settings;
            }
            return new Dictionary<string, Tuple<int, int, int, int>>(); // 返回空字典
        }

        // 获取枪的坐标
        public static Tuple<int, int, int, int> GetGunResolution(string resolution)
        {
            if (GUNS_RESOLUTION_SETTINGS.TryGetValue(resolution, out var coordinates))
            {
                return coordinates;
            }
            return Tuple.Create(0, 0, 0, 0); // 返回默认值
        }

        // 获取右键单击开镜坐标位置
        public static Tuple<int, int> GetClickSetting(string resolution)
        {
            if (CLICK_SETTINGS.TryGetValue(resolution, out var coordinates))
            {
                return coordinates;
            }
            return Tuple.Create(0, 0); // 返回默认值
        }
    }
}