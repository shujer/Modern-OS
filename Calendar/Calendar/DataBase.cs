using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Calendar.Models;

namespace Calendar
{
	class DataBase
	{
		private static SQLiteConnection db = App.getConn();
		public static ObservableCollection<DayItem> Initialize(int m)
        {
            ObservableCollection<DayItem> allitems = new ObservableCollection<DayItem>();
            using (var todo = db.Prepare("SELECT * FROM DayItems WHERE Month = ?"))
            {
                todo.Bind(1, m);
                while (SQLiteResult.ROW == todo.Step())
                {
                    
                    allitems.Add(new DayItem(int.Parse(todo[1].ToString()), int.Parse(todo[2].ToString()), (string)todo[3]));
                }
            }
            return allitems;
        }

		public static void Insert(int month, int day, string note)
		{
			try
            {
                using (var todosql = db.Prepare("INSERT INTO DayItems (Year, Month, Day, Note) VALUES (?, ?, ?, ?)"))
                {
                    todosql.Bind(1, 2017);
                    todosql.Bind(2, month);
                    todosql.Bind(3, day);
                    todosql.Bind(4, note);
                    todosql.Step();
                }
            }
            catch (Exception ex)
            {
                var i = new MessageDialog("Insert Error!").ShowAsync();
            }
		}

        public static void Delete(int month)
        {
            try
            {
                using (var todo = db.Prepare("DELETE FROM DayItems WHERE Month = ?"))
                {
                    todo.Bind(1, month);
                    todo.Step();
                }
            }
            catch (Exception ex)
            {
                var i = new MessageDialog("Delete Error!").ShowAsync();
            }
        }

        public static void Update(int month, int day, string note)
        {
            try
            {
                using (var todo = db.Prepare("UPDATE DayItems SET Note = ? WHERE Month = ? AND Day = ?"))
                {
                    todo.Bind(1, note);
                    todo.Bind(2, month);
                    todo.Bind(3, day);
                    todo.Step();
                }
            }
            catch (Exception ex)
            {
                var i = new MessageDialog("Update Error!").ShowAsync();
            }
        }

        public static StringBuilder Find(string key)
        {
            StringBuilder result = new StringBuilder();
            using (var todo = db.Prepare("SELECT * FROM DayItems WHERE Note LIKE '%" + key + "%'"))
            {
                while (SQLiteResult.ROW == todo.Step())
                {
                    string temp = "Date:  " + int.Parse(todo[0].ToString()) + "/" + int.Parse(todo[1].ToString()) + "/" + int.Parse(todo[2].ToString()) +  "   Note: " + (string)todo[3] + "\n";
                    result.Append(temp);
                }
                return result;
            }
        }
	}
}