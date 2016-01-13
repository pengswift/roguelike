using UnityEngine;
using System.Collections;

public class Enemy : MovingObject {

	public int playerDamage;

	private Animator animator;
	private Transform target; // 存储玩家信息
	private bool skipMove;

	public AudioClip enemyAttack1;
	public AudioClip enemyAttack2;


	protected override void Start() {

		// 添加到GameMangaer 脱管
		GameManager.instance.AddEnemyToList(this);
		animator = GetComponent<Animator>();
		target = GameObject.FindGameObjectWithTag ("Player").transform;

		base.Start ();
	}

	protected override bool AttemptMove<T> (int xDir, int yDir) {
		bool canMove = false;
		// 如果跳过， 重置skipMove 返回
		if (skipMove) {
			skipMove = false;
			return canMove;
		}

		// 尝试移动
		canMove = base.AttemptMove<T> (xDir, yDir);

		// 跳过 没移动一次，休息一次
		skipMove = true;

		return canMove;
	}

	public void MoveEnemy() {
		int xDir = 0;
		int yDir = 0;

		// 根据玩家的位置，移动自身位置
		if (Mathf.Abs (target.position.x - transform.position.x) < float.Epsilon) {
			yDir = target.position.y > transform.position.y ? 1 : -1;
		} else {
			xDir = target.position.x > transform.position.x ? 1 : -1;
		}

		AttemptMove<Player> (xDir, yDir);
	}

	protected override void OnCantMove<T>(T component) {
		Player hitPlayer = component as Player;

		hitPlayer.LoseFood (playerDamage);
		animator.SetTrigger ("enemyAttack");
		SoundManager.instance.RandomizeSfx (enemyAttack1, enemyAttack2);
	}
}
