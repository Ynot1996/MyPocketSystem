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
                ErrorLabel.Text = "請輸入 Email 和密碼";
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
                    await DisplayAlert("成功", "登入成功！", "OK");
                    Application.Current.MainPage = new MainPage();
                }
                else
                {
                    ErrorLabel.Text = "登入失敗，請檢查帳號密碼";
                    ErrorLabel.IsVisible = true;
                }
            }

            catch (Exception ex)
            {
                ErrorLabel.Text = $"錯誤: {ex.ToString()}"; // 開發用
                //ErrorLabel.Text = "系統錯誤，請稍後再試"; // 上線用
                ErrorLabel.IsVisible = true;
            }
        }
    }
}
    
