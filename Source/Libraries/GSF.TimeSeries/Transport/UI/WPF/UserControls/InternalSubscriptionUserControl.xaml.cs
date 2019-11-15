//******************************************************************************************************
//  InternalSubscriptionUserControl.xaml.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/23/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Text.RegularExpressions;
using GSF.TimeSeries.Transport.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GSF.ComponentModel.DataAnnotations;

namespace GSF.TimeSeries.Transport.UI.UserControls
{
    /// <summary>
    /// Interaction logic for InternalSubscriptionUserControl.xaml
    /// </summary>
    public partial class InternalSubscriptionUserControl
    {
        #region [ Members ]

        // Fields
        private bool m_defaultsInitialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="InternalSubscriptionUserControl"/>.
        /// </summary>
        public InternalSubscriptionUserControl() => InitializeComponent();

        #endregion

        #region [ Properties ]

        private SubscriberRequestViewModel ViewModel => Resources["ViewModel"] as SubscriberRequestViewModel;

        #endregion

        #region [ Methods ]

        private void InternalSubscriptionUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            SubscriberRequestViewModel model = ViewModel;
            model?.Dispose();
        }

        private void EnableDataGapRecovery_Checked(object sender, RoutedEventArgs e)
        {
            if (!m_defaultsInitialized && EnableDataGapRecovery.IsChecked.GetValueOrDefault())
            {
                SubscriberRequestViewModel model = ViewModel;

                if ((object)model != null)
                    model.InitializeDefaultInternalSubscriptionSettings();

                m_defaultsInitialized = true;
            }
        }

        private void Acronym_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourcePrefix.Content = $"Use Source Prefix: \"{Acronym.Text}!\"";
        }

        private void STTP_Checked(object sender, RoutedEventArgs e)
        {
            openPDCPort.Content = "7165";
            openHistorianPort.Content = "7175";
            SIEGatePort.Content = "7170";
            openMICPort.Content = "7195";
            HostNameNote.Visibility = Visibility.Visible;

            SubscriberRequestViewModel model = ViewModel;

            if ((object)model != null)
                model.PublisherPort = 7165;
        }

        private void GEP_Checked(object sender, RoutedEventArgs e)
        {
            openPDCPort.Content = "6165";
            openHistorianPort.Content = "6175";
            SIEGatePort.Content = "6170";
            openMICPort.Content = "6195";
            HostNameNote.Visibility = Visibility.Hidden;

            SubscriberRequestViewModel model = ViewModel;

            if ((object)model != null)
                model.PublisherPort = 6165;
        }

        private void Acronym_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !ValidAcronym(((TextBox)sender).Text + e.Text);

        private void PortRange_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !InRange(((TextBox)sender).Text + e.Text, ushort.MinValue, ushort.MaxValue);

        private void PositiveInt_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !IsPositiveInt(e.Text);

        private void PositiveFloat_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = !IsPositiveFloat(e.Text);

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Regex s_acronymValidator = new Regex($"[a-z{AcronymValidationAttribute.ValidationPattern.Substring(2)}");
        private static readonly Regex s_positiveFloat = new Regex("[0-9.]+");
        private static readonly Regex s_positiveInt = new Regex("[0-9]+");

        // Static Methods

        private static bool ValidAcronym(string text) => s_acronymValidator.IsMatch(text);

        private static bool IsPositiveFloat(string text) => s_positiveFloat.IsMatch(text);

        private static bool IsPositiveInt(string text) => s_positiveInt.IsMatch(text);

        private static bool InRange(string text, int low, int high)
        {
            if (int.TryParse(text, out int value))
                return value > low && value < high;

            return false;
        }

        #endregion
    }
}
