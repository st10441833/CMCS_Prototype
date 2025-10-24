using System.Text;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Globalization;

namespace CMCS_Prototype
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Claim> pendingClaims;
        private ObservableCollection<ManagerClaim> managerClaims;
        private ObservableCollection<Document> uploadedDocuments;
        private ObservableCollection<TrackedClaim> trackedClaims;
        private int claimCounter = 1;

        // South African culture for Rand currency formatting
        private readonly CultureInfo southAfricanCulture = new CultureInfo("en-ZA");

        public MainWindow()
        {
            InitializeComponent();
            InitializeMockData();
            InitializeStatusTracking();
            UpdateStatistics();
        }

        private void InitializeMockData()
        {
            // Initialize collections
            uploadedDocuments = new ObservableCollection<Document>();
            lstDocuments.ItemsSource = uploadedDocuments;

            trackedClaims = new ObservableCollection<TrackedClaim>();
            cmbTrackClaims.ItemsSource = trackedClaims;

            // Create sample pending claims
            pendingClaims = new ObservableCollection<Claim>
            {
                new Claim {
                    ClaimId = "C001",
                    LecturerId = "L001",
                    LecturerName = "Dr. Smith",
                    Month = "October",
                    TotalHours = 45,
                    Amount = 6750,
                    SubmissionDate = DateTime.Now.AddDays(-2),
                    Status = "Pending Coordinator Review",
                    DocumentCount = 2,
                    Notes = "Regular monthly teaching hours"
                },
                new Claim {
                    ClaimId = "C002",
                    LecturerId = "L002",
                    LecturerName = "Prof. Johnson",
                    Month = "October",
                    TotalHours = 38,
                    Amount = 5700,
                    SubmissionDate = DateTime.Now.AddDays(-1),
                    Status = "Pending Coordinator Review",
                    DocumentCount = 1,
                    Notes = "Includes exam marking hours"
                }
            };
            dgPendingClaims.ItemsSource = pendingClaims;

            // Create sample manager approval queue
            managerClaims = new ObservableCollection<ManagerClaim>
            {
                new ManagerClaim {
                    ClaimId = "C003",
                    LecturerName = "Dr. Brown",
                    Month = "October",
                    Amount = 7200,
                    CoordinatorStatus = "Approved",
                    CoordinatorName = "Dr. Wilson",
                    SubmissionDate = DateTime.Now.AddDays(-3),
                    FinalStatus = "Pending Final Approval"
                },
                new ManagerClaim {
                    ClaimId = "C004",
                    LecturerName = "Prof. Davis",
                    Month = "September",
                    Amount = 5800,
                    CoordinatorStatus = "Approved",
                    CoordinatorName = "Dr. Wilson",
                    SubmissionDate = DateTime.Now.AddDays(-5),
                    FinalStatus = "Pending Final Approval"
                }
            };
            dgManagerApproval.ItemsSource = managerClaims;
        }

        private void InitializeStatusTracking()
        {
            // Add sample claims for tracking
            trackedClaims.Add(new TrackedClaim
            {
                ClaimId = "C001",
                Month = "October",
                Year = "2024",
                Status = "Pending Coordinator Review",
                Progress = 33
            });

            trackedClaims.Add(new TrackedClaim
            {
                ClaimId = "C002",
                Month = "September",
                Year = "2024",
                Status = "Approved by Coordinator",
                Progress = 66
            });
        }

        private void UpdateStatistics()
        {
            // Update coordinator summary
            txtCoordinatorSummary.Text = $"Total Pending Claims: {pendingClaims.Count}";

            // Update manager statistics
            int approvedCount = managerClaims.Count(mc => mc.FinalStatus == "Final Approved");
            int pendingCount = managerClaims.Count(mc => mc.FinalStatus == "Pending Final Approval");

            txtTotalInQueue.Text = managerClaims.Count.ToString();
            txtApprovedCount.Text = approvedCount.ToString();
            txtPendingCount.Text = pendingCount.ToString();
        }

        // ENHANCED: Lecturer Functions
        private void BtnUploadDocument_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Supported files (.pdf;.docx;.xlsx)|*.pdf;*.docx;*.xlsx|PDF files (*.pdf)|*.pdf|Word documents (*.docx)|*.docx|Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    FilterIndex = 1,
                    Multiselect = true
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        FileInfo fileInfo = new FileInfo(filename);

                        // Check file size (10MB limit)
                        if (fileInfo.Length > 10 * 1024 * 1024)
                        {
                            MessageBox.Show($"File {fileInfo.Name} exceeds 10MB limit. Please choose a smaller file.",
                                          "File Too Large", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        uploadedDocuments.Add(new Document
                        {
                            FileName = System.IO.Path.GetFileName(filename),
                            FileSize = FormatFileSize(fileInfo.Length),
                            FilePath = filename
                        });
                    }
                    UpdateStatus($"Uploaded {openFileDialog.FileNames.Length} document(s) successfully");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Document upload failed", ex);
            }
        }

        private void BtnRemoveDocument_Click(object sender, RoutedEventArgs e)
        {
            if (lstDocuments.SelectedItem != null)
            {
                uploadedDocuments.Remove((Document)lstDocuments.SelectedItem);
                UpdateStatus("Document removed");
            }
            else
            {
                MessageBox.Show("Please select a document to remove.", "No Selection",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnSubmitClaim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(cmbMonth.Text) || string.IsNullOrEmpty(txtYear.Text))
                {
                    MessageBox.Show("Please select month and year.", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(txtHours.Text, out double hours) || hours <= 0)
                {
                    MessageBox.Show("Please enter valid hours worked (greater than 0).", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtRate.Text, out decimal rate) || rate <= 0)
                {
                    MessageBox.Show("Please enter a valid hourly rate.", "Validation Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Calculate amount
                decimal amount = (decimal)hours * rate;

                // Create new claim
                string newClaimId = $"C{claimCounter++:D3}";
                var newClaim = new Claim
                {
                    ClaimId = newClaimId,
                    LecturerId = "L001", // Mock lecturer ID
                    LecturerName = "Current User", // Mock lecturer name
                    Month = cmbMonth.Text,
                    TotalHours = hours,
                    Amount = amount,
                    SubmissionDate = DateTime.Now,
                    Status = "Pending Coordinator Review",
                    DocumentCount = uploadedDocuments.Count,
                    Notes = txtNotes.Text
                };

                // Add to pending claims
                pendingClaims.Add(newClaim);

                // Add to tracking
                trackedClaims.Add(new TrackedClaim
                {
                    ClaimId = newClaimId,
                    Month = cmbMonth.Text,
                    Year = txtYear.Text,
                    Status = "Pending Coordinator Review",
                    Progress = 25
                });

                // Show success message
                MessageBox.Show($"Claim {newClaimId} submitted successfully!\n\n" +
                              $"Month: {cmbMonth.Text} {txtYear.Text}\n" +
                              $"Total Hours: {hours}\n" +
                              $"Amount: {FormatCurrency(amount)}\n" +
                              $"Documents: {uploadedDocuments.Count}\n\n" +
                              $"Status: Pending Coordinator Review",
                              "Claim Submitted", MessageBoxButton.OK, MessageBoxImage.Information);

                UpdateStatus($"Claim {newClaimId} submitted successfully - awaiting coordinator approval");

                // Clear form
                ClearForm();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Claim submission failed", ex);
            }
        }

        private void BtnClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            UpdateStatus("Form cleared");
        }

        private void ClearForm()
        {
            cmbMonth.SelectedIndex = -1;
            cmbMonth.Text = "";
            txtYear.Text = DateTime.Now.Year.ToString();
            txtHours.Text = "0";
            txtRate.Text = "150";
            txtNotes.Text = "";
            uploadedDocuments.Clear();
        }

        private void TxtHours_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Real-time calculation can be added here if needed
        }

        private void BtnTrackStatus_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTrackClaims.SelectedItem == null)
            {
                MessageBox.Show("Please select a claim to track from the dropdown list.",
                              "No Claim Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedClaim = (TrackedClaim)cmbTrackClaims.SelectedItem;
            MessageBox.Show($"Claim Status: {selectedClaim.ClaimId}\n\n" +
                          $"Month: {selectedClaim.Month} {selectedClaim.Year}\n" +
                          $"Current Status: {selectedClaim.Status}\n" +
                          $"Progress: {selectedClaim.Progress}%\n\n" +
                          $"Next Stage: {GetNextStage(selectedClaim.Status)}",
                          "Claim Status Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CmbTrackClaims_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTrackClaims.SelectedItem is TrackedClaim selectedClaim)
            {
                txtTrackStatus.Text = $"Status: {selectedClaim.Status}";
                pbStatus.Value = selectedClaim.Progress;
                txtStatusDetails.Text = $"Claim {selectedClaim.ClaimId} for {selectedClaim.Month} {selectedClaim.Year}\n" +
                                      $"Progress: {selectedClaim.Progress}% complete\n" +
                                      $"Next: {GetNextStage(selectedClaim.Status)}";
            }
        }

        private string GetNextStage(string currentStatus)
        {
            switch (currentStatus)
            {
                case "Submitted":
                    return "Coordinator Review";
                case "Pending Coordinator Review":
                    return "Coordinator Approval";
                case "Approved by Coordinator":
                    return "Manager Final Approval";
                case "Pending Final Approval":
                    return "Payment Processing";
                case "Final Approved":
                    return "Payment Processing";
                default:
                    return "Process Complete";
            }
        }

        // ENHANCED: Coordinator Functions
        private void BtnViewClaimDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var claim = button?.DataContext as Claim;

            if (claim != null)
            {
                MessageBox.Show($"Claim Details: {claim.ClaimId}\n\n" +
                              $"Lecturer: {claim.LecturerName} ({claim.LecturerId})\n" +
                              $"Period: {claim.Month}\n" +
                              $"Hours: {claim.TotalHours}\n" +
                              $"Amount: {FormatCurrency(claim.Amount)}\n" +
                              $"Status: {claim.Status}\n" +
                              $"Documents: {claim.DocumentCount}\n" +
                              $"Notes: {claim.Notes}\n" +
                              $"Submitted: {claim.SubmissionDate:yyyy-MM-dd HH:mm}",
                              "Claim Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnApproveClaim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var claim = button?.DataContext as Claim;

                if (claim != null)
                {
                    // Update claim status
                    claim.Status = "Approved by Coordinator";

                    // Move to manager queue
                    managerClaims.Add(new ManagerClaim
                    {
                        ClaimId = claim.ClaimId,
                        LecturerName = claim.LecturerName,
                        Month = claim.Month,
                        Amount = claim.Amount,
                        CoordinatorStatus = "Approved",
                        CoordinatorName = "Coordinator User", // Mock coordinator name
                        SubmissionDate = claim.SubmissionDate,
                        FinalStatus = "Pending Final Approval"
                    });

                    // Update tracking
                    var trackedClaim = trackedClaims.FirstOrDefault(tc => tc.ClaimId == claim.ClaimId);
                    if (trackedClaim != null)
                    {
                        trackedClaim.Status = "Approved by Coordinator";
                        trackedClaim.Progress = 66;
                    }

                    pendingClaims.Remove(claim);
                    UpdateStatus($"Approved claim {claim.ClaimId} for {claim.LecturerName}");
                    MessageBox.Show($"Claim {claim.ClaimId} approved for {claim.LecturerName}. Moved to Academic Manager queue.",
                                  "Approval Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                    UpdateStatistics();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Claim approval failed", ex);
            }
        }

        private void BtnRejectClaim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var claim = button?.DataContext as Claim;

                if (claim != null)
                {
                    string reason = ShowInputDialog("Please enter reason for rejection:", "Rejection Reason", "Insufficient documentation");

                    if (!string.IsNullOrEmpty(reason))
                    {
                        // Update tracking
                        var trackedClaim = trackedClaims.FirstOrDefault(tc => tc.ClaimId == claim.ClaimId);
                        if (trackedClaim != null)
                        {
                            trackedClaim.Status = "Rejected by Coordinator";
                            trackedClaim.Progress = 100;
                        }

                        pendingClaims.Remove(claim);
                        UpdateStatus($"Rejected claim {claim.ClaimId} for {claim.LecturerName}");
                        MessageBox.Show($"Claim {claim.ClaimId} rejected for {claim.LecturerName}.\nReason: {reason}\n\nLecturer will be notified.",
                                      "Claim Rejected", MessageBoxButton.OK, MessageBoxImage.Warning);

                        UpdateStatistics();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Claim rejection failed", ex);
            }
        }

        private void BtnRefreshClaims_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Refreshed pending claims list");
            // In a real application, this would fetch from database
            MessageBox.Show("Claims list refreshed successfully.",
                          "Refresh Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExportCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"claims_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Simple CSV export implementation
                    using (var writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        writer.WriteLine("ClaimID,LecturerName,Month,Hours,Amount,Status,SubmissionDate");
                        foreach (var claim in pendingClaims)
                        {
                            writer.WriteLine($"\"{claim.ClaimId}\",\"{claim.LecturerName}\",\"{claim.Month}\",{claim.TotalHours},{claim.Amount},\"{claim.Status}\",\"{claim.SubmissionDate:yyyy-MM-dd}\"");
                        }
                    }
                    UpdateStatus($"Exported {pendingClaims.Count} claims to CSV");
                    MessageBox.Show($"Successfully exported {pendingClaims.Count} claims to CSV.",
                                  "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("CSV export failed", ex);
            }
        }

        // ENHANCED: Academic Manager Functions
        private void BtnViewManagerDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var claim = button?.DataContext as ManagerClaim;

            if (claim != null)
            {
                MessageBox.Show($"Manager Claim Details: {claim.ClaimId}\n\n" +
                              $"Lecturer: {claim.LecturerName}\n" +
                              $"Period: {claim.Month}\n" +
                              $"Amount: {FormatCurrency(claim.Amount)}\n" +
                              $"Coordinator: {claim.CoordinatorName}\n" +
                              $"Coordinator Status: {claim.CoordinatorStatus}\n" +
                              $"Final Status: {claim.FinalStatus}\n" +
                              $"Submitted: {claim.SubmissionDate:yyyy-MM-dd}",
                              "Manager Claim Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnFinalApprove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var claim = button?.DataContext as ManagerClaim;

                if (claim != null)
                {
                    claim.FinalStatus = "Final Approved";

                    // Update tracking
                    var trackedClaim = trackedClaims.FirstOrDefault(tc => tc.ClaimId == claim.ClaimId);
                    if (trackedClaim != null)
                    {
                        trackedClaim.Status = "Final Approved - Payment Processing";
                        trackedClaim.Progress = 100;
                    }

                    managerClaims.Remove(claim);
                    UpdateStatus($"Final approval granted for claim {claim.ClaimId}");
                    MessageBox.Show($"Claim {claim.ClaimId} fully approved and sent for payment processing.\n\n" +
                                  $"Lecturer: {claim.LecturerName}\n" +
                                  $"Amount: {FormatCurrency(claim.Amount)}",
                                  "Final Approval", MessageBoxButton.OK, MessageBoxImage.Information);

                    UpdateStatistics();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Final approval failed", ex);
            }
        }

        private void BtnReturnClaim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var claim = button?.DataContext as ManagerClaim;

                if (claim != null)
                {
                    string reason = ShowInputDialog("Please enter reason for returning claim:", "Return Reason", "Additional information required");

                    if (!string.IsNullOrEmpty(reason))
                    {
                        // Update tracking
                        var trackedClaim = trackedClaims.FirstOrDefault(tc => tc.ClaimId == claim.ClaimId);
                        if (trackedClaim != null)
                        {
                            trackedClaim.Status = "Returned for Clarification";
                            trackedClaim.Progress = 50;
                        }

                        managerClaims.Remove(claim);
                        UpdateStatus($"Claim {claim.ClaimId} returned for clarification");
                        MessageBox.Show($"Claim {claim.ClaimId} returned to coordinator for additional information.\nReason: {reason}",
                                      "Claim Returned", MessageBoxButton.OK, MessageBoxImage.Warning);

                        UpdateStatistics();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Claim return failed", ex);
            }
        }

        private void BtnRefreshManagerQueue_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Refreshed manager approval queue");
            MessageBox.Show("Manager queue refreshed successfully.",
                          "Refresh Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int approvedCount = managerClaims.Count(mc => mc.FinalStatus == "Final Approved");
                int pendingCount = managerClaims.Count(mc => mc.FinalStatus == "Pending Final Approval");
                decimal totalAmount = managerClaims.Where(mc => mc.FinalStatus == "Final Approved")
                                                 .Sum(mc => mc.Amount);

                MessageBox.Show($"Academic Manager Report\n\n" +
                              $"Total Claims in Queue: {managerClaims.Count}\n" +
                              $"Final Approved: {approvedCount}\n" +
                              $"Pending Final Approval: {pendingCount}\n" +
                              $"Total Amount Approved: {FormatCurrency(totalAmount)}\n\n" +
                              $"Report Generated: {DateTime.Now:yyyy-MM-dd HH:mm}",
                              "Manager Report", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateStatus("Generated manager report");
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Report generation failed", ex);
            }
        }

        // Utility Methods
        private void UpdateStatus(string message)
        {
            txtStatus.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void ShowErrorMessage(string action, Exception ex)
        {
            MessageBox.Show($"{action}. Error: {ex.Message}",
                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            UpdateStatus($"Error: {action}");
        }

        // Format currency in South African Rands
        private string FormatCurrency(decimal amount)
        {
            return amount.ToString("C", southAfricanCulture);
        }

        // Custom Input Dialog to replace Microsoft.VisualBasic.Interaction.InputBox
        private string ShowInputDialog(string prompt, string title, string defaultValue = "")
        {
            Window inputDialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            Grid grid = new Grid();
            grid.Margin = new Thickness(10);
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock promptText = new TextBlock
            {
                Text = prompt,
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(promptText, 0);

            TextBox inputTextBox = new TextBox
            {
                Text = defaultValue,
                Margin = new Thickness(0, 0, 0, 15),
                Height = 25
            };
            Grid.SetRow(inputTextBox, 1);

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(buttonPanel, 2);

            Button okButton = new Button
            {
                Content = "OK",
                Width = 75,
                Height = 25,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += (s, e) => { inputDialog.DialogResult = true; inputDialog.Close(); };

            Button cancelButton = new Button
            {
                Content = "Cancel",
                Width = 75,
                Height = 25,
                IsCancel = true
            };
            cancelButton.Click += (s, e) => { inputDialog.DialogResult = false; inputDialog.Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(promptText);
            grid.Children.Add(inputTextBox);
            grid.Children.Add(buttonPanel);

            inputDialog.Content = grid;
            inputTextBox.Focus();
            inputTextBox.SelectAll();

            return inputDialog.ShowDialog() == true ? inputTextBox.Text : null;
        }
    }

    // ENHANCED Data Models
    public class Claim
    {
        public string ClaimId { get; set; }
        public string LecturerId { get; set; }
        public string LecturerName { get; set; }
        public string Month { get; set; }
        public double TotalHours { get; set; }
        public decimal Amount { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; }
        public int DocumentCount { get; set; }
        public string Notes { get; set; }
    }

    public class ManagerClaim
    {
        public string ClaimId { get; set; }
        public string LecturerName { get; set; }
        public string Month { get; set; }
        public decimal Amount { get; set; }
        public string CoordinatorStatus { get; set; }
        public string CoordinatorName { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string FinalStatus { get; set; }
    }

    public class Document
    {
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
    }

    // NEW: Tracking model
    public class TrackedClaim
    {
        public string ClaimId { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string Status { get; set; }
        public int Progress { get; set; }
        public string TrackDisplay => $"{ClaimId} - {Month} {Year} ({Status})";
    }
}