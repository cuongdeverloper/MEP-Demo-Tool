using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using HicasDemoMEP.Views;
using HicasDemoMEP.ViewModels;
using HicasDemoMEP.Services;
using HicasDemoMEP.Utils;

namespace HicasDemoMEP.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TaoBanVeMEP : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            MEPService service = new MEPService(doc);

            try
            {
                MainViewModel vm = new MainViewModel(uidoc, service);

                while (true)
                {
                    Window1 mainWindow = new Window1(vm);

                    vm.CloseWindow = () => mainWindow.Close();

                    mainWindow.ShowDialog();


                    if (vm.PendingRevitTask == "CreateSheet")
                    {
                        vm.PendingRevitTask = "";
                        try
                        {
                            var refs = uidoc.Selection.PickObjects(ObjectType.Element, new MEPSelectionFilter(), "Quét chọn cụm ống (Nhấn ESC để hủy)");
                            if (refs.Count > 0)
                            {
                                service.CreateSpoolSheet(refs);
                                TaskDialog.Show("Thành công", "Đã tạo bản vẽ Spool Sheet thành công!");
                            }
                        }
                        catch {  }
                    }
                    else if (vm.PendingRevitTask == "PickInfo")
                    {
                        vm.PendingRevitTask = "";
                        try
                        {
                            var r = uidoc.Selection.PickObject(ObjectType.Element, new MEPSelectionFilter(), "Click vào một ống (Nhấn ESC để hủy)");
                            vm.ElementDetailText = service.GetElementInfo(doc.GetElement(r));
                        }
                        catch {  }
                    }
                    else if (vm.PendingRevitTask == "AutoTag")
                    {
                        vm.PendingRevitTask = "";

                        if (doc.ActiveView is View3D view3D && !view3D.IsLocked)
                        {
                            TaskDialog.Show("Lưu ý", "Vui lòng khóa góc nhìn 3D (Lock 3D View) hoặc mở mặt bằng (Plan) trước khi dùng lệnh Auto Tag!");
                            continue;
                        }

                        try
                        {
                            var refs = uidoc.Selection.PickObjects(ObjectType.Element, new MEPSelectionFilter(), "Quét chọn ống để gắn Tag (Nhấn ESC để hủy)");
                            if (refs.Count > 0)
                            {
                                service.AutoTagPipes(refs, doc.ActiveView);
                                TaskDialog.Show("Thành công", $"Đã tự động gắn Tag cho {refs.Count} ống!");
                            }
                        }
                        catch {  }
                    }
                    else if (vm.PendingRevitTask == "AutoDim") 
                    {
                        vm.PendingRevitTask = "";

                        if (!(doc.ActiveView is ViewPlan))
                        {
                            TaskDialog.Show("Lưu ý", "Lệnh Auto Dimension hiện tại chỉ hỗ trợ trên mặt bằng (Floor Plan / Ceiling Plan)!");
                            continue;
                        }

                        try
                        {
                            var refs = uidoc.Selection.PickObjects(ObjectType.Element, new MEPSelectionFilter(), "Quét chọn ống để đo kích thước (Nhấn ESC để hủy)");
                            if (refs.Count > 0)
                            {
                                service.AutoDimensionPipes(refs, doc.ActiveView);
                                TaskDialog.Show("Thành công", $"Đã đo kích thước cho {refs.Count} ống!");
                            }
                        }
                        catch {  }
                    }
                    else
                    {
                        break;
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}