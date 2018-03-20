using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using Todos.Models;
using Windows.UI.Popups;

namespace Todos.Service
{

    public class TodoItemDataBase
    {
        #region singleton       
        static TodoItemDataBase instance = null;
        static readonly object padlock = new object();
        public static TodoItemDataBase Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new TodoItemDataBase();
                    }
                    return instance;
                }
            }
        }
        #endregion

        public static SQLiteConnection conn = null;
        private static String DB_NAME = "TodoList.db";
        private static String SQL_SELECT_FROM_ID = "SELECT Id, Title, Description, Date, Completed, ImageUri FROM TodoItem WHERE Id = ?";
        private static String SQL_SELECT_ALL = "SELECT * FROM TodoItem";
        private static String SQL_INSERT = "INSERT INTO TodoItem (Id, Title, Description, Date, Completed, ImageUri) VALUES (?, ?, ?, ?, ?, ?)";
        private static String SQL_UPDATE = "UPDATE TodoItem SET Title = ?, Description = ?, Date = ?, Completed = ?, ImageUri = ? WHERE Id = ?";
        private static String SQL_DELETE = "DELETE FROM TodoItem WHERE Id = ?";
        public static String SQL_QUERY_COLOMN = "SELECT Title, Description, Date FROM TodoItem ";
        private static String SQL_CREATE_TABLE = @"CREATE TABLE IF NOT EXISTS
                                                   TodoItem (Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                                                             Title  VARCHAR( 140 ),
                                                             Description    VARCHAR( 140 ),
                                                             Date   VARCHAR( 140 ),
                                                             Completed  VARCHAR( 140 ),
                                                             ImageUri   VARCHAR( 140 )
                                                   );";


        public void Init()
        {
            conn = new SQLiteConnection(DB_NAME);
            conn.ChangesCount();
            try
            {
                using (var statement = conn.Prepare(SQL_CREATE_TABLE))
                {
                statement.Step();
                }
            }catch (SQLiteException ex)
            {
                throw new SQLiteException("fail to create :" + ex);
            }
            
        }

        #region insertItem
        public void Insert(TodoItem newItem)
        {
            try
            {
                using (var item = conn.Prepare(SQL_INSERT))
                {
                    string duedate = JsonConvert.SerializeObject(newItem.duedate);
                    string completed = JsonConvert.SerializeObject(newItem.completed);
                    string imageUri = JsonConvert.SerializeObject(newItem.pic.UriSource);
                    item.Bind(1, newItem.id);
                    item.Bind(2, newItem.title);
                    item.Bind(3, newItem.description);
                    item.Bind(4, duedate);
                    item.Bind(5, completed);
                    item.Bind(6, imageUri);
                    item.Step();
                }
            }catch(Exception ex)
            {
                throw new Exception("Insert fail: " + ex);
            }
        }
    #endregion

        #region selectItemfromId
        public TodoItem Select(Int64 id)
        {
            TodoItem item = new TodoItem();
            using(var statement = conn.Prepare(SQL_SELECT_FROM_ID))
            {
                statement.Bind(1, id);
                if(SQLiteResult.DONE == statement.Step())
                {
                    item = new TodoItem();
                    item.id = (Int64)statement[0];
                    item.title = (String)statement[2];
                    item.description = (String)statement[3];
                    string tmp = (string)statement[4];
                    item.duedate = DateTime.Parse(tmp);
                    item.completed = Boolean.Parse((string)statement[5]);
                    String tmpu = (String)statement[6];
                    item.pic = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(tmpu));
                }
            }
            return item;
        }
        #endregion

        #region queryItem
        public string Query(string message)
        {
            string result = "";
            using (var statement = conn.Prepare(SQL_QUERY_COLOMN + "WHERE (Title LIKE '%"+message+"%' OR Description LIKE '%" + message + "%' OR Date LIKE '%" + message + "%')"))
            {
                while (statement.Step() != SQLiteResult.DONE)
                {
                    String tmpDate = (String)statement[2];
                    DateTime date = (DateTime)JsonConvert.DeserializeObject(tmpDate);
                    result += "Title: " + (string)statement[0] + " Description: " + (string)statement[1] + "Date: " + date.GetDateTimeFormats()[0] + "\n";
                }
            }
            return result;
        }
        #endregion

        #region deleteItemfromId
        public void Delete(Int64 id)
        {
            using (var statement = conn.Prepare(SQL_DELETE))
            {
                statement.Bind(1, id);
                statement.Step();
            }
        }
        #endregion

        #region updateItem
        public void Update(TodoItem ud)
        {
            TodoItem existingItem = Select(ud.id);
            if(existingItem != null)
            {
                using(var item = conn.Prepare(SQL_UPDATE))
                {
                    string duedate = JsonConvert.SerializeObject(ud.duedate);
                    string completed = JsonConvert.SerializeObject(ud.completed);
                    string imageUri = JsonConvert.SerializeObject(ud.pic.UriSource);
                    item.Bind(1, ud.title);
                    item.Bind(2, ud.description);
                    item.Bind(3, duedate);
                    item.Bind(4, completed);
                    item.Bind(5, imageUri);
                    item.Bind(6, ud.id);
                    item.Step();
                }
            }
        }
        #endregion

        #region selectAll
        public List<TodoItem> SelectAll()
        {
            List<TodoItem> newList = new List<TodoItem>();
            TodoItem item = new TodoItem();
            using (var statement = conn.Prepare(SQL_SELECT_ALL))
            {
                while (statement.Step() != SQLiteResult.DONE)
                {                    
                    item = new TodoItem();
                    item.id = (Int64)statement[0];
                    item.title = (String)statement[1];
                    item.description = (String)statement[2];
                    String tmp = (String)statement[3];
                    item.duedate = (DateTime)JsonConvert.DeserializeObject(tmp);
                    item.completed = Boolean.Parse((string)statement[4]);
                    String tmpu = (String)JsonConvert.DeserializeObject((string)statement[5]);
                    item.pic = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(tmpu));
                    newList.Add(item);
                }
            }
            return newList;
        } 
#endregion


    }
}