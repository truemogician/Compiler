using Microsoft.Toolkit.Uwp.Notifications;

namespace CMinusMinus.Test {
	public static class Utilities {
		public static void SendNotification(string title, params string[] contents) {
			var builder = new ToastContentBuilder().AddText(title);
			foreach (var content in contents)
				builder.AddText(content);
			try {
				builder.Show();
			}
			catch { }
		}
	}
}