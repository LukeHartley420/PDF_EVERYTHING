using MahApps.Metro.Controls;

namespace PDF_EVERYTHING;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MetroWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        MergeWindow mergeWindow = new MergeWindow();
        mergeWindow.Show();
        this.Close();
    }

    private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
    {
        CompressWindow compressWindow = new CompressWindow();
        compressWindow.Show();
        this.Close();
    }

    private void Button_Click_2(object sender, System.Windows.RoutedEventArgs e)
    {
        ConvertWindow convertWindow = new ConvertWindow();
        convertWindow.Show();
        this.Close();
    }

    private void Button_Click_3(object sender, System.Windows.RoutedEventArgs e)
    {
        SplitWindow splitWindow = new SplitWindow();
        splitWindow.Show();
        this.Close();
    }
}