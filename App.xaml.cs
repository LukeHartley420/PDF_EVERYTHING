using System;
using System.Windows;

namespace PDF_EVERYTHING
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length > 0)
            {
                if (e.Args[0] == "--set-default-pdf-reader")
                {
                    SetDefaultPdfReader.SetDefault();
                    MessageBox.Show("PDF_EVERYTHING has been set as the default PDF reader.", "Default PDF Reader", MessageBoxButton.OK, MessageBoxImage.Information);
                    Shutdown();
                    return;
                }

                if (e.Args[0].EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    PdfReaderWindow pdfReaderWindow = new PdfReaderWindow(e.Args[0]);
                    pdfReaderWindow.Show();
                }
                else
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                }
            }
            else
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}



