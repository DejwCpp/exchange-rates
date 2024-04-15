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
        private Label GoldLabel;
        private double GoldPrice;
        private List<RootObject> RateData;
        private double EntryValue;

        public MainPage()
        {
            InitializeComponent();
            LoadAPIInfo();
            SetEntryProperties();
            AddGoldLabel();
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
            public double mid { get; set; }
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
                    double price = EntryValue / rate.mid;

                    Label rateLabel = new Label
                    {
                        Text = $"{rate.code}: {Math.Round(price, 2)}",
                        FontSize = 18,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, marginTop, 0, 0)
                    };
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

            RemoveMainGridLabels();

            if (entry.Text.Length > 0 )
            {
                // if illegal sign entered - prevent from writing it
                if (!LegalSign(newText[newText.Length - 1]))
                {
                    entry.Text = newText.Substring(0, newText.Length - 1);
                    return;
                }

                EntryValue = Convert.ToDouble(entry.Text);

                AddGoldLabel();
                GoldLabel.Text = "Gold: " + Math.Round(EntryValue / GoldPrice, 2) + " grams";

                CreateRateLabels(160);

                return;
            }

            AddGoldLabel();
            GoldLabel.Text = "Gold: 0 grams";

            EntryValue = 0;
            CreateRateLabels(160);
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

        private void AddGoldLabel()
        {
            GoldLabel = new Label
            {
                Text = "Gold: 0 grams",
                FontSize = 24,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 110, 0, 0)
            };
            mainGrid.Children.Add(GoldLabel);
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
