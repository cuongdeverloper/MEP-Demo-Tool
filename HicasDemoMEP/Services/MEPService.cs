using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace HicasDemoMEP.Services
{
    public class MEPService
    {
        private Document _doc;
        public MEPService(Document doc) => _doc = doc;

        // 1. Logic Quét ống
        public (int total, double lengthMm, List<ElementId> errorIds) ScanPipes()
        {
            var collector = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_PipeCurves)
                .WhereElementIsNotElementType();

            int total = 0;
            double lengthMm = 0;
            List<ElementId> errorIds = new List<ElementId>();

            foreach (Pipe pipe in collector.Cast<Pipe>())
            {
                total++;
                var lengthParam = pipe.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                if (lengthParam != null) lengthMm += (lengthParam.AsDouble() * 304.8);

                bool isDisconnected = pipe.ConnectorManager.Connectors
                    .Cast<Connector>()
                    .Any(c => c.Domain == Domain.DomainPiping && !c.IsConnected);

                if (isDisconnected) errorIds.Add(pipe.Id);
            }
            return (total, lengthMm, errorIds);
        }

        // 2. Logic Tìm Element theo ID
        public (Element element, string message) FindElement(string inputId)
        {
            if (!long.TryParse(inputId, out long idValue))
                return (null, "Vui lòng nhập ID là số nguyên.");

            Element elem = _doc.GetElement(new ElementId(idValue));
            return (elem, elem != null ? "Thành công" : "Không tìm thấy Element.");
        }

        // 3. Logic Lấy thông tin chi tiết
        public string GetElementInfo(Element elem)
        {
            if (elem == null) return "Chưa có thông tin...";

            var sizeParam = elem.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE)
                           ?? elem.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
            string size = sizeParam != null ? sizeParam.AsValueString() : "N/A";

            var offsetParam = elem.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM);
            double offsetMm = offsetParam != null ? offsetParam.AsDouble() * 304.8 : 0;

            return $"Tên: {elem.Name}\nID: {elem.Id}\nKích thước: {size}\nCao độ: {Math.Round(offsetMm, 2)} mm";
        }

        // 4. Logic Tạo Sheet bản vẽ
        public void CreateSpoolSheet(IList<Reference> selectedRefs)
        {
            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            foreach (Reference r in selectedRefs)
            {
                BoundingBoxXYZ bbox = _doc.GetElement(r).get_BoundingBox(null);
                if (bbox == null) continue;
                minX = Math.Min(minX, bbox.Min.X); minY = Math.Min(minY, bbox.Min.Y); minZ = Math.Min(minZ, bbox.Min.Z);
                maxX = Math.Max(maxX, bbox.Max.X); maxY = Math.Max(maxY, bbox.Max.Y); maxZ = Math.Max(maxZ, bbox.Max.Z);
            }

            using (Transaction t = new Transaction(_doc, "Tạo bản vẽ MEP tự động"))
            {
                t.Start();
                var viewType = new FilteredElementCollector(_doc).OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>().First(v => v.ViewFamily == ViewFamily.ThreeDimensional);

                View3D newView = View3D.CreateIsometric(_doc, viewType.Id);
                newView.Name = "Spool-" + Guid.NewGuid().ToString().Substring(0, 5);
                newView.SetSectionBox(new BoundingBoxXYZ { Min = new XYZ(minX - 1, minY - 1, minZ - 1), Max = new XYZ(maxX + 1, maxY + 1, maxZ + 1) });

                var titleBlock = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof(FamilySymbol)).FirstElement();
                ViewSheet sheet = ViewSheet.Create(_doc, titleBlock.Id);
                Viewport.Create(_doc, sheet.Id, newView.Id, new XYZ(1.5, 1, 0));
                t.Commit();
            }
        }
    }
}