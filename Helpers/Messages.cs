using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BellaLink.App.Helpers
{
	// Mensagem enviada quando um comentário é adicionado
	public class CommentAddedMessage : ValueChangedMessage<string>
	{
		public CommentAddedMessage(string videoId) : base(videoId)
		{
		}
	}
}