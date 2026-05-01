using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISDisciplineDesc.Models
{
    public class Documents
    {
        public int id {  get; set; }
        public string Name { get; set; }
        public DateTime DateDispatch {  get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string Division { get; set; }
        public string unit { get; set; }
        public string file_url { get; set; }
    }
}
