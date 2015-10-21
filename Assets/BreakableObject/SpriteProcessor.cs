using UnityEngine;
using System.Collections;

public class SpriteProcessor : MonoBehaviour
{
	public Sprite Sprite;
	public GameObject BoxPrefab;

	public void BuildObject()
	{
		var tTexture = Sprite.texture;
		for (int x=0; x<tTexture.width; x += 4) {
			for (int y=0; y<tTexture.height; y += 4) {
				var tColor = tTexture.GetPixel(x, y);
				if (tColor.a > 0) {
					GameObject tBox = Instantiate<GameObject>(BoxPrefab);
					tBox.transform.parent = transform;
					tBox.transform.localScale = new Vector3(0.4f, 0.4f, 1);
					tBox.transform.position = new Vector3((0.1f * x), (0.1f * y), 0);
					tBox.GetComponent<SpriteRenderer>().color = tColor;
				}
			}
		}
		gameObject.AddComponent<Rigidbody2D>();
		gameObject.AddComponent<BreakableContainer>();
		DestroyImmediate(this);
	}
}
