using System;
using System.Collections.ObjectModel;
using System.Windows;
using SocietyManagementSystem.Services;
using SocietyManagementSystem.Models;

namespace SocietyManagementSystem
{
    public partial class TenantWindow : Window
    {
        private readonly MongoDbService _mongoDbService;
        public ObservableCollection<Tenant> Tenants { get; set; } = new();
        public string[] Titles { get; } = new[] { "Mr", "Mrs", "Miss", "Ms", "Dr" };
        public string[] Genders { get; } = new[] { "Male", "Female", "Other" };
        private Tenant? selectedTenant;

        public TenantWindow(string societyName)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            _mongoDbService = new MongoDbService(societyName);
            dgTenants.ItemsSource = Tenants;
            cmbTitle.ItemsSource = Titles;
            cmbGender.ItemsSource = Genders;
            LoadTenants();
        }

        private async void LoadTenants()
        {
            var tenants = await _mongoDbService.GetAllTenantsAsync();
            Tenants.Clear();
            foreach (var tenant in tenants)
            {
                Tenants.Add(tenant);
            }
        }

        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var tenant = new Tenant
            {
                Title = cmbTitle.Text,
                FirstName = txtFirstName.Text,
                MiddleName = txtMiddleName.Text,
                LastName = txtLastName.Text,
                Gender = cmbGender.Text,
                Email = txtEmail.Text,
                Phone = txtPhone.Text,
                DateOfBirth = dpDOB.SelectedDate,
                Anniversary = dpAnniversary.SelectedDate
            };
            bool success = await _mongoDbService.AddTenantAsync(tenant);
            if (success)
            {
                Tenants.Add(tenant);
                ClearFields();
            }
            else
            {
                MessageBox.Show("Failed to add tenant.");
            }
        }

        private async void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTenant != null)
            {
                selectedTenant.Title = cmbTitle.Text;
                selectedTenant.FirstName = txtFirstName.Text;
                selectedTenant.MiddleName = txtMiddleName.Text;
                selectedTenant.LastName = txtLastName.Text;
                selectedTenant.Gender = cmbGender.Text;
                selectedTenant.Email = txtEmail.Text;
                selectedTenant.Phone = txtPhone.Text;
                selectedTenant.DateOfBirth = dpDOB.SelectedDate;
                selectedTenant.Anniversary = dpAnniversary.SelectedDate;
                bool success = await _mongoDbService.UpdateTenantAsync(selectedTenant);
                if (success)
                {
                    dgTenants.Items.Refresh();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to update tenant.");
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTenant != null && selectedTenant.Id != null)
            {
                bool success = await _mongoDbService.DeleteTenantAsync(selectedTenant.Id);
                if (success)
                {
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
                dpDOB.SelectedDate = selectedTenant.DateOfBirth;
                dpAnniversary.SelectedDate = selectedTenant.Anniversary;
            }
        }

        private void ClearFields()
        {
            cmbTitle.Text = "";
            txtFirstName.Text = "";
            txtMiddleName.Text = "";
            txtLastName.Text = "";
            cmbGender.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            dpDOB.SelectedDate = null;
            dpAnniversary.SelectedDate = null;
            dgTenants.UnselectAll();
            selectedTenant = null;
        }
    }

    public class Tenant
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? Anniversary { get; set; }
    }
}
