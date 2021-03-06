﻿using System;
using System.Diagnostics;
using System.Linq;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Utility.Interfaces;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Launcher.Utility
{
    /// <summary>
    /// Class that provides automatic injection to applications with the feature enabled.
    /// </summary>
    public class AutoInjector
    {
        private MainPageViewModel _mainPageViewModel;
        private IProcessWatcher   _processWatcher;

        /* Construction */
        public AutoInjector(MainPageViewModel mainPageViewModel)
        {
            _mainPageViewModel = mainPageViewModel;
            _processWatcher    = App.IsElevated ? (IProcessWatcher) new WmiProcessWatcher() : ProcessWatcher.Instance;
            _processWatcher.OnNewProcess += ProcessWatcherOnOnNewProcess;
        }

        /* Implementation */
        private void ProcessWatcherOnOnNewProcess(Process newProcess)
        {
            try
            {
                string fullPath = newProcess.GetExecutablePath();
                var config = _mainPageViewModel.Applications.FirstOrDefault(x => string.Equals(x.Config.AppLocation, fullPath, StringComparison.OrdinalIgnoreCase));
                if (config != null && config.Config.AutoInject)
                {
                    var appInjector = new ApplicationInjector(newProcess);
                    appInjector.Inject();
                }
            }
            catch (Exception)
            {
                // Ignored
            }
        }
    }
}
