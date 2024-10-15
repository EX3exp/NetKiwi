﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetKiwi.Backend;
namespace NetKiwi
{
    public class SharpKiwi
    {
        static string modelPath = "model/";
        private Kiwi _kiwi;
        public Kiwi kiwi
        {
            get { return _kiwi; }
            private set { 
                if (value == null) throw new ArgumentNullException("Kiwi cannot be null");
                _kiwi = value; 
            }
        }
        private KiwiBuilder _kiwiBuilder;
        public KiwiBuilder builder
        {
            get { return _kiwiBuilder; }
            private set
            {
                if (value == null) throw new ArgumentNullException("KiwiBuilder cannot be null");
                _kiwiBuilder = value;
            }
        }

        public SharpKiwi()
        {
            KiwiLoader.LoadDll(null);
            builder = new KiwiBuilder(modelPath);
            this.kiwi = builder.Build();
        }
        public Result[] Analyze(string text)
        {
            return kiwi.Analyze(text);
        }

        /*
        public Result[][] AnalyzeMulti(string[] texts)
        {
            List<Result[]> results = new List<Result[]>();

            

            kiwi.AnalyzeMulti(
                (i) =>
                {
                    if (i >= texts.Length) return null;
                    return texts[i];
                },
                (i, result) =>
                {
                    results.Add(result);
                    return 0;
                }
            );

            return results.ToArray();
        }
        */
        public ExtractedWord[] ExtractWords(string[] texts)
        {
            var words = builder.ExtractWords((i) =>
            {
                if (i >= texts.Length) return null;
                return texts[i];
            });

            return words;
        }

        

    }
}