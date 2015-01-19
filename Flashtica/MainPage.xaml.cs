#region Namespace Declaration
using Flashtica.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Phone.Devices.Power;
using Windows.System;
using Windows.Media.Devices;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Media.Imaging;
using Windows.System.Display;
using Windows.Storage;
using Windows.Media.SpeechRecognition;
#endregion

namespace Flashtica
{
    public sealed partial class MainPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public MainPage()
        {
            this.InitializeComponent();
            
            
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            
        }
        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }
        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommandDefinition1.xml"));
                await Windows.Media.SpeechRecognition.VoiceCommandManager.InstallCommandSetsFromStorageFileAsync(storageFile);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        private static async Task<DeviceInformation> GetCameraID(Windows.Devices.Enumeration.Panel desiredCamera)
        {
            DeviceInformation deviceID = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture))
                .FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredCamera);
            if (deviceID != null) return deviceID;
            else throw new Exception(string.Format("Camera {0} doesn't exist", desiredCamera));
        }

        bool isOn = false;
        MediaCapture mediaDev;
        TorchControl tc;
        private DisplayRequest _displayRequest;
        
        private async Task<bool> ini()
        {
            var cameraID = await GetCameraID(Windows.Devices.Enumeration.Panel.Back);
            mediaDev = new MediaCapture();
            await mediaDev.InitializeAsync(new MediaCaptureInitializationSettings
            {
                VideoDeviceId = cameraID.Id
            });
            var videoDev = mediaDev.VideoDeviceController;
            tc = videoDev.TorchControl;

            return true;
        }
        
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // Flashlight is off - start initialize
            if (!isOn)
            {
                await ini();

            }

            // Is tc is supported on the current Device, enable Flashlight
            //if (tc.Supported)
            //{
            //    if (tc.PowerSupported)
            //        tc.PowerPercent = 100;

                if (tc.Enabled)
                {
                    tc.Enabled = false;
                    // Dispose MediaCapture
                    mediaDev.Dispose();
                    isOn = false;
                }
                else
                {
                    tc.Enabled = true;
                    isOn = true;
                }
            //}
        }


        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (_displayRequest == null)
                _displayRequest = new DisplayRequest();
            if ((sender as ToggleSwitch).IsOn)
            {
                _displayRequest.RequestActive();
            }
            else
            {
                _displayRequest.RequestRelease();
            }

        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(About));
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=596fba0f-9af0-4d03-98ba-67502a2b27d2"));
        }

        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            Launcher.LaunchUriAsync(new Uri("mailto:flashtica@outlook.com"));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(TorchPage));
        }

        //private void MyButtonProperties(object sender, RoutedEventArgs e)
        //{
        //    var buttonoff = new ImageBrush();
        //    buttonoff.ImageSource = new BitmapImage(new Uri("ms-appx:/Assets/Flashtica Button Off.png", UriKind.Relative));
        //    ImageBrush buttonon = new ImageBrush();
        //    buttonon.ImageSource = new BitmapImage(new Uri("ms-appx:/Assets/Flashtica Button.png", UriKind.Relative));
        //    if (tc.Enabled)
        //    {
        //        FlashButton.Background = buttonon;
        //    }
        //    else
        //    {
        //        FlashButton.Background = buttonoff;
        //    }
        //}
    }
}
