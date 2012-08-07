/***
 * Licensed to the Austrian Association for Software Tool Integration (AASTI)
 * under one or more contributor license agreements. See the NOTICE file
 * distributed with this work for additional information regarding copyright
 * ownership. The AASTI licenses this file to you under the Apache License,
 * Version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ***/

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
        private const string autoStartValue = "OpenEngSBWindowsService";
        private static readonly string settingsPath;

        static Settings()
        {
            settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\MSNotification\settings.xml";
        }

        private IDictionary<string, object> settingsChanged = new Dictionary<string, object>();
        private RegistryKey autoStartKey;

        private bool _autoStart;
        private string _version;
        private string _destination;

        protected override void InitializeCorrectInstance(ref Settings instance)
        {
            instance.Version = DefaultVersion;

            try
            {
                FileInfo fi = new FileInfo(settingsPath);
                if (!fi.Directory.Exists)
                    Directory.CreateDirectory(fi.DirectoryName);

                XmlSerializer ser = new XmlSerializer(GetType());

                using (var sr = new StreamReader(settingsPath))
                {
                    instance = (Settings)ser.Deserialize(sr);
                }
            }
            catch (FileNotFoundException fex)
            {
                // file not found is ok, most probably no settings are there yet
                logger.Warn("Couldn't load the settings-file.", fex);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to load settings.", ex);
            }
        }

        protected override void Initialize()
        {
            autoStartKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            _autoStart = (autoStartKey.GetValue(autoStartValue) != null);

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

            foreach (var item in settingsChanged)
            {
                t.GetProperty(item.Key).SetValue(this, item.Value, null);
            }

            settingsChanged.Clear();
        }

        void instance_PrePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!settingsChanged.ContainsKey(e.PropertyName))
                settingsChanged.Add(e.PropertyName, GetType().GetProperty(e.PropertyName).GetValue(this, null));
        }

        void SettingsBindings_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!e.Handled)
                e.CanExecute = settingsChanged.Count > 0;
        }

        void SaveBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_autoStart)
            {
                string commandLine = Environment.CommandLine;

                if (!Environment.GetCommandLineArgs().Contains("/m"))
                    commandLine += " /m";

                autoStartKey.SetValue(autoStartValue, commandLine);
            }
            else
                autoStartKey.DeleteValue(autoStartValue, false);

            try
            {
                XmlSerializer ser = new XmlSerializer(GetType());

                using (var sw = new StreamWriter(settingsPath))
                {
                    ser.Serialize(sw, this);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to save settings.", ex);
            }

            settingsChanged.Clear();
        }

        [XmlIgnore]
        public CommandBinding SaveBinding { get; private set; }

        [XmlIgnore]
        public CommandBinding ResetBinding { get; private set; }

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
