using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace HicasDemoMEP
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Hicas MEP";
            string panelName = "Tools";

            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception)
            {
            }

            RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData btnData = new PushButtonData(
                "cmdTaoBanVeMEP",
                "Demo\nMEP Tool",
                assemblyPath,
                "HicasDemoMEP.Commands.TaoBanVeMEP"); // Đường dẫn tới class Command

            btnData.ToolTip = "Công cụ kiểm tra ống và xuất bản vẽ tự động";

            // 6. Thêm nút vào Panel
            panel.AddItem(btnData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}