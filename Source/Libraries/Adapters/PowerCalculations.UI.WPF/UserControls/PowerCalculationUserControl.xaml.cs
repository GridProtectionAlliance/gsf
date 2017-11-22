using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GSF.TimeSeries.UI.UserControls;
using GSF.TimeSeries.UI.ViewModels;
using PowerCalculations.PowerMultiCalculator;
using PowerCalculations.UI.WPF.ViewModels;

namespace PowerCalculations.UI.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for PowerCalculationUserControl.xaml
    /// </summary>
    public partial class PowerCalculationUserControl
	{
        private PowerCalculationViewModel m_dataContext;

		public PowerCalculationUserControl()
		{
			InitializeComponent();
            m_dataContext = new PowerCalculationViewModel(14);
            m_dataContext.PropertyChanged += ViewModel_PropertyChanged;
            DataContext = m_dataContext;

            try
            {
                // Validate that data operation and adapter instance exist within database
                PowerCalculationConfigurationValidation.ValidateDatabaseDefinitions();
            }
            catch
            {
                // This should never cause unhanded exception
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(m_dataContext.CurrentItem) && m_dataContext.IsNewRecord)
                DataGridList.SelectedIndex = -1;
        }

        private void PhasorSelector_Loaded(object sender, RoutedEventArgs e)
        {
            PhasorSelectionUserControl phasorSelector;
            PhasorSelectionViewModel dataContext;

            phasorSelector = e.OriginalSource as PhasorSelectionUserControl;

            if ((object)phasorSelector == null)
                return;

            dataContext = GetDataContext(phasorSelector);
            dataContext.Clear();

            if (phasorSelector == VoltagePhasorSelector)
                dataContext.PropertyChanged += VoltagePhasorChanged;
            else if (phasorSelector == CurrentPhasorSelector)
                dataContext.PropertyChanged += CurrentPhasorChanged;
        }

        private void MeasurementPager_Loaded(object sender, RoutedEventArgs e)
        {
            MeasurementPagerUserControl measurementPager;
            Measurements dataContext;

            measurementPager = e.OriginalSource as MeasurementPagerUserControl;

            if ((object)measurementPager == null)
                return;

            dataContext = GetDataContext(measurementPager);
            dataContext.Clear();

            if (measurementPager == ActiveMeasurementPager)
                dataContext.PropertyChanged += ActiveMeasurementChanged;
            else if (measurementPager == ReactiveMeasurementPager)
                dataContext.PropertyChanged += ReactiveMeasurementChanged;
            else if (measurementPager == ApparentMeasurementPager)
                dataContext.PropertyChanged += ApparentMeasurementChanged;
        }

        private void VoltagePhasorChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentItem")
                ((PowerCalculationViewModel)DataContext).CurrentItem.VoltagePhasor = GetDataContext(VoltagePhasorSelector).CurrentItem;
        }

        private void CurrentPhasorChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == "CurrentItem")
				((PowerCalculationViewModel)DataContext).CurrentItem.CurrentPhasor = GetDataContext(CurrentPhasorSelector).CurrentItem;
        }

        private void ActiveMeasurementChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentItem")
                ((PowerCalculationViewModel)DataContext).CurrentItem.ActivePowerOutputMeasurement = ActiveMeasurementPager.CurrentItem;
        }

        private void ReactiveMeasurementChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentItem")
                ((PowerCalculationViewModel)DataContext).CurrentItem.ReactivePowerOutputMeasurement = ReactiveMeasurementPager.CurrentItem;
        }

        private void ApparentMeasurementChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentItem")
                ((PowerCalculationViewModel)DataContext).CurrentItem.ApparentPowerOutputMeasurement = ApparentMeasurementPager.CurrentItem;
        }

        private PhasorSelectionViewModel GetDataContext(PhasorSelectionUserControl phasorSelector)
        {
            return phasorSelector.DataContext as PhasorSelectionViewModel;
        }

        private Measurements GetDataContext(MeasurementPagerUserControl measurementPager)
        {
            StackPanel rootPanel = measurementPager.FindName("RootPanel") as StackPanel;

            if ((object)rootPanel == null)
                return null;

            return rootPanel.DataContext as Measurements;
        }

        private void ClearVoltagePhasor_Click(object sender, RoutedEventArgs e)
        {
            GetDataContext(VoltagePhasorSelector).Clear();
        }

        private void ClearCurrentPhasor_Click(object sender, RoutedEventArgs e)
        {
            GetDataContext(CurrentPhasorSelector).Clear();
        }

        private void ClearActiveMeasurement_Click(object sender, RoutedEventArgs e)
        {
            GetDataContext(ActiveMeasurementPager).Clear();
        }

        private void ClearReactiveMeasurement_Click(object sender, RoutedEventArgs e)
        {
            GetDataContext(ReactiveMeasurementPager).Clear();
        }

        private void ClearApparentMeasurement_Click(object sender, RoutedEventArgs e)
        {
            GetDataContext(ApparentMeasurementPager).Clear();
        }

        private void Popup_Opened(object sender, System.EventArgs e)
        {
            CloseAllPopups(sender as Popup);
        }

        private void CloseAllPopups()
        {
            CloseAllPopups(null);
        }

        private void CloseAllPopups(Popup except)
        {
            List<Popup> popups = new List<Popup>();
            List<DependencyObject> children = LogicalTreeHelper.GetChildren(GridDetailView).OfType<DependencyObject>().ToList();

            while (children.Any())
            {
                popups.AddRange(children.OfType<Popup>());
                children = children.SelectMany(child => LogicalTreeHelper.GetChildren(child).OfType<DependencyObject>()).ToList();
            }

            foreach (Popup popup in popups)
            {
                if (popup != except)
                    popup.IsOpen = false;
            }
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DependencyObject ancestor = e.OriginalSource as DependencyObject;
            Popup popup;

            do
            {
                ancestor = LogicalTreeHelper.GetParent(ancestor);
                popup = ancestor as Popup;
            }
            while ((object)ancestor != null && (object)popup == null);

            if ((object)popup != null)
                popup.IsOpen = false;
        }

        private void PowerCalculationUserControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CloseAllPopups();
        }

        private void RuntimeIDTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = e.OriginalSource as TextBlock;

            if ((object)textBlock != null)
                textBlock.Text = m_dataContext.RuntimeID;
        }

        private void InitializeButton_Click(object sender, RoutedEventArgs e)
        {
            m_dataContext.InitializeAdapter();
        }
    }
}
