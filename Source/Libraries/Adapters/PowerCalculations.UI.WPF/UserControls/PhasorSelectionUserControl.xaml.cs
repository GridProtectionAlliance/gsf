using PowerCalculations.UI.WPF.ViewModels;
using PowerCalculations.UI.DataModels;

namespace PowerCalculations.UI.WPF.UserControls
{
	/// <summary>
	/// Interaction logic for PhasorSelectionUserControl.xaml
	/// </summary>
	public partial class PhasorSelectionUserControl
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

		public PhasorType PhasorTypeFilter
		{
			get { return (DataContext as PhasorSelectionViewModel).PhasorTypeFilter; }
			set { (DataContext as PhasorSelectionViewModel).PhasorTypeFilter = value; }
		}
	}
}
