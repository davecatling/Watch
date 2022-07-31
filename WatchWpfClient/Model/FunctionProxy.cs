using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WatchWpfClient.Model
{
    public class FunctionProxy
    {
        private WatchConfig? _config;

        public FunctionProxy()
        {
            var configPath = Path.Combine(Assembly.GetExecutingAssembly().Location, @"\Model\WatchConfig.json");
            GetConfig(configPath);
        }

        private void GetConfig(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File {path} not found");
            _config = JsonConvert.DeserializeObject<WatchConfig>(path);
        }
    }
}
