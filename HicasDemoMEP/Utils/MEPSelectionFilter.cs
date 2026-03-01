using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace HicasDemoMEP.Utils
{
    public class MEPSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category != null)
            {
                long bicValue = elem.Category.Id.Value;

                return bicValue == (long)BuiltInCategory.OST_PipeCurves ||
                       bicValue == (long)BuiltInCategory.OST_PipeFitting;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}