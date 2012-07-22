﻿namespace Org.OpenEngSB.Connector.MSNotification.Common
{
    using log4net;

    public abstract class Singleton<T>
        where T : Singleton<T>, new()
    {
        protected static readonly ILog _logger = LogManager.GetLogger(typeof(T));
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
