using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class Program
{
    public static async Task Main(string[] args)
    {
    
        string area = "�����������"; // ������ ����� ����
        var apartments = await SearchAndSortApartments(area);

        // �����
        Console.WriteLine("�������� � ������������ ������ ������������ � ��������� ������:");
        foreach (var apartment in apartments)
        {
            double priceByn = double.Parse(apartment["price_byn"].ToString()) / 100.0;  //������� ���������� �����
            Console.WriteLine($"������: {apartment["ad_link"]}, ����: {priceByn:F2} BYN");
        }
    }

    // ����� � ���������� �� ���������
    public static async Task<List<JObject>> SearchAndSortApartments(string area)
    {
        string url = "https://api.kufar.by/search-api/v2/search/rendered-paginated?cat=1010&cur=USD&gtsy=country-belarus~province-minsk~locality-minsk&lang=ru&rnt=2&size=30&typ=let";

        // list �partments
        List<JObject> apartments = await GetApartmentsFromApi(url);

        // ���������� ������� �� ������ � ������������ ������ ������������
        List<JObject> filteredApartments = new List<JObject>();
        foreach (var apartment in apartments)
        {
            var adParameters = apartment["ad_parameters"] as JArray;
            if (adParameters != null)
            {
                bool isOnlineBooking = adParameters.Any(attr => attr["p"]?.ToString() == "booking_enabled" && attr["v"]?.ToObject<bool>() == true);
                var location = adParameters.FirstOrDefault(attr => attr["p"]?.ToString() == "area")?["vl"]?.ToString();

                if (isOnlineBooking && location == area)
                {
                    filteredApartments.Add(apartment);
                }
            }
        }

        // ���������� �� ���������
        filteredApartments.Sort((a, b) =>
        {
            double priceA = double.Parse(a["price_byn"].ToString());
            double priceB = double.Parse(b["price_byn"].ToString());
            return priceA.CompareTo(priceB);
        });

        return filteredApartments;
    }

    // �������� ������ �� API
    public static async Task<List<JObject>> GetApartmentsFromApi(string url)
    {
        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string jsonResponse = await response.Content.ReadAsStringAsync();

        JObject data = JObject.Parse(jsonResponse);
        return data["ads"].ToObject<List<JObject>>();
    }
}

