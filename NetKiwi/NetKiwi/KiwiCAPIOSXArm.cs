using NetKiwi.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetKiwi.Backend
{
    using CString = IntPtr;
    using KiwiHandle = IntPtr;
    using KiwiBuilderHandle = IntPtr;
    using KiwiResHandle = IntPtr;
    using KiwiWsHandle = IntPtr;
    using KiwiTypoHandle = IntPtr;
    using KiwiJoinerHandle = IntPtr;
    using KiwiMorphsetHandle = IntPtr;
    using KiwiPretokenizedHandle = IntPtr;

    public class KiwiCAPIOSXArm: KiwiCAPIBase
    {

        private const string dll_name = "runtimes/macos-arm64/lib/libkiwi.dylib";

        // global functions
        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_version();

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_error();

        // builder functions
        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiBuilderHandle kiwi_builder_init(CString modelPath, int maxCache, int options);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_builder_close(KiwiBuilderHandle handle);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_builder_add_word(KiwiBuilderHandle handle, CString word, CString pos, float score);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_builder_load_dict(KiwiBuilderHandle handle, CString dictPath);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiWsHandle kiwi_builder_extract_words_w(KiwiBuilderHandle handle, CReader reader, IntPtr userData, int minCnt, int maxWordLen, float minScore, float posThreshold);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiWsHandle kiwi_builder_extract_add_words_w(KiwiBuilderHandle handle, CReader reader, IntPtr userData, int minCnt, int maxWordLen, float minScore, float posThreshold);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiHandle kiwi_builder_build(KiwiBuilderHandle handle, KiwiTypoHandle typos, float typo_cost_threshold);

        // analyzer functions
        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_get_option(KiwiHandle handle, int option);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern void kiwi_set_option(KiwiHandle handle, int option, int value);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_get_option_f(KiwiHandle handle, int option);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern void kiwi_set_option_f(KiwiHandle handle, int option, float value);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiResHandle kiwi_analyze_w(KiwiHandle handle, IntPtr text, int topN, int matchOptions, KiwiMorphsetHandle blocklist, KiwiPretokenizedHandle pretokenized);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiResHandle kiwi_analyze(KiwiHandle handle, IntPtr text, int topN, int matchOptions, KiwiMorphsetHandle blocklist, KiwiPretokenizedHandle pretokenized);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_analyze_mw(KiwiHandle handle, CReader reader, CReceiver receiver, IntPtr userData, int topN, int matchOptions);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_analyze_m(KiwiHandle handle, CReader reader, CReceiver receiver, IntPtr userData, int topN, int matchOptions);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_close(KiwiHandle handle);

        // result management functions
        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_size(KiwiResHandle result);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_res_prob(KiwiResHandle result, int index);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_word_num(KiwiResHandle result, int index);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr kiwi_res_form_w(KiwiResHandle result, int index, int num);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr kiwi_res_tag_w(KiwiResHandle result, int index, int num);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr kiwi_res_form(KiwiResHandle result, int index, int num);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr kiwi_res_tag(KiwiResHandle result, int index, int num);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_position(KiwiResHandle result, int index, int num);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_length(KiwiResHandle result, int index, int num);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_word_position(KiwiResHandle result, int index, int num);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_sent_position(KiwiResHandle result, int index, int num);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern ref TokenInfo kiwi_res_token_info(KiwiResHandle result, int index, int num);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_res_close(KiwiResHandle result);

        // word management functions
        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_ws_size(KiwiWsHandle result);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr kiwi_ws_form_w(KiwiWsHandle result, int index);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_ws_score(KiwiWsHandle result, int index);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_ws_freq(KiwiWsHandle result, int index);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern float kiwi_ws_pos_score(KiwiWsHandle result, int index);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_ws_close(KiwiWsHandle result);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiTypoHandle kiwi_typo_init();

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiTypoHandle kiwi_typo_get_basic();

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiTypoHandle kiwi_typo_get_default(int typoSet);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_typo_add(KiwiTypoHandle typo, IntPtr orig, int origSize, IntPtr error, int errorSize, float cost, int condition);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_typo_close(KiwiTypoHandle typo);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern KiwiJoinerHandle kiwi_new_joiner(KiwiHandle handle, int lmSearch);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_joiner_add(KiwiJoinerHandle handle, CString form, CString tag, int option);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_joiner_get(KiwiJoinerHandle handle);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern CString kiwi_joiner_get_w(KiwiJoinerHandle handle);

        [DllImport(dll_name, CallingConvention = CallingConvention.Cdecl)]
        public static extern int kiwi_joiner_close(KiwiJoinerHandle handle);
        public override Result[] ToResult(KiwiResHandle kiwiresult)
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

                    var ti = kiwi_res_token_info(kiwiresult, i, j);
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
                }
                ret[i].prob = kiwi_res_prob(kiwiresult, i);
            }
            return ret;
        }

        public override ExtractedWord[] ToExtractedWord(KiwiWsHandle kiwiresult)
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

        public override IntPtr Version()
        {
            return kiwi_version();
        }

        public override IntPtr Analyze_w(IntPtr kiwiHandle, IntPtr text, int topN, int matchOptions, IntPtr blocklist, IntPtr pretokenized)
        {
            return kiwi_analyze_w(kiwiHandle, text, topN, matchOptions, blocklist, pretokenized);
        }

        public override IntPtr Error()
        {
            return kiwi_error();
        }

        public override int Res_close(IntPtr res)
        {
            return kiwi_res_close(res);
        }

        public override int Analyze_mw(IntPtr handle, CReader reader, CReceiver receiver, IntPtr userData, int topN, int matchOptions)
        {
            return kiwi_analyze_mw(handle, reader, receiver, userData, topN, matchOptions);
        }

        public override IntPtr Typo_init()
        {
            return kiwi_typo_init();
        }

        public override IntPtr Typo_get_default(int typoSet)
        {
            return kiwi_typo_get_default(typoSet);
        }

        public override int Typo_add(IntPtr typo, IntPtr orig, int origSize, IntPtr error, int errorSize, float cost, int condition)
        {
            return kiwi_typo_add(typo, orig, origSize, error, errorSize, cost, condition);
        }

        public override int Typo_close(IntPtr typo)
        {
            return kiwi_typo_close(typo);
        }

        public override IntPtr Builder_init(IntPtr modelPath, int maxCache, int options)
        {
            return kiwi_builder_init(modelPath, maxCache, options);
        }

        public override int Builder_close(IntPtr handle)
        {
            return kiwi_builder_close(handle);
        }

        public override int Builder_add_word(IntPtr handle, IntPtr word, IntPtr pos, float score)
        {
            return kiwi_builder_add_word(handle, word, pos, score);
        }

        public override IntPtr Builder_build(IntPtr handle, IntPtr typos, float typo_cost_threshold)
        {
            return kiwi_builder_build(handle, typos, typo_cost_threshold);
        }

        public override int Builder_load_dict(IntPtr handle, IntPtr dictPath)
        {
            return kiwi_builder_load_dict(handle, dictPath);
        }

        public override IntPtr Builder_extract_words_w(IntPtr handle, CReader reader, IntPtr userData, int minCnt, int maxWordLen, float minScore, float posThreshold)
        {
            return kiwi_builder_extract_words_w(handle, reader, userData, minCnt, maxWordLen, minScore, posThreshold);
        }

        public override int Ws_close(IntPtr result)
        {
            return kiwi_ws_close(result);
        }

        public override IntPtr Builder_extract_add_words_w(IntPtr handle, CReader reader, IntPtr userData, int minCnt, int maxWordLen, float minScore, float posThreshold)
        {
            return kiwi_builder_extract_add_words_w(handle, reader, userData, minCnt, maxWordLen, minScore, posThreshold);
        }

        public override IntPtr Joiner_get_w(IntPtr handle)
        {
            return kiwi_joiner_get_w(handle);
        }

        public override int Joiner_add(IntPtr handle, IntPtr form, IntPtr tag, int option)
        {
            return kiwi_joiner_add(handle, form, tag, option);
        }

        public override int Joiner_close(IntPtr handle)
        {
            return kiwi_joiner_close(handle);
        }

        public override IntPtr New_joiner(IntPtr handle, int lmSearch)
        {
            return kiwi_new_joiner(handle, lmSearch);
        }

        public override int Get_option(IntPtr handle, int option)
        {
            return kiwi_get_option(handle, option);
        }

        public override void Set_option(IntPtr handle, int option, int value)
        {
            kiwi_set_option(handle, option, value);
        }

        public override float Get_option_f(IntPtr handle, int option)
        {
            return kiwi_get_option_f(handle, option);
        }

        public override void Set_option_f(IntPtr handle, int option, float value)
        {
            kiwi_set_option_f(handle, option, value);
        }

        public override int Close(IntPtr handle)
        {
            return kiwi_close(handle);
        }
    }
}
