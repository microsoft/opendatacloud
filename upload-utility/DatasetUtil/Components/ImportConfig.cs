using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Msr.Odr.Model;

namespace DatasetUtil.Components
{
    public class ImportConfig
    {
        public static ImportConfig Instance { get; private set; }

        private static Logger Log => Logger.Instance;
        private static CommandArgs Args => CommandArgs.Instance;

        public DatasetImportProperties Properties { get; private set; }
        public string SourcePath { get; private set; }


        public static async Task Load()
        {
            Instance = new ImportConfig();

            var cwd = Directory.GetCurrentDirectory();
            Log.Add($"Working from {cwd}");

            var configFileName = Path.Combine(cwd, Constants.ConfigFileName);
            if (!File.Exists(configFileName))
            {
                throw new FileNotFoundException("Could not find import configuration file.", Constants.ConfigFileName);
            }

            Log.Add($"Reading configuration from {Constants.ConfigFileName}");
            var fileText = await File.ReadAllTextAsync(configFileName, Encoding.UTF8);
            Instance.Properties = JsonConvert.DeserializeObject<DatasetImportProperties>(fileText);
            Instance.SourcePath = cwd;
        }

        private ImportConfig()
        {
        }
    }
}
