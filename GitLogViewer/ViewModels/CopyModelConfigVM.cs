using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using ModelConfigProMax.Libraries;
using ModelConfigProMax.Model;
using ModelConfigProMax.Services;
using ModelConfigProMax.Utilies;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ModelConfigProMax.ViewModels
{
    public class CopyModelConfigVM : ViewModelBase
    {
        public CopyModelConfigVM(ObservableCollection<BaseUnitModel> collectionView_baseUnits)
        {
            CollectionView_BaseUnits = collectionView_baseUnits;
            updateAppName();
            updateBaseUnits();
        }

        public CopyModelConfigVM(ObservableCollection<AppInfo> collectionView_AppInfo)
        {
            CollectionView_AppInfo = collectionView_AppInfo;
            updateAppName();
        }

        #region Members
        private ObservableCollection<BaseUnitModel> _CollectionView_BaseUnits;
        private static ObservableCollection<AppInfo> _CollectionView_AppInfo;
        private string _searchBaseUnit;
        private string _searchAppName;
        private ModelConfigVM _mergeModel;

        #endregion

        #region Properties
        public string TitleName { get; set; }
        public string SelectedPath { get; set; } = "";
        public ModelConfigVM modelConfigVM { get; set; }

        public ObservableCollection<BaseUnitModel> CollectionView_BaseUnits { get => _CollectionView_BaseUnits; set { _CollectionView_BaseUnits = value; OnPropertyChanged(nameof(CollectionView_BaseUnits)); } }
        public ObservableCollection<AppInfo> CollectionView_AppInfo { get => _CollectionView_AppInfo; set { _CollectionView_AppInfo = value; OnPropertyChanged(nameof(CollectionView_AppInfo)); } }
        //public ObservableCollection<AppInfo> CollectionView_AppInfo { get; set; } = new ObservableCollection<AppInfo>();

        public ObservableCollection<BaseUnitWrapper> ListBaseUnits { get; } = new ObservableCollection<BaseUnitWrapper>();
        public ObservableCollection<AppNameWrapper> ListAppNames { get; } = new ObservableCollection<AppNameWrapper>();

        public string SearchBaseUnit { get => _searchBaseUnit; set { _searchBaseUnit = value; OnPropertyChanged(); updateBaseUnits(value);}}
        public string SearchAppName { get => _searchAppName; set { _searchAppName = value; OnPropertyChanged(); updateAppName(value); } }

        #endregion

        #region ICommand
        public ICommand SubmitBaseUnitClickCommand => new RelayCommand(SubmitBaseUnitClick);

        #endregion

        #region Action ICommand
        private void SubmitBaseUnitClick(object _obj)
        {
            if(ListBaseUnits != null && ListAppNames != null)
            {
                var selectedBaseUnits = ListBaseUnits
                .Where(p => p.IsSelected_BaseUnits && p.BaseUnit != null)
                .Select(p => p.BaseUnit)
                .ToList();

                if (selectedBaseUnits.Count == 0)
                {
                    MessageBox.Show("Select at least one baseunit to copy.");
                    return;
                }

                var selectedAppNames = ListAppNames
                .Where(t => t.IsSelected_AppNames && t.AppName != null)
                .Select(t => t.AppName)
                .ToList();

                if (selectedAppNames == null || selectedAppNames.Count == 0)
                {
                    MessageBox.Show("Select destination folder");
                    return;
                }

                bool isMergeSuccess = false;
                _mergeModel = new ModelConfigVM();
                if (_mergeModel != null)
                {
                    foreach (var target in selectedAppNames)
                    {
                        _mergeModel.mergeModelConfig(selectedBaseUnits, new List<AppInfo> { target });
                        isMergeSuccess = true;
                    }
                }
                if (isMergeSuccess)
                {
                    MessageBox.Show("Copy successfully!!");
                }
                else
                {
                    MessageBox.Show("Copy failed!!");
                }
            }

            // ปิดหน้าต่าง
            var window = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);

            if (window != null)
                window.Close();
        }

        #endregion

        #region Commands
        private void updateBaseUnits(string searchQuery = null)
        {
            ListBaseUnits.Clear();

            foreach (BaseUnitModel bu in CollectionView_BaseUnits)
            {
                if (bu.Items != null && bu.Items.Count > 0)
                {
                    foreach (BaseUnitModel sub_bu in bu.Items)
                    {
                        if (string.IsNullOrEmpty(searchQuery) || sub_bu.Name.ToLower().Contains(searchQuery.ToLower()))
                        {
                            ListBaseUnits.Add(new BaseUnitWrapper { IsSelected_BaseUnits = false, BaseUnit = sub_bu });
                        }
                    }
                }
            }
        }

        private void updateAppName(string searchQuery = null)
        {
            ListAppNames.Clear();

            if (CollectionView_AppInfo != null)
            {
                foreach (AppInfo an in CollectionView_AppInfo)
                {
                    if (an != null)
                    {
                        if (string.IsNullOrEmpty(searchQuery) || an.Name.ToLower().Contains(searchQuery.ToLower()))
                        {
                            ListAppNames.Add(new AppNameWrapper { IsSelected_AppNames = false, AppName = an });
                        }
                    }
                }
            }
        }


        #endregion
    }

    public class BaseUnitWrapper
    {
        public bool IsSelected_BaseUnits { get; set; }
        public BaseUnitModel BaseUnit { get; set; }
    }

    public class AppNameWrapper
    {
        public bool IsSelected_AppNames { get; set; }
        public AppInfo AppName { get; set; }

    }
}

