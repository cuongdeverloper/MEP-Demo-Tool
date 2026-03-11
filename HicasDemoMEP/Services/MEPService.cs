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

        // 5. Logic Tự động gắn Tag cho ống
        public void AutoTagPipes(IList<Reference> selectedRefs, View activeView)
        {
            using (Transaction t = new Transaction(_doc, "Auto Tag Pipes"))
            {
                t.Start();
                foreach (Reference r in selectedRefs)
                {
                    Element elem = _doc.GetElement(r);
                    if (elem is Pipe pipe)
                    {
                        LocationCurve locCurve = pipe.Location as LocationCurve;
                        if (locCurve != null)
                        {
                            XYZ midPoint = locCurve.Curve.Evaluate(0.5, true);

                            try
                            {
                                IndependentTag tag = IndependentTag.Create(
                                    _doc,
                                    activeView.Id,
                                    new Reference(pipe),
                                    true,
                                    TagMode.TM_ADDBY_CATEGORY,
                                    TagOrientation.Horizontal,
                                    midPoint);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                t.Commit();
            }
        }
        // 6. Logic Tự động đo kích thước (Auto Dimension)
        public void AutoDimensionPipes(IList<Reference> selectedRefs, View activeView)
        {
            using (Transaction t = new Transaction(_doc, "Auto Dimension Pipes"))
            {
                t.Start();

                // Yêu cầu Revit tính toán điểm tham chiếu (Reference) của hình học
                Options geomOptions = new Options { ComputeReferences = true, View = activeView };

                foreach (Reference r in selectedRefs)
                {
                    Element elem = _doc.GetElement(r);
                    if (elem is Pipe pipe)
                    {
                        LocationCurve locCurve = pipe.Location as LocationCurve;
                        if (locCurve == null || !(locCurve.Curve is Line pipeLine)) continue;

                        XYZ direction = pipeLine.Direction;

                        // Chỉ hỗ trợ ống vẽ ngang trên mặt bằng (Bỏ qua ống đứng trục Z)
                        if (Math.Abs(direction.Z) > 0.99) continue;

                        ReferenceArray refArray = new ReferenceArray();

                        // Lấy hình học của ống để tìm 2 mặt cắt ở 2 đầu
                        GeometryElement geomElem = pipe.get_Geometry(geomOptions);
                        if (geomElem != null)
                        {
                            foreach (GeometryObject geomObj in geomElem)
                            {
                                if (geomObj is Solid solid)
                                {
                                    foreach (Face face in solid.Faces)
                                    {
                                        if (face is PlanarFace planarFace)
                                        {
                                            // Nếu mặt phẳng vuông góc với hướng của ống -> Nó là cái nắp ở 2 đầu
                                            if (Math.Abs(planarFace.FaceNormal.DotProduct(direction)) > 0.99)
                                            {
                                                if (planarFace.Reference != null)
                                                    refArray.Append(planarFace.Reference);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (refArray.Size == 2)
                        {
                            XYZ cross = XYZ.BasisZ.CrossProduct(direction).Normalize();
                            XYZ offset = cross * (500.0 / 304.8);

                            XYZ p1 = pipeLine.GetEndPoint(0) + offset;
                            XYZ p2 = pipeLine.GetEndPoint(1) + offset;
                            Line dimLine = Line.CreateBound(p1, p2);

                            try
                            {
                                _doc.Create.NewDimension(activeView, dimLine, refArray);
                            }
                            catch {  }
                        }
                    }
                }
                t.Commit();
            }
        }
    }
}