using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc
{
    public class UserData //Конструктор для пользователя
    {
        public int id { get; set; }
        public string login { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string division { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string unit { get; set; }

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
