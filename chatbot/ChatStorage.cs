using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using chatbot.Models;

namespace chatbot.Services
{
    public static class ChatStorage
    {
        private static string GetFilePath()
        {
            // Tenta usar um local mais acessível para salvar o arquivo JSON
            try
            {
                // Para Windows, usa o diretório LocalApplicationData com o nome do app
                if (OperatingSystem.IsWindows())
                {
                    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var appFolder = Path.Combine(appDataPath, "Chatbot");
                    
                    // Cria a pasta se não existir
                    if (!Directory.Exists(appFolder))
                    {
                        Directory.CreateDirectory(appFolder);
                    }
                    
                    return Path.Combine(appFolder, "chats.json");
                }
            }
            catch
            {
                // Se falhar, usa o diretório padrão do MAUI
            }
            
            // Fallback para o diretório padrão do MAUI (funciona em todas as plataformas)
            return Path.Combine(FileSystem.AppDataDirectory, "chats.json");
        }
        
        private static readonly string FilePath = GetFilePath();

        // 📍 Obter o caminho do arquivo JSON (útil para debug)
        public static string GetChatsFilePath() => FilePath;

        // 🚀 Carrega todos os chats
        public static async Task<List<ChatSession>> LoadChatsAsync()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return new List<ChatSession>();
                }

                string json = await File.ReadAllTextAsync(FilePath);
                return JsonSerializer.Deserialize<List<ChatSession>>(json) ?? new List<ChatSession>();
            }
            catch
            {
                return new List<ChatSession>(); // Em caso de erro, começa vazio
            }
        }

        // 💾 Salva todos os chats
        public static async Task SaveChatsAsync(List<ChatSession> chats)
        {
            string json = JsonSerializer.Serialize(chats, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json);
        }

        // ➕ Cria um novo chat com ID automático
        public static async Task<ChatSession> CreateNewChatAsync(string title = "Nova Conversa")
        {
            var chats = await LoadChatsAsync();

            int nextId = chats.Count + 1;
            string newId = $"chat{nextId}";

            var newChat = new ChatSession
            {
                Id = newId,
                Title = title,
                Messages = new List<ChatMessage>()
            };

            chats.Add(newChat);
            await SaveChatsAsync(chats);

            return newChat;
        }

        // 🔍 Obter chat por ID
        public static async Task<ChatSession?> GetChatByIdAsync(string id)
        {
            var chats = await LoadChatsAsync();
            return chats.FirstOrDefault(c => c.Id == id);
        }

        // 📝 Adicionar mensagem a uma conversa
        public static async Task AddMessageToChatAsync(string chatId, string role, string text)
        {
            var chats = await LoadChatsAsync();
            var chat = chats.FirstOrDefault(c => c.Id == chatId);

            if (chat != null)
            {
                chat.Messages.Add(new ChatMessage { Role = role, Text = text });
                await SaveChatsAsync(chats);
            }
        }
    }
}