using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace PasswordVaultTests;

public class SeleniumTests : IDisposable
{
    private IWebDriver driver;
    private WebDriverWait wait;
    private ChromeOptions options = new ChromeOptions();
    
    //Setup constructor
    public SeleniumTests()
    {
        var options = new ChromeOptions();
        options.AddArgument("--disable-search-engine-choice-screen");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--start-maximized");
        options.AddArgument("--headless=new");
        options.AddArgument("--allow-insecure-localhost");
        //This one is needed locally
        // options.AddArgument("--user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");
            
        driver = new ChromeDriver(options);
        driver.Navigate().GoToUrl("http://localhost:5011");
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
    }
    
    [Fact]
    public void IsHomeStartPage()
    {
        var title = driver.Title;
        Assert.True(title == "City Vault", "The title of the page should be 'City Vault'.");
    }
    
    [Fact]
    public void CanUserLogin()
    {
        var usernameInput = driver.FindElement(By.XPath("/html/body/div[1]/main/article/div[1]/form/div[1]/input"));
        var passwordInput = driver.FindElement(By.XPath("/html/body/div[1]/main/article/div[1]/form/div[2]/input"));
        var loginButton = driver.FindElement(By.XPath("/html/body/div[1]/main/article/div[1]/form/button"));
        usernameInput.SendKeys("test");
        passwordInput.SendKeys("test");
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

        // wait.Until(driver => loginButton.Enabled);
        loginButton.Click();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        var logoutButton = driver.FindElement(By.XPath("/html/body/div[1]/main/article/div[2]/div[2]/button"));
        Assert.True(logoutButton.Displayed, "Logout button should be visible after successful login.");
    }

    public void Dispose()
    {
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        driver.Quit();
    }
}