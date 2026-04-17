using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WpfService.Models
{
    public class CommonParams
    {
        public static CommonParams Instance { get; } = new CommonParams();
        private CommonParams() { }

        [Category("1. Spectrometer")]
        public int ReadCount { get; set; }
        [Category("1. Spectrometer")]
        public int TakeCount { get; set; }
        [Category("2. Device")]
        public bool Align { get; set; }

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

                foreach (var prop in typeof(CommonParams).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var token = jObj.GetValue(prop.Name, StringComparison.OrdinalIgnoreCase);
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
            var props = typeof(CommonParams).GetProperties(BindingFlags.Public | BindingFlags.Instance);
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
