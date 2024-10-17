using NetKiwi.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetKiwi.Backend
{
    using KiwiResHandle = IntPtr;
    using KiwiWsHandle = IntPtr;

    public abstract class KiwiCAPIBase
    {
            public static IntPtr LoadDll(string rootPath, string subfolder_name, string dll_name)
        {
            var is64 = IntPtr.Size == 8;
            var suffix = is64 ? "x64" : "x86";
            string subfolderFinal;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                subfolderFinal = Path.Combine(subfolder_name + suffix);
            }
            else
            {
                subfolderFinal = subfolder_name;
            }
            return LoadKiwi(Path.Combine(rootPath, "runtimes", subfolderFinal, "lib", dll_name));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TokenInfo
        {
            public uint chrPosition; /* 시작 위치(UTF16 문자 기준) */
            public uint wordPosition; /* 어절 번호(공백 기준)*/
            public uint sentPosition; /* 문장 번호*/
            public uint lineNumber; /* 줄 번호*/
            public ushort length; /* 길이(UTF16 문자 기준) */
            public byte _tag; /* 품사 태그 */
            public byte senseId; /* 의미 번호 */
            public float score; /* 해당 형태소의 언어모델 점수 */
            public float typoCost; /* 오타가 교정된 경우 오타 비용. 그렇지 않은 경우 0 */
            public uint typoFormId; /* 교정 전 오타의 형태에 대한 정보 (typoCost가 0인 경우 의미 없음) */
            public uint pairedToken; /* SSO, SSC 태그에 속하는 형태소의 경우 쌍을 이루는 반대쪽 형태소의 위치(-1인 경우 해당하는 형태소가 없는 것을 뜻함) */
            public uint subSentPosition; /* 인용부호나 괄호로 둘러싸인 하위 문장의 번호. 1부터 시작. 0인 경우 하위 문장이 아님을 뜻함 */
        }


        private const int RTLD_NOW = 2;


        public static IntPtr CopyMemoryKiwi(IntPtr dest, IntPtr src, uint count)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
                static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
                CopyMemory(dest, src, count);
                return IntPtr.Zero;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                [DllImport("libSystem.dylib", EntryPoint = "memcpy")]
                static extern IntPtr memcpy(IntPtr dest, IntPtr src, uint count);
                return memcpy(dest, src, count);
            }
            else
            {
                [DllImport("libc.so.6", EntryPoint = "memcpy")]
                static extern IntPtr memcpy(IntPtr dest, IntPtr src, uint count);
                return memcpy(dest, src, count);
            }
        }
        public static IntPtr LoadKiwi(string path)
        {
            IntPtr ret = IntPtr.Zero;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                [DllImport("kernel32.dll")]
                static extern IntPtr LoadLibrary(string dllToLoad);
                ret = LoadLibrary(path);
            }
            else
            {
                [DllImport("libdl.so", EntryPoint = "dlopen")]
                static extern IntPtr dlopen(string fileName, int flags);
                ret = dlopen(path, RTLD_NOW);
            }
            if (ret == IntPtr.Zero)
            {
                throw new KiwiException("Failed to load Kiwi library from " + path);
            }
            return ret;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int CReader(int id, IntPtr buf, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int CReceiver(int id, IntPtr kiwi_res, IntPtr userData);

        public abstract Result[] ToResult(KiwiResHandle kiwiresult);
        public abstract ExtractedWord[] ToExtractedWord(KiwiWsHandle kiwiresult);
        public abstract IntPtr Version();

        public abstract IntPtr Analyze_w(IntPtr kiwiHandle, IntPtr text, int topN, int matchOptions, IntPtr blocklist, IntPtr pretokenized);
        public abstract IntPtr Error();
        public abstract int Res_close(IntPtr res);
        public abstract int Analyze_mw(IntPtr handle, CReader reader, CReceiver receiver, IntPtr userData, int topN, int matchOptions);
        public abstract IntPtr Typo_init();
        public abstract IntPtr Typo_get_default(int typoSet);
        public abstract int Typo_add(IntPtr typo, IntPtr orig, int origSize, IntPtr error, int errorSize, float cost, int condition);
        public abstract int Typo_close(IntPtr typo);
        public abstract IntPtr Builder_init(IntPtr modelPath, int maxCache, int options);
        public abstract int Builder_close(IntPtr handle);
        public abstract int Builder_add_word(IntPtr handle, IntPtr word, IntPtr pos, float score);
        public abstract IntPtr Builder_build(IntPtr handle, IntPtr typos, float typo_cost_threshold);
        public abstract int Builder_load_dict(IntPtr handle, IntPtr dictPath);
        public abstract IntPtr Builder_extract_words_w(IntPtr handle, CReader reader, IntPtr userData, int minCnt, int maxWordLen, float minScore, float posThreshold);
        public abstract int Ws_close(IntPtr result);
        public abstract IntPtr Builder_extract_add_words_w(IntPtr handle, CReader reader, IntPtr userData, int minCnt, int maxWordLen, float minScore, float posThreshold);
        public abstract IntPtr Joiner_get_w(IntPtr handle);
        public abstract int Joiner_add(IntPtr handle, IntPtr form, IntPtr tag, int option);
        public abstract int Joiner_close(IntPtr handle);
        public abstract IntPtr New_joiner(IntPtr handle, int lmSearch);
        public abstract int Get_option(IntPtr handle, int option);
        public abstract void Set_option(IntPtr handle, int option, int value);
        public abstract float Get_option_f(IntPtr handle, int option);
        public abstract void Set_option_f(IntPtr handle, int option, float value);
        public abstract int Close(IntPtr handle);

    }
}
