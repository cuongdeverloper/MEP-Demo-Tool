using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HicasDemoMEP.Views;
using HicasDemoMEP.ViewModels; // Đảm bảo đã using namespace chứa MainViewModel

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
                Window1 mainWindow = null;

                Action hideAction = () => mainWindow?.Hide();
                Action showAction = () => mainWindow?.Show();

                MainViewModel vm = new MainViewModel(uidoc, hideAction, showAction);

                mainWindow = new Window1(vm);

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