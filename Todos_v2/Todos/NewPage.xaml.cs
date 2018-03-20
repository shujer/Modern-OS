using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Todos.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Todos
{
    public sealed partial class NewPage : Page
    {
        public NewPage()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.CornflowerBlue;
            viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;
        }

        private ViewModels.TodoItemViewModel ViewModel;

        public object rootFrame { get; private set; }

        private BitmapImage newPic = new BitmapImage();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (e.Parameter.GetType() == typeof(ViewModels.TodoItemViewModel))
                {
                    this.ViewModel = (ViewModels.TodoItemViewModel)(e.Parameter);
                }
                if (ViewModel.SelectedItem == null)
                {
                    MyTitle.Text = "";
                    MyDescription.Text = "";
                    MyDate.Date = DateTime.Now.Date;
                    createButton.Content = "Create";
                    ViewModel.SelectedItem = null;
                    var i = new MessageDialog("Welcome!").ShowAsync();
                }
                else
                {
                    createButton.Content = "Update";
                    MyTitle.Text = ViewModel.SelectedItem.title;
                    MyDescription.Text = ViewModel.SelectedItem.description;
                    MyDate.Date = ViewModel.SelectedItem.duedate;
                    background_img.Source = ViewModel.SelectedItem.pic;
                }
            }
            catch (Exception)
            {
                throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
            }
             ((App)App.Current).BackRequested += OnBackRequested;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null) return;
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
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
        {//将selectionItem设为null

            string error_message = FindErrorMessage();
            if (error_message != "")
            {
                var i = new MessageDialog(error_message).ShowAsync();
            }
            else
            {    // check the textbox and datapicker
                 // if ok
                UpdateTiles();
                if (ViewModel.SelectedItem == null)
                {
                    if (newPic.UriSource == null)
                    {
                        var u = new Uri("ms-appx:///Assets/background2.jpg");
                        newPic = new BitmapImage(u);
                    }
                    TodoItem newItem = new TodoItem(MyTitle.Text,MyDescription.Text,MyDate.Date,false,newPic);
                    ViewModel.AddTodoItemViewModel(newItem);
                }
                else
                {
                    if (newPic.UriSource == null)
                    {
                        newPic = new BitmapImage(ViewModel.SelectedItem.pic.UriSource);
                    }
                    TodoItem newItem = new TodoItem(MyTitle.Text, MyDescription.Text, MyDate.Date, ViewModel.SelectedItem.completed, newPic);
                    newItem.id = ViewModel.SelectedItem.id;
                    ViewModel.UpdateTodoItemViewModel(newItem);
                }
                Frame.Navigate(typeof(MainPage), ViewModel);
            }
        }

        private void DeleteButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem != null)
            {
                ViewModel.RemoveTodoItem();
            }
            Frame.Navigate(typeof(MainPage), ViewModel);
        }


        private async void SelectPictureButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a stream for the selected file 
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            // Ensure a file was selected 
            if (file != null)
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap 
                    newPic.DecodePixelWidth = 350; //match the target Image.Width, not shown
                    await newPic.SetSourceAsync(fileStream);
                    var t = fileStream.AsStream(); 
                    var tmpUri = new Uri("ms-appx:///Assets/" + file.Name);
                    newPic.UriSource = tmpUri;
                    background_img.Source = newPic;
                }
            }
           
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MyTitle.Text = "";
            MyDescription.Text = "";
            MyDate.Date = DateTime.Now.Date;
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
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
            TileNotification notification = new TileNotification(doc);
            // Send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }
    }
}

