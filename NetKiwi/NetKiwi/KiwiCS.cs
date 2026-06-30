using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;


namespace NetKiwi.Backend
{
    // based on https://github.com/bab2min/kiwi-gui

    /*
     Copyright (C) 2024 Min-chul Lee
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.
 
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Lesser General Public License for more details.
 
    You should have received a copy of the GNU Lesser General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>.
     
     */

    using CString = IntPtr;
    using KiwiHandle = IntPtr;
    using KiwiBuilderHandle = IntPtr;
    using KiwiResHandle = IntPtr;
    using KiwiWsHandle = IntPtr;
    using KiwiSsHandle = IntPtr;
    using KiwiTypoHandle = IntPtr;
    using KiwiJoinerHandle = IntPtr;
    using KiwiMorphsetHandle = IntPtr;
    using KiwiPretokenizedHandle = IntPtr;
    using KiwiPreparedTypoHandle = IntPtr;
    using KiwiSwtokenizerHandle = IntPtr;


    internal class Utf8String : IDisposable
    {
        private IntPtr _ptr;
        private bool _disposed = false;

        public IntPtr IntPtr => _ptr;
        public int BufferLength { get { return iBufferSize; } }
        int iBufferSize;
        public Utf8String(string aValue)
        {
            if (aValue == null)
            {
                _ptr = IntPtr.Zero;
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(aValue);
                _ptr = Marshal.AllocHGlobal(bytes.Length + 1);
                Marshal.Copy(bytes, 0, _ptr, bytes.Length);
                Marshal.WriteByte(_ptr, bytes.Length, 0);
                iBufferSize = bytes.Length + 1;
            }
        }
        public void Dispose()
        {
            if (!_disposed && _ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_ptr);
                _ptr = IntPtr.Zero;
                _disposed = true;
            }
        }

