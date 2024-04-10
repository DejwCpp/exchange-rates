using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace exchange_rates
{
    public partial class MainPage : ContentPage
    {
        private Label GoldInfo;

        public MainPage()
        {
            InitializeComponent();
            LoadGoldInfo();
        }

        private async void LoadGoldInfo()
        {
            await GetGoldInfo();
        }

        async Task GetGoldInfo()
        {
            try
            {
                // creates HTTP client
                using var client = new HttpClient();

                string goldPriceURL = "http://api.nbp.pl/api/cenyzlota";

                // GET request to the API
                HttpResponseMessage response = await client.GetAsync(goldPriceURL);

                // check if status code is 200 OK
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var goldInfoList = JsonSerializer.Deserialize<List<GoldInfoModel>>(responseBody);

                    var latestGoldPrice = goldInfoList.FirstOrDefault();

                    GoldInfo = new Label
                    {
                        Text = latestGoldPrice.Price.ToString(),
                        FontSize = 18,
                        TextColor = Colors.White
                    };

                    mainGrid.Children.Add(GoldInfo);
                }
                else
                {
                    await DisplayAlert("Data retrieval error", $"Error message: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        public class GoldInfoModel
        {
            [JsonPropertyName("data")]
            public string Date { get; set; }

            [JsonPropertyName("cena")]
            public double Price { get; set; }
        }
    }
}
