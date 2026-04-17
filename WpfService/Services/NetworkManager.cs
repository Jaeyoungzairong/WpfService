using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;

namespace WpfService.Services
{
    public class NetworkManager
    {
        public static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        //public static NetworkInterface GetEthernetInterface()
        //{
        //    var ethernets = NetworkInterface.GetAllNetworkInterfaces()
        //        .Where(n =>
        //            n.OperationalStatus == OperationalStatus.Up &&
        //            n.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
        //            !ContainsIgnoreCase(n.Description, "Virtual") &&
        //            !ContainsIgnoreCase(n.Description, "VMware") &&
        //            !ContainsIgnoreCase(n.Description, "Hyper-V") &&
        //            !ContainsIgnoreCase(n.Description, "Loopback") &&
        //            !ContainsIgnoreCase(n.Description, "TAP") &&
        //            !ContainsIgnoreCase(n.Description, "Npcap") &&
        //            !ContainsIgnoreCase(n.Description, "Packet") &&
        //            !ContainsIgnoreCase(n.Description, "Bridge"))
        //        .OrderByDescending(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
        //        .ToList();

        //    return ethernets.FirstOrDefault();
        //}

        public static List<NetworkInterface> GetEthernetInterfaces()
        {
            var ethernets = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up &&
                    n.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    !ContainsIgnoreCase(n.Description, "Virtual") &&
                    !ContainsIgnoreCase(n.Description, "VMware") &&
                    !ContainsIgnoreCase(n.Description, "Hyper-V") &&
                    !ContainsIgnoreCase(n.Description, "Loopback") &&
                    !ContainsIgnoreCase(n.Description, "TAP") &&
                    !ContainsIgnoreCase(n.Description, "Npcap") &&
                    !ContainsIgnoreCase(n.Description, "Packet") &&
                    !ContainsIgnoreCase(n.Description, "Bridge"))
                //.OrderByDescending(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .ToList();

            return ethernets;
        }

        public static NetworkInterface GetEthernetInterface(string ifName)
        {
            if (string.IsNullOrEmpty(ifName))
                return null;

            var ethernets = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.Name == ifName &&
                    n.OperationalStatus == OperationalStatus.Up &&
                    n.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    !ContainsIgnoreCase(n.Description, "Virtual") &&
                    !ContainsIgnoreCase(n.Description, "VMware") &&
                    !ContainsIgnoreCase(n.Description, "Hyper-V") &&
                    !ContainsIgnoreCase(n.Description, "Loopback") &&
                    !ContainsIgnoreCase(n.Description, "TAP") &&
                    !ContainsIgnoreCase(n.Description, "Npcap") &&
                    !ContainsIgnoreCase(n.Description, "Packet") &&
                    !ContainsIgnoreCase(n.Description, "Bridge"))
                //.OrderByDescending(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .ToList();

            return ethernets.FirstOrDefault();
        }

        public static List<NetworkInterface> GetWifiInterfaces()
        {
            var ethernets = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up &&
                    n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                    !ContainsIgnoreCase(n.Description, "Virtual") &&
                    !ContainsIgnoreCase(n.Description, "VMware") &&
                    !ContainsIgnoreCase(n.Description, "Hyper-V") &&
                    !ContainsIgnoreCase(n.Description, "Loopback") &&
                    !ContainsIgnoreCase(n.Description, "TAP") &&
                    !ContainsIgnoreCase(n.Description, "Npcap") &&
                    !ContainsIgnoreCase(n.Description, "Packet") &&
                    !ContainsIgnoreCase(n.Description, "Bridge"))
                .ToList();

            return ethernets;
        }

        public static int SetStaticIPv4(string ifName, string ip, string mask, string gateway)
        {
            // 예: netsh interface ip set address name="Ethernet" static 192.168.0.50 255.255.255.0 192.168.0.1 1
            var args = $"interface ip set address name=\"{ifName}\" static {ip} {mask} {gateway} 1";
            
            int code = RunNetsh(args);
            if (code != 0)
                return code;

            code = RunNetsh($"interface ip set dns name=\"{ifName}\" static 8.8.8.8 primary");
            if (code != 0)
                return code;

            code = RunNetsh($"interface ip add dns name=\"{ifName}\" 1.1.1.1 index=2");
            return code;
        }

        public static int SetDhcpIPv4(string ifName)
        {
            // IP 자동
            int code = RunNetsh($"interface ip set address name=\"{ifName}\" dhcp");
            if (code != 0) 
                return code;

            // DNS 자동
            code = RunNetsh($"interface ip set dns name=\"{ifName}\" dhcp");
            return code;
        }

        private static bool ContainsIgnoreCase(string source, string value)
        {
            if (source == null || value == null)
                return false;
            else
                return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int RunNetsh(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using var p = Process.Start(psi)!;
            string stdout = p.StandardOutput.ReadToEnd();
            string stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (!string.IsNullOrWhiteSpace(stdout))
                Dialog.Error(stdout);
            if (!string.IsNullOrWhiteSpace(stderr))
                Dialog.Error(stderr);

            return p.ExitCode;
        }

        
    }
}
