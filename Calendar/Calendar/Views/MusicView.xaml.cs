using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Calendar.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicView : Page
    {
        public MusicView()
        {
            this.InitializeComponent();
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            InnerEllipse.Visibility = Visibility.Visible;
            EllStoryboard.Begin();
            Mymedia.Play();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            EllStoryboard.Pause();
            Mymedia.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            EllStoryboard.Stop();
            Mymedia.Stop();
        }

        async private void ChooseMusicButton_Click(object sender, RoutedEventArgs e)
        {
            Mymedia.Stop();
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".wma");
            openPicker.FileTypeFilter.Add(".mp3");

            var file = await openPicker.PickSingleFileAsync();

            // mediaPlayer is a MediaElement defined in XAML
            if (file != null)
            {
                string SelectFileType = file.FileType.ToString();
                if (SelectFileType == ".mp4" || SelectFileType == ".wmv")
                {
                    RotatePanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    RotatePanel.Visibility = Visibility.Visible;

                }
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                Mymedia.SetSource(stream, file.ContentType);
                Mymedia.Play();
            }
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            // mediaPlayer.IsFullWindow = !media.IsFullWindow;
            ApplicationView view = ApplicationView.GetForCurrentView();

            bool isInFullScreenMode = view.IsFullScreenMode;

            if (isInFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }

        private void Mymedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            MySlider.Maximum = Mymedia.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
