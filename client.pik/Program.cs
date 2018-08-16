using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
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
			wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
			driver.Manage().Window.Size = new System.Drawing.Size(1500, 900);
		}

		[Test]
		public void CheckNews()
		{
			var users = CreateUsers();
			var today = DateTime.Now;

			foreach (var user in users)
			{
				Login(user);
				string flatNewsId = "";

				var txtName = $"news_id_{user.Name}.txt";
				FileInfo textFileInfo = CreateFileInfoToAssemblyDirectory(txtName);

				if (!textFileInfo.Exists)
				{
					using (var f = File.Create(textFileInfo.FullName)) { }
				}

				var logName = "log.txt";
				FileInfo logFileInfo = CreateFileInfoToAssemblyDirectory(logName);

				if (!logFileInfo.Exists)
				{
					using (var f = File.Create(logFileInfo.FullName)) { }
				}

				var logsCountName = "logs_count.txt";
				FileInfo logsCountFileInfo = CreateFileInfoToAssemblyDirectory(logsCountName);

				if (!logsCountFileInfo.Exists)
				{
					using (var f = File.Create(logsCountFileInfo.FullName)) { };
					File.WriteAllText(logsCountFileInfo.FullName, "0");
				}

				var continErrorsName = "errors_conti.txt";
				FileInfo continErrorsInfo = CreateFileInfoToAssemblyDirectory(continErrorsName);

				if (!continErrorsInfo.Exists)
				{
					using (var f = File.Create(continErrorsInfo.FullName)) { };
					File.WriteAllText(continErrorsInfo.FullName, "0");
				}

				try
				{
					if (user.FlatGuid != null)
					{
						var newsIdString = File.ReadAllText(textFileInfo.FullName);

						driver.Url = $"https://client.pik.ru/object/{user.FlatGuid}/news";
						WaitForNewsPageLoad();
						var newsFlatLinkParts = driver.FindElements(By.CssSelector("a.News-list--link"))[0].GetAttribute("href").Split('/');
						flatNewsId = newsFlatLinkParts[newsFlatLinkParts.Length - 1];

						if (newsIdString != flatNewsId)
						{
							var message = $"В личном кабинете ПИК есть свежие новости! https://client.pik.ru/object/{user.FlatGuid}/news";
							SendTelegram(user, message);

							File.WriteAllText(textFileInfo.FullName, flatNewsId);
						}

						driver.Url = $"https://client.pik.ru/object/{user.FlatGuid}/info";
						wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".PropertiesList-blockName")));
						wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("button")));

						Thread.Sleep(10000);
						var buttonsCount = driver.FindElements(By.CssSelector("button")).Count;

						if (buttonsCount != 13)
						{
							var message = $"В личном кабинете ПИК изменилось количество кнопок, ломись туда! https://client.pik.ru/object/{user.FlatGuid}/news";
							SendTelegram(user, message);
						}
					}

					if (user.PantryGuid != null)
					{
						var newsIdString = File.ReadAllText(textFileInfo.FullName);

						driver.Url = $"https://client.pik.ru/object/{user.PantryGuid}/news";
						WaitForNewsPageLoad();
						var newsPantryLinkParts = driver.FindElements(By.CssSelector("a.News-list--link"))[0].GetAttribute("href").Split('/');
						var pantryNewsId = newsPantryLinkParts[newsPantryLinkParts.Length - 1];

						if (newsIdString != pantryNewsId)
						{
							var message = $"В личном кабинете ПИК есть свежие новости! https://client.pik.ru/object/{user.PantryGuid}/news";
							SendTelegram(user, message);

							File.WriteAllText(textFileInfo.FullName, flatNewsId);
						}
					}

					Logout();

					if (today.Hour == 17 && (today.Minute == 0))
					{
						var message = "В личном кабинете ПИК нет свежих новостей";
						SendTelegram(user, message);
					}

					File.WriteAllText(continErrorsInfo.FullName, "0");
				}

				catch (Exception e)
				{
					var logsCount = Int32.Parse(File.ReadAllText(logsCountFileInfo.FullName));
					logsCount++;

					File.WriteAllText(logsCountFileInfo.FullName, logsCount.ToString());

					File.AppendAllText(logFileInfo.FullName, Environment.NewLine + $"{today} - {e.Message}");

					var contiErrorsCount = Int32.Parse(File.ReadAllText(continErrorsInfo.FullName));
					contiErrorsCount++;

					if (contiErrorsCount > 9)
					{
						SendTelegramToAdmin($"Тест зафейлился {contiErrorsCount} раз подряд!!! Нужно что-то сделать");
						SendTelegramToAdmin($"Общее количество ошибок - {logsCount}, смотри файл лога");
						contiErrorsCount = 0;
					}

					File.WriteAllText(continErrorsInfo.FullName, contiErrorsCount.ToString());
				}
			}


			//var mess = "Тест прошёл успешно";
			//Assert.Pass(mess);
		}

		private static FileInfo CreateFileInfoToAssemblyDirectory(string name)
		{
			return new FileInfo(Path.Combine(
					Path.GetDirectoryName(
						Assembly.GetExecutingAssembly().Location),
						name));
		}

		private void Logout()
		{
			wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".Header-logoutButton")));
			driver.FindElement(By.CssSelector(".Header-logoutButton")).Click();
			wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("input#login")));
		}

		public static List<User> CreateUsers()
		{
			var users = new List<User>();



			return users;
		}

		private void SendTelegram(User user, string message)
		{
			var telegramURL = @"https://api.telegram.org";
			var token = ConfigurationManager.AppSettings["bot_token"];
			var chat_id = user.ChatId;
			string url = $"{telegramURL}/bot{token}/sendMessage?chat_id={chat_id}&text={message}";
			driver.Url = url;
			wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("body > pre")));
			driver.Url = "https://client.pik.ru/object";

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

		private void SendTelegramToAdmin(string message)
		{
			var telegramURL = @"https://api.telegram.org";
			var token = ConfigurationManager.AppSettings["bot_token"];
			var chat_id = "168694373";
			string url = $"{telegramURL}/bot{token}/sendMessage?chat_id={chat_id}&text={message}";
			driver.Url = url;
		}

		private void WaitForNewsPageLoad()
		{
			wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("a.News-list--link")));
		}

		private void Login(User user)
		{
			var login = user.Login;
			var pass = user.Password;

			if (login == "" || pass == "") { throw new System.ArgumentException("Please provide correct login data"); }

			driver.Url = "https://client.pik.ru/auth";
			driver.FindElement(By.CssSelector("input#login")).SendKeys(Keys.Home + login);
			driver.FindElement(By.CssSelector("input#password")).SendKeys(Keys.Home + pass);

			var oldPage = driver.FindElement(By.CssSelector("div.Page"));

			driver.FindElement(By.CssSelector("button.Button.Button-marginBottom")).Click();
			wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".Header-account--text")));
		}

		[TearDown]
		public void Stop()
		{
			driver.Quit();
			driver = null;
		}
	}
}