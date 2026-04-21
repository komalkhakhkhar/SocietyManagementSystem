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
    public partial class MaintenanceWindow : Window
    {
        private readonly MongoDbService _mongoDbService;
        public ObservableCollection<MaintenanceViewModel> Requests { get; set; } = new();
        public ObservableCollection<Tenant> Tenants { get; set; } = new();
        public string[] Categories { get; } = Enum.GetNames(typeof(MaintenanceCategory));
        public string[] Priorities { get; } = Enum.GetNames(typeof(MaintenancePriority));
        private MaintenanceViewModel? selectedRequest;

        public MaintenanceWindow(string societyName)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            _mongoDbService = new MongoDbService(societyName);
            dgRequests.ItemsSource = Requests;
            cmbCategory.ItemsSource = Categories;
            cmbPriority.ItemsSource = Priorities;
            LoadTenants();
            LoadRequests();
        }

        private async void LoadTenants()
        {
            var tenants = await _mongoDbService.GetAllTenantsAsync();
            Tenants.Clear();
            foreach (var tenant in tenants)
            {
                Tenants.Add(tenant);
            }
            cmbTenant.ItemsSource = Tenants;
        }

        private async void LoadRequests()
        {
            var requests = await _mongoDbService.GetAllMaintenanceRequestsAsync();
            Requests.Clear();
            foreach (var request in requests)
            {
                var tenant = Tenants.FirstOrDefault(t => t.Id == request.TenantId);
                var viewModel = new MaintenanceViewModel
                {
                    Id = request.Id,
                    TenantId = request.TenantId,
                    TenantName = tenant != null ? $"{tenant.FirstName} {tenant.LastName}" : "Unknown",
                    Title = request.Title,
                    Description = request.Description,
                    Category = request.Category,
                    Priority = request.Priority,
                    Status = request.Status,
                    CreatedDate = request.CreatedDate,
                    ResolvedDate = request.ResolvedDate
                };
                Requests.Add(viewModel);
            }
        }

        private async void BtnAddRequest_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTenant.SelectedItem is not Tenant selectedTenant) return;
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) return;
            if (string.IsNullOrWhiteSpace(txtDescription.Text)) return;
            if (cmbCategory.SelectedItem == null) return;
            if (cmbPriority.SelectedItem == null) return;

            var request = new MaintenanceRequest
            {
                TenantId = selectedTenant.Id,
                Title = txtTitle.Text,
                Description = txtDescription.Text,
                Category = (MaintenanceCategory)Enum.Parse(typeof(MaintenanceCategory), cmbCategory.Text),
                Priority = (MaintenancePriority)Enum.Parse(typeof(MaintenancePriority), cmbPriority.Text)
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                string errors = string.Join("\n", validationResults.Select(v => v.ErrorMessage));
                MessageBox.Show($"Validation errors:\n{errors}");
                return;
            }

            bool success = await _mongoDbService.AddMaintenanceRequestAsync(request);
            if (success)
            {
                LoadRequests();
                ClearFields();
            }
            else
            {
                MessageBox.Show("Failed to add request.");
            }
        }

        private async void BtnUpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRequest != null && selectedRequest.Id != null)
            {
                // Simple status update to next status
                MaintenanceStatus newStatus = selectedRequest.Status switch
                {
                    MaintenanceStatus.Open => MaintenanceStatus.InProgress,
                    MaintenanceStatus.InProgress => MaintenanceStatus.Closed,
                    MaintenanceStatus.Closed => MaintenanceStatus.Open,
                    _ => MaintenanceStatus.Open
                };

                bool success = await _mongoDbService.UpdateMaintenanceStatusAsync(selectedRequest.Id, newStatus);
                if (success)
                {
                    LoadRequests();
                }
                else
                {
                    MessageBox.Show("Failed to update status.");
                }
            }
        }

        private async void BtnDeleteRequest_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRequest != null && selectedRequest.Id != null)
            {
                bool success = await _mongoDbService.DeleteMaintenanceRequestAsync(selectedRequest.Id);
                if (success)
                {
                    Requests.Remove(selectedRequest);
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to delete request.");
                }
            }
        }

        private void DgRequests_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedRequest = dgRequests.SelectedItem as MaintenanceViewModel;
        }

        private void ClearFields()
        {
            cmbTenant.SelectedItem = null;
            txtTitle.Text = "";
            txtDescription.Text = "";
            cmbCategory.SelectedItem = null;
            cmbPriority.SelectedItem = null;
            dgRequests.UnselectAll();
            selectedRequest = null;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class MaintenanceViewModel
    {
        public string? Id { get; set; }
        public string? TenantId { get; set; }
        public string? TenantName { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public MaintenanceCategory Category { get; set; }
        public MaintenancePriority Priority { get; set; }
        public MaintenanceStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
    }
}