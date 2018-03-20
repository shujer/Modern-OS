using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Calendar.ViewModels
{
    class DayItemViewModel
    {
        private ObservableCollection<Models.DayItem> allItems = new ObservableCollection<Models.DayItem>();
        public ObservableCollection<Models.DayItem> AllItems { get { return this.allItems; } }

        private Models.DayItem selectedItem = default(Models.DayItem);
        public Models.DayItem SelectedItem { get { return selectedItem; } set { this.selectedItem = value; } }

        public DayItemViewModel()
        {          
        }

        public void AddDayItem(int m)
        {
            ObservableCollection<Models.DayItem> t = DataBase.Initialize(m);
             if (t.Count == 0)
            {
                int size = 31;
                if (m == 4 || m == 6 || m == 9 || m == 11)
                    size = 30;
                if (m == 2)
                    size = 28;
                for (int i = 1; i <= size; i++)
                {
                    this.allItems.Add(new Models.DayItem(m, i, ""));
                    DataBase.Insert(m, i, "");
                }
            }
            else
            {
                for (int i = 0; i < t.Count; i++)
                    this.allItems.Add(new Models.DayItem(m, i + 1, t[i].note));
            }
            updateTile();
        }

        public void DeleteAllItems()
        {
            this.allItems.Clear();
        }


        public void UpdateDayItem(int month, int day, string note)
        {
            foreach (var i in allItems)
            {
                if (i.month == month && i.day == day) {
                    i.note = note;
                    break;
                }
            }
            DataBase.Update(month, day, note);
            updateTile();
        }

        public async void updateTile()
        {
            Uri u = new Uri("ms-appx:///Tile.xml");
            StorageFile xml = await StorageFile.GetFileFromApplicationUriAsync(u);

            XmlDocument doc = await XmlDocument.LoadFromFileAsync(xml);

            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();

            XmlNodeList tileText = doc.GetElementsByTagName("text");
            for (int i = 0; i < allItems.Count; ++i)
            {
                if (!allItems[i].note.Equals(""))
                {
                    string weekdate = "";
                    int w = 0, y = 17, c = 20, m, d;
                    m = allItems[i].month;
                    if (m == 1) m = 13;
                    if (m == 2) m = 14;
                    d = allItems[i].day;
                    w = (y + (y / 4) + (c / 4) - 2 * c + (26 * (m + 1) / 10) + d - 1) % 7;
                    switch (w)
                    {
                        case 1: weekdate = "星期一"; break;
                        case 2: weekdate = "星期二"; break;
                        case 3: weekdate = "星期三"; break;
                        case 4: weekdate = "星期四"; break;
                        case 5: weekdate = "星期五"; break;
                        case 6: weekdate = "星期六"; break;
                        case 0: weekdate = "星期日"; break;
                        default: break;
                    }

                    tileText[0].InnerText = allItems[i].month.ToString() + "月" + allItems[i].day.ToString() + "日";

                    tileText[1].InnerText = allItems[i].month.ToString() + "月" + allItems[i].day.ToString() + "日";
                    tileText[2].InnerText = weekdate;
                    tileText[3].InnerText = allItems[i].note;

                    tileText[4].InnerText = allItems[i].month.ToString() + "月" + allItems[i].day.ToString() + "日";
                    tileText[5].InnerText = weekdate;
                    tileText[6].InnerText = allItems[i].note;

                    TileNotification notification = new TileNotification(doc);
                    updater.Update(notification);
                }
            }
        }
    }
}