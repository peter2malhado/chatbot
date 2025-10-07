using System.Collections.ObjectModel;
using chatbot.Models;
using chatbot.Services;

namespace chatbot
{
    public partial class FrontPage : ContentPage
    {
        public ObservableCollection<ChatSession> Conversations { get; set; } = new();

        public FrontPage()
        {
            InitializeComponent();
            BindingContext = this;

            LoadChats();
        }

        private async void LoadChats()
        {
            var chats = await ChatStorage.LoadChatsAsync();
            Conversations.Clear();
            foreach (var chat in chats)
                Conversations.Add(chat);
        }

        // 👉 Botão "Nova Conversa"
        private async void OnStartChatClicked(object sender, EventArgs e)
        {
            var newChat = await ChatStorage.CreateNewChatAsync("Nova Conversa");
            Conversations.Add(newChat);

            // Abre a página do novo chat
            await Navigation.PushAsync(new chatpage(newChat.Id));
        }

        // 👉 Quando o utilizador seleciona uma conversa existente
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
