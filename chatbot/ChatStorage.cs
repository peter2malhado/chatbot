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
        private static readonly string FilePath =
            Path.Combine(FileSystem.AppDataDirectory, "chats.json");

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