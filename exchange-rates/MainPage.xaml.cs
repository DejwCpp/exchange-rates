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
        public GoldInfoModel GlobalGoldData;

        public MainPage()
        {
            InitializeComponent();
            LoadGoldInfo();
            SetEntryProperties();
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

                    GlobalGoldData = latestGoldPrice;

                    GoldInfo = new Label
                    {
                        Text = $"Gold price: {latestGoldPrice.Price} per gram",
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
        
        // signs that can be used in entry
        private bool LegalSign(char ch)
        {
            char[] legalSigns = {
                ',', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'
            };

            return Array.IndexOf(legalSigns, ch) != -1;
        }

        private void SetEntryProperties()
        {
            entry.Focus();
            entry.IsTextPredictionEnabled = false;

            entry.TextChanged += (sender, args) =>
            {
                string newText = args.NewTextValue;
                string filteredText = "";

                foreach (char sign in newText)
                {
                    if (LegalSign(sign))
                    {
                        // add number format logic here e.g. 50000 -> 50 000

                        filteredText += sign;
                    }
                }
                entry.Text = filteredText;
            };
        }

        private void BtnSubmitClicked(object sender, EventArgs e)
        {
            double money = Convert.ToDouble(entry.Text);

            goldAmount.Text = "You can buy: " + Math.Round(money / GlobalGoldData.Price, 2) + " grams of gold";
        }
    }
}
