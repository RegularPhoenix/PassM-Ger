namespace PassM_Ger;

public partial class Settings : ContentPage
{
	public Settings() => InitializeComponent();

	private async void HardReset(object sender, EventArgs e)
	{
		HardResetBtn.IsEnabled = false;
		string c = await DisplayPromptAsync("Warning", "Are you sure you want to delete all data?\nThis action cannot be undone. Type CONFIRM to proceed.");
		if (c is "CONFIRM")
		{
			SecureStorage.Default.RemoveAll();
			await DisplayAlert("Done", "All data deleted successfully", "OK");
		}
		else if(c is not null) HardReset(sender, e);
		HardResetBtn.IsEnabled = true;
	}
}