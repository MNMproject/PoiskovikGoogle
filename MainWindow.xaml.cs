using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace PoiskovikGoogle
{
    public class WebList : INotifyPropertyChanged
    {
        public WebList() { }

        public WebList(string web)
        {
            Web = web;
        }

        private string web;

        public string Web
        {
            get { return web; }
            set
            {
                web = value;
                OnPropertyChanged("web");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }
        }
    }

    public partial class MainWindow : Window
    {

        public static ObservableCollection<WebList> web = new();

        public MainWindow()
        {
            InitializeComponent();
            Vizor1.ItemsSource = web;
        }

        private void ButtonSeach_Click(object sender, RoutedEventArgs e)
        {
            web.Clear();
            if (GoogleWeb.IsChecked == true && YandexWeb.IsChecked==false)
            {
                ListWeb(SearchGoogle());
                
            }
            else if (YandexWeb.IsChecked == true && GoogleWeb.IsChecked==false)
            {
                ListWeb(SearchYandex());
            }
        }

        private (string, char, char, char, int) SearchGoogle()
        {
            var client = new HttpClient();
            const string URL = "https://www.google.com/search?q=";
            var search = URL + Seach.Text;
            var responseStream = client.GetStreamAsync(new Uri(search)).Result;
            using var stream = new StreamReader(responseStream);
            var response = stream.ReadToEndAsync().Result;
            (string, char, char, char, int) WebRequest = (response, 'q', '=', 'h', 0);
            return WebRequest;
        }

        private (string, char, char, char, int) SearchYandex()
        {
            var client = new HttpClient();
            const string URL = "https://yandex.ru/search/?text=";
            var search = URL + Seach.Text;
            var responseStream = client.GetStreamAsync(new Uri(search)).Result;
            using var stream = new StreamReader(responseStream);
            var response = stream.ReadToEndAsync().Result;
            (string, char, char, char, int) WebRequest = (response, 't', ';', 'h', 1);
            return WebRequest;
        }

        private void ListWeb((string, char, char, char, int) WebRequest)
        {
            char oldSymbl = ' ';
            char semiSymbl = ' ';
            char newSymbl = ' ';
            char tempSymbl = ' ';
            bool flagSearchStart = false;
            bool flagSearchStop = false;
            string temp = "";
            string newTemp = "";
            foreach (var symbl in WebRequest.Item1)
            {
                WebList newWeb = new();
                newSymbl = symbl;
                tempSymbl = semiSymbl;
                if (newSymbl == ';')
                {
                    flagSearchStop = true;
                }
                else if (oldSymbl == WebRequest.Item2 && semiSymbl == WebRequest.Item3 && newSymbl == WebRequest.Item4)
                {
                    flagSearchStart = true;
                    flagSearchStop = false;
                }
                if (flagSearchStart && !flagSearchStop)
                {
                    temp += newSymbl;
                }
                else if (flagSearchStart && flagSearchStop)
                {
                    newTemp = temp.Remove(temp.Length - (4 + WebRequest.Item5));
                    newWeb.Web = newTemp;
                    web.Add(newWeb);
                    newTemp = "";
                    temp = "";
                    flagSearchStart = false;
                    flagSearchStop = false;
                }
                semiSymbl = symbl;
                oldSymbl = tempSymbl;
            }
        }

        private void YandexWeb_Click(object sender, RoutedEventArgs e)
        {
            GoogleWeb.IsChecked = false;
            YandexWeb.IsChecked = true;
            Menu.IsSubmenuOpen = false;
        }

        private void GoogleWeb_Click(object sender, RoutedEventArgs e)
        {
            GoogleWeb.IsChecked = true;
            YandexWeb.IsChecked = false;
            Menu.IsSubmenuOpen = false;
        }

        private void Vizor1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WebList selectWeb = (WebList)Vizor1.SelectedItem;
            if(selectWeb is not null)
            {
                Uri uri = new Uri(selectWeb.Web, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                {
                    MessageBox.Show("Неправильно задан адрес! Укажите адрес в формате 'http://www.microsoft.com'");
                    return;
                }
                BrowserG.Navigate(uri);
            }
        }
    }
}
