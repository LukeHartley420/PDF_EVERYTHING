using System;
using System.Collections.Generic;
using System.Windows;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Threading.Tasks;

namespace PDF_EVERYTHING
{
    /// <summary>
    /// Interaction logic for MergeWindow.xaml
    /// </summary>
    public partial class MergeWindow : MetroWindow
    {
        private List<string> pdfFiles = new List<string>();
        private PdfDocument mergedPdf;

        public MergeWindow()
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
                LabelStatus.Content = $"{pdfFiles.Count} PDF(s) added.";
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count < 2)
            {
                LabelStatus.Content = "2 or more PDFs need to be uploaded.";
                return;
            }

            ProgressBarMerge.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                mergedPdf = new PdfDocument();
                foreach (string file in pdfFiles)
                {
                    PdfDocument inputPdf = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                    foreach (PdfPage page in inputPdf.Pages)
                    {
                        mergedPdf.AddPage(page);
                    }
                }
            });

            ProgressBarMerge.Visibility = Visibility.Collapsed;
            LabelStatus.Content = "Merged Successfully.";
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (mergedPdf == null)
            {
                LabelStatus.Content = "Please merge your PDFs first.";
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                mergedPdf.Save(saveFileDialog.FileName);
                LabelStatus.Content = "File saved successfully.";
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
        }
    }
}

