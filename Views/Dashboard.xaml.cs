using System;
using System.Windows;
using System.Windows.Controls;
using SocietyManagementSystem.Services;
using SocietyManagementSystem.Models;
using System.Linq;

namespace SocietyManagementSystem
{
    public partial class Dashboard : Window
    {
        private readonly MongoDbService _mongoDbService;
        private readonly string _societyName;

        public Dashboard(string societyName)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            _societyName = societyName;
            _mongoDbService = new MongoDbService(societyName);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
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
                            var tenantWindow = new TenantWindow(_societyName);
                            tenantWindow.Show();
                            break;
                        case "Manage Rents":
                            var rentWindow = new RentWindow(_societyName);
                            rentWindow.Show();
                            break;
                        case "Maintenance Requests":
                            var maintenanceWindow = new MaintenanceWindow(_societyName);
                            maintenanceWindow.Show();
                            break;
                        case "Announcements":
                            var noticeWindow = new NoticeWindow(_societyName);
                            noticeWindow.Show();
                            break;
                    }
                }
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }
    }
}
