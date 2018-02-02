using OxfordSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Countdown
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int Score = 0;
        public int Limit = 0;
        public SoundPlayer Player = new SoundPlayer();
        public int TimeLeft = 30;
        public DispatcherTimer Timer = new DispatcherTimer();
        public Random Rand = new Random();
        public List<char> PickedChars = new List<char>();
        public char[] Vowels = new char[] { 'a', 'e', 'i', 'o', 'u' };
        public char[] Const = new char[] { 'q', 'w', 'r', 't', 'y', 'p', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm' };
        public const string BaseUrl = "https://od-api.oxforddictionaries.com/api/v1";
        public HttpClient Http = new HttpClient();
        public OxfordSharpClient OxSharpClient { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            var data = (File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt"))).Split(' ');
            OxSharpClient = new OxfordSharpClient(data[0], data[1]);
            Title = "Score: 0";
            ProgressBar.Maximum = 30;
            ProgressBar.Minimum = 0;
            ProgressBar.Value = 30;
            Timer.Interval = TimeSpan.FromSeconds(1);
            Timer.Tick += _timer_Tick;
        }

        private async void _timer_Tick(object sender, EventArgs e)
        {
            ProgressBar.Value -= 1;
            TimeLeft -= 1;
            LabelTimer.Content = TimeLeft;
            if (TimeLeft == 0)
            {
                Player.Stop();
                Timer.Stop();
                InputBox.IsEnabled = false;
                VowelButton.IsEnabled = false;
                ConstButton.IsEnabled = false;
                ConfirmButton.IsEnabled = false;
                var text = InputBox.Text.ToLower();
                var pickedCharsCopy = PickedChars;
                bool IsValid = true;
                foreach (var character in text.ToCharArray())
                {
                    if (pickedCharsCopy.Contains(character))
                        pickedCharsCopy.Remove(character);
                    else
                    {
                        IsValid = false;
                        break;
                    }
                }
                if (!IsValid)
                {
                    MessageBox.Show("Characters in word do not match those picked");
                    Reset();
                    return;
                }
                var Result = await OxSharpClient.GetResultsAsync(text);
                if (Result == null) MessageBox.Show("Invalid word");
                else
                {
                    Score += text.Length;
                    Title = $"Score: {Score}";
                }
                Reset();
            }
        }

        private void VowelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Limit == 20) return;
            Limit++;
            var vchar = Vowels[Rand.Next(0, Vowels.Length)];
            PickedChars.Add(vchar);
            UpdateCountBox();
        }

        private void ConstButton_Click(object sender, RoutedEventArgs e)
        {
            if (Limit == 20) return;
            Limit++;
            var cchar = Const[Rand.Next(0, Const.Length)];
            PickedChars.Add(cchar);
            UpdateCountBox();
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            InputBox.IsEnabled = true;
            ConfirmButton.IsEnabled = false;
            ConstButton.IsEnabled = false;
            VowelButton.IsEnabled = false;
            Timer.Start();
            await Task.Delay(1000);
            await Task.Run(() =>
            {
                Player.SoundLocation = Path.Combine(Directory.GetCurrentDirectory(), "Countdown Theme.wav");
                Player.PlaySync();
            });
        }
        
        private void UpdateCountBox() => CountBox.Text = string.Concat(PickedChars.Select(x => x));

        private void Reset()
        {

            InputBox.IsEnabled = true;
            VowelButton.IsEnabled = true;
            ConstButton.IsEnabled = true;
            ConfirmButton.IsEnabled = true;
            TimeLeft = 30;
            ProgressBar.Value = 30;
            Limit = 0;
            LabelTimer.Content = "30";
            CountBox.Clear();
            InputBox.Clear();
            PickedChars.Clear();
        }

    }
}
