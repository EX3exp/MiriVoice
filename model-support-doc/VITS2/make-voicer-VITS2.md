### MiriVoice - Make Voicer (VITS2)
[<img src="..\..\Misc\title.png" height="57"/>](../../README.md)

📜🧐 :
[English](make-voicer-VITS2.md) | [한국어](make-voicer-VITS2-ko.md)
#### [EN]

## 📚 1. Abstract
- We will train `VITS2` model in this page.
- [More About VITS2 Model (VITS2 Paper)](https://arxiv.org/abs/2307.16430)

## 📚 2. Things to Prepare
### ❇️ 1. Recorded Samples - `recorded.zip`
- First, you have to record samples, by reading **Corpus** line by line.
> 🤔What is Corpus?
> - Corpus means **a collection of texts**. (And its plural form is 'Corpora'.)
> - The Fields that Corpora are used is enormous, but in our case, We will treat the word 'Corpus' as **Dataset for TTS Model's train**.


> 🤔What Corpus should I use? How Can I record them?
> - You can use [Recstar](https://github.com/sdercolin/recstar), or other Recording Software.
> - **You can just download one from [MiriVoiceSupport-CorpusManager](https://github.com/EX3exp/MiriVoiceSupport-CorpusManager/blob/main/README.md).
>   After finishing your recording, please follow instructions below!**
> - *If you want, you can make your own `dataset.zip` yourself.*

<br>

❇️ After Recording, We will have bunch of wav files.<br>
Please collect each wav files into **One Folder**, like below:
```
📂 recorded
├─ 💿MV-KOR-AA-NORMAL_0-001.wav
├─ 💿MV-KOR-AA-NORMAL_0-002.wav
├─ 💿MV-KOR-AA-NORMAL_0-012.wav
├─ 💿MV-KOR-AA-BRIGHT_1-001.wav
├─ 💿MV-KOR-AA-BRIGHT_1-022.wav
└─ ... 
```
Then compress into `.zip`.
```
🗂️ recorded.zip
├─ 💿MV-KOR-AA-NORMAL_0-001.wav
├─ 💿MV-KOR-AA-NORMAL_0-002.wav
├─ 💿MV-KOR-AA-NORMAL_0-012.wav
├─ 💿MV-KOR-AA-BRIGHT_1-001.wav
├─ 💿MV-KOR-AA-BRIGHT_1-022.wav
└─ ... 
```

❇️ We're having nice `recorded.zip` now, **all we have to do is making `dataset.zip` with it.**
- 🔽 Please click below, this notebook will do every `dataset.zip` stuffs for you!

    [<img src="https://colab.research.google.com/assets/colab-badge.svg">](https://colab.research.google.com/github/EX3exp/MiriVoiceSupport-CorpusManager/blob/main/MiriVoice_Corpus_Dataset_Generator.ipynb)


### ❇️ 2. Dataset - `dataset.zip`
- You will having `dataset.zip` with below structure, it might be already generated in proper google drive's path.
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
### ❇️ 3. VITS2 Training Notebook
- 🔽 Click below and run the Colab Notebook.

    [<img src="https://colab.research.google.com/assets/colab-badge.svg">](https://colab.research.google.com/github/EX3exp/MiriVoiceSupport-VITS2/blob/main/VITS2_MiriVoice_Support.ipynb)


### ❇️ 4. VITS2 Voicer Export Notebook
- After training, we will having `G_*.pth`, `D_*.pth`. Please choose one best checkpoint.
- We cannot use `.pth` file in `MiriVoice`. We need to convert it into `.onnx` format.
- 🔽 Click below to export your model into `MiriVoice`'s Voicer format.

    [<img src="https://colab.research.google.com/assets/colab-badge.svg">](https://colab.research.google.com/github/EX3exp/MiriVoiceSupport-VITS2/blob/main/MiriVoicer_VITS2_Exporter.ipynb)

- You will now have `<voicer name>.zip` now, like :
    ```
    🗂️ <voicer name>.zip
    ├─ ⬜voicer.onnx
    ├─ 📜voicer.yaml
    ├─ 📜config.yaml
    └─ 📜readme.txt
    ```    
- You can edit `voicer.yaml` to add icon or protrait, [please see here.](../../voicer-yaml.md)

### ❇️ 5. Install Voicer in MiriVoice
1. Click `Tools` -> `Install Voicer`.
2. Open your `<voicer name>.zip` and wait. Your `<voicer name>.zip` will be extracted in `Voicer` Folder.
3. Done! You can use your voicer now.