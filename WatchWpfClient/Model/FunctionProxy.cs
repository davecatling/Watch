using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using WatchWpfClient.Model.Dtos;

namespace WatchWpfClient.Model
{
    public class FunctionProxy
    {
        private WatchConfig? _config;

        private string _sessionToken;

        public FunctionProxy()
        {
            var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, @"WatchConfig.json");
            GetConfig(configPath);
        }

        public async Task<bool> NewUser(NewUserDto newUserDto)
        {
            var url = $"{_config!.URL}NewUser?code={_config.NewUserCode}";
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync(url, newUserDto);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return true;
            throw new InvalidOperationException(result);
        }

        public async Task<bool> Login(string handle, string password)
        {
            var url = $"{_config!.URL}Login?code={_config.LoginCode}&handle={WebUtility.UrlEncode(handle)}&password={WebUtility.UrlEncode(password)}";
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {                
                _sessionToken = result;
                return true;
            }
            throw new InvalidOperationException(result);
        }

        public async Task<string> Write(MessageDto messageDto)
        {
            if (_sessionToken == null || _sessionToken == string.Empty)
                throw new InvalidOperationException("No current session");
            var url = $"{_config!.URL}Write?code={_config.WriteCode}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sessionToken);
            var response = await client.PostAsJsonAsync(url, messageDto);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return result;
            throw new InvalidOperationException(result);
        }

        public async Task<string> GrantAccess(string channelNumber, string handle)
        {
            if (_sessionToken == null || _sessionToken == string.Empty)
                throw new InvalidOperationException("No current session");
            var url = $"{_config!.URL}GrantAccess?code={_config.GrantAccessCode}&channelNumber={channelNumber}&handle={WebUtility.UrlEncode(handle)}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sessionToken);
            var response = await client.PostAsync(url, null);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return result;
            throw new InvalidOperationException(result);
        }

        public async Task<string> PublicKey(string handle)
        {
            if (_sessionToken == null || _sessionToken == string.Empty)
                throw new InvalidOperationException("No current session");
            var url = $"{_config!.URL}PublicKey?code={_config.PublicKeyCode}&handle={WebUtility.UrlEncode(handle)}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sessionToken);
            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return result;
            throw new InvalidOperationException(result);
        }

        public async Task<List<MessageDto>> Read(string channelNumber)
        {
            if (_sessionToken == null || _sessionToken == string.Empty)
                throw new InvalidOperationException("No current session");
            var url = $"{_config!.URL}Read?code={_config.ReadCode}&channelNumber={channelNumber}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sessionToken);
            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(result);
            var dtos = JsonConvert.DeserializeObject<List<MessageDto>>(result);
            //var messages = dtos.Select(dto => new Message(dto)).ToList();
            return dtos.OrderBy(m => m.TimeStamp).ToList();
        }

        private void GetConfig(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File {path} not found");
            var serializer = new JsonSerializer();
            using var streamReader = new StreamReader(path);
            using var jsonReader = new JsonTextReader(streamReader);
            _config = serializer.Deserialize<WatchConfig>(jsonReader);
        }
    }
}
