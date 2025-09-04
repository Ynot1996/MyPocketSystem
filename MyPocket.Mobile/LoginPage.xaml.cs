using System.Net.Http;
using System.Net.Http.Json;

namespace MyPocket.Mobile
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            ErrorLabel.IsVisible = false;
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ErrorLabel.Text = "�п�J Email �M�K�X";
                ErrorLabel.IsVisible = true;
                return;
            }
            try
            {
                string baseUrl = "http://10.0.2.2:5239";
                var httpClient = new HttpClient();
                var loginDto = new { Email = email, Password = password };
                var response = await httpClient.PostAsJsonAsync($"{baseUrl}/api/Account/Login", loginDto);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("���\", "�n�J���\�I", "OK");
                    Application.Current.MainPage = new MainPage();
                }
                else
                {
                    ErrorLabel.Text = "�n�J���ѡA���ˬd�b���K�X";
                    ErrorLabel.IsVisible = true;
                }
            }

            catch (Exception ex)
            {
                ErrorLabel.Text = $"���~: {ex.ToString()}"; // �}�o��
                //ErrorLabel.Text = "�t�ο��~�A�еy��A��"; // �W�u��
                ErrorLabel.IsVisible = true;
            }
        }
    }
}
    
