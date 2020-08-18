# GameObjectExporter
特定のGameObjectを含めたUnitypackageを作成します。
デフォルトでEditor拡張とVRCSDK(VRChatで使われるSDK)を省きます。

バージョン: v1.1

## 使い方
1. InspectorにあるGameObjectを右クリックする
2. Export GameObjectを選択する
3. Defaultまたはignore～を選択する
  * Default: VRCSDKとEditor拡張を含めない
  * ignore Shader: Default + Shaderを含めない
  * ignore DynamicBone: Default + DynamicBoneを含めない
  * ignore Shader and DynamicBone: 上記すべてを含めない
  * README: ツールの説明が表示される
4. DesktopにGameObject名.unitypackageで書き出されています。

unitypackageインポート時にGameObjectはPrefabとしてAssetsフォルダの直下に配置されるようになっています。

## 更新履歴
* v1.1 
	* プログレスバーが出るように(PR: sansuke05)
	* DynamicBone関連ファイルの一部が正しく取得できていなかった不具合を修正
	* Shader関連ファイルの取得方法を改善
* v1.0 ツールを作成