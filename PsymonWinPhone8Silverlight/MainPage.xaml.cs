using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Diagnostics;

using Psymon.Business;
using System.Threading;
using Microsoft.Phone.Shell;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using PsymonWinPhone8Silverlight;

namespace Psymon
{
    public partial class MainPage : PhoneApplicationPage
    {

        private Game _game = null;
        private Timer _delayTimer = null;
        private Timer _progressTimer = null;
        private int _progressIncrement = 0;
        private long _highScore = 0;

        private readonly SoundEffect _clickSound = SoundEffect.FromStream(TitleContainer.OpenStream("Content/Audio/UI_Clicks01.wav"));
        private readonly SoundEffect _clapSound = SoundEffect.FromStream(TitleContainer.OpenStream("Content/Audio/applause-light-02.wav"));
        private readonly SoundEffect _groanSound = SoundEffect.FromStream(TitleContainer.OpenStream("Content/Audio/crowd-groan.wav"));

        public MainPage()
        {
            InitializeComponent();

            // Microsoft.Advertising.Mobile.UI.AdControl.TestMode = false;
        }

        private void InitializeUI()
        {
            this.MoveTimeProgress.Minimum = 0;
            this.MoveTimeProgress.Maximum = 9;

            ResetProgress();
            Output("Hit Play to begin!");
        }

        private void ResetProgress()
        {
            TakeUIAction(() => { this.MoveTimeProgress.Value = MoveTimeProgress.Maximum; });
        }

        private void InitializeGame()
        {
            var root = App.Current.RootVisual as PhoneApplicationFrame;

            Game saveGame = null;

            if (root != null)
            {
                saveGame = root.DataContext as Game;
            }

            if (saveGame != null)
            {
                saveGame.Dispatcher = this.Dispatcher;
                _game = saveGame;
            }
            else
            {
                _game = new Game(this.Dispatcher);
            }

            Debug.Assert(_game != null);

            _game.GameStartTurn += new EventHandler<EventArgs>(GameStartTurn);
            _game.GameWon += new EventHandler<EventArgs>(GameWon);
            _game.GameLost += new EventHandler<EventArgs>(GameLost);

            if (root != null)
            {
                root.DataContext = _game;
            }

            LayoutRoot.DataContext = _game;
        }

        private void InitializeTimers()
        {
            var stamp = DateTime.Now;

            _delayTimer = new Timer(TimeIsUp, stamp, -1, -1);
            _progressTimer = new Timer(ChangeProgress, stamp, -1, -1);

            _game.TimeStamp = stamp;
        }


        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeGame();
            InitializeTimers();
            InitializeUI();
            SetInfo();
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // DestoryTimers();
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //
            // TODO: Persist to Isolated Storage here
            //
        }

        private void DestoryTimers()
        {
            if (_delayTimer != null)
            {
                _delayTimer.Change(-1, -1);
                ((IDisposable)_delayTimer).Dispose();
                _delayTimer = null;
            }

            if (_progressTimer != null)
            {
                _progressTimer.Change(-1, -1);
                ((IDisposable)_progressTimer).Dispose();
                _progressTimer = null;
            }
        }

        private void AdvanceGame()
        {
            _game.Advance();
        }

        private void TimeIsUp(object state)
        {
            if (_game.TimeStamp != ((DateTime)state))
            {
                return;
            }

            GameLost(null, null);
        }

        private void ChangeProgress(object state)
        {
            TakeUIAction(() => { this.MoveTimeProgress.Value -= 1; });
        }

        private void SetProgress(int timeAvailable)
        {
            _progressIncrement = timeAvailable / 10;
            TakeUIAction(() => { this.MoveTimeProgress.Value = MoveTimeProgress.Maximum; });
        }

        private void GameWon(object sender, EventArgs e)
        {
            GameOver();
            TakeUIAction(() => { Output("You Won! Hit Play for the next level!"); });
            TakeUIAction(() => { PlayApplause(); });
        }

        private void GameLost(object sender, EventArgs e)
        {
            GameOver();
            _game.Reset(_game.GameDifficulty);
            TakeUIAction(() => { Output("Oops! Try again!"); });
            TakeUIAction(() => { PlayGroan(); });
        }

        private void GameOver()
        {
            TakeUIAction(() => { StopTimer(); });
            _game.SetAllColorPropertyState(false);
            UpdateHighScore();
        }

