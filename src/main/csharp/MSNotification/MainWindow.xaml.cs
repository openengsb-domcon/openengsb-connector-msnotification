namespace Org.OpenEngSB.Connector.MSNotification
{
    using System;
    using System.Windows;
    using System.Windows.Controls.Primitives;
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
            NotificationDomainConnector.Instance.Notified += new EventHandler<SimpleEventArgs<Notification>>(Instance_Notified);

            InitializeComponent();
        }

        void Instance_Notified(object sender, SimpleEventArgs<Notification> e)
        {
            _balloon.DataContext = e.Data;
            NotifyIcon.ShowCustomBalloon(_balloon, PopupAnimation.Fade, 10000);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= new RoutedEventHandler(MainWindow_Loaded);

            settingsView.DataContext = Settings.Instance;

            CommandBindings.Add(Settings.Instance.SaveBinding);
            CommandBindings.Add(Settings.Instance.ResetBinding);
        }
    }
}
