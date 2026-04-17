using Newtonsoft.Json;

namespace WpfService.Models
{
    public class ApiResponse
    {
        [JsonProperty("result")]
        public bool Result { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("errMsg")]
        public string ErrMsg { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        public bool IsExpired { get; set; }

        public bool IsTimeout { get; set; }

        public ApiResponse()
        {

        }

        public ApiResponse(string errMsg)
        {
            ErrMsg = errMsg;
        }

        public override string ToString()
        {
            return $"Result:{Result}\r\nUrl:{Url}\r\nErrMsg:{ErrMsg}\r\nData:{Data}";
        }
    }
}
