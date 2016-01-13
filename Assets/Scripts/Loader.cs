using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

	// 将gameManager 及其 子管理组件制作成prefab. 通过Loader 脚本加载
	public GameObject gameManager;
	public GameObject soundManager;

	void Awake() {
		if (GameManager.instance == null) {
			Instantiate (gameManager);
		}

		if (SoundManager.instance == null) {
			Instantiate (soundManager);
		}

	}
}
