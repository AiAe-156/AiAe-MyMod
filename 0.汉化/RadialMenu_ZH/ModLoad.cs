using HarmonyLib;
using KMod;

namespace RadialMenuZH
{
    // 独立中文补丁：不修改 Radial Menu 原 mod，靠 Harmony 在显示层翻译。
    // 原 mod 更新（替换它自己的 dll）不会影响本补丁；若原 mod 内部结构变化导致
    // 找不到挂载点，本补丁只记录日志、安静跳过，绝不影响游戏运行。
    public class ModLoad : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            // 真正的安装放在 Localization.Initialize 的 Postfix 里执行。
            // 原因（见 Global.cs 启动顺序）：
            //   modManager.Load(Content.DLL)  -> 这里会调用所有 mod 的 OnLoad（含 Radial Menu）
            //   Localization.Initialize()     -> 在其之后调用
            // 因此在 Initialize 的 Postfix 中：
            //   1) Radial Menu 的程序集必定已加载，可反射到 RadialMenuController；
            //   2) Radial Menu 用 Strings.Add 注册的英文键位名已写入，可被我们覆盖为中文。
            harmony.PatchAll();
        }
    }
}
