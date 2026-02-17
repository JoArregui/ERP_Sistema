using ERP.Domain.Entities;
using System.Net.Http.Json;

namespace ERP.Web.Services
{
    public class ComprasWebService
    {
        private readonly HttpClient _http;

        public ComprasWebService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DocumentoComercial>> GetPedidosPendientes()
        {
            return await _http.GetFromJsonAsync<List<DocumentoComercial>>("api/compras/pendientes") ?? new();
        }

        public async Task<bool> RecepcionarPedido(int id, string numeroAlbaran)
        {
            var response = await _http.PostAsync($"api/compras/recepcionar/{id}?numeroAlbaran={numeroAlbaran}", null);
            return response.IsSuccessStatusCode;
        }
    }
}