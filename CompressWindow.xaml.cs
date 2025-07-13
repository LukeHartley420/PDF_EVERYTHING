using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.Windows.Controls;

namespace PDF_EVERYTHING
{
    /// <summary>
    /// Interaction logic for CompressWindow.xaml
    /// </summary>
    public partial class CompressWindow : MetroWindow
    {
        private List<string> pdfFiles = new List<string>();
        private List<string> compressedFiles = new List<string>();

        public CompressWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string file in openFileDialog.FileNames)
                {
                    if (!pdfFiles.Contains(file))
                    {
                        pdfFiles.Add(file);
                        PdfListBox.Items.Add(file);
                    }
                }
                UpdateTotalSize();
            }
        }

        private void UpdateTotalSize()
        {
            long totalSize = pdfFiles.Sum(file => new FileInfo(file).Length);
            LabelTotalSize.Content = $"Total Size: {totalSize / 1024} KB";
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count == 0)
            {
                LabelStatus.Content = "Please upload PDFs to compress.";
                return;
            }

            string compressionLevel = ((ComboBoxItem)ComboBoxCompression.SelectedItem)?.Content.ToString();
            if (string.IsNullOrEmpty(compressionLevel))
            {
                LabelStatus.Content = "Please choose a compression strength.";
                return;
            }

            string pdfSettings;
            switch (compressionLevel)
            {
                case "Medium":
                    pdfSettings = "/printer";
                    break;
                case "High":
                    pdfSettings = "/screen";
                    break;
                default:
                    LabelStatus.Content = "Invalid compression level.";
                    return;
            }

            // Set the Ghostscript path
            string ghostscriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GhostscriptFiles", "gswin64c.exe");
            // Check if Ghostscript exists
            if (!File.Exists(ghostscriptPath))
            {
                MessageBox.Show("Error: Ghostscript executable not found!\nExpected at: " + ghostscriptPath, "Ghostscript Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show progress bar
            ProgressBarCompression.Visibility = Visibility.Visible;
            ProgressBarCompression.IsIndeterminate = true;

            try
            {
                compressedFiles.Clear();
                foreach (string pdfFile in pdfFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(pdfFile);
                    string tempCompressedFilePath = Path.Combine(Path.GetTempPath(), $"{fileName}_compressed.pdf");

                    // Construct the Ghostscript command arguments
                    string arguments = $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS={pdfSettings} -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{tempCompressedFilePath}\" \"{pdfFile}\"";

                    await Task.Run(() =>
                    {
                        ProcessStartInfo processStartInfo = new ProcessStartInfo
                        {
                            FileName = ghostscriptPath, // Use local Ghostscript
                            Arguments = arguments,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (Process process = Process.Start(processStartInfo))
                        {
                            string errorOutput = process.StandardError.ReadToEnd();
                            process.WaitForExit();

                            if (process.ExitCode != 0)
                            {
                                throw new Exception("Ghostscript Error: " + errorOutput);
                            }
                        }
                    });

                    // Add the compressed file path to the list
                    compressedFiles.Add(tempCompressedFilePath);
                }

                long totalCompressedSize = compressedFiles.Sum(file => new FileInfo(file).Length);
                LabelStatus.Content = $"Completed Successfully! Total Compressed Size: {totalCompressedSize / 1024} KB";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during compression:\n" + ex.Message, "Compression Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                LabelStatus.Content = "Compression failed.";
            }
            finally
            {
                ProgressBarCompression.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (compressedFiles.Count == 0 || compressedFiles.All(file => !File.Exists(file)))
            {
                LabelStatus.Content = "Please compress your PDFs first.";
                return;
            }

            try
            {
                foreach (string compressedFile in compressedFiles)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "PDF files (*.pdf)|*.pdf",
                        FileName = Path.GetFileName(compressedFile)
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.Copy(compressedFile, saveFileDialog.FileName, true);
                    }
                }

                LabelStatus.Content = "Files saved successfully.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during saving:\n" + ex.Message, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                LabelStatus.Content = "Save failed.";
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            pdfFiles.Clear();
            PdfListBox.Items.Clear();
            LabelStatus.Content = "All PDFs removed.";
            LabelTotalSize.Content = "Total Size: 0 KB";
        }
    }
}

