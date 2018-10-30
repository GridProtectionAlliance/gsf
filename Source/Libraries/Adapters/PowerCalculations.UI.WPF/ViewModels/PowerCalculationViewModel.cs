using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GSF.Data;
using GSF.TimeSeries.UI;
using PowerCalculations.UI.DataModels;

namespace PowerCalculations.UI.WPF.ViewModels
{
	public class PowerCalculationViewModel : PagedViewModelBase<PowerCalculation, int>
	{
        public PowerCalculationViewModel()
            : this(0)
        {
            // For designer
        }

		public PowerCalculationViewModel(int itemsPerPage, bool autoSave = true)
			: base(itemsPerPage, autoSave)
		{

		}

		public override int GetCurrentItemKey()
		{
			return CurrentItem.ID;
		}

		public override string GetCurrentItemName()
		{
			return CurrentItem.CircuitDescription;
		}

		public override bool IsNewRecord
		{
			get { return CurrentItem.ID == 0; }
		}

        public string RuntimeID
        {
            get
            {
                try
                {
                    using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
                    {
                        int id = database.ExecuteScalar<int>("SELECT ID FROM CustomActionAdapter WHERE AdapterName = 'PHASOR!POWERCALC'");
                        return CommonFunctions.GetRuntimeID("CustomActionAdapter", id, database);
                    }
                }
                catch
                {
                    return "-1";
                }
            }
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

				if (ItemsKeys == null)
				{
					ItemsKeys = PowerCalculation.LoadKeys(null);

					if ((object)SortSelector != null)
					{
						if (SortDirection == "ASC")
							ItemsKeys = ItemsKeys.OrderBy(SortSelector).ToList();
						else
							ItemsKeys = ItemsKeys.OrderByDescending(SortSelector).ToList();
					}
				}

				var pageKeys = ItemsKeys.Skip((CurrentPageNumber - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
				ItemsSource = PowerCalculation.Load(null, pageKeys);

				OnLoaded();
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null)
				{
					Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Load Power Calculations Exception:", MessageBoxImage.Error);
					CommonFunctions.LogException(null, "Load Power Calculations", ex.InnerException);
				}
				else
				{
					Popup(ex.Message, "Load Power Calculations Exception:", MessageBoxImage.Error);
					CommonFunctions.LogException(null, "Load Power Calculations", ex);
				}
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

        public void InitializeAdapter()
        {
            try
            {
                if (Confirm("Do you want to Initialize the power calculation adapter?", "Confirm Initialize"))
                    Popup(CommonFunctions.SendCommandToService("Initialize " + RuntimeID), "Initialize", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Popup(ex.Message, "Initialize Adapter:", MessageBoxImage.Error);
                CommonFunctions.LogException(null, "Initialize Adapter", ex);
            }
        }
	}
}
