﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using NetKiwi;
using NetKiwi.Backend;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace test
{
    [TestClass]
    public class UnitTest
    {
        static string modelPath = "model/";

        public SharpKiwi InitModel()
        {
            SharpKiwi sharpKiwi = new SharpKiwi();
            return sharpKiwi;
        }

        [TestMethod]
        public void TestInitialize()
        {
            SharpKiwi sharpKiwi = new();
            Assert.IsTrue(sharpKiwi.kiwi != null);
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

        /*
        [TestMethod]
        public void TestAnalyzeMulti()
        {
            SharpKiwi sharpKiwi = new SharpKiwi();
            string[] arr = new string[100];
            for (int i = 0; i < 100; ++i)
            {
                arr[i] = String.Format("테스트 {0}입니다.", i);
            }

            var res = sharpKiwi.AnalyzeMulti(arr);
        }
        */

        [TestMethod]
        public void TestExtractWords()
        {
            SharpKiwi sharpKiwi = new SharpKiwi();
            string[] arr = new string[100];
            for (int i = 0; i < 100; ++i)
            {
                arr[i] = String.Format("이것은 {0}번째 킼윜입니다.", i);
            }

            var words = sharpKiwi.ExtractWords(arr);
        }

        [TestMethod]
        public void TestExtractAddWords()
        {
            SharpKiwi sharpKiwi = new SharpKiwi();
            string[] arr = new string[100];
            for (int i = 0; i < 100; ++i)
            {
                arr[i] = String.Format("이것은 {0}번째 킼윜입니다.", i);
            }

            var words = sharpKiwi.ExtractWords(arr);
            Console.WriteLine(words);
            var res = sharpKiwi.Analyze("이것은 킼윜입니다.");
            Console.WriteLine(res);
            Assert.IsTrue(res[0].morphs.Length > 0);
            Assert.IsTrue(res[0].morphs[0].form == "이것");
        }
    }
}
