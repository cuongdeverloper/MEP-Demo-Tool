using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
// Dòng dưới đây là chìa khóa để sửa lỗi "could not be found"
using HicasDemoMEP.Views;

namespace HicasDemoMEP.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TaoBanVeMEP : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            try
            {
                Window1 mainWindow = new Window1(uidoc);
                mainWindow.ShowDialog();

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