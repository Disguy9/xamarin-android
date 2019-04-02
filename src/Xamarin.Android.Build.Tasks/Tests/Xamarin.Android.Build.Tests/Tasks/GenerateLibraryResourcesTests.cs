﻿using NUnit.Framework;
using System.IO;
using Xamarin.Android.Tasks;

namespace Xamarin.Android.Build.Tests
{
	[TestFixture]
	public class GenerateLibraryResourcesTests : BaseTest
	{
		string temp;
		string main_r_txt;
		string r_txt;
		string manifest;
		string output_dir;

		const string app_r_txt_contents =
@"int anim foo_anim 0x7f010000
int attr foo_attr 0x7f030000
int bool foo_bool 0x7f040000
int color foo_color 0x7f050000
int dimen foo_dimen 0x7f060000
int drawable foo_drawable 0x7f070007
int id foo_id 0x7f080000
int integer foo_integer 0x7f090000
int interpolator foo_interpolator 0x7f0a0000
int layout foo_layout 0x7f0b0000
int string foo_string 0x7f0c0000
int style foo_style 0x7f0d0000
int[] styleable Foo_styleable { 0x010100b3, 0x010100f4 }
int styleable Foo_styleable_bar 0
int styleable Foo_styleable_baz 1";

		const string AndroidManifest =
			"<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" android:versionCode=\"1\" android:versionName=\"1.0\" package=\"com.mycompanyname.foo\" />";

		const string Header = @"/* AUTO-GENERATED FILE. DO NOT MODIFY.
 *
 * This class was automatically generated by
 * Xamarin.Android from the resource data it found.
 * It should not be modified by hand.
 */
";

		[SetUp]
		public void Setup ()
		{
			string tempDirectoryName = Path.Combine ("temp", TestName);
			temp = Path.Combine (Root, tempDirectoryName);
			Directory.CreateDirectory (temp);
			main_r_txt = Path.Combine (temp, "app-r.txt");
			manifest = Path.Combine (temp, "AndroidManifest.xml");
			r_txt = Path.Combine (temp, "R.txt");
			output_dir = Path.Combine (temp, "src");

			File.WriteAllText (main_r_txt, app_r_txt_contents);
			File.WriteAllText (manifest, AndroidManifest);

			var references = CreateFauxReferencesDirectory (Path.Combine (tempDirectoryName, "references"), new [] {
				new ApiInfo { Id = "28", Level = 28, Name = "Pie", FrameworkVersion = "v9.0",  Stable = true },
			});
			MonoAndroidHelper.RefreshSupportedVersions (new [] {
				Path.Combine (references, "MonoAndroid"),
			});
		}

		[TearDown]
		public void TearDown ()
		{
			Directory.Delete (temp, recursive: true);
		}

		string TempDirectory () => Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());

		string ReplaceLineEndings (string s) => s.Replace ("\r\n", "\n");

		void RunTask (string expected = null, bool fileExists = true)
		{
			var task = new GenerateLibraryResources {
				BuildEngine = new MockBuildEngine (TestContext.Out),
				ResourceSymbolsTextFile = main_r_txt,
				OutputDirectory = output_dir,
				LibraryTextFiles = new [] { r_txt },
				ManifestFiles = new [] { manifest },
			};
			Assert.IsTrue (task.Execute (), "Execute() failed!");

			var r_java = Path.Combine (output_dir, "com", "mycompanyname", "foo", "R.java");
			if (fileExists) {
				FileAssert.Exists (r_java);
				Assert.AreEqual (ReplaceLineEndings (Header + expected), ReplaceLineEndings (File.ReadAllText (r_java)));
			} else {
				FileAssert.DoesNotExist (r_java);
			}
		}

		[Test]
		public void Anim ()
		{
			File.WriteAllText (r_txt, "int anim foo_anim 0x00000000");
			RunTask (expected:
@"package com.mycompanyname.foo;

public final class R {
	public static final class anim {
		public static final int foo_anim = 0x7f010000;
	}
}
");
		}

		[Test]
		public void Id ()
		{
			File.WriteAllText (r_txt, "int id foo_id 0x00000000");
			RunTask (expected:
@"package com.mycompanyname.foo;

public final class R {
	public static final class id {
		public static final int foo_id = 0x7f080000;
	}
}
");
		}

		[Test]
		public void Styleable ()
		{
			File.WriteAllText (r_txt,
@"int[] styleable Foo_styleable { 0x00000000, 0x00000000 }
int styleable Foo_styleable_bar 0
int styleable Foo_styleable_baz 1");
			RunTask (expected:
@"package com.mycompanyname.foo;

public final class R {
	public static final class styleable {
		public static final int[] Foo_styleable = new int[] { 0x010100b3, 0x010100f4 };
		public static final int Foo_styleable_bar = 0;
		public static final int Foo_styleable_baz = 1;
	}
}
");
		}

		[Test]
		public void NoMatches ()
		{
			File.WriteAllText (r_txt, "int id asdf 0x00000000");
			RunTask (expected:
@"package com.mycompanyname.foo;

public final class R {
}
");
		}

		//This one should just write the main R.txt file
		[Test]
		public void TextFileDoesNotExist ()
		{
			RunTask (expected:
@"package com.mycompanyname.foo;

public final class R {
	public static final class anim {
		public static final int foo_anim = 0x7f010000;
	}
	public static final class attr {
		public static final int foo_attr = 0x7f030000;
	}
	public static final class bool {
		public static final int foo_bool = 0x7f040000;
	}
	public static final class color {
		public static final int foo_color = 0x7f050000;
	}
	public static final class dimen {
		public static final int foo_dimen = 0x7f060000;
	}
	public static final class drawable {
		public static final int foo_drawable = 0x7f070007;
	}
	public static final class id {
		public static final int foo_id = 0x7f080000;
	}
	public static final class integer {
		public static final int foo_integer = 0x7f090000;
	}
	public static final class interpolator {
		public static final int foo_interpolator = 0x7f0a0000;
	}
	public static final class layout {
		public static final int foo_layout = 0x7f0b0000;
	}
	public static final class string {
		public static final int foo_string = 0x7f0c0000;
	}
	public static final class style {
		public static final int foo_style = 0x7f0d0000;
	}
	public static final class styleable {
		public static final int[] Foo_styleable = new int[] { 0x010100b3, 0x010100f4 };
		public static final int Foo_styleable_bar = 0;
		public static final int Foo_styleable_baz = 1;
	}
}
");
		}

		[Test]
		public void ManifestDoesNotExist ()
		{
			File.Delete (manifest);
			File.WriteAllText (r_txt, "int id asdf 0x00000000");
			RunTask (fileExists: false);
		}
	}
}
