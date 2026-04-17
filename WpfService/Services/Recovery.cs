using WpfService.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace WpfService.Services
{
    public class Recovery
    {
        public static Recovery Instance { get; } = new Recovery();
        private Recovery() { }

        private SerialPort _serial;
        private bool _isReboot = false;
        private bool _isInterrupt = false;

        public bool IsDebug { get; set; }

        public bool IsRKDevToolExists => File.Exists(Config.Instance.RecoveryFileName);

        public event Action<string, string> ResponseReceived;

        public bool Open(string port)
        {
            try
            {
                Close();
                _serial = new SerialPort(port, 1500000, Parity.None, 8, StopBits.One);
                _serial.WriteTimeout = 3000;
                _serial.Open();

                if (_serial.IsOpen)
                {
                    _serial.DataReceived += SerialPort_DataReceived;
                    ResponseReceived?.Invoke("SERIAL", "Success to open serial");
                    return true;
                }
                else
                {
                    ResponseReceived?.Invoke("ERROR", "Failed to open serial");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ResponseReceived?.Invoke("ERROR", ex.Message);
                return false;
            }
        }

        public void Close()
        {
            if (_serial != null && _serial.IsOpen)
            {
                _serial.DataReceived -= SerialPort_DataReceived;
                _serial.Close();
            }
        }

        public async Task<bool> StartRecoveryAsync()
        {
            _isInterrupt = false;
            _isReboot = false;

            var procs = Process.GetProcessesByName("RKDevTool");
            if (procs.Length > 0)
            {
                ResponseReceived?.Invoke("APP", "Try to kill RKDevTool");
                foreach (var proc in procs)
                {
                    proc.Kill();
                    await Task.Delay(200);
                }
                ResponseReceived?.Invoke("APP", "Success to kill RKDevTool");
            }

            await Task.Delay(200);
            try
            {
                ResponseReceived?.Invoke("1", "Try to reboot");
                _serial.WriteLine("");
                await Task.Delay(200);
                _serial.WriteLine("reboot");
                await Task.Delay(500);

                if (!_isReboot)
                {
                    ResponseReceived?.Invoke("ERROR", "Failed to reboot");
                    return false;
                }

                ResponseReceived?.Invoke("1", "Success to reboot");
                await Task.Delay(1500);
                ResponseReceived("2", "Try to interrupt");
                for (int i = 0; i < 30; i++)
                {
                    if (_isInterrupt)
                        break;

                    _serial.Write(new byte[] { 0x03 }, 0, 1);
                    await Task.Delay(200);
                }

                if (!_isInterrupt)
                {
                    ResponseReceived?.Invoke("ERROR", "Failed to interrupt");
                    return false;
                }

                ResponseReceived?.Invoke("2", "Success to interrupt");
                await Task.Delay(200);
                _serial.WriteLine("");
                await Task.Delay(200);
                ResponseReceived("3", "Try to access recovery mode");
                _serial.WriteLine("rockusb 0 mmc 0");
                await Task.Delay(200);
                return true;
            }
            catch (Exception ex)
            {
                ResponseReceived?.Invoke("ERROR", ex.Message);
                return false;
            }
            finally
            {
                Close();
            }
        }

        public Process OpenRKDevTool()
        {
            ResponseReceived?.Invoke("APP", "Try to open RKDevTool");
            if (!IsRKDevToolExists)
            {
                ResponseReceived?.Invoke("ERROR", "The RKDevTool was not found.");
                return null;
            }

            string fileName = Config.Instance.RecoveryFileName;

            try
            {
                var proc = new Process();
                proc.StartInfo.FileName = fileName;
                proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(fileName);
                bool ret = proc.Start();
                if (ret)
                    ResponseReceived?.Invoke("APP", "Success to open RKDevTool");
                else
                    ResponseReceived?.Invoke("ERROR", "Failed to open RKDevTool");

                return proc;
            }
            catch (Exception ex)
            {
                ResponseReceived?.Invoke("ERROR", ex.Message);
                return null;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = _serial.ReadExisting();
            if (!_isReboot && data.Contains("reboot"))
                _isReboot = true;
            //if (!_isReboot && data.Contains("reboot,shell"))
            //    _isReboot = true;

            if (data.Contains("<INTERRUPT>"))
                _isInterrupt = true;

            if (IsDebug)
                ResponseReceived?.Invoke($"{DateTime.Now:HH:mm:ss.fff}", data);
        }
    }
}
