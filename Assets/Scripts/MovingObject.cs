using UnityEngine;
using System.Collections;

// 抽象类， 所有移动对象可继承该类
public abstract class MovingObject : MonoBehaviour {

	public float moveTime = 0.1f;     // 移动时间
	public LayerMask blockingLayer;   // 碰撞检测层

	private BoxCollider2D boxCollider; // box2d碰撞器
	private Rigidbody2D rb2D; // 刚体组建
	private float inverseMoveTime; // 移动速度  设置的sprite 1单位 ＝ 32像素，所以移动单个位置的速度是 1/moveTime

	// 可被子类重载
	protected virtual void Start() {
		boxCollider = GetComponent<BoxCollider2D> ();
		rb2D = GetComponent<Rigidbody2D> ();

		inverseMoveTime = 1f / moveTime;
	}

	// 平滑移动
	protected IEnumerator SmoothMovement(Vector3 end) {
		// 计算到目标位置的距离的平方
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		// 如果数值有效
		while (sqrRemainingDistance > float.Epsilon) {
			// maxDistanceDelta = v*t  最大移动距离 如果超过该距离， 则取end值
			// 计算平滑移动到的位置
			Vector3 newPosition = Vector3.MoveTowards (rb2D.position, end, inverseMoveTime * Time.deltaTime);
			// 移动到该位置
			rb2D.MovePosition (newPosition);
			// 重新计算到目标位置距离的平方
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;

			// 循环执行，直到退出
			yield return null;
		}
	}

	// 移动距离
	protected bool Move(int xDir, int yDir, out RaycastHit2D hit) {
		Vector2 start = transform.position;
		// 目标位置
		Vector2 end = start + new Vector2 (xDir, yDir);

		// 取消自身碰撞器， 不然 光线会先检测到自身
		boxCollider.enabled = false;

		// 射线监测碰撞物体 
		hit = Physics2D.Linecast (start, end, blockingLayer);

		boxCollider.enabled = true;

		// 如果没有，则平滑的移动到该位置
		if (hit.transform == null) {
			StartCoroutine (SmoothMovement (end));
			return true;
		}

		return false;
	}

	// 可被子类重载
	// 尝试移动
	protected virtual bool AttemptMove<T> (int xDir, int yDir) 
		where T : Component {
		RaycastHit2D hit;

		// 移动
		bool canMove = Move (xDir, yDir, out hit);

		// 如果没有碰撞到什么，则直接返回
		if (hit.transform == null) {
			return true;
		}

		// 如果碰撞到什么， 执行OnCantMove方法，不能移动的方法， 用于触发其它事件
		T hitComponent = hit.transform.GetComponent<T> ();

		if (!canMove && hitComponent != null) {
			OnCantMove (hitComponent);
		}

		return canMove;
	}

	// 可被子类实现
	protected abstract void OnCantMove<T> (T component) where T : Component;

}
