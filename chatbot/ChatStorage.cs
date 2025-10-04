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

        // ✅ Carrega todas as conversas
        public static async Task<List<ChatSession>> LoadChatsAsync()
        {
            if (!File.Exists(FilePath))
                return new List<ChatSession>();

            var json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<List<ChatSession>>(json) ?? new();
        }

        // ✅ Salva todas as conversas no JSON
        public static async Task SaveChatsAsync(List<ChatSession> chats)
        {
            var json = JsonSerializer.Serialize(chats, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(FilePath, json);
        }
    }
}