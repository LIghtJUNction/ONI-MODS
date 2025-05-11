using HarmonyLib;
using KMod;
using PeterHan.PLib.AVC;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.Options;
using PeterHan.PLib.PatchManager;

using System;


namespace LIghtJUNction.DuplicantAgent
{
    public sealed class DuplicantAgentMod : UserMod2
    {        [PLibMethod(RunAt.AfterDbInit)]
        internal static void AfterDbInit(Harmony harmony)
        {
            try
            {
                // 确保在此时访问实例是安全的，因为在OnLoad已经注册了选项
                var options = DuplicantAgentOptions.Instance;
                if (options.Enable)
                {
                    PUtil.LogDebug("DuplicantAgent On");
                }
                else
                {
                    PUtil.LogDebug("DuplicantAgent Off");
                }
            }
            catch (Exception ex)
            {
                PUtil.LogWarning("无法获取DuplicantAgentOptions实例: " + ex.ToString());
            }
            

        }        
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            try 
            {
                // 首先初始化PLib和注册选项
                PUtil.InitLibrary();
                new PLocalization().Register();                // 检查注册
                PUtil.LogDebug("PLib已初始化");                // 创建选项实例并注册选项
                var options = new POptions();
                options.RegisterOptions(this, typeof(DuplicantAgentOptions));

                PUtil.LogDebug("已注册选项和选项变更监听器");

                // 延迟加载 - 不立即访问Instance
                // var opts = DuplicantAgentOptions.Instance;

                // 注册补丁
                new PPatchManager(harmony).RegisterPatchClass(typeof(DuplicantAgentMod));
                
                // 版本检查
                new PVersionCheck().Register(this, new SteamVersionChecker());

                PUtil.LogDebug("DuplicantAgent模组加载完成");
            }
            catch (Exception ex)
            {
                // 记录异常，但不阻止模组加载
                PUtil.LogError("DuplicantAgent初始化错误: " + ex.ToString());
            }
        }
        

    }
}





