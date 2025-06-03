#region Using declarations
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using NinjaTrader.Core;
using NinjaTrader.Gui.NinjaScript;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns
{
    public class AddOnExporter : AddOnBase
    {
        private ExportNinjaScript exportWindow;

        private static string DefaultTemplateDirectory { get { return Path.Combine(Globals.UserDataDir, "templates", "AddOnExport"); } } 
        private static string CustomDirectory { get { return Path.Combine(Globals.UserDataDir, "bin", "Custom"); } } 

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "AddOn Exporter";
            }
        }

        protected override void OnWindowCreated(Window window)
        {
            exportWindow = window as ExportNinjaScript ?? exportWindow;
            if (exportWindow == null)
                return;

            exportWindow.Dispatcher.Invoke(() =>
            {
                var grid = exportWindow.Content as Grid;
                if (grid == null)
                    return;

                var gridContextMenu = new ContextMenu();

                var saveToFileMenuItem = new MenuItem { Header = "Save Template" };
                saveToFileMenuItem.Click += SaveToFileMenuItem_Click;
                gridContextMenu.Items.Add(saveToFileMenuItem);

                var loadFromFileMenuItem = new MenuItem { Header = "Load Template" };
                loadFromFileMenuItem.Click += LoadFromFileMenuItem_Click;
                gridContextMenu.Items.Add(loadFromFileMenuItem);

                grid.ContextMenu = gridContextMenu;
            });
        }

        // NOTE: this exists as an extension method elsewhere, this is mainly so I can share this script uncompiled
        private static TElement FindByName<TElement>(FrameworkElement parent, string name) where TElement : class
        {
            return parent.FindName(name) as TElement;
        }

        private void SaveToFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(DefaultTemplateDirectory))
                Directory.CreateDirectory(DefaultTemplateDirectory);

            var wVm = exportWindow.DataContext as ExportNinjaScriptViewModel;
            if (wVm == null)
                return;

            var fileBrowser = new System.Windows.Forms.SaveFileDialog
            {
                Title = "Save AddOn Export Template",
                FileName = "AddOnExport.xml",
                InitialDirectory = DefaultTemplateDirectory, 
                RestoreDirectory = true, 
                Filter = "Export Template|*.xml"
            };
            if (fileBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            int version;
            var cfg = new ExportTemplate
            {
                ExportCompiled = FindByName<CheckBox>(exportWindow, "chkExportCompiledAssembly").IsChecked ?? false,
                ProtectAssembly = wVm.IsProtectAssemblyChecked,
                ProductName = FindByName<TextBox>(exportWindow, "txtProduct").Text,
                Version = Enumerable.Range(0, 4)
                    .Select(i => int.TryParse(FindByName<TextBox>(exportWindow, "txtVersion" + i).Text, out version) ? version : 0)
                    .ToArray(),
                ExportItems = wVm.ExportItems
                    .Select(i => new ExportTemplateItem { Filename = i.Filename.Replace(CustomDirectory, "*"), Type = i.ExportType })
                    .ToList()
            };

            var serializer = new XmlSerializer(typeof(ExportTemplate));
            using (var textWriter = new StreamWriter(fileBrowser.FileName))
                serializer.Serialize(textWriter, cfg);
        }

        private void LoadFromFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var wVm = exportWindow.DataContext as ExportNinjaScriptViewModel;
            if (wVm == null)
                return;

            var fileBrowser = new System.Windows.Forms.OpenFileDialog
            {
                Title = "Load AddOn Export Template",
                FileName = "AddOnExport.xml",
                Filter = "Export Template|*.xml",
                InitialDirectory = DefaultTemplateDirectory
            };
            if (fileBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var serializer = new XmlSerializer(typeof(ExportTemplate));

            ExportTemplate cfg;
            using (var textReader = new StreamReader(fileBrowser.FileName))
                cfg = (ExportTemplate) serializer.Deserialize(textReader);

            FindByName<CheckBox>(exportWindow, "chkExportCompiledAssembly").IsChecked = cfg.ExportCompiled;
            wVm.IsProtectAssemblyChecked = cfg.ProtectAssembly;
            FindByName<TextBox>(exportWindow, "txtProduct").Text = cfg.ProductName;

            for (var i = 0; i < 4; i++)
                FindByName<TextBox>(exportWindow, "txtVersion" + i).Text = cfg.Version[i].ToString();

            wVm.ExportItems.Clear();
            foreach (var i in cfg.ExportItems)
                wVm.ExportItems.Add(new ExportItem
                {
                    ExportType = i.Type,
                    Filename = i.Filename.Replace("*", CustomDirectory)
                });
        }

        public class ExportTemplate
        {
            public bool ExportCompiled { get; set; }
            public bool ProtectAssembly { get; set; }
            public string ProductName { get; set; }
            public int[] Version { get; set; }
            public List<ExportTemplateItem> ExportItems { get; set; }
        }
    
        public class ExportTemplateItem
        {
            public string Filename { get; set; }
            public ExportType Type { get; set; }
        }
    }
}
