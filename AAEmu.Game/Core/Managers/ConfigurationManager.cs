using System;
using System.Collections.Generic;
using System.IO;

using AAEmu.Commons.Utils;

using Newtonsoft.Json;

using NLog;

namespace AAEmu.Game.Core.Managers
{
    public class ConfigurationManager : Singleton<ConfigurationManager>
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private Dictionary<string, string> _configurations;

        public void Load()
        {
            _log.Info("Loading ConfigurationManager...");

            #region FileManager
            _configurations = new Dictionary<string, string>();
            Dictionary<string, string> d = new Dictionary<string, string>();
            try
            {
                string data = File.ReadAllText("Data/configurations.json");
                d = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                foreach (var entry in d)
                {
                    _configurations.Add(entry.Key, entry.Value);
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
            #endregion
        }

        public string GetConfiguration(string configName)
        {
            if (configName == "")
            {
                throw new Exception("ConfigurationManager - No string received");
            }
            if (_configurations.ContainsKey(configName))
            {
                return _configurations[configName];
            }
            return "";
        }
    }
}
