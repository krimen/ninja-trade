using NinjaTrader.Cbi;
using NinjaTrader.Custom.Indicators.Customs.AddOns;
using NinjaTrader.Gui;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using System;
using System.Linq;
using System.Windows;

namespace NinjaTrader.NinjaScript.AddOns
{
    public class CopyTradingAddOn : AddOnBase
    {
        private NTMenuItem copyTradingByGabriel;
        private NTMenuItem existingMenuItemInControlCenter;

        // Same as other NS objects. However there's a difference: this event could be called in any thread
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Example AddOn demonstrating some of the framework's capabilities";
                Name = "AddOn Framework";
            }
        }

        // Will be called as a new NTWindow is created. It will be called in the thread of that window
        protected override void OnWindowCreated(Window window)
        {
            // We want to place our AddOn in the Control Center's menus
            ControlCenter cc = window as ControlCenter;
            if (cc == null)
                return;

            /* Determine we want to place our AddOn in the Control Center's "New" menu
            Other menus can be accessed via the control's "Automation ID". For example: toolsMenuItem, workspacesMenuItem, connectionsMenuItem, helpMenuItem. */
            existingMenuItemInControlCenter = cc.FindFirst("ControlCenterMenuItemNew") as NTMenuItem;
            if (existingMenuItemInControlCenter == null)
                return;

            // 'Header' sets the name of our AddOn seen in the menu structure
            copyTradingByGabriel = new NTMenuItem { Header = "Copy Trading By Gabriel", Style = Application.Current.TryFindResource("MainMenuItem") as Style };

            // Add our AddOn into the "New" menu
            existingMenuItemInControlCenter.Items.Add(copyTradingByGabriel);

            // Subscribe to the event for when the user presses our AddOn's menu item
            copyTradingByGabriel.Click += OnMenuItemClick;
        }

        // Will be called as a new NTWindow is destroyed. It will be called in the thread of that window
        protected override void OnWindowDestroyed(Window window)
        {
            if (copyTradingByGabriel != null && window is ControlCenter)
            {
                if (existingMenuItemInControlCenter != null && existingMenuItemInControlCenter.Items.Contains(copyTradingByGabriel))
                    existingMenuItemInControlCenter.Items.Remove(copyTradingByGabriel);

                copyTradingByGabriel.Click -= OnMenuItemClick;
                copyTradingByGabriel = null;
            }
        }

        // Open our AddOn's window when the menu item is clicked on
        private void OnMenuItemClick(object sender, RoutedEventArgs e)
        {
            Core.Globals.RandomDispatcher.BeginInvoke(new Action(() => new CopyTradingWindow().Show()));
        }
    }
}
