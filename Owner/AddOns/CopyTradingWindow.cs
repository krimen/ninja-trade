using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;

namespace NinjaTrader.Custom.Indicators.Customs.AddOns
{
    public class CopyTradingWindow : NTWindow, IWorkspacePersistence
    {
        public CopyTradingWindow()
        {
            Caption = "Copy Trading By Gabriel Rodrigues";

            Width = 1085;
            Height = 900;

            // TabControl should be created for window content if tab features are wanted
            TabControl tc = new TabControl();

            // Attached properties defined in TabControlManager class should be set to achieve tab moving, adding/removing tabs
            TabControlManager.SetIsMovable(tc, true);
            TabControlManager.SetCanAddTabs(tc, false);
            TabControlManager.SetCanRemoveTabs(tc, false);

            // if ability to add new tabs is desired, TabControl has to have attached property "Factory" set.
            TabControlManager.SetFactory(tc, new CopyTradingWindowFactory());
            Content = tc;

            /* In order to have link buttons functionality, tab control items must be derived from Tools.NTTabPage
            They can be added using extention method AddNTTabPage(NTTabPage page) */
            tc.AddNTTabPage(new CopyTradingTab());

            // WorkspaceOptions property must be set
            Loaded += (o, e) =>
            {
                if (WorkspaceOptions == null)
                    WorkspaceOptions = new WorkspaceOptions("CopyTrading-" + Guid.NewGuid().ToString("N"), this);
            };

            Caption = "Copy Trading By Gabriel Rodrigues";
            Width = 400;
            Height = 300;

          
        }

        IWorkspacePersistence member;
        public WorkspaceOptions WorkspaceOptions { get; set; }


        // IWorkspacePersistence member. Required for restoring window from workspace
        public void Restore(XDocument document, XElement element)
        {
            if (MainTabControl != null)
                MainTabControl.RestoreFromXElement(element);
        }

        // IWorkspacePersistence member. Required for saving window to workspace
        public void Save(XDocument document, XElement element)
        {
            if (MainTabControl != null)
                MainTabControl.SaveToXElement(element);
        }
    }
}
