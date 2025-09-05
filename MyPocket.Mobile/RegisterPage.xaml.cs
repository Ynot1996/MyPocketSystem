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
                ErrorLabel.Text = "請填寫所有必填欄位";
                ErrorLabel.IsVisible = true;
                return;
            }
            if (password != confirmPassword)
            {
                ErrorLabel.Text = "密碼與確認密碼不一致";
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
                    await DisplayAlert("成功", "註冊成功，請登入！", "OK");
                    await Navigation.PopAsync(); // 返回上一頁
                }
                else
                {
                    ErrorLabel.Text = "註冊失敗，請檢查資料";
                    ErrorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = $"錯誤: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }
    }
}