        private void UpdateHighScore()
        {
            _highScore = (_game.Score > _game.HighScore) ? _game.Score : _game.HighScore;
            _game.HighScore = _highScore;

            TakeUIAction(() => { HighScoreLabel.Text = _highScore.ToString(); });
        }

        private void GameStartTurn(object sender, EventArgs e)
        {
            var game = sender as Game;
            var delay = game.DelayTime;

            TakeUIAction(() => { SetHit(true); });
            TakeUIAction(() => { SetProgress(delay); });
            TakeUIAction(() => { StartTimer(delay); });
        }

        private void StartTimer(int delay)
        {
            _delayTimer.Change(delay, _game.DelayTime);
            _progressTimer.Change(_progressIncrement, _progressIncrement);   
        }

        private void StopTimer()
        {
            _delayTimer.Change(-1, -1);
            _progressTimer.Change(-1, -1);
        }

        private void PlayPattern()
        {
            Debug.Assert(_game != null && _game.Pattern != null);

            if (_game == null || _game.Pattern == null)
            {
                //
                // TODO: Log error
                //

                return;
            }

            var g = new Thread(() => { _game.PlayPattern(); });

            g.Start();

        }

        private void PlayApplause()
        {
            FrameworkDispatcher.Update();
            _clapSound.Play();
        }

        private void PlayClick()
        {
            FrameworkDispatcher.Update();
            _clickSound.Play();
        }

        private void PlayGroan()
        {
            FrameworkDispatcher.Update();
            _groanSound.Play();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetHit(false);
            PlayClick();

            var btn = sender as Button;

            if (btn == null || !_game.Started)
            {
                //
                // TODO: Log exception.
                //
                return;
            }

            StopTimer();

            int btnId = Convert.ToInt32(btn.Tag);

            _game.Move(btnId);

            SetProgress(_game.DelayTime);
            SetScore();

            StartTimer(_game.DelayTime);
            SetHit(true);
        }

        private void Output(string message)
        {
            TakeUIAction(() => { OutputLabel.Text = message; });
        }

        private void SetInfo()
        {
            var msg = _game.GameDifficulty.ToString();
            var fore = (_game.GameDifficulty == Game.Difficulty.Hard) ? Colors.Red : System.Windows.Media.Color.FromArgb(0xFF, 0x7F, 0xFF, 0x00);
            var brsh = new SolidColorBrush(fore);


            TakeUIAction(() =>
            {
                InfoLabel.Foreground = brsh;
                InfoLabel.Text = msg.ToUpperInvariant();

                ScoreLabel.Text = _game.Score.ToString();
                HighScoreLabel.Text = _game.HighScore.ToString();
            });
        }

        private void SetScore()
        {
            TakeUIAction(() =>
            {
                ScoreLabel.Text = _game.Score.ToString();
            });
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            _game.Reset();
            Output("Game reset!");
            SetInfo();
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (_game != null)
            {

                if (_game.Started && !_game.InTurn)
                {
                    return;
                }

                if (_game.Started && _game.GameScore != Game.Disposition.Won)
                {
                    return;
                }
            }

            ResetProgress();
            SetInfo();

            AdvanceGame();

            Output("Start playing ...");

            SetHit(false);
            PlayPattern();
        }

        private void ReplayButton_Click(object sender, EventArgs e)
        {
            if (!_game.Started || !_game.InTurn)
            {
                return;
            }

            StopTimer();
            PlayPattern();
        }

        private void SetHit(bool canHit)
        {
            var btns = LayoutRoot.Children.Where(c => c is Button);

            foreach (Button b in btns)
            {
                b.IsHitTestVisible = canHit;
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            var menu = sender as ApplicationBarMenuItem;

            if (menu == null)
            {
                return;
            }

            SetGameLevel(menu.Text);
        }

        private void SetGameLevel(string difficulty)
        {
            if (String.IsNullOrEmpty(difficulty))
            {
                return;
            }

            if (_game == null || _game.InTurn)
            {
                return;
            }

            _game.GameDifficulty = (difficulty.Equals("hard", StringComparison.CurrentCultureIgnoreCase)) ? Game.Difficulty.Hard : Game.Difficulty.Easy;
            SetInfo();
        }

        private void ApplicationBarMenuHelp_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/HelpPage.xaml", UriKind.Relative));
        }

        private void TakeUIAction(Action act)
        {
            this.Dispatcher.BeginInvoke(act);
        }

    }
}
