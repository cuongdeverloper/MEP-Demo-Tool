using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace HicasDemoMEP
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // 1. Khai báo tên Tab và Panel
            string tabName = "Hicas MEP";
            string panelName = "Tools";

            // 2. Tạo Tab mới trên Revit
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception)
            {
                // Bỏ qua nếu Tab đã tồn tại
            }

            // 3. Tạo Ribbon Panel bên trong Tab
            RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

            // 4. Lấy đường dẫn của file .dll hiện tại
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // 5. Khai báo Nút bấm (PushButton)
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