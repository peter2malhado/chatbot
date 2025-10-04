
using System.Collections.ObjectModel;
namespace chatbot
{
    public partial class FrontPage : ContentPage
    {

        public ObservableCollection<string> Conversations { get; set; } = new();
        public FrontPage()
        {
            InitializeComponent();
            BindingContext = this;


            Conversations.Add("Conversa 1");
            Conversations.Add("Ajuda C#");
            Conversations.Add("Ideias de Projeto");
        }

        private void OnStartChatClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new chatpage());
        }

        private void OnChatSelected(object sender, EventArgs e)
        {
            if (sender is Label label && label.Text is string conversationName)
            {
                // Abre a conversa correspondente (aqui tu depois vais passar o histórico)
                Navigation.PushAsync(new chatpage(conversationName));
            }




        }
    }
}































