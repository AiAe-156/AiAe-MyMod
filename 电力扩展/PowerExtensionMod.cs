using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace PowerExtension
{
    // 缺氧模组的主入口类，必须继承 UserMod2
    public class PowerExtensionMod : UserMod2
    {
        public override void OnLoad(HarmonyLib.Harmony harmony)
        {
            base.OnLoad(harmony);

            // [PLib 初始化] 启用 PLib 核心功能。传入 false 避免初始化不必要的额外特性
            PUtil.InitLibrary(false);

            // [注册配置菜单] 将我们下面定义的 ModOptions 类注册为本模组的设置菜单
            new POptions().RegisterOptions(this, typeof(ModOptions));
        }
    }
}