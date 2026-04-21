using System;
using System.Collections.ObjectModel;
using System.Windows;
using SocietyManagementSystem.Services;
using SocietyManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace SocietyManagementSystem
{
    public partial class TenantWindow : Window
    {
        private readonly MongoDbService _mongoDbService;
        private readonly string _societyName;
        public ObservableCollection<Tenant> Tenants { get; set; } = new();
        public ObservableCollection<Tenant> AllTenants { get; set; } = new();
        public string[] Titles { get; } = new[] { "Mr", "Mrs", "Miss", "Ms", "Dr" };
        public string[] Genders { get; } = new[] { "Male", "Female", "Other" };
        public string[] Statuses { get; } = new[] { "Active", "Inactive", "Evicted" };
        private Tenant? selectedTenant;

        public TenantWindow(string societyName)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            _societyName = societyName;
            _mongoDbService = new MongoDbService(societyName);
            dgTenants.ItemsSource = Tenants;
            cmbTitle.ItemsSource = Titles;
            cmbGender.ItemsSource = Genders;
            
            // Add "Please select..." option to Status combobox
            var statusList = new List<string> { "Please select status..." };
            statusList.AddRange(Statuses);
            cmbStatus.ItemsSource = statusList;
            cmbStatus.SelectedIndex = 0;  // Set to "Please select..."
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _mongoDbService.InitializeIndexesAsync();
                await LoadTenants();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadTenants()
        {
            var tenants = await _mongoDbService.GetAllTenantsAsync();
            AllTenants.Clear();
            Tenants.Clear();
            foreach (var tenant in tenants)
            {
                AllTenants.Add(tenant);
                Tenants.Add(tenant);
            }
            await LoadStats();
        }

        private async Task LoadStats()
        {
            try
            {
                txtTotalTenants.Text = Tenants.Count.ToString();

                var rents = await _mongoDbService.GetAllRentsAsync();
                var pendingRents = rents?.Where(r => r.Status == RentStatus.Pending).ToList() ?? new List<Rent>();
                txtActiveRents.Text = pendingRents.Count.ToString();
                
                // Debug
                System.Diagnostics.Debug.WriteLine($"Total Rents: {rents?.Count ?? 0}, Pending Rents: {pendingRents.Count}");
                foreach (var rent in rents ?? new List<Rent>())
                {
                    System.Diagnostics.Debug.WriteLine($"  Rent Status: {rent.Status}, TenantId: {rent.TenantId}");
                }

                var maintenance = await _mongoDbService.GetAllMaintenanceRequestsAsync();
                var openMaintenance = maintenance?.Where(m => m.Status == MaintenanceStatus.Open).ToList() ?? new List<MaintenanceRequest>();
                txtPendingMaintenance.Text = openMaintenance.Count.ToString();
                
                // Debug
                System.Diagnostics.Debug.WriteLine($"Total Maintenance: {maintenance?.Count ?? 0}, Open: {openMaintenance.Count}");
                foreach (var m in maintenance ?? new List<MaintenanceRequest>())
                {
                    System.Diagnostics.Debug.WriteLine($"  Maintenance Status: {m.Status}, Title: {m.Title}");
                }

                var notices = await _mongoDbService.GetAllNoticesAsync();
                var activeNotices = notices?.Where(n => n.IsActive).ToList() ?? new List<Notice>();
                txtActiveNotices.Text = activeNotices.Count.ToString();
                
                // Debug
                System.Diagnostics.Debug.WriteLine($"Total Notices: {notices?.Count ?? 0}, Active: {activeNotices.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadStats Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                MessageBox.Show($"Error loading stats: {ex.Message}\n{ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtTotalTenants.Text = "0";
                txtActiveRents.Text = "0";
                txtPendingMaintenance.Text = "0";
                txtActiveNotices.Text = "0";
            }
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate status selection
                if (string.IsNullOrEmpty(cmbStatus.Text) || cmbStatus.Text == "Please select status...")
                {
                    MessageBox.Show("Please select a valid status.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cmbStatus.Focus();
                    return;
                }

                if (!Enum.TryParse<TenantStatus>(cmbStatus.Text, out var status))
                {
                    MessageBox.Show($"Invalid status: {cmbStatus.Text}. Please select a valid status.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cmbStatus.Focus();
                    return;
                }

                var tenant = new Tenant
                {
                    Title = cmbTitle.Text,
                    FirstName = txtFirstName.Text,
                    MiddleName = txtMiddleName.Text,
                    LastName = txtLastName.Text,
                    Gender = cmbGender.Text,
                    Email = txtEmail.Text,
                    Phone = txtPhone.Text,
                    UnitNumber = txtUnitNumber.Text,
                    Status = status,
                    DateOfBirth = dpDOB.SelectedDate,
                    Anniversary = dpAnniversary.SelectedDate
                };

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(tenant);
                if (!Validator.TryValidateObject(tenant, validationContext, validationResults, true))
                {
                    string errors = string.Join("\n", validationResults.Select(v => v.ErrorMessage));
                    MessageBox.Show($"Validation errors:\n{errors}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool success = await _mongoDbService.AddTenantAsync(tenant);
                if (success)
                {
                    AllTenants.Add(tenant);
                    Tenants.Add(tenant);
                    ClearFields();
                    MessageBox.Show("Tenant added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to add tenant.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding tenant: {ex.Message}\n\n{ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTenant != null)
            {
                try
                {
                    // Validate status selection
                    if (string.IsNullOrEmpty(cmbStatus.Text) || cmbStatus.Text == "Please select status...")
                    {
                        MessageBox.Show("Please select a valid status.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        cmbStatus.Focus();
                        return;
                    }

                    if (!Enum.TryParse<TenantStatus>(cmbStatus.Text, out var status))
                    {
                        MessageBox.Show($"Invalid status: {cmbStatus.Text}. Please select a valid status.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        cmbStatus.Focus();
                        return;
                    }

                    selectedTenant.Title = cmbTitle.Text;
                    selectedTenant.FirstName = txtFirstName.Text;
                    selectedTenant.MiddleName = txtMiddleName.Text;
                    selectedTenant.LastName = txtLastName.Text;
                    selectedTenant.Gender = cmbGender.Text;
                    selectedTenant.Email = txtEmail.Text;
                    selectedTenant.Phone = txtPhone.Text;
                    selectedTenant.UnitNumber = txtUnitNumber.Text;
                    selectedTenant.Status = status;
                    selectedTenant.DateOfBirth = dpDOB.SelectedDate;
                    selectedTenant.Anniversary = dpAnniversary.SelectedDate;

                    var validationResults = new List<ValidationResult>();
                    var validationContext = new ValidationContext(selectedTenant);
                    if (!Validator.TryValidateObject(selectedTenant, validationContext, validationResults, true))
                    {
                        string errors = string.Join("\n", validationResults.Select(v => v.ErrorMessage));
                        MessageBox.Show($"Validation errors:\n{errors}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    bool success = await _mongoDbService.UpdateTenantAsync(selectedTenant);
                    if (success)
                    {
                        dgTenants.Items.Refresh();
                        ClearFields();
                        MessageBox.Show("Tenant updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to update tenant.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating tenant: {ex.Message}\n\n{ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a tenant to edit.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTenant != null && selectedTenant.Id != null)
            {
                bool success = await _mongoDbService.DeleteTenantAsync(selectedTenant.Id);
                if (success)
                {
                    AllTenants.Remove(selectedTenant);
                    Tenants.Remove(selectedTenant);
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to delete tenant.");
                }
            }
        }

        private void DgTenants_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedTenant = dgTenants.SelectedItem as Tenant;
            if (selectedTenant != null)
            {
                cmbTitle.Text = selectedTenant.Title;
                txtFirstName.Text = selectedTenant.FirstName;
                txtMiddleName.Text = selectedTenant.MiddleName;
                txtLastName.Text = selectedTenant.LastName;
                cmbGender.Text = selectedTenant.Gender;
                txtEmail.Text = selectedTenant.Email;
                txtPhone.Text = selectedTenant.Phone;
                txtUnitNumber.Text = selectedTenant.UnitNumber;
                cmbStatus.Text = selectedTenant.Status.ToString();
                dpDOB.SelectedDate = selectedTenant.DateOfBirth;
                dpAnniversary.SelectedDate = selectedTenant.Anniversary;
            }
        }

        private void ClearFields()
        {
            cmbTitle.SelectedIndex = -1;
            txtFirstName.Text = "";
            txtMiddleName.Text = "";
            txtLastName.Text = "";
            cmbGender.SelectedIndex = -1;
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtUnitNumber.Text = "";
            cmbStatus.SelectedIndex = 0;  // Reset to "Please select status..."
            dpDOB.SelectedDate = null;
            dpAnniversary.SelectedDate = null;
            dgTenants.UnselectAll();
            selectedTenant = null;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }

        private void BtnRents_Click(object sender, RoutedEventArgs e)
        {
            RentWindow rentWindow = new RentWindow(_societyName);
            rentWindow.Closed += async (s, ev) =>
            {
                // Refresh stats when RentWindow closes
                await LoadStats();
            };
            rentWindow.Show();
        }

        private void BtnMaintenance_Click(object sender, RoutedEventArgs e)
        {
            MaintenanceWindow maintenanceWindow = new MaintenanceWindow(_societyName);
            maintenanceWindow.Closed += async (s, ev) =>
            {
                // Refresh stats when MaintenanceWindow closes
                await LoadStats();
            };
            maintenanceWindow.Show();
        }

        private void BtnNotices_Click(object sender, RoutedEventArgs e)
        {
            NoticeWindow noticeWindow = new NoticeWindow(_societyName);
            noticeWindow.Closed += async (s, ev) =>
            {
                // Refresh stats when NoticeWindow closes
                await LoadStats();
            };
            noticeWindow.Show();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string search = txtSearch.Text.ToLower();
            Tenants.Clear();
            foreach (var t in AllTenants)
            {
                if (string.IsNullOrEmpty(search) ||
                    (t.FirstName?.ToLower().Contains(search) == true) ||
                    (t.LastName?.ToLower().Contains(search) == true) ||
                    (t.Email?.ToLower().Contains(search) == true))
                {
                    Tenants.Add(t);
                }
            }
        }
    }
}
