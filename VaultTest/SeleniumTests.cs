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
        // options.AddArgument("--headless");
            
        driver = new ChromeDriver(options);
        driver.Navigate().GoToUrl("http://localhost:5011");
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }
    
    [Fact]
    public void IsHomeStartPage()
    {
        var title = driver.Title;
        Assert.True(title == "City Vault", "The title of the page should be 'City Vault'.");
    }
    
    [Fact]
    public void CanPressLockButton()
    {
        var unlockButton = driver.FindElement(By.XPath("/html/body/div[1]/main/article/div/div[2]/img"));
        unlockButton.Click();
        var passwordInput = wait.Until(driver => driver.FindElement(By.XPath("/html/body/div[1]/main/article/div[2]")));
        Assert.True(passwordInput.Displayed, "Password input field should be visible after pressing the unlock button.");
    }

    public void Dispose()
    {
        driver.Quit();
    }
}