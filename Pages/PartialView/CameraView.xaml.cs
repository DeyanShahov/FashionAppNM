namespace FashionApp.Pages.PartialView;

public partial class CameraView : ContentView
{
    public event EventHandler Clicked;
    public CameraView()
	{
		InitializeComponent();
	}

    private void Capture_Clicked(object sender, EventArgs e) => Clicked?.Invoke(this, e);

    private void Close_Clicked(object sender, EventArgs e) => Clicked?.Invoke(this, e);
}