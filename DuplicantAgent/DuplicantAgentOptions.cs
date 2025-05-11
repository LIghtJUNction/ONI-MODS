using System;
using Newtonsoft.Json;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace LIghtJUNction.DuplicantAgent
{

    [ModInfo("https://github.com/LIghtJUNction/ONI-Mods")]
    [ConfigFile(SharedConfigLocation: true)]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DuplicantAgentOptions
    {
        public const int CURRENT_CONFIG_VERSION = 1;
        private static DuplicantAgentOptions instance;    
        
        public static DuplicantAgentOptions Instance
    {
        get
        {
            var opts = instance;
            if (opts == null)
            {

                opts = POptions.ReadSettings<DuplicantAgentOptions>();
                if (opts == null || opts.ConfigVersion < CURRENT_CONFIG_VERSION)
                {
                    opts = new DuplicantAgentOptions();
                    // 设置默认值
                    opts.ConfigVersion = CURRENT_CONFIG_VERSION;
                    opts.Enable = true;
                    POptions.WriteSettings(opts);
                }
                instance = opts;


            }

            return opts;
        }
    }  
        
        
        #region config

        

        [JsonProperty]
        public int ConfigVersion { get; set; }

        [Option("启用ONI Agent", "启用或禁用此模组的聊天功能")]
        [JsonProperty]
        public bool Enable { get; set; }

        [Option("BaseUrl , 目前仅支持OPENAI格式响应，请填写根节点(*/v1/)")]
        [JsonProperty]
        public string BaseUrl { get; set; }

        [Option("API密钥", "用于连接到聊天服务的API密钥")]
        [JsonProperty]
        public string ApiKey { get; set; }

        [Option("模型", "模型名，必须是")]
        [JsonProperty]
        public string Model { get; set; }

        // 数值选项示例
        [Option("最大消息长度", "单条消息的最大允许字符数")]
        [Limit(100, 2000)]
        [JsonProperty]
        public int MaxMessageLength { get; set; } 



        #endregion config

        #region prompt


        // system prompts
        [Option("系统提示", "全局系统提示词")]
        [JsonProperty]
        public string SystemPrompt { get; set; }


        // name prompts 初始命名&个性初始化提示词
        [Option("小人初始化命名提示词", "用于命名的提示词")]
        [JsonProperty]
        public string NamePrompt { get; set; }

        #endregion prompt

        // personality prompts
        [Option("小人个性提示词","用于为小人生成独一无二的个性")]
        [JsonProperty]
        public string PersonalityPrompt { get; set; }        
        
        public DuplicantAgentOptions()
        {
            ConfigVersion = CURRENT_CONFIG_VERSION;
            Enable = true;
            BaseUrl = "https://api.openai.com/v1/";
            ApiKey = "sk-xxx";
            Model = "gpt-4.1-nano";
            MaxMessageLength = 500;
            SystemPrompt = PROMPTS.SYSTEMPROMPT;
            NamePrompt = PROMPTS.NAMEPROMPT;
            PersonalityPrompt = PROMPTS.PERSONALITYPROMPT;
        }

    }
    
    
    
    
    
    



}
