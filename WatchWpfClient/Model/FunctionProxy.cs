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

        private string? _sessionToken;

        public FunctionProxy()
        {
            var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, @"WatchConfig.json");
            GetConfig(configPath);
        }

        private void ValidateSessionExists()
        {
            if (_sessionToken == null || _sessionToken == string.Empty)
                throw new InvalidOperationException("No current session");
        }

        public async Task<string> NewUser(NewUserDto newUserDto)
        {
            var url = $"{_config!.URL}NewUser?code={_config.NewUserCode}";
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync(url, newUserDto);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return result;
            throw new InvalidOperationException("Invalid new user response");
        }

        public async Task<LoginDto> Login(string handle, string password)
        {
            var url = $"{_config!.URL}Login?code={_config.LoginCode}&handle={WebUtility.UrlEncode(handle)}&password={WebUtility.UrlEncode(password)}";
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var dto = JsonConvert.DeserializeObject<LoginDto>(result);
                _sessionToken = dto.SessionToken;
                return dto;
            }
            throw new InvalidOperationException(result);
        }

        public async Task<string> PasswordReset(string handle, string channelNumber, string password)
        {
            var url = $"{_config!.URL}PasswordReset?code={_config.PasswordResetCode}";
            var passwordResetDto = new PasswordResetDto()
            {
                ChannelNumber = channelNumber,
                Handle = handle,
                Password = password
            };
            // Encrypt signature value using private key

            var client = new HttpClient();
            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var dto = JsonConvert.DeserializeObject<LoginDto>(result);
                _sessionToken = dto.SessionToken;
                return dto;
            }
            throw new InvalidOperationException(result);
        }

        public async Task<string> Write(MessageDto messageDto)
        {
            ValidateSessionExists();
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
            ValidateSessionExists();
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
            ValidateSessionExists();
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
            ValidateSessionExists();
            var url = $"{_config!.URL}Read?code={_config.ReadCode}&channelNumber={channelNumber}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sessionToken);
            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(result);
            var dtos = JsonConvert.DeserializeObject<List<MessageDto>>(result);
            return dtos.OrderBy(m => m.TimeStamp).ToList();
        }

        public async Task<List<string>> ChannelHandles(string channelNumber)
        {
            ValidateSessionExists();
            var url = $"{_config!.URL}ChannelHandles?code={_config.ChannelHandlesCode}&channelNumber={channelNumber}";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sessionToken);
            var response = await client.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(result);
            var handles = JsonConvert.DeserializeObject<List<string>>(result);
            return handles.OrderBy(h => h).ToList();
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
