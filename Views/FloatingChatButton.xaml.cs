using Microsoft.Maui.Controls;
using System;

namespace BellaLink.App.Views
{
	public partial class FloatingChatButton : ContentView
	{
		private double prevX, prevY;

		public FloatingChatButton()
		{
			InitializeComponent();
		}

		private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
		{
			switch (e.StatusType)
			{
				case GestureStatus.Started:
					prevX = 0;
					prevY = 0;
					ChatBubble.Opacity = 0.8;
					break;

				case GestureStatus.Running:
					double deltaX = e.TotalX - prevX;
					double deltaY = e.TotalY - prevY;
					ChatBubble.TranslationX += deltaX;
					ChatBubble.TranslationY += deltaY;
					prevX = e.TotalX;
					prevY = e.TotalY;
					break;

				case GestureStatus.Completed:
				case GestureStatus.Canceled:
					ChatBubble.Opacity = 1.0;
					break;
			}
		}

		private async void OnTapped(object sender, EventArgs e)
		{
			// CORREÇÃO CS0618: ScaleToAsync
			await ChatBubble.ScaleToAsync(0.9, 50);
			await ChatBubble.ScaleToAsync(1.0, 50);

			await Shell.Current.GoToAsync("ChatListPage");
		}
	}
}