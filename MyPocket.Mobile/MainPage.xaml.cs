using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.ObjectModel;

namespace MyPocket.Mobile
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<UserDTO> Users { get; set; } = new();

        public MainPage()
        {
            InitializeComponent();
            UsersListView.ItemsSource = Users;
            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {
                // Adjust baseUrl to match the actual API endpoint
                string baseUrl = "http://10.0.2.2:5000"; // 10.0.2.2 is the host loopback for the Android emulator
                var httpClient = new HttpClient();
                var users = await httpClient.GetFromJsonAsync<List<UserDTO>>($"{baseUrl}/api/Users");
                if (users != null)
                {
                    Users.Clear();
                    foreach (var user in users)
                        Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("錯誤", ex.Message, "OK");
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }

    public class UserDTO
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
    }
}
