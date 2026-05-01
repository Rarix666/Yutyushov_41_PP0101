using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Models
{
    public class AdminData
    {
        public int id {  get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string login { get; set; }
        public string name { get; set; }
        public string unit { get; set; }
        public string division { get; set; }
        public bool is_locked { get; set; }

        public string StatusText => is_locked ? "Заблокирован" : "Активен";
    }
}
