### 미리보이스 - 성우 만들기 (VITS2)
[<img src="..\..\Misc\title.png" height="57"/>](../../readme/README-ko.md)

📜🧐 :
[English](make-voicer-VITS2.md) | [한국어](make-voicer-VITS2-ko.md)
#### [KO]

## 📚 1. 개괄
- `VITS2` 모델을 훈련해 봅시다.
- [VITS2 모델에 관해 더 알고 싶다면 (VITS2 논문)](https://arxiv.org/abs/2307.16430)

## 📚 2. 준비물
### ❇️ 1. 녹음본 - `recorded.zip`
- 먼저 **말뭉치**를 읽어 녹음한 녹음본이 필요해요. 
> 🤔말뭉치가 뭐에요?
> - 말뭉치는 **여러 문장들을 모은 것**을 의미해요.
> - 말뭉치가 쓰이는 분야는 매우 많습니다. 우리는 이 글에서 '말뭉치'를 **TTS 모델 훈련에 사용되는 데이터셋**으로 지칭할 거에요.


> 🤔어떤 말뭉치를 사용해야 하죠? 녹음은 어떻게 하나요?
> - [Recstar](https://github.com/sdercolin/recstar)를 사용해도 좋고, 다른 녹음 프로그램을 사용해도 좋아요.
> - 말뭉치는 **[MiriVoiceSupport-CorpusManager](https://github.com/EX3exp/MiriVoiceSupport-CorpusManager/blob/main/readme/README-ko.md)에서 다운받을 수 있어요.     
>   녹음을 끝낸 후, 하단의 지시사항들을 계속 따라가세요!**
> - *원한다면, 스스로 `dataset.zip`을 만들어 사용해도 됩니다.*

<br>

❇️ 녹음이 끝났다면, 많은 wav 파일들이 보일 거에요.<br>
각 wav 파일들을 **한 폴더** 안에 모아 주세요. 

예시:
```
📂 recorded
├─ 💿MV-KOR-AA-NORMAL_0-001.wav
├─ 💿MV-KOR-AA-NORMAL_0-002.wav
├─ 💿MV-KOR-AA-NORMAL_0-012.wav
├─ 💿MV-KOR-AA-BRIGHT_1-001.wav
├─ 💿MV-KOR-AA-BRIGHT_1-022.wav
└─ ... 
```
그리고 `.zip`으로 압축해 주세요.
```
🗂️ recorded.zip
├─ 💿MV-KOR-AA-NORMAL_0-001.wav
├─ 💿MV-KOR-AA-NORMAL_0-002.wav
├─ 💿MV-KOR-AA-NORMAL_0-012.wav
├─ 💿MV-KOR-AA-BRIGHT_1-001.wav
├─ 💿MV-KOR-AA-BRIGHT_1-022.wav
└─ ... 
```

❇️ 멋진 `recorded.zip` 을 얻었어요. **이제 이걸 가지고 `dataset.zip` 을 만들어야 합니다**.
- 🔽 아래를 누르면, 이 노트북 친구가 `dataset.zip`을 만들어 줄 거에요.

    [<img src="https://colab.research.google.com/assets/colab-badge.svg">](https://colab.research.google.com/github/EX3exp/MiriVoiceSupport-CorpusManager/blob/main/MiriVoice_Corpus_Dataset_Generator.ipynb)


### ❇️ 2. 데이터셋  - `dataset.zip`
- 아마 아래의 구조를 띤 `dataset.zip`이 구글 드라이브에 생성되어 있을 거에요.
```
🗂️ dataset.zip
├─ 📂train
│  ├─ 📜filelist_train.txt.cleaned
│  ├─ 💿MV-KOR-AA-NORMAL_0-001.wav
│  ├─ 💿MV-KOR-AA-NORMAL_0-002.wav
│  ├─ 💿MV-KOR-AA-BRIGHT_1-001.wav
│  └─ ... 
└─ 📂validation
   ├─ 📜filelist_val.txt.cleaned
   ├─ 💿MV-KOR-AA-NORMAL_0-012.wav
   ├─ 💿MV-KOR-AA-BRIGHT_1-022.wav
   └─ ...
```
### ❇️ 3. VITS2 훈련 노트북
- 🔽 아래를 눌러 훈련 노트북을 실행하세요.

    [<img src="https://colab.research.google.com/assets/colab-badge.svg">](https://colab.research.google.com/github/EX3exp/MiriVoiceSupport-VITS2/blob/main/VITS2_MiriVoice_Support.ipynb)


### ❇️ 4. VITS2 Voicer Export Notebook
- 훈련이 끝났다면, `G_*.pth`, `D_*.pth` 파일이 있을 거에요. 가장 마음에 드는 체크포인트를 골라 줍니다.
- 우린 `미리보이스`에서 `.pth` 파일을 사용할 수 없어서, `.onnx`로 변환해 주어야 해요.
- 🔽 아래를 눌러 노트북을 실행하세요. 모델을 `미리보이스`에서 구동되는 성우로 내보낼 거에요.

    [<img src="https://colab.research.google.com/assets/colab-badge.svg">]("https://colab.research.google.com/github/EX3exp/MiriVoiceSupport-VITS2/blob/main/MiriVoicer_VITS2_Exporter.ipynb)

- 아래와 같은 `<성우 이름>.zip`이 생겼을 거에요. :
    ```
    🗂️ <성우 이름>.zip
    ├─ ⬜voicer.onnx
    ├─ 📜voicer.yaml
    ├─ 📜config.yaml
    └─ 📜readme.txt
    ```    
- `voicer.yaml`을 편집해 아이콘이나 전신 일러스트 등을 바꾸거나, 추가할 수 있어요. [이곳](../../readme/voicer-yaml-ko.md)을 읽어 보세요.

### ❇️ 5. 미리보이스에 성우 설치하기
1. `도구` -> `성우 설치`를 클릭하세요.
2. `<성우 이름>.zip`을 열고 기다립니다. `<성우 이름>.zip`이 `Voicer` 폴더 내에 압축 해제될 거에요.
3. 완료! 이제 성우를 사용할 수 있어요.