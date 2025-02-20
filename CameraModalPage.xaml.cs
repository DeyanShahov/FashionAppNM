using Microsoft.Maui.Controls;
using System.IO;
using System.Threading.Tasks;

#if __ANDROID__
using AndroidX.Emoji2.Text.FlatBuffer;
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
using Android.OS;
#endif

namespace FashionApp
{
    public partial class CameraModalPage : ContentPage
    {
        public CameraModalPage()
        {
            InitializeComponent();
        }

        private async void OnCaptureClicked(object sender, EventArgs e)
        {
            //try
            //{
            //    var image = await CameraView.CaptureAsync();
            //    if (image != null)
            //    {
            //        // Изпращане на изображението обратно на MaskEditor
            //        MessagingCenter.Send(this, "ImageCaptured", image);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    await DisplayAlert("Error", $"Failed to capture image: {ex.Message}", "OK");
            //}

            await MyCameraView.CaptureImage(CancellationToken.None);
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            // Затваряне на модалния прозорец
            Navigation.PopModalAsync();
        }


        private void MyCameraView_MediaCaptured(object sender, CommunityToolkit.Maui.Views.MediaCapturedEventArgs e)
        {
            // Копиране на потока, за да се избегне затварянето на оригиналния поток
            var memoryStream = new MemoryStream();
            e.Media.CopyTo(memoryStream);
            memoryStream.Position = 0; // Връщане на позицията на потока на начало

            if (Dispatcher.IsDispatchRequired)
            {
                Dispatcher.Dispatch(() => MyImage.Source = ImageSource.FromStream(() => memoryStream));
                Dispatcher.Dispatch(() => MessagingCenter.Send(this, "ImageCaptured", memoryStream));
                return;
            }

            MyImage.Source = ImageSource.FromStream(() => memoryStream);
            MessagingCenter.Send(this, "ImageCaptured", memoryStream);
        }
    }
} 