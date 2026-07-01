using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetKiwi.Backend;
using System;
using System.Text;

namespace NetKiwi.Test
{
    [TestClass]
    public class UnitTest
    {
        //static string modelPath = "";

        public Kiwi InitModel()
        {
            var builder = new KiwiBuilder();
            return builder.Build();
        }

        [TestMethod]
        public void TestInitialize()
        {
            var kiwi = InitModel();
        }

        [TestMethod]
        public void TestAnalyze()
        {
            var kiwi = InitModel();
            var res = kiwi.Analyze("테스트입니다.");
            Assert.IsTrue(res[0].morphs.Length > 0);
            Assert.IsTrue(res[0].morphs[0].form == "테스트");
            Assert.IsTrue(res[0].morphs[0].tag == "NNG");
        }

        [TestMethod]
        public void TestAnalyzeMulti()
        {
            var kiwi = InitModel();
            string[] arr = new string[100];
            for (int i = 0; i < 100; ++i)
            {
                arr[i] = String.Format("테스트 {0}입니다.", i);
            }

            kiwi.AnalyzeMulti((i) =>
            {
                if (i >= arr.Length) return null;
                return arr[i];
            }, (i, res) =>
            {
                Assert.IsTrue(res[0].morphs[0].form == "테스트");
                Assert.IsTrue(res[0].morphs[1].form == String.Format("{0}", i));
                return 0;
            });
        }

        [TestMethod]
        [Ignore("TestExtractWords skipped: Native kiwi_builder_extract_words_w causes AccessViolationException. Investigation required.")]
        public void TestExtractWords()
        {
            var kiwib = new KiwiBuilder();
            string[] arr = new string[100];
            for (int i = 0; i < 100; ++i)
            {
                arr[i] = String.Format("이것은 {0}번째 킼윜입니다.", i);
            }

            var words = kiwib.ExtractWords((i) =>
            {
                if (i >= arr.Length) return null;
                return arr[i];
            });

            Assert.IsNotNull(words);
        }

        [TestMethod]
        [Ignore("TestExtractAddWords skipped: Native kiwi_builder_extract_add_words_w causes AccessViolationException. Investigation required.")]
        public void TestExtractAddWords()
        {
            var kiwib = new KiwiBuilder();
            
            string[] arr = new string[100];
            for (int i = 0; i < 100; ++i)
            {
                arr[i] = String.Format("이것은 {0}번째 킼윜입니다.", i);
            }

            var words = kiwib.ExtractAddWords((i) =>
            {
                if (i >= arr.Length) return null;
                return arr[i];
            });

            var kiwi = kiwib.Build();

            var res = kiwi.Analyze("이것은 킼윜입니다.");
        }
    }
}