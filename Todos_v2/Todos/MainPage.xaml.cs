using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Todos.Models;
using Windows.Data.Xml;
using Windows.Data.Xml.Dom;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.StartScreen;
using Windows.ApplicationModel.Background;
using Windows.System;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Todos
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /*单例模式 Singleton Pattern
        static MainPage instance = null;
        static readonly object padlock = new object();
        public static MainPage Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MainPage();
                    }
                    return instance;
                }
            }
        }*/

        public MainPage()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
            this.ViewModel = new ViewModels.TodoItemViewModel();
            ReloadData();
        }

        public void ReloadData()
        {
            List<TodoItem> list = new List<TodoItem>();
              list  = Service.TodoItemDataBase.Instance.SelectAll();
            if (list.Count == 0)
            {
                System.Console.WriteLine("No Histroy data");
            }
            else
            {
                for(var i = 0; i < list.Count; i++)
                {
                    try
                    {
                        ViewModel.AddTodoItemViewModel(list[i]);
                    } catch(Exception ex)
                    {
                        throw new Exception("Fail to load data: " + ex);
                    }
                   
                }
            }
        }

        ViewModels.TodoItemViewModel ViewModel { get; set; }

        private BitmapImage newPic = new BitmapImage();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter.GetType() == typeof(ViewModels.TodoItemViewModel))
            {
                this.ViewModel = (ViewModels.TodoItemViewModel)(e.Parameter);
            }

            if (ViewModel.SelectedItem == null)
            {
                MyTitle.Text = "";
                MyDescription.Text = "";
                createButton.Content = "Create";
            }
            else
            {
                createButton.Content = "Update";
                MyTitle.Text = ViewModel.SelectedItem.title;
                MyDescription.Text = ViewModel.SelectedItem.description;
                MyDate.Date = ViewModel.SelectedItem.duedate;
            }
            ((App)App.Current).BackRequested += OnBackRequested;
            DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;
        }

       private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null) return;
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ((App)App.Current).BackRequested -= OnBackRequested;
            DataTransferManager.GetForCurrentView().DataRequested -= MainPage_DataRequested;
        }

        async private void MainPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var dp = args.Request.Data;
            var deferral = args.Request.GetDeferral();
            var photoFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/background2.jpg"));
            dp.Properties.Title = ViewModel.SelectedItem.title;
            dp.Properties.Description = ViewModel.SelectedItem.description;
            dp.SetStorageItems(new List<StorageFile> { photoFile });
            deferral.Complete(); ;
        }

        private void TodoItem_ItemClicked(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedItem = (Models.TodoItem)(e.ClickedItem);
            if (addAppBarButton.IsEnabled != false)
            {
                Frame.Navigate(typeof(NewPage), ViewModel);
            }
            else
            {
                Frame.Navigate(typeof(MainPage), ViewModel);
            }
        }

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedItem = null;
            Frame.Navigate(typeof(NewPage), ViewModel);
        }


        private string FindErrorMessage()
        {
            string error_message = "";
            if (MyTitle.Text == "") error_message += "Title can't not be null. ";
            if (MyDescription.Text == "") error_message += "Details can't not be null. ";
            if (MyDate.Date < DateTime.Now.Date) error_message += "Due Date can't be earlier";
            return error_message;
        }

        private void CreateButton_Clicked(object sender, RoutedEventArgs e)
        {
            string error_message = FindErrorMessage();
            if (error_message != "")
            {
                var i = new MessageDialog(error_message).ShowAsync();
            }
            else
            {    // check the textbox and datapicker
                 // if ok
                 //更新磁铁
                UpdateTiles();
                if (ViewModel.SelectedItem == null)
                {
                    if (newPic.UriSource == null)
                    {
                        var u = new Uri("ms-appx:///Assets/background2.jpg");
                        newPic = new BitmapImage(u);
                    }
                    TodoItem newItem = new TodoItem(MyTitle.Text, MyDescription.Text, MyDate.Date, false, newPic);
                    ViewModel.AddTodoItemViewModel(newItem);
                }
                else
                {
                    TodoItem newItem = new TodoItem(MyTitle.Text, MyDescription.Text, MyDate.Date, ViewModel.SelectedItem.completed, ViewModel.SelectedItem.pic);
                    newItem.id = ViewModel.SelectedItem.id;
                    ViewModel.UpdateTodoItemViewModel(newItem);
                    ViewModel.SelectedItem = null;
                }
                
                Frame.Navigate(typeof(MainPage), ViewModel);
            }
        }

        private async void UpdateTiles()
        {
            string newTitle = MyTitle.Text;
            string newDescription = MyDescription.Text;
            Uri u = new Uri("ms-appx:///tile.xml");
            StorageFile xmlFile = await StorageFile.GetFileFromApplicationUriAsync(u);
            // Load the string into an XmlDocument
            XmlDocument doc = await XmlDocument.LoadFromFileAsync(xmlFile);
            //通过XML dom向磁贴添加更新的内容
            for (var i = 0; i <= 3; i++)
            {
                XmlNodeList targetRoot = doc.GetElementsByTagName("text");
                targetRoot[i * 2].InnerText = newTitle;
                if (i > 1)
                {
                    targetRoot[i * 2 + 1].InnerText = newDescription;
                }
            }
            // Then create the tile notification
            TileNotification notification = new TileNotification(doc);
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            // Send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }

        private void DeleteButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem != null)
            {
                ViewModel.RemoveTodoItem();
            }
            Frame.Navigate(typeof(MainPage), ViewModel);
        }

        private void CancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            MyTitle.Text = "";
            MyDescription.Text = "";
            MyDate.Date = DateTime.Now.Date;
            ViewModel.SelectedItem = null;
            Frame.Navigate(typeof(MainPage), ViewModel);
        }

        private void SharePhoto_Clicked(object sender, RoutedEventArgs e)
        {
            //获得点击按钮对应的条目
            dynamic ori = e.OriginalSource;
            ViewModel.SelectedItem = (TodoItem)ori.DataContext;
            DataTransferManager.ShowShareUI();
        }

        private void EditItem_Clicked(object sender, RoutedEventArgs e)
        {
            var dc = (sender as FrameworkElement).DataContext;
            var item = ToDoListView.ContainerFromItem(dc) as ListViewItem;
            ViewModel.SelectedItem = (Models.TodoItem)(item.Content);
            if (addAppBarButton.IsEnabled != false)
            {
                Frame.Navigate(typeof(NewPage), ViewModel);
            }
            else
            {
                Frame.Navigate(typeof(MainPage), ViewModel);
            }
        }

        private void DeleteItem_Clicked(object sender, RoutedEventArgs e)
        {
            var dc = (sender as FrameworkElement).DataContext;
            var item = ToDoListView.ContainerFromItem(dc) as ListViewItem;
            ViewModel.SelectedItem = (Models.TodoItem)(item.Content);
            if (ViewModel.SelectedItem != null)
            {
                ViewModel.RemoveTodoItem();
            }
            Frame.Navigate(typeof(MainPage), ViewModel);
        }

        private void Search_ButtonClicked(object sender, RoutedEventArgs e)
        {
            string searchResult = Service.TodoItemDataBase.Instance.Query(SearchBox.Text);
            var i = new MessageDialog(searchResult).ShowAsync();
        }

        private void Search_KeydownEnter(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                string searchResult = Service.TodoItemDataBase.Instance.Query(SearchBox.Text);
            }
        }

       private void checkbox_Click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            dynamic ori = e.OriginalSource;
             ViewModel.SelectedItem = (TodoItem)ori.DataContext;
            if(cb.IsChecked == true)
            {
                ViewModel.SelectedItem.completed = true;
            } else
            {
                ViewModel.SelectedItem.completed = false;
            }
            ViewModel.UpdateTodoItemViewModel(ViewModel.SelectedItem);
        }
    }
}
