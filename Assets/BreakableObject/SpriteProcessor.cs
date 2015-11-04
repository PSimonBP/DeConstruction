using UnityEngine;
using System.Collections;

public class SpriteProcessor : MonoBehaviour
{
	Sprite Sprite;
	public GameObject BoxPrefab;

	public void BuildObject()
	{
		Sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
		var tTexture = Sprite.texture;
		int px = 0;
		int py = 0;
		for (int x=(int)Sprite.textureRect.x; x<(int)(Sprite.textureRect.x + Sprite.textureRect.width); x++) {
			py = 0;
			for (int y=(int)Sprite.textureRect.y; y<(int)(Sprite.textureRect.y + Sprite.textureRect.height); y++) {
				var tColor = tTexture.GetPixel(x, y);
				if (tColor.a > 0) {
					GameObject tBox = Instantiate<GameObject>(BoxPrefab);
					tBox.transform.parent = transform;
					tBox.transform.localScale = new Vector3(0.2f, 0.2f, 1);
					tBox.transform.localPosition = new Vector3(0.2f * (px - Sprite.textureRect.width / 2), 0.2f * (py - Sprite.textureRect.height / 2), 0);
					tBox.GetComponent<SpriteRenderer>().color = tColor;
				}
				py++;
			}
			px++;
		}
		Rigidbody2D tBody = gameObject.AddComponent<Rigidbody2D>();
		tBody.isKinematic = true;
		tBody.drag = 1;
		tBody.angularDrag = 1;
		gameObject.AddComponent<BreakableContainer>();
		var tSprite = GetComponent<SpriteRenderer>();
		if (tSprite)
			tSprite.enabled = false;
		DestroyImmediate(this);
	}
}
