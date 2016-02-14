CM3D2.AlwaysColorChangeEx.Plugin
---

パーツごとに色変更やパラメータ調整を可能とするプラグインです。  
まだまだ不具合が残っている可能性があります。

オリジナル作者である[kf-cm3d2](https://github.com/kf-cm3d2) さんとは別人による改造版です。  
[オリジナル](https://github.com/kf-cm3d2/CM3D2.AlwaysColorChange.Plugin)から改造しすぎたため、拡張版として別プロジェクト化しました。  

---
## ■ 導入
#### ◇前提条件  
UnityInjectorが導入済みであること

#### ◇動作確認環境
  **1.25**

#### ◇インストール  

[Releases](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/releases)ページから、対応するバージョンのzipファイルをダウンロードし、  
解凍後、UnityInjectorフォルダ以下をプラグインフォルダにコピーしてください。

#### ◇自分でコンパイルする場合  
compile.batをサンプルで用意しています。  
レジストリが正しく設定されていればそのままコンパイルできます。  
もし、レジストリの状態と異なるCM3D2ディレクトリを使いたい場合は、  
cmpile.batファイルの以下の環境変数のパスを設定して、バッチを実行してください。
* CM3D2_DIR=C:\KISS\CM3D2  

## ■ 機能概要
#### ◇対象シーン
 * メイドエディット
 * 男エディット
 * 夜伽
 * ダンス
 * ADVパート
 * イベント回想

で動作します。

★ 撮影モードや他プラグインで操作対象のメイドを非表示にした場合、  
　強制的に **メイド選択画面** になります。

##### ◇機能説明（概要）
* **[メイド選択](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/wiki/%E7%94%BB%E9%9D%A2%E8%AA%AC%E6%98%8E#%E3%83%A1%E3%82%A4%E3%83%89%E9%81%B8%E6%8A%9E)**  
  操作対象のメイドを選択します。  
  撮影モードや他プラグインで複数メイドを表示した場合に利用してください。
* **[マスク選択](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/wiki/%E7%94%BB%E9%9D%A2%E8%AA%AC%E6%98%8E#%E3%83%9E%E3%82%B9%E3%82%AF%E3%82%A2%E3%82%A4%E3%83%86%E3%83%A0%E9%81%B8%E6%8A%9E)**  
  maskItemで消去されているアイテムを表示させることができます。  
   マスクアイテム選択画面で、各スロットのマスクの有無を指定してください。
  「適用」ボタンで反映します。
* **[表示ノード選択](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/wiki/%E7%94%BB%E9%9D%A2%E8%AA%AC%E6%98%8E#%E8%A1%A8%E7%A4%BA%E3%83%8E%E3%83%BC%E3%83%89%E9%81%B8%E6%8A%9E)**   
  ノード（身体の部位）の表示・非表示を切り替えられます。  
  「適用」ボタンで反映します。
* **[プリセット保存](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/wiki/%E7%94%BB%E9%9D%A2%E8%AA%AC%E6%98%8E#%E3%83%97%E3%83%AA%E3%82%BB%E3%83%83%E3%83%88%E4%BF%9D%E5%AD%98)**  
  現在の衣装・設定値をプリセットとして保存します。  
  プリセット適用で呼び出すことがます。  
* **[プリセット適用](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/wiki/%E7%94%BB%E9%9D%A2%E8%AA%AC%E6%98%8E#%E3%83%97%E3%83%AA%E3%82%BB%E3%83%83%E3%83%88%E9%81%A9%E7%94%A8)**  
  保存したプリセットを選択・適用します。
  適用時に、プリセットから適用する項目を選択できます。
  - マスク
  - ノード表示
  - 身体
  - 衣装
  - 衣装外し  
    これがチェックされている場合、プリセットで衣装を設定していないスロットは衣装を外します  
* **[menuエクスポート](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/wiki/%E7%94%BB%E9%9D%A2%E8%AA%AC%E6%98%8E#menu%E3%82%A8%E3%82%AF%E3%82%B9%E3%83%9D%E3%83%BC%E3%83%88)**  
  現在の選択スロットのアイテムをエクスポートします。  
  関連するめくれやずらしmodelなどにも対応
* **[テクスチャ変更](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/wiki/%E7%94%BB%E9%9D%A2%E8%AA%AC%E6%98%8E#%E3%83%86%E3%82%AF%E3%82%B9%E3%83%81%E3%83%A3%E5%A4%89%E6%9B%B4)**  
  テクスチャファイル選択による変更と、スライダーによる色変更が可能です。  
  - ファイル選択によるテクスチャ変更  
    CM3D2のarcに同梱されているtexファイル、あるいはMODに登録されているtexファイルは、    
    テキストフィールドにファイル名を指定する事で適用できます(.tex省略可能)  
    それ以外のファイルは、「...」ボタンから参照してください。  
    「...」ボタンからファイル参照画面が表示されるので、対象ファイルを選択して、「選択」ボタンを押下することでテクスチャが適用されます。  

    ※　texファイルはMODフォルダ以下など、CM3D2から読み込めるパスに存在する必要あり。  
    pngはダイアログで選択できればパスは任意  
    *対応形式* :.tex, .png
  - スライダーによる色変更  
    freeカラー選択時は変更できません。  
   「保存」ボタンでテクスチャ出力に対応(0.0.5.1～)

## ■ 使用方法
##### ◇メニュー操作
F12キーでメニューが開きます。  
キーを変更したい場合は、
* Config/AlwaysColroChangeEx.ini

のToggleWindowキーで指定してください。  
iniファイルの詳細は[wiki](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/wiki/ini%E3%83%95%E3%82%A1%E3%82%A4%E3%83%AB)を参照

![GUI](http://i.imgur.com/sMm5lo9.jpg  "GUI")

### ■ [画面説明](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/wiki/%E7%94%BB%E9%9D%A2%E8%AA%AC%E6%98%8E)

### ■ [既知の問題](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/wiki/Known-Issue)

### ■ [TODO](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/wiki/TODO)

## ■ 権利

* オリジナルの作者である[kf-cm3d2](https://github.com/kf-cm3d2)さんの
意向である以下に従ってください。  
(オリジナルの[プロジェクト](https://github.com/kf-cm3d2/CM3D2.AlwaysColorChange.Plugin)READMEに記載)
~~~
ソースの改変・再配布等は自由にしていただいてかまいません。  
むしろやりたいこと・それを超える機能を実装してくれる人歓迎。  
~~~

CustomJsonWriter.csのみ、[MITライセンス](http://www.jsonfx.net/license/)となります  
（書式変更のため、JsonFXのクラス継承で一部ロジックを使用）


## ■ 更新履歴
#### 0.2.1.0 (最新版)
 * **不具合修正 (Fixed)**
  * テクスチャの色変更機能の反映タイミングを修正
    - スライダー移動時、KeyUpタイミングのみで反映するよう修正  
  * 手持アイテムや拘束具・バイブなどのカラーチェンジができなくなっていた問題を修正  
 * **その他 (Misc)**
  * 脱衣などで非表示になっているスロットを編集可能に変更  
    - 未読み込みのスロットは除外

#### 以前の更新履歴
 * [Releases](https://github.com/trzr/CM3D2.AlwaysColorChangeEx.Plugin/Releases)を参照
 * プロジェクトリネーム( **0.1.0.0** )以前の履歴は、[旧プロジェクト](https://github.com/trzr/CM3D2.AlwaysColorChange.Plugin#-%E6%9B%B4%E6%96%B0%E5%B1%A5%E6%AD%B4)を参照
