using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WinRep_Code.Src
{
    public class EssentialTweaks
    {
        // private
        public enum ShellAction
        {
            PS7,
            PS5
        }

        public enum TelemetryAction
        {
            Disable,
            Enable
        }

        public enum StorageSenseAction
        {
            Disable,
            Enable
        }

        private static readonly List<(string Path, string Name, int Value)> RegistryChangesForDebloatEdge = new()
        {
        ("SOFTWARE\\Policies\\Microsoft\\EdgeUpdate", "CreateDesktopShortcutDefault", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "EdgeEnhanceImagesEnabled", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "PersonalizationReportingEnabled", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "ShowRecommendationsEnabled", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "HideFirstRunExperience", 1),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "UserFeedbackAllowed", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "ConfigureDoNotTrack", 1),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "AlternateErrorPagesEnabled", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "EdgeCollectionsEnabled", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "EdgeFollowEnabled", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "EdgeShoppingAssistantEnabled", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "MicrosoftEdgeInsiderPromotionEnabled", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "ShowMicrosoftRewards", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "WebWidgetAllowed", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "DiagnosticData", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "EdgeAssetDeliveryServiceEnabled", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "CryptoWalletEnabled", 0),
        ("SOFTWARE\\Policies\\Microsoft\\Edge", "WalletDonationEnabled", 0)
        };

        private void SetStartupType(string serviceName, string startupType)
        {
            string typeFlag = startupType switch
            {
                "Automatic" => "auto",
                "Manual" => "demand",
                "Disabled" => "disabled",
                "AutomaticDelayedStart" => "delayed-auto",
                _ => throw new ArgumentException("Unknown startup type: " + startupType)
            };

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = $"config \"{serviceName}\" start= {typeFlag}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine($"Successfully set {serviceName} to {startupType}");
            }
            else
            {
                Console.WriteLine($"Failed to set {serviceName}: {error}");
            }
        }

        private bool IsCommandAvailable(string command)
        {
            try
            {
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return !string.IsNullOrWhiteSpace(output);
            }
            catch
            {
                return false;
            }
        }

        private void DeleteAllFilesInDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"Directory not found: {path}");
                    return;
                }

                foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"Deleted file: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not delete file: {file} - {ex.Message}");
                    }
                }

                foreach (string dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        Console.WriteLine($"Deleted directory: {dir}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not delete directory: {dir} - {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to clean directory: {path} - {ex.Message}");
            }
        }

        private bool SetRegistryKeyValue(RegistryKey? key, string? regPath, string name, object value, RegistryValueKind valueKind)
        {
            try
            {
                if (key != null && regPath == null) {
                    key.SetValue(name, value, valueKind);
                    Console.WriteLine("Registry value has been set using provided key instance.");
                } else {
                    using (RegistryKey newKey = Registry.LocalMachine.CreateSubKey(regPath))
                    {
                        if (newKey == null) {
                            Console.WriteLine("Could not open or create registry key.");
                            return false;
                        }

                        newKey.SetValue(name, value, valueKind);
                        Console.WriteLine("Registry value has been set using newly created key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied. Please run the application as administrator.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error modifying registry: {ex.Message}");
                return false;
            }
            return true;
        }

        // public
        // copy the Tweaks from https://winutil.christitus.com/dev/ copy Essential Tweaks only
        public bool SetWindowsTerminalDefaultShell(ShellAction action)
        {
            string powershell7Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerShell", "7");
            string terminalPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState\settings.json"
            );

            string targetTerminalName = action == ShellAction.PS7 ? "PowerShell" : "Windows PowerShell";

            if (action == ShellAction.PS7 && !Directory.Exists(powershell7Path))
            {
                Console.WriteLine("PowerShell 7 is not installed. Installing via winget...");
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = "install --id Microsoft.Powershell -e",
                        UseShellExecute = true
                    })?.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error launching winget: {ex.Message}");
                    return false;
                }
            }
            else if (action == ShellAction.PS7)
            {
                Console.WriteLine("PowerShell 7 is already installed.");
            }

            if (!IsCommandAvailable("wt"))
            {
                Console.WriteLine("Windows Terminal is not installed. Skipping terminal preference.");
                return false;
            }

            // Check if settings.json exists
            if (!File.Exists(terminalPath))
            {
                Console.WriteLine($"Settings file not found at: {terminalPath}");
                return false;
            }

            Console.WriteLine("Settings file found.");
            string jsonContent = File.ReadAllText(terminalPath);
            JsonNode root = JsonNode.Parse(jsonContent);

            if (root?["profiles"]?["list"] is JsonArray profiles)
            {
                var targetProfile = profiles.FirstOrDefault(p =>
                    p?["name"]?.ToString() == targetTerminalName);

                if (targetProfile != null && targetProfile["guid"] != null)
                {
                    root["defaultProfile"] = targetProfile["guid"]?.ToString();

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                    File.WriteAllText(terminalPath, root.ToJsonString(options));
                    Console.WriteLine($"Default profile updated to \"{targetTerminalName}\".");
                }
                else
                {
                    Console.WriteLine($"No \"{targetTerminalName}\" profile found in settings.json.");
                }
            }
            return true;
        }

        public bool CreateSystemRestorePoint(string description)
        {
            try
            {
                ManagementClass mc = new ManagementClass("SystemRestore");
                ManagementBaseObject parameters = mc.GetMethodParameters("CreateRestorePoint");

                parameters["Description"] = description;
                parameters["RestorePointType"] = 10; // MODIFY_SETTINGS
                parameters["EventType"] = 100;       // BEGIN_SYSTEM_CHANGE

                ManagementBaseObject result = mc.InvokeMethod("CreateRestorePoint", parameters, null);

                uint returnValue = (uint)result["ReturnValue"];

                if (returnValue == 0)
                {
                    Console.WriteLine("Restore point created successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to create restore point. Return code: {returnValue}");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while creating restore point: {ex.Message}");
                return false;
            }
        }

        public bool ResetWifiAdapter()
        {
            string[] ProcessArgumentsList =
            {
                "netsh winsock reset",
                "netsh int ip reset",
                "ipconfig /release",
                "ipconfig /renew",
                "ipconfig /flushdns"
            };

            Process p = new Process();
            p.StartInfo.FileName = "powercfg";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;

            try
            {
                foreach (string arg in ProcessArgumentsList)
                {
                    p.StartInfo.Arguments = arg;
                    p.Start();
                    p.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to reset Wifi Adapter: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool ApplyEdgeDebloatTweaks()
        {
            foreach (var (path, name, value) in RegistryChangesForDebloatEdge)
            {
                SetRegistryKeyValue(null, path, name, value, RegistryValueKind.DWord);
            }
            return true;
        }   

        public bool DeleteTemporaryFiles()
        {
            string windowsTemp = @"C:\Windows\Temp";
            string userTemp = Path.GetTempPath();

            DeleteAllFilesInDirectory(windowsTemp);
            DeleteAllFilesInDirectory(userTemp);

            return true;
        }

        public bool DisableActivityHistory()
        {
            const string regPath = @"SOFTWARE\Policies\Microsoft\Windows\System";

            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(regPath))
            {
                SetRegistryKeyValue(key, null, "EnableActivityFeed", 0, RegistryValueKind.DWord);
                SetRegistryKeyValue(key, null, "PublishUserActivities", 0, RegistryValueKind.DWord);
                SetRegistryKeyValue(key, null, "UploadUserActivities", 0, RegistryValueKind.DWord);
            }

            return true;
        }

        public bool DisableConsumerFeatures()
        {
            const string regPath = @"SOFTWARE\Policies\Microsoft\Windows\CloudContent"; 
            SetRegistryKeyValue(null, regPath, "DisableWindowsConsumerFeatures", 1, RegistryValueKind.DWord);
            return true;
        }

        public bool DisableGameDVR()
        {
            const string regPath = @"HKCU:\System\GameConfigStore";

            using (RegistryKey? userKey = Registry.CurrentUser.CreateSubKey(regPath))
            {
                SetRegistryKeyValue(userKey, null, "GameDVR_FSEBehavior", 2, RegistryValueKind.DWord);
                SetRegistryKeyValue(userKey, null, "GameDVR_Enabled", 0, RegistryValueKind.DWord);
                SetRegistryKeyValue(userKey, null, "GameDVR_HonorUserFSEBehaviorMode", 1, RegistryValueKind.DWord);
                SetRegistryKeyValue(userKey, null, "GameDVR_EFSEFeatureFlags", 0, RegistryValueKind.DWord);
            }
            string hklmPath = @"SOFTWARE\Policies\Microsoft\Windows\GameDVR";
            SetRegistryKeyValue(null, hklmPath, "AllowGameDVR", 0, RegistryValueKind.DWord);

            return true;
        }

        public bool DisableHibernation()
        {
            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "powercfg.exe",
                    Arguments = "/hibernate off",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using Process proc = Process.Start(psi)!;
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                Console.WriteLine(" Executed: powercfg.exe /hibernate off");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Failed to disable hibernation via powercfg: {ex.Message}");
                return false;
            }

            SetRegistryKeyValue(null, @"System\CurrentControlSet\Control\Session Manager\Power", "HibernateEnabled", 0, RegistryValueKind.DWord);

            SetRegistryKeyValue(null, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings", "ShowHibernateOption", 0, RegistryValueKind.DWord);

            return true;
        }

        public bool DisableHomegroup()
        {
            try
            {
                SetStartupType("HomeGroupListener", "Manual");
                SetStartupType("HomeGroupProvider", "Manual");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to disable Homegroup services: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool DisableLocationTracking()
        {
            SetRegistryKeyValue(null, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Deny", RegistryValueKind.String);
            SetRegistryKeyValue(null, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Sensor\Overrides\{BFA794E4-F964-4FDB-90F6-51056BFE4B44}", "SensorPermissionState", 0, RegistryValueKind.DWord);
            SetRegistryKeyValue(null, @"SYSTEM\CurrentControlSet\Services\lfsvc\Service\Configuration", "Status", 0, RegistryValueKind.DWord);
            SetRegistryKeyValue(null, @"SYSTEM\Maps", "AutoUpdateEnabled", 0, RegistryValueKind.DWord);

            return true;
        }

        public bool DisablePowershell7Telemetry(TelemetryAction action)
        {
            string value = action == TelemetryAction.Disable ? "1" : "";
            try
            {
                Environment.SetEnvironmentVariable("POWERSHELL_TELEMETRY_OPTOUT", value, EnvironmentVariableTarget.Machine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set PowerShell telemetry opt-out: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool SetStorageSense(StorageSenseAction action)
        {
            int value = action == StorageSenseAction.Disable ? 0 : 1;
            SetRegistryKeyValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01", value, RegistryValueKind.DWord);
            return true;
        }

        public bool DisableTelemetry()
        {
            var registryChanges = new List<(string Path, string Name, object Value, RegistryValueKind Kind)>
            {
                (@"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Siuf\Rules", "DoNotShowFeedbackNotifications", 1, RegistryValueKind.DWord),
                (@"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableTailoredExperiencesWithDiagnosticData", 1, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "DisabledByGroupPolicy", 1, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Diagnostics\DiagTrack", "Disabled", 1, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DODownloadMode", 1, RegistryValueKind.DWord),
                (@"SOFTWARE\Policies\Microsoft\Assistance\Client\1.0", "fAllowToGetHelp", 0, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnthusiastMode", 1, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\People", "PeopleBand", 0, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, RegistryValueKind.DWord),
                (@"SYSTEM\CurrentControlSet\Control\FileSystem", "LongPathsEnabled", 1, RegistryValueKind.DWord),
                (@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "SearchOrderConfig", 1, RegistryValueKind.DWord),
                (@"SYSTEM\CurrentControlSet\Control\PriorityControl", "SystemResponsiveness", 0, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", 4294967295, RegistryValueKind.DWord),
                (@"Control Panel\Desktop", "MenuShowDelay", 1, RegistryValueKind.DWord),
                (@"HKEY_USERS\.DEFAULT\Control Panel\Desktop", "AutoEndTasks", 1, RegistryValueKind.DWord),
                (@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "ClearPageFileAtShutdown", 0, RegistryValueKind.DWord),
                (@"SYSTEM\CurrentControlSet\Services\Ndu", "Start", 2, RegistryValueKind.DWord),
                (@"Control Panel\Mouse", "MouseHoverTime", "400", RegistryValueKind.String),
                (@"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", "IRPStackSize", 30, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Feeds", "EnableFeeds", 0, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", 2, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "HideSCAMeetNow", 1, RegistryValueKind.DWord),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ScoobeSystemSettingEnabled", 0, RegistryValueKind.DWord)
            };

            try
            {
                foreach (var (path, name, value, kind) in registryChanges)
                {
                    SetRegistryKeyValue(null, path, name, value, kind);
                }

                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"))
                {
                    SetRegistryKeyValue(key, null, "ContentDeliveryAllowed", 0, RegistryValueKind.DWord);
                    SetRegistryKeyValue(key, null, "OemPreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                    SetRegistryKeyValue(key, null, "PreInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                    SetRegistryKeyValue(key, null, "PreInstalledAppsEverEnabled", 0, RegistryValueKind.DWord);
                    SetRegistryKeyValue(key, null, "SilentInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                    SetRegistryKeyValue(key, null, "SubscribedContent-338387Enabled", 0, RegistryValueKind.DWord);
                    SetRegistryKeyValue(key, null, "SubscribedContent-338388Enabled", 0, RegistryValueKind.DWord);
                    SetRegistryKeyValue(key, null, "SubscribedContent-338389Enabled", 0, RegistryValueKind.DWord);
                    SetRegistryKeyValue(key, null, "SubscribedContent-353698Enabled", 0, RegistryValueKind.DWord);
                    SetRegistryKeyValue(key, null, "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to disable telemetry: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool DisableWifiSense()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\WcmSvc\wifinetworkmanager\config"))
                {
                    SetRegistryKeyValue(key, null, "Value", 0, RegistryValueKind.DWord);
                    SetRegistryKeyValue(key, null, "Value", 0, RegistryValueKind.DWord);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied. Please run the application as administrator.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disabling Wi-Fi Sense: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool EnableEndTaskWithRightClick()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings"))
                {
                    SetRegistryKeyValue(key, null, "TaskbarEndTask", 1, RegistryValueKind.DWord);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied. Please run the application as administrator.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enabling end task with right click: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool DisableEndTaskWithRightClick()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings"))
                {
                    SetRegistryKeyValue(key, null, "TaskbarEndTask", 0, RegistryValueKind.DWord);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied. Please run the application as administrator.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disabling end task with right click: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool PreferIPv4OverIPv6()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters"))
                {
                    SetRegistryKeyValue(key, null, "DisabledComponents", 32, RegistryValueKind.DWord);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Access denied. Please run the application as administrator.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting IPv4 preference: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool RunDiskCleanup()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cleanmgr.exe",
                    Arguments = "/d C: /VERYLOWDISK",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();

                Process.Start(new ProcessStartInfo
                {
                    FileName = "Dism.exe",
                    Arguments = "/online /Cleanup-Image /StartComponentCleanup /ResetBase",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to run disk cleanup: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool SetHibernationAsDefault()
        {
            try
            {
                SetRegistryKeyValue(null, @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\238C9FA8-0AAD-41ED-83F4-97BE242C8F20\7bc4a2f9-d8fc-4469-b07b-33eb785aaca0", "Attributes", 2, RegistryValueKind.DWord);

                SetRegistryKeyValue(null, @"SYSTEM\CurrentControlSet\Control\Power\PowerSettings\abfc2519-3608-4c2a-94ea-171b0ed546ab\94ac6d29-73ce-41a6-809f-6363ba21b47e", "Attributes", 2, RegistryValueKind.DWord);

                Process p = new Process();

                p.StartInfo.FileName = "powercfg";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;

                string[] ProcessArgumentsList =
                {
                "/hibernate on",
                "/change standby-timeout-ac 60",
                "/change standby-timeout-dc 60",
                "/change monitor-timeout-ac 10",
                "/change monitor-timeout-dc 1",
            };
                foreach (string arg in ProcessArgumentsList)
                {
                    p.StartInfo.Arguments = arg;
                    p.Start();
                    p.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set hibernation as default: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool ResetHibernationToDefault()
        {
            string[] ProcessArgumentsList =
            {
                "/hibernate off",
                "/change standby-timeout-ac 15",
                "/change standby-timeout-dc 15",
                "/change monitor-timeout-ac 15",
                "/change monitor-timeout-dc 15",
            };

            Process p = new Process();
            p.StartInfo.FileName = "powercfg";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            try
            {
                foreach (string arg in ProcessArgumentsList)
                {
                    p.StartInfo.Arguments = arg;
                    p.Start();
                    p.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to reset hibernation to default: {ex.Message}");
                return false;
            }
            return true;
        }

        public bool SetServicesToManual()
        {
            string[] services =
            {
            "defragsvc",
            "diagnosticshub.standardcollector.service",
            "diagsvc",
            "dmwappushservice",
            "dot3svc",
            "edgeupdate",
            "edgeupdatem",
            "embeddedmode",
            "fdPHost",
            "fhsvc",
            "hidserv",
            "icssvc",
            "lfsvc",
            "lltdsvc",
            "lmhosts",
            "msiserver",
            "netprofm",
            "p2pimsvc",
            "p2psvc",
            "perceptionsimulation",
            "pla",
            "seclogon",
            "smphost",
            "spectrum",
            "svsvc",
            "swprv",
            "upnphost",
            "vds",
            "vm3dservice",
            "vmicguestinterface",
            "vmicheartbeat",
            "vmickvpexchange",
            "vmicrdv",
            "vmicshutdown",
            "vmictimesync",
            "vmicvmsession",
            "vmicvss",
            "vmvss",
            "wbengine",
            "wcncsvc",
            "webthreatdefsvc",
            "wercplsupport",
            "wisvc",
            "wlidsvc",
            "wlpasvc",
            "wmiApSrv",
            "workfolderssvc",
            "wuauserv",
            "wudfsvc",
            };

            try
            {
                foreach (var service in services)
                {
                    SetStartupType(service, "Manual");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set services to manual: {ex.Message}");
                return false;
            }
            return true;

        }
    }
}
