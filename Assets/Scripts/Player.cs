using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 玩家对象，继承MoveingObject
public class Player : MovingObject {
	public int wallDamage = 1; // 内墙攻击伤害
	public int pointsPerFood = 10; // 食物增加的生命点数，  
	public int pointsPerSoda = 20; 
	// 苏打增加的生命点数，  这些数值 其它可以存放到Food 和 Soda类本身，因为此类游戏简单，故放在此处

	public float restartLevelDelay = 1f;
	public Text foodText;

	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;

	public float gestureSlideDistance = 30f;


	private Animator animator; // 动画
	private int food; // 食物数量
	private Vector2 touchOrigin = -Vector2.one;

	protected override void Start() {
		animator = GetComponent<Animator> ();
		food = GameManager.instance.playerFoodPoints; // 从GameManager实例中获取当前玩家的食物数量
		// 因为随着关卡的递增，玩家的食物不会重置，所有要有一个全局的地方记录之

		foodText.text = "Food: " + food;

		base.Start ();
	}

	// 更新循环
	void Update() {
		// 如果不是玩家回合，直接退出
		if (!GameManager.instance.playersTurn)
			return;

		int horizontal = 0;
		int vertical = 0;

		#if UNITY_STANDALONE || UNITY_WEBPLAYER

		// 获取水平 和 垂直 方向上坐标轴的值
		horizontal = (int)Input.GetAxisRaw ("Horizontal");
		vertical = (int)Input.GetAxisRaw ("Vertical");

		// 如果水平方向有值，垂直方向上的值重置为0
		if (horizontal != 0) {
			vertical = 0;
		}

		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

		if (Input.touchCount > 0) {
			Touch myTouch = Input.touches[0];

			if (myTouch.phase == TouchPhase.Began) {
				touchOrigin = myTouch.position;

			} else if (myTouch.phase == TouchPhase.Ended) {
				Vector2 touchEnd = myTouch.position;

				float x = touchEnd.x - touchOrigin.x;
				float y = touchEnd.y - touchOrigin.y;

				touchOrigin.x = -1;

				// 解决在ios landspace设置上 touchOrigin.x 值时负数
				// 设置滑动距离的判断，解决误操作问题.. 
				// unity在 ios 设备landspace中坐标 简直神奇了。左上角不是0，0点， 而且四个角没一个是0，0点。

				if (Mathf.Abs(x) > gestureSlideDistance || Mathf.Abs(y) > gestureSlideDistance) {
					if (Mathf.Abs(x) > Mathf.Abs(y)) {
						horizontal = x > 0 ? 1 : -1;
					} else {
						vertical = y > 0 ? 1 : -1;
					}
				}
			}
		}

		#endif

		// 当水平 或者 垂直的坐标轴有值时， 尝试移动
		if (horizontal != 0 ||  vertical != 0) {
			// T 是 Wall, 不可移动物体 
			// 当不可移动的碰撞物体 多时，此方法不在通用。
			// 则考虑通过tag 获取该对象
			AttemptMove<Wall> (horizontal, vertical);
		}

	}
		
	// 当不可用时，记录下玩家的当前食物数量
	private void OnDisable() {
		GameManager.instance.playerFoodPoints = food;
	}

	// 子类重载之
	protected override bool AttemptMove<T> (int xDir, int yDir) {
		// 每次尝试，食物减少
		food--;
		foodText.text = "Food: " + food;
		// 尝试移动，父类会自动调用子类的
		bool canMove = base.AttemptMove<T> (xDir, yDir);


		// 修正官网Move 判断例子， 解决move twice两次的歧义 原来方法没有问题，但不好理解
		// debug 模式下 -1 的平方是 1， 但定义的变量 却等于0, 只在第二次出现此情况，恰恰因为此情况 while 只进入了一次
		// Roguelike 作者也 在论坛上说过，该教程可能不是最好的教程
		if (canMove) {
			SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
		}

		CheckIfGameOver ();
		// 玩家回合结束
		GameManager.instance.playersTurn = false;

		return canMove;
	}

	protected override void OnCantMove<T> (T component) {
		Wall hitWall = component as Wall;
		hitWall.DamageWall (wallDamage);

		// 播放攻击动画
		animator.SetTrigger ("playerChop");
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "Exit") {
			// 重启游戏
			Invoke ("Restart", restartLevelDelay);
			enabled = false;
		} else if (other.tag == "Food") {

			food += pointsPerFood;
			foodText.text = "+" + pointsPerFood + " Foods: " + food;
			SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);
			other.gameObject.SetActive (false);
		} else if (other.tag == "Soda") {
			food += pointsPerSoda;
			foodText.text = "+" + pointsPerSoda + " Foods: " + food;
			SoundManager.instance.RandomizeSfx (drinkSound1, drinkSound2);
			other.gameObject.SetActive (false);
		}
	}

	private void Restart() {
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);	
	}

	public void LoseFood(int loss) {
		animator.SetTrigger ("playerHit");

		food -= loss;
		foodText.text = "-" + loss + " Foods: " + food;
		CheckIfGameOver ();
	}

	private void CheckIfGameOver() {
		if (food <= 0) {
			SoundManager.instance.PlaySingle (gameOverSound);
			SoundManager.instance.musicSource.Stop ();
			GameManager.instance.GameOver ();
		}
	}

}
