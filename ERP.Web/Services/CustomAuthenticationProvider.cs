/* using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ERP.Web.Services
{
    public class CustomAuthenticationProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public CustomAuthenticationProvider(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            IEnumerable<Claim> claims;
            try
            {
                // SOPORTE DUAL: Bypass para desarrollo o JWT real
                if (token == "bypass-token")
                {
                    claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "Usuario Desarrollo"),
                        new Claim("EmpresaId", "1"), // ID de empresa por defecto para pruebas
                        new Claim(ClaimTypes.Role, "Admin")
                    };
                }
                else
                {
                    claims = ParseClaimsFromJwt(token);
                }
            }
            catch
            {
                // Si el token está corrupto, limpiamos y devolvemos anónimo
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }

        public void NotifyUserAuthentication(string token)
        {
            IEnumerable<Claim> claims;

            if (token == "bypass-token")
            {
                claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Usuario Desarrollo"),
                    new Claim("EmpresaId", "1"),
                    new Claim(ClaimTypes.Role, "Admin")
                };
            }
            else
            {
                claims = ParseClaimsFromJwt(token);
            }

            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in element.EnumerateArray())
                        {
                            claims.Add(new Claim(kvp.Key, item.ToString()));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
                    }
                }
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
} */





using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Security.Claims;
using ERP.Domain.Constants; // Para los permisos

namespace ERP.Web.Services
{
    public class CustomAuthenticationProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;

        public CustomAuthenticationProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // MODO BYPASS: Si hay token, creamos una identidad con TODOS los permisos
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Administrador de Desarrollo"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            // Añadimos todos los permisos de AppPermissions para que el NavMenu se vea completo
            foreach (var permission in AppPermissions.All)
            {
                claims.Add(new Claim("Permission", permission));
            }

            var identity = new ClaimsIdentity(claims, "bypass");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public void NotifyUserAuthentication(string token) => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        public void NotifyUserLogout() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}