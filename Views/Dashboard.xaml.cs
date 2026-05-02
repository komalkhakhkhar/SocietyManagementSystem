using System;
using System.Windows;
using System.Windows.Controls;
using SocietyManagementSystem.Services;
using SocietyManagementSystem.Models;
using System.Linq;

namespace SocietyManagementSystem
{
    public partial class Dashboard : Page
    {
        private readonly MongoDbService _mongoDbService;
        private readonly string _societyName;

        public Dashboard(string societyName)
        {
            InitializeComponent();
        _societyName = societyName;
        _mongoDbService = new MongoDbService(societyName);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadStatsAsync();
    }

        private async System.Threading.Tasks.Task LoadStatsAsync()
        {
            try
            {
                // Load tenants count
                var tenants = await _mongoDbService.GetAllTenantsAsync();
                txtTotalTenants.Text = tenants?.Count.ToString() ?? "0";

                // Load active rents count
                var rents = await _mongoDbService.GetAllRentsAsync();
                var pendingRents = rents?.Where(r => r.Status == RentStatus.Pending).ToList() ?? new System.Collections.Generic.List<Rent>();
                txtActiveRents.Text = pendingRents.Count.ToString();

                // Load pending maintenance count
                var maintenance = await _mongoDbService.GetAllMaintenanceRequestsAsync();
                var openMaintenance = maintenance?.Where(m => m.Status == MaintenanceStatus.Open).ToList() ?? new System.Collections.Generic.List<MaintenanceRequest>();
                txtPendingMaintenance.Text = openMaintenance.Count.ToString();

                // Load active notices count
                var notices = await _mongoDbService.GetAllNoticesAsync();
                var activeNotices = notices?.Where(n => n.IsActive).ToList() ?? new System.Collections.Generic.List<Notice>();
                txtActiveNotices.Text = activeNotices.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard stats: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Card_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Child is Grid grid)
            {
                var stackPanel = grid.Children[0] as StackPanel;
                var title = stackPanel?.Children[0] as TextBlock;
                
                if (title != null)
                {
                    switch (title.Text)
                    {
                        case "Manage Tenants":
                            NavigationService?.Navigate(new TenantWindow(_societyName));
                            break;
                        case "Manage Rents":
                            NavigationService?.Navigate(new RentWindow(_societyName));
                            break;
                        case "Maintenance Requests":
                            NavigationService?.Navigate(new MaintenanceWindow(_societyName));
                            break;
                        case "Announcements":
                            NavigationService?.Navigate(new NoticeWindow(_societyName));
                            break;
                    }
                }
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Login());
        }
    }
}
