using Calendar.Views;
using LeftNavi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Calendar
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // 为不同的菜单创建不同的List类型
        //查询Icon: https://docs.microsoft.com/zh-cn/windows/uwp/style/segoe-ui-symbol-font
        private List<NavMenuItem> navMenuPrimaryItem = new List<NavMenuItem>(
            new[]
            {
                new NavMenuItem()
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Icon = "\xE10F",
                    Label = "主页",
                    Selected = Visibility.Visible,
                    DestPage = typeof(HomeView)
                },

                new NavMenuItem()
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Icon = "\xE11A",
                    Label = "搜索",
                    Selected = Visibility.Collapsed,
                    DestPage = typeof(SearchView)
                },
                  new NavMenuItem()
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Icon = "\xE15D",
                    Label = "音乐",
                    Selected = Visibility.Collapsed,
                    DestPage = typeof(MusicView)
                }
            });

        public MainPage()
        {
            this.InitializeComponent();
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.Purple;
            titleBar.ForegroundColor = Colors.White;
            // 绑定导航菜单
            NavMenuPrimaryListView.ItemsSource = navMenuPrimaryItem;
            //NavMenuSecondaryListView.ItemsSource = navMenuSecondaryItem;
            // SplitView 开关
            PaneOpenButton.Click += (sender, args) =>
            {
                RootSplitView.IsPaneOpen =!RootSplitView.IsPaneOpen;                 
            };
            // 导航事件
            NavMenuPrimaryListView.ItemClick += NavMenuListView_ItemClick;
            NavMenuSecondaryListView.ItemClick += NavMenuListView_ItemClick;

            // 默认页
            RootFrame.SourcePageType = typeof(HomeView);
        }

        private void NavMenuListView_ItemClick(object sender, ItemClickEventArgs e)
        {
       
            // 遍历，将选中Rectangle隐藏
            foreach (var np in navMenuPrimaryItem)
            {
               np.Selected = Visibility.Collapsed;
            }
            //foreach (var ns in navMenuSecondaryItem)
            //{
            //    ns.Selected = Visibility.Collapsed;
            //}
            NavMenuItem item = e.ClickedItem as NavMenuItem;
            // Rectangle显示并导航
            item.Selected = Visibility.Visible;
            if (item.DestPage != null)
            {
                RootFrame.Navigate(item.DestPage);
            }
            RootSplitView.IsPaneOpen = false;
        }
    }
}
