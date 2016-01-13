using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour {

	// 内墙被攻击之后显示的sprite
	public Sprite dmgSprite;
	public int hp = 3; // 血量

	public AudioClip chopSound1;
	public AudioClip chopSound2;

	private SpriteRenderer spriteRenderer;

	void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}

	public void DamageWall(int loss) {
		SoundManager.instance.RandomizeSfx (chopSound1, chopSound2);
		// 被攻击后，替换sprite
		spriteRenderer.sprite = dmgSprite;
		// 血量减少
		hp -= loss;
		if (hp <= 0) {
			// 设置不渲染，（此处也可销毁，但由于该游戏场景资源过小，不必立刻销毁，在下载重新加载关卡时被自动销毁即可。
			// 如果游戏规则有复活功能，可不必在重新常见该组件，而且重新创建组件必须在之前保存坐标，不可取
			gameObject.SetActive (false);
		}
	}
}
