using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ReactNative.Managed;
using Newtonsoft.Json.Linq;
using Windows.Security.Credentials;

namespace RNSensitiveInfo
{
    [ReactModule]
    public sealed class RNSensitiveInfoModule
    {

        [ReactMethod]
        public async void getItem(string key, JObject options, IReactPromise<string> promise)
        {
            if (string.IsNullOrEmpty(key))
            {
                promise.Reject(new ReactError { Message = "KEY IS REQURIED" });
            }

            try
            {
                string name = sharedPreferences(options);

                var credential = prefs(name, key);
                if (credential != null)
                {
                    promise.Resolve(credential.Password);
                }
                else
                {
                    throw new Exception("credential NOT FOUND");
                }
            }
            catch (Exception ex)
            {
                promise.Reject(new ReactError { Message = "ERROR GET : " + ex.Message });
            }
        }

        [ReactMethod]
        public async void setItem(string key, string value, JObject options, IReactPromise<string> promise)
        {
            if (string.IsNullOrEmpty(key))
            {
                promise.Reject(new ReactError { Message = "KEY IS REQURIED" });
                return;
            }

            try
            {
                string name = sharedPreferences(options);
                putExtra(key, value, name);

                promise.Resolve(value);
            }
            catch (Exception ex)
            {
                promise.Reject(new ReactError { Message = "ERROR SET : " + ex.Message });
            }
        }

        [ReactMethod]
        public async void deleteItem(string key, JObject options, IReactPromise<string> promise)
        {
            if (string.IsNullOrEmpty(key))
            {
                promise.Reject(new ReactError { Message = "KEY IS REQURIED" });
                return;
            }

            try
            {
                string name = sharedPreferences(options);
                var vault = new PasswordVault();
                var credential = vault.Retrieve(name, key);
                vault.Remove(credential);

                promise.Resolve(key);
            }
            catch (Exception ex)
            {
                promise.Reject(new ReactError { Message = "ERROR DELETE : " + ex.Message });
            }
        }

        [ReactMethod]
        public async void getAllItems(JObject options, IReactPromise<string> promise)
        {
            try
            {
                string name = sharedPreferences(options);
                Dictionary<string, string> result = new Dictionary<string, string>();

                var vault = new PasswordVault();
                var credentialList = vault.FindAllByResource(name);
                if (credentialList.Count > 0)
                {
                    credentialList.ToList().ForEach(item =>
                    {
                        var credential = prefs(name, item.UserName);
                        result[item.UserName] = credential.Password;
                    });
                    
                }

                JObject json = JObject.FromObject(result);
                promise.Resolve("test");
            }
            catch (Exception ex)
            {
                promise.Reject(new ReactError { Message = "ERROR GET ALL : " + ex.Message });
            }
        }

        private PasswordCredential prefs(string name, string key)
        {
            PasswordCredential credential = null;

            var vault = new PasswordVault();
            return vault.Retrieve(name, key);
        }

        private void putExtra(string key, string value, string name)
        {
            try
            {
                var vault = new PasswordVault();
                vault.Add(new PasswordCredential(name, key, value));
            }
            catch (Exception e)
            {
                throw new Exception("ERROR SAVE PasswordVault " + e.Message);
            }
            
        }


        private string sharedPreferences(JObject options)
        {
            string name = options.Value<string>("sharedPreferencesName") ?? "shared_preferences";
            if (name == null)
            {
                name = "shared_preferences";
            }
            return name;
        }

    }
}