        ~Utf8String()
        {
            Dispose();
        }
    }

    internal class Utf16String : IDisposable
    {
        private bool _disposed = false;

        IntPtr _ptr;
        public IntPtr IntPtr => _ptr;
        public int BufferLength { get { return iBufferSize; } }
        int iBufferSize;
        public Utf16String(string aValue)
        {
            if (aValue == null)
            {
                _ptr = IntPtr.Zero;
            }
            else
            {
                byte[] bytes = new UnicodeEncoding().GetBytes(aValue);
                _ptr = Marshal.AllocHGlobal(bytes.Length + 2);
                Marshal.Copy(bytes, 0, _ptr, bytes.Length);
                Marshal.WriteByte(_ptr, bytes.Length, 0);
                Marshal.WriteByte(_ptr, bytes.Length + 1, 0);
                iBufferSize = bytes.Length + 2;
            }
        }
        public void Dispose()
        {
            if (!_disposed &&  _ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_ptr);
                _ptr = IntPtr.Zero;
                _disposed = true;
            }

        }

        ~Utf16String()
        {
            Dispose();
        }
    }

    internal class Utf8StringArray : IDisposable
    {
        private bool _disposed = false;
        private IntPtr _iPtr, _bPtr;
        public IntPtr IntPtr => _bPtr;

        public Utf8StringArray(string[] aValue)
        {
            if (aValue == null)
            {
                _iPtr = IntPtr.Zero;
            }
            else
            {
                int totalLength = 0;
                byte[][] pool = new byte[aValue.Length][];
                for (int i = 0; i < aValue.Length; i++)
                {
                    pool[i] = Encoding.UTF8.GetBytes(aValue[i]);
                    totalLength += pool[i].Length + 1;
                }
                _iPtr = Marshal.AllocHGlobal(totalLength);
                _bPtr = Marshal.AllocHGlobal(IntPtr.Size * pool.Length);

                int offset = 0;
                for (int i = 0; i < aValue.Length; i++)
                {
                    Marshal.Copy(pool[i], 0, _iPtr + offset, pool[i].Length);
                    Marshal.WriteByte(_iPtr, pool[i].Length, 0);
                    Marshal.WriteIntPtr(_bPtr, IntPtr.Size * i, _iPtr + offset);
                    offset += pool[i].Length + 1;
                }
            }
        }
        public void Dispose()
        {
            if (!_disposed && _iPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_iPtr);
                Marshal.FreeHGlobal(_bPtr);
                _iPtr = IntPtr.Zero;
                _bPtr = IntPtr.Zero;
                _disposed = true;
            }
        }

        ~Utf8StringArray()
        {
            Dispose();
        }
    }
    public class KiwiException : Exception
    {
        public KiwiException()
        {
        }

        public KiwiException(string message) : base(message)
        {
        }

        public KiwiException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Kiwi Model's output<br></br>
    /// </summary>
    public struct Result
    {
        public Token[] morphs;
        public float prob;
    }

    public struct ExtractedWord
    {
        public string word;
        public float score, posScore;
        public int freq;
    }


    /// <summary>
    /// Token Class<br></br>
    /// </summary>
    public struct Token
    {
        public string form;
        public string tag;
        public uint chrPosition; /* 시작 위치(UTF16 문자 기준) */
        public uint wordPosition; /* 어절 번호(공백 기준)*/
        public uint sentPosition; /* 문장 번호*/
        public uint lineNumber; /* 줄 번호*/
        public ushort length; /* 길이(UTF16 문자 기준) */
        public byte senseId; /* 의미 번호 */
        public float score; /* 해당 형태소의 언어모델 점수 */
        public float typoCost; /* 오타가 교정된 경우 오타 비용. 그렇지 않은 경우 0 */
        public uint typoFormId; /* 교정 전 오타의 형태에 대한 정보 (typoCost가 0인 경우 의미 없음) */
        public uint pairedToken; /* SSO, SSC 태그에 속하는 형태소의 경우 쌍을 이루는 반대쪽 형태소의 위치(-1인 경우 해당하는 형태소가 없는 것을 뜻함) */
        public uint subSentPosition; /* 인용부호나 괄호로 둘러싸인 하위 문장의 번호. 1부터 시작. 0인 경우 하위 문장이 아님을 뜻함 */
        public Dialect dialect; /* 방언 정보 */
    }

    public struct Morpheme
    {
        public string form;
        public string tag;
        public byte senseId; /* 의미 번호 */
        public float userScore; /* 사용자 정의 점수 */
        public uint lmMorphemeId; /* 언어모델 형태소 ID */
        public uint origMorphemeId; /* 원래 형태소 ID */
        public Dialect dialect; /* 방언 정보 */
    }

    public enum Option
    {
        IntegrateAllomorph = 1 << 0,
        LoadDefaultDict = 1 << 1,
        LoadTypoDict = 1 << 2,
        LoadMultiDict = 1 << 3,
    }

    public enum ModelType
    {
        Default = 0x0000,
        Largest = 0x0100,
        Knlm = 0x0200,
        Sbg = 0x0300,
        Cong = 0x0400,
        CongGlobal = 0x0500,
    }

    public enum Match
    {
        Url = 1 << 0,
        Email = 1 << 1,
        Hashtag = 1 << 2,
        Mention = 1 << 3,
        Serial = 1 << 4,
        Emoji = 1 << 5,

        OovRuleOnly = 0 << 8,
        OovChrModel = 1 << 8,
        OovChrFreqModel = 2 << 8,
        OovChrFreqBranchModel = 3 << 8,
        OovMask = 3 << 8,

        NormalizeCoda = 1 << 16,

        JoinNounPrefix = 1 << 17,
        JoinNounSuffix = 1 << 18,
        JoinVerbSuffix = 1 << 19,
        JoinAdjSuffix = 1 << 20,
        JoinAdvSuffix = 1 << 21,
        JoinVSuffix = JoinVerbSuffix | JoinAdjSuffix,
        JoinAffix = JoinNounPrefix | JoinNounSuffix | JoinVSuffix | JoinAdvSuffix,
        SplitComplex = 1 << 22,
        ZCoda = 1 << 23,
        CompatibleJamo = 1 << 24,
        SplitSaisiot = 1 << 25,
        MergeSaisiot = 1 << 26,
        JoinParticleYo = 1 << 27,
        UseOldSplitter = 1 << 30,

        All = Url | Email | Hashtag | Mention | Serial | Emoji | ZCoda,
        AllWithNormalizing = All | NormalizeCoda,
    }

    public enum Dialect
    {
        Standard = 0,
        Gyeonggi = 1 << 0,
        Chungcheong = 1 << 1,
        Gangwon = 1 << 2,
        Gyeongsang = 1 << 3,
        Jeolla = 1 << 4,
        Jeju = 1 << 5,
        Hwanghae = 1 << 6,
        Hamgyeong = 1 << 7,
        Pyeongan = 1 << 8,
        Archaic = 1 << 9,
        All = Archaic * 2 - 1,
    }

    public enum DefaultTypoSet
    {
        WithoutTypo = 0,
        BasicTypoSet = 1,
        ContinualTypoSet = 2,
        BasicTypoSetWithContinual = 3,
        LengtheningTypoSet = 4,
        BasicTypoSetWithContinualAndLengthening = 5,
        Dialect = 6,
    }


    public class TypoTransformer: IDisposable
    {
        private bool _disposed = false; 
        private bool readOnly = false;
        public KiwiTypoHandle inst;

        public TypoTransformer()
        {
            inst = KiwiCAPI.kiwi_typo_init();
        }

        public TypoTransformer(DefaultTypoSet defaultTypoSet)
        {
            readOnly = true;
            inst = KiwiCAPI.kiwi_typo_get_default((int)defaultTypoSet);
        }

        public int Add(string[] orig, string[] error, float cost, int condition)
        {
            if (readOnly)
            {
                throw new InvalidOperationException("default typo object cannot be modified!");
            }
            using (var origArray = new Utf8StringArray(orig))
            using (var errorArray = new Utf8StringArray(error))
            {
                return KiwiCAPI.kiwi_typo_add(inst, origArray.IntPtr, orig.Length, errorArray.IntPtr, error.Length, cost, condition);
            }
        }

        public PreparedTypoTransformer Prepare()
        {
            KiwiPreparedTypoHandle h = KiwiCAPI.kiwi_typo_prepare(inst);
            if (h == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
            return new PreparedTypoTransformer(h);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    if (inst != IntPtr.Zero && !readOnly)
                    {
                        KiwiCAPI.kiwi_typo_close(inst);
                        inst = IntPtr.Zero;
                    }
                }
                catch
                {
                    // Prevent exceptions from being thrown in the finalizer
                }
                _disposed = true;
            }
        }

        ~TypoTransformer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class PreparedTypoTransformer : IDisposable
    {
        internal KiwiPreparedTypoHandle inst;
        private bool _disposed = false;

        internal PreparedTypoTransformer(KiwiPreparedTypoHandle handle)
        {
            inst = handle;
        }

        public void Dispose()
        {
            if (!_disposed && inst != IntPtr.Zero)
            {
                KiwiCAPI.kiwi_prepared_typo_close(inst);
                inst = IntPtr.Zero;
                _disposed = true;
            }
        }

        ~PreparedTypoTransformer()
        {
            Dispose();
        }
    }
    public class KiwiBuilder : IDisposable
    {
        private bool _disposed = false;

        private string _modelPath; 

        public delegate string Reader(int id);

        public KiwiBuilderHandle inst => _inst;
        private KiwiBuilderHandle _inst;
        private Reader reader;
        private Tuple<int, string> readItem;

        
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    if (inst != IntPtr.Zero)
                    {
                        KiwiCAPI.kiwi_builder_close(inst);
                        _inst = IntPtr.Zero;
                    }
                }
                catch
                {
                    // Prevent exceptions from being thrown in the finalizer
                }
                _disposed = true;
            }
        }

        private static KiwiCAPI.CReader readerInst = (int id, IntPtr buf, IntPtr userData) =>
        {
            GCHandle handle = (GCHandle)userData;
            KiwiBuilder ki = handle.Target as KiwiBuilder;
            if (ki.readItem?.Item1 != id)
            {
                ki.readItem = new Tuple<int, string>(id, ki.reader(id));
            }
            if (buf == IntPtr.Zero)
            {
                return ki.readItem.Item2 ?.Length ?? 0;
            }

            using (var textStr = new Utf16String(ki.readItem.Item2))
            {
                KiwiCAPI.CopyMemory(buf, textStr.IntPtr, (uint)ki.readItem.Item2.Length * 2);
            }


            return 0;
        };
        public KiwiBuilder()
        {
            _ = typeof(KiwiCAPI).TypeHandle; // Load the KiwiCAPI type to ensure the static constructor is called

            string modelPath = Path.Combine(AppContext.BaseDirectory, "netkiwi", "models");
            if (!Directory.Exists(modelPath))
            {
                throw new DirectoryNotFoundException($"The model directory '{modelPath}' does not exist.");
            }

            _modelPath = modelPath;

            // Initialize the native builder instance with default parameters
            int numThreads = 0;
            Option options = Option.LoadDefaultDict | Option.LoadTypoDict | Option.LoadMultiDict | Option.IntegrateAllomorph;
            ModelType modelType = ModelType.Default;
            Dialect enabledDialects = Dialect.Standard;

            using (var pathStr = new Utf8String(modelPath))
            {
                _inst = KiwiCAPI.kiwi_builder_init(pathStr.IntPtr, numThreads, (int)options | (int)modelType, (int)enabledDialects);
                if (inst == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
            }
        }


        public KiwiBuilder(string modelPath,
            int numThreads = 0,
            Option options = Option.LoadDefaultDict | Option.LoadTypoDict | Option.LoadMultiDict | Option.IntegrateAllomorph,
            ModelType modelType = ModelType.Default,
            Dialect enabledDialects = Dialect.Standard)
        {
            using (var pathStr = new Utf8String(modelPath))
            {
                _inst = KiwiCAPI.kiwi_builder_init(pathStr.IntPtr, numThreads, (int)options | (int)modelType, (int)enabledDialects);
                if (inst == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
            }
        }

        public abstract class Stream
        {
            public abstract UIntPtr Read(IntPtr buffer, UIntPtr length);
            public abstract long Seek(long offset, int whence);
            public abstract void Close();

        }

        public delegate Stream StreamObjectFactory(string filename);

        public KiwiBuilder(StreamObjectFactory streamObjectFactory,
            int numThreads = 0,
            Option options = Option.LoadDefaultDict | Option.LoadTypoDict | Option.LoadMultiDict | Option.IntegrateAllomorph,
            ModelType modelType = ModelType.Default,
            Dialect enabledDialects = Dialect.Standard)
        {
            KiwiCAPI.StreamObjectFactory streamFactoryDelegate = (CString filename) =>
            {
                string fn = Marshal.PtrToStringAnsi(filename);
                var stream = streamObjectFactory(fn);

                // convert to C API delegates
                KiwiCAPI.StreamReadFunc readFunc = (IntPtr userData, IntPtr buffer, UIntPtr length) =>
                {
                    GCHandle handle = (GCHandle)userData;
                    Stream strm = handle.Target as Stream;
                    return strm.Read(buffer, length);
                };
                KiwiCAPI.StreamSeekFunc seekFunc = (IntPtr userData, long offset, int whence) =>
                {
                    GCHandle handle = (GCHandle)userData;
                    Stream strm = handle.Target as Stream;
                    return strm.Seek(offset, whence);
                };
                KiwiCAPI.StreamCloseFunc closeFunc = (IntPtr userData) =>
                {
                    GCHandle handle = (GCHandle)userData;
                    Stream strm = handle.Target as Stream;
                    strm.Close();
                    handle.Free();
                };
                GCHandle streamHandle = GCHandle.Alloc(stream);
                KiwiCAPI.StreamObject so = new KiwiCAPI.StreamObject()
                {
                    read = readFunc,
                    seek = seekFunc,
                    close = closeFunc,
                    userData = (IntPtr)streamHandle,
                };
                return so;
            };
            _inst = KiwiCAPI.kiwi_builder_init_stream(streamFactoryDelegate, numThreads, (int)options | (int)modelType, (int)enabledDialects);
            if (inst == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
        }

        public int AddWord(string word, string pos, float score)
        {
            using (var wordStr = new Utf8String(word))
            using (var posStr = new Utf8String(pos))
            {
                int ret = KiwiCAPI.kiwi_builder_add_word(inst, wordStr.IntPtr, posStr.IntPtr, score);
                if (ret < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
                return ret;
            }
        }

        public int LoadDictionary(string dictPath)
        {
            using (var pathStr = new Utf8String(dictPath))
            {
                int ret = KiwiCAPI.kiwi_builder_load_dict(inst, pathStr.IntPtr);
                if (ret < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
                return ret;
            }
        }
        public ExtractedWord[] ExtractWords(Reader reader, int minCnt = 5, int maxWordLen = 10, float minScore = 0.1f, float posThreshold = -3)
        {
            GCHandle handle = GCHandle.Alloc(this);
            this.reader = reader;
            
            readItem = null;
            KiwiWsHandle ret = KiwiCAPI.kiwi_builder_extract_words_w(inst, readerInst, (IntPtr)handle, minCnt, maxWordLen, minScore, posThreshold);
            handle.Free();
            if (inst == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
            ExtractedWord[] words = KiwiCAPI.ToExtractedWord(ret);
            KiwiCAPI.kiwi_ws_close(ret);
            return words;
        }

        public ExtractedWord[] ExtractAddWords(Reader reader, int minCnt = 5, int maxWordLen = 10, float minScore = 0.1f, float posThreshold = -3)
        {
            this.reader = reader;
            GCHandle handle = GCHandle.Alloc(this);
            
            readItem = null;

            KiwiWsHandle ret = KiwiCAPI.kiwi_builder_extract_add_words_w(inst, readerInst, (IntPtr)handle, minCnt, maxWordLen, minScore, posThreshold);
            handle.Free();
            if (inst == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
            ExtractedWord[] words = KiwiCAPI.ToExtractedWord(ret);
            KiwiCAPI.kiwi_ws_close(ret);
            return words;
        }

        public Kiwi Build()
        {
            KiwiHandle ret = KiwiCAPI.kiwi_builder_build(inst, (IntPtr)null, 0);
            if (ret == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
            return new Kiwi(ret);
        }

        public Kiwi Build(TypoTransformer typo, float typoCostThreshold = 2.5f)
        {
            KiwiHandle ret = KiwiCAPI.kiwi_builder_build(inst, typo.inst, typoCostThreshold);
            if (ret == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
            return new Kiwi(ret);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~KiwiBuilder()
        {
            Dispose(false);
        }
    }

    public class KiwiJoiner
    {
        private bool _disposed = false;
        private KiwiJoinerHandle inst;
        public KiwiJoiner(KiwiJoinerHandle _inst)
        {
            inst = _inst;
        }

        public string Get()
        {
            return Marshal.PtrToStringUni(KiwiCAPI.kiwi_joiner_get_w(inst));
        }

        public void Add(string form, string tag, int option = 1)
        {
            using (var formStr = new Utf8String(form))
            using (var tagStr = new Utf8String(tag))
            {
                if (KiwiCAPI.kiwi_joiner_add(inst, formStr.IntPtr, tagStr.IntPtr, option) < 0)
                    throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    if (inst != IntPtr.Zero)
                    {
                        if (KiwiCAPI.kiwi_joiner_close(inst) < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
                        inst = IntPtr.Zero;
                    }
                }
                catch
                {
                    // Prevent exceptions from being thrown in the finalizer
                }
                _disposed = true;
            }
        }

        ~KiwiJoiner()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    public class Kiwi : IDisposable
    {
        private bool _disposed = false;
        public delegate string Reader(int id);
        public delegate int Receiver(int id, Result[] res);

        private KiwiHandle inst;
        private Reader reader;
        private Receiver receiver;
        private Tuple<int, string> readItem;


        private static KiwiCAPI.CReader readerInst = (int id, IntPtr buf, IntPtr userData) =>
        {
            GCHandle handle = (GCHandle)userData;
            Kiwi ki = handle.Target as Kiwi;
            if (ki.readItem?.Item1 != id)
            {
                ki.readItem = new Tuple<int, string>(id, ki.reader(id));
            }
            if (buf == IntPtr.Zero)
            {
                return ki.readItem.Item2?.Length ?? 0;
            }
            using (var textStr = new Utf16String(ki.readItem.Item2))
            {
                KiwiCAPI.CopyMemory(buf, textStr.IntPtr, (uint)ki.readItem.Item2.Length * 2);
            }
            return 0;
        };


        private static KiwiCAPI.CReceiver receiverInst = (int id, KiwiResHandle kiwi_res, IntPtr userData) =>
        {
            GCHandle handle = (GCHandle)userData;
            Kiwi ki = handle.Target as Kiwi;
            return ki.receiver(id, KiwiCAPI.ToResult(kiwi_res));
        };

        public Kiwi(KiwiHandle _inst)
        {
            inst = _inst;
        }

        public static string Version()
        {
            return Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_version());
        }

        public Result[] Analyze(string text, int topN = 1, Match matchOptions = Match.All, Dialect allowedDialects = Dialect.Standard, float dialectCost = 3.0f, PreparedTypoTransformer typoTransformer = null, float typoThreshold = 2.5f)
        {
            KiwiCAPI.AnalyzeOption option = new KiwiCAPI.AnalyzeOption
            {
                matchOptions = (int)matchOptions,
                blocklist = IntPtr.Zero,
                openEnding = 0,
                allowedDialects = (int)allowedDialects,
                dialectCost = dialectCost,
                typoTransformer = typoTransformer?.inst ?? IntPtr.Zero,
                typoThreshold = typoThreshold
            };
            using (var textStr = new Utf16String(text))
            {
                KiwiResHandle res = KiwiCAPI.kiwi_analyze_w(inst, textStr.IntPtr, topN, option, IntPtr.Zero);
                if (res == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
                Result[] ret = KiwiCAPI.ToResult(res);
                KiwiCAPI.kiwi_res_close(res);
                return ret;
            }
        }

        public void AnalyzeMulti(Reader reader, Receiver receiver, int topN = 1, Match matchOptions = Match.All, Dialect allowedDialects = Dialect.Standard, float dialectCost = 3.0f, PreparedTypoTransformer typoTransformer = null, float typoThreshold = 2.5f)
        {
            GCHandle handle = GCHandle.Alloc(this);
            this.reader = reader;
            this.receiver = receiver;
            readItem = null;
            KiwiCAPI.AnalyzeOption option = new KiwiCAPI.AnalyzeOption
            {
                matchOptions = (int)matchOptions,
                blocklist = IntPtr.Zero,
                openEnding = 0,
                allowedDialects = (int)allowedDialects,
                dialectCost = dialectCost,
                typoTransformer = typoTransformer?.inst ?? IntPtr.Zero,
                typoThreshold = typoThreshold
            };
            int ret = KiwiCAPI.kiwi_analyze_mw(inst, readerInst, receiverInst, (IntPtr)handle, topN, option);
            handle.Free();
            if (ret < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
        }

        public KiwiJoiner NewJoiner(bool lmSearch = true)
        {
            var h = KiwiCAPI.kiwi_new_joiner(inst, lmSearch ? 1 : 0);
            if (h == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
            return new KiwiJoiner(h);
        }

        public bool IntegrateAllomorph
        {
            get { return KiwiCAPI.kiwi_get_global_config(inst).integrateAllomorph != 0; }
            set
            {
                var config = KiwiCAPI.kiwi_get_global_config(inst);
                config.integrateAllomorph = (byte)(value ? 1 : 0);
                KiwiCAPI.kiwi_set_global_config(inst, config);
            }
        }

        public int MaxUnkFormSize
        {
            get { return (int)KiwiCAPI.kiwi_get_global_config(inst).maxUnkFormSize; }
            set
            {
                var config = KiwiCAPI.kiwi_get_global_config(inst);
                config.maxUnkFormSize = (uint)value;
                KiwiCAPI.kiwi_set_global_config(inst, config);
            }
        }

        public int SpaceTolerance
        {
            get { return (int)KiwiCAPI.kiwi_get_global_config(inst).spaceTolerance; }
            set
            {
                var config = KiwiCAPI.kiwi_get_global_config(inst);
                config.spaceTolerance = (uint)value;
                KiwiCAPI.kiwi_set_global_config(inst, config);
            }
        }

        public float CutOffThreshold
        {
            get { return KiwiCAPI.kiwi_get_global_config(inst).cutOffThreshold; }
            set
            {
                var config = KiwiCAPI.kiwi_get_global_config(inst);
                config.cutOffThreshold = value;
                KiwiCAPI.kiwi_set_global_config(inst, config);
            }
        }

        public float OovRuleScale
        {
            get { return KiwiCAPI.kiwi_get_global_config(inst).oovRuleScale; }
            set
            {
                var config = KiwiCAPI.kiwi_get_global_config(inst);
                config.oovRuleScale = value;
                KiwiCAPI.kiwi_set_global_config(inst, config);
            }
        }

        public float OovRuleBias
        {
            get { return KiwiCAPI.kiwi_get_global_config(inst).oovRuleBias; }
            set
            {
                var config = KiwiCAPI.kiwi_get_global_config(inst);
                config.oovRuleBias = value;
                KiwiCAPI.kiwi_set_global_config(inst, config);
            }
        }

        public float SpacePenalty
        {
            get { return KiwiCAPI.kiwi_get_global_config(inst).spacePenalty; }
            set
            {
                var config = KiwiCAPI.kiwi_get_global_config(inst);
                config.spacePenalty = value;
                KiwiCAPI.kiwi_set_global_config(inst, config);
            }
        }

        public float TypoCostWeight
        {
            get { return KiwiCAPI.kiwi_get_global_config(inst).typoCostWeight; }
            set
            {
                var config = KiwiCAPI.kiwi_get_global_config(inst);
                config.typoCostWeight = value;
                KiwiCAPI.kiwi_set_global_config(inst, config);
            }
        }
        /// <summary>
        /// 모델 사전에서 조건에 맞는 형태소를 찾아 그 ID를 반환합니다.
        /// </summary>
        /// <param name="form">형태소 형태 (null일 경우 모든 형태)</param>
        /// <param name="tag">품사 태그 (null일 경우 모든 품사)</param>
        /// <param name="senseId">의미 번호 (-1일 경우 모든 의미)</param>
        /// <param name="maxCount">반환할 최대 개수</param>
        /// <returns>찾은 형태소 ID 배열</returns>
        public uint[] FindMorphemes(string form = null, string tag = null, int senseId = -1, int maxCount = 100)
        {
            IntPtr ptr = Marshal.AllocHGlobal(sizeof(uint) * maxCount);
            try
            {
                using (var formStr = new Utf8String(form))
                using (var tagStr = new Utf8String(tag))
                {
                    int count = KiwiCAPI.kiwi_find_morphemes(inst, formStr.IntPtr, tagStr.IntPtr, senseId, ptr, maxCount);
                    if (count < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));

                    uint[] result = new uint[count];
                    for (int i = 0; i < count; i++)
                    {
                        result[i] = (uint)Marshal.ReadInt32(ptr, i * sizeof(uint));
                    }
                    return result;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    if (inst != IntPtr.Zero)
                    {
                        if (KiwiCAPI.kiwi_close(inst) < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));
                        inst = IntPtr.Zero;
                    }
                }
                catch
                {
                    // Prevent exceptions from being thrown in the finalizer
                }
                _disposed = true;
            }
        }
        ~Kiwi()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    /// <summary>
        /// 모델 사전에서 특정 접두사로 시작하는 형태소를 찾아 그 ID를 반환합니다.
        /// </summary>
        /// <param name="formPrefix">형태소 형태 접두사</param>
        /// <param name="tag">품사 태그 (null일 경우 모든 품사)</param>
        /// <param name="senseId">의미 번호 (-1일 경우 모든 의미)</param>
        /// <param name="maxCount">반환할 최대 개수</param>
        /// <returns>찾은 형태소 ID 배열</returns>
        public uint[] FindMorphemesWithPrefix(string formPrefix, string tag = null, int senseId = -1, int maxCount = 100)
        {
            IntPtr ptr = Marshal.AllocHGlobal(sizeof(uint) * maxCount);
            try
            {
                using (var formStr = new Utf8String(formPrefix))
                using (var tagStr = new Utf8String(tag))
                {
                    int count = KiwiCAPI.kiwi_find_morphemes_with_prefix(inst, formStr.IntPtr, tagStr.IntPtr, senseId, ptr, maxCount);
                    if (count < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));

                    uint[] result = new uint[count];
                    for (int i = 0; i < count; i++)
                    {
                        result[i] = (uint)Marshal.ReadInt32(ptr, i * sizeof(uint));
                    }
                    return result;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// 형태소 ID로부터 형태소 정보를 조회합니다.
        /// </summary>
        /// <param name="morphId">형태소 ID</param>
        /// <returns>형태소 정보</returns>
        public Morpheme GetMorphemeInfo(uint morphId)
        {
            CString formPtr = KiwiCAPI.kiwi_get_morpheme_form_w(inst, morphId);
            if (formPtr == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));

            string form = Marshal.PtrToStringUni(formPtr);
            KiwiCAPI.kiwi_free_morpheme_form(formPtr);

            Morpheme morpheme = new Morpheme();
            var mi = KiwiCAPI.kiwi_get_morpheme_info(inst, morphId);

            morpheme.form = form;
            morpheme.tag = Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_tag_to_string(inst, mi.tag));
            morpheme.senseId = mi.senseId;
            morpheme.userScore = mi.userScore;
            morpheme.lmMorphemeId = mi.lmMorphemeId;
            morpheme.origMorphemeId = mi.origMorphemeId;
            morpheme.dialect = (Dialect)mi.dialect;
            return morpheme;
        }

        /// <summary>
        /// 주어진 형태소와 가장 유사한 형태소들을 반환합니다.
        /// </summary>
        /// <param name="morphId">형태소 ID</param>
        /// <param name="topN">반환할 최대 개수</param>
        /// <returns>유사한 형태소 ID와 점수의 배열</returns>
        public (uint id, float score)[] MostSimilarWords(uint morphId, int topN = 10)
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<KiwiCAPI.SimilarityPair>() * topN);
            try
            {
                int count = KiwiCAPI.kiwi_cong_most_similar_words(inst, morphId, ptr, topN);
                if (count < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));

                var result = new (uint, float)[count];
                for (int i = 0; i < count; i++)
                {
                    var pair = Marshal.PtrToStructure<KiwiCAPI.SimilarityPair>(ptr + i * Marshal.SizeOf<KiwiCAPI.SimilarityPair>());
                    result[i] = (pair.id, pair.score);
                }
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// 두 형태소 간의 유사도를 반환합니다.
        /// </summary>
        /// <param name="morphId1">첫 번째 형태소 ID</param>
        /// <param name="morphId2">두 번째 형태소 ID</param>
        /// <returns>유사도 점수</returns>
        public float WordSimilarity(uint morphId1, uint morphId2)
        {
            return KiwiCAPI.kiwi_cong_similarity(inst, morphId1, morphId2);
        }

        /// <summary>
        /// 주어진 문맥과 가장 유사한 문맥들을 반환합니다.
        /// </summary>
        /// <param name="contextId">문맥 ID</param>
        /// <param name="topN">반환할 최대 개수</param>
        /// <returns>유사한 문맥 ID와 점수의 배열</returns>
        public (uint id, float score)[] MostSimilarContexts(uint contextId, int topN = 10)
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<KiwiCAPI.SimilarityPair>() * topN);
            try
            {
                int count = KiwiCAPI.kiwi_cong_most_similar_contexts(inst, contextId, ptr, topN);
                if (count < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));

                var result = new (uint, float)[count];
                for (int i = 0; i < count; i++)
                {
                    var pair = Marshal.PtrToStructure<KiwiCAPI.SimilarityPair>(ptr + i * Marshal.SizeOf<KiwiCAPI.SimilarityPair>());
                    result[i] = (pair.id, pair.score);
                }
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// 두 문맥 간의 유사도를 반환합니다.
        /// </summary>
        /// <param name="contextId1">첫 번째 문맥 ID</param>
        /// <param name="contextId2">두 번째 문맥 ID</param>
        /// <returns>유사도 점수</returns>
        public float ContextSimilarity(uint contextId1, uint contextId2)
        {
            return KiwiCAPI.kiwi_cong_context_similarity(inst, contextId1, contextId2);
        }

        /// <summary>
        /// 주어진 문맥으로부터 예측되는 다음 단어들을 반환합니다.
        /// </summary>
        /// <param name="contextId">문맥 ID</param>
        /// <param name="topN">반환할 최대 개수</param>
        /// <returns>예측된 단어 ID와 점수의 배열</returns>
        public (uint id, float score)[] PredictWordsFromContext(uint contextId, int topN = 10)
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<KiwiCAPI.SimilarityPair>() * topN);
            try
            {
                int count = KiwiCAPI.kiwi_cong_predict_words_from_context(inst, contextId, ptr, topN);
                if (count < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));

                var result = new (uint, float)[count];
                for (int i = 0; i < count; i++)
                {
                    var pair = Marshal.PtrToStructure<KiwiCAPI.SimilarityPair>(ptr + i * Marshal.SizeOf<KiwiCAPI.SimilarityPair>());
                    result[i] = (pair.id, pair.score);
                }
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// 두 문맥의 차이로부터 예측되는 다음 단어들을 반환합니다.
        /// </summary>
        /// <param name="contextId">기본 문맥 ID</param>
        /// <param name="bgContextId">배경 문맥 ID</param>
        /// <param name="weight">가중치</param>
        /// <param name="topN">반환할 최대 개수</param>
        /// <returns>예측된 단어 ID와 점수의 배열</returns>
        public (uint id, float score)[] PredictWordsFromContextDiff(uint contextId, uint bgContextId, float weight, int topN = 10)
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<KiwiCAPI.SimilarityPair>() * topN);
            try
            {
                int count = KiwiCAPI.kiwi_cong_predict_words_from_context_diff(inst, contextId, bgContextId, weight, ptr, topN);
                if (count < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));

                var result = new (uint, float)[count];
                for (int i = 0; i < count; i++)
                {
                    var pair = Marshal.PtrToStructure<KiwiCAPI.SimilarityPair>(ptr + i * Marshal.SizeOf<KiwiCAPI.SimilarityPair>());
                    result[i] = (pair.id, pair.score);
                }
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// 주어진 형태소 배열로부터 문맥 ID를 생성합니다.
        /// </summary>
        /// <param name="morphIds">형태소 ID 배열</param>
        /// <returns>문맥 ID</returns>
        public uint ToContextId(uint[] morphIds)
        {
            IntPtr ptr = Marshal.AllocHGlobal(sizeof(uint) * morphIds.Length);
            try
            {
                int[] intArray = new int[morphIds.Length];
                for (int i = 0; i < morphIds.Length; i++)
                {
                    intArray[i] = (int)morphIds[i];
                }
                Marshal.Copy(intArray, 0, ptr, morphIds.Length);
                return KiwiCAPI.kiwi_cong_to_context_id(inst, ptr, morphIds.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// 문맥을 구성하고 있는 형태소 배열을 조회합니다.
        /// </summary>
        /// <param name="contextId">문맥 ID</param>
        /// <param name="maxSize">반환할 최대 개수</param>
        /// <returns>형태소 ID 배열</returns>
        public uint[] FromContextId(uint contextId, int maxSize = 100)
        {
            IntPtr ptr = Marshal.AllocHGlobal(sizeof(uint) * maxSize);
            try
            {
                int count = KiwiCAPI.kiwi_cong_from_context_id(inst, contextId, ptr, maxSize);
                if (count < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.kiwi_error()));

                uint[] result = new uint[count];
                for (int i = 0; i < count; i++)
                {
                    result[i] = (uint)Marshal.ReadInt32(ptr, i * sizeof(uint));
                }
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static string TagToPOS(string tag)
        {
            if (tag == null) return null;
            tag = tag.ToLower();
            string suffix = "";
            if (tag.EndsWith("-i")) suffix = " (불규칙 활용)";
            else if (tag.EndsWith("-r")) suffix = " (규칙 활용)";

            if (tag == "nng") return "일반 명사";
            if (tag == "nnp") return "고유 명사";
            if (tag == "nnb") return "의존 명사";
            if (tag == "nr") return "수사";
            if (tag == "np") return "대명사";
            if (tag == "vv") return "동사" + suffix;
            if (tag == "va") return "형용사" + suffix;
            if (tag == "vx") return "보조 용언" + suffix;
            if (tag == "vcp") return "긍정 지정사";
            if (tag == "vcn") return "부정 지정사";
            if (tag == "mm") return "관형사";
            if (tag == "mag") return "일반 부사";
            if (tag == "maj") return "접속 부사";
            if (tag == "ic") return "감탄사";
            if (tag == "jks") return "주격 조사";
            if (tag == "jkc") return "보격 조사";
            if (tag == "jkg") return "관형격 조사";
            if (tag == "jko") return "목적격 조사";
            if (tag == "jkb") return "부사격 조사";
            if (tag == "jkv") return "호격 조사";
            if (tag == "jkq") return "인용격 조사";
            if (tag == "jx") return "보조사";
            if (tag == "jc") return "접속 조사";
            if (tag == "ep") return "선어말 어미";
            if (tag == "ef") return "종결 어미";
            if (tag == "ec") return "연결 어미";
            if (tag == "etn") return "명사형 전성 어미";
            if (tag == "etm") return "관형형 전성 어미";
            if (tag == "xpn") return "체언 접두사";
            if (tag == "xsn") return "명사 파생 접미사";
            if (tag == "xsv") return "동사 파생 접미사" + suffix;
            if (tag == "xsa") return "형용사 파생 접미사" + suffix;
            if (tag == "xsm") return "부사 파생 접미사";
            if (tag == "xr") return "어근";
            if (tag == "sf") return "종결 부호";
            if (tag == "sp") return "구분 부호";
            if (tag == "ss") return "인용 부호 및 괄호";
            if (tag == "sso") return "SS 중 여는 부호";
            if (tag == "ssc") return "SS 중 닫는 부호";
            if (tag == "se") return "줄임표";
            if (tag == "so") return "붙임표";
            if (tag == "sw") return "기타 특수 문자";
            if (tag == "sl") return "알파벳";
            if (tag == "sh") return "한자";
            if (tag == "sn") return "숫자";
            if (tag == "sb") return "순서 있는 글머리";
            if (tag == "un") return "분석 불능";
            if (tag == "w_url") return "URL 주소";
            if (tag == "w_email") return "이메일 주소";
            if (tag == "w_hashtag") return "해시태그";
            if (tag == "w_mention") return "멘션";
            if (tag == "w_serial") return "일련번호";
            if (tag == "w_emoji") return "이모지";
            if (tag == "z_coda") return "덧붙은 받침";
            if (tag == "z_siot") return "사이시옷";
            return null;
        }

        public static string DialectToString(Dialect dialect)
        {
            List<string> parts = new List<string>();
            if ((dialect & Dialect.Gyeonggi) != 0) parts.Add("경기");
            if ((dialect & Dialect.Chungcheong) != 0) parts.Add("충청");
            if ((dialect & Dialect.Gangwon) != 0) parts.Add("강원");
            if ((dialect & Dialect.Gyeongsang) != 0) parts.Add("경상");
            if ((dialect & Dialect.Jeolla) != 0) parts.Add("전라");
            if ((dialect & Dialect.Jeju) != 0) parts.Add("제주");
            if ((dialect & Dialect.Hwanghae) != 0) parts.Add("황해");
            if ((dialect & Dialect.Hamgyeong) != 0) parts.Add("함경");
            if ((dialect & Dialect.Pyeongan) != 0) parts.Add("평안");
            if ((dialect & Dialect.Archaic) != 0) parts.Add("옛말");

            if (parts.Count == 0) return "표준어";
            return string.Join(",", parts);
        }
    }
    }