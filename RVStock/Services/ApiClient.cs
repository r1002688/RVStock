using System.Net.Http;
using System.Net.Http.Json;
using RVStockSHARED.Models;

namespace RVStock.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;

        public ApiClient()
        {
            _http = new HttpClient { BaseAddress = new Uri("http://localhost:5000/") };
        }

        // Onderdelen
        public Task<List<Onderdeel>?> GetOnderdelenAsync() =>
            _http.GetFromJsonAsync<List<Onderdeel>>("api/onderdelen");

        public Task<HttpResponseMessage> ScanAsync(string barcode) =>
            _http.PostAsJsonAsync("api/onderdelen/scan", barcode);

        public Task<HttpResponseMessage> CreateOnderdeelAsync(Onderdeel o) =>
            _http.PostAsJsonAsync("api/onderdelen", o);

        public Task<HttpResponseMessage> UpdateOnderdeelAsync(Onderdeel o) =>
            _http.PutAsJsonAsync($"api/onderdelen/{o.Id}", o);

        public Task<HttpResponseMessage> DeleteOnderdeelAsync(int id) =>
            _http.DeleteAsync($"api/onderdelen/{id}");

        // Leveranciers
        public Task<List<Leverancier>?> GetLeveranciersAsync() =>
            _http.GetFromJsonAsync<List<Leverancier>>("api/leveranciers");

        public Task<HttpResponseMessage> CreateLeverancierAsync(Leverancier l) =>
            _http.PostAsJsonAsync("api/leveranciers", l);

        public Task<HttpResponseMessage> UpdateLeverancierAsync(Leverancier l) =>
            _http.PutAsJsonAsync($"api/leveranciers/{l.Id}", l);

        public Task<HttpResponseMessage> DeleteLeverancierAsync(int id) =>
            _http.DeleteAsync($"api/leveranciers/{id}");

        // Bestelijnen
        public Task<List<Bestellijn>?> GetOpenBestelijnenAsync() =>
            _http.GetFromJsonAsync<List<Bestellijn>>("api/bestelijnen/open");
    }
}
