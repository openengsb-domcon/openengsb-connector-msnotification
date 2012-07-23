namespace Org.OpenEngSB.Connector.MSNotification
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using Org.OpenEngSB.Connector.MSNotification.Common;
    using Org.OpenEngSB.Connector.MSNotification.Connector;
    using Org.OpenEngSB.Connector.MSNotification.GUI.Controls;
    using Org.OpenEngSB.Domain.NotificationInterfaces;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CustomBalloon _balloon = new CustomBalloon();

        public MainWindow()
        {
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Closed += new EventHandler(MainWindow_Closed);
            NotificationDomainConnector.Instance.Notified += new EventHandler<SimpleEventArgs<Notification>>(Instance_Notified);
            _balloon.MouseLeftButtonDown += new MouseButtonEventHandler(_balloon_MouseLeftButtonDown);
            StateChanged += new EventHandler(MainWindow_StateChanged);

            InitializeComponent();
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
                NotifyIcon.Icon = GUI.Images.Resources.openEngSB;
            }
        }

        void _balloon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = System.Windows.WindowState.Normal;
            Activate();
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            NotificationDomainConnector.Instance.Dispose();
        }

        void Instance_Notified(object sender, SimpleEventArgs<Notification> e)
        {
            _balloon.DataContext = e.Data;
            NotifyIcon.ShowCustomBalloon(_balloon, PopupAnimation.Fade, 10000);

            if (!IsActive)
                NotifyIcon.Icon = GUI.Images.Resources.openEngSB_info;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= new RoutedEventHandler(MainWindow_Loaded);

            settingsView.DataContext = Settings.Instance;
            lstNotifications.ItemsSource = NotificationDomainConnector.Instance.Notifications;

            CommandBindings.Add(Settings.Instance.SaveBinding);
            CommandBindings.Add(Settings.Instance.ResetBinding);

            if (Environment.GetCommandLineArgs().Contains("/m"))
            {
                this.WindowState = System.Windows.WindowState.Minimized;
            }
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo(e.Parameter.ToString());
            Process p = new Process() { StartInfo = psi };

            p.Start();
        }

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var param = e.Parameter as string;

            e.CanExecute = !string.IsNullOrWhiteSpace(param);
            e.Handled = true;
        }
    }
}
