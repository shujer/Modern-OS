using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Todos.ViewModels;

namespace Todos.Models
{
    public class TodoItem 
    {

        public Int64 id { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public DateTime duedate { get; set; }

        public bool completed { get; set; }

        public BitmapImage pic { get; set ; }
       

        public TodoItem(string _title, string _description, DateTimeOffset _date, bool _completed, BitmapImage _pic)
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            this.id = BitConverter.ToInt64(buffer, 0);          
            this.title = _title;
            this.description = _description;
            this.completed = _completed;
            this.duedate = _date.Date;
            if (_pic == null)
            {
                _pic = new BitmapImage(new Uri("ms-appx:///Assets/background2.jpg"));
            }
            this.pic = _pic;
        }
        public TodoItem()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            this.id = BitConverter.ToInt64(buffer,0);
            this.title = default(string);
            this.description = default(string);
            this.completed = false;//默认为未完成
            this.duedate = DateTime.Now.Date;
            this.pic = new BitmapImage(new Uri("ms-appx:///Assets/background2.jpg"));
        }
        }
}
