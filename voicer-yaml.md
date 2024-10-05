### MiriVoice - voicer.yaml
[<img src="Misc\title.png" height="57"/>](README.md)

📜🧐 :
[English](voicer-yaml.md) | [한국어](readme/voicer-yaml-ko.md)
#### [EN]

## 📚 1. Abstract
- We will learn about `voicer.yaml` in this page.
- `voicer.yaml` carries voicer's informations, icon images, etc.

## 📚 2. What is `voicer.yaml`?
<img src="Misc\voicer-yaml-1.png" />
<img src="Misc\voicer-yaml-2.png" />

| item        | description                                                                                          |
| ----------- | ---------------------------------------------------------------------------------------------------- |
| name        | What is this Voicer's displayed name?                                                                |
| nickname    | Usually 1-5 letters, which used in `Lines Input`.                                                         |
| description(Voicer) | What is this Voicer's description?                                                                   |
| voice       | Who provided voice for this Voicer?                                                                  |
| author      | Who manages and owns this Voicer?                                                                    |
| engineer    | Who trained this Voicer's model?                                                                     |
| illustrator | Who drew this Voicer's icon or portrait?                                                            |
| web         | Link to Voicer's official webpage.                                                                   |
| icon        | Filepath to Voicer's icon image. You might use `jpg, png, or bmp`. <br>Displayed size is `100px * 100px`.     |
| portrait    | Filepath to Voicer's portrait image. You might use `jpg, png, or bmp`. <br>Displayed size is `400px * 800px`. |
| type        | What type of model does this Voicer use?                                                             |
| configPath  | Path to config file, `config.yaml` in default.                                                     |
| langCode    | What Language does this Voicer use? (You can input Language Code, e.g: `ko`, `en-US`). <br>If there are no Existing `Phonemizer` which supports that langCode's Language, It will be ignored.                   |
| version     | Voicer's version.                                                                                    |
| readme      | Path to readme file, `readme.txt` in default.                                                      |
| voicerMetas | Carries informations about Voicer's styles. <br>The first came styles becomes Voicer's default style.             |
| └─style               | Style's displayed name, it used in `Lines Input`.                                                                             |
| └─speakerId           | SpeakerId used in Model Inference.                                                                                              |
| └─phrase              | Voicer's sample phrase. Each phrase will be synthesized with corresponding style, when pressed `speaker icon` in `Voicers Window`. |
| └─description (Style) | What is this Style's description?                                                                                               |