using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using PI.Annotations;

namespace PI.Core
{
    public class Scene: UserControl, INotifyPropertyChanged
    {
        #region private

        private bool _isRunning, _isReady;
        private Box _smallBox, _bigBox;
        private int _count = 0;
        private double _gap = 40.0;

        #endregion

        #region properties

        public bool IsReady
        {
            get => _isReady;
            set
            {
                if (_isReady != value)
                {
                    _isReady = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public int CalculationResult
        {
            get { return (int)GetValue(CalculationResultProperty); }
            set { SetValue(CalculationResultProperty, value); }
        }

        public ICommand StartCommand
        {
            get { return (ICommand)GetValue(StartCommandProperty); }
            set { SetValue(StartCommandProperty, value); }
        }

        public ICommand ResetCommand
        {
            get { return (ICommand)GetValue(ResetCommandProperty); }
            set { SetValue(ResetCommandProperty, value); }
        }

        public int Digits
        {
            get { return (int)GetValue(DigitsProperty); }
            set { SetValue(DigitsProperty, value); }
        }

        public int Times
        {
            get { return (int)GetValue(TimesProperty); }
            set { SetValue(TimesProperty, value); }
        }

        public static readonly DependencyProperty ResetCommandProperty = DependencyProperty.Register("ResetCommand", typeof(ICommand), typeof(Scene), new PropertyMetadata(null));
        public static readonly DependencyProperty TimesProperty = DependencyProperty.Register("Times", typeof(int), typeof(Scene), new PropertyMetadata(50_000));
        public static readonly DependencyProperty DigitsProperty = DependencyProperty.Register("Digits", typeof(int), typeof(Scene), new PropertyMetadata(7));
        public static readonly DependencyProperty StartCommandProperty = DependencyProperty.Register("StartCommand", typeof(ICommand), typeof(Scene), new PropertyMetadata(null));
        public static readonly DependencyProperty CalculationResultProperty = DependencyProperty.Register("CalculationResult", typeof(int), typeof(Scene), new PropertyMetadata(0));

        #endregion

        #region override

        protected override void OnRender(DrawingContext dc)
        {
            var width = this.ActualWidth;
            var height = this.ActualHeight;
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)), 1);
            dc.DrawRectangle(Brushes.WhiteSmoke, pen, new Rect(new Size(width, height)));
            for (var x = 0.0; x < width; x += _gap)
                dc.DrawLine(pen, new Point(x, 0), new Point(x, height));
            for (var y = 0.0; y < height; y += _gap)
                dc.DrawLine(pen, new Point(0, height - y), new Point(width, height - y));
            _smallBox?.Draw(dc);
            _bigBox?.Draw(dc);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Init();
        }

        #endregion

        #region methods

        private void Reset()
        {
            IsRunning = false;
            IsReady = true;
            CalculationResult = _count = 0;
            _smallBox?.Reset(100, 0);
            _bigBox?.Reset(300);
            InvalidateVisual();
        }

        private void Run()
        {
            IsRunning = true;
            IsReady = false;
            Task.Run(() =>
            {
                while (IsRunning)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _bigBox.M = Math.Pow(100, Digits-1);
                        for (var i = 0; i < Times; i++)
                        {
                            if (_bigBox.Move(Times) == false)
                            {
                                IsRunning = false;
                            }
                            _smallBox.Move(Times);

                            if (IsCollide())
                            {
                                var v1 = _bigBox.Bounce(_smallBox);
                                var v2 = _smallBox.Bounce(_bigBox);

                                _bigBox.V = v1;
                                _smallBox.V = v2;
                                _count++;
                                CalculationResult = _count;
                            }

                            if (_smallBox.HitWall())
                                _count++;
                            _bigBox.HitWall();
                        }

                        InvalidateVisual();
                    });
                    Thread.Sleep(5);
                }
            });
        }

        private bool IsCollide()
        {
            if (_smallBox.X + _smallBox.Width >= _bigBox.X)
                return true;
            return false;
        }

        private void Init()
        {
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));

            _smallBox =new Box(100, 20, this.ActualWidth, this.ActualHeight, Brushes.DeepPink, 0);
            _bigBox = new Box(300, 150, this.ActualWidth, this.ActualHeight, Brushes.LightSeaGreen);

            InvalidateVisual();
        }

        #endregion

        #region commands

        private bool CanExecuteStartCommand(object arg)
        {
            return IsRunning == false && IsReady == true;
        }

        private void ExecutedResetCommand(object obj)
        {
            Reset();
        }

        private bool CanExecutedResetCommand(object arg)
        {
            return IsRunning == false && IsReady == false;
        }

        private void ExecuteStartCommand(object obj)
        {
           Run();
        }

        #endregion

        #region event

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public Scene()
        {
            SnapsToDevicePixels = true;
            StartCommand = new RelayCommand(ExecuteStartCommand, CanExecuteStartCommand);
            ResetCommand=new RelayCommand(ExecutedResetCommand, CanExecutedResetCommand);
            Reset();

            Times = 100_000;
            Digits = 6;
        }
    }
}
