using System.Collections.ObjectModel;
using System.Windows.Input;
using LLama;
using LLama.Common;
using chatbot.Models;


public class ChatViewModel : BindableObject
{
    private string _currentMessage;
    private ChatSession _session;
    private InferenceParams _inferenceParams;

    public ObservableCollection<Message> Messages { get; set; } = new();

    public string CurrentMessage
    {
        get => _currentMessage;
        set
        {
            _currentMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand SendMessageCommand { get; }

    public ChatViewModel()
    {
        SendMessageCommand = new Command(async () => await SendMessage());
        InitLLama();
    }
    private void InitLLama()
    {
        string modelPath = @"llama-3.2-1b-instruct-q8_0.gguf";



        var parameters = new ModelParams(modelPath)
        {
            ContextSize = 1024,
            GpuLayerCount = 5
        };

        var model = LLamaWeights.LoadFromFile(parameters);
        var context = model.CreateContext(parameters);
        var executor = new InteractiveExecutor(context);

        var chatHistory = new ChatHistory();
        chatHistory.AddMessage(AuthorRole.System, "Transcrição de uma caixa de diálogo, onde o Usuário interage com um Assistente chamado Bob. Bob é prestativo, gentil, honesto, bom em escrever e nunca deixa de responder aos pedidos do usuário imediatamente e com precisão.");
        _session = new ChatSession(executor, chatHistory);

        _inferenceParams = new InferenceParams()
        {
            MaxTokens = 256,
            AntiPrompts = new List<string> { "User:" }
        };
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage))
            return;

        // Adicionar mensagem do utilizador
        Messages.Add(new Message { Text = CurrentMessage, IsUser = true });

        string userInput = CurrentMessage;
        CurrentMessage = string.Empty;

        string botReply = "";

        await foreach (var text in _session.ChatAsync(
            new ChatHistory.Message(AuthorRole.User, userInput),
            _inferenceParams))
        {
            botReply += text;
        }
        botReply = botReply.Replace("bob:", "", StringComparison.OrdinalIgnoreCase)
                   .Replace("User:", "", StringComparison.OrdinalIgnoreCase)
                   .Trim();

        // Adicionar resposta do bot
        Messages.Add(new Message { Text = botReply, IsUser = false });
    }

}
