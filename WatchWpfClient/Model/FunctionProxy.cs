using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WatchFunctions.Dtos;
using WatchWpfClient.Model.Dtos;

namespace WatchWpfClient.Model
{
    public class FunctionProxy
    {
        private WatchConfig? _config;

        private string _sessionToken;

        public FunctionProxy()
        {
            var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Model\WatchConfig.json");
            GetConfig(configPath);
        }

        public async Task<bool> NewUser(Dtos.NewUserDto newUserDto)
        {
            var url = $"{_config!.URL}NewUser?code={_config.NewUserCode}";
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync(url, newUserDto);
            var result = await response.Content.ReadAsStringAsync();
            return result.StartsWith($"Welcome {newUserDto.Handle}");
        }

        public async Task<string> Login(string handle, string password)
        {
            var url = $"{_config!.URL}Login?code={_config.LoginCode}&handle={handle}&password={password}";
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            _sessionToken = result;
            return result;
        }

        public async Task<List<Message>> Read(string channelNumber)
        {
            if (_sessionToken == null || _sessionToken == string.Empty)
                throw new InvalidOperationException("No current session");
            var url = $"{_config!.URL}Read?code={_config.ReadCode}&channelNumber={channelNumber}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sessionToken);
            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var dtos = JsonConvert.DeserializeObject<List<MessageDto>>(result);
            return dtos.Select(dto => new Message(dto)).ToList();
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
