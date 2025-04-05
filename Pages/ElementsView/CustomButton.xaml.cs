namespace FashionApp.Pages.ElementsView;

public partial class CustomButton : ContentView
{
    public static readonly BindableProperty ButtonTextProperty =
           BindableProperty.Create(nameof(ButtonText), typeof(string), typeof(CustomButton), string.Empty);

    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public static readonly BindableProperty ButtonWidthProperty =
           BindableProperty.Create(nameof(ButtonWidth), typeof(double), typeof(CustomButton), 100.0); 

    public double ButtonWidth
    {
        get => (double)GetValue(ButtonWidthProperty);
        set => SetValue(ButtonWidthProperty, value);
    }

    public event EventHandler Clicked;

    public CustomButton()
	{
		InitializeComponent();
	}

    private void InternalButton_Clicked(object sender, EventArgs e) => Clicked?.Invoke(this, e);
}