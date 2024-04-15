using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;

namespace exchange_rates
{
    public partial class MainPage : ContentPage
    {
        private Label GoldInfo;
        public GoldInfoModel GlobalGoldData;

        public MainPage()
        {
            InitializeComponent();
            LoadAPIInfo();
            SetEntryProperties();
        }

        private async void LoadAPIInfo()
        {
            await GetAPIInfo();
        }

        async Task GetAPIInfo()
        {
            try
            {
                // creates HTTP client
                using var client = new HttpClient();

                string goldPriceURL = "http://api.nbp.pl/api/cenyzlota";
                string ratesPriceURL = "http://api.nbp.pl/api/exchangerates/tables/a";

                // GET request to the API
                HttpResponseMessage goldResponse = await client.GetAsync(goldPriceURL);
                HttpResponseMessage ratesResponse = await client.GetAsync(ratesPriceURL);

                // check if status code is 200 OK for gold
                if (goldResponse.IsSuccessStatusCode)
                {
                    string responseBody = await goldResponse.Content.ReadAsStringAsync();

                    var goldInfoList = System.Text.Json.JsonSerializer.Deserialize<List<GoldInfoModel>>(responseBody);

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
                    await DisplayAlert("Data retrieval error", $"Error message: {goldResponse.StatusCode}", "OK");
                }

                // check if status code is 200 OK for rates
                if (ratesResponse.IsSuccessStatusCode)
                {
                    string responseBody = await ratesResponse.Content.ReadAsStringAsync();

/*                  List<RootObject> data = Newtonsoft.Json.JsonSerializer.Deserialize<List<GoldInfoModel>>(responseBody);*/
                    List<RootObject> data = JsonConvert.DeserializeObject<List<RootObject>>(responseBody);

                    foreach (var item in data)
                    {
                        foreach (var rate in item.rates)
                        {
                            await DisplayAlert(rate.currency, rate.mid.ToString(), "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        public class GoldInfoModel
        {
            [JsonPropertyName ("data")]
            public string Date { get; set; }

            [JsonPropertyName ("cena")]
            public double Price { get; set; }
        }

        public class Rate
        {
            public string currency { get; set; }
            public string code { get; set; }
            public decimal mid { get; set; }
        }

        public class RootObject
        {
            public string table { get; set; }
            public string no { get; set; }
            public DateTime effectiveDate { get; set; }
            public List<Rate> rates { get; set; }
        }

        private void SetEntryProperties()
        {
            entry.Focus();
            entry.IsTextPredictionEnabled = false;

            entry.TextChanged += EntryTextChanged;
        }

        private void EntryTextChanged(object sender, TextChangedEventArgs args)
        {
            string newText = args.NewTextValue;
            string filteredText = "";

            foreach (char sign in newText)
            {
                if (LegalSign(sign))
                {
                    // Add number format logic here e.g. 50000 -> 50 000
                    filteredText += sign;
                }
            }
            entry.Text = filteredText;

            if (!string.IsNullOrEmpty(entry.Text))
            {
                if (double.TryParse(entry.Text, out double money))
                {
                    goldAmount.Text = "You can buy: " + Math.Round(money / GlobalGoldData.Price, 2) + " grams of gold";
                }
            }
            else
            {
                goldAmount.Text = "You can buy: 0 grams of gold";
            }
        }

        // signs that can be used in entry
        private bool LegalSign(char ch)
        {
            char[] legalSigns = {
                ',', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'
            };

            return Array.IndexOf(legalSigns, ch) != -1;
        }
    }
}
