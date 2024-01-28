using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace WallAreaByRoom
{
    /// <summary>
    /// Implements the Revit add-in interface IExternalApplication
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Application : IExternalApplication
    {
        /// <summary>
        /// Implements the on Shutdown event
        /// </summary>
        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

        /// <summary>
        /// Implements the OnStartup event
        /// </summary>
        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panel = RibbonPanel(application);
            if (panel != default) 
            { 
                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

                AddButton(panel, thisAssemblyPath,
                    "WallAreaByRoomBtn",
                    "Площадь наружных стен",
                    "WallAreaByRoom.WallAreaByRoomCommand",
                    "Описание появится позже...", 
                    new BitmapImage(
                        new Uri(Path.Combine(
                            Path.GetDirectoryName(thisAssemblyPath), @"CGN_AddIns\ICO", "32x32_SkirtingBoard.png"))));
            }

            return Result.Succeeded;    
        }

        /// <summary></summary>
        private void AddButton(RibbonPanel panel, string thisAssemblyPath, string btnName, string btnTxt, string btnClassName, string tooltip, BitmapImage img)
        {
            PushButton button = panel.AddItem(new PushButtonData(btnName,btnTxt,thisAssemblyPath,btnClassName)) as PushButton;
            button.ToolTip = tooltip;
            button.LargeImage = img;
        }

        /// <summary></summary>
        public RibbonPanel RibbonPanel(UIControlledApplication a)
        {
            string tab = "Testing Pannel";
            RibbonPanel result;
            try
            {
                a.CreateRibbonTab(tab);
            }
            catch {}

            try 
            { 
                result = a.CreateRibbonPanel(tab, "Testing group");
            }
            catch
            {
                result = default;
            }

            return result;
        }
    }
}