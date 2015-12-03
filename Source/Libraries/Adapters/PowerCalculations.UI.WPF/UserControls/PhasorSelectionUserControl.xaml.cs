using System.ComponentModel;
using PowerCalculations.UI.DataModels;
using PowerCalculations.UI.WPF.ViewModels;

namespace PowerCalculations.UI.WPF.UserControls
{
	/// <summary>
	/// Interaction logic for PhasorSelectionUserControl.xaml
	/// </summary>
	public partial class PhasorSelectionUserControl
	{
        private PhasorSelectionViewModel m_dataContext;
        private PhasorType m_phasorTypeFilter;

        public PhasorSelectionUserControl()
		{
			InitializeComponent();
        }

        private void PhasorSelectionUserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                m_dataContext = new PhasorSelectionViewModel(16);
                m_dataContext.PropertyChanged += ViewModel_PropertyChanged;
                m_dataContext.PhasorTypeFilter = m_phasorTypeFilter;
                DataContext = m_dataContext;
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(m_dataContext.CurrentItem) && m_dataContext.IsNewRecord)
                DataGridList.SelectedIndex = -1;
        }

        public Phasor SelectedPhasor
        {
            get
            {
                return IsLoaded ? ((PhasorSelectionViewModel)DataContext).CurrentItem : null;
            }
        }

		public PhasorType PhasorTypeFilter
		{
			get
            {
                return m_phasorTypeFilter;
            }
			set
            {
                var dataContext = DataContext as PhasorSelectionViewModel;

                m_phasorTypeFilter = value;

                if ((object)dataContext != null)
                    dataContext.PhasorTypeFilter = value;
            }
		}
    }
}
