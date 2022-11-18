namespace PassM_Ger;

public partial class MainPage : ContentPage
{
	#region Main
	public MainPage() => InitializeComponent();

	private string MasterKey;

	protected override async void OnAppearing()
	{
		MasterKey = await PasswordManager.GetMasterKey();
		base.OnAppearing();
	}

	private static void AssignText(Label label, string text)
	{
		label.Text = text;
		SemanticScreenReader.Announce(label.Text);
	}
	private static void AssignText(Entry label, string text)
	{
		label.Text = text;
		SemanticScreenReader.Announce(label.Text);
	}
	#endregion

	#region Alphabet
	private enum charArrayIndex : byte
	{
		Default = 0,
		Numbers = 1,
		Capitals = 2,
		Symbols = 3,
	}

	private readonly Dictionary<byte, string> charArrays = new()
	{
		{ 0, "abcdefghijklmnopqrstuvwxyz" },
		{ 1, "0123456789" },
		{ 2, "ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
		{ 3, "~`! @#$%^&*()_-+={[}]|\\:;\"'<,>.?/" },
	};

	private char[] UpdateAlphabet() =>
		string.Join("", new string[] {
				charArrays[(byte)charArrayIndex.Default],
				IncludeNumbersCB.IsChecked ? charArrays[(byte)charArrayIndex.Numbers] : null,
				IncludeCapitalsCB.IsChecked ? charArrays[(byte)charArrayIndex.Capitals] : null,
				IncludeSymbolsCB.IsChecked ? charArrays[(byte)charArrayIndex.Symbols] : null,
			}.Where(s => s is not null)
		).ToCharArray();
	#endregion

	#region Interactable 
	private void LengthStp_ValueChanged(object sender, ValueChangedEventArgs e)
		=> AssignText(LengthLbl, $"Length: {LengthStp.Value}");

	private void Generate(object sender, EventArgs e)
	{
		char[] alphabet = UpdateAlphabet();
		string password = "";
		for (int i = 0; i < LengthStp.Value; i++) password += alphabet[Random.Shared.Next(0, alphabet.Length - 1)];
		AssignText(PasswordEnt, password);
	}

	private void CopyToClipboard(object sender, EventArgs e)
		=> Clipboard.SetTextAsync(PasswordEnt.Text);

	private async void SaveToManager(object sender, EventArgs e)
	{
		if (MasterKey is null) await DisplayAlert("Warning", "You don't have a Vault yet.\nTo create it, visit \"The Vault\" tab.", "OK");
		else if(string.IsNullOrWhiteSpace(PasswordEnt.Text)) await DisplayAlert("Warning", "Password cannot be empty", "OK");
		else
		{
			string result = await DisplayPromptAsync("New password", "Create a name for new password", maxLength: 20, placeholder: "New Password");
			if (result is not null)
				switch (result)
				{
					case string s when string.IsNullOrWhiteSpace(s):
						await DisplayAlert("Warning", "Name cannot be empty", "OK");
						break;
					case string s when s.EndsWith("#s!"):
						await PasswordManager.SetPassword(s[..^3], PasswordEnt.Text);
						await DisplayAlert("Success", $"Password  {PasswordEnt.Text}\nwas added to The Vault with name {s[..^3]}", "OK");
						break;
					default:
						await PasswordManager.SetPassword(result, PasswordEnt.Text);
						await DisplayAlert("Success", $"Password  {PasswordEnt.Text}\nwas added to The Vault with name {result}", "OK");
						break;
				}
		}
	}
	#endregion
}