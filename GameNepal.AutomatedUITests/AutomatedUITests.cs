using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using Xunit;
using OpenQA.Selenium.Support.UI;

namespace GameNepal.AutomatedUITests
{
    public class AutomatedUITests:IDisposable
    {
        private readonly IWebDriver _driver;
        public AutomatedUITests()
        {
            _driver = new ChromeDriver(Environment.CurrentDirectory);
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }

        [Fact]
        public void Signup_test()
        {
            _driver.Navigate().GoToUrl("http://localhost:49550/Home/Register");
            Assert.Equal("Register", _driver.Title);
            Assert.Contains("Game Nepal - your gaming companion", _driver.PageSource);

        }
        [Fact]
        public void Register_test()
        {
            _driver.Navigate().GoToUrl("http://localhost:49550/Home/Register");
            _driver.FindElement(By.Id("FirstName"))
                .SendKeys("TestFirst");
            _driver.FindElement(By.Id("LastName"))
               .SendKeys("TestName");
            _driver.FindElement(By.Id("Email"))
               .SendKeys("test113@abc.com");
            _driver.FindElement(By.Id("Phone"))
               .SendKeys("0912345125");
            _driver.FindElement(By.XPath("//*[@type='radio'][1]")).Click();
            _driver.FindElement(By.Id("City"))
               .SendKeys("TestCity");
            _driver.FindElement(By.XPath("//*[@id='AgeGroup']/option[2]")).Click();

            _driver.FindElement(By.Id("pwdReEntered"))
               .SendKeys("Test1234@");
            _driver.FindElement(By.Id("pwdReEntered"))
               .SendKeys("Test1234@");
            _driver.FindElement(By.Id("btnSend"))
                .Click();
            Assert.Equal("Home", _driver.Title);
        }
    }
}
