# 🥝 NetKiwi
- 🥝[Kiwi (지능형 한국어 형태소 분석기)](https://github.com/bab2min/Kiwi?tab=readme-ov-file)를 C#에서 사용할 수 있도록 만든 패키지에요.
- 🥝 Supported OS: `Windows x86`, `Windows x64`, `MacOS arm64`, `MacOS x86_x64`, `Linux x86_x64`
- 🥝Current Kiwi Version: `0.23.2`

## 🥝 Install
    dotnet add package NetKiwi
## 🥝 Licence
- Kiwi: LGPL 3.0
## 🥝 Basic Usage

```cs
using NetKiwi.Backend;
public void ExampleAnalyze()
{
    Kiwi kiwi = new KiwiBuilder().Build(); // Kiwi 객체를 생성합니다

    Result[] result = kiwi.Analyze("모든 국민은 인간으로서의 존엄과 가치를 가지며, 행복을 추구할 권리를 가진다. 국가는 개인이 가지는 불가침의 기본적 인권을 확인하고 이를 보장할 의무를 진다."); // 결과를 받아 옵니다
    foreach (Result res in result) // foreach로 순회합니다
    {
        foreach (Token t in res.morphs)
        {
            Console.WriteLine($"분석된 형태소: {t.form}");
            Console.WriteLine($"분석된 형태소의 태그: {t.tag}");
        }
    }
}
```

```cs
using NetKiwi.Backend;
public void ExampleAnalyzeWithUsing()
{

    using (Kiwi kiwi = new KiwiBuilder().Build()) // 메모리 관리를 위해 using 블록을 사용하는 것을 추천합니다.
    {
        Result[] result = kiwi.Analyze("모든 국민은 인간으로서의 존엄과 가치를 가지며, 행복을 추구할 권리를 가진다. 국가는 개인이 가지는 불가침의 기본적 인권을 확인하고 이를 보장할 의무를 진다."); // 결과를 받아 옵니다
        foreach (Result res in result) // foreach로 순회합니다
        {
            foreach (Token t in res.morphs)
            {
                Console.WriteLine($"분석된 형태소: {t.form}");
                Console.WriteLine($"분석된 형태소의 태그: {t.tag}");
            }
        }
    } // using 블록이 끝나면 Kiwi 객체가 자동으로 Dispose 됩니다
}
```

```cs
using NetKiwi.Backend;

public void ExampleAnalyzeMulti() // 여러 문장을 동시에 분석하는 예제입니다.
{
    using (Kiwi kiwi = new KiwiBuilder().Build())
    {
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
            Console.WriteLine($"{res.Length}"); // res의 길이는 TopN과 같습니다(이 예제에서는 TopN = 1).
                                                // AnalyzeMulti는 각 문장에 대해 topN개의 결과를 반환합니다. 같은 문장에 대해 여러 개의 결과를 얻고 싶다면 topN을 늘리면 됩니다.
            Console.WriteLine($"문장 {i}의 분석 결과:");
            Console.WriteLine($"분석된 형태소 수: {res[0].morphs.Length}");
            Console.WriteLine($"분석된 형태소: {string.Join(", ", res[0].morphs.Select(m => m.form))}");
            return 0;
        });
    }
}
```

## 🥝 Reference
- [bab2min/kiwi-gui](https://github.com/bab2min/kiwi-gui)
