﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.ReactNative.Managed;
using Microsoft.ReactNative;
using Windows.Security.Credentials;

namespace RNSensitiveInfo
{
    [ReactModule("RNSensitiveInfo")]
    class RNSensitiveInfoModule
    {
        
        [ReactMethod]
        public void getItem(string key, JObject options, IReactPromise<string> result)
        {
            if (string.IsNullOrEmpty(key))
            {
                result.Reject(new ReactError { Exception = new ArgumentNullException("KEY IS REQUIRED") });
                return;
            }

            try
            {
                string name = sharedPreferences(options);

                var credential = prefs(name, key);
                if (credential != null)
                {
                    result.Resolve(credential.Password);
                }
                else
                {
                    throw new Exception("credential NOT FOUND");
                    // How about reject exception here?
                }
            }
            catch (Exception ex)
            {
                result.Reject(new ReactError { Message = "ERROR GET : " + ex.Message });
            }
        }

        [ReactMethod]
        public void setItem(string key, string value, JObject options, IReactPromise<string> result)
        {
            if (string.IsNullOrEmpty(key))
            {
                result.Reject(new ReactError { Exception = new ArgumentNullException("KEY IS REQUIRED") });
                return;
            }

            try
            {
                string name = sharedPreferences(options);
                putExtra(key, value, name);

                result.Resolve(value);
            }
            catch (Exception ex)
            {
                result.Reject(new ReactError { Message = "ERROR SET : " + ex.Message });
            }
        }

        [ReactMethod]
        public void deleteItem(string key, JObject options, IReactPromise<string> result)
        {
            if (string.IsNullOrEmpty(key))
            {
                result.Reject(new ReactError { Exception = new ArgumentNullException("KEY IS REQUIRED") });
                return;
            }

            try
            {
                string name = sharedPreferences(options);
                var vault = new PasswordVault();
                var credential = vault.Retrieve(name, key);
                vault.Remove(credential);

                result.Resolve(key);
            }
            catch (Exception ex)
            {
                result.Reject(new ReactError { Message = "ERROR DELETE : " + ex.Message });
            }
        }

        public void getAllItems(JObject options, IReactPromise<Dictionary<string, string>> result)
        {
            try
            {
                string name = sharedPreferences(options);
                Dictionary<string, string> ret = new Dictionary<string, string>();

                var vault = new PasswordVault();
                var credentialList = vault.FindAllByResource(name);
                if (credentialList.Count > 0)
                {
                    credentialList.ToList().ForEach(item =>
                    {
                        var credential = prefs(name, item.UserName);
                        ret[item.UserName] = credential.Password;
                    });

                }
                result.Resolve(ret);
            }
            catch (Exception ex)
            {
                result.Reject(new ReactError { Message = "ERROR GET ALL : " + ex.Message });
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
            string name = options.Value<string>("sharedPreferencesName") ?? "keystore";
            if (name == null)
            {
                name = "keystore";
            }
            return name;
        }

    }
}
