using static PassM_Ger.PasswordManager;

namespace PassM_Ger;

public partial class TheVault : ContentPage
{
	#region Main
	public static IEnumerable<PasswordPair> PasswordPairs { get; private set; }

	public TheVault() => InitializeComponent();

	protected override void OnAppearing()
	{ 
		UpdateList();
		base.OnAppearing();
	}

	private async void UpdateList()
	{
		PasswordPairs = await GetPasswords();
		PasswordListView.ItemsSource = PasswordPairs;
		HeaderLbl.IsVisible = PasswordPairs is null && !PasswordPairs.Any();
		selectedPasswordPair = null;
		ChangeNameBtn.IsEnabled = ChangePasswordBtn.IsEnabled = CopyBtn.IsEnabled = DeletePasswordBtn.IsEnabled = false;
		ChangeNameBtn.BackgroundColor = ChangePasswordBtn.BackgroundColor = CopyBtn.BackgroundColor = DeletePasswordBtn.BackgroundColor = Color.FromArgb("#FF778899");
	}
	#endregion

	#region Buttons
	private Tuple<PasswordPair, int> selectedPasswordPair = null;

	private void OnPasswordSelected(object sender, SelectedItemChangedEventArgs e)
	{
		selectedPasswordPair = (PasswordPairs.ToList()[e.SelectedItemIndex], e.SelectedItemIndex).ToTuple();
		ChangeNameBtn.IsEnabled = ChangePasswordBtn.IsEnabled = CopyBtn.IsEnabled = DeletePasswordBtn.IsEnabled = true;
		ChangeNameBtn.BackgroundColor = ChangePasswordBtn.BackgroundColor = CopyBtn.BackgroundColor = DeletePasswordBtn.BackgroundColor = Color.FromArgb("#FF228B22");
	}

	private void CopyToClipboard(object sender, EventArgs e)
		=> Clipboard.SetTextAsync(selectedPasswordPair.Item1.Value);

	private async void ChangeName(object sender, EventArgs e)
	{
		string newName = await DisplayPromptAsync("Change name", "Enter new name");
		if (newName is not null)
		{
			await SetPassword(newName, selectedPasswordPair.Item1.Value, selectedPasswordPair.Item2);
			UpdateList();
		}		
	}

	private async void ChangePassword(object sender, EventArgs e)
	{
		string newValue = await DisplayPromptAsync("Change password", "Enter new password");
		if (newValue is not null)
		{
			await SetPassword(selectedPasswordPair.Item1.Name, newValue, selectedPasswordPair.Item2);
			UpdateList();
		}			
	}

	private async void RemovePassword(object sender, EventArgs e)
	{
		bool confirmed = await DisplayAlert("Delete password", "Are you sure you want to delete this password?", "Confirm", "Cancel");
		if (confirmed)
		{
			await PasswordManager.RemovePassword(selectedPasswordPair.Item2);
			UpdateList();
		}
	}
	#endregion
}

