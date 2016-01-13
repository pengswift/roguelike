using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	// 关卡启动延时， 利用这段时间设置画幕关闭时间
	public float levelStartDelay = 2f;
	// 回合切换延时
	public float turnDelay = 0.1f;
	public int playerFoodPoints = 100;

	// 静态 GameManager对象
	public static GameManager instance = null;
	[HideInInspector] public bool playersTurn = true;

	// 管理组件统一由gameManager 统一支配
	private BoardManager boardScript;
	private int level = 1;
	private List<Enemy> enemies;
	private bool enemiesMoving;

	private Text levelText;
	private GameObject levelImage;

	// 设置开启标示
	private bool doingSetup = true;

	void Awake() {
		// 单例的一种方式
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
		// 加载新场景时，使其寄生的宿主不被删除
		DontDestroyOnLoad (gameObject);

		enemies = new List<Enemy>();

		// 获取BoardManager 支配权
		boardScript = GetComponent<BoardManager> ();

		// 初始化游戏
		InitGame ();
	}

	void OnLevelWasLoaded(int index) {
		// 当关卡被加载时调用
		level++;
		InitGame ();
	}

	void InitGame() {
		// 启动设置
		doingSetup = true;

		levelImage = GameObject.Find ("LevelImage");
		levelText = GameObject.Find ("LevelText").GetComponent<Text>();
		levelText.text = "Day " + level;

		levelImage.SetActive (true);

		Invoke ("HideLevelImage", levelStartDelay);

		enemies.Clear ();
		boardScript.SetupScene (level);
	}

	void HideLevelImage() {
		levelImage.SetActive (false);

		// 当画幕隐藏时，设置结束
		doingSetup = false;
	}


	void Update() {
		if (playersTurn || enemiesMoving || doingSetup) {
			return;
		}

		// 敌人ai 交给gameManager 脱管
		StartCoroutine (MoveEnemies ());
	}

	public void AddEnemyToList(Enemy script) {
		enemies.Add (script);
	}

	// 游戏结束
	public void GameOver() {
		levelText.text = "After " + level + " days, you starved.";
		levelImage.SetActive (true);

		enabled = false;
	}

	IEnumerator MoveEnemies() {
		// 敌人移动开启
		enemiesMoving = true;

		// 思考ing
		yield return new WaitForSeconds (turnDelay);

		// 如果没有敌人，思考ing 
		if (enemies.Count == 0) {
			yield return new WaitForSeconds (turnDelay);
		}


		for (int i = 0; i < enemies.Count; i++) {
			// ai系统较弱， 每次移动玩一个敌人要思考ing
			enemies [i].MoveEnemy ();
			yield return new WaitForSeconds (enemies[i].moveTime);
		}

		// 当敌人操作完时， 交换控制权
		playersTurn = true;

		// 地方移动结束
		enemiesMoving = false;
	}
		
}
