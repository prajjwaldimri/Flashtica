#region Namespace Declaration

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.System;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Flashtica.Common;
using Panel = Windows.Devices.Enumeration.Panel;

#endregion

namespace Flashtica
{
    public abstract sealed partial class MainPage : Page
    {

        private readonly NavigationHelper _navigationHelper;
        private readonly ObservableDictionary _defaultViewModel = new ObservableDictionary();

        public MainPage()
        {
            InitializeComponent();
            
            
            _navigationHelper = new NavigationHelper(this);
            _navigationHelper.LoadState += NavigationHelper_LoadState;
            _navigationHelper.SaveState += NavigationHelper_SaveState;

            
        }
        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return _defaultViewModel; }
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
            _navigationHelper.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommandDefinition1.xml"));
                await VoiceCommandManager.InstallCommandSetsFromStorageFileAsync(storageFile);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        private static async Task<DeviceInformation> GetCameraId(Panel desiredCamera)
        {
            var deviceId = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture))
                .FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredCamera);
            if (deviceId != null) return deviceId;
            throw new Exception(string.Format("Camera {0} doesn't exist", desiredCamera));
        }

        bool _isOn;
        MediaCapture _mediaDev;
        TorchControl _tc;
        private DisplayRequest _displayRequest;

        
        private async Task<bool> Ini()
        {
            var cameraId = await GetCameraId(Panel.Back);
            _mediaDev = new MediaCapture();
            await _mediaDev.InitializeAsync(new MediaCaptureInitializationSettings
            {
                VideoDeviceId = cameraId.Id
            });
            var videoDev = _mediaDev.VideoDeviceController;
            _tc = videoDev.TorchControl;

            return true;
        }
        
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            
            // Flashlight is off - start initialize
            if (!_isOn)
            {
                await Ini();

            }

             //Is tc is supported on the current Device, enable Flashlight
            if (_tc.Supported)
            {
                
                
                if (_tc.PowerSupported)
                    _tc.PowerPercent = 100;

                if (_tc.Enabled)
                {
                    _tc.Enabled = false;
                    // Dispose MediaCapture
                    _mediaDev.Dispose();
                    _isOn = false;
                    
                    //await mediaDev.StopRecordAsync();
                    
                }
                else
                {
                    // But wait, for this to work with Blue camera drivers, we have to Start a recording session
                    // Create video encoding profile as MP4 
                    var videoEncodingProperties = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Vga);

                    // Start Video Recording
                    var videoStorageFile = await ApplicationData.Current.TemporaryFolder
                        .CreateFileAsync("tempVideo.mp4", CreationCollisionOption.GenerateUniqueName);
                    await _mediaDev.StartRecordToStorageFileAsync(videoEncodingProperties, videoStorageFile);
                    _tc.Enabled = true;
                    _isOn = true; 
                    
                }
            }
        }


        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (_displayRequest == null)
                _displayRequest = new DisplayRequest();
            var toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null && toggleSwitch.IsOn)
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
            Frame.Navigate(typeof(About));
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
            Frame.Navigate(typeof(TorchPage));
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
