using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using SautinSoft;

namespace PDF_EVERYTHING
{
    /// <summary>
    /// Interaction logic for ConvertWindow.xaml
    /// </summary>
    public partial class ConvertWindow : MetroWindow
    {
        private List<string> pdfFiles = new List<string>();
        private List<string> convertedFiles = new List<string>();

        public ConvertWindow()
        {
            InitializeComponent();
        }

        private void SelectPdfButton_Click(object sender, RoutedEventArgs e)
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
                LabelStatus.Content = $"{pdfFiles.Count} PDF(s) selected.";
            }
        }

        private async void ConvertToWordButton_Click(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count == 0)
            {
                LabelStatus.Content = "Please select PDFs to convert.";
                return;
            }

            if (FormatComboBox.SelectedItem == null)
            {
                LabelStatus.Content = "Please select a format to convert to.";
                return;
            }

            string selectedFormat = ((ComboBoxItem)FormatComboBox.SelectedItem).Tag.ToString();
            convertedFiles.Clear();
            string tempDirectory = Path.Combine(Path.GetTempPath(), "PDF_EVERYTHING");
            Directory.CreateDirectory(tempDirectory);

            ProgressBarConvert.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                foreach (string pdfFile in pdfFiles)
                {
                    string outputFilePath = Path.Combine(tempDirectory, Path.GetFileNameWithoutExtension(pdfFile) + $".{selectedFormat}");

                    // Convert PDF to the selected format
                    PdfFocus pdfFocus = new PdfFocus();
                    pdfFocus.OpenPdf(pdfFile);

                    if (pdfFocus.PageCount > 0)
                    {
                        switch (selectedFormat)
                        {
                            case "docx":
                                pdfFocus.ToWord(outputFilePath);
                                break;
                            case "xlsx":
                                pdfFocus.ToExcel(outputFilePath);
                                break;
                            case "html":
                                pdfFocus.ToHtml(outputFilePath);
                                break;
                            case "txt":
                                pdfFocus.ToText(outputFilePath);
                                break;
                            case "png":
                                pdfFocus.ImageOptions.ImageFormat = PdfFocus.CImageOptions.ImageFormats.Png;
                                pdfFocus.ImageOptions.Dpi = 300;
                                pdfFocus.ToImage(outputFilePath);
                                break;
                        }
                        convertedFiles.Add(outputFilePath);
                    }
                }
            });

            ProgressBarConvert.Visibility = Visibility.Collapsed;
            LabelStatus.Content = "Conversion completed successfully.";
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            if (convertedFiles.Count == 0)
            {
                LabelStatus.Content = "Please convert PDFs first.";
                return;
            }

            foreach (string convertedFile in convertedFiles)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "All files (*.*)|*.*",
                    FileName = Path.GetFileName(convertedFile)
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        // Check if the file already exists and delete it if necessary
                        if (System.IO.File.Exists(saveFileDialog.FileName))
                        {
                            System.IO.File.Delete(saveFileDialog.FileName);
                        }

                        // Copy the file to the new location
                        System.IO.File.Copy(convertedFile, saveFileDialog.FileName, true);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            LabelStatus.Content = "Files saved successfully.";
        }

        private void BackToMainMenuButton_Click(object sender, RoutedEventArgs e)
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
        }
    }
}

