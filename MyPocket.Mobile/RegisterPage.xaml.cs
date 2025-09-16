using System.Net.Http;
using System.Net.Http.Json;

namespace MyPocket.Mobile
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            ErrorLabel.IsVisible = false;
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;
            var confirmPassword = ConfirmPasswordEntry.Text;
            var nickname = NicknameEntry.Text?.Trim();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ErrorLabel.Text = "�ж�g�Ҧ��������";
                ErrorLabel.IsVisible = true;
                return;
            }
            if (password != confirmPassword)
            {
                ErrorLabel.Text = "�K�X�P�T�{�K�X���@�P";
                ErrorLabel.IsVisible = true;
                return;
            }
            try
            {
                string baseUrl = "http://10.0.2.2:5239";
                var httpClient = new HttpClient();
                var form = new MultipartFormDataContent();
                form.Add(new StringContent(email), "Email");
                form.Add(new StringContent(password), "Password");
                form.Add(new StringContent(confirmPassword), "ConfirmPassword");
                form.Add(new StringContent(nickname ?? ""), "Nickname");

                var response = await httpClient.PostAsync($"{baseUrl}/api/Users", form);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("���\", "���U���\�A�еn�J�I", "OK");
                    await Navigation.PopAsync(); // ��^�W�@��
                }
                else
                {
                    ErrorLabel.Text = "���U���ѡA���ˬd���";
                    ErrorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = $"���~: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }
    }
}
