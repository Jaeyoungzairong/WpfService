using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WpfService.Models;

namespace WpfService.Services
{
    public class ApiClient
    {
        private static HttpClient _httpClient;
        private static CancellationTokenSource _cts;
        private static string _sessionId;

        public static string Ip { get => _ip; set => _ip = value; }
        private static string _ip = "192.168.1.101";

        public static int Port { get => _port; set => _port = value; }
        private static int _port = 8443;

        public static string BaseUrl => $"https://{Config.Instance.HostAddress}:{Config.Instance.Port}";

        public static bool IsConnected 
        { 
            get => _isConnected && _httpClient != null; 
            set => _isConnected = value; 
        }
        private static bool _isConnected = false;

        public static bool IsDebug { get; set; }

        public static void Open()
        {
            _httpClient?.Dispose();

            var handler = new HttpClientHandler
            {
                UseCookies = false,
                //AllowAutoRedirect = false,
                PreAuthenticate = false,
                UseProxy = false,

                //CookieContainer = _cookieContainer,
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                //ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true // self-signed cert 허용
                //AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                AutomaticDecompression = DecompressionMethods.None
            };

            _httpClient = new HttpClient(handler, true)
            {
                BaseAddress = new Uri(BaseUrl),
            };


                    //json형식으로 받기 위해 Accept 헤더설정
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.ConnectionClose = false; // Keep-Alive 유지


            //var sp = ServicePointManager.FindServicePoint(_httpClient.BaseAddress);
            //sp.ConnectionLeaseTimeout = -1;//60 * 1000; // 1분마다 새 연결 생성
            //sp.MaxIdleTime = Timeout.Infinite; // Idle 연결 무한 유지
            //sp.UseNagleAlgorithm = false; // 작은 패킷 처리 최적화
        }

        public static void Close()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
        }


        #region Method
        public static async Task<ApiResponse> WaitingForServerRespose(int retryCount = 100)
        {
            Open();
            ApiResponse response = new ApiResponse("");
            for (int i = 0; i < retryCount; i++)
            {
                response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/common/init"), TimeSpan.FromSeconds(3));
                if (response.Result)
                {
                    _isConnected = true;
                    break;
                }
                await Task.Delay(1000);
            }
            return response;
        }

