using System.Reflection;
using System.Text.Json;
using libJson;

namespace UtilityAppSettings;
/// <summary>
/// for creating output folder, settings file and reading of values from the settings file
/// </summary>
public static class MyAppSettings
{
    // get the folder name where the currently executing program resides
    private readonly static string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    private readonly static string _settingsPath = appDir + @"\MySettings.json";
    //=======================================================================================================
    // Create and Initialize a MySettings.json file if none exist
    // Need to edit the values in this file for your particular setting
    static bool InitializeSettings()
    {
        try
        {
            MySetting ms = new ();
            ms.ProxyIPaddress = "127.0.0.1";
            ms.Port = 4090; // make port execute in different port than 8000.
            ms.UserDataFolder = appDir + "\\chromium_user_data_folder";
            Console.WriteLine("Write the path for Chromium (chrome.exe) archive. Chromium will be used by this script.");
            string chromium_path = Console.ReadLine();
            
            if (chromium_path == null || !chromium_path.EndsWith("chrome.exe"))
            {
                Console.WriteLine("Please insert the chrome.exe path.");
                return false;
            }


            ms.ChromiumPath = chromium_path;
            JsonSerializerOptions opt = new ();
            opt.WriteIndented = true;
            string jsonString = Json.Serialize<MySetting>(ms, opt);
            FileLib.FileUtils.WriteAllText(_settingsPath, jsonString);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    //=======================================================================================================
    // read settings from MySettings.json file
    public static MySetting ReadSettings()
    {
        try
        {
            if (CreateDataOutFolder())
            {
                string settingsFile = _settingsPath;

                string jsonString = FileLib.FileUtils.ReadAllText(settingsFile);
                if (jsonString != null)
                {
                    if (jsonString.Length > 2)
                    {
                        MySetting s = Json.Deserialize<MySetting>(jsonString);
                        return s;
                    }
                    else
                        return null;
                }
                else
                {
                    Console.WriteLine($"Settings file not found! Creating one instead at {_settingsPath}");
                    bool bSuccess = InitializeSettings();

                    // if settings file created successfully, read the values from it
                    if (bSuccess)
                    {
                        jsonString = FileLib.FileUtils.ReadAllText(settingsFile);
                        if (jsonString != null)
                        {
                            if (jsonString.Length > 2)
                            {
                                MySetting s = Json.Deserialize<MySetting>(jsonString);
                                return s;
                            }
                            else
                                return null;
                        }
                        else return null;
                    }
                    else return null;
                }
            }
            else
            {
                Console.WriteLine("error creating data out folder");
                Console.WriteLine("press any key to exit");
                Console.ReadKey(true);
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
    //=======================================================================================================
    // create the "data-out" folder for storing the log and warc files
    static bool CreateDataOutFolder()
    {
        if (Directory.Exists(appDir + @"\data-out\"))
        {
            return true;
        }
        else
        {
            try
            {
                Directory.CreateDirectory(appDir + @"\data-out\");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                Console.WriteLine("press any key to exit");
                Console.ReadKey(true);
                return false;
            }
        }
    }
}



