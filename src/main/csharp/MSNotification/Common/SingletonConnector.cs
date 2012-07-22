namespace Org.OpenEngSB.Connector.MSNotification.Common
{
    using log4net;
    using Org.Openengsb.Loom.CSharp.Bridge.Implementation;

    public abstract class SingletonConnector<T> : RegistrationFunctions
        where T : SingletonConnector<T>, new()
    {
        protected static readonly ILog _logger = LogManager.GetLogger(typeof(T));
        private static T _instance = null;

        protected SingletonConnector() : base(_logger) { }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                    _instance.InitializeCorrectInstance(ref _instance);
                    _instance.Initialize();
                }

                return _instance;
            }
        }

        protected virtual void InitializeCorrectInstance(ref T instance)
        {

        }

        protected virtual void Initialize()
        {

        }
    }
}
