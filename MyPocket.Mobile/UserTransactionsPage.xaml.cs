using System.Net.Http;
using System.Net.Http.Json;
using MyPocket.Mobile.DTOs;

namespace MyPocket.Mobile
{
    public partial class UserTransactionsPage : ContentPage
    {
        private Guid _userId;
        public List<TransactionDTO> Transactions { get; set; } = new();

        public UserTransactionsPage(Guid userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadTransactions();
        }

        private async void LoadTransactions()
        {
            try
            {
                string baseUrl = "http://10.0.2.2:5239";
                var httpClient = new HttpClient();
                var transactions = await httpClient.GetFromJsonAsync<List<TransactionDTO>>($"{baseUrl}/api/Transactions/user/{_userId}");
                if (transactions != null)
                {
                    TransactionsView.ItemsSource = transactions;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("¿ù»~", ex.Message, "OK");
            }
        }
    }
}
