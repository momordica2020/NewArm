using Emgu.CV.Ocl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewArm.Core
{
    public class Config
    {
        
        public bool actMouseLeft { get; set; }
        public bool actMouseRight { get; set; }

        public int cdTimeMs { get; set; }
        public Keys actKey { get; set; }
    }

    public class ConfigManager
    {
        public string FilePath;
        public Config config;
        //public List<Config> configs;

        public ConfigManager()
        {
            FilePath = "config.json";
            config = new Config();
        }


        public Config Load()
        {
            try
            {
                // 从文件中读取 JSON 字符串
                if (!File.Exists(FilePath))
                {
                    throw new FileNotFoundException("Configuration file not found.");
                }
                string jsonString = File.ReadAllText(FilePath);

                config = JsonConvert.DeserializeObject<Config>(jsonString);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取配置文件时出错: {ex.Message}");
            }

            if (config == null) Init();
            return config;
        }


        void Init()
        {
            config = new Config
            {
                actMouseLeft = true,
                actMouseRight = false,
                actKey = Keys.F1,
                cdTimeMs = 50,
            };
        }

        public void Save()
        {

            try
            {
                if (config == null) Init();
                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(FilePath, jsonString);

                if (!File.Exists(FilePath))
                {
                    throw new FileNotFoundException("Configuration file not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置文件时出错: {ex.Message}");
            }
        }
    }
}
