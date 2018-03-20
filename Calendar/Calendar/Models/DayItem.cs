using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendar.Models
{
    class DayItem
    {
        private string id;

        public string color { get; set; }

        public int year { get; set; }

        public int month { get; set; }

        public int day { get; set; }

        public string note { get; set; }

        public DayItem(int month, int day, string note)
        {
            this.id = Guid.NewGuid().ToString(); //生成id
            this.color = "DarkGray";
            this.month = month;
            this.day = day;
            this.note = note;
        }
    }
}
