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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PowerCalculations.UI.WPF.ViewModels;

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
	}
}
