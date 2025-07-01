using ModelConfigProMax.Libraries;
using ModelConfigProMax.Model;
using ModelConfigProMax.Services;
using ModelConfigProMax.Utilies;
using ModelConfigProMax.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ModelConfigProMax.ViewModels
{
    public class ModelConfigVM : ViewModelBase
    {
        public ModelConfigVM()
        {
            _cmd = new CMDCommand();
            _cmd.removeDirectory("temp");
            if (!File.Exists("log.txt")) { File.Create("log.txt").Close(); }
            StreamWriter writer = new StreamWriter("log.txt", true, Encoding.UTF8);
            BaseUnitInfoViewModel = new BaseUnitInfoVM();
            VariantSuggestionViewModel = new VariantSuggestionVM();
            //hwWorkingVM = new HwWorkingVM();
            _AllAppInfo = new ObservableCollection<AppInfo>() { new AppInfo() { Name = "Please select work path" } };
            _AllDeliveryAppInfo = new ObservableCollection<AppInfo>() { new AppInfo() { Name = "Please select work path" } };
            collectionView_AppInfo = _AllAppInfo;
            AppName = "No Selected Application";
            DeliveryPath = "";
            LoadwithHeadRevision = false;
            LoadDeliveryAppFile();
            
        }

        #region Private Variable
        private Dictionary<string, BaseUnitModel> delete_list;
        private bool _LoadwithHeadRevision;
        private static string _WorkPath;
        private string _SearchApp;
        private string _AppName;
        private ObservableCollection<AppInfo> _AllAppInfo;
        private ObservableCollection<AppInfo> _AllDeliveryAppInfo;
        private string _DeliveryPath;
        private string _CfdApplPath;
        private List<string> _FolderList = new List<string>();
        private int _SelectedFolderIndex;
        private string _SelectedFolder;
        private AppInfo _AppInfo_SelectedItem;
        private List<string> _ErrorLog;
        private ObservableCollection<AppInfo> _collectionView_AppInfo;
        private CMDCommand _cmd;
        private StreamWriter writer;
        //private BaseUnitInfoVM BaseUnitInfoViewModel;
        private VariantSuggestionVM VariantSuggestionViewModel;
        private Window hwWorkingView;

        #endregion

        #region Public Variable 
        public bool LoadwithHeadRevision { get => _LoadwithHeadRevision; set { _LoadwithHeadRevision = value; OnPropertyChanged(nameof(LoadwithHeadRevision)); } }
        public BaseUnitInfoVM BaseUnitInfoViewModel { get; set; }
        //public HwWorkingVM hwWorkingVM { get; set; }
        public string SearchApp { get => _SearchApp; set { _SearchApp = value; SearchAppName(value); OnPropertyChanged(nameof(SearchApp)); } }
        public string AppName { get => _AppName; set { _AppName = value; OnPropertyChanged(nameof(AppName)); } }
        public string DeliveryPath { get => _DeliveryPath; set { _DeliveryPath = value; OnPropertyChanged(nameof(DeliveryPath)); } }
        public string CfdApplPath { get => _CfdApplPath; set { _CfdApplPath = value; OnPropertyChanged(nameof(CfdApplPath)); } }
        public List<string> FolderList { get => _FolderList; set { _FolderList = value; OnPropertyChanged(nameof(FolderList)); } }
        public int SelectedFolderIndex { get => _SelectedFolderIndex; set { _SelectedFolderIndex = value; OnPropertyChanged(nameof(SelectedFolderIndex)); } }
        public string SelectedFolder { get => _SelectedFolder; set { _SelectedFolder = value; SelectedFolderChanged(); OnPropertyChanged(nameof(SelectedFolder)); } }
        public AppInfo AppInfo_SelectedItem { get => _AppInfo_SelectedItem; set { _AppInfo_SelectedItem = value; OnPropertyChanged(nameof(AppInfo_SelectedItem)); } }
        public ObservableCollection<AppInfo> collectionView_AppInfo { get => _collectionView_AppInfo; set { _collectionView_AppInfo = value; OnPropertyChanged(nameof(collectionView_AppInfo)); } }
        public CopyModelConfigVM CopyConfigVM { get; set; }

        #endregion

        #region ICommand
        public ICommand SearchAppCommand => new RelayCommand(SearchAppName);
        public ICommand SelectedAppInfo_DoubleClickCommand => new RelayCommand(SelectedAppInfo_DoubleClick);
        public ICommand AddSelectedApp_RightClickCommand => new RelayCommand(AddSelectedApp_RightClick);
        public ICommand AddCompareAppCommand => new RelayCommand(AddCompareApp);
        public ICommand OpenModelConfigCommand => new RelayCommand(OpenModelConfig);
        public ICommand OpenDeliveryModelConfigCommand => new RelayCommand(OpenDeliveryModelConfig);
        public ICommand OpenCFDWorkingViewCommand => new RelayCommand(OpenCFDWorkingView);
        public ICommand OpenAppConfigCommand => new RelayCommand(OpenAppConfig);
        public ICommand OpenAppFolderCommand => new RelayCommand(OpenAppFolder);
        public ICommand RevertModelConfigCommand => new RelayCommand(RevertModelConfig);
        public ICommand SaveModelConfigCommand => new RelayCommand(SaveModelConfig);

        #endregion

        #region Action ICommand
        private void SearchAppName(object _appname) //for SearchApp key enter command
        {
            string appname = (string)_appname;
            if (IsAppNameCorrectFormat(appname))
            {
                var progressWindow = new ProgressDialog();
                ProgressDialogVM progressVM = new ProgressDialogVM("Searching Application");
                progressWindow.DataContext = progressVM;
                progressWindow.Show();
                SearchAppNameString(appname);
                progressWindow.Close();
            }
        }

        private void SearchAppName(string _appname) //for SearchApp text changed 
        {
            string appname = _appname;
            if (IsAppNameCorrectFormat(appname))
            {
                var progressWindow = new ProgressDialog();
                ProgressDialogVM progressVM = new ProgressDialogVM("Searching Application");
                progressWindow.DataContext = progressVM;
                progressWindow.Show();
                SearchAppNameString(appname);
                progressWindow.Close();
            }
        }

        private void SearchAppNameString(string appname)
        {
            ObservableCollection<AppInfo> AllAppInfo;
            ObservableCollection<AppInfo> fillter_app = new ObservableCollection<AppInfo>();
            string pattern = "^" + appname.ToLower().Replace("*", ".+");
            if (SelectedFolder == "Anwendung")
            {
                AllAppInfo = _AllAppInfo;
            }
            else
            {
                AllAppInfo = _AllDeliveryAppInfo;
            }
            foreach (AppInfo appinfo in AllAppInfo)
            {
                if (Regex.IsMatch(appinfo.Name.ToLower(), pattern, RegexOptions.IgnoreCase))
                {
                    fillter_app.Add(appinfo.Clone());
                }
            }
            collectionView_AppInfo = fillter_app;
        }

        private async void SelectedAppInfo_DoubleClick(object _appname)
        {
            var progress_status = new Dictionary<string, string>();
            progress_status.Add("Status", "true");
            progress_status.Add("Text", "Loading...");
            if (SelectedFolder == "Anwendung")
            {
                // Update progress view
                progress_status["Text"] = "Updating baseunits...";
                MVVM_Library.Mediator.NotifyColleagues("ProgressViewUpdate", progress_status);
                // Update baseunit info
                await Task.Run(() =>
                {
                    string compare_app_path = "";
                    if (SelectedFolder == "Anwendung")
                    {
                        compare_app_path = AppInfo_SelectedItem.ConfigPath;
                    }
                    AppName = AppInfo_SelectedItem.Name;
                    AppInfo_SelectedItem.AllBaseUnit = loadModelConfig(AppInfo_SelectedItem);
                    AppInfo_SelectedItem.AllBaseUnit = VariantSuggestionViewModel.Check(AppInfo_SelectedItem);
                    BaseUnitInfoViewModel.updateBaseUnitInfoList(_WorkPath, AppInfo_SelectedItem.AllBaseUnit);
                    BaseUnitInfoViewModel.CompareAppName = "";
                    DeliveryPath = AppInfo_SelectedItem.DeliveryPath;
                    // Display result
                    string msg = $"Complete with {_ErrorLog.Count.ToString()} Errors..!!\n";
                    foreach (string log in _ErrorLog)
                    {
                        msg += log + "\n";
                    }
                    DialogResult dialogResult = MessageBox.Show(msg, "Load ModelConfig", MessageBoxButtons.OK);
                    // Update progress view
                    progress_status["Status"] = "false";
                    MVVM_Library.Mediator.NotifyColleagues("ProgressViewUpdate", progress_status);
                    
                });
                VariantSuggestionViewModel.DisplaySuggestion();
                
            }
            else
            {
                string path = Path.Combine(_WorkPath, "Anwendung", AppName, "ModelConfig.xml");
                DialogResult dialogResult = MessageBox.Show("Add Compare Delivery Application ModelConfig need to take sometime.\n" +
                    "Do you want to continue?", "Add Compare Delivery App", MessageBoxButtons.OKCancel);
                if (dialogResult == DialogResult.OK)
                {
                    AddCompareApp(null);
                }
            }
            MVVM_Library.Mediator.NotifyColleagues("RefreshViewUpdate", null);
        }

        private void AddSelectedApp_RightClick(object _appname)
        {
            var progressWindow = new ProgressDialog();
            //ProgressDialogVM progressVM = new ProgressDialogVM("Loading ModelConfig");
            //progressWindow.DataContext = progressVM;
            //progressWindow.Show();
            //AppName = AppInfo_SelectedItem.Name;
            //AppInfo_SelectedItem.AllBaseUnit = loadModelConfig(AppInfo_SelectedItem);
            //BaseUnitInfoViewModel.updateBaseUnitInfoList(_WorkPath, AppInfo_SelectedItem.AllBaseUnit);
            //AppInfo_SelectedItem.AllBaseUnit = VariantSuggestionViewModel.Check(AppInfo_SelectedItem); // Check Suggestion variant
            //progressWindow.Close();
            //DeliveryPath = AppInfo_SelectedItem.DeliveryPath;
            //string msg = $"Complete with {_ErrorLog.Count.ToString()} Errors..!!\n";
            //foreach (string log in _ErrorLog)
            //{
            //    msg += log + "\n";
            //}
            //DialogResult dialogResult = MessageBox.Show(msg, "Load ModelConfig", MessageBoxButtons.OK);

        }

        private void OpenModelConfig(object _appname)
        {
            string path = Path.Combine(_WorkPath, "Anwendung", AppName, "ModelConfig.xml");
            if (File.Exists(path))
            {
                _cmd.openTextFile(path);
            }
        }

        private void OpenDeliveryModelConfig(object _appname)
        {
            if (File.Exists(DeliveryPath))
            {
                _cmd.openTextFile(DeliveryPath);
            }
        }

        private void OpenCFDWorkingView(object _appname)
        {
            var hw_input = new Dictionary<string, string>();
            hw_input.Add("CfdApplPath", CfdApplPath);
            MVVM_Library.Mediator.NotifyColleagues("HwConfigUpdate", hw_input);
        }

        private void OpenAppConfig(object _appname)
        {
            string path = Path.Combine(_WorkPath, "Anwendung", AppName, "ApplicationConfig.xml");
            if (File.Exists(path))
            {
                _cmd.openTextFile(path);
            }
        }

        private void OpenAppFolder(object _appname)
        {
            string path = Path.Combine(_WorkPath, "Anwendung", AppName);
            if (Directory.Exists(path))
            {
                _cmd.openFolder(path);
            }
        }

        private void RevertModelConfig(object _appname)
        {
            string path = Path.Combine(_WorkPath, "Anwendung", AppName, "ModelConfig.xml");
            DialogResult dialogResult = MessageBox.Show("To revert ModelConfig need to take sometime to reload Application.\n" +
                "Do you want to continue?", "Revert ModelConfig", MessageBoxButtons.OKCancel);
            if (dialogResult == DialogResult.OK)
            {
                if (File.Exists(path))
                {
                    _cmd.restoreGit(_WorkPath, path);
                    SelectedAppInfo_DoubleClick(null);
                }
            }
        }

        private void SaveModelConfig(object _appname)
        {
            string path = Path.Combine(_WorkPath, "Anwendung", AppName, "ModelConfig.xml");
            if (File.Exists(path))
            {
                delete_list = new Dictionary<string, BaseUnitModel>();
                XDocument XMLdoc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
                foreach (BaseUnitModel BUmodel in AppInfo_SelectedItem.AllBaseUnit)
                {
                    updateXmlRevision(XMLdoc, BUmodel);
                }

                XMLdoc.Save(path);
                string xml_text = File.ReadAllText(path);
                while (xml_text.Contains("\r\n      \r\n"))
                {
                    xml_text = xml_text.Replace("\r\n      \r\n", "\r\n").Replace("\r\n   \r\n", "\r\n");
                }
                while (xml_text.Contains("\r\n   \r\n"))
                {
                    xml_text = xml_text.Replace("\r\n   \r\n", "\r\n");
                }
                File.WriteAllText(path, xml_text);

                foreach (string bu in delete_list.Keys)
                {
                    if (delete_list[bu].IsDeleted && (delete_list[bu].Type == BUTypeEnum.BaseUnitClusters))
                    {
                        AppInfo_SelectedItem.AllBaseUnit.Remove(delete_list[bu]);
                    }
                    else
                    {
                        delete_list[bu].RemoveItem(bu);
                        if ((delete_list[bu].Items.Count == 0) && (delete_list[bu].Type == BUTypeEnum.BaseUnitClusters))
                            AppInfo_SelectedItem.AllBaseUnit.Remove(delete_list[bu]);
                    }
                }
                DialogResult dialogResult = MessageBox.Show("Complete !!", "Save ModelConfig", MessageBoxButtons.OK);
            }
        }

        #endregion

        #region Private Command
        private void getAllApplication(string path)
        {
            _AllAppInfo = new ObservableCollection<AppInfo>();
            collectionView_AppInfo.Clear();
            DirectoryInfo work_place = new DirectoryInfo(path);
            DirectoryInfo[] all_folder = work_place.GetDirectories();
            List<DirectoryInfo> applications = new List<DirectoryInfo>();
            // Check the folder contain ModelConfig
            foreach (DirectoryInfo app in all_folder)
            {
                string modelconfig_path = Path.Combine(app.FullName, "ModelConfig.xml");
                if (File.Exists(modelconfig_path))
                {
                    applications.Add(app);
                }
            }
            var progressWindow = new ProgressDialog();
            ProgressDialogVM progressVM = new ProgressDialogVM("Loading from " + applications.Count.ToString() + " files");
            progressWindow.DataContext = progressVM;
            progressWindow.Show();
            int cnt = 0;
            foreach (DirectoryInfo app in applications)
            {
                string modelconfig_path = Path.Combine(app.FullName, "ModelConfig.xml");
                _AllAppInfo.Add(getApplicationModelConfig(app.Name, modelconfig_path));
                //var t_get_config = Task.Run(() => getApplicationModelConfig(app.Name, modelconfig_path));
                //await Task.WhenAll(t_get_config);
                //_AllAppInfo.Add(t_get_config.Result);
                cnt += 1;
                progressVM.updateRunningText("Get all application name " + cnt.ToString() + " from " + applications.Count.ToString());
            }
            progressWindow.Close();
        }

        private AppInfo getApplicationModelConfig(string app_name, string path)
        {
            AppInfo appinfo = new AppInfo() { Name = app_name, ConfigPath = path };
            return appinfo;
        }

        private ObservableCollection<BaseUnitModel> loadModelConfig(AppInfo app)
        {
            ObservableCollection<BaseUnitModel> AllBaseUnit = new ObservableCollection<BaseUnitModel>();
            List<string> check_duplicate_bu = new List<string>();
            Dictionary<string, BaseUnitModel> DeliveryBaseUnit = loadDeliveryModelConfig(app);
            _ErrorLog = new List<string>();
            try
            {
                XmlDocument XMLdoc = new XmlDocument();
                XMLdoc.Load(app.ConfigPath);
                //XmlNamespaceManager nsmgr = new XmlNamespaceManager(XMLdoc.NameTable);
                //nsmgr.AddNamespace("bk", "urn:samples");
                XmlNodeList ProcessorNodeList = XMLdoc.GetElementsByTagName("processors");
                foreach (XmlNode processor_node in ProcessorNodeList)
                {
                    string processor_name = processor_node.SelectSingleNode("cpuBlockName").Attributes["Value"].Value;
                    BaseUnitModel processor = new BaseUnitModel(processor_name) { Type = BUTypeEnum.BaseUnitClusters };
                    XmlNodeList BaseUnitClusterNodeList = processor_node.SelectNodes("BaseUnitClusters");
                    if (BaseUnitClusterNodeList.Count > 0)
                    {
                        foreach (XmlNode cluster_node in BaseUnitClusterNodeList)
                        {
                            BaseUnitModel baseunit_cluster = getBaseUnitCluster(cluster_node, DeliveryBaseUnit);
                            baseunit_cluster.ParentModel = processor;
                            processor.AddItem(baseunit_cluster);
                        }
                    }
                    XmlNodeList BaseUnitNodeList = processor_node.SelectNodes("baseUnits");
                    if (BaseUnitNodeList.Count > 0)
                    {
                        foreach (XmlNode baseunit_node in BaseUnitNodeList)
                        {
                            BaseUnitModel baseunit = readBaseUnitNode(baseunit_node);
                            baseunit.ParentModel = processor;
                            if (baseunit.Next == "true")
                            {
                                string new_name = baseunit.Name + "/" + baseunit.Variant;
                                //check_duplicate_bu.Add(new_name);
                                baseunit.Name = new_name;
                                processor.AddItem(getBaseUnitInfo(baseunit, DeliveryBaseUnit));
                            }
                            else
                            {
                                //check_duplicate_bu.Add(baseunit.Name);
                                processor.AddItem(getBaseUnitInfo(baseunit, DeliveryBaseUnit));
                            }
                            if (!check_duplicate_bu.Contains(baseunit.Name))
                                check_duplicate_bu.Add(baseunit.Name);
                            else
                                ;

                        }
                    }
                    AllBaseUnit.Add(processor);
                }

                XmlNode CfdApplNode = XMLdoc.DocumentElement.SelectSingleNode("CfdAppl");
                if (CfdApplNode != null)
                    CfdApplPath = CfdApplNode.Attributes["Value"].Value;
            }
            catch (Exception e)
            {
                _ErrorLog.Add("Load ModelConfig not Complete");
                AllBaseUnit.Add(new BaseUnitModel("Cannot Load ModelConfig"));
                AllBaseUnit.Add(new BaseUnitModel(e.Message));
                AllBaseUnit.Add(new BaseUnitModel(e.StackTrace));
            }
            return AllBaseUnit;
        }

        private Dictionary<string, BaseUnitModel> loadDeliveryModelConfig(AppInfo app)
        {
            Dictionary<string, BaseUnitModel> delivery_app = new Dictionary<string, BaseUnitModel>();
            if (LoadwithHeadRevision)
            {
                try
                {
                    //string file_path = "temp//" + app.DeliveryPath + "//ModelConfig.xml";
                    string file_path = app.DeliveryPath;
                    if (!File.Exists(file_path))
                    {
                        //string app_path = findDeliveryPath(app.Name);
                        string app_path = getArtifactoryPath(AppInfo_SelectedItem.Name);
                        //if (!checkoutDelivaryAppModelConfig(app_path))
                        //    _ErrorLog.Add("Cannot checkout ModelConfig from Delivery SVN");
                        //app.DeliveryPath = checkoutDeliveryAppModelConfig(app_path);
                        app.DeliveryPath = downloadArtifactoryApp(app_path);
                        file_path = app.DeliveryPath;
                    }
                    delivery_app = loadModelConfigDict(file_path);

                }
                catch
                {
                    //_ErrorLog.Add("Cannot read Delivery ModelConfig");
                    //delivery_app.Add("-1", new BaseUnitModel("Cannot read Delivery ModelConfig"));
                }
            }
            return delivery_app;
        }

        private Dictionary<string, BaseUnitModel> loadModelConfigDict(string file_path)
        {
            Dictionary<string, BaseUnitModel> baseunit_dict = new Dictionary<string, BaseUnitModel>();
            List<string> check_duplicate_bu = new List<string>();
            if (File.Exists(file_path))
            {
                XmlDocument XMLdoc = new XmlDocument();
                XMLdoc.Load(file_path);
                //XmlNamespaceManager nsmgr = new XmlNamespaceManager(XMLdoc.NameTable);
                //nsmgr.AddNamespace("bk", "urn:samples");
                XmlNodeList ProcessorNodeList = XMLdoc.GetElementsByTagName("processors");
                foreach (XmlNode processor_node in ProcessorNodeList)
                {
                    string processor_name = processor_node.SelectSingleNode("cpuBlockName").Attributes["Value"].Value;
                    string processor_ssm = processor_node.SelectSingleNode("ssm").Attributes["Value"].Value;
                    string processor_cpuName = processor_node.SelectSingleNode("cpuName").Attributes["Value"].Value;
                    XmlNodeList BaseUnitClusterNodeList = processor_node.SelectNodes("BaseUnitClusters");
                    Dictionary<string, BaseUnitModel> emtry_dict = new Dictionary<string, BaseUnitModel>();
                    if (BaseUnitClusterNodeList.Count > 0)
                    {
                        foreach (XmlNode cluster_node in BaseUnitClusterNodeList)
                        {
                            BaseUnitModel baseUnit = getBaseUnitCluster(cluster_node, emtry_dict);
                            baseUnit.ParentModel = new BaseUnitModel(processor_name) { ssm = processor_ssm, cpuName = processor_cpuName };
                            foreach (BaseUnitModel bu in baseUnit.Items)
                            {
                                check_duplicate_bu.Add(bu.Name);
                                if (!baseunit_dict.ContainsKey(bu.Name))
                                {
                                    bu.ParentModel = baseUnit;
                                    baseunit_dict.Add(bu.Name, bu);
                                }
                            }
                        }
                    }
                    XmlNodeList BaseUnitNodeList = processor_node.SelectNodes("baseUnits");
                    if (BaseUnitNodeList.Count > 0)
                    {
                        foreach (XmlNode baseunit_node in BaseUnitNodeList)
                        {
                            BaseUnitModel baseUnit = readBaseUnitNode(baseunit_node);
                            baseUnit.ParentModel = new BaseUnitModel(processor_name) { ssm = processor_ssm, cpuName = processor_cpuName };
                            string bu_name = baseUnit.Name;
                            if (baseUnit.Next == "true")
                                bu_name = baseUnit.Name + "/" + baseUnit.Variant;
                            if (!check_duplicate_bu.Contains(bu_name))
                                check_duplicate_bu.Add(bu_name);
                            if (!baseunit_dict.ContainsKey(bu_name))
                            {
                                baseunit_dict.Add(bu_name, baseUnit);
                            }
                            //if (check_duplicate_bu.Contains(baseUnit.Name)) // detected domain name
                            //{
                            //    string new_name = baseunit_dict[baseUnit.Name].Name + "/" + baseunit_dict[baseUnit.Name].Variant;
                            //    if (!check_duplicate_bu.Contains(new_name)) // bring bu in domain to dict
                            //    {
                            //        check_duplicate_bu.Add(new_name);
                            //        baseunit_dict[baseUnit.Name].Name = new_name;
                            //        if (!baseunit_dict.ContainsKey(new_name))
                            //        {
                            //            baseunit_dict.Add(new_name, baseunit_dict[baseUnit.Name]);
                            //        }
                            //    }
                            //    new_name = baseUnit.Name + "/" + baseUnit.Variant;
                            //    check_duplicate_bu.Add(new_name);
                            //    baseUnit.Name = new_name;
                            //    if (!baseunit_dict.ContainsKey(new_name)) // store current bu to dict
                            //    {
                            //        baseunit_dict.Add(new_name, baseUnit);
                            //    }
                            //}
                            //else
                            //{
                            //    check_duplicate_bu.Add(baseUnit.Name);
                            //    if (!baseunit_dict.ContainsKey(baseUnit.Name))
                            //    {
                            //        baseunit_dict.Add(baseUnit.Name, baseUnit);
                            //    }
                            //}
                        }
                    }
                }
            }
            return baseunit_dict;
        }

        private BaseUnitModel getBaseUnitCluster(XmlNode node, Dictionary<string, BaseUnitModel> DeliveryBaseUnit)
        {
            List<string> check_duplicate_bu = new List<string>();
            string type = node.SelectSingleNode("type").Attributes["Value"].Value;
            BUTypeEnum cluster_type = BUTypeEnum.UNDEFINED;
            if (type == "hardware") { cluster_type = BUTypeEnum.Interface; }
            else if (type == "sgvm") { cluster_type = BUTypeEnum.Interface; }
            // Interface node
            XmlNode interface_node = node.SelectSingleNode("interface");
            BaseUnitModel baseunit = readBaseUnitNode(interface_node);
            string cluster_name = baseunit.Name;
            BaseUnitModel cluster = new BaseUnitModel(cluster_name, cluster_type);
            baseunit.Type = BUTypeEnum.Interface;
            baseunit.ParentModel = cluster;
            if (baseunit.Next == "true")
            {
                string new_name = baseunit.Name + "/" + baseunit.Variant;
                check_duplicate_bu.Add(new_name);
                baseunit.Name = new_name;
                cluster.AddItem(getBaseUnitInfo(baseunit, DeliveryBaseUnit));
            }
            else
            {
                check_duplicate_bu.Add(baseunit.Name);
                cluster.AddItem(getBaseUnitInfo(baseunit, DeliveryBaseUnit));
            }

            // Model node
            XmlNode model_node = node.SelectSingleNode("model");
            baseunit = readBaseUnitNode(model_node);
            baseunit.Type = BUTypeEnum.Model;
            baseunit.ParentModel = cluster;
            if (baseunit.Next == "true")
            {
                string new_name = baseunit.Name + "/" + baseunit.Variant;
                check_duplicate_bu.Add(new_name);
                baseunit.Name = new_name;
                cluster.AddItem(getBaseUnitInfo(baseunit, DeliveryBaseUnit));
            }
            else
            {
                check_duplicate_bu.Add(baseunit.Name);
                cluster.AddItem(getBaseUnitInfo(baseunit, DeliveryBaseUnit));
            }

            return cluster;
        }

        private BaseUnitModel readBaseUnitNode(XmlNode node)
        {
            string name = node.SelectSingleNode("name").Attributes["Value"].Value;
            string revision = node.SelectSingleNode("revision").Attributes["Value"].Value;
            string variant = node.SelectSingleNode("variant").Attributes["Value"].Value;
            XmlNode branch_node = node.SelectSingleNode("branch");
            string branch = branch_node != null ? branch_node.Attributes["Value"].Value : "";
            XmlNode git_node = node.SelectSingleNode("git");
            string git = git_node != null ? git_node.Attributes["Value"].Value : "false";
            XmlNode next_node = node.SelectSingleNode("next");
            string next = next_node != null ? next_node.Attributes["Value"].Value : "false";

            BaseUnitModel model = new BaseUnitModel(name, BUTypeEnum.BaseUnit)
            {
                Variant = variant,
                //Variants = new List<string>() { variant },
                //SelectedVariantIndex = 0,
                Revision = revision,
                Branch = branch,
                Git = git,
                Next = next
            };
            return model;
        }

        private BaseUnitModel getBaseUnitInfo(BaseUnitModel baseUnit, Dictionary<string, BaseUnitModel> DeliveryBaseUnit)
        {
            string bu_name = baseUnit.Name.Replace('-', '_');
            string bu_variant = baseUnit.Variant;
            string path = Path.Combine(_WorkPath, "BaseUnits", bu_name);
            string bu_repo = _WorkPath;
            if (baseUnit.Next == "true")
            {
                bu_name = bu_name.Replace("/", "\\");
                bu_variant = baseUnit.Branch;
                path = Path.Combine(_WorkPath, "BaseUnitsNext", bu_name + "_" + bu_variant);
                if (!Directory.Exists(path))
                {
                    path = Path.Combine(_WorkPath, "BaseUnitsNext", bu_name);
                }
                bu_repo = path;
            }
            baseUnit.Path = path;
            baseUnit.Repo = bu_repo;
            //List<string> variants = new List<string>() { bu_variant }; ;
            //int variant_idx = 0;
            List<string> variants = GeneralCommand.getBaseUnitVariantFromFolder(bu_name, path);
            int variant_idx = variants.IndexOf(bu_variant);
            if (variant_idx < 0)
            {
                variants.Add(bu_variant);
                variant_idx = variants.IndexOf(bu_variant);
            }
            BaseUnitReleaseModel release_rev = getBaseUnitReleaseRevision(baseUnit, bu_variant, baseUnit.Revision, baseUnit.Next); //get release revision from file BaseUnitReleases.xml
            string delivery_rev = !DeliveryBaseUnit.ContainsKey(baseUnit.Name) ? "0" : DeliveryBaseUnit[baseUnit.Name].Revision;
            BaseUnitReleaseModel model_rev = getGitRelease(baseUnit); //get release revision from git repository
            BaseUnitModel model = new BaseUnitModel(baseUnit.Name, BUTypeEnum.BaseUnit)
            {
                Variant = bu_variant,
                VariantOriginal = bu_variant,
                Variants = variants,
                SelectedVariantIndex = variant_idx,
                SelectedVariant = bu_variant,
                Revision = baseUnit.Revision,
                RevisionOriginal = baseUnit.Revision,
                ReleaseRevision = release_rev.Revision,
                HeadRevision = model_rev.Revision,
                DeliveryRevision = delivery_rev,
                TestApp = release_rev.TestedApp,
                Branch = baseUnit.Branch,
                Git = baseUnit.Git,
                Next = baseUnit.Next,
                Path = baseUnit.Path,
                Repo = baseUnit.Repo,
                Type = baseUnit.Type,
                ReleaseComment = release_rev,
                ParentModel = baseUnit.ParentModel
            };

            return model;
        }

        private BaseUnitReleaseModel getBaseUnitReleaseRevision(BaseUnitModel baseUnit, string variant, string revision, string bu_next)
        {
            Dictionary<long, BaseUnitReleaseModel> release_model_dict = new Dictionary<long, BaseUnitReleaseModel>() { { 0, new BaseUnitReleaseModel() { Revision = "0", TestedApp = "", Comment = "" } } };
            List<long> revision_list = new List<long>() { 0 };
            string path = Path.Combine(baseUnit.Path, "BaseUnitReleases.xml");
            if (File.Exists(path))
            {
                try
                {
                    XmlDocument XMLdoc = new XmlDocument();
                    XMLdoc.Load(path);
                    XmlNode root = XMLdoc.DocumentElement;
                    XmlNodeList ReleaseNodeList = root.SelectNodes("releases");
                    foreach (XmlNode release_node in ReleaseNodeList)
                    {
                        if (variant == release_node.SelectSingleNode("Version").Attributes["Value"].Value)
                        {
                            BaseUnitReleaseModel release_model = getBaseUnitRelease(release_node);
                            writer.WriteLine(baseUnit.Name + ", " + variant);
                            long release_revision = 0;
                            if (IsRevisionCorrectFormat(release_model.Revision))
                            {
                                release_revision = Int64.Parse(release_model.Revision.Replace("_", ""));
                            }
                            else
                            {
                                writer.WriteLine(baseUnit.Name + ", " + variant + ": Revision is not correct");
                            }
                            if (!release_model_dict.ContainsKey(release_revision))
                            {
                                release_model_dict.Add(release_revision, release_model);
                                revision_list.Add(release_revision);
                            }
                        }
                    }
                    revision_list.Sort();
                }
                catch
                {
                    _ErrorLog.Add("Cannot read BaseUnitReleases from " + baseUnit.Name);
                }
            }
            return release_model_dict[revision_list[revision_list.Count - 1]];
        }

        private BaseUnitReleaseModel getBaseUnitRelease(XmlNode release_node)
        {
            string release_revision = release_node.SelectSingleNode("SvnRevision").Attributes["Value"].Value;
            string release_testedapp = release_node.SelectSingleNode("TestedApplication").Attributes["Value"].Value;
            string ExternComment = release_node.SelectSingleNode("ExternComment").Attributes["Value"].Value;
            string InternComment = release_node.SelectSingleNode("InternComment").Attributes["Value"].Value;
            string Features = release_node.SelectSingleNode("Features").Attributes["Value"].Value;
            string Bugfixes = release_node.SelectSingleNode("Bugfixes").Attributes["Value"].Value;
            string OpenPoints = release_node.SelectSingleNode("OpenPoints").Attributes["Value"].Value;
            string Dependencies = release_node.SelectSingleNode("Dependencies").Attributes["Value"].Value;
            BaseUnitReleaseModel release_model = new BaseUnitReleaseModel()
            {
                Revision = release_revision,
                TestedApp = release_testedapp,
                ExternComment = ExternComment,
                InternComment = InternComment,
                Features = Features,
                Bugfixes = Bugfixes,
                OpenPoints = OpenPoints,
                Dependencies = Dependencies
            };
            return release_model;
        }

        private BaseUnitReleaseModel getGitRelease(BaseUnitModel baseUnit)
        {
            string release_revision = "0";
            if (LoadwithHeadRevision)
            {
                if (Directory.Exists(baseUnit.Path))
                {
                    release_revision = GeneralCommand.getBaseUnitLastestRevision(baseUnit);
                }
            }
            BaseUnitReleaseModel release_model = new BaseUnitReleaseModel() { Revision = release_revision, TestedApp = "", Comment = "" };
            return release_model;
        }

        private void SelectedFolderChanged()
        {
            if (SelectedFolder == "Anwendung")
            {
                collectionView_AppInfo = _AllAppInfo;
                CopyConfigVM = new CopyModelConfigVM(_AllAppInfo);
            }
            else
            {
                collectionView_AppInfo = _AllDeliveryAppInfo;
            }
        }

        private ObservableCollection<BaseUnitModel> LoadCompareModelConfig(ObservableCollection<BaseUnitModel> allBaseUnit, string compare_config_path)
        {
            ObservableCollection<BaseUnitModel> AllBaseUnit = new ObservableCollection<BaseUnitModel>(allBaseUnit);
            Dictionary<string, BaseUnitModel> baseunit_dict = loadModelConfigDict(compare_config_path);
            foreach (BaseUnitModel domain in AllBaseUnit)
            {
                foreach (BaseUnitModel bu in domain.Items)
                {
                    if (bu.Items.Count > 0)
                    {
                        int delete_cnt = 0;
                        foreach (BaseUnitModel sub_bu in bu.Items)
                        {
                            if (baseunit_dict.ContainsKey(sub_bu.Name))
                            {
                                sub_bu.CompareRevision = baseunit_dict[sub_bu.Name].Revision;
                                baseunit_dict.Remove(sub_bu.Name);
                            }
                            else
                            {
                                sub_bu.IsDeleted = true;
                                delete_cnt += 1;
                            }
                        }
                        if (delete_cnt == 2)
                            bu.IsDeleted = true;
                    }
                    else
                    {
                        //string bu_name = bu.Name;
                        //if(bu.Next == "true")
                        //    bu_name = bu.Name + "/" + bu.Variant;
                        if (baseunit_dict.ContainsKey(bu.Name))
                        {
                            bu.CompareRevision = baseunit_dict[bu.Name].Revision;
                            baseunit_dict.Remove(bu.Name);
                        }
                        else
                        {
                            bu.IsDeleted = true;
                        }
                    }
                }
            }
            foreach (BaseUnitModel bu in baseunit_dict.Values)
            {
                if ((bu.Type == BUTypeEnum.Interface) && (bu.Items.Count > 0))
                {
                    BaseUnitModel processor_model = AllBaseUnit.Where(X => X.Name == bu.ParentModel.Name).FirstOrDefault();
                    if (processor_model == null)
                    {
                        processor_model = new BaseUnitModel(bu.ParentModel.Name) { Type = BUTypeEnum.BaseUnitClusters, IsNew = true, ssm = bu.ParentModel.ssm, cpuName = bu.ParentModel.cpuName };
                        AllBaseUnit.Add(processor_model);
                    }
                    BaseUnitModel interface_model = new BaseUnitModel(bu.Name) { Type = bu.Type, IsNew = true };
                    foreach (BaseUnitModel sub_bu in bu.Items)
                    {
                        interface_model.AddItem(new BaseUnitModel(sub_bu.Name)
                        {
                            Variant = sub_bu.Variant,
                            VariantOriginal = sub_bu.Variant,
                            Revision = "0",
                            RevisionOriginal = sub_bu.Revision,
                            ReleaseRevision = "0",
                            HeadRevision = "0",
                            DeliveryRevision = "0",
                            IsNew = true,
                            Type = sub_bu.Type,
                            CompareRevision = sub_bu.Revision
                        });
                    }
                    processor_model.AddItem(interface_model);
                }
                else if (bu.Type == BUTypeEnum.BaseUnit)
                {
                    BaseUnitModel processor_model = AllBaseUnit.Where(X => X.Name == bu.ParentModel.Name).FirstOrDefault();
                    if (processor_model == null)
                    {
                        processor_model = new BaseUnitModel(bu.ParentModel.Name) { Type = BUTypeEnum.BaseUnitClusters, IsNew = true, ssm = bu.ParentModel.ssm, cpuName = bu.ParentModel.cpuName };
                        AllBaseUnit.Add(processor_model);
                    }
                    if (bu.Next == "true")
                    {
                        bu.Name = bu.Name + "/" + bu.Variant;
                        bu.Variant = bu.Branch;
                    }
                    processor_model.AddItem(new BaseUnitModel(bu.Name)
                    {
                        Variant = bu.Variant,
                        VariantOriginal = bu.Variant,
                        Branch = bu.Branch,
                        Revision = "0",
                        RevisionOriginal = bu.Revision,
                        ReleaseRevision = "0",
                        HeadRevision = "0",
                        DeliveryRevision = "0",
                        IsNew = true,
                        Type = bu.Type,
                        CompareRevision = bu.Revision
                    });
                }
            }
            return AllBaseUnit;
        }

        private void AddCompareApp(object _appname)
        {
            var progress_status = new Dictionary<string, string>();
            progress_status.Add("Status", "true");
            progress_status.Add("Text", "Adding application...");
            MVVM_Library.Mediator.NotifyColleagues("ProgressViewUpdate", progress_status);

            // Add application to table
            string compare_app_path = "";
            if (SelectedFolder == "Anwendung")
            {
                compare_app_path = AppInfo_SelectedItem.ConfigPath;
            }
            else
            {
                if (!File.Exists(AppInfo_SelectedItem.DeliveryPath))
                {
                    //compare_app_path = findDeliveryPath(AppInfo_SelectedItem.Name);
                    compare_app_path = getArtifactoryPath(AppInfo_SelectedItem.Name);
                    //compare_app_path = checkoutDeliveryAppModelConfig(compare_app_path);
                    compare_app_path = downloadArtifactoryApp(compare_app_path);
                    AppInfo_SelectedItem.DeliveryPath = compare_app_path;
                }
                else
                    compare_app_path = AppInfo_SelectedItem.DeliveryPath;

            }
            AppInfo_SelectedItem.AllBaseUnit = LoadCompareModelConfig(BaseUnitInfoViewModel.BaseUnitList, compare_app_path);
            BaseUnitInfoViewModel.updateBaseUnitInfoList(_WorkPath, AppInfo_SelectedItem.AllBaseUnit);
            BaseUnitInfoViewModel.CompareAppName = AppInfo_SelectedItem.Name;

            // Update progress view
            progress_status["Status"] = "false";
            MVVM_Library.Mediator.NotifyColleagues("ProgressViewUpdate", progress_status);
        }

        private void LoadDeliveryAppFile()
        {
            string filePath = System.Environment.CurrentDirectory + "\\" + "delivery_app.json";
            if (File.Exists(filePath))
            {
                JObject jsonn = JObject.Parse(File.ReadAllText(filePath));
                _AllDeliveryAppInfo = new ObservableCollection<AppInfo>();
                foreach (var release in jsonn)
                {
                    foreach (var app in release.Value)
                    {
                        _AllDeliveryAppInfo.Add(new AppInfo() { Name = release.Key + "/" + app.ToString() });
                    }
                }
            }
        }

        private void updateXmlRevision(XDocument XMLdoc, BaseUnitModel BUmodel)
        {
            if ((BUmodel.Type == BUTypeEnum.BaseUnitClusters) && BUmodel.IsDeleted)
            {
                XElement parent_baseunit = getParentNode(XMLdoc, BUmodel);
                parent_baseunit.Remove();
                delete_list.Add(BUmodel.Name, BUmodel);
            }
            else
            {
                if (BUmodel.Items.Count > 0)
                {
                    if ((BUmodel.Type == BUTypeEnum.Interface) && (BUmodel.IsDeleted))
                    {
                        string name = BUmodel.Name;
                        string el_tag = "interface";

                        XElement baseunit = (from el in XMLdoc.Descendants(el_tag)
                                             where (string)el.Element("name").Attribute("Value") == name
                                             select el).First();
                        baseunit.Parent.Remove();
                        XElement parent_baseunit = getParentNode(XMLdoc, BUmodel.ParentModel);
                        if ((parent_baseunit.Descendants("baseUnits").Count() == 0) && (parent_baseunit.Descendants("BaseUnitClusters").Count() == 0))
                            parent_baseunit.Remove();
                        delete_list.Add(name, BUmodel.ParentModel);
                    }
                    else if ((BUmodel.Type == BUTypeEnum.Interface) && (BUmodel.IsNew))
                    {
                        string name = BUmodel.Name;
                        string variant = BUmodel.Variant;
                        string revision = BUmodel.Revision;
                        string el_tag = "interface";
                        //string parent_el_tag = "processors";
                        string parent_name = BUmodel.ParentModel.Name;
                        string type = "hardware";
                        if (name.Contains("_SGVM_"))
                            type = "sgvm";

                        //XElement parent_baseunit = (from el in XMLdoc.Descendants(parent_el_tag)
                        //                            where (string)el.Element("cpuBlockName").Attribute("Value") == parent_name
                        //                            select el).First();

                        XElement parent_baseunit = getParentNode(XMLdoc, BUmodel.ParentModel);

                        XElement cluster_baseunit = new XElement("BaseUnitClusters", new XText("\n         "),
                                                        new XElement("type",
                                                            new XAttribute("Value", type)));
                        foreach (BaseUnitModel bu in BUmodel.Items)
                        {
                            name = bu.Name;
                            variant = bu.Variant;
                            revision = bu.Revision;
                            if (bu.Type == BUTypeEnum.Interface)
                                el_tag = "interface";
                            else if (bu.Type == BUTypeEnum.Model)
                                el_tag = "model";

                            cluster_baseunit.Add(new XText("\n         "), new XElement(el_tag, new XText("\n            "),
                                                new XElement("name",
                                                    new XAttribute("Value", name)), new XText("\n            "),
                                                new XElement("revision",
                                                    new XAttribute("Value", revision)), new XText("\n            "),
                                                new XElement("variant",
                                                    new XAttribute("Value", variant)), new XText("\n         ")));
                            bu.IsNew = false;
                            bu.RevisionOriginal = revision;
                            bu.VariantOriginal = variant;

                        }

                        cluster_baseunit.Add(new XText("\n      "));
                        XElement last_node = parent_baseunit.Descendants("BaseUnitClusters").LastOrDefault();
                        if (last_node != null)
                            last_node.AddAfterSelf(new XText("\n	  "), cluster_baseunit, new XText("\n   "));
                        else
                            parent_baseunit.Add(new XText("   "), cluster_baseunit, new XText("\n   "));
                        //parent_baseunit.Descendants("BaseUnitClusters").LastOrDefault().AddAfterSelf(new XText("   "), cluster_baseunit, new XText("\n   "));
                        //parent_baseunit.Add(new XText("   "), cluster_baseunit, new XText("\n   "));
                        BUmodel.IsNew = false;
                        if (BUmodel.ParentModel.IsNew)
                            BUmodel.ParentModel.IsNew = false;
                    }
                    else
                    {
                        foreach (BaseUnitModel bu in BUmodel.Items)
                        {
                            updateXmlRevision(XMLdoc, bu);
                        }
                    }
                }
                else
                {
                    string name = BUmodel.Name;
                    string ori_variant = BUmodel.VariantOriginal;
                    string new_variant = BUmodel.Variant;
                    string revision = BUmodel.Revision;

                    string el_tag = "baseUnits";
                    if (BUmodel.Type == BUTypeEnum.Interface)
                        el_tag = "interface";
                    else if (BUmodel.Type == BUTypeEnum.Model)
                        el_tag = "model";

                    string parent_name = BUmodel.ParentModel.Name;
                    string parent_el_tag = "processors";
                    if (BUmodel.IsDeleted)
                    {
                        try
                        {
                            XElement baseunit = (from el in XMLdoc.Descendants(el_tag)
                                                 where (string)el.Element("name").Attribute("Value") == name
                                                 select el).FirstOrDefault();
                            if (BUmodel.Next == "true")
                            {
                                new_variant = name.Split('/')[1];
                                name = name.Split('/')[0];
                                baseunit = (from el in XMLdoc.Descendants(el_tag)
                                            where (((string)el.Element("name").Attribute("Value") == name) && ((string)el.Element("variant").Attribute("Value") == new_variant))
                                            select el).FirstOrDefault();
                            }
                            if (baseunit != null)
                            {
                                baseunit.Remove();
                                XElement parent_baseunit = getParentNode(XMLdoc, BUmodel.ParentModel);
                                delete_list.Add(BUmodel.Name, BUmodel.ParentModel);
                                //BUmodel.ParentModel.RemoveItem(name);
                            }
                        }
                        catch
                        {
                            return;
                        }
                    }
                    else if (BUmodel.IsNew)
                    {
                        try
                        {
                            //XmlNodeList ProcessorNodeList = XMLdoc.GetElementsByTagName("processors");
                            //var bookTitle = XMLdoc.Descendants(parent_el_tag);
                            //foreach (var title in bookTitle)
                            //{
                            //    if (title.Element("cpuBlockName").Attribute("Value").Value == parent_name)
                            //    {
                            //        ;
                            //    }

                            //}

                            //XElement parent_baseunit = (from el in XMLdoc.Descendants(parent_el_tag)
                            //                     where (string)el.Element("cpuBlockName").Attribute("Value") == parent_name
                            //                     select el).First();
                            XElement parent_baseunit = getParentNode(XMLdoc, BUmodel.ParentModel);

                            XElement bu_node = new XElement(el_tag, new XText("\n         "),
                                                    new XElement("name",
                                                        new XAttribute("Value", name)), new XText("\n         "),
                                                    new XElement("revision",
                                                        new XAttribute("Value", revision)), new XText("\n         "),
                                                    new XElement("variant",
                                                        new XAttribute("Value", new_variant)), new XText("\n      "));

                            //parent_baseunit.Add(new XText("   "), bu_node, new XText("\n   "));
                            XElement last_node = parent_baseunit.Descendants("baseUnits").LastOrDefault();
                            if (last_node != null)
                                last_node.AddAfterSelf(new XText("\n      "), bu_node, new XText("\n   "));
                            else
                                parent_baseunit.Add(new XText("   "), bu_node, new XText("\n   "));

                            //parent_baseunit.Add(bu_node);
                            BUmodel.IsNew = false;
                            BUmodel.RevisionOriginal = revision;
                            BUmodel.VariantOriginal = new_variant;
                            if (BUmodel.ParentModel.IsNew)
                                BUmodel.ParentModel.IsNew = false;

                        }
                        catch
                        {
                            return;
                        }
                    }
                    else if (BUmodel.Revision != null)
                    {
                        if (BUmodel.Name.Contains("/"))
                        {
                            string ori_branch = BUmodel.VariantOriginal;
                            string new_branch = BUmodel.Variant;
                            string[] str_list = BUmodel.Name.Split('/');
                            name = str_list[0];
                            ori_variant = str_list[str_list.Length - 1];
                            try
                            {
                                XElement baseunit = (from el in XMLdoc.Descendants(el_tag)
                                                     where ((string)el.Element("name").Attribute("Value") == name) && ((string)el.Element("variant").Attribute("Value") == ori_variant) && ((string)el.Element("branch").Attribute("Value") == ori_branch)
                                                     select el).FirstOrDefault();
                                if (baseunit == null)
                                    baseunit = (from el in XMLdoc.Descendants(el_tag)
                                                where ((string)el.Element("name").Attribute("Value") == name) && ((string)el.Element("variant").Attribute("Value") == ori_variant)
                                                select el).FirstOrDefault();

                                if (baseunit.Element("branch") == null)
                                {
                                    string branch = "";
                                    if (new_variant == "")
                                        new_variant = ori_variant;
                                    if (new_variant.EndsWith("_GMN"))
                                    {
                                        branch = "GMN";
                                        new_variant = new_variant.Replace("_GMN", "");
                                    }
                                    else if (new_variant.Contains("KW"))
                                    {
                                        branch = GeneralCommand.GetNKNumberFromString(new_variant);
                                        new_variant = new_variant.Replace("_" + branch, "");
                                    }
                                    else if (name == "HVStorage")
                                    {
                                        branch = GeneralCommand.GetHVStorageBranch(new_variant);
                                        new_variant = GeneralCommand.GetHVStorageVariant(new_variant);
                                    }
                                    if (branch != "")
                                    {
                                        XElement branch_node = new XElement("branch", new XAttribute("Value", branch));
                                        baseunit.Element("variant").AddAfterSelf(new XText("\n         "), branch_node);
                                        baseunit.Element("variant").Attribute("Value").Value = new_variant;
                                    }
                                }
                                else
                                {
                                    baseunit.Element("branch").Attribute("Value").Value = new_variant;
                                }

                                baseunit.Element("revision").Attribute("Value").Value = revision;
                            }
                            catch
                            {
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                XElement baseunit = (from el in XMLdoc.Descendants(el_tag)
                                                     where (string)el.Element("name").Attribute("Value") == name
                                                     select el).First();
                                baseunit.Element("variant").Attribute("Value").Value = new_variant;
                                baseunit.Element("revision").Attribute("Value").Value = revision;
                            }
                            catch
                            {
                                return;
                            }
                        }
                        if (BUmodel.IsVariantChanged)
                        {
                            BUmodel.VariantOriginal = BUmodel.Variant;
                            BUmodel.IsVariantChanged = false;
                        }
                        if (BUmodel.IsRevisionChanged)
                        {
                            BUmodel.RevisionOriginal = BUmodel.Revision;
                            BUmodel.IsRevisionChanged = false;
                        }
                    }
                }
            }
        }

        private XElement getParentNode(XDocument XMLdoc, BaseUnitModel parent_model)
        {
            string tag = "processors";
            string name = parent_model.Name;
            XElement node = (from el in XMLdoc.Descendants(tag)
                             where (string)el.Element("cpuBlockName").Attribute("Value") == parent_model.Name
                             select el).FirstOrDefault();
            if (node == null)
            {
                node = new XElement(tag, new XText("\n      "),
                            new XElement("cpuBlockName",
                            new XAttribute("Value", parent_model.Name)), new XText("\n      "),
                            new XElement("ssm",
                            new XAttribute("Value", parent_model.ssm)), new XText("\n      "),
                            new XElement("cpuName",
                            new XAttribute("Value", parent_model.cpuName)), new XText("\n   "));
                XMLdoc.Descendants(tag).LastOrDefault().AddAfterSelf(new XText("\n   "), node);
            }
            return node;
        }

        private string findDeliveryPath(string app_name)
        {
            string delivery_path = @"https://lpintrae.muc:4756/svn/mdl01/Delivery/Releases";
            string path = "";
            if (_AllDeliveryAppInfo != null)
            {
                path = (from app in _AllDeliveryAppInfo
                        where (app.Name.Contains(app_name))
                        select app.Name).FirstOrDefault();
            }
            if (path != null)
            {
                path = delivery_path + "/" + path;
            }
            else
            {
                string ret = _cmd.listSvnPath(delivery_path);
                List<string> release_list = ret.Replace("/\r", "").Split('\n').ToList();
                release_list.RemoveAt(release_list.Count - 1);
                release_list = release_list.OrderByDescending(x => x.Substring(0, 3)).ToList();
                foreach (string release in release_list)
                {
                    if (release != "")
                    {
                        string sub_path = delivery_path + "/" + release + "/" + app_name;
                        string log = _cmd.checkSvnPath(sub_path); // if not found will return ""
                        if (log.Contains("Revision:"))
                        {
                            path = sub_path;
                            break;
                        }
                    }
                }
            }
            return path;
        }

        private string getArtifactoryPath(string app_name)
        {
            string delivery_path = @"https://common.artifactory.cc.bmwgroup.net/artifactory/hil-testing-xil-models/releases/";
            string path = "";
            string name = app_name;
            if (name.Contains(";"))
                name = name.Split(';')[0].Split('/')[1];
            if ((_AllDeliveryAppInfo != null) && !((_AllDeliveryAppInfo.Count == 1) && _AllDeliveryAppInfo[0].Name.Contains("Please select work path")))
            {
                path = (from app in _AllDeliveryAppInfo
                        where (app.Name.Contains(name))
                        select app.Name.Replace(";", "/")).FirstOrDefault();
                if (path != null)
                {
                    path = delivery_path + path;
                }
            }
            else
            {
                // find delivery artifact path
                path = findArtifactoryPath(app_name);

            }
            return path;
        }

        private string findArtifactoryPath(string app_name)
        {
            string delivery_path = @"https://common.artifactory.cc.bmwgroup.net/artifactory/hil-testing-xil-models/releases/";
            string app_path = "";
            List<string> ReleaseList = new List<string>();
            string result = _cmd.getArtifactoryString(delivery_path);
            foreach (string path in GeneralCommand.GetReleasePathFromString(result))
            {
                string dir = path;
                string sub_path = delivery_path + dir;
                dir = dir.Replace("/", "");
                ReleaseList.Add(dir);
            }
            ReleaseList = ReleaseList.OrderByDescending(x => x.Substring(0, 3)).ToList();
            string nk_num = GeneralCommand.GetNKNumberFromString(app_name);
            foreach (string release in ReleaseList)
            {
                string release_path = delivery_path + release + "/";
                result = _cmd.getArtifactoryString(release_path);
                if (result.Contains(nk_num))
                {
                    foreach (string name in GeneralCommand.GetReleasePathFromString(result))
                    {
                        if (name.Contains(app_name))
                        {
                            string sub_path = release_path + name;
                            List<string> release_list = GeneralCommand.GetReleaseNumberFromString(_cmd.getArtifactoryString(sub_path));
                            release_list.Reverse();
                            foreach (string release_date in release_list)
                            {
                                string sub_release_path = sub_path + release_date + "/";
                                result = _cmd.getArtifactoryString(sub_release_path);
                                if (result.Contains("IPM_HIL_App.zip"))
                                {
                                    app_path = sub_path + release_date;
                                    break;
                                }
                            }
                        }
                        if (app_path != "")
                            break;
                    }
                }
                if (app_path != "")
                    break;
            }

            return app_path;
        }

        private string checkoutDeliveryAppModelConfig(string app_path)
        {
            string[] path_list = app_path.Split(new string[] { "/Releases/" }, StringSplitOptions.None);
            string file_path = "temp/" + path_list[path_list.Length - 1];
            if (!Directory.Exists(file_path))
            {
                Directory.CreateDirectory(file_path);
            }
            string file_config = file_path + "/ModelConfigBuild.xml";
            string config_path = app_path + "/ModelConfigBuild.xml";
            if (!_cmd.checkSvnPath(config_path).Contains("Revision:"))
            {
                file_config = file_path + "/ModelConfig.xml";
                config_path = app_path + "/ModelConfig.xml";

            }
            string log = _cmd.exportSvn(config_path, file_config);
            //if (log == "")
            //    file_path = app_path;

            return file_config;
        }

        private string downloadArtifactoryApp(string app_path)
        {
            string[] path_list = app_path.Split(new string[] { "/releases/" }, StringSplitOptions.None);
            string file_path = "temp/" + path_list[path_list.Length - 1];
            if (!Directory.Exists(file_path))
            {
                Directory.CreateDirectory(file_path);
            }
            string file_config = file_path + "/ModelConfigBuild.xml";
            if (!File.Exists(file_config))
            {
                string zip_file = app_path + "/IPM_HIL_App.zip";
                string file_name = file_path + "/IPM_HIL_App.zip";
                _cmd.downloadArtifactory(zip_file, file_name);
                string config_path = file_name.Replace(".zip", "");
                if (!Directory.Exists(config_path))
                {
                    Directory.CreateDirectory(config_path);
                }
                ZipFile.ExtractToDirectory(file_name, file_path);
            }

            return file_config;
        }

        private bool IsAppNameCorrectFormat(string appname)
        {
            if (appname == "")
                return true;
            else
                return Regex.IsMatch(appname, "[a-zA-Z0-9*]");
        }

        private bool IsRevisionCorrectFormat(string revision)
        {
            return Regex.IsMatch(revision, "[0-9]");
        }

        #endregion

        #region Public Command
        public void updateWorkPath(string workpath)
        {
            _WorkPath = workpath;
            string path = Path.Combine(workpath, "Anwendung");
            if (Directory.Exists(path))
            {
                //var t_get_config = Task.Run(() => getAllApplication(path));
                getAllApplication(path);
            }
            else
            {
                _AllAppInfo = new ObservableCollection<AppInfo>() { new AppInfo() { Name = "Cannot find Anwendung floder" } };
            }
            FolderList = new List<string> { "Anwendung", "Delivery" };
            SelectedFolderIndex = 0;
            SelectedFolder = "Anwendung";
        }

        public void updateReleaseList(ObservableCollection<DeliveryAppModel> release_list)
        {
            if (release_list != null)
            {
                _AllDeliveryAppInfo = new ObservableCollection<AppInfo>();
                foreach (DeliveryAppModel release in release_list)
                {
                    foreach (var app in release.Items)
                    {
                        _AllDeliveryAppInfo.Add(new AppInfo() { Name = release.Name + "/" + app.Name + ";" + app.Date });
                    }
                }
            }
        }

        public void mergeModelConfig(List<BaseUnitModel> selectedBaseUnits, List<AppInfo> selectedAppNames)
        {
            var missingBaseUnits = new List<string>();

            foreach (var target in selectedAppNames)
            {
                try
                {
                    var existingModelConfig = loadModelConfig(target);
                    if (existingModelConfig == null)
                    {
                        // Handle the case where model config does not exist
                        // You might want to create a default config here
                        existingModelConfig = new ObservableCollection<BaseUnitModel>();
                    }

                    bool isUpdated = false;
                    foreach (var baseUnit in selectedBaseUnits)
                    {
                        var existingBaseUnit = existingModelConfig.FirstOrDefault(p => p.Items.Any(i => i.Name == baseUnit.Name));
                        if (existingBaseUnit != null)
                        {
                            existingBaseUnit.Revision = baseUnit.Revision;
                            existingBaseUnit.Variant = baseUnit.Variant;
                            isUpdated = true;
                        }
                        else
                        {
                            missingBaseUnits.Add(baseUnit.Name);
                        }
                    }

                    if (isUpdated)
                    {
                        saveModelConfig(existingModelConfig, target);
                    }
                    else
                    {
                        // ไม่มี BaseUnit ที่ตรงกัน
                        MessageBox.Show($"ไม่มี BaseUnit ที่ตรงกันสำหรับ {target.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error merging model config for {target.Name}: {ex.Message}");
                }
            }

            if (missingBaseUnits.Count > 0)
            {
                MessageBox.Show($"ไม่มี BaseUnit ต่อไปนี้: {string.Join(", ", missingBaseUnits)}");
            }
        }

        public void saveModelConfig(ObservableCollection<BaseUnitModel> modelConfig, AppInfo appInfo)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(appInfo.ConfigPath);

                var processorsNode = xmlDoc.SelectSingleNode("//processors") as XmlElement;

                if (processorsNode == null)
                {
                    processorsNode = xmlDoc.CreateElement("processors") as XmlElement;
                    xmlDoc.DocumentElement.AppendChild(processorsNode);
                }

                foreach (var baseUnit in modelConfig)
                {
                    var baseUnitNode = xmlDoc.SelectSingleNode($"//baseUnits[@Name='{baseUnit.Name}']") as XmlElement;
                    if (baseUnitNode == null)
                    {
                        baseUnitNode = xmlDoc.CreateElement("baseUnits") as XmlElement;
                        processorsNode.AppendChild(baseUnitNode);
                    }

                    var nameNode = xmlDoc.SelectSingleNode($"//baseUnits[@Name='{baseUnit.Name}']/name") as XmlElement;
                    if (nameNode == null)
                    {
                        nameNode = xmlDoc.CreateElement("name") as XmlElement;
                        baseUnitNode.AppendChild(nameNode);
                    }
                    nameNode.SetAttribute("Value", baseUnit.Name);

                    var revisionNode = xmlDoc.SelectSingleNode($"//baseUnits[@Name='{baseUnit.Name}']/revision") as XmlElement;
                    if (revisionNode == null)
                    {
                        revisionNode = xmlDoc.CreateElement("revision") as XmlElement;
                        baseUnitNode.AppendChild(revisionNode);
                    }
                    revisionNode.SetAttribute("Value", baseUnit.Revision);

                    var variantNode = xmlDoc.SelectSingleNode($"//baseUnits[@Name='{baseUnit.Name}']/variant") as XmlElement;
                    if (variantNode == null)
                    {
                        variantNode = xmlDoc.CreateElement("variant") as XmlElement;
                        baseUnitNode.AppendChild(variantNode);
                    }
                    variantNode.SetAttribute("Value", baseUnit.Variant);
                }

                xmlDoc.Save(appInfo.ConfigPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving model config for {appInfo.Name}: {ex.Message}");
            }
        }


        #endregion

    }
}
