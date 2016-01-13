using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour {
	[Serializable]
	public class Count {
		public int minimum;
		public int maximum;

		public Count(int min, int max) {
			minimum = min;
			maximum = max;
		}
	}

	public int columns = 8; // 列数
	public int rows = 8; // 行数
	public Count wallCount = new Count(5, 9);  // 内墙数量
	public Count foodCount = new Count(1, 5); // 食物数量
	public GameObject exit; // 退出组件
	public GameObject[] floorTiles; // 食物组件列表
	public GameObject[] wallTiles; // 内墙组件列表
	public GameObject[] foodTiles; // 食物列表
	public GameObject[] enemyTiles; // 敌人组件列表
	public GameObject[] outerWallTiles; // 外墙组件列表

	private Transform boardHolder; // Board父对象组件 
	private List<Vector3> gridPositions = new List<Vector3>(); // 用于放置食物、敌人、内墙的位置集合

	// 初始化gridPositions 位置信息
	void InitialiseList() {
		gridPositions.Clear ();
		// 收集中间6*6的位置坐标， 排除最外层墙 和 靠近外墙的一层
		for (int x = 1; x < columns-1; x++) {
			for (int y = 1; y < rows - 1; y++) {
				gridPositions.Add (new Vector3(x, y, 0f));
			}
		}
	}

	// 地板制作
	void BoardSetup() {
		boardHolder = new GameObject ("Board").transform;

		for (int x = -1; x < columns + 1; x++) {
			for (int y = -1; y < rows + 1; y++) {
				// 随机地板对象
				GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
				// 将最外面一层变更为外墙对象
				if (x == -1 || x == columns || y == -1 || y == rows) {
					toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
				}
				// 实例化prefab
				GameObject instance = Instantiate (toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
				// 添加到父节点
				instance.transform.SetParent (boardHolder);
			}
		}
	}

	// 随机位置方法，用于后面生成随机食物、敌人和内墙
	Vector3 RandomPosition() {
		int randomIndex = Random.Range (0, gridPositions.Count);
		Vector3 randomPosition = gridPositions [randomIndex];
		// 一旦该位置被随机出来，将被从list中移除
		gridPositions.RemoveAt (randomIndex);
		return randomPosition;
	}

	// 在随机位置显示组件 (传入瓦片列表， 随机区间)
	void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum) {
		int objectCount = Random.Range (minimum, maximum+1);

		for (int i = 0; i < objectCount; i++) {
			// 随机一个位置
			Vector3 randomPosition = RandomPosition ();
			// 随机一个瓦片对象
			GameObject tileChoice = tileArray [Random.Range (0, tileArray.Length)];
			// 实例化prefab
			Instantiate (tileChoice, randomPosition, Quaternion.identity);
		}
	}

	// 公共方法，设置场景 （传入关卡等级）
	public void SetupScene(int level) {
		// 创建地板
		BoardSetup ();
	    // 初始化grid位置信息
		InitialiseList ();
		// 随机生成内墙 和 食物
		LayoutObjectAtRandom (wallTiles, wallCount.minimum, wallCount.maximum);
		LayoutObjectAtRandom (foodTiles, foodCount.minimum, foodCount.maximum);

		// 以2为底, level的对数 取正 生成敌人的数量
		int enemyCount = (int)Mathf.Log (level, 2f);
		// 随机生成敌人
		LayoutObjectAtRandom (enemyTiles, enemyCount, enemyCount);

		// 放置退出组件
		Instantiate (exit, new Vector3(columns-1, rows-1, 0f), Quaternion.identity);
	}

}
