using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWPTemplate.Pages
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance?.OpenExternalLink(sender, e);
        }
    }
}