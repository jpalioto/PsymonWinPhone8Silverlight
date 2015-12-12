using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;

using System.Linq;

using Phone.Utilities;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Psymon.Business
{
    [DataContract(Name="Game", Namespace="http://www.jpalioto.com/")]
    public class Game : INotifyPropertyChanged
    {
        private int _move = 0;
        private Pattern _pattern = null;

        Dispatcher _disp = null;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EventArgs>     GameStartTurn;
        public event EventHandler<EventArgs>     GameWon;
        public event EventHandler<EventArgs>     GameLost;

        public Game(Dispatcher disp)
        {
            HighScore = 0;
            Reset();
            _disp = disp;
        }

        public void PlayPattern()
        {
            SetAllColorPropertyState(false);
            Started = true;

            Thread.Sleep(250);

            var props = this.GetType().GetProperties().Where( p => p.Name.Contains("ButtonState") ).OrderBy( p => p.Name).ToArray();

            foreach (int i in Pattern)
            {
                var p = props[i];

                props[i].SetValue(this, true, null);
                NotifyChange(p.Name);

                Thread.Sleep(Pause);

                props[i].SetValue(this, false, null);
                NotifyChange(p.Name);

                Thread.Sleep(Pause);
            }

            Thread.Sleep(250);
            SetAllColorPropertyState(true);
            OnGameStart();
        }

        public void Move(int moveVal)
        {
            Debug.Assert(_pattern.Count == Level);

            bool correct = (_pattern[_move] == moveVal);

            if (!correct)
            {
                GameOver();
                OnGameLost();
            }
            else if (correct && _move == _pattern.Count - 1)
            {
                AdjustScore(1000);
                GameOver();
                OnGameWon();
            }
            else
            {
                AdjustScore(100);
                _move++;
            }

        }

        private void AdjustScore(int baseScore)
        {
            var mod = (GameDifficulty == Difficulty.Hard) ? 2 : 1;

            Score += baseScore * Level * mod;
        }


        public void Advance()
        {
            var timeDialtionFactor = 0.90;
            var minPause = 500;
            var numToGenerate = 1;

            Level++;

            if (GameDifficulty == Difficulty.Hard)
            {
                MaxVal = 6;
                timeDialtionFactor = 0.75;
                minPause = 150;
            }

            if (Level % 2 == 0)
            {
                Pause = Convert.ToInt32( Pause * timeDialtionFactor );

                if (Pause < minPause)
                    Pause = minPause;
            }

            if (Level > 5)
            {
                MaxVal = (GameDifficulty == Difficulty.Hard) ? 6 : 4;
                DelayTime = (GameDifficulty == Difficulty.Hard ) ? 1500: 3000;
            }

            if (Level > 10)
            {
                MaxVal = 6;
                DelayTime = (GameDifficulty == Difficulty.Hard) ? 1250 : 2500;
            }

            if (Level > 15)
            {
                MaxVal = 6;
                DelayTime = (GameDifficulty == Difficulty.Hard) ? 1000 : 2000;
            }

            if (Level > 20)
            {
                MaxVal = 6;
                DelayTime = (GameDifficulty == Difficulty.Hard) ? 750 : 1500;
            }

            if (Level > 25)
            {
                numToGenerate = 2;
            }

            GenerateNewPattern(numToGenerate);
        }

        public void Reset()
        {
            Reset(Difficulty.Easy);
        }

        public void Reset( Difficulty gameDifficulty )
        {
            if (_pattern != null)
            {
                _pattern.Clear();
            }

            _move = 0;
            Level = 1;

            InTurn = false;
            Started = false;
            GameScore = Disposition.None;
            Pause = 1000;
            Score = 0;
            
            DelayTime = (gameDifficulty == Difficulty.Hard) ? 2000 : 4000;
            MaxVal = (gameDifficulty == Difficulty.Hard) ? 6 : 4;
            GameDifficulty = gameDifficulty;
            
            GenerateNewPattern();
        }

        public void SetAllColorPropertyState(bool enabled)
        {
            var props = this.GetType().GetProperties().Where(p => p.Name.Contains("ButtonState")).OrderBy(p => p.Name).ToArray();

            foreach (PropertyInfo p in props)
            {
                p.SetValue(this, enabled, null);
                NotifyChange(p.Name);
            }
        }

        public void NotifyChange(string name)
        {
            _disp.BeginInvoke(() => { PropertyChanged(this, new PropertyChangedEventArgs(name)); });
        }

        
        public void Start()
        {
            Started = true;
        }

        public void Stop()
        {
            Started = false;
        }

        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public bool Started { get; set; }

        [DataMember]
        public int DelayTime { get; set; }

        [DataMember]
        public bool InTurn { get; set; }

        [DataMember]
        public int Pause { get; set; }

        [DataMember]
        public long Score { get; set; }

        [DataMember]
        public Difficulty GameDifficulty { get; set; }

        [DataMember]
        public long HighScore { get; set; }

        [DataMember]
        public Pattern Pattern { get { return _pattern; } set { _pattern = value; } }

        [DataMember]
        public Disposition GameScore
        {
            get;
            set;
        }

        [DataMember]
        public bool RedButtonState
        {
            get;
            set;
        }

        [DataMember]
        public bool GreenButtonState
        {
            get;
            set;
        }

        [DataMember]
        public bool BlueButtonState
        {
            get;
            set;
        }

        [DataMember]
        public bool CyanButtonState
        {
            get;
            set;
        }

        [DataMember]
        public bool MagentaButtonState
        {
            get;
            set;
        }

        [DataMember]
        public bool YellowButtonState
        {
            get;
            set;
        }

        public Dispatcher Dispatcher { get { return _disp; } set { _disp = value; } }

        protected virtual void OnGameStart()
        {
            InTurn = true;
            GameStartTurn.RaiseEvent(this, new EventArgs());
        }

        protected virtual void OnGameWon()
        {
            Started = false;
            InTurn = false;
            GameScore = Disposition.Won;
            GameWon.RaiseEvent(this, new EventArgs());
        }

        protected virtual void OnGameLost()
        {
            Started = false;
            InTurn = false;
            GameScore = Disposition.Lost;
            GameLost.RaiseEvent(this, new EventArgs());
        }

        private void GenerateNewPattern()
        {
            GenerateNewPattern(1);
        }
        
        private void GenerateNewPattern(int numberToAdd)
        {
            _pattern = PatternManager.GeneratePattern(_pattern, numberToAdd, MaxVal);
        }

        private void GameOver()
        {
            _move = 0;
            GameScore = Disposition.None;
        }

        private int MaxVal { get; set; }

        public enum Disposition
        {
            Lost = -1,
            None = 0,
            Won  = 1
        }

        [Flags]
        public enum Difficulty
        {
            Easy,
            Hard
        }
    }
}
