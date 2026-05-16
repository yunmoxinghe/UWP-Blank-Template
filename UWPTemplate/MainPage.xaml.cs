using System;
using System.Collections.ObjectModel;
using UWPTemplate.Pages;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewBackRequestedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
using NavigationViewPaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode;

namespace UWPTemplate
{
    public sealed partial class MainPage : Page
    {
        public static MainPage? Instance { get; private set; }
        public ObservableCollection<string> BreadcrumbItems { get; } = new ObservableCollection<string>();
        private readonly ResourceLoader _loader = new ResourceLoader();

        public MainPage()
        {
            this.InitializeComponent();
            Instance = this;

            TitleBarAppName.Text = Package.Current.DisplayName;
            ImgAppIcon.Source = new BitmapImage(Package.Current.Logo);

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(TitleBarArea);

            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(typeof(Pages.HomePage));
            ContentFrame.Navigated += ContentFrame_Navigated;

            this.Loaded += MainPage_Loaded;

            CoreWindow.GetForCurrentThread().Activated += MainPage_CoreWindowActivated;
        }

        private void MainPage_CoreWindowActivated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            bool isActive = args.WindowActivationState != CoreWindowActivationState.Deactivated;
            TitleBarAppName.Opacity = isActive ? 1.0 : 0.5;
        }

        public async void OpenExternalLink(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string btnUrl)
            {
                var dialog = new Dialogs.ExternalOpenDialog();
                var result = await dialog.ShowAsync();
                if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                    await Launcher.LaunchUriAsync(new Uri(btnUrl));
            }
            else if (sender is HyperlinkButton link && link.Tag is string linkUrl)
            {
                var dialog = new Dialogs.ExternalOpenDialog();
                var result = await dialog.ShowAsync();
                if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                    await Launcher.LaunchUriAsync(new Uri(linkUrl));
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            UpdateBackButton();
        }

        private void NavView_ItemInvoked(NavigationView sender,
            Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            string tag = args.IsSettingsInvoked ? "settings" : args.InvokedItemContainer.Tag?.ToString();
            Type targetPage = tag switch
            {
                "home" => typeof(Pages.HomePage),
                "settings" => typeof(Pages.SettingsPage),
                _ => null
            };

            if (targetPage != null && ContentFrame.CurrentSourcePageType != targetPage)
                ContentFrame.Navigate(targetPage);
        }

        private void NavView_BackRequested(NavigationView sender,
            NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack) ContentFrame.GoBack();
        }

        private void ContentFrame_Navigated(object sender,
            Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            UpdateBackButton();
            UpdateSelectedItem(e.SourcePageType);
            UpdateBreadcrumb(e.SourcePageType);
        }

        private void UpdateBackButton() =>
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

        private void UpdateSelectedItem(Type pageType)
        {
            if (pageType == typeof(Pages.SettingsPage))
                NavView.SelectedItem = NavView.SettingsItem;
            else if (pageType == typeof(Pages.HomePage))
                NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void UpdateBreadcrumb(Type pageType)
        {
            BreadcrumbItems.Clear();
            if (pageType == typeof(Pages.SettingsPage))
            {
                BreadcrumbItems.Add(_loader.GetString("Settings_Breadcrumb"));
                BreadcrumbPanel.Visibility = Visibility.Visible;
            }
            else
            {
                BreadcrumbPanel.Visibility = Visibility.Collapsed;
            }
        }

        public void ApplySettings()
        {
            try
            {
                var s = SettingsManager.Instance;

                NavView.PaneDisplayMode = s.PanePosition == "Top"
                    ? NavigationViewPaneDisplayMode.Top
                    : NavigationViewPaneDisplayMode.Left;

                ElementSoundPlayer.State = s.EnableSound
                    ? ElementSoundPlayerState.On
                    : ElementSoundPlayerState.Off;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplySettings Error: {ex.Message}");
            }
        }
    }
}