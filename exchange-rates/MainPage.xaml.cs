﻿using System;
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
                    await DisplayAlert("Data retrieval error", $"Error message: {goldResponse.StatusCode}", "OK");
                }

                // check if status code is 200 OK for rates
                if (ratesResponse.IsSuccessStatusCode)
                {
                    string responseBody = await ratesResponse.Content.ReadAsStringAsync();

                    List<ExchangeRatesTable> exchangeRates = JsonSerializer.Deserialize<List<ExchangeRatesTable>>(responseBody);

                    foreach (var table in exchangeRates)
                    {
                        foreach (var rate in table.Rates)
                        {
                            await DisplayAlert(rate.Currency, rate.Mid.ToString(), "OK");
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

        public class ExchangeRatesTable
        {
            public string Table {  get; set; }
            public string No { get; set; }
            public DateTime EffectiveDate {  get; set; }
            public List<Rate> Rates { get; set; }
        }

        public class Rate
        {
            public string Currency { get; set; }
            public string Code { get; set; }
            public double Mid {  get; set; }
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
                Changed();
            };
        }

        private void BtnSubmitClicked(object sender, EventArgs e)
        {
            if (entry.Text.Length > 0)
            {
                double money = Convert.ToDouble(entry.Text);

                goldAmount.Text = "You can buy: " + Math.Round(money / GlobalGoldData.Price, 2) + " grams of gold";
            }
        }

        private void Changed()
        {
            if (entry.Text.Length > 0)
            {
                double money = Convert.ToDouble(entry.Text);

                goldAmount.Text = "You can buy: " + Math.Round(money / GlobalGoldData.Price, 2) + " grams of gold";
            }
        }

        private void btnSubmit_Focused(object sender, FocusEventArgs e)
        {

        }
    }
}
