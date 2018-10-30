using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GSF.TimeSeries.UI;
using PowerCalculations.UI.DataModels;

namespace PowerCalculations.UI.WPF.ViewModels
{
	public class PhasorSelectionViewModel : PagedViewModelBase<Phasor, int>
	{
		private bool m_filterChanged = false;

        public PhasorSelectionViewModel()
            : this(0)
        {
            // For designer
        }

		public PhasorSelectionViewModel(int itemsPerPage, bool autoSave = true)
			: base(itemsPerPage, autoSave)
		{

		}

		private PhasorType m_phasorType = PhasorType.Any;
		public PhasorType PhasorTypeFilter
		{
			get { return m_phasorType; }
			set
			{
				if (m_phasorType == value) return;
				m_phasorType = value;
				m_filterChanged = true;
				OnPropertyChanged("PhasorType");
				Load();
			}
		}

		public override int GetCurrentItemKey()
		{
			return CurrentItem.ID;
		}

		public override string GetCurrentItemName()
		{
			return CurrentItem.Label;
		}

		public override bool IsNewRecord
		{
			get { return CurrentItem.ID == 0; }
		}

		/// <summary>
		/// Loads collection of <see cref="PowerCalculation"/> information stored in the database.
		/// </summary>
		/// <remarks>This method is overridden because MethodInfo.Invoke in the base class did not like optional parameters.</remarks>
		public override void Load()
		{
			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				if (OnBeforeLoadCanceled())
					throw new OperationCanceledException("Load was canceled.");

				if (ItemsKeys == null || m_filterChanged)
				{
					ItemsKeys = Phasor.LoadKeys(null, PhasorTypeFilter);
					m_filterChanged = false;

					if ((object)SortSelector != null)
					{
						if (SortDirection == "ASC")
							ItemsKeys = ItemsKeys.OrderBy(SortSelector).ToList();
						else
							ItemsKeys = ItemsKeys.OrderByDescending(SortSelector).ToList();
					}
				}

				var pageKeys = ItemsKeys.Skip((CurrentPageNumber - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
				ItemsSource = Phasor.Load(null, pageKeys);

				OnLoaded();
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null)
				{
					Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Load Phasors Exception:", MessageBoxImage.Error);
					CommonFunctions.LogException(null, "Load Phasors", ex.InnerException);
				}
				else
				{
					Popup(ex.Message, "Load Phasors Exception:", MessageBoxImage.Error);
					CommonFunctions.LogException(null, "Load Phasors", ex);
				}
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}
	}
}
