using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Calendar.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomeView : Page
    {
        public HomeView()
        {
            this.InitializeComponent();
            var viewTitleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.BackgroundColor = Windows.UI.Colors.MediumPurple;
           // viewTitleBar.ButtonBackgroundColor = Windows.UI.Colors.CornflowerBlue;


            this.ViewModel = new ViewModels.DayItemViewModel();
            DateTime today = DateTime.Today;
            int m = today.Month;
            int d = today.Day;
            month_select.SelectedIndex = m - 1;
            ViewModel.AllItems[d - 1].color = "CornflowerBlue";
        }

        Calendar.ViewModels.DayItemViewModel ViewModel{get; set;}

        private void CalendarLists_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedItem = (Models.DayItem)(e.ClickedItem);
            note.Text = ViewModel.SelectedItem.note;
            note_write_refresh();
            if (note.Text == "") note.Text = "无事件";
            int w = 0, y = 17, c = 20, m, d;
            m = ViewModel.SelectedItem.month;
            if (m == 1) m = 13;
            if (m == 2) m = 14;
            d = ViewModel.SelectedItem.day;
            w = (y + (y / 4) + (c / 4) - 2 * c + (26 * (m + 1) / 10) + d - 1) % 7;
           // var i = new MessageDialog(w.ToString()).ShowAsync();
            switch (w)
            {
                case 1: date.Text = "星期一 " + d.ToString(); break;
                case 2: date.Text = "星期二 " + d.ToString(); break;
                case 3: date.Text = "星期三 " + d.ToString(); break;
                case 4: date.Text = "星期四 " + d.ToString(); break;
                case 5: date.Text = "星期五 " + d.ToString(); break;
                case 6: date.Text = "星期六 " + d.ToString(); break;
                case 0: date.Text = "星期日 " + d.ToString(); break;
            }
            
        }

        private void month_select_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            note.Text = "";
            date.Text = "";
            note_write_refresh();
            int m = int.Parse(month_select.SelectedIndex.ToString()) + 1;
            ViewModel.DeleteAllItems();
            ViewModel.AddDayItem(m);
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem != null)
            {
                int m = 0, d = 0;
                m = ViewModel.SelectedItem.month;
                d = ViewModel.SelectedItem.day;
                ViewModel.UpdateDayItem(m, d, note_write.Text);
                note_write_refresh();
                note.Text = note_write.Text;
            }
            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            note_write.Text = "";
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            button.Opacity = 1;
            note.IsTextSelectionEnabled = true;
            note_write.Visibility = Visibility.Visible;
            note.Visibility = Visibility.Collapsed;
            note_write.Text = note.Text;
            if (note_write.Text == "无事件")
                note_write.Text = "";
        }

        private void note_write_refresh()
        {
            note_write.Visibility = Visibility.Collapsed;
            note.Visibility = Visibility.Visible;
            button.Opacity = 0;
        }
    }
}
