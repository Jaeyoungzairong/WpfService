using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace WpfService.Models
{
    public class CuvetteParams
    {
        public static CuvetteParams Instance { get; } = new CuvetteParams();
        private CuvetteParams() { }

        [Category("1. Common")]
        public int ReadTime { get; set; }
        [Category("1. Common")]
        public int Xenon { get; set; }
        [Category("1. Common")]
        public double WaveShift { get; set; }

        [Category("2. Smoothing")]
        public bool Smoothing { get; set; }
        [Category("2. Smoothing")]
        public int SmoothingParam1 { get; set; }
        [Category("2. Smoothing")]
        public double SmoothingParam2 { get; set; }


        [Category("3. Check Point")]
        public double Checkpoint1 { get; set; }
        [Category("3. Check Point")]
        public double Checkpoint2 { get; set; }
        [Category("3. Check Point")]
        public double Checkpoint3 { get; set; }
        [Category("4. Correction(%)")]
        public double Calibrate { get; set; }
        [Category("4. Correction(%)")]
        public double Correction1 { get; set; }
        [Category("4. Correction(%)")]
        public double Correction2 { get; set; }
        [Category("4. Correction(%)")]
        public double Correction3 { get; set; }


        public bool SetData(object obj)
        {
            try
            {
                JObject jObj;
                if (obj is JObject j)
                    jObj = j;
                else if (obj is string s)
                    jObj = JObject.Parse(s);
                else
                    jObj = JObject.FromObject(obj);

                foreach (var prop in typeof(CuvetteParams).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var token = jObj?.GetValue(prop.Name, StringComparison.OrdinalIgnoreCase);
                    if (token != null)
                    {
                        var value = token.ToObject(prop.PropertyType);
                        prop.SetValue(this, value);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        public string ToQueryString()
        {
            var props = typeof(CuvetteParams).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var list = props
                .Select(p => new
                {
                    Key = p.Name.ToLower(),
                    Value = p.GetValue(this)?.ToString()
                })
                .Where(p => !string.IsNullOrWhiteSpace(p.Value))
                .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}");

            return string.Join("&", list);
        }
    }
}
