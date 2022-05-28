using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace PetBookAPI.Extras
{
    public class Environment
    {
        private JObject LoadSecretKeys()
        {
            try
            {
                var path = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "env.config.json");
                using (StreamReader fileReader = new StreamReader(path))
                {
                    string json = fileReader.ReadToEnd();
                    JObject secret = (JObject)JsonConvert.DeserializeObject(json);
                    return secret;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public string Key(string key)
        {
            var secretKeys = LoadSecretKeys();
            string exact = secretKeys.GetValue(key).ToString();

            return exact;
        }
    }
}