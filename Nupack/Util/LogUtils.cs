using System;
using System.IO;
using CnSharp.VisualStudio.NuPack.Packaging;

namespace CnSharp.VisualStudio.NuPack.Util
{
    public class LogUtils
    {
        private static string path = ConfigHelper.AppDataDir + @"\Logs\";
        static LogUtils()
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 写入消息
        /// </summary>
        /// <param name="msg"></param>

        public static void WriteLog(string msg)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            try
            {
                string content = "=======================================================================================\r\n" + "时间：" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t\t信息：" + msg + "\r\n******************************************************************************\r\n";
                File.AppendAllText(path + System.DateTime.Now.ToString("yyyyMMdd") + ".txt", content);
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
    }
}
