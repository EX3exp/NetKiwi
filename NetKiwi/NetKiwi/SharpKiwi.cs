using NetKiwi.Backend;
using System.Runtime.InteropServices;
namespace NetKiwi
{
    /// <summary>
    /// Main Class for Kiwi C# wrapper. 
    /// </summary>
    public class SharpKiwi: IDisposable
    {
        static string modelPath = "netkiwi/model/";
        private Kiwi _kiwi;
        readonly KiwiCAPIBase kiwiCAPI;
        public Kiwi kiwi
        {
            get { return _kiwi; }
            private set { 
                if (value == null) throw new ArgumentNullException("Kiwi cannot be null");
                _kiwi = value; 
            }
        }
        private KiwiBuilder _kiwiBuilder;
        private KiwiBuilder builder
        {
            get { return _kiwiBuilder; }
            set
            {
                if (value == null) throw new ArgumentNullException("KiwiBuilder cannot be null");
                _kiwiBuilder = value;
            }
        }

        /// <summary>
        /// Main Constructor for SharpKiwi. 
        /// </summary>
        /// <exception cref="KiwiException"></exception>
        public SharpKiwi()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                kiwiCAPI = new KiwiCAPIWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                kiwiCAPI = new KiwiCAPILinux();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
                {
                    kiwiCAPI = new KiwiCAPIOSXArm();
                }
                else
                {
                    kiwiCAPI = new KiwiCAPIOSXIntel();
                }
                    
            }
            else
            {
                throw new KiwiException("Unsupported OS");
            }

            KiwiLoader.LoadDll(null);
            builder = new KiwiBuilder(kiwiCAPI, modelPath);
            this.kiwi = builder.Build();
        }

        /// <summary>
        /// Analyzes given text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Extract words from given texts.
        /// </summary>
        /// <param name="texts"></param>
        /// <returns></returns>
        public ExtractedWord[] ExtractWords(string[] texts)
        {
            var words = builder.ExtractWords((i) =>
            {
                if (i >= texts.Length) return null;
                return texts[i];
            });

            return words;
        }

        ~SharpKiwi()
        {
            _kiwi.Dispose();
            _kiwiBuilder.Dispose();
        }

        void IDisposable.Dispose()
        {
            _kiwi.Dispose();
            _kiwiBuilder.Dispose();
            
        }
    }
}
