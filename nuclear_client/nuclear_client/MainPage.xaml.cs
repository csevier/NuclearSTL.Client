using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace nuclear_client
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            var task = Task.Run(async () => await UpdateGeolocation());
        }
        private async Task UpdateGeolocation() 
        {
            for (;;)
            {
                try
                {
                    var location = await Geolocation.GetLocationAsync();
                    if (location != null)
                    {
                        string status = await SaveLocationToServer(location.Latitude, location.Longitude, DateTime.Now);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Lat.Text = location.Latitude.ToString();
                            Lon.Text = location.Longitude.ToString();
                            Status.Text = status;
                        });
                    }
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    // Handle not supported on device exception
                }
                catch (FeatureNotEnabledException fneEx)
                {
                    // Handle not enabled on device exception
                }
                catch (PermissionException pEx)
                {
                    // Handle permission exception
                }
                catch (Exception ex)
                {
                    // Unable to get location
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Status.Text = "Offline";
                    });
                }
            }
        }

        private async Task<string> SaveLocationToServer(double lat, double lon, DateTime time)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                //"2021-03-17T02:24:35.535"
                string b = time.ToString("yyyy-MM-ddTHH:mm:ss");
                string payload = $@"{{""timeStamp"": ""{b}"", ""lat"": {lat}, ""lon"": {lon}}}";
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("http://lunagis.com:5001/Geostamp"),
                    Content = content,
                    };

                HttpResponseMessage result = await client.SendAsync(request);
                return result.StatusCode.ToString();

            }
        }
    }
}