﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Launcher
{
    /// <summary>
    /// Contains all code for setting up the running of Reloaded at startup.
    /// </summary>
    public static class Setup
    {
        #region XAML String Resource Constants
        // ReSharper disable InconsistentNaming
        private const string XAML_SplashCreatingDefaultConfig = "SplashCreatingDefaultConfig";
        private const string XAML_SplashCleaningConfigurations = "SplashCleaningConfigurations";
        private const string XAML_SplashPreparingViewModels = "SplashPreparingViewModels";
        private const string XAML_SplashLoadCompleteIn = "SplashLoadCompleteIn";
        // ReSharper restore InconsistentNaming
        #endregion XAML String Resource Constants

        private static bool _loadExecuted = false;

        /// <summary>
        /// Sets up the overall application state for either running or testing.
        /// Note: This method uses Task.Delay for waiting the minimum splash delay without blocking the thread, it is synchronous.
        /// </summary>
        /// <param name="getText">A function that accepts the name of a XAML resource and retrieves the text to update with.</param>
        /// <param name="updateText">A function that updates the visible text onscreen.</param>
        /// <param name="minimumSplashDelay">Minimum amount of time to wait to complete the loading process.</param>
        public static async Task SetupApplication(Func<string, string> getText, Action<string> updateText, int minimumSplashDelay)
        {
            if (!_loadExecuted)
            {
                // Benchmark load time.
                _loadExecuted = true;
                Stopwatch watch = new Stopwatch();
                watch.Start();

                // Setting up localization.
                SetupLocalization();

                // Make Default Config if Necessary.
                updateText(getText(XAML_SplashCreatingDefaultConfig));
                CreateNewConfigIfNotExist();

                // Cleaning up App/Loader/Mod Configurations
                // This is unused for cases when e.g. user installs mod before game or lacks a dependency.
                /*
                updateText(getText(XAML_SplashCleaningConfigurations));
                CleanupConfigurations();
                */

                // Preparing viewmodels.
                updateText(getText(XAML_SplashPreparingViewModels));
                SetupViewModels();

                // Wait until splash screen time.
                updateText($"{getText(XAML_SplashLoadCompleteIn)} {watch.ElapsedMilliseconds}ms");
                
                while (watch.ElapsedMilliseconds < minimumSplashDelay)
                {
                    await Task.Delay(100);
                }
            }
        }

        /// <summary>
        /// Sets up localization for the system language.
        /// </summary>
        private static void SetupLocalization()
        {
            // Set language dictionary.
            var langDict = new ResourceDictionary();
            string culture = Thread.CurrentThread.CurrentCulture.ToString();
            string languageFilePath = AppDomain.CurrentDomain.BaseDirectory + $"/Languages/{culture}.xaml";
            if (File.Exists(languageFilePath))
                langDict.Source = new Uri(languageFilePath, UriKind.Absolute);

            Application.Current.Resources.MergedDictionaries.Add(langDict);
        }

        /// <summary>
        /// Creates a new configuration if the config does not exist.
        /// </summary>
        private static void CreateNewConfigIfNotExist()
        {
            var configReader = new LoaderConfigReader();
            if (!configReader.ConfigurationExists())
                configReader.WriteConfiguration(new LoaderConfig());
        }

        /// <summary>
        /// Sets up viewmodels to be used in the individual mod loader pages.
        /// </summary>
        private static void SetupViewModels()
        {
            IoC.GetConstant<MainPageViewModel>();
            IoC.GetConstant<AddAppViewModel>(); // Consumes MainPageViewModel, make sure it goes after it.
            IoC.GetConstant<ManageModsViewModel>();
        }

        /// <summary>
        /// Cleans up App/Loader/Mod Configurations from nonexisting
        /// references such as removed mods.
        /// </summary>
        private static void CleanupConfigurations()
        {
            var configCleaner = new ConfigCleaner();
            configCleaner.Clean();
        }
    }
}