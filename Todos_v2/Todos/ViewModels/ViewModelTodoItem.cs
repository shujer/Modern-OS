using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todos.Models;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Todos.ViewModels
{
    class TodoItemViewModel : BindableBase
    {

        private ObservableCollection<TodoItem> allItems = new ObservableCollection<TodoItem>();
        public ObservableCollection<TodoItem> AllItems { get { return this.allItems; } }

        private TodoItem selectedItem = default(TodoItem);
        public TodoItem SelectedItem { get { return selectedItem; } set { this.selectedItem = value; } }

        public TodoItemViewModel()
        {
        }

        public void AddTodoItemViewModel(TodoItem t)
        {
            this.allItems.Add(t);
            Service.TodoItemDataBase.Instance.Insert(t);
        }


        public void RemoveTodoItem()
        {
            Service.TodoItemDataBase.Instance.Delete(this.selectedItem.id);
            this.allItems.Remove(this.selectedItem);
            this.selectedItem = null;
        }

        public void UpdateTodoItemViewModel(TodoItem t)
        {
            t.id = this.selectedItem.id;
            this.allItems.Add(t);
            Service.TodoItemDataBase.Instance.Update(t);
            this.allItems.Remove(this.selectedItem);
        }
    }
}
