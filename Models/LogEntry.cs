using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Models
{
    public class LogEntry //Конструктор логов
    {
        public int id {  get; set; }
        public DateTime timestamp { get; set; }
        public string level { get; set; }
        public string message { get; set; }
        public string user_login { get; set; }
        public string machine_name { get; set; }
        public string app_version { get; set; }
    }
}
