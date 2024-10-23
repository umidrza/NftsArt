using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Newtonsoft.Json;
using NftsArt.Model.Dtos.User;
using System.Net.Http.Headers;

namespace NftsArt.Web;

public class ApiClient(
        HttpClient httpClient, 
        ProtectedLocalStorage localStorage)
{
    public async Task SetAuthorizeHeader()
    {
        var sessionState = (await localStorage.GetAsync<LoginResponseDto>("sessionState")).Value;
        if (sessionState != null && !string.IsNullOrEmpty(sessionState.Token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionState.Token);
        }
    }

    public async Task<T> GetFromJsonAsync<T>(string path)
    {
        await SetAuthorizeHeader();
        return await httpClient.GetFromJsonAsync<T>(path);
    }

    public async Task<T1> PostAsync<T1, T2>(string path, T2 postDto)
    {
        await SetAuthorizeHeader();
        var res = await httpClient.PostAsJsonAsync(path, postDto);
        if (res != null && res.IsSuccessStatusCode)
        {
            var content = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T1>(content);
        }

        return default;
    }

    public async Task<T> PostAsync<T>(string path)
    {
        await SetAuthorizeHeader();
        var res = await httpClient.PostAsync(path, null!);
        if (res != null && res.IsSuccessStatusCode)
        {
            var content = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        return default;
    }

    public async Task<T1> PutAsync<T1, T2>(string path, T2 postDto)
    {
        await SetAuthorizeHeader();
        var res = await httpClient.PutAsJsonAsync(path, postDto);
        if (res != null && res.IsSuccessStatusCode)
        {
            var content = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T1>(content);
        }

        return default;
    }
    public async Task<T> DeleteAsync<T>(string path)
    {
        await SetAuthorizeHeader();
        return await httpClient.DeleteFromJsonAsync<T>(path);
    }
}