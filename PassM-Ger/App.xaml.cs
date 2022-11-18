namespace PassM_Ger;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}

	protected override Window CreateWindow(IActivationState activationState)
	{
		Window window = base.CreateWindow(activationState);
		window.MinimumHeight = window.MaximumHeight = 354;
		window.MinimumWidth = window.MaximumWidth = 720;
		return window;
	}
}
