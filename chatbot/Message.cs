using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatbot.Models
{
    public class Message
    {
        public string Text { get; set; }
        public bool IsUser { get; set; } // true = utilizador, false = assistente
    }
}
