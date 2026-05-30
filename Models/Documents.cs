using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Models
{
    public class Documents //Конструктор данных документов
    {
        public int id {  get; set; }
        public string Name { get; set; }
        public DateTime DateDispatch {  get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public int ? Division { get; set; }
        public int ? unit { get; set; }
        public string file_url { get; set; }
    }
}
