using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using NftsArt.Model.Dtos.User;
using NftsArt.Model.Helpers;
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

    public async Task<Result<T>> GetFromJsonAsync<T>(string path)
    {
        await SetAuthorizeHeader();
        return await httpClient.GetFromJsonAsync<Result<T>>(path);
    }

    public async Task<Result<T1>> PostAsync<T1, T2>(string path, T2 postDto)
    {
        await SetAuthorizeHeader();
        var res = await httpClient.PostAsJsonAsync(path, postDto);

        if (res == null || !res.IsSuccessStatusCode)
            return default!;
        
        return await res.Content.ReadFromJsonAsync<Result<T1>>();
    }

    public async Task<Result<T1>> PutAsync<T1, T2>(string path, T2 postDto)
    {
        await SetAuthorizeHeader();
        var res = await httpClient.PutAsJsonAsync(path, postDto);

        if (res == null || !res.IsSuccessStatusCode)
            return default!;

        return await res.Content.ReadFromJsonAsync<Result<T1>>();

    }
    public async Task<Result<T>> DeleteAsync<T>(string path)
    {
        await SetAuthorizeHeader();
        return await httpClient.DeleteFromJsonAsync<Result<T>>(path);
    }
}