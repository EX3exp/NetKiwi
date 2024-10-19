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

    using KiwiBuilderHandle = IntPtr;
    using KiwiHandle = IntPtr;
    using KiwiJoinerHandle = IntPtr;
    using KiwiResHandle = IntPtr;
    using KiwiTypoHandle = IntPtr;
    using KiwiWsHandle = IntPtr;


    internal class Utf8String : IDisposable
    {
        IntPtr iPtr;
        public IntPtr IntPtr { get { return iPtr; } }
        public int BufferLength { get { return iBufferSize; } }
        int iBufferSize;
        public Utf8String(string aValue)
        {
            if (aValue == null)
            {
                iPtr = IntPtr.Zero;
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(aValue);
                iPtr = Marshal.AllocHGlobal(bytes.Length + 1);
                Marshal.Copy(bytes, 0, iPtr, bytes.Length);
                Marshal.WriteByte(iPtr, bytes.Length, 0);
                iBufferSize = bytes.Length + 1;
            }
        }
        public void Dispose()
        {
            if (iPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(iPtr);
                iPtr = IntPtr.Zero;
            }
        }
    }

    internal class Utf16String : IDisposable
    {
        IntPtr iPtr;
        public IntPtr IntPtr { get { return iPtr; } }
        public int BufferLength { get { return iBufferSize; } }
        int iBufferSize;
        public Utf16String(string aValue)
        {
            if (aValue == null)
            {
                iPtr = IntPtr.Zero;
            }
            else
            {
                byte[] bytes = new UnicodeEncoding().GetBytes(aValue);
                iPtr = Marshal.AllocHGlobal(bytes.Length + 2);
                Marshal.Copy(bytes, 0, iPtr, bytes.Length);
                Marshal.WriteByte(iPtr, bytes.Length, 0);
                Marshal.WriteByte(iPtr, bytes.Length + 1, 0);
                iBufferSize = bytes.Length + 2;
            }
        }
        public void Dispose()
        {
            if (iPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(iPtr);
                iPtr = IntPtr.Zero;
            }
        }
    }

    internal class Utf8StringArray : IDisposable
    {

        IntPtr iPtr, bPtr;
        public IntPtr IntPtr { get { return bPtr; } }

        public Utf8StringArray(string[] aValue)
        {
            if (aValue == null)
            {
                iPtr = IntPtr.Zero;
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
                iPtr = Marshal.AllocHGlobal(totalLength);
                bPtr = Marshal.AllocHGlobal(IntPtr.Size * pool.Length);

                int offset = 0;
                for (int i = 0; i < aValue.Length; i++)
                {
                    Marshal.Copy(pool[i], 0, iPtr + offset, pool[i].Length);
                    Marshal.WriteByte(iPtr, pool[i].Length, 0);
                    Marshal.WriteIntPtr(bPtr, IntPtr.Size * i, iPtr + offset);
                    offset += pool[i].Length + 1;
                }
            }
        }
        public void Dispose()
        {
            if (iPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(iPtr);
                Marshal.FreeHGlobal(bPtr);
                iPtr = IntPtr.Zero;
                bPtr = IntPtr.Zero;
            }
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
        /// <summary>
        /// Target Morpheme<br></br>
        /// 형태소
        /// </summary>
        public string form;
        /// <summary>
        /// Tag<br></br>
        /// 태그
        /// </summary>
        public string tag;
        /// <summary>
        /// Start position(UTF16 characters)<br></br>
        /// 시작 위치(UTF16 문자 기준)
        /// </summary>
        public uint chrPosition;
        /// <summary>
        /// Index of the word (based on spaces)<br></br>
        /// 어절 번호(공백 기준)
        /// </summary>
        public uint wordPosition;
        /// <summary>
        /// Index of the sentence<br></br>
        /// 문장 번호
        /// </summary>
        public uint sentPosition;
        /// <summary>
        /// Index of the line<br></br>
        /// 줄 번호
        /// </summary>
        public uint lineNumber;

        /// <summary>
        /// Length(UTF16 characters)<br></br>
        /// 길이(UTF16 문자 기준)
        /// </summary>
        public ushort length;
        /// <summary>
        /// Id of the sense of the word<br></br>
        /// 의미 번호
        /// </summary>
        public byte senseId;
        /// <summary>
        /// Language model score of the target morpheme<br></br>
        /// 해당 형태소의 언어모델 점수
        /// </summary>
        public float score;
        /// <summary>
        /// Typo fixing cost if the target morpheme's wrong typo has been fixed<br></br>
        /// 오타가 교정된 경우 오타 비용. 그렇지 않은 경우 0
        /// </summary>
        public float typoCost;
        /// <summary>
        /// Wrong typo's information before typo is fixed. It loses its meaning if typoCost was 0.<br></br>
        /// 교정 전 오타의 형태에 대한 정보 (typoCost가 0인 경우 의미 없음)
        /// </summary>
        public uint typoFormId;
        /// <summary>
        /// If the morpheme belongs to SSO, SSC tag, the position of the opposite morpheme. If it is -1, it means there is no corresponding morpheme.<br></br>
        /// SSO, SSC 태그에 속하는 형태소의 경우 쌍을 이루는 반대쪽 형태소의 위치(-1인 경우 해당하는 형태소가 없는 것을 뜻함)
        /// </summary>
        public uint pairedToken;
        /// <summary>
        /// Index of the sub-sentence enclosed by quotation marks or parentheses. Starts from 1. if 0, it means that it was not a sub-sentence.<br></br>
        /// 인용부호나 괄호로 둘러싸인 하위 문장의 번호. 1부터 시작. 0인 경우 하위 문장이 아님을 뜻함
        /// </summary>
        public uint subSentPosition; 
    }

    public enum Option
    {
        IntegrateAllomorph = 1 << 0,
        LoadDefaultDict = 1 << 1,
        LoadTypoDict = 1 << 2,
        LoadMultiDict = 1 << 3,
        MaxUnkFormSize = 0x8002,
        SpaceTolerance = 0x8003,
        CutOffThreshold = 0x9001,
        UnkFormScoreScale = 0x9002,
        UnkFormScoreBias = 0x9003,
        SpacePenalty = 0x9004,
        TypoCostWeight = 0x9005,
    }

    public enum ModelType
    {
        KNLM = 0x0000,
        SBG = 0x0100,
    }

    public enum Match
    {
        Url = 1 << 0,
        Email = 1 << 1,
        Hashtag = 1 << 2,
        Mention = 1 << 3,
        Serial = 1 << 4,
        All = Url | Email | Hashtag | Mention | Serial,

        NormalizeCoda = 1 << 16,

        JoinNounPrefix = 1 << 17,
        JoinNounSuffix = 1 << 18,
        JoinVerbSuffix = 1 << 19,
        JoinAdjSuffix = 1 << 20,
        JoinAdvSuffix = 1 << 21,
        SplitComplex = 1 << 22,
        ZCoda = 1 << 23,
    }

    public class KiwiLoader
    {
        private static IntPtr dllHandle = IntPtr.Zero;
        public static string RootPath;
        public static string GetDefaultPath()
        {
            RootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
            }

            return Path.Combine(RootPath, "netkiwi");
        }
        public static bool LoadDll(string path = null)
        {
            if (dllHandle != IntPtr.Zero) return true;

            if (path == null)
            {
                path = GetDefaultPath();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return (dllHandle = KiwiCAPIBase.LoadDll(path, "win-", "kiwi.dll")) != IntPtr.Zero;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    return (dllHandle = KiwiCAPIBase.LoadDll(path, "macos-arm64", "libkiwi.dylib")) != IntPtr.Zero;
                }
                else
                {
                    return (dllHandle = KiwiCAPIBase.LoadDll(path, "macos-x86-x64", "libkiwi.dylib")) != IntPtr.Zero;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return (dllHandle = KiwiCAPIBase.LoadDll(path, "linux-x86-x64", "libkiwi.so")) != IntPtr.Zero;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

        }
    }

    public enum DefaultTypoSet
    {
        WithoutTypo = 0,
        BasicTypoSet,
        ContinualTypoSet,
        BasicTypoSetWithContinual,
    }

    
    public class TypoTransformer
    {
        private bool readOnly = false;
        public KiwiTypoHandle inst;
        readonly KiwiCAPIBase KiwiCAPI;
        
        public TypoTransformer(KiwiCAPIBase kiwiCAPI)
        {
            KiwiCAPI = kiwiCAPI;
            inst = KiwiCAPI.Typo_init();
        }

        public TypoTransformer(KiwiCAPIBase kiwiCAPI, DefaultTypoSet defaultTypoSet)
        {
            KiwiCAPI = kiwiCAPI;
            readOnly = true;
            inst = KiwiCAPI.Typo_get_default((int)defaultTypoSet);
        }

        public int Add(string[] orig, string[] error, float cost, int condition)
        {
            if (readOnly)
            {
                throw new InvalidOperationException("default typo object cannot be modified!");
            }
            return KiwiCAPI.Typo_add(inst, new Utf8StringArray(orig).IntPtr, orig.Length, new Utf8StringArray(error).IntPtr, error.Length, cost, condition);
        }

        ~TypoTransformer()
        {
            if (inst != null && !readOnly)
            {
                KiwiCAPI.Typo_close(inst);
            }
        }

        public void Dispose()
        {
            if (inst != null && !readOnly)
            {
                KiwiCAPI.Typo_close(inst);
            }
        }
    }

    public class KiwiBuilder
    {
        public delegate string Reader(int id);

        private KiwiBuilderHandle inst;
        private Reader reader;
        private Tuple<int, string> readItem;
        readonly KiwiCAPIBase KiwiCAPI;

        
        private static KiwiCAPIBase.CReader readerInst = (int id, IntPtr buf, IntPtr userData) =>
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

            KiwiCAPIBase.CopyMemoryKiwi(buf, new Utf16String(ki.readItem.Item2).IntPtr, (uint)ki.readItem.Item2.Length * 2);


            return 0;
        };
        public KiwiBuilder(KiwiCAPIBase kiwiCAPI)
        {
            KiwiCAPI = kiwiCAPI;
            if (KiwiLoader.LoadDll()) return;
            if (KiwiLoader.LoadDll(KiwiLoader.GetDefaultPath() + "\\..")) return;
            if (KiwiLoader.LoadDll(KiwiLoader.GetDefaultPath() + "\\..\\..")) return;
        }


        public KiwiBuilder(KiwiCAPIBase kiwiCAPI, string modelPath, int numThreads = 0, Option options = Option.LoadDefaultDict | Option.LoadTypoDict | Option.LoadMultiDict | Option.IntegrateAllomorph, ModelType modelType = ModelType.KNLM)
        {
            KiwiCAPI = kiwiCAPI;
            inst = KiwiCAPI.Builder_init(new Utf8String(modelPath).IntPtr, numThreads, (int)options | (int)modelType);
            if (inst == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
        }
        public int AddWord(string word, string pos, float score)
        {
            int ret = KiwiCAPI.Builder_add_word(inst, new Utf8String(word).IntPtr, new Utf8String(pos).IntPtr, score);
            if (ret < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            return ret;
        }

        public int LoadDictionary(string dictPath)
        {
            int ret = KiwiCAPI.Builder_load_dict(inst, new Utf8String(dictPath).IntPtr);
            if (ret < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            return ret;
        }
        public ExtractedWord[] ExtractWords(Reader reader, int minCnt = 5, int maxWordLen = 10, float minScore = 0.1f, float posThreshold = -3)
        {
            GCHandle handle = GCHandle.Alloc(this);
            this.reader = reader;
            readItem = null;
            KiwiWsHandle ret = KiwiCAPI.Builder_extract_words_w(inst, readerInst, (IntPtr)handle, minCnt, maxWordLen, minScore, posThreshold);
            handle.Free();
            if (inst == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            ExtractedWord[] words = KiwiCAPI.ToExtractedWord(ret);
            KiwiCAPI.Ws_close(ret);
            return words;
        }
        public ExtractedWord[] ExtractAddWords(Reader reader, int minCnt = 5, int maxWordLen = 10, float minScore = 0.1f, float posThreshold = -3)
        {
            GCHandle handle = GCHandle.Alloc(this);
            this.reader = reader;
            readItem = null;
            KiwiWsHandle ret = KiwiCAPI.Builder_extract_add_words_w(inst, readerInst, (IntPtr)handle, minCnt, maxWordLen, minScore, posThreshold);
            handle.Free();
            if (inst == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            ExtractedWord[] words = KiwiCAPI.ToExtractedWord(ret);
            KiwiCAPI.Ws_close(ret);
            return words;
        }

        public Kiwi Build()
        {
            KiwiHandle ret = KiwiCAPI.Builder_build(inst, (IntPtr)null, 0);
            if (ret == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            return new Kiwi(KiwiCAPI, ret);
        }

        public Kiwi Build(TypoTransformer typo, float typoCostThreshold = 2.5f)
        {
            KiwiHandle ret = KiwiCAPI.Builder_build(inst, typo.inst, typoCostThreshold);
            if (ret == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            return new Kiwi(KiwiCAPI, ret);
        }

        ~KiwiBuilder()
        {
            if (inst != IntPtr.Zero)
            {
                if (KiwiCAPI.Builder_close(inst) < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            }
        }

        public void Dispose()
        {
            if (inst != IntPtr.Zero)
            {
                if (KiwiCAPI.Builder_close(inst) < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            }
        }
    }

    public class KiwiJoiner
    {
        private KiwiJoinerHandle inst;
        readonly KiwiCAPIBase KiwiCAPI;
        public KiwiJoiner(KiwiCAPIBase kiwiCAPI, KiwiJoinerHandle _inst)
        {
            KiwiCAPI = kiwiCAPI;
            inst = _inst;
        }

        public string Get()
        {
            return Marshal.PtrToStringUni(KiwiCAPI.Joiner_get_w(inst));
        }

        public void Add(string form, string tag, int option = 1)
        {
            if (KiwiCAPI.Joiner_add(inst, new Utf8String(form).IntPtr, new Utf8String(tag).IntPtr, option) < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
        }

        ~KiwiJoiner()
        {
            if (inst != IntPtr.Zero)
            {
                if (KiwiCAPI.Joiner_close(inst) < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            }
        }

        public void Dispose()
        {
            if (inst != IntPtr.Zero)
            {
                if (KiwiCAPI.Joiner_close(inst) < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            }
        }
    }
    public class Kiwi
    {
        public delegate string Reader(int id);
        public delegate int Receiver(int id, Result[] res);

        private KiwiHandle inst;
        private Reader reader;
        private Receiver receiver;
        private Tuple<int, string> readItem;

        readonly KiwiCAPIBase KiwiCAPI;

        public Kiwi(KiwiCAPIBase kiwiCAPI)
        {
            KiwiCAPI = kiwiCAPI;
            readerInst = (int id, IntPtr buf, IntPtr userData) =>
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
                KiwiCAPIBase.CopyMemoryKiwi(buf, new Utf16String(ki.readItem.Item2).IntPtr, (uint)ki.readItem.Item2.Length * 2);

                return 0;
            };

            receiverInst = (int id, KiwiResHandle kiwi_res, IntPtr userData) =>
            {
                GCHandle handle = (GCHandle)userData;
                Kiwi ki = handle.Target as Kiwi;
                return ki.receiver(id, KiwiCAPI.ToResult(kiwi_res));
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                KiwiCAPI = new KiwiCAPIWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    KiwiCAPI = new KiwiCAPIOSXArm();
                }
                else
                {
                    KiwiCAPI = new KiwiCAPIOSXIntel();
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                KiwiCAPI = new KiwiCAPILinux();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            if (KiwiLoader.LoadDll()) return;
            if (KiwiLoader.LoadDll(KiwiLoader.GetDefaultPath() + "\\..")) return;
            if (KiwiLoader.LoadDll(KiwiLoader.GetDefaultPath() + "\\..\\..")) return;
        }

        private KiwiCAPIBase.CReader readerInst;

        private KiwiCAPIBase.CReceiver receiverInst;
        public Kiwi(KiwiCAPIBase kiwiCAPI, KiwiHandle _inst)
        {
            KiwiCAPI = kiwiCAPI;
            inst = _inst;
        }

        public string Version()
        {
            return Marshal.PtrToStringAnsi(KiwiCAPI.Version());
        }

        public Result[] Analyze(string text, int topN = 1, Match matchOptions = Match.All)
        {
            KiwiResHandle res = KiwiCAPI.Analyze_w(inst, new Utf16String(text).IntPtr, topN, (int)matchOptions, IntPtr.Zero, IntPtr.Zero);
            if (res == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            Result[] ret = KiwiCAPI.ToResult(res);
            KiwiCAPI.Res_close(res);
            return ret;
        }

        public void AnalyzeMulti(Reader reader, Receiver receiver, int topN = 1, Match matchOptions = Match.All)
        {
            GCHandle handle = GCHandle.Alloc(this);
            this.reader = reader;
            this.receiver = receiver;
            readItem = null;
            int ret = KiwiCAPI.Analyze_mw(inst, readerInst, receiverInst, (IntPtr)handle, topN, (int)matchOptions);
            handle.Free();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (ret < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            }
            else
            {
                if (ret < 0) throw new KiwiException(Marshal.PtrToStringUTF8(KiwiCAPI.Error()));
            }
        }

        public KiwiJoiner NewJoiner(bool lmSearch = true)
        {
            var h = KiwiCAPI.New_joiner(inst, lmSearch ? 1 : 0);
            if (h == IntPtr.Zero) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            return new KiwiJoiner(KiwiCAPI, h);
        }

        public bool IntegrateAllomorph
        {
            get { return KiwiCAPI.Get_option(inst, (int)Option.IntegrateAllomorph) != 0; }
            set { KiwiCAPI.Set_option(inst, (int)Option.IntegrateAllomorph, value ? 1 : 0); }
        }

        public int MaxUnkFormSize
        {
            get { return KiwiCAPI.Get_option(inst, (int)Option.MaxUnkFormSize); }
            set { KiwiCAPI.Set_option(inst, (int)Option.MaxUnkFormSize, value); }
        }

        public int SpaceTolerance
        {
            get { return KiwiCAPI.Get_option(inst, (int)Option.SpaceTolerance); }
            set { KiwiCAPI.Set_option(inst, (int)Option.SpaceTolerance, value); }
        }

        public float CutOffThreshold
        {
            get { return KiwiCAPI.Get_option_f(inst, (int)Option.CutOffThreshold); }
            set { KiwiCAPI.Set_option_f(inst, (int)Option.CutOffThreshold, value); }
        }

        public float UnkFormScoreScale
        {
            get { return KiwiCAPI.Get_option_f(inst, (int)Option.UnkFormScoreScale); }
            set { KiwiCAPI.Set_option_f(inst, (int)Option.UnkFormScoreScale, value); }
        }

        public float UnkFormScoreBias
        {
            get { return KiwiCAPI.Get_option_f(inst, (int)Option.UnkFormScoreBias); }
            set { KiwiCAPI.Set_option_f(inst, (int)Option.UnkFormScoreBias, value); }
        }

        public float SpacePenalty
        {
            get { return KiwiCAPI.Get_option_f(inst, (int)Option.SpacePenalty); }
            set { KiwiCAPI.Set_option_f(inst, (int)Option.SpacePenalty, value); }
        }

        public float TypoCostWeight
        {
            get { return KiwiCAPI.Get_option_f(inst, (int)Option.TypoCostWeight); }
            set { KiwiCAPI.Set_option_f(inst, (int)Option.TypoCostWeight, value); }
        }

        ~Kiwi()
        {
            if (inst != IntPtr.Zero)
            {
                if (KiwiCAPI.Close(inst) < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            }
        }

        public void Dispose()
        {
            if (inst != IntPtr.Zero)
            {
                if (KiwiCAPI.Close(inst) < 0) throw new KiwiException(Marshal.PtrToStringAnsi(KiwiCAPI.Error()));
            }
        }
    }
}