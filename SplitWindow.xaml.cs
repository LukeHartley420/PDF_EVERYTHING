using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using PdfiumViewer;
using iText.Kernel.Pdf;

namespace PDF_EVERYTHING
{
    /// <summary>
    /// Interaction logic for SplitWindow.xaml
    /// </summary>
    public partial class SplitWindow : MetroWindow
    {
        private string pdfFile;
        private List<int> splitPoints = new List<int>();
        private List<string> splitFiles = new List<string>();
        public List<BitmapImage> PdfPages { get; set; } = new List<BitmapImage>();

        public SplitWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UploadPdfButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Multiselect = false
            };
            if (openFileDialog.ShowDialog() == true)
            {
                pdfFile = openFileDialog.FileName;
                LabelStatus.Content = "PDF uploaded successfully.";
                RenderPdfPages();
            }
        }

        private void RenderPdfPages()
        {
            PdfPages.Clear();
            using (var document = PdfiumViewer.PdfDocument.Load(pdfFile))
            {
                for (int i = 0; i < document.PageCount; i++)
                {
                    using (var image = document.Render(i, 300, 300, true))
                    {
                        using (MemoryStream imageStream = new MemoryStream())
                        {
                            image.Save(imageStream, ImageFormat.Png);
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = new MemoryStream(imageStream.ToArray());
                            bitmapImage.EndInit();
                            PdfPages.Add(bitmapImage);
                        }
                    }
                }
            }
            PdfPagesPanel.ItemsSource = null;
            PdfPagesPanel.ItemsSource = PdfPages;
        }

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var image = sender as System.Windows.Controls.Image;
            if (image != null)
            {
                int pageIndex = PdfPagesPanel.Items.IndexOf(image.Source) + 1;
                if (splitPoints.Contains(pageIndex))
                {
                    splitPoints.Remove(pageIndex);
                }
                else
                {
                    splitPoints.Add(pageIndex);
                }
                LabelStatus.Content = $"Split points added after pages: {string.Join(", ", splitPoints)}.";
            }
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(pdfFile) || splitPoints.Count == 0)
            {
                LabelStatus.Content = "Please upload a PDF and add split points first.";
                return;
            }

            splitFiles.Clear();
            string tempDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PDF_EVERYTHING");
            Directory.CreateDirectory(tempDirectory);

            // Load PDF document
            using (iText.Kernel.Pdf.PdfReader pdfReader = new iText.Kernel.Pdf.PdfReader(pdfFile))
            using (iText.Kernel.Pdf.PdfDocument pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader))
            {
                // Sort split points and add the end of the document as the last split point
                splitPoints.Sort();
                splitPoints.Add(pdfDocument.GetNumberOfPages() + 1);

                int startPage = 1;
                for (int i = 0; i < splitPoints.Count; i++)
                {
                    int endPage = splitPoints[i] - 1;
                    string splitFilePath = System.IO.Path.Combine(tempDirectory, $"Split_{startPage}_to_{endPage}.pdf");

                    // Create a new document for the split part
                    using (iText.Kernel.Pdf.PdfWriter pdfWriter = new iText.Kernel.Pdf.PdfWriter(splitFilePath))
                    using (iText.Kernel.Pdf.PdfDocument splitDocument = new iText.Kernel.Pdf.PdfDocument(pdfWriter))
                    {
                        for (int j = startPage; j <= endPage; j++)
                        {
                            pdfDocument.CopyPagesTo(j, j, splitDocument);
                        }

                        splitFiles.Add(splitFilePath);
                    }

                    startPage = splitPoints[i];
                }
            }

            LabelStatus.Content = "Split successfully.";

            // Prompt user to save each split part
            foreach (string splitFile in splitFiles)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = System.IO.Path.GetFileName(splitFile)
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        // Check if the file already exists and delete it if necessary
                        if (File.Exists(saveFileDialog.FileName))
                        {
                            File.Delete(saveFileDialog.FileName);
                        }

                        // Copy the file to the new location
                        File.Copy(splitFile, saveFileDialog.FileName, true);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            LabelStatus.Content = "Split part(s) saved successfully!";
        }

        private void BackToMainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            pdfFile = null;
            splitPoints.Clear();
            splitFiles.Clear();
            PdfPages.Clear();
            PdfPagesPanel.ItemsSource = null;
            LabelStatus.Content = "All PDFs removed.";
        }
    }
}
