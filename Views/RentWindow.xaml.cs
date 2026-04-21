using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SocietyManagementSystem.Services;
using SocietyManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SocietyManagementSystem
{
    public partial class RentWindow : Window
    {
        private readonly MongoDbService _mongoDbService;
        public ObservableCollection<RentViewModel> Rents { get; set; } = new();
        public ObservableCollection<Tenant> Tenants { get; set; } = new();
        private RentViewModel? selectedRent;

        public RentWindow(string societyName)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            _mongoDbService = new MongoDbService(societyName);
            dgRents.ItemsSource = Rents;
            this.Loaded += async (s, e) => 
            {
                await LoadTenantsAsync();
                await LoadRentsAsync();
            };
        }

        private async Task LoadTenantsAsync()
        {
            try
            {
                var tenants = await _mongoDbService.GetAllTenantsAsync();
                Tenants.Clear();
                foreach (var tenant in tenants)
                {
                    Tenants.Add(tenant);
                }
                cmbTenant.ItemsSource = Tenants;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tenants: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadRentsAsync()
        {
            try
            {
                var rents = await _mongoDbService.GetAllRentsAsync();
                Rents.Clear();
                foreach (var rent in rents)
                {
                    var tenant = Tenants.FirstOrDefault(t => t.Id == rent.TenantId);
                    var viewModel = new RentViewModel
                    {
                        Id = rent.Id,
                        TenantId = rent.TenantId,
                        TenantName = tenant != null ? $"{tenant.FirstName} {tenant.LastName}" : "Unknown",
                        Amount = rent.Amount,
                        DueDate = rent.DueDate,
                        PaidDate = rent.PaidDate,
                        Status = rent.Status,
                        Month = rent.Month,
                        Year = rent.Year
                    };
                    Rents.Add(viewModel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rents: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnAddRent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbTenant.SelectedItem is not Tenant selectedTenant)
                {
                    MessageBox.Show("Please select a tenant.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtAmount.Text, out decimal amount))
                {
                    MessageBox.Show("Please enter a valid amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dpDueDate.SelectedDate == null)
                {
                    MessageBox.Show("Please select a due date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtMonth.Text, out int month))
                {
                    MessageBox.Show("Please enter a valid month.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtYear.Text, out int year))
                {
                    MessageBox.Show("Please enter a valid year.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var rent = new Rent
                {
                    TenantId = selectedTenant.Id,
                    Amount = amount,
                    DueDate = dpDueDate.SelectedDate.Value,
                    Month = month,
                    Year = year,
                    Status = RentStatus.Pending
                };

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(rent);
                if (!Validator.TryValidateObject(rent, validationContext, validationResults, true))
                {
                    string errors = string.Join("\n", validationResults.Select(v => v.ErrorMessage));
                    MessageBox.Show($"Validation errors:\n{errors}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool success = await _mongoDbService.AddRentAsync(rent);
                if (success)
                {
                    await LoadRentsAsync();
                    ClearFields();
                    MessageBox.Show("Rent added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to add rent.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding rent: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnMarkPaid_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRent != null && selectedRent.Id != null)
            {
                try
                {
                    bool success = await _mongoDbService.MarkRentAsPaidAsync(selectedRent.Id, DateTime.Now);
                    if (success)
                    {
                        await LoadRentsAsync();
                        MessageBox.Show("Rent marked as paid!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to mark as paid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error marking rent as paid: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BtnDeleteRent_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRent != null && selectedRent.Id != null)
            {
                bool success = await _mongoDbService.DeleteRentAsync(selectedRent.Id);
                if (success)
                {
                    Rents.Remove(selectedRent);
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to delete rent.");
                }
            }
        }

        private void DgRents_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedRent = dgRents.SelectedItem as RentViewModel;
        }

        private void ClearFields()
        {
            cmbTenant.SelectedItem = null;
            txtAmount.Text = "";
            dpDueDate.SelectedDate = null;
            txtMonth.Text = "";
            txtYear.Text = "";
            dgRents.UnselectAll();
            selectedRent = null;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class RentViewModel
    {
        public string? Id { get; set; }
        public string? TenantId { get; set; }
        public string? TenantName { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public RentStatus Status { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}