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

namespace Org.OpenEngSB.Connector.MSNotification.Common
{
    using log4net;
    using Org.Openengsb.Loom.CSharp.Bridge.Implementation;

    public abstract class SingletonConnector<T> : RegistrationFunctions
        where T : SingletonConnector<T>, new()
    {
        protected static readonly ILog logger = LogManager.GetLogger(typeof(T));
        private static T _instance = null;

        protected SingletonConnector() : base(logger) { }

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
