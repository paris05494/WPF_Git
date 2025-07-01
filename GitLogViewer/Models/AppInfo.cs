using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ModelConfigProMax.Model
{
    public class AppInfo : INotifyPropertyChanged
    {
        #region NotifyProperty 
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion
        
        public AppInfo()
        {
            IsLoaded = false;
            DeliveryPath = "";
        }
        public ObservableCollection<BaseUnitModel> AllBaseUnit { get; set; }
        private string _Name { get; set; }
        private string _ConfigPath { get; set; }
        private string _DeliveryPath { get; set; }
        private bool _IsLoaded { get; set; }
        public string Name { get => _Name; set { _Name = value; OnPropertyChanged(nameof(Name)); } }
        public string ConfigPath { get => _ConfigPath; set { _ConfigPath = value; OnPropertyChanged(nameof(ConfigPath)); } }
        public string DeliveryPath { get => _DeliveryPath; set { _DeliveryPath = value; OnPropertyChanged(nameof(DeliveryPath)); } }
        public bool IsLoaded { get => _IsLoaded; set { _IsLoaded = value; OnPropertyChanged(nameof(IsLoaded)); } }

        public AppInfo Clone()
        {
            return new AppInfo() { Name = this.Name, ConfigPath = this.ConfigPath, IsLoaded = this.IsLoaded, AllBaseUnit = this.AllBaseUnit };
        }
    }

}
