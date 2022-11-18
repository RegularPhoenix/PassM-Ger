using System.Collections.ObjectModel;

namespace PassM_Ger;

public partial class PasswordManager : ContentPage
{
	#region Main
	public PasswordManager() => InitializeComponent();

	private string masterKey;

	protected override async void OnAppearing()
	{
		masterKey = await GetMasterKey();

		if (masterKey is null)
		{
			WelcomeTextLbl.IsVisible = true;
			HeaderLbl.FontSize = 36;
			MasterKeyEnt.Placeholder = "Press enter to confirm";
		}
		else
		{
			WelcomeTextLbl.IsVisible = false;
			HeaderLbl.FontSize = 48;
			MasterKeyEnt.Placeholder = "Enter the master password";
		}
		base.OnAppearing();
	}
	#endregion

	#region Passwords
	public class PasswordPair
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}

	/// <summary>
	/// Adds the Key-Value pair as value to the SecureStorage with key "kvp{index}".
	/// If the pair with that index already exists in the SecureStorage, overwrites its value.
	/// If the index is not specified, the key will be "kvp{last password index + 1}".
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <param name="index"></param>
	internal static async Task SetPassword(string key, string value, int index = -1)
	{
		if (index is -1)
		{
			int passwordsCount = (await GetPasswords()).Count();
			await SecureStorage.Default.SetAsync($"kvp{passwordsCount}", $"{key}#s!{value}");
		}
		else await SecureStorage.Default.SetAsync($"kvp{index}", $"{key}#s!{value}");
	}

	/// <summary>
	/// Gets all saved passwords
	/// </summary>
	/// <returns>The IEnumerable that contains all PasswordPairs currently stored in the SecureStorage.</returns>
	internal static async Task<IEnumerable<PasswordPair>> GetPasswords()
	{
		List<PasswordPair> keyValuePairs = new();
		int count = 0;
		while (true)
		{
			string get = await GetKVP($"kvp{count}");
			if (get is not null)
			{
				string[] kvp = get.Split("#s!", 2);
				keyValuePairs.Add(new PasswordPair { Name = kvp[0], Value = kvp[1] });
				count++;
			}
			else return keyValuePairs;
		}
	}

	/// <summary>
	/// Removes the password at the specified index (key in SecureStorage is "kvp{index}").
	/// </summary>
	/// <param name="index"></param>
	internal static async Task RemovePassword(int index)
	{
		// Remove the PasswordPair at index
		SecureStorage.Default.Remove($"kvp{index}");

		while (true)
		{
			string get = await GetKVP($"kvp{index + 1}");
			if (get is not null)
			{
				string[] kvp = get.Split("#s!", 2);
				await SecureStorage.Default.SetAsync($"kvp{index}", $"{kvp[0]}#s!{kvp[1]}");
				index++;
				SecureStorage.Default.Remove($"kvp{index}");
			}
			else break;
		}
	}

	private static async Task<string> GetKVP(string key) => await SecureStorage.Default.GetAsync(key);
	#endregion

	#region Master Key

	/// <summary>
	/// Gets the current master key.
	/// </summary>
	/// <returns>String that represents the master key.</returns>
	internal static async Task<string> GetMasterKey() => await SecureStorage.Default.GetAsync("master");

	private static async Task SetMasterKey(string masterKey) => await SecureStorage.Default.SetAsync("master", masterKey);

	private async void MasterKeyEntered(object sender, EventArgs e)
	{
		string key = ((Entry)sender).Text;
		string masterKey = await GetMasterKey();

		if (masterKey is null)
		{
			bool correct = !string.IsNullOrEmpty(key) && key.Length is >= 8 and <= 127;

			string reply = key switch
			{
				string s when string.IsNullOrEmpty(s) => "Password cannot be empty",
				string s when s.Length < 8 => "Password is too short (Minimum length: 8)",
				string s when s.Length > 127 => "Password is too long (Maximum length: 127)",
				_ => $"Your password is: {key}.\nDo you want to proceed?",
			};

			bool answer = false;

			if (correct) answer = await DisplayAlert("Master Key", reply, "Confirm", "Return");
			else await DisplayAlert("Master Key", reply, "OK");

			if (answer is true)
			{
				await SetMasterKey(key);
				await DisplayAlert("Success", "Master password was created successfully.\nPlease restart the app to access your vault.", "OK");
			}
		}
		else if (key == masterKey) await Navigation.PushAsync(new TheVault());
		else await DisplayAlert("Warning", "Incorrect password", "OK");
	}
	#endregion
}