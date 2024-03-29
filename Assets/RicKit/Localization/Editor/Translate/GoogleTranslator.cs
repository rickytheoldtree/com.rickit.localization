using System;
using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using RicKit.Localization.Utils;
using UnityEditor;
using UnityEngine;

namespace RicKit.Localization.Translate
{
   public class GoogleTranslator
{
    private static GoogleTranslator instance;
    private readonly Config.Config.WebDriverType driverType;
    private readonly IWebDriver driver;
    private string url;

    private static string DriversPath
    {
        get
        {
            var config = LocalizationEditorUtils.GetConfig();
            if (config.customDriverPath)
            {
                return config.driverPath;
            }
            var guids = AssetDatabase.FindAssets("Selenium.WebDriver.4.15.0");
            if (guids.Length == 0)
            {
                Debug.LogError("未找到驱动路径");
                return null;
            }
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            path = Path.Combine(path, "drivers");
            path = Path.GetFullPath(path);
            return path;
        }
    }

    public static GoogleTranslator GetInstance(Config.Config.WebDriverType driver)
    {
        if(instance == null)
        {
            instance = new GoogleTranslator(driver);
        }
        else if(instance.driverType != driver)
        {
            Close();
            instance = new GoogleTranslator(driver);
        }
        return instance;
    }
    private GoogleTranslator(Config.Config.WebDriverType e)
    {
        driverType = Config.Config.WebDriverType.Edge;
        switch (e)
        {
            default:
            case Config.Config.WebDriverType.Edge:
                driver = new EdgeDriver(DriversPath);
                break;
            case Config.Config.WebDriverType.Chrome:
                driver = new ChromeDriver(DriversPath);
                break;
            case Config.Config.WebDriverType.FireFox:
                driver = new FirefoxDriver(DriversPath);
                break;
        }
    }
    public bool Translate(string txt, string from, string to, Action<List<string>> callback)
    {
        url = $"https://translate.google.com/?hl=en&sl={LocalizationEditorUtils.GetIsoCode(from)}" +
              $"&tl={LocalizationEditorUtils.GetIsoCode(to)}" +
              $"&text={GetUrlSafeString(txt)}" +
              "&op=translate";
        try
        {
            driver.Navigate().GoToUrl(url);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }
        //每隔1秒尝试1次获取翻译结果，最多尝试10次
        var count = 0;
        bool success = false;
        while (count < 10)
        {
            try
            {
                // find button aria-label="Copy translation"
                var button = driver.FindElement(By.CssSelector("button[aria-label='Copy translation']"));
                if (button != null)
                {
                    button.Click();
                    success = true;
                    break;
                }
            }
            catch
            {
                // ignored
            }
            count++;
            System.Threading.Tasks.Task.Delay(1000).Wait();
        }
        if (!success)
        {
            Debug.LogError("点击10次后还没翻译完，检查一下网络，或者换一个vpn吧");
            return false;
        }
        System.Threading.Tasks.Task.Delay(1000).Wait();
        //访问Windows粘贴板
        var text = GUIUtility.systemCopyBuffer;
        if (!LocalizationEditorUtils.GetSplit(text, out var values))
        {
            return false;
        }
        callback?.Invoke(values);
        return true;
    }

    public static void Close()
    {
        if (instance == null) return;
        instance.driver.Quit();
        instance = null;
    }
    
    private static string GetUrlSafeString(string str)
    {
        str = str.Replace(" ", "%20");
        str = str.Replace("=", "%3D");
        str = str.Replace("#", "%23");
        str = str.Replace("&", "%26");
        str = str.Replace("+", "%2B");
        return str;
    }
} 
}

