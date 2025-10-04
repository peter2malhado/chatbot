using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LLama;
using LLama.Common;
using chatbot.Services;
using chatbot.Models;
using Microsoft.Maui.Controls; // For BindableObject / Command

public class ChatViewModel : BindableObject
{
    private string _currentMessage;

    // LLama session (nome diferente + tipo qualificado para evitar ambiguidade)
    private LLama.ChatSession _llamaSession;

    private InferenceParams _inferenceParams;

    // Conversa ativa (o teu modelo)
    private chatbot.Models.ChatSession currentSession;
    private List<chatbot.Models.ChatSession> allChats;

    public ObservableCollection<Message> Messages { get; } = new();

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

        // Inicializa o LLaMA em background para não bloquear a UI
        Task.Run(() => InitLLama());

        // Carrega as conversas guardadas (não bloqueante)
        _ = LoadSessionAsync();
    }

    private void InitLLama()
    {
        try
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

            // Usa o tipo qualificado LLama.ChatSession
            _llamaSession = new LLama.ChatSession(executor, chatHistory);

            _inferenceParams = new InferenceParams()
            {
                MaxTokens = 256,
                AntiPrompts = new List<string> { "User:", "System:", "Assistant:" } // evita que o modelo escreva papéis
            };
        }
        catch (Exception ex)
        {
            // Guarda/mostra o erro (podes adaptar para um logger ou mostrar na UI)
            System.Diagnostics.Debug.WriteLine("InitLLama error: " + ex);
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage))
            return;

        // Texto do utilizador
        string userInput = CurrentMessage;

        // Adicionar mensagem do utilizador na UI
        Messages.Add(new Message { Text = userInput, IsUser = true });

        // Garantir que a sessão atual e a lista de chats estão carregadas
        if (allChats == null || currentSession == null)
        {
            allChats = await ChatStorage.LoadChatsAsync();
            currentSession = allChats.FirstOrDefault(c => c.Id == "default");
            if (currentSession == null)
            {
                currentSession = new chatbot.Models.ChatSession
                {
                    Id = "default",
                    Title = "Conversa Principal"
                };
                allChats.Add(currentSession);
            }
        }

        // Adicionar ao histórico guardado e salvar
        currentSession.Messages.Add(new ChatMessage { Role = "user", Text = userInput });
        await ChatStorage.SaveChatsAsync(allChats);

        // limpa input
        CurrentMessage = string.Empty;

        // Gerar resposta do modelo
        string botReply = "";

        if (_llamaSession == null)
        {
            // O modelo ainda não carregou — resposta curta para o utilizador
            botReply = "O modelo ainda está a carregar. Tenta novamente em alguns segundos.";
        }
        else
        {
            await foreach (var text in _llamaSession.ChatAsync(
                new ChatHistory.Message(AuthorRole.User, userInput),
                _inferenceParams))
            {
                botReply += text;
            }

            botReply = botReply
                .Replace("bob:", "", StringComparison.OrdinalIgnoreCase)
                .Replace("User:", "", StringComparison.OrdinalIgnoreCase)
                .Trim();
        }

        // Adicionar resposta do bot na UI
        Messages.Add(new Message { Text = botReply, IsUser = false });

        // Guardar resposta no JSON
        currentSession.Messages.Add(new ChatMessage { Role = "bot", Text = botReply });
        await ChatStorage.SaveChatsAsync(allChats);
    }

    private async Task LoadSessionAsync()
    {
        try
        {
            allChats = await ChatStorage.LoadChatsAsync();

            currentSession = allChats.FirstOrDefault(c => c.Id == "default");
            if (currentSession == null)
            {
                currentSession = new chatbot.Models.ChatSession
                {
                    Id = "default",
                    Title = "Conversa Principal"
                };
                allChats.Add(currentSession);
                await ChatStorage.SaveChatsAsync(allChats);
            }

            // Carregar mensagens salvas para a UI
            foreach (var msg in currentSession.Messages)
            {
                Messages.Add(new Message { Text = msg.Text, IsUser = (msg.Role == "user") });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("LoadSessionAsync error: " + ex);
        }
    }
}
