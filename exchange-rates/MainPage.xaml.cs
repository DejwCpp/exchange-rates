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
        private double GoldPrice;
        private List<RootObject> RateData;
        private double EntryValue;

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

                    dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);

                    GoldPrice = jsonObject[0].cena;

                    GoldInfo = new Label
                    {
                        Text = $"Gold price: {GoldPrice} per gram",
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

                    RateData = JsonConvert.DeserializeObject<List<RootObject>>(responseBody);

                    CreateRateLabels(160);
                }
                else
                {
                    await DisplayAlert("Data retrieval error", $"Error message: {ratesResponse.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
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

        private void CreateRateLabels(int marginTop)
        {
            foreach (var item in RateData)
            {
                foreach (var rate in item.rates)
                {
                    Label rateLabel = new Label
                    {
                        Text = $"{rate.code}: {rate.mid * Convert.ToInt32(EntryValue)}",
                        FontSize = 18,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, marginTop, 0, 0)
                    };
                    mainGrid.SetRow(rateLabel, 1);
                    mainGrid.Children.Add(rateLabel);

                    marginTop += 30;
                }
            }
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
                EntryValue = Convert.ToDouble(entry.Text);

                goldAmount.Text = "You can buy: " + Math.Round(EntryValue / GoldPrice, 2) + " grams of gold";

                RemoveMainGridLabels();
                CreateRateLabels(160);
            }
            else
            {
                goldAmount.Text = "You can buy: 0 grams of gold";

                RemoveMainGridLabels();
                EntryValue = 0;
                CreateRateLabels(160);
            }
        }

        private void RemoveMainGridLabels()
        {
            List<VisualElement> elementsToRemove = new List<VisualElement>();

            foreach (VisualElement child in mainGrid.Children)
            {
                if (child != entry && child != entry_zl)
                {
                    elementsToRemove.Add(child);
                }
            }
            foreach (VisualElement elementToRemove in elementsToRemove)
            {
                mainGrid.Children.Remove(elementToRemove);
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
