namespace Org.OpenEngSB.Connector.MSNotification
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Input;
    using System.Xml.Serialization;
    using Microsoft.Win32;
    using Org.OpenEngSB.Connector.MSNotification.Common;

    public class Settings : Singleton<Settings>, INotifyPropertyChanged
    {
        private const string DefaultVersion = "3.0.0";
        private static readonly string _settingsPath;

        static Settings()
        {
            _settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\MSNotification\settings.xml";
        }

        private IDictionary<string, object> _settingsChanged = new Dictionary<string, object>();

        protected override void InitializeCorrectInstance(ref Settings instance)
        {
            instance.Version = DefaultVersion;

            try
            {
                FileInfo fi = new FileInfo(_settingsPath);
                if (!fi.Directory.Exists)
                    Directory.CreateDirectory(fi.DirectoryName);

                XmlSerializer ser = new XmlSerializer(GetType());

                using (var sr = new StreamReader(_settingsPath))
                {
                    instance = (Settings)ser.Deserialize(sr);
                }
            }
            catch (FileNotFoundException fex)
            {
                // file not found is ok, most probably no settings are there yet
                _logger.Warn("Couldn't load the settings-file.", fex);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load settings.", ex);
            }
        }

        protected override void Initialize()
        {
            _autoStartKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            _autoStart = (_autoStartKey.GetValue(_autoStartValue) != null);

            SaveBinding = new CommandBinding(ApplicationCommands.Save);
            SaveBinding.Executed += new ExecutedRoutedEventHandler(SaveBinding_Executed);
            SaveBinding.CanExecute += new CanExecuteRoutedEventHandler(SettingsBindings_CanExecute);

            ResetBinding = new CommandBinding(ApplicationCommands.Redo);
            ResetBinding.Executed += new ExecutedRoutedEventHandler(ResetBinding_Executed);
            ResetBinding.CanExecute += new CanExecuteRoutedEventHandler(SettingsBindings_CanExecute);

            PrePropertyChanged += new PropertyChangedEventHandler(instance_PrePropertyChanged);
        }

        void ResetBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Type t = GetType();

            foreach (var item in _settingsChanged)
            {
                t.GetProperty(item.Key).SetValue(this, item.Value, null);
            }

            _settingsChanged.Clear();
        }

        void instance_PrePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_settingsChanged.ContainsKey(e.PropertyName))
                _settingsChanged.Add(e.PropertyName, GetType().GetProperty(e.PropertyName).GetValue(this, null));
        }

        void SettingsBindings_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!e.Handled)
                e.CanExecute = _settingsChanged.Count > 0;
        }

        void SaveBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_autoStart)
            {
                string commandLine = Environment.CommandLine;

                if (!Environment.GetCommandLineArgs().Contains("/m"))
                    commandLine += " /m";

                _autoStartKey.SetValue(_autoStartValue, commandLine);
            }
            else
                _autoStartKey.DeleteValue(_autoStartValue, false);

            try
            {
                XmlSerializer ser = new XmlSerializer(GetType());

                using (var sw = new StreamWriter(_settingsPath))
                {
                    ser.Serialize(sw, this);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to save settings.", ex);
            }

            _settingsChanged.Clear();
        }

        [XmlIgnore]
        public CommandBinding SaveBinding { get; private set; }

        [XmlIgnore]
        public CommandBinding ResetBinding { get; private set; }

        private string _version;

        public string Version
        {
            get { return _version; }

            set
            {
                if (_version != value)
                {
                    PrePropertyChanged.SafeInvoike(this, "Version");
                    _version = value;
                    PropertyChanged.SafeInvoike(this, "Version");
                }
            }
        }


        private string _destination;

        public string Destination
        {
            get { return _destination; }

            set
            {
                if (_destination != value)
                {
                    PrePropertyChanged.SafeInvoike(this, "Destination");
                    _destination = value;
                    PropertyChanged.SafeInvoike(this, "Destination");
                }
            }
        }

        #region AutoStart in the registry

        private const string _autoStartValue = "OpenEngSBWindowsService";
        private RegistryKey _autoStartKey;

        private bool _autoStart;

        [XmlIgnore]
        public bool AutoStart
        {
            get
            {
                return _autoStart;
            }

            set
            {
                if (_autoStart != value)
                {
                    PrePropertyChanged.SafeInvoike(this, "AutoStart");
                    _autoStart = value;
                    PropertyChanged.SafeInvoike(this, "AutoStart");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public event PropertyChangedEventHandler PrePropertyChanged;
    }
}
