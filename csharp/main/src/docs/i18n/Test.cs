namespace ST.Examples.i18n
{
	using System;
	using System.IO;
	using System.Resources;
	using System.Threading;
	using System.Globalization;
	using StringTemplate		= Antlr.StringTemplate.StringTemplate;
	using StringTemplateGroup	= Antlr.StringTemplate.StringTemplateGroup;
	
	/** Test internationalization/localization by showing that StringTemplate
	 *  easily deals with multiple versions of the same string.  The StringTemplates
	 *  and strings properties file are looked up using CLASSPATH.
	 */

	public class Test
	{
		class ResourceWrapper
		{
			ResourceManager mgr;

			public ResourceWrapper(ResourceManager mgr)
			{
				this.mgr	= mgr;
			}
			public string intro
			{
				get { return mgr.GetString("intro"); }
			}
			public string page1_title
			{
				get { return mgr.GetString("page1_title"); }
			}
			public string page2_title
			{
				get { return mgr.GetString("page2_title"); }
			}
			public string page1_content
			{
				get { return mgr.GetString("page1_content"); }
			}
			public string page2_content
			{
				get { return mgr.GetString("page2_content"); }
			}
		}

		public static void Main(string[] args)
		{
			// choose a skin or site "look" to present
			string skin = "blue";

			// allow overrides for language and skin from arguments on command line
			string language = null;
			if ( args.Length > 0 ) 
			{
				language = args[0];
			}
			if ( args.Length > 1)
			{
				if ( "blue".Equals(args[1]) || "red".Equals(args[1]) )
					skin = args[1];
			}

			TryToSetRequestedLocale(language);

			// load strings from a properties files like en.strings
			ResourceManager resMgr = new ResourceManager("ST.Examples.i18n.Content.Strings", typeof(ResourceWrapper).Assembly);
			ResourceWrapper strings = new ResourceWrapper(resMgr);

			// get a template group rooted at appropriate skin
			string absoluteSkinRootDirectoryName = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, skin);
			StringTemplateGroup templates = new StringTemplateGroup("test", absoluteSkinRootDirectoryName);

			// generate some pages; every page gets strings table to pull strings from
			StringTemplate page1ST = templates.GetInstanceOf("page1");
			page1ST.SetAttribute("strings", strings);
			StringTemplate page2ST = templates.GetInstanceOf("page2");
			page2ST.SetAttribute("strings", strings);

			// render to text
			Console.Out.WriteLine(page1ST);
			Console.Out.WriteLine(page2ST);
		}

		static private void TryToSetRequestedLocale(string language)
		{
			if (language != null)
			{
				try 
				{
					CultureInfo selectedLocale = new CultureInfo(language);
					Thread.CurrentThread.CurrentUICulture = selectedLocale;
				}
				catch
				{
					// use default locale
				}
			}
		}
	}
}