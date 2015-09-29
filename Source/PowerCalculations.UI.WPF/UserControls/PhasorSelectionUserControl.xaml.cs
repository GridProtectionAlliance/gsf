using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using PowerCalculations.UI.WPF.ViewModels;
using PowerCalculations.UI.DataModels;

namespace PowerCalculations.UI.WPF.UserControls
{
	/// <summary>
	/// Interaction logic for PhasorSelectionUserControl.xaml
	/// </summary>
	public partial class PhasorSelectionUserControl : UserControl
	{
		public PhasorSelectionUserControl()
		{
			InitializeComponent();
			DataContext = new PhasorSelectionViewModel(16);
		}

		public Phasor SelectedPhasor
		{
			get { return ((PhasorSelectionViewModel) DataContext).CurrentItem; }
		}
	}
}
