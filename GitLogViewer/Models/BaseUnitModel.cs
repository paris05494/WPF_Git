using ModelConfigProMax.Libraries;
using ModelConfigProMax.Utilies;
using ModelConfigProMax.ViewModels;
using ModelConfigProMax.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ModelConfigProMax.Model
{
    public class BaseUnitItem : ObservableObject
    {
        private string _Name;
        private string _ssm;
        private string _cpuName;
        private string _Variant;
        private string _VariantOriginal;
        private List<string> _Variants;
        private int _SelectedVariantIndex;
        private string _SelectedVariant;
        private List<string> _CurrentPath;
        private string _Branch;
        private string _Git;
        private string _Next;
        private string _Revision;
        private string _RevisionOriginal;
        private string _ReleaseRevision;
        private string _HeadRevision;
        private string _DeliveryRevision;
        private string _CompareRevision;
        private string _TestApp;
        private string _Comment;
        private string _Path;
        private string _Repo;
        //private string _CfdApplPath;
        //private string _Status;
        private BaseUnitReleaseModel _ReleaseComment;
        private BUTypeEnum _Type;
        private bool _IsExpanded;
        private bool _IsNew;
        private bool _IsDeleted;
        private bool _IsVariantChanged;
        private bool _IsRevisionChanged;
        private bool _HilightReleaseRevision;
        private bool _HilightHeadRevision;
        private bool _HilightDeliveryRevision;
        private bool _EnableUseCompareRevision;
        public string Name { get => _Name; set { _Name = value; RaisePropertyChanged(nameof(Name)); } }
        public string ssm { get => _ssm; set { _ssm = value; RaisePropertyChanged(nameof(ssm)); } }
        public string cpuName { get => _cpuName; set { _cpuName = value; RaisePropertyChanged(nameof(cpuName)); } }
        public string Variant { get => _Variant; set { _Variant = value?.Trim(); addVariantToList(Variant); IsVariantChanged = Variant != VariantOriginal ? true : false; RaisePropertyChanged(nameof(Variant)); } }
        public string VariantOriginal { get => _VariantOriginal; set { _VariantOriginal = value; IsVariantChanged = false; RaisePropertyChanged(nameof(VariantOriginal)); } }
        public List<string> Variants { get => _Variants; set { _Variants = value; RaisePropertyChanged(nameof(Variants)); } }
        public int SelectedVariantIndex { get => _SelectedVariantIndex; set { _SelectedVariantIndex = value; RaisePropertyChanged(nameof(SelectedVariantIndex)); } }
        public string SelectedVariant { get => _SelectedVariant; set { _SelectedVariant = value; Variant = value; RaisePropertyChanged(nameof(SelectedVariant)); } }
        public List<string> CurrentPaths { get => _CurrentPath; set { _CurrentPath = value; RaisePropertyChanged(nameof(CurrentPaths)); } }
        public string Branch { get => _Branch; set { _Branch = value; RaisePropertyChanged(nameof(Branch)); } }
        public string Git { get => _Git; set { _Git = value; RaisePropertyChanged(nameof(Git)); } }
        public string Next { get => _Next; set { _Next = value; RaisePropertyChanged(nameof(Next)); } }
        public string Revision { get => _Revision; set { _Revision = getRevisionFormat(value); IsRevisionChanged = Revision != RevisionOriginal ? true : false; RaisePropertyChanged(nameof(Revision)); } }
        public string RevisionOriginal { get => _RevisionOriginal; set { _RevisionOriginal = getRevisionFormat(value); IsRevisionChanged = false; RaisePropertyChanged(nameof(RevisionOriginal)); } }
        public string ReleaseRevision { get => _ReleaseRevision; set { _ReleaseRevision = getRevisionFormat(value); updateHighestRevision(); RaisePropertyChanged(nameof(ReleaseRevision)); } }
        public string HeadRevision { get => _HeadRevision; set { _HeadRevision = getRevisionFormat(value); updateHighestRevision(); RaisePropertyChanged(nameof(HeadRevision)); } }
        public string DeliveryRevision { get => _DeliveryRevision; set { _DeliveryRevision = getRevisionFormat(value); updateHighestRevision(); RaisePropertyChanged(nameof(DeliveryRevision)); } }
        public string CompareRevision { get => _CompareRevision; set { _CompareRevision = getRevisionFormat(value); EnableUseCompareRevision = true; RaisePropertyChanged(nameof(CompareRevision)); } }
        public string TestApp { get => _TestApp; set { _TestApp = value; RaisePropertyChanged(nameof(TestApp)); } }
        public string Comment { get => _Comment; set { _Comment = value; RaisePropertyChanged(nameof(Comment)); } }
        public string Path { get => _Path; set { _Path = value; RaisePropertyChanged(nameof(Path)); } }
        public string Repo { get => _Repo; set { _Repo = value; RaisePropertyChanged(nameof(Repo)); } }
        //public string CfdApplPath { get => _CfdApplPath; set { _CfdApplPath = value; RaisePropertyChanged(nameof(CfdApplPath)); } }
        //public string Status { get => _Status; set { _Status = value; RaisePropertyChanged(nameof(Status)); } }
        public BaseUnitReleaseModel ReleaseComment { get => _ReleaseComment; set { _ReleaseComment = value; RaisePropertyChanged(nameof(ReleaseComment)); } }
        public bool IsExpanded { get => _IsExpanded; set { _IsExpanded = value; RaisePropertyChanged(nameof(IsExpanded)); } }
        public bool IsNew { get => _IsNew; set { _IsNew = value; RaisePropertyChanged(nameof(IsNew)); } }
        public bool IsDeleted { get => _IsDeleted; set { _IsDeleted = value; RaisePropertyChanged(nameof(IsDeleted)); } }
        public bool IsVariantNotNull { get { return _Variant != null ? true : false; } }
        //public bool IsVariantNotNull { get { return _Variants != null ? true : false; } }
        public bool IsRevisionNotNull { get { return Revision != null ? true : false; } }
        public bool IsReleaseRevisionNotNull { get { return ReleaseRevision != null ? true : false; } }
        public bool IsHeadRevisionNotNull { get { return HeadRevision != null ? true : false; } }
        public bool IsDeliveryRevisionNotNull { get { return DeliveryRevision != null ? true : false; } }
        public bool IsRevisionChanged { get => _IsRevisionChanged; set { _IsRevisionChanged = value; RaisePropertyChanged(nameof(IsRevisionChanged)); } }
        public bool EnableUseCompareRevision { get => _EnableUseCompareRevision; set { _EnableUseCompareRevision = value; RaisePropertyChanged(nameof(EnableUseCompareRevision)); } }
        public bool IsVariantChanged { get => _IsVariantChanged; set { _IsVariantChanged = value; RaisePropertyChanged(nameof(IsVariantChanged)); } }
        public bool HilightReleaseRevision { get => _HilightReleaseRevision; set { _HilightReleaseRevision = value; RaisePropertyChanged(nameof(HilightReleaseRevision)); } }
        public bool HilightHeadRevision { get => _HilightHeadRevision; set { _HilightHeadRevision = value; RaisePropertyChanged(nameof(HilightHeadRevision)); } }
        public bool HilightDeliveryRevision { get => _HilightDeliveryRevision; set { _HilightDeliveryRevision = value; RaisePropertyChanged(nameof(HilightDeliveryRevision)); } }
        public BUTypeEnum Type { get => _Type; set { _Type = value; RaisePropertyChanged(nameof(Type)); } }
        public BaseUnitInfoVM BaseInfoVm { get; set; }

        private string getRevisionFormat(string rev)
        {
            string ret = "0";
            if (rev != null)
            {
                ret = rev.Replace("_", "");
                if (ret.Length >= 12)
                {
                    ret = ret.Substring(ret.Length - 12);
                    string sub_str = ret.Substring(ret.Length - 6);
                    ret = ret.Replace(sub_str, "_" + sub_str);
                }
            }
            return ret;
        }
        private void addVariantToList(string variant)
        {
            if (Variants == null)
                Variants = new List<string>();
            if (!Variants.Contains(variant))
            {
                Variants.Add(variant);
                SelectedVariantIndex = Variants.IndexOf(variant);
            }
        }
        private void updateHighestRevision()
        {
            HilightReleaseRevision = false;
            HilightHeadRevision = false;
            HilightDeliveryRevision = false;
            Int64 rev = (Revision != null) && (Revision != "") ? Int64.Parse(Revision.Replace("_", "")) : 0;
            Int64 relrev = (ReleaseRevision != null) && (ReleaseRevision != "") ? Int64.Parse(ReleaseRevision.Replace("_", "")) : 0;
            Int64 headrev = (HeadRevision != null) && (HeadRevision != "") ? Int64.Parse(HeadRevision.Replace("_", "")) : 0;
            Int64 delrev = (DeliveryRevision != null) && (DeliveryRevision != "") ? Int64.Parse(DeliveryRevision.Replace("_", "")) : 0;
            List<Int64> list = new List<Int64>() { rev, relrev, headrev, delrev };
            int idx = list.IndexOf(list.Max());
            if (idx == 1)
                HilightReleaseRevision = true;
            else if (idx == 2)
                HilightHeadRevision = true;
            else if (idx == 3)
                HilightDeliveryRevision = true;
        }
    }

    public class BaseUnitModel : BaseUnitItem
    {
        public BaseUnitModel(string name, BUTypeEnum type = BUTypeEnum.UNDEFINED)
        {
            Name = name;
            Type = type;
            Path = "";
            IsExpanded = true;
            IsRevisionChanged = false;
            IsVariantChanged = false;
            IsNew = false;
            IsDeleted = false;
            EnableUseCompareRevision = false;
            Variants = new List<string>();
            //Variants = new List<string> { "" };
            //SelectedVariantIndex = 0;
            //SelectedVariant = "Anwendung";
            //this.HilightRevision = true;
            Items = new ObservableCollection<BaseUnitModel>();
        }

        private ObservableCollection<BaseUnitModel> _Items;
        public ObservableCollection<BaseUnitModel> Items
        {
            get { return _Items; }
            set
            {
                if (_Items != value)
                {
                    _Items = value;
                    RaisePropertyChanged("Items");
                }
            }
        }
        private BaseUnitModel _ParentModel;
        public BaseUnitModel ParentModel
        {
            get { return _ParentModel; }
            set
            {
                if (_ParentModel != value)
                {
                    _ParentModel = value;
                }
            }
        }
        public bool HasLog
        {
            get
            {
                if ((Revision == "") || (Revision == null))
                    return false;
                else
                    return true;
            }
        }
        public BaseUnitModel GetParentModel()
        {
            return this.ParentModel;
        }

        public void AddItem(BaseUnitModel newItem)
        {
            newItem.ParentModel = this;
            this.Items.Add(newItem);

        }

        public int GetItemIndex(string name)
        {
            int ret = -1;
            bool isExist = false;
            int idx = 0;
            for (idx = 0; idx < this.Items.Count; idx++)
            {
                if (this.Items[idx].Name == name)
                {
                    isExist = true;
                    break;
                }
            }
            if (isExist)
                ret = idx;
            return ret;

        }

        public void RemoveItem(string name)
        {
            bool isExist = false;
            int idx = 0;
            for (idx = 0; idx < this.Items.Count; idx++)
            {
                if (this.Items[idx].Name == name)
                {
                    isExist = true;
                    break;
                }
            }
            if (isExist)
                this.Items.RemoveAt(idx);

        }

    }

}
