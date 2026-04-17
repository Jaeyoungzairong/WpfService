using WpfService.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace WpfService.Models
{
    public class Config
    {
        public static Config Instance { get; } = new Config();
        private Config() { }

        private readonly string _filePath = Application.StartupPath;
        private readonly string _fileName = "Config.data";

        private string _hostAddress = "192.168.1.101";
        private int _port = 8443;
        private string _recoveryFileName = Path.Combine(Application.StartupPath, "RKDevTool_Release_v2.93", "RKDevTool.exe");

        #region Properties
        [Category("1. Network")]
        [DisplayName("1. Host Address")]
        public string HostAddress { get => _hostAddress; set => _hostAddress = value; }
        [Category("1. Network")]
        [DisplayName("2. Port")]
        public int Port { get => _port; set => _port = value; }
        [Category("2. Recovery")]
        [DisplayName("1. Application")]
        public string RecoveryFileName { get => _recoveryFileName; set => _recoveryFileName = value; }

        #endregion

        #region Method
        public bool ReadXml()
        {
            try
            {
                string fileName = Path.Combine(_filePath, _fileName);
                if (!File.Exists(fileName))
                    return false;

                XmlHelper<Config> helper = new XmlHelper<Config>();
                Config curDataHandler = helper.Read(fileName);
                if (curDataHandler == null)
                    return false;

                HostAddress = curDataHandler.HostAddress;
                Port = curDataHandler.Port;
                RecoveryFileName = curDataHandler.RecoveryFileName;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void WriteXml()
        {
            try
            {
                if (Directory.Exists(_filePath))
                {
                    string fileName = Path.Combine(_filePath, _fileName);
                    XmlHelper<Config> helper = new XmlHelper<Config>();
                    helper.Save(fileName, this);
                }
            }
            catch { }
        }
        #endregion
    }
}
