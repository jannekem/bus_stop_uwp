using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace bus_stop
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int MAX_BLINKS = 20;

        Uri departuresPage;
        Uri nearYouPage;
        Uri weatherPage;
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer blink_timer = new DispatcherTimer();
        DispatcherTimer weather_update_timer = new DispatcherTimer();

        bool stopBus = false;
        int blink_counter = 0;

        private AudioPlayer _player = new AudioPlayer();
        
       

        public MainPage()
        {
            this.InitializeComponent();
            departuresPage = new Uri("https://beta.reittiopas.fi/pysakit/HSL:1130111");
            nearYouPage = new Uri("https://beta.reittiopas.fi/lahellasi");
            weatherPage = new Uri("http://busstopofthefuture.azurewebsites.net/get_weather.html");
            map_view_progress.IsActive = true;
            map_view.Navigate(departuresPage);

            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += timer_Tick;
            timer.Start();

            blink_timer.Interval = TimeSpan.FromSeconds(0.2);
            blink_timer.Tick += blinker_Tick;
            blink_timer.Start();

            
            weather_webview.Navigate(this.weatherPage);
            weather_update_timer.Interval = TimeSpan.FromMinutes(5);
            weather_update_timer.Tick += weather_update_Tick;
            weather_update_timer.Start();

            Window.Current.SizeChanged += Current_SizeChanged;
            update_scroll_viewer();

           
        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var audio_siren = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/siren.mp3"));
            await _player.LoadFileAsync(audio_siren);

            var advert = new BitmapImage((new Uri("ms-appx:///Assets/blackfriday.png")));
            ad_image.Source = advert;
            ad_image.Stretch = Stretch.UniformToFill;

        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            update_scroll_viewer();
        }

        private void update_scroll_viewer()
        {
            scroll_viewer.Height = Window.Current.Bounds.Height;
            scroll_viewer.Width = Window.Current.Bounds.Width;
        }

        void timer_Tick(object sender, object e)
        {
            clock_text.Text = DateTime.Now.TimeOfDay.ToString().Substring(0,8);    
        }

        void blinker_Tick(object sender, object e)
        {
            if(stopBus && blink_counter < MAX_BLINKS)
            {
                blinker_Grid.Visibility = Visibility.Visible;
                if (blink_counter % 2 == 0)
                {
                    blinker_rect.Fill = new SolidColorBrush(Windows.UI.Colors.Blue);
                }
                else
                {
                    blinker_rect.Fill = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                blink_counter++;
            }
            else
            {
                blinker_Grid.Visibility = Visibility.Collapsed;
                blink_counter = 0;
                stopBus = false;
            }
        }

        void weather_update_Tick(object sender, object e)
        {
            weather_webview.Navigate(this.weatherPage);
        }

        private void WebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            map_view_progress.IsActive = false;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            map_view.Navigate(departuresPage);
        }

        private async void button_Stop_Click(object sender, RoutedEventArgs e)
        {
            stopBus = true;
            _player.Play("siren.mp3", 1);
        }

        private void button_near_you_Click(object sender, RoutedEventArgs e)
        {
            map_view.Navigate(nearYouPage);
        }
    }
}
