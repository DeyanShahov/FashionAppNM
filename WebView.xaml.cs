using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
namespace FashionApp;

public partial class WebViewPage : ContentPage
{
	private async Task SaveImageFromUrl(byte[] imageData)
	{
		try
		{
			var fileName = $"image_{DateTime.Now:yyyyMMdd_HHmmss}.png";

			using var stream = new MemoryStream(imageData);
#if WINDOWS
			await File.WriteAllBytesAsync($"C:\\Users\\Public\\Pictures\\{fileName}", imageData);
			await DisplayAlert("Success", "Image saved to Pictures folder", "OK");
#elif ANDROID
			var context = Platform.CurrentActivity;
			if (OperatingSystem.IsAndroidVersionAtLeast(29))
			{
				var resolver = context.ContentResolver;
				var contentValues = new Android.Content.ContentValues();
				contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
				contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
				contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, "DCIM/FashionApp");

				var imageUri = resolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, contentValues);
				using var os = resolver.OpenOutputStream(imageUri);
				await stream.CopyToAsync(os);
			}
			else
			{
				var storagePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
				var path = System.IO.Path.Combine(storagePath.ToString(), fileName);
				File.WriteAllBytes(path, imageData);
			}
			await DisplayAlert("Success", "Image saved to Gallery", "OK");
#endif
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to save image: {ex.Message}", "OK");
		}
	}

	public WebViewPage()
	{
		InitializeComponent();

		var htmlContent = @"
			<html>
			<head>
				<meta name='viewport' content='width=device-width, initial-scale=1'>
				<style>
					body { 
						font-family: Arial; 
						margin: 0;
						padding: 20px;
						display: flex;
						flex-direction: column;
						align-items: center;
					}
					.canvas-container {
						position: relative;
						margin-top: 20px;
					}
					canvas {
						border: 1px solid #ccc;
					}
					.controls {
						margin-bottom: 20px;
					}
					input[type=color], button {
						margin: 0 5px;
						padding: 5px 10px;
					}
					input[type=range] {
						width: 200px;
						margin: 0 10px;
					}
				</style>
			</head>
			<body>
				<div class='controls'>
					<input type='color' id='colorPicker' value='#000000'>
					<input type='range' id='brushSize' min='1' max='50' value='5'>
					<button onclick='clearCanvas()'>Clear</button>
					<button onclick='undo()'>Undo</button>
					<button onclick='saveImage()'>Save</button>
				</div>
				<div class='canvas-container'>
					<canvas id='canvas'></canvas>
				</div>
				<script>
					const canvas = document.getElementById('canvas');
					const ctx = canvas.getContext('2d');
					const colorPicker = document.getElementById('colorPicker');
					const brushSize = document.getElementById('brushSize');
					let isDrawing = false;
					let lastX = 0;
					let lastY = 0;
					let drawingHistory = [];
					let currentStep = -1;

					// Загрузка изображения
					const img = new Image();
					img.crossOrigin = 'anonymous';
					img.onload = function() {
						canvas.width = img.width;
						canvas.height = img.height;
						ctx.drawImage(img, 0, 0);
						saveState();
					};
					img.src = 'https://via.placeholder.com/500x500'; // Тук можете да сложите URL на вашето изображение

					function saveState() {
						currentStep++;
						if (currentStep < drawingHistory.length) {
							drawingHistory.length = currentStep;
						}
						drawingHistory.push(canvas.toDataURL());
					}

					function undo() {
						if (currentStep > 0) {
							currentStep--;
							const img = new Image();
							img.onload = function() {
								ctx.clearRect(0, 0, canvas.width, canvas.height);
								ctx.drawImage(img, 0, 0);
							};
							img.src = drawingHistory[currentStep];
						}
					}

					function startDrawing(e) {
						isDrawing = true;
						[lastX, lastY] = [e.offsetX, e.offsetY];
					}

					function draw(e) {
						if (!isDrawing) return;
						ctx.beginPath();
						ctx.moveTo(lastX, lastY);
						ctx.lineTo(e.offsetX, e.offsetY);
						ctx.strokeStyle = colorPicker.value;
						ctx.lineWidth = brushSize.value;
						ctx.lineCap = 'round';
						ctx.stroke();
						[lastX, lastY] = [e.offsetX, e.offsetY];
					}

					function stopDrawing() {
						if (isDrawing) {
							isDrawing = false;
							saveState();
						}
					}

					function clearCanvas() {
						ctx.clearRect(0, 0, canvas.width, canvas.height);
						ctx.drawImage(img, 0, 0);
						saveState();
					}

					function saveImage() {
						const dataUrl = canvas.toDataURL('image/png');
						window.location.href = 'saveimage://' + dataUrl;
					}

					canvas.addEventListener('mousedown', startDrawing);
					canvas.addEventListener('mousemove', draw);
					canvas.addEventListener('mouseup', stopDrawing);
					canvas.addEventListener('mouseout', stopDrawing);

					// Поддержка сенсорных экранов
					canvas.addEventListener('touchstart', (e) => {
						e.preventDefault();
						const touch = e.touches[0];
						const rect = canvas.getBoundingClientRect();
						const x = touch.clientX - rect.left;
						const y = touch.clientY - rect.top;
						[lastX, lastY] = [x, y];
						isDrawing = true;
					});

					canvas.addEventListener('touchmove', (e) => {
						e.preventDefault();
						if (!isDrawing) return;
						const touch = e.touches[0];
						const rect = canvas.getBoundingClientRect();
						const x = touch.clientX - rect.left;
						const y = touch.clientY - rect.top;
						ctx.beginPath();
						ctx.moveTo(lastX, lastY);
						ctx.lineTo(x, y);
						ctx.strokeStyle = colorPicker.value;
						ctx.lineWidth = brushSize.value;
						ctx.lineCap = 'round';
						ctx.stroke();
						[lastX, lastY] = [x, y];
					});

					canvas.addEventListener('touchend', stopDrawing);
				</script>
			</body>
			</html>";

		var htmlSource = new HtmlWebViewSource
		{
			Html = htmlContent
		};
		webView.Source = htmlSource;

		webView.Navigating += async (sender, e) =>
		{
			if (e.Url.StartsWith("saveimage://"))
			{
				e.Cancel = true;
				var imageUrl = e.Url.Replace("saveimage://", "");
				var imageData = Convert.FromBase64String(imageUrl.Split(',')[1]);
				await SaveImageFromUrl(imageData);
			}
		};
	}
}