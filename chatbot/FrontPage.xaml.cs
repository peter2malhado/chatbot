using System.Collections.ObjectModel;
using chatbot.Models;

namespace chatbot
{
    public partial class FrontPage : ContentPage
    {
        public ObservableCollection<ChatSession> Conversations { get; set; } = new();

        public FrontPage()
        {
            InitializeComponent();
            BindingContext = this;

            Conversations.Add(new ChatSession { Id = "chat1", Title = "Conversa 1" });
            Conversations.Add(new ChatSession { Id = "chat2", Title = "Ajuda C#" });
            Conversations.Add(new ChatSession { Id = "chat3", Title = "Ideias de Projeto" });
        }

        private void OnStartChatClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new chatpage()); // Nova conversa
        }

        private async void OnChatSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ChatSession selectedChat)
            {
                await Navigation.PushAsync(new chatpage(selectedChat.Id));
            }

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
