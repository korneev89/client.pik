﻿using System;
using System.IO;
using System.Reflection;
<<<<<<< HEAD
using System.Text.RegularExpressions;
using System.Threading;
=======
>>>>>>> parent of 6cba918... worked version
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
			//options.AddArgument("headless");
			driver = new ChromeDriver(options);
			wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
		}

		[Test]
		public void DownloadSololearnCourse()
		{
<<<<<<< HEAD
			LoginSoloLearn();

			string book= string.Empty;

			book += @"<html><head><title>Python Book from SoloLearn</title><link rel=""stylesheet"" href=""styles.css""></head><body><div>";

			var bookName = "pythonBook.html";
			FileInfo bookNameInfo = CreateFileInfoToAssemblyDirectory(bookName);

			if (!bookNameInfo.Exists)
			{
				using (var f = File.Create(bookNameInfo.FullName)) { };
			}

			int modulesCount = driver.FindElements(By.CssSelector("div.appModuleCircle")).Count;

			int module = 0;
			try
			{
				for (; module < modulesCount; module++)
				{
					if (module > 5)
					{
						var certElement = driver.FindElement(By.CssSelector(".certificate"));

						IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
						jse.ExecuteScript("arguments[0].scrollIntoView(true);", certElement);

						// wait for scroll action
						Thread.Sleep(500);
					}
					driver.FindElements(By.CssSelector("div.appModuleCircle"))[module].Click();

					wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector(".appLesson.checkpoint")));
					//Thread.Sleep(500);

					string moduleName = $"<h1>Module {module + 1} - {driver.FindElement(By.CssSelector(".module.layer span.title")).Text}</h1>";
					book += moduleName;

					int lessonsCount = driver.FindElements(By.CssSelector(".appLesson.checkpoint")).Count;

					int lesson = 0;
					for (; lesson < lessonsCount; lesson++)
					{
						wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector(".appLesson.checkpoint")));
						//Thread.Sleep(500);

						string lessonName = $"<h2>Lesson {lesson + 1} - {driver.FindElements(By.CssSelector(".appLesson.checkpoint"))[lesson].FindElement(By.CssSelector("div.name")).Text}</h2>";
						book += lessonName;

						driver.FindElements(By.CssSelector(".appLesson.checkpoint"))[lesson].Click();

						wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector("span.video")));
						//Thread.Sleep(500);

						int videosCount = driver.FindElements(By.CssSelector("span.video")).Count;

						int video = 0;
						for (; video < videosCount; video++)
						{
							wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector("span.video")));
							//Thread.Sleep(500);

							driver.FindElements(By.CssSelector("span.video"))[video].Click();

							wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector("#textContent")));
							//Thread.Sleep(500);

							string content = driver.FindElement(By.CssSelector("#textContent")).GetAttribute("innerHTML");

							content = content.Replace("<h1>","<h3>").Replace(@"</h1>", @"</h3>");

							book += content;
							Thread.Sleep(500);
						}

						driver.FindElement(By.CssSelector("#navigateBackButton")).Click();
					}

					driver.FindElement(By.CssSelector("#navigateBackButton")).Click();
				}
			}
			catch (Exception e)
			{
				Assert.Warn("тест не завершился до конца | " + e.Message);
			}
			finally
			{
				book += @"</div></body></html>";

				var s1 = "\\\"";
				var s2 = "\\";
				book.Replace(s1, s2);

				//delete all "a" tags from book
				var tryItButtonPattern = @"<a(.+?)(?=<)<\/a>";

				book = Regex.Replace(book, tryItButtonPattern, String.Empty);

				Regex.Replace(book,tryItButtonPattern,"");
				File.WriteAllText(bookNameInfo.FullName, book);
			}
		}

		[Test]
		public void CheckRM()
		{
			var rmUser = new User
			{
				Name = "dkor",
				Login = "",
				Password = "",
				ChatId = "168694373"
			};

			var continErrorsName = "RM_errors_conti.txt";
			FileInfo continErrorsInfo = CreateFileInfoToAssemblyDirectory(continErrorsName);

			if (!continErrorsInfo.Exists)
			{
				using (var f = File.Create(continErrorsInfo.FullName)) { };
				File.WriteAllText(continErrorsInfo.FullName, "0");
			}

			try
			{
				LoginRM(rmUser);
				File.WriteAllText(continErrorsInfo.FullName, "0");
			}

			catch (Exception e)
			{
				var contiErrorsCount = Int32.Parse(File.ReadAllText(continErrorsInfo.FullName));
				contiErrorsCount++;

				if (contiErrorsCount > 5)
				{
					SendTelegramToRMAdmins($"Не удалось зайти в систему Redmine {contiErrorsCount} раз подряд!!! Свяжитесь с Корнеевым Дмитрием");
					contiErrorsCount = 0;
				}

				File.WriteAllText(continErrorsInfo.FullName, contiErrorsCount.ToString());
			}
		}

		private void LoginSoloLearn()
		{
			driver.Url = "https://www.sololearn.com/Play/Python";
			driver.FindElement(By.CssSelector(".btn.btn-default.facebook")).Click();
			driver.FindElement(By.CssSelector("input#email")).SendKeys(Keys.Home + "");
			driver.FindElement(By.CssSelector("input#pass")).SendKeys(Keys.Home + "");
			driver.FindElement(By.CssSelector("#loginbutton")).Click();

			wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector("div.appModuleCircle")));
		}

		private static FileInfo CreateFileInfoToAssemblyDirectory(string name)
		{
			return new FileInfo(Path.Combine(
					Path.GetDirectoryName(
						Assembly.GetExecutingAssembly().Location),
						name));
		}

		private void SendTelegramToRMAdmins(string message)
		{
			var telegramURL = @"https://api.telegram.org";
			var token = ConfigurationManager.AppSettings["RM_bot_token"];

			List<string> addressees = new List<string>
=======
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
>>>>>>> parent of 6cba918... worked version
			{
				"168694373", //dkorneev
				"347947909" //avb
			};

<<<<<<< HEAD
			foreach (string a in addressees)
			{
				string url = $"{telegramURL}/bot{token}/sendMessage?chat_id={a}&text={message}";
				driver.Url = url;
			}
		}

		private void LoginRM(User user)
=======
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
>>>>>>> parent of 6cba918... worked version
		{
			var login = "";
			var pass = "";

			if (login == "" || pass == "") { throw new ArgumentException("Please provide correct login data"); }

			driver.Url = "http://195.19.40.194:81/redmine/login";
			driver.FindElement(By.CssSelector("input#username")).SendKeys(Keys.Home + login);
			driver.FindElement(By.CssSelector("input#password")).SendKeys(Keys.Home + pass);

			var oldPage = driver.FindElement(By.CssSelector("form"));

<<<<<<< HEAD
			driver.FindElement(By.CssSelector(".us-log-in-btn")).Click();
			wait.Until(ExpectedConditions.StalenessOf(oldPage));
=======
			driver.FindElement(By.CssSelector("button.Button.Button-marginBottom")).Click();
			wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".DropdownLink-content")));
>>>>>>> parent of 6cba918... worked version
		}

		[TearDown]
		public void Stop()
		{
			driver.Quit();
			driver = null;
		}
	}
}