namespace IndiaRose.WebAPI.Sdk.Runner
{
	public class SettingsModel
	{
		public string TopBackgroundColor { get; set; }

		public string BottomBackgroundColor { get; set; }

		public string ReinforcerColor { get; set; }

		public string TextColor { get; set; }

		public int SelectionAreaHeight { get; set; }

		public int IndiagramDisplaySize { get; set; }

		public string FontName { get; set; }

		public int FontSize { get; set; }

		public bool IsReinforcerEnabled { get; set; }

		public bool IsDragAndDropEnabled { get; set; }

		public bool IsCategoryNameReadingEnabled { get; set; }

		public bool IsBackHomeAfterSelectionEnabled { get; set; }

		public bool IsMultipleIndiagramSelectionEnabled { get; set; }

		public float TimeOfSilenceBetweenWords { get; set; }

		public SettingsModel()
		{
			TopBackgroundColor = "FF0000";
			BottomBackgroundColor = "0000FF";
			ReinforcerColor = "FF00FF";
			TextColor = "000000";
			SelectionAreaHeight = 70;
			IndiagramDisplaySize = 200;
			FontName = "/system/fonts/serif.ttf";
			FontSize = 22;
			IsReinforcerEnabled = true;
			IsDragAndDropEnabled = false;
			IsCategoryNameReadingEnabled = true;
			IsBackHomeAfterSelectionEnabled = true;
			IsMultipleIndiagramSelectionEnabled = false;
			TimeOfSilenceBetweenWords = 2.5f;
		}
	}

}
