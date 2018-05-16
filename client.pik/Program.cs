using System;
using System.IO;
using System.Net;
using System.Reflection;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace client.pik
{
	[TestFixture]
	public class Program
	{
		private IWebDriver driver;
		private WebDriverWait wait;

		[SetUp]
		public void Start()
		{
			ChromeOptions options = new ChromeOptions();
			options.AddArgument("headless");
			driver = new ChromeDriver(options);
			wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
		}

		[Test]
		public void CheckNews()
		{
			Login();
			string flatNewsId = "";

			var textFileInfo =
				new FileInfo(Path.Combine(
					Path.GetDirectoryName(
						Assembly.GetExecutingAssembly().Location),
						@"news_id.txt"));

			if (!textFileInfo.Exists)
			{
				using (var f = File.Create(textFileInfo.FullName)) { }
			}
			
			var newsIdString = File.ReadAllText(textFileInfo.FullName);

			var FlatNewsLink = "https://client.pik.ru/object/c8ac67d6-9831-e711-857e-001ec9d56418/news";
			var pantryNewsLink = "https://client.pik.ru/object/d61206ad-e721-e811-b0fc-0050568859fb/news";

			try
			{
				driver.Url = FlatNewsLink;
				WaitForNewsPageLoad();
				var newsFlatLinkParts = driver.FindElements(By.CssSelector("a.News-list--link"))[0].GetAttribute("href").Split('/');
				flatNewsId = newsFlatLinkParts[newsFlatLinkParts.Length - 1];
				Assert.AreEqual(newsIdString, flatNewsId, "по квартире");


				driver.Url = pantryNewsLink;
				WaitForNewsPageLoad();
				var newsPantryLinkParts = driver.FindElements(By.CssSelector("a.News-list--link"))[0].GetAttribute("href").Split('/');
				var pantryNewsId = newsPantryLinkParts[newsPantryLinkParts.Length - 1];
				Assert.AreEqual(newsIdString, pantryNewsId, "по кладовке");
			}
			catch (AssertionException e)
			{
				var message = $"В личном кабинете ПИК есть свежие новости! {FlatNewsLink}";
				SendTelegram(message);

				using (var f = File.Create(textFileInfo.FullName)){}
				File.WriteAllText(textFileInfo.FullName, flatNewsId);
			}
			catch (Exception e)
			{
				var message = $"Дружок, твой тест упал :( Смотри ошибку: {e.Message}";
				SendTelegram(message);
			}

			var today = DateTime.Now;
			if (today.Hour == 17 && (today.Minute == 0 || today.Minute == 1))
			{
				var message = "В личном кабинете ПИК нет свежих новостей";
				SendTelegram(message);
			}

			var mess = "В личном кабинете ПИК нет свежих новостей";
			Assert.Pass(mess);

		}

		private void SendTelegram(string message)
		{
			var telegramURL = @"https://api.telegram.org";
			var token = "";
			var chat_id = "";
			string url = $"{telegramURL}/bot{token}/sendMessage?chat_id={chat_id}&text={message}";
			driver.Url = url;

			/* using proxy
			 
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			WebProxy myproxy = new WebProxy("10.14.188.239", 8080)
			{
				BypassProxyOnLocal = false
			};

			request.Proxy = myproxy;
			request.Method = "POST";
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			*/
		}

		private void WaitForNewsPageLoad()
		{
			wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("a.News-list--link")));
		}

		private void Login()
		{
			var login = "";
			var pass = "";

			if (login == "" || pass == "") { throw new System.ArgumentException("Please provide correct login data"); }

			driver.Url = "https://client.pik.ru/auth";
			driver.FindElement(By.CssSelector("input#login")).SendKeys(Keys.Home + login);
			driver.FindElement(By.CssSelector("input#password")).SendKeys(Keys.Home + pass);

			var oldPage = driver.FindElement(By.CssSelector("div.Page"));

			driver.FindElement(By.CssSelector("button.Button.Button-marginBottom")).Click();
			wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".DropdownLink-content")));
		}

		[TearDown]
		public void Stop()
		{
			driver.Quit();
			driver = null;
		}
	}
}