        private static async Task<ApiResponse> SendAsync(HttpRequestMessage request, TimeSpan timeout)
        {
            if (IsDebug)
            {
                await Task.Delay(200);
                return new ApiResponse { Result = true, Data = "Debug" };
            }

            //Open();
            if (_httpClient == null)
                return new ApiResponse("No connection.");
            else if (_cts != null)
                return new ApiResponse("API is running...");

            if (!string.IsNullOrEmpty(_sessionId))
                request.Headers.Add("Session-Id", _sessionId);
            //request.Headers.AcceptEncoding.Clear();

            //.Headers.Add("Cookie", $"shelf_session_id={_sessionId}");
            //request.Headers.Add("cookie", $"sessionId={_sessionId}");

            _cts = new CancellationTokenSource(timeout);
            try
            {
                //var response = await _httpClient.SendAsync(request, _cts.Token);
                string json;
                //var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                var response = await _httpClient.SendAsync(request, _cts.Token);
                var bytes = await response.Content.ReadAsByteArrayAsync();
                if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                {
                    using (var ms = new MemoryStream(bytes))
                    using (var gz = new GZipStream(ms, CompressionMode.Decompress))
                    using (var outMs = new MemoryStream())
                    {
                        await gz.CopyToAsync(outMs, 81920, _cts.Token);
                        bytes = outMs.ToArray();
                    }
                }

                json = Encoding.UTF8.GetString(bytes);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse($"{response.StatusCode}({response.StatusCode.GetHashCode()}) : {json}")
                    {
                        IsExpired = response.StatusCode == HttpStatusCode.Unauthorized
                    };
                }

                return JsonConvert.DeserializeObject<ApiResponse>(json);

                //return JsonConvert.DeserializeObject<ApiResponse>(json)
                //return response.IsSuccessStatusCode
                //    ? JsonConvert.DeserializeObject<ApiResponse>(json)
                //    : new ApiResponse($"{response.StatusCode}({response.StatusCode.GetHashCode()}) : {json}", isExpired: response.StatusCode == HttpStatusCode.Unauthorized);
            }
            catch (OperationCanceledException)
            {
                return new ApiResponse("API response time-out") { IsTimeout = true };
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.ToString());
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                //Close();
            }
        }

        private static async Task<ApiResponse> SendAsyncGzip(HttpRequestMessage request, TimeSpan timeout)
        {
            //Open();
            if (_httpClient == null)
                return new ApiResponse("No connection.");
            else if (_cts != null)
                return new ApiResponse("API is running...");

            //request.Headers.AcceptEncoding.Clear();
            if (!string.IsNullOrEmpty(_sessionId))
                request.Headers.Add("Session-Id", _sessionId);

            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            _cts = new CancellationTokenSource(timeout);
            try
            {
                var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    _cts.Token
                );

                var bytes = await response.Content.ReadAsByteArrayAsync();
                using (MemoryStream ms = new MemoryStream(bytes))
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
                using (MemoryStream outMs = new MemoryStream())
                {
                    await gz.CopyToAsync(outMs, 81920, _cts.Token);
                    bytes = outMs.ToArray();
                }

                var json = Encoding.UTF8.GetString(bytes);

                return response.IsSuccessStatusCode
                    ? JsonConvert.DeserializeObject<ApiResponse>(json)
                    : new ApiResponse($"{response.StatusCode}({(int)response.StatusCode}) : {json}");
            }
            catch (OperationCanceledException)
            {
                return new ApiResponse("API response time-out");
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.ToString());
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                //Close();
            }
        }
        private static async Task<ApiResponse> Get(string requestUri, TimeSpan timeout)
        {
            if (_cts != null)
                return new ApiResponse("API is running...");

            _cts = new CancellationTokenSource(timeout);
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
                if (_sessionId != null)
                {
                    req.Headers.Add("Cookie", $"shelf_session_id={_sessionId}");
                }
                var response = await _httpClient.SendAsync(req, _cts.Token);
                //var response = await _httpClient.GetAsync(requestUri, _cts.Token);
                string json = await response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode
                    ? JsonConvert.DeserializeObject<ApiResponse>(json)
                    : new ApiResponse($"{response.StatusCode}({response.StatusCode.GetHashCode()}) : {json}");

                //if (response.IsSuccessStatusCode)
                //    return JsonConvert.DeserializeObject<ApiResponse>(json);
                //else
                //    return new ApiResponse(json);
            }
            catch (OperationCanceledException)
            {
                return new ApiResponse("API response time-out");
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
            }
        }

        private static async Task<ApiResponse> Post(string requestUri, TimeSpan timeout)
        {
            if (_cts != null)
                return new ApiResponse("API is running...");

            _cts = new CancellationTokenSource(timeout);
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

                if (!string.IsNullOrEmpty(_sessionId))
                {
                    request.Headers.Add("Cookie", $"shelf_session_id={_sessionId}");
                }
                var response = await _httpClient.SendAsync(request, _cts.Token);
                //var response = await _httpClient.PostAsync(requestUri, null, _cts.Token);
                string json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<ApiResponse>(json);
                else
                    return new ApiResponse($"{response.StatusCode}({response.StatusCode.GetHashCode()}) : {json}");
            }
            catch (OperationCanceledException)
            {
                return new ApiResponse("API response time-out");
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex.Message);
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
            }
        }
        #endregion

        #region Common
        public static async Task<ApiResponse> GetSwVersion()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/common/version"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> GetSeirialNumber()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/common/sn"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> SetSeirialNumber(string serialNumber)
        {
            string requestUri = $"/common/sn?val={serialNumber}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(5));
        }
        #endregion

        #region Params
        public static async Task<ApiResponse> GetCommonParams()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/params/common"), TimeSpan.FromSeconds(3));
        }

        public static async Task<ApiResponse> SetCommonParams()
        {
            string queryString = CommonParams.Instance.ToQueryString();
            string requestUri = $"/params/common?{queryString}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> GetDnaParams()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/params/dna"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> SetDnaParams()
        {
            string queryString = DnaParams.Instance.ToQueryString();
            string requestUri = $"/params/dna?{queryString}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> GetUvParams()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/params/uv"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> SetUvParams()
        {
            string queryString = UvParams.Instance.ToQueryString();
            string requestUri = $"/params/uv?{queryString}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> GetCuvetteParams()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/params/cuvette"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> SetCuvetteParams()
        {
            string queryString = CuvetteParams.Instance.ToQueryString();
            string requestUri = $"/params/cuvette?{queryString}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(5));
        }
        #endregion

        #region IO
        public static async Task<ApiResponse> GetFwVersion()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/io/version"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> MoveLimit()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, "/io/z-limit"), TimeSpan.FromSeconds(30));
        }

        public static async Task<ApiResponse> MoveAlign()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, "/io/z-align"), TimeSpan.FromSeconds(30));
        }

        public static async Task<ApiResponse> Move(int pos)
        {
            string requestUri = $"/io/z-move?val={pos}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(30));
        }

        public static async Task<ApiResponse> MoveStep(int step)
        {
            string requestUri = $"/io/z-step?val={step}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(30));
        }

        public static async Task<ApiResponse> MovePedestal()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, "/io/p-move"), TimeSpan.FromSeconds(10));
        }

        public static async Task<ApiResponse> MoveCuvette()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, "/io/c-move"), TimeSpan.FromSeconds(10));
        }

        public static async Task<ApiResponse> GetTemperature()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/io/temp"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> SetTemperature(double temp)
        {
            string requestUri = $"/io/temp?val={temp}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> GetXenon()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/io/xenon"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> SetXenon(int xenon)
        {
            string requestUri = $"/io/xenon?val={xenon}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(5));
        }
        #endregion

        #region Spectrometer
        public static async Task<ApiResponse> GetSpectrometerSN()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/spec/sn"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> GetSpectrometerPN()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/spec/pn"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> GetWave()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/spec/wave"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> Read(int readCount, int readTime)
        {
            string requestUri = $"/spec/read?count={readCount}&readtime={readTime}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> ReadDark()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/spec/read-dark"), TimeSpan.FromSeconds(5));
        }
        #endregion

        #region DB
        public static async Task<ApiResponse> Login(string id, string pw)
        {
            string requestUri = $"/db/login?id={Uri.EscapeDataString(id)}&pw={Uri.EscapeDataString(pw)}";
            var response = await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(5));
            if (response.Result)
                _sessionId = response.Data?.ToString() ?? null;

            return response;
        }

        public static async Task<ApiResponse> MasterLogin(string pw)
        {
            string requestUri = $"/db/login?id=master&pw={Uri.EscapeDataString(pw)}";
            var response = await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(5));
            //if (response.Result)
            //    _sessionId = response.Data?.ToString() ?? null;

            return response;
        }

        public static async Task<ApiResponse> GetUsers()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/db/users"), TimeSpan.FromSeconds(5));
        }

        public static async Task<ApiResponse> GetExperiments(DateTime from, DateTime to)
        {
            string requestUri = $"/db/experiments?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            //return await SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), TimeSpan.FromSeconds(10));
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), TimeSpan.FromSeconds(10));
        }

        public static async Task<ApiResponse> GetMeasurements(int id)
        {
            string requestUri = $"/db/measurements?id={id}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), TimeSpan.FromSeconds(10));
        }

        public static async Task<ApiResponse> GetDB()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/db/download"), TimeSpan.FromSeconds(30));
        }
        #endregion

        #region Function
        public static async Task<ApiResponse> Blank(string type)
        {
            string requestUri = $"/func/blank?type={type}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), TimeSpan.FromSeconds(10));
        }

        public static async Task<ApiResponse> Sample(string type, string name, string start = "", string end = "", string factor = "", string factorName = "")
        {
            string requestUri = $"/func/meas?type={type}&name={name}&start={start}&end={end}&factor={factor}&factor_name={factorName}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), TimeSpan.FromSeconds(10));
        }

        public static async Task<ApiResponse> EndExperiment(string type, string name = "")
        {
            string requestUri = $"/func/end?type={type}&name={name}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), TimeSpan.FromSeconds(10));
        }
        #endregion

        #region Log
        public static async Task<ApiResponse> GetLogDates()
        {
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/log/dates"), TimeSpan.FromSeconds(10));
        }

        //Date형식은 yyyy-MM-dd
        public static async Task<ApiResponse> GetLogText(string date)
        {
            string requestUri = $"/log/text?date={date}";
            return await SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), TimeSpan.FromSeconds(10));
        }
        #endregion
    }
}
