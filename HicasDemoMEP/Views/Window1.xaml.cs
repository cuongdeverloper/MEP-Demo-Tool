using System;
using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;
using RevitUI = Autodesk.Revit.UI;
using HicasDemoMEP.Services;
using HicasDemoMEP.Utils;

namespace HicasDemoMEP.Views
{
    public partial class Window1 : Window
    {
        private RevitUI.UIDocument _uidoc;
        private Document _doc;
        private MEPService _service;
        private List<ElementId> _errorIds = new List<ElementId>();

        public Window1(RevitUI.UIDocument uidoc)
        {
            InitializeComponent();
            _uidoc = uidoc;
            _doc = uidoc.Document;
            _service = new MEPService(_doc);
        }

        private void btnScanPipes_Click(object sender, RoutedEventArgs e)
        {
            var res = _service.ScanPipes();
            txtTotalPipes.Text = $"Tổng số Pipe: {res.total}";
            txtTotalLength.Text = $"Tổng chiều dài: {Math.Round(res.lengthMm, 2)} mm";
            txtUnconnected.Text = $"Số Pipe chưa connect: {res.errorIds.Count}";
            _errorIds = res.errorIds;
            btnHighlight.IsEnabled = _errorIds.Count > 0;
        }

        private void btnHighlight_Click(object sender, RoutedEventArgs e)
        {
            if (_errorIds.Count > 0) _uidoc.Selection.SetElementIds(_errorIds);
            RevitUI.TaskDialog.Show("Thông báo", $"Đã bôi xanh {_errorIds.Count} ống lỗi!");
        }

        private void btnFindId_Click(object sender, RoutedEventArgs e)
        {
            var res = _service.FindElement(txtElementId.Text.Trim());
            if (res.element != null)
            {
                _uidoc.Selection.SetElementIds(new List<ElementId> { res.element.Id });
                _uidoc.ShowElements(res.element.Id);
            }
            else
            {
                RevitUI.TaskDialog.Show("Lỗi", res.message);
            }
        }

        private void btnPickInfo_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            try
            {
                var r = _uidoc.Selection.PickObject(RevitUI.Selection.ObjectType.Element, new MEPSelectionFilter(), "Chọn một ống");
                txtElementDetail.Text = _service.GetElementInfo(_doc.GetElement(r));
            }
            catch { }
            finally { this.Show(); }
        }

        private void btnCreateSheet_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            try
            {
                var refs = _uidoc.Selection.PickObjects(RevitUI.Selection.ObjectType.Element, new MEPSelectionFilter(), "Quét chọn cụm ống");
                if (refs.Count > 0)
                {
                    _service.CreateSpoolSheet(refs);
                    RevitUI.TaskDialog.Show("Thành công", "Đã tạo bản vẽ!");
                }
            }
            catch { }
            finally { this.Show(); }
        }
    }
}