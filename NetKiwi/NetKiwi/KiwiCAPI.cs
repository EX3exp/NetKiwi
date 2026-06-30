using System.Runtime.InteropServices;

namespace NetKiwi.Backend
{
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

    public static class KiwiCAPI
    {
        private const string LibName = "kiwi";
        private static bool _isInitialized = false;

        static KiwiCAPI()
        {
            InitiallizeLibraryResolver();
        }

        private static void InitiallizeLibraryResolver()
        {
            if (_isInitialized) return;
            NativeLibrary.SetDllImportResolver(typeof(KiwiCAPI).Assembly, (libraryName, assembly, searchPath) =>
            {
                if (libraryName == LibName)
                {
                    string basePath = AppContext.BaseDirectory;
                    string libPath = "";

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        libPath = Path.Combine(basePath, "netkiwi", "runtimes", "win-x64", "native", "kiwi.dll");
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        libPath = Path.Combine(basePath, "netkiwi", "runtimes", "linux-x64", "native", "libkiwi.so");
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        libPath = Path.Combine(basePath, "netkiwi", "runtimes", "osx-x64", "native", "libkiwi.dylib");
                    }

                    if (File.Exists(libPath))
                    {
                        return NativeLibrary.Load(libPath);
                    }
                }
                return IntPtr.Zero;
            });
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
            public ushort dialect; /* 방언 정보 */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MorphemeInfo
        {
            public byte tag; /* 품사 태그 */
            public byte senseId; /* 의미 번호 */
            public float userScore; /* 사용자 정의 점수 */
            public uint lmMorphemeId; /* 언어모델 형태소 ID */
            public uint origMorphemeId; /* 원래 형태소 ID */
            public ushort dialect; /* 방언 정보 */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SimilarityPair
        {
            public uint id;
            public float score;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AnalyzeOption
        {
            public int matchOptions; /* KIWI_MATCH_* 열거형 참고 */
            public KiwiMorphsetHandle blocklist; /* 분석 후보 탐색 과정에서 blocklist에 포함된 형태소들은 배제됩니다 */
            public int openEnding; /* 마지막 형태소 다음 문장을 종결하지 않고 열린 상태로 끝낼지를 설정합니다 */
            public int allowedDialects; /* KIWI_DIALECT_* 열거형 참고 */
            public float dialectCost; /* 방언 형태소에 추가되는 비용. 기본값은 3 */
            public KiwiPreparedTypoHandle typoTransformer; /* 분석 시 사용할 오타 교정기. null인 경우 사용하지 않습니다 */
            public float typoThreshold; /* 오타 교정 비용 임계값. 기본값은 2.5 */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Config
        {
            public byte integrateAllomorph; /* 이형태 형태소의 통합 여부 */
            public float cutOffThreshold; /* 분석 과정에서 이 값보다 더 크게 차이가 나는 후보들은 제거합니다 */
            public float oovRuleScale; /* 미등재 형태 추출 시 사용하는 기울기 값 */
            public float oovRuleBias; /* 미등재 형태 추출 시 사용하는 편향 값 */
            public float oovChrBias; /* 미등재 형태 추출 시 사용하는 문자 기반 점수의 편향 값 */
            public float oovGlobalWeight; /* 미등재 형태 추출 시 사용하는 전역 빈도 가중치 */
            public float oovLocalWeight; /* 미등재 형태 추출 시 사용하는 국부 빈도 가중치 */
            public float oovGlobalMinFreq; /* 미등재 형태 추출 시 사용하는 전역 최소 빈도 */
            public float spacePenalty; /* 공백 패널티 */
            public float typoCostWeight; /* 오타 비용의 가중치 */
            public uint maxUnkFormSize; /* 미등재 형태의 최대 크기 */
            public uint maxUnkFormSizeFollowedByJClass; /* (조사가 뒤따르는 경우) 미등재 형태의 최대 크기 */
            public uint spaceTolerance; /* 공백 허용치 */
        }


        private const int RTLD_NOW = 2;


        public static void CopyMemory(IntPtr dest, IntPtr src, uint count)
        {
            // .NET 5 upper
            #if NET5_0_OR_GREATER
            unsafe
            {
                NativeMemory.Copy(src.ToPointer(), dest.ToPointer(), count);
            }
            #else // .NET Standard 2.0 / .NET Framework
            unsafe            
            {                
                Buffer.MemoryCopy(src.ToPointer(), dest.ToPointer(), count, count);            
            }
            #endif
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int CReader(int id, IntPtr buf, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int CReceiver(int id, IntPtr kiwi_res, IntPtr userData);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UIntPtr StreamReadFunc(IntPtr userData, IntPtr buffer, UIntPtr length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long StreamSeekFunc(IntPtr userData, long offset, int whence);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StreamCloseFunc(IntPtr userData);

        [StructLayout(LayoutKind.Sequential)]
        public struct StreamObject
        {
            public StreamReadFunc read;
            public StreamSeekFunc seek;
            public StreamCloseFunc close;
            public IntPtr userData;
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate StreamObject StreamObjectFactory(CString filename);
        // global functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_version();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_error();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void kiwi_clear_error();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_get_script_name(byte script);

        // builder functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiBuilderHandle kiwi_builder_init(CString modelPath, int numThreads, int options, int enabledDialects);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiBuilderHandle kiwi_builder_init_stream(StreamObjectFactory streamObjectFactory, int numThreads, int options, int enabledDialects);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_builder_close(KiwiBuilderHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_builder_add_word(KiwiBuilderHandle handle, CString word, CString pos, float score);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_builder_add_alias_word(KiwiBuilderHandle handle, CString alias, CString pos, float score, CString origWord);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_builder_add_word_with_def(KiwiBuilderHandle handle, CString word, CString pos, int senseId, int dialect, float score);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_builder_add_alias_word_with_def(KiwiBuilderHandle handle, CString alias, CString pos, int senseId, int dialect, float score, CString origWord);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_builder_add_pre_analyzed_word(KiwiBuilderHandle handle, CString form, int size, IntPtr analyzedMorphs, IntPtr analyzedPos, float score, IntPtr positions);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_builder_add_rule(KiwiBuilderHandle handle, CString pos, IntPtr replacer, IntPtr userData, float score);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_builder_load_dict(KiwiBuilderHandle handle, CString dictPath);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiWsHandle kiwi_builder_extract_words_w(KiwiBuilderHandle handle, CReader reader, IntPtr userData, int minCnt, int maxWordLen, float minScore, float posThreshold);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiWsHandle kiwi_builder_extract_add_words_w(KiwiBuilderHandle handle, CReader reader, IntPtr userData, int minCnt, int maxWordLen, float minScore, float posThreshold);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiHandle kiwi_builder_build(KiwiBuilderHandle handle, KiwiTypoHandle typos, float typo_cost_threshold);

        // analyzer initialization functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiHandle kiwi_init(CString modelPath, int numThreads, int options, int enabledDialects);

        // analyzer functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void kiwi_set_global_config(KiwiHandle handle, Config config);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Config kiwi_get_global_config(KiwiHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_get_option(KiwiHandle handle, int option);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void kiwi_set_option(KiwiHandle handle, int option, int value);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_get_option_f(KiwiHandle handle, int option);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void kiwi_set_option_f(KiwiHandle handle, int option, float value);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiResHandle kiwi_analyze_w(KiwiHandle handle, IntPtr text, int topN, AnalyzeOption option, KiwiPretokenizedHandle pretokenized);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiResHandle kiwi_analyze(KiwiHandle handle, IntPtr text, int topN, AnalyzeOption option, KiwiPretokenizedHandle pretokenized);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_analyze_mw(KiwiHandle handle, CReader reader, CReceiver receiver, IntPtr userData, int topN, AnalyzeOption option);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_analyze_m(KiwiHandle handle, CReader reader, CReceiver receiver, IntPtr userData, int topN, AnalyzeOption option);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiSsHandle kiwi_split_into_sents_w(KiwiHandle handle, IntPtr text, int matchOptions, ref KiwiResHandle tokenizedRes);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiSsHandle kiwi_split_into_sents(KiwiHandle handle, IntPtr text, int matchOptions, ref KiwiResHandle tokenizedRes);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiMorphsetHandle kiwi_new_morphset(KiwiHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_close(KiwiHandle handle);

        // result management functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_size(KiwiResHandle result);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_res_prob(KiwiResHandle result, int index);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_word_num(KiwiResHandle result, int index);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr kiwi_res_form_w(KiwiResHandle result, int index, int num);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr kiwi_res_tag_w(KiwiResHandle result, int index, int num);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr kiwi_res_form(KiwiResHandle result, int index, int num);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr kiwi_res_tag(KiwiResHandle result, int index, int num);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_position(KiwiResHandle result, int index, int num);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_length(KiwiResHandle result, int index, int num);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_word_position(KiwiResHandle result, int index, int num);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_sent_position(KiwiResHandle result, int index, int num);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_res_score(KiwiResHandle result, int index, int num);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_res_typo_cost(KiwiResHandle result, int index, int num);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr kiwi_res_token_info(KiwiResHandle result, int index, int num);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_morpheme_id(KiwiResHandle result, int index, int num, KiwiHandle kiwiHandle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_close(KiwiResHandle result);

        // word management functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_ws_size(KiwiWsHandle result);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr kiwi_ws_form_w(KiwiWsHandle result, int index);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_ws_score(KiwiWsHandle result, int index);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_ws_freq(KiwiWsHandle result, int index);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_ws_pos_score(KiwiWsHandle result, int index);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_ws_close(KiwiWsHandle result);

        // sentence splitting functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_ss_size(KiwiSsHandle result);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_ss_begin_position(KiwiSsHandle result, int index);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_ss_end_position(KiwiSsHandle result, int index);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_ss_close(KiwiSsHandle result);

        // morphset functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_morphset_add(KiwiMorphsetHandle handle, CString form, CString tag);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_morphset_add_w(KiwiMorphsetHandle handle, IntPtr form, CString tag);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_morphset_close(KiwiMorphsetHandle handle);

        // pretokenized functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiPretokenizedHandle kiwi_pt_init();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_pt_add_span(KiwiPretokenizedHandle handle, int begin, int end);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_pt_add_token_to_span(KiwiPretokenizedHandle handle, int spanId, CString form, CString tag, int begin, int end);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_pt_add_token_to_span_w(KiwiPretokenizedHandle handle, int spanId, IntPtr form, CString tag, int begin, int end);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_pt_close(KiwiPretokenizedHandle handle);

        // swtokenizer functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiSwtokenizerHandle kiwi_swt_init(CString path, KiwiHandle kiwi);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_swt_encode(KiwiSwtokenizerHandle handle, CString text, int textSize, IntPtr tokenIds, int tokenIdsBufSize, IntPtr offsets, int offsetBufSize);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_swt_decode(KiwiSwtokenizerHandle handle, IntPtr tokenIds, int tokenSize, IntPtr text, int textBufSize);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_swt_close(KiwiSwtokenizerHandle handle);

        // typo transformer functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiTypoHandle kiwi_typo_init();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiTypoHandle kiwi_typo_get_basic();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiTypoHandle kiwi_typo_get_default(int typoSet);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_typo_add(KiwiTypoHandle typo, IntPtr orig, int origSize, IntPtr error, int errorSize, float cost, int condition);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiTypoHandle kiwi_typo_copy(KiwiTypoHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_typo_update(KiwiTypoHandle handle, KiwiTypoHandle src);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_typo_scale_cost(KiwiTypoHandle handle, float scale);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_typo_set_continual_typo_cost(KiwiTypoHandle handle, float threshold);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_typo_set_lengthening_typo_cost(KiwiTypoHandle handle, float threshold);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_typo_close(KiwiTypoHandle typo);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiPreparedTypoHandle kiwi_typo_prepare(KiwiTypoHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_prepared_typo_close(KiwiPreparedTypoHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiJoinerHandle kiwi_new_joiner(KiwiHandle handle, int lmSearch);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_joiner_add(KiwiJoinerHandle handle, CString form, CString tag, int option);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_joiner_get(KiwiJoinerHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_joiner_get_w(KiwiJoinerHandle handle);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_joiner_close(KiwiJoinerHandle handle);

        // morpheme finding functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_find_morphemes(KiwiHandle handle, CString form, CString tag, int senseId, IntPtr morphIds, int maxCount);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_find_morphemes_with_prefix(KiwiHandle handle, CString formPrefix, CString tag, int senseId, IntPtr morphIds, int maxCount);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_tag_to_string(KiwiHandle handle, byte pos_tag);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern MorphemeInfo kiwi_get_morpheme_info(KiwiHandle handle, uint morphId);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_get_morpheme_form_w(KiwiHandle handle, uint morphId);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_get_morpheme_form(KiwiHandle handle, uint morphId);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_free_morpheme_form(CString form);

        // context and similarity functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_cong_most_similar_words(KiwiHandle handle, uint morphId, IntPtr output, int topN);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_cong_similarity(KiwiHandle handle, uint morphId1, uint morphId2);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_cong_most_similar_contexts(KiwiHandle handle, uint contextId, IntPtr output, int topN);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_cong_context_similarity(KiwiHandle handle, uint contextId1, uint contextId2);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_cong_predict_words_from_context(KiwiHandle handle, uint contextId, IntPtr output, int topN);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_cong_predict_words_from_context_diff(KiwiHandle handle, uint contextId, uint bgContextId, float weight, IntPtr output, int topN);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint kiwi_cong_to_context_id(KiwiHandle handle, IntPtr morphIds, int size);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_cong_from_context_id(KiwiHandle handle, uint contextId, IntPtr morphIds, int maxSize);

        public static Result[] ToResult(KiwiResHandle kiwiresult)
        {
            int resCount = kiwi_res_size(kiwiresult);
            if (resCount < 0) throw new KiwiException(Marshal.PtrToStringAnsi(kiwi_error()));
            Result[] ret = new Result[resCount];
            for (int i = 0; i < resCount; ++i)
            {
                int num = kiwi_res_word_num(kiwiresult, i);
                ret[i].morphs = new Token[num];
                for (int j = 0; j < num; ++j)
                {
                    ret[i].morphs[j].form = Marshal.PtrToStringUni(kiwi_res_form_w(kiwiresult, i, j));
                    ret[i].morphs[j].tag = Marshal.PtrToStringUni(kiwi_res_tag_w(kiwiresult, i, j));

                    IntPtr tiPtr = kiwi_res_token_info(kiwiresult, i, j);
                    if (tiPtr != IntPtr.Zero)
                    {
                        TokenInfo ti = Marshal.PtrToStructure<TokenInfo>(tiPtr);
                        ret[i].morphs[j].chrPosition = ti.chrPosition;
                        ret[i].morphs[j].wordPosition = ti.wordPosition;
                        ret[i].morphs[j].sentPosition = ti.sentPosition;
                        ret[i].morphs[j].lineNumber = ti.lineNumber;
                        ret[i].morphs[j].length = ti.length;
                        ret[i].morphs[j].senseId = ti.senseId;
                        ret[i].morphs[j].score = ti.score;
                        ret[i].morphs[j].typoCost = ti.typoCost;
                        ret[i].morphs[j].typoFormId = ti.typoFormId;
                        ret[i].morphs[j].pairedToken = ti.pairedToken;
                        ret[i].morphs[j].subSentPosition = ti.subSentPosition;
                        ret[i].morphs[j].dialect = (Dialect)ti.dialect;
                    }
                }
                ret[i].prob = kiwi_res_prob(kiwiresult, i);
            }
            return ret;
        }

        public static ExtractedWord[] ToExtractedWord(KiwiWsHandle kiwiresult)
        {
            int resCount = kiwi_ws_size(kiwiresult);
            if (resCount < 0) throw new KiwiException(Marshal.PtrToStringAnsi(kiwi_error()));
            ExtractedWord[] ret = new ExtractedWord[resCount];
            for (int i = 0; i < resCount; ++i)
            {
                ret[i].word = Marshal.PtrToStringUni(kiwi_ws_form_w(kiwiresult, i));
                ret[i].score = kiwi_ws_score(kiwiresult, i);
                ret[i].posScore = kiwi_ws_pos_score(kiwiresult, i);
                ret[i].freq = kiwi_ws_freq(kiwiresult, i);
            }
            return ret;
        }
    }
}
