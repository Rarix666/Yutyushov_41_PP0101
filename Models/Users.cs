using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Models
{
    public class UserData //Конструктор для авторизованного пользователя
    {
        public int id { get; set; }
        public string login { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public int division { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public int unit { get; set; }
        public string avatar_url { get; set; }
        public string flash_serial { get; set; }
        public string division_name { get; set; }

    }

    public class Divisions
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Units
    {
        public int id { get; set; }
        public string number { get; set; }
    }
}
