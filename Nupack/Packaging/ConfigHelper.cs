using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CnSharp.VisualStudio.NuPack.Util;
using EnvDTE;

namespace CnSharp.VisualStudio.NuPack.Packaging
{
    public static class ConfigHelper
    {
        public static readonly string AppDataDir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), Common.ProductName);
        private static readonly string NuGetConfigPath = Path.Combine(AppDataDir, NuGetConfig.FileName);


        public static NuGetConfig ReadNuGetConfig()
        {
            if (!File.Exists(NuGetConfigPath))
                return new NuGetConfig();
            return XmlSerializerHelper.LoadObjectFromXml<NuGetConfig>(NuGetConfigPath);
        }

        /// <summary>
        /// 获取默认的nuget.config配置
        /// </summary>
        /// <param name="nugetConfigPath"></param>
        /// <returns></returns>
        public static NuGetSource GetDefaultNugetConfig(string nugetConfigPath)
        {
            if (!File.Exists(nugetConfigPath))
            {
                return new NuGetSource();
            }

            var nugetSource = new NuGetSource();

            XElement xElement = XElement.Load(nugetConfigPath);

            var elements = xElement.Elements();
            if (elements.Count() > 0)
            {
                foreach (var ele in elements)
                {
                    switch (ele.Name.LocalName.ToLower())
                    {
                        case "packagesourcecredentials":
                            var credentialsElements = ele.Elements()?.Elements();
                            var apiKeyElement = credentialsElements?.FirstOrDefault(item => item.Attribute("key").Value.Equals("ClearTextPassword", StringComparison.OrdinalIgnoreCase));
                            nugetSource.ApiKey = apiKeyElement?.Attribute("value").Value;

                            var userNameElement = credentialsElements?.FirstOrDefault(item => item.Attribute("key").Value.Equals("Username", StringComparison.OrdinalIgnoreCase));
                            nugetSource.UserName = userNameElement?.Attribute("value").Value;

                            break;

                        case "config":

                            XElement defaultPushSourceElement = ele.Elements()?.FirstOrDefault(item => item.Attribute("key").Value.Equals("defaultPushSource", StringComparison.OrdinalIgnoreCase));
                            string defaultPushName = defaultPushSourceElement?.Attribute("value").Value;
                            if (!string.IsNullOrEmpty(defaultPushName))
                            {
                                var urlElement = elements.FirstOrDefault(item => item.Name.LocalName.Equals("packageSources", StringComparison.OrdinalIgnoreCase))?.Elements()?.FirstOrDefault(item => item.Attribute("key").Value == defaultPushName);
                                nugetSource.Url = urlElement?.Attribute("value").Value;
                            }

                            break;

                        default:
                            break;
                    }

                }
            }

            nugetSource.Checked = true;
            LogUtils.WriteLog($"ApiKey:{nugetSource.ApiKey} Url:{nugetSource.Url} UserName:{nugetSource.UserName}");
            
            return nugetSource;
        }

        public static void Save(this NuGetConfig config)
        {
            var xml = XmlSerializerHelper.GetXmlStringFromObject(config);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var dir = Path.GetDirectoryName(NuGetConfigPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            doc.Save(NuGetConfigPath);
        }

        public static ProjectNuPackConfig ReadNuPackConfig(this Project project)
        {
            var file = Path.Combine(AppDataDir, "\\" + project.UniqueName + ProjectNuPackConfig.Ext);
            if (!File.Exists(file))
                return new ProjectNuPackConfig(project);
            var config = XmlSerializerHelper.LoadObjectFromXml<ProjectNuPackConfig>(file);
            config.Project = project;
            return config;
        }

        public static void Save(this ProjectNuPackConfig config)
        {
            var file = Path.Combine(AppDataDir, "\\" + config.Project.UniqueName + ProjectNuPackConfig.Ext);
            var xml = XmlSerializerHelper.GetXmlStringFromObject(config);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            doc.Save(file);
        }

        //private static string FindNuGetExePath()
        //{
        //    var pathes = Environment.GetEnvironmentVariables();
        //}
    }

    public class NuGetSource
    {
        public string Url { get; set; }
        public string ApiKey { get; set; }
        public string UserName { get; set; }

        public bool Checked { get; set; }
    }

    public class NuGetConfig
    {
        public NuGetConfig()
        {
            Sources = new List<NuGetSource>();
        }
        public string NugetPath { get; set; }
        public List<NuGetSource> Sources { get; set; }
        public const string FileName = "new.nuget.nupack.config";

        public void AddOrUpdateSource(NuGetSource source)
        {
            Sources.RemoveAll(m => m.Url == source.Url);
            Sources.Add(source);
        }
    }

    public class ProjectNuPackConfig
    {
        [XmlIgnore]
        public Project Project { get; set; }

        public ProjectNuPackConfig()
        {

        }

        public ProjectNuPackConfig(Project project)
        {
            Project = project;
        }

        public const string Ext = ".nupack.config";

        public string FileName => Project.UniqueName + Ext;

        public string PackageOutputDirectory { get; set; } = "bin\\NuGet\\";
    }

}
