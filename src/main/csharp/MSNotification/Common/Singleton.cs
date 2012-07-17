namespace Org.OpenEngSB.Connector.MSNotification.Common
{
    public abstract class Singleton<T>
        where T : Singleton<T>, new()
    {
        private static T _instance = null;

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
