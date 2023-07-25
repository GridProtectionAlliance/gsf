using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WavSubscriptionDemo
{
    /// Changes and improvements made =>
    /// 2. Popup improvements
    /// 3. Added SelectedDate DependencyProperty
    ///
    /// Things that will be needed for this control to work properly (and look good :) ) =>
    /// 1. A bitmap image 32x32 added as an embedded resource
    ///
    /// Licensing =>
    /// The Code Project Open License (CPOL)
    /// http://www.codeproject.com/info/cpol10.aspx

    public partial class DateTimePicker : UserControl
    {
        public const string DateTimeFormat = "MM/dd/yyyy hh:mm tt";

        #region "Properties"

        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        #endregion

        #region "DependencyProperties"

        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register("SelectedDate",
            typeof(DateTime), typeof(DateTimePicker), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        public DateTimePicker()
        {
            InitializeComponent();
            CalDisplay.SelectedDatesChanged += CalDisplay_SelectedDatesChanged;
            CalDisplay.SelectedDate = DateTime.Now;

            BitmapSource ConvertGDI_To_WPF(Bitmap bm)
            {
                IntPtr hBm = bm.GetHbitmap();
                BitmapSource bms = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBm, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                bms.Freeze();
                return bms;
            }

            Bitmap bitmap1 = Properties.Resources.DateTimePicker;
            bitmap1.MakeTransparent(System.Drawing.Color.White);
            CalIco.Source = ConvertGDI_To_WPF(bitmap1);
        }

        #region "EventHandlers"

        private void CalDisplay_SelectedDatesChanged(object sender, EventArgs e)
        {
            string hours = (Hours?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "0";
            string minutes = (Min?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "0";
            string ampm = (AMPM?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "AM";

            DateTime dateTime = DateTime.Parse($"{hours}:{minutes} {ampm}");
            TimeSpan timeSpan = TimeSpan.Parse(dateTime.ToString("HH:mm"));
            
            DateTime date = CalDisplay.SelectedDate.GetValueOrDefault().Date + timeSpan;
            DateDisplay.Text = date.ToString(DateTimeFormat);
            SelectedDate = date;
        }

        private void SaveTime_Click(object sender, RoutedEventArgs e)
        {
            CalDisplay_SelectedDatesChanged(SaveTime, EventArgs.Empty);
            PopUpCalendarButton.IsChecked = false;
        }

        private void Time_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalDisplay_SelectedDatesChanged(sender, e);
        }

        private void CalDisplay_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // that it's not necessary to click twice after opening the calendar  https://stackoverflow.com/q/6024372
            if (Mouse.Captured is CalendarItem)
                Mouse.Capture(null);
        }

        #endregion
    }
    
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value!;
        }
    }
}