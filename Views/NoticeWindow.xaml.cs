using System;
using System.Collections.ObjectModel;
using System.Windows;
using SocietyManagementSystem.Services;
using SocietyManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SocietyManagementSystem
{
    public partial class NoticeWindow : Window
    {
        private readonly MongoDbService _mongoDbService;
        public ObservableCollection<Notice> Notices { get; set; } = new();
        public string[] Audiences { get; } = Enum.GetNames(typeof(NoticeAudience));
        private Notice? selectedNotice;

        public NoticeWindow(string societyName)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            _mongoDbService = new MongoDbService(societyName);
            dgNotices.ItemsSource = Notices;
            cmbAudience.ItemsSource = Audiences;
            LoadNotices();
        }

        private async void LoadNotices()
        {
            var notices = await _mongoDbService.GetAllNoticesAsync();
            Notices.Clear();
            foreach (var notice in notices)
            {
                Notices.Add(notice);
            }
        }

        private async void BtnPostNotice_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) return;
            if (string.IsNullOrWhiteSpace(txtContent.Text)) return;
            if (cmbAudience.SelectedItem == null) return;

            var notice = new Notice
            {
                Title = txtTitle.Text,
                Content = txtContent.Text,
                PostedBy = "Admin", // TODO: Use current user
                Audience = (NoticeAudience)Enum.Parse(typeof(NoticeAudience), cmbAudience.Text)
            };

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(notice);
            if (!Validator.TryValidateObject(notice, validationContext, validationResults, true))
            {
                string errors = string.Join("\n", validationResults.Select(v => v.ErrorMessage));
                MessageBox.Show($"Validation errors:\n{errors}");
                return;
            }

            bool success = await _mongoDbService.AddNoticeAsync(notice);
            if (success)
            {
                LoadNotices();
                ClearFields();
            }
            else
            {
                MessageBox.Show("Failed to post notice.");
            }
        }

        private async void BtnToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (selectedNotice != null && selectedNotice.Id != null)
            {
                bool success = await _mongoDbService.ToggleNoticeActiveAsync(selectedNotice.Id);
                if (success)
                {
                    LoadNotices();
                }
                else
                {
                    MessageBox.Show("Failed to toggle active status.");
                }
            }
        }

        private async void BtnDeleteNotice_Click(object sender, RoutedEventArgs e)
        {
            if (selectedNotice != null && selectedNotice.Id != null)
            {
                bool success = await _mongoDbService.DeleteNoticeAsync(selectedNotice.Id);
                if (success)
                {
                    Notices.Remove(selectedNotice);
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to delete notice.");
                }
            }
        }

        private void DgNotices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedNotice = dgNotices.SelectedItem as Notice;
            if (selectedNotice != null)
            {
                txtTitle.Text = selectedNotice.Title;
                txtContent.Text = selectedNotice.Content;
                cmbAudience.Text = selectedNotice.Audience.ToString();
            }
        }

        private void ClearFields()
        {
            txtTitle.Text = "";
            txtContent.Text = "";
            cmbAudience.SelectedItem = null;
            dgNotices.UnselectAll();
            selectedNotice = null;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}