using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NokiaSimulation;

public partial class MainWindow : Window
{
	public ObservableCollection<string> AvailableTexts { get; set; }
	public List<string> Words { get; set; }

	public bool IsWorking;

	public MainWindow()
	{
		InitializeComponent();
	
		DataContext = this;
		AvailableTexts = new();

		if (File.Exists(@"..\..\..\Words.json"))
		{
			var json = File.ReadAllText(@"..\..\..\Words.json");
			Words = JsonSerializer.Deserialize<List<string>>(json)!;
		}
	}

	private void Add_Click(object sender, RoutedEventArgs e)
	{
		if (!Words.Contains(txtBox.Text))
		{
			Words.Add(txtBox.Text);

			Task.Run(() =>
			{
				var jsonStr = JsonSerializer.Serialize(Words);
				File.WriteAllText(@"..\..\..\Words.json", jsonStr);

				MessageBox.Show("Add process is successfully.", "", MessageBoxButton.OK, MessageBoxImage.Information);
			});
		}
	}

	private void Text_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (string.IsNullOrWhiteSpace(txtBox.Text))
		{
			AvailableTexts.Clear();
			return;
		}

		if (IsWorking) return;

		Task.Run(() =>
		{
			Dispatcher.Invoke(() =>
			{
				AvailableTexts.Clear();

				foreach (var word in Words)
					if (word.ToLower().StartsWith(txtBox.Text.ToLower()))
						AvailableTexts.Add(word);

				IsWorking = true;
				if (AvailableTexts.Count > 0)
				{
					string init = AvailableTexts[0];
					int start = txtBox.Text.Length;
					int length = init.Length - txtBox.Text.Length;
					txtBox.Text += init.Remove(0, start);
					txtBox.Select(start, length);
				}
				IsWorking = false;
			});
		});
	}

	private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (sender is ListView list)
		{
			IsWorking = true;
			txtBox.Text = list.SelectedItem as string;
			AvailableTexts.Clear();
			IsWorking = false;
		}
	}
}