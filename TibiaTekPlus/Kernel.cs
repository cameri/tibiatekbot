﻿using System;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using Tibia;
using Tibia.Util;
using TibiaTekPlus;
using TibiaTekPlus.Plugins;



namespace TibiaTekPlus
{
    public partial class Kernel
    {
        #region Events

        /// <summary>
        /// Prototype for plugin notifications.
        /// </summary>
        /// <param name="plugin"></param>
        public delegate void PluginNotification(Plugin plugin);

        /// <summary>
        /// Event fired when a plugin is loaded.
        /// </summary>
        public PluginNotification PluginLoaded;

        #endregion


        #region Forms

        /// <summary>
        /// Provides access to the Plug-ins Form
        /// </summary>
        public PluginManagerForm pluginsForm;

        #endregion

        #region Objects/Variables

        Tibia.Objects.Client client;
        public Tibia.Util.Timer timer;
        public PluginCollection plugins;

        #endregion

        /// <summary>
        /// Constructor of the kernel.
        /// </summary>
        public Kernel()
        {
            /* Instantiate forms */
            pluginsForm = new PluginManagerForm();

            /* Instatiate timers */
            timer = new Tibia.Util.Timer(3000, false);
            timer.OnExecute += new Tibia.Util.Timer.TimerExecution(timer_OnExecute);

            /* Plug-in related */
            plugins = new PluginCollection();
        }

        /// <summary>
        /// Starts the kernel, enables the use of plug-ins. This function is called when the main form is ready.
        /// </summary>
        public void Enable()
        {
            foreach (IPlugin plugin in plugins)
            {
                if (plugin.State == PluginState.Running)
                {
                    try
                    {
                        plugin.Enable();
                    }
                    catch (NotImplementedException)
                    {
                        // Do nothing
                    }
                }
            }
        }

        /// <summary>
        /// Stops the kernel, stops all plug-ins currently running. This function is called when disconnected or exiting.
        /// </summary>
        public void Disable()
        {
            foreach (IPlugin plugin in plugins)
            {
                if (plugin.State == PluginState.Running)
                {
                    try
                    {
                        plugin.Disable();
                    }
                    catch (NotImplementedException)
                    {
                        // Do nothing
                    }
                }
            }
        }

        /// <summary>
        /// Pauses the kernel, all plug-ins running are paused
        /// </summary>
        public void Pause()
        {
            foreach (IPlugin plugin in plugins)
            {
                if (plugin.State == PluginState.Running)
                {
                    try
                    {
                        plugin.Pause();
                    }
                    catch (NotImplementedException)
                    {
                        // Do nothing
                    }
                }
            }
        }

        /// <summary>
        /// Resumes the kernel, all paused plug-ins are resumed
        /// </summary>
        public void Resume()
        {
            foreach (IPlugin plugin in plugins)
            {
                if (plugin.State == PluginState.Paused)
                {
                    try
                    {
                        plugin.Resume();
                    }
                    catch (NotImplementedException)
                    {
                        // Do nothing
                    }
                }
            }
        }

        /// <summary>
        /// Gets a reference to the client object.
        /// </summary>
        public Tibia.Objects.Client Client
        {
            get
            {
                return client;
            }
        }

        public int PerformPluginUninstallation()
        {
            int count = 0;
            XmlDocument document = new XmlDocument();
            document.Load("TibiaTekPlus.Plugins.xml");
            string filepath;
            foreach (XmlElement element in document["plugins"]["pending"]["uninstall"])
            {
                filepath = Path.Combine(Application.StartupPath, element.GetAttribute("fullname") + ".dll");
                try
                {
                    if (File.Exists(filepath))
                    {
                        File.SetAttributes(filepath, FileAttributes.Normal);
                        File.Delete(filepath);
                        document["plugins"]["pending"]["uninstall"].RemoveChild(element);
                        count++;
                    }
                } catch (Exception){
                }
            }
            document.Save("TibiaTekPlus.Plugins.xml");
            return count;
        }

        public int PerformPluginInstallation()
        {
            int count = 0;
            XmlDocument document = new XmlDocument();
            document.Load("TibiaTekPlus.Plugins.xml");
            string filepath;
            foreach (XmlElement element in document["plugins"]["pending"]["install"])
            {
                filepath = Path.Combine(Application.StartupPath, element.GetAttribute("fullname") + ".dll");
                if (File.Exists(filepath))
                {
                    XmlElement newelem = (XmlElement)element.Clone();
                    document["plugins"]["pending"]["install"].RemoveChild(element);
                    document["plugins"]["installed"].AppendChild(newelem);
                    count++;
                }
                else
                {
                    MessageBox.Show("Unable to install the following plug-in:\nTitle: " + element["title"] + ".\nAuthor: " + element["author"] + ".\nReason: The file '" + element.GetAttribute("fullname") + ".dll' was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    document["plugins"]["pending"]["install"].RemoveChild(element);
                }
            }
            document.Save("TibiaTekPlus.Plugins.xml");
            return count;
        }

        public int InstalledPluginsCount
        {
            get
            {
                XmlDocument document = new XmlDocument();
                document.Load("TibiaTekPlus.Plugins.xml");
                return document["plugins"]["installed"].ChildNodes.Count;
            }
        }

        public int LoadPlugins()
        {
            int count = 0;
            XmlDocument document = new XmlDocument();
            document.Load("TibiaTekPlus.Plugins.xml");
            string path;
            foreach (XmlElement element in document["plugins"]["installed"])
            {
                path = Path.Combine(Application.StartupPath, element.GetAttribute("fullname") + ".dll");
                if (File.Exists(path))
                {
                    Plugin plugin = (Plugin)Activator.CreateInstance(Type.GetType(element["assemblyQualifiedName"].InnerText));
                    plugins.Add(plugin);
                    count++;
                    if (PluginLoaded != null)
                    {
                        PluginLoaded.Invoke(plugin);
                    }
                }
                else
                {
                    MessageBox.Show("Unable to load the following plug-in:\nTitle: " + element["title"] + ".\nAuthor: " + element["author"] + ".\nReason: The file '" + element.GetAttribute("fullname") + ".dll' was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return count;
        }

    }

    /// <summary>
    /// Defines the states that the kernel can take.
    /// </summary>
    public enum KernelState
    {
        Stopped,
        Running,
        Paused
    }
}