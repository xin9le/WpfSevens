using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfSevens
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum TimerDisplayModeEnum
        {
            TableAndPlayerCards = 0,
            PlayerCard = 1,
            Result = 2,
            MessageShow = 3,
        }

        private Image[,] _GameTable = new Image[13, 4];
        private Border[,] _GameBorderTable = new Border[13, 4];
        private Table _Table = new Table();
        private List<IPlayer> _PlayerList = new List<IPlayer>();
        private Dictionary<IPlayer, BitmapImage> _PlayerBitmapImage = new Dictionary<IPlayer, BitmapImage>();
        private Dictionary<IPlayer, List<Image>> _PlayerPutImage = new Dictionary<IPlayer, List<Image>>();
        private Dictionary<IPlayer, Image> _PlayerImage = new Dictionary<IPlayer, Image>();
        private Dictionary<Card, Image> _CardImage = new Dictionary<Card, Image>();
        private TimerDisplayModeEnum _TimerDisplayMode = TimerDisplayModeEnum.TableAndPlayerCards;
        private string _TimerMessage = "";

        private DispatcherTimer _DispatcherTimer;
        private int _WaitTimer = 0;

        public MainWindow()
        {
            InitializeComponent();

            for (var x = 0; x < 13; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    var border =  new Border();
                    border.Background = Brushes.Red;
                    border.Margin = new Thickness(10);
                    border.Visibility = System.Windows.Visibility.Hidden;
                    Grid.SetRow(border, y);
                    Grid.SetColumn(border, x);
                    _GameBorderTable[x, y] = border;

                    griTable.Children.Add(border);

                    var imageNumber = 66;

                    var image = new System.Windows.Controls.Image();
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(string.Format("pack://application:,,,/Assets/IG{0:000}.png", imageNumber + x + y * 13));
                    bitmapImage.EndInit();

                    image.Source = bitmapImage;
                    image.Margin = new Thickness(3);
                    image.Visibility = System.Windows.Visibility.Hidden;
                    Grid.SetRow(image, y);
                    Grid.SetColumn(image, x);

                    _GameTable[x, y] = image;

                    griTable.Children.Add(image);
                }

            }

        }

        private void btnShuffleCard_Click(object sender, RoutedEventArgs e)
        {
            btnShuffleCard.IsEnabled = false;
            btnCheckCard.IsEnabled = true;

            _PlayerList.Add(new PlayerSuzuki());
            _PlayerList.Add(new PlayerKojima());
            _PlayerList.Add(new PlayerIshino());

            _Table.GameStart(_PlayerList);

            for (var x = 0; x < 13; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    _GameTable[x, y].Visibility = System.Windows.Visibility.Visible;
                }
            }

        }

        private void btnCheckCard_Click(object sender, RoutedEventArgs e)
        {
            btnCheckCard.IsEnabled = false;
            btnStart.IsEnabled = true;

            foreach (var player in _PlayerList)
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(string.Format("pack://application:,,,/Assets/{0}", player.GetPalyerImageName()));
                bitmapImage.EndInit();

                _PlayerBitmapImage.Add(player, bitmapImage);

                var imageList = new List<Image>();    
                foreach (var card in _Table.GetPlayerCards(player))
                {
                    var image = new System.Windows.Controls.Image();
                    image.Source = bitmapImage;
                    image.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    image.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    image.Margin = new Thickness(20);
                    Grid.SetRow(image, (int)card.CardType);
                    Grid.SetColumn(image, card.CardNumber - 1);

                    griTable.Children.Add(image);
                    imageList.Add(image);

                    //カードを積む
                    var playerCardImage = new System.Windows.Controls.Image();
                    playerCardImage.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    playerCardImage.Source = _GameTable[card.CardNumber - 1, (int)card.CardType].Source;
                    playerCardImage.Visibility = System.Windows.Visibility.Hidden;
                    Grid.SetColumn(playerCardImage, 1);

                    griPlayer.Children.Add(playerCardImage);
                    _CardImage.Add(card, playerCardImage);
                }
                _PlayerPutImage.Add(player, imageList);

                var playerImage = new System.Windows.Controls.Image();
                playerImage.Source = bitmapImage;
                playerImage.Visibility = System.Windows.Visibility.Hidden;

                griPlayer.Children.Add(playerImage);
                _PlayerImage.Add(player, playerImage);
            }

        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;

            for (var x = 0; x < 13; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    griTable.Children.Remove(_GameTable[x, y]);
                    griTable.Children.Add(_GameTable[x, y]);
                }
            }
            _Table.CheckStartPlayer();
            ShowPlayer();
            ShowTable();

            _DispatcherTimer = new DispatcherTimer();
            _DispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _DispatcherTimer.Tick += dispatcherTimer_Tick;
            _DispatcherTimer.Start();
        }

        void ShowPlayer()
        {
            var getPlayer = _Table.GetPlayer();
            var cards = _Table.GetPlayerCards(getPlayer);
            var possibleCards = Table.GetPutPossibleCards(cards, _Table.GetPutCards());
            foreach (var player in _PlayerList)
            {
                _PlayerImage[player].Visibility = getPlayer==player ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            }
            foreach (var cardImage in _CardImage)
            {
                cardImage.Value.Visibility = System.Windows.Visibility.Hidden;   
            }

            var width = griPlayer.ActualWidth / 9 * 6;
            var playerCards = cards.Select((card, i) => new { card = card, index = i });
            foreach (var card in playerCards)
            {
                _CardImage[card.card].Margin = new Thickness(card.index * (width / playerCards.Count()), 0, 0, 0);
                _CardImage[card.card].Visibility = System.Windows.Visibility.Visible;
            }
            //出せるカード場所を表示
            for (var x = 0; x < 13; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    _GameBorderTable[x, y].Visibility = possibleCards.Any(row => row.CardNumber == (x + 1) && (int)row.CardType == y) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                }
            }

        }

        void ShowTable()
        {
            var putCards = _Table.GetPutCards();

            foreach (var card in _Table.GetAllCards())
            {
                _GameTable[card.CardNumber - 1, (int)card.CardType].Visibility = (putCards.Where(row => row == card).Any()) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            }

        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_WaitTimer > 0)
            {
                _WaitTimer--;
                return;
            }

            switch (_TimerDisplayMode)
            {
                case TimerDisplayModeEnum.TableAndPlayerCards:
                    lblMessage.Content = "";
                    ShowPlayer();
                    ShowTable();

                    _TimerDisplayMode = TimerDisplayModeEnum.PlayerCard;
                    _WaitTimer = 5;
                    
                    break;
                case TimerDisplayModeEnum.PlayerCard:
                    if (!_Table.IsGameEnd)
                    {
                        var card = _Table.Trun(ref _TimerMessage);

                        if (card != null)
                        {
                            _WaitTimer = 5;
                            var storyboard = new Storyboard();
                            
                            {
                                var doubleAnimation = new DoubleAnimation();

                                Storyboard.SetTarget(doubleAnimation, _CardImage[card]);
                                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
                                doubleAnimation.From = 1;
                                doubleAnimation.To = 0;
                                doubleAnimation.Duration = new TimeSpan(0, 0, 0, 0, (_WaitTimer - 1) * 100);
                                storyboard.Children.Add(doubleAnimation);
                            }
                            {
                                var doubleAnimation = new DoubleAnimation();
                                var cardImage = _GameTable[card.CardNumber - 1, (int)card.CardType];

                                cardImage.Opacity = 0;
                                cardImage.Visibility = System.Windows.Visibility.Visible;

                                Storyboard.SetTarget(doubleAnimation, cardImage);
                                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
                                doubleAnimation.From = 0;
                                doubleAnimation.To = 1;
                                doubleAnimation.Duration = new TimeSpan(0, 0, 0, 0, (_WaitTimer - 1) * 100);
                                storyboard.Children.Add(doubleAnimation);
                            }

                            // アニメーションを開始します
                            storyboard.Begin();
                        }
                    }

                    if (!string.IsNullOrEmpty(_TimerMessage))
                    {
                        _TimerDisplayMode = TimerDisplayModeEnum.MessageShow;
                    }
                    else
                    {
                        _TimerDisplayMode = TimerDisplayModeEnum.TableAndPlayerCards;
                    }
                    break;
                case TimerDisplayModeEnum.MessageShow:
                    _TimerDisplayMode = TimerDisplayModeEnum.TableAndPlayerCards;
                    lblMessage.Content = _TimerMessage;
                    _WaitTimer = 30;
                    if (_Table.IsGameEnd)
                    {
                        _DispatcherTimer.Stop();
                    }
                    break;
            }
        }
    }
}
