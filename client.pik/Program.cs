using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
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
			var nl = @"%0A";

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

				var lastMessageId = "lastMessageId.txt";
				FileInfo lMIdFileInfo = CreateFileInfoToAssemblyDirectory(lastMessageId);

				if (!lMIdFileInfo.Exists)
				{
					using (var f = File.Create(lMIdFileInfo.FullName)) { }
				}

				var activeSteps = "activesteps.txt";
				FileInfo activeStepsFileInfo = CreateFileInfoToAssemblyDirectory(activeSteps);

				if (!activeStepsFileInfo.Exists)
				{
					using (var f = File.Create(activeStepsFileInfo.FullName)) { }
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
							var title = driver.FindElement(By.CssSelector("h3.News-one--title")).Text;

							var beautyContent = "";
							var contentParts = driver.FindElements(By.CssSelector(".News-one--content div, .News-one--content p"));
							foreach (var part in contentParts)
							{
								beautyContent += $"{part.Text}{nl}";
							}

							//var content = driver.FindElement(By.CssSelector(".News-one--content")).Text;
							var infoMessage = $"<b>{title}</b>{nl}{nl}{beautyContent}";

							var message = $"В личном кабинете ПИК есть свежие новости! https://client.pik.ru/object/{user.FlatGuid}/news";
							SendTelegram(user, message);
							SendTelegram(user, infoMessage);

							File.WriteAllText(textFileInfo.FullName, flatNewsId);
						}

						///check status

						var statusLink = $"https://client.pik.ru/object/{user.FlatGuid}/status";
						driver.Url = statusLink;

						wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".StatusDealPage-circle--active")));
						int activeStatusCount = driver.FindElements(By.CssSelector(".StatusDealPage-circle--active")).Count;

						var activeStepsFromFile = File.ReadAllText(activeStepsFileInfo.FullName);

						if (activeStepsFromFile.Length != 0)
						{
							if (int.Parse(activeStepsFromFile) != activeStatusCount)
							{
								var msg = $"В личном кабинете ПИК изменилось количество статусов ({activeStepsFromFile} ➡️ {activeStatusCount}) {statusLink}";
								SendTelegram(user, msg);
								File.WriteAllText(activeStepsFileInfo.FullName, activeStatusCount.ToString());
							}
						}
						else
						{
							File.WriteAllText(activeStepsFileInfo.FullName, activeStatusCount.ToString());
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
							var title = driver.FindElement(By.CssSelector("h3.News-one--title")).Text;

							var beautyContent = "";
							var contentParts = driver.FindElements(By.CssSelector(".News-one--content div, .News-one--content p"));
							foreach (var part in contentParts)
							{
								beautyContent += $"{part.Text}{nl}";
							}
							
							var infoMessage = $"<b>{title}</b>{nl}{nl}{beautyContent}";

							var message = $"В личном кабинете ПИК есть свежие новости! https://client.pik.ru/object/{user.PantryGuid}/news";
							SendTelegram(user, message);
							SendTelegram(user, infoMessage);

							File.WriteAllText(textFileInfo.FullName, flatNewsId);
						}
					}

					Logout();

					if (today.Hour == 17 && (today.Minute == 0))
					{
						var message = "В личном кабинете ПИК нет свежих новостей";
						var sentMsgId = SendTelegramWithId(user, message);

						var lastSentMsgId = File.ReadAllText(lMIdFileInfo.FullName);

						if (lastSentMsgId.Length != 0)
						{
							DeleteTelegramMessage(user, lastSentMsgId);
						}

						File.WriteAllText(lMIdFileInfo.FullName, sentMsgId);
					}

					File.WriteAllText(continErrorsInfo.FullName, "0");
				}

				catch (Exception e)
				{
					var logsCount = int.Parse(File.ReadAllText(logsCountFileInfo.FullName));
					logsCount++;

					File.WriteAllText(logsCountFileInfo.FullName, logsCount.ToString());

					File.AppendAllText(logFileInfo.FullName, Environment.NewLine + $"{today} - {e.Message}");

					var contiErrorsCount = int.Parse(File.ReadAllText(continErrorsInfo.FullName));
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

		private void DeleteTelegramMessage(User user, string messageId)
		{
			var telegramURL = @"https://api.telegram.org";
			var token = ConfigurationManager.AppSettings["bot_token"];
			var chat_id = user.ChatId;
			string url = $"{telegramURL}/bot{token}/deleteMessage?chat_id={chat_id}&message_id={messageId}";
			driver.Url = url;
			wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("body > pre")));
			//driver.Url = "https://client.pik.ru/object";
		}

		private void CheckButtonsCount(User user)
		{
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
			var dkor = new User
			{
				ChatId = "168694373",
				FlatGuid = "c8ac67d6-9831-e711-857e-001ec9d56418",
				PantryGuid = "d61206ad-e721-e811-b0fc-0050568859fb",
				Name = "dkor",
				Login = "", //without +7
				Password = ""
			};

			users.Add(dkor);
			return users;
		}

		private void SendTelegram(User user, string message)
		{
			var telegramURL = @"https://api.telegram.org";
			var token = ConfigurationManager.AppSettings["bot_token"];
			var chat_id = user.ChatId;
			string url = $"{telegramURL}/bot{token}/sendMessage?chat_id={chat_id}&text={message}&parse_mode=HTML";
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
		private string SendTelegramWithId(User user, string message)
		{
			var telegramURL = @"https://api.telegram.org";
			var token = ConfigurationManager.AppSettings["bot_token"];
			var chat_id = user.ChatId;
			string url = $"{telegramURL}/bot{token}/sendMessage?chat_id={chat_id}&text={message}&parse_mode=HTML";
			driver.Url = url;

			var response = driver.FindElement(By.CssSelector("body")).Text;
			var msgIdPattern = @"(message_id"":\d*)";
			var msgIdSentence = Regex.Match(response, msgIdPattern).Value;

			var msgId = msgIdSentence.Split(':')[1];

			wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("body > pre")));
			return msgId;
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