using System;
using System.ComponentModel;
using GSF.TimeSeries.UI.UserControls;
using PowerCalculations.UI.WPF.ViewModels;

namespace PowerCalculations.UI.WPF.UserControls
{
	/// <summary>
	/// Interaction logic for PowerCalculationUserControl.xaml
	/// </summary>
	public partial class PowerCalculationUserControl
	{
		public PowerCalculationUserControl()
		{
			DataContext = new PowerCalculationViewModel(16);
			InitializeComponent();

			((PhasorSelectionViewModel)GetVoltagePhasorSelector().DataContext).PropertyChanged += VoltagePhasorChanged;
			((PhasorSelectionViewModel)GetCurrentPhasorSelector().DataContext).PropertyChanged += CurrentPhasorChanged;
		}

		private void CurrentPhasorChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == "CurrentItem")
				((PowerCalculationViewModel)DataContext).CurrentItem.CurrentPhasor = ((PhasorSelectionViewModel)GetCurrentPhasorSelector().DataContext).CurrentItem;
		}

		private void VoltagePhasorChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == "CurrentItem")
				((PowerCalculationViewModel)DataContext).CurrentItem.VoltagePhasor = ((PhasorSelectionViewModel)GetVoltagePhasorSelector().DataContext).CurrentItem;
		}

		private PhasorSelectionUserControl GetVoltagePhasorSelector()
		{
			return FindName("VoltagePhasorSelector") as PhasorSelectionUserControl;
		}

		private PhasorSelectionUserControl GetCurrentPhasorSelector()
		{
			return FindName("CurrentPhasorSelector") as PhasorSelectionUserControl;
		}

		private void RealMeasurementUpdated(object sender, EventArgs e)
		{
			((PowerCalculationViewModel) DataContext).CurrentItem.RealPowerOutputMeasurement = RealMeasurementPager.CurrentItem;
		}

		private void ReactiveMeasurementUpdated(object sender, EventArgs e)
		{
			((PowerCalculationViewModel)DataContext).CurrentItem.ReactivePowerOutputMeasurement = ReactiveMeasurementPager.CurrentItem;
		}

		private void ActiveMeasurementUpdated(object sender, EventArgs e)
		{
			((PowerCalculationViewModel)DataContext).CurrentItem.ActivePowerOutputMeasurement = ActiveMeasurementPager.CurrentItem;
		}
	}
}
