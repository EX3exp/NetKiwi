# 🥝 NetKiwi
- 🥝[Kiwi (지능형 한국어 형태소 분석기)](https://github.com/bab2min/Kiwi?tab=readme-ov-file)를 C#에서 사용할 수 있도록 만든 패키지에요.
- 🥝 Supported OS: `Windows x86`, `Windows x64`, `MacOS arm64`, `MacOS x86_x64`, `Linux x86_x64`
- 🥝Current Kiwi Version: `0.19.0`

## 🥝 Install
    dotnet add package NetKiwi
## 🥝 Licence
- Kiwi: LGPL 3.0
## 🥝 Basic Usage

```cs
using System;
using NetKiwi; // SharpKiwi는 이 네임스페이스 소속입니다
using NetKiwi.Backend; // Result와 Token은 이 네임스페이스 소속입니다
using (SharpKiwi kiwi = new SharpKiwi()) // 리소스를 효율적으로 사용하기 위해, using과 함께 사용하는 것을 추천합니다
{
    Result[] result = kiwi.Analyze("여기에 분석할 문장을 입력합니다"); // 결과를 받아 옵니다
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
using NetKiwi; // SharpKiwi는 이 네임스페이스 소속입니다
using NetKiwi.Backend; // Result는 이 네임스페이스 소속입니다
SharpKiwi kiwi = new SharpKiwi(); // 이런 식으로 using 없이 사용해도 됩니다
Result[] result = kiwi.Analyze("여기에 분석할 문장을 입력합니다");
```
## 🥝 Reference
- [bab2min/kiwi-gui](https://github.com/bab2min/kiwi-gui)
