using System;
using System.Collections.Generic;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HicasDemoMEP.Core;
using HicasDemoMEP.Services;

namespace HicasDemoMEP.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly MEPService _service;
        private List<ElementId> _errorIds = new List<ElementId>();

        // --- CÁC BIẾN MỚI CHO VÒNG LẶP FORM (DIALOG LOOP) ---
        public Action CloseWindow { get; set; }
        public string PendingRevitTask { get; set; } = string.Empty;

        #region Properties (Binding dữ liệu lên UI)

        private string _totalPipesText = "Tổng số Pipe: 0";
        public string TotalPipesText { get => _totalPipesText; set { _totalPipesText = value; OnPropertyChanged(); } }

        private string _totalLengthText = "Tổng chiều dài: 0 mm";
        public string TotalLengthText { get => _totalLengthText; set { _totalLengthText = value; OnPropertyChanged(); } }

        private string _unconnectedText = "Số Pipe chưa connect: 0";
        public string UnconnectedText { get => _unconnectedText; set { _unconnectedText = value; OnPropertyChanged(); } }

        private bool _isHighlightEnabled;
        public bool IsHighlightEnabled { get => _isHighlightEnabled; set { _isHighlightEnabled = value; OnPropertyChanged(); } }

        private string _elementIdInput;
        public string ElementIdInput { get => _elementIdInput; set { _elementIdInput = value; OnPropertyChanged(); } }

        private string _elementDetailText = "Chưa có thông tin...";
        public string ElementDetailText { get => _elementDetailText; set { _elementDetailText = value; OnPropertyChanged(); } }

        #endregion

        #region Commands (Binding sự kiện nút bấm)

        public ICommand ScanPipesCommand { get; }
        public ICommand HighlightCommand { get; }
        public ICommand CreateSheetCommand { get; }
        public ICommand FindIdCommand { get; }
        public ICommand PickInfoCommand { get; }
        public ICommand AutoTagCommand { get; }
        public ICommand AutoDimCommand { get; }
        #endregion

        // Hàm khởi tạo mới (Chỉ nhận uidoc và service)
        public MainViewModel(UIDocument uidoc, MEPService service)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
            _service = service;

            // Khởi tạo Commands
            ScanPipesCommand = new RelayCommand(ExecuteScanPipes);
            HighlightCommand = new RelayCommand(ExecuteHighlight);
            FindIdCommand = new RelayCommand(ExecuteFindId);

            // Các lệnh cần đóng form để thao tác chuột trên Revit
            CreateSheetCommand = new RelayCommand(ExecuteCreateSheet);
            PickInfoCommand = new RelayCommand(ExecutePickInfo);
            AutoTagCommand = new RelayCommand(ExecuteAutoTag);
            AutoDimCommand = new RelayCommand(ExecuteAutoDim);
        }

        private void ExecuteScanPipes(object obj)
        {
            var res = _service.ScanPipes();
            TotalPipesText = $"Tổng số Pipe: {res.total}";
            TotalLengthText = $"Tổng chiều dài: {Math.Round(res.lengthMm, 2)} mm";
            UnconnectedText = $"Số Pipe chưa connect: {res.errorIds.Count}";
            _errorIds = res.errorIds;
            IsHighlightEnabled = _errorIds.Count > 0;
        }

        private void ExecuteHighlight(object obj)
        {
            if (_errorIds.Count > 0)
            {
                _uidoc.Selection.SetElementIds(_errorIds);
                Autodesk.Revit.UI.TaskDialog.Show("Thông báo", $"Đã bôi xanh {_errorIds.Count} ống lỗi!");
            }
        }

        private void ExecuteFindId(object obj)
        {
            var res = _service.FindElement(ElementIdInput?.Trim());
            if (res.element != null)
            {
                _uidoc.Selection.SetElementIds(new List<ElementId> { res.element.Id });
                _uidoc.ShowElements(res.element.Id);
            }
            else
            {
                Autodesk.Revit.UI.TaskDialog.Show("Lỗi", res.message);
            }
        }


        private void ExecuteCreateSheet(object obj)
        {
            PendingRevitTask = "CreateSheet";
            CloseWindow?.Invoke();
        }

        private void ExecutePickInfo(object obj)
        {
            PendingRevitTask = "PickInfo";
            CloseWindow?.Invoke();
        }

        private void ExecuteAutoTag(object obj)
        {
            PendingRevitTask = "AutoTag";
            CloseWindow?.Invoke();
        }
        private void ExecuteAutoDim(object obj)
        {
            PendingRevitTask = "AutoDim";
            CloseWindow?.Invoke();
        }
    }
}