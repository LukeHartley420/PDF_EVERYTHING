using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using PdfiumViewer;

namespace PDF_EVERYTHING
{
    /// <summary>
    /// Interaction logic for PdfReaderWindow.xaml
    /// </summary>
    public partial class PdfReaderWindow : MetroWindow
    {
        private string _pdfFilePath;
        private int _zoomLevel = 100;

        public PdfReaderWindow()
        {
            InitializeComponent();
        }

        public PdfReaderWindow(string pdfFilePath) : this()
        {
            _pdfFilePath = pdfFilePath;
            LoadPdf();
        }

        private void LoadPdf()
        {
            if (string.IsNullOrEmpty(_pdfFilePath))
            {
                MessageBox.Show("PDF file path is not set.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var document = PdfiumViewer.PdfDocument.Load(_pdfFilePath))
                {
                    var pages = new List<BitmapImage>();
                    for (int i = 0; i < document.PageCount; i++)
                    {
                        using (var image = document.Render(i, _zoomLevel, _zoomLevel, true))
                        {
                            using (MemoryStream imageStream = new MemoryStream())
                            {
                                image.Save(imageStream, ImageFormat.Png);
                                BitmapImage bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.StreamSource = new MemoryStream(imageStream.ToArray());
                                bitmapImage.EndInit();
                                pages.Add(bitmapImage);
                            }
                        }
                    }
                    PdfPagesControl.ItemsSource = pages;
                    Console.WriteLine($"Loaded {pages.Count} pages.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (string.IsNullOrEmpty(_pdfFilePath))
            {
                return;
            }

            _zoomLevel = (int)e.NewValue;
            LoadPdf();
        }

        private void ToolboxButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
