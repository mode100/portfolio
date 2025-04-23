# ポートフォリオ

<!-- vscode-markdown-toc -->
* 1. [ネームバトルロワイヤル](#)
	* 1.1. [概要](#-1)
	* 1.2. [開発の狙い](#-1)
	* 1.3. [使用した技術・ツール](#-1)
	* 1.4. [開発における工夫](#-1)
	* 1.5. [開発期間](#-1)
* 2. [掲示板サイト](#-1)
	* 2.1. [概要](#-1)
	* 2.2. [開発の狙い](#-1)
	* 2.3. [使用した技術・ツール](#-1)
	* 2.4. [開発における工夫](#-1)
	* 2.5. [開発期間](#-1)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->


##  1. <a name=''></a>ネームバトルロワイヤル
###  1.1. <a name='-1'></a>概要
名前から一意に生成されるキャラクターを使って戦うゲーム。

<https://nazenavi.com/games/namebattler/index.html>

###  1.2. <a name='-1'></a>開発の狙い
- 次のゲーム作りに活かす為、短期間で完成させることを心掛けた

###  1.3. <a name='-1'></a>使用した技術・ツール
- typescript
- Node.js
    - phaser -> ゲーム制作用のライブラリ
    - js-cookie -> cookieに関するライブラリ
    - mersennetwister -> メルセンヌ・ツイスタ―に関するライブラリ

###  1.4. <a name='-1'></a>開発における工夫
- ゲーム内UIは、Canvasの上にHTML要素を重ねることで実現
- ステートパターンを使用
- Cursorを使用 (※1週間程で無料分が終了し、使用不可になった)

###  1.5. <a name='-1'></a>開発期間
- 3週間
    - 1週間 -> 基本ロジックの作成
    - 2週間 -> 実際にプレイして、アイデアが思いついたら随時機能を追加していった

##  2. <a name='-1'></a>掲示板サイト
###  2.1. <a name='-1'></a>概要
簡単な機能しか無い、最小の掲示板。

<https://bulletin-board-game.vercel.app>

[ソースコード](https://github.com/mode100/vercel-site1)

###  2.2. <a name='-1'></a>開発の狙い
- Reactの基本的な書き方を学ぶ為
- 無料で実験的なサイトを作成したい

###  2.3. <a name='-1'></a>使用した技術・ツール
- typescript
- Node.js
    - React
- Supabase -> 基本無料のデータベースを利用できるサービス
- Vercel -> 基本無料のウェブホスティングサービス

###  2.4. <a name='-1'></a>開発における工夫
- ChatGPTに大枠を作成してもらい、その後に調整する
- 作業しやすい環境にする為、IDEをVSCodeからVisualStudioに変更した

###  2.5. <a name='-1'></a>開発期間
- 3日

