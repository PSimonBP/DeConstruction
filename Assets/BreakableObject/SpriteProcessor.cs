using UnityEngine;
using System.Collections;

public class SpriteProcessor : MonoBehaviour
{
	public Sprite Sprite;
	public GameObject BoxPrefab;

	public void BuildObject ()
	{
		var tTexture = Sprite.texture;
		for (int x=0; x<tTexture.width; x++) {
			for (int y=0; y<tTexture.height; y++) {
				var tColor = tTexture.GetPixel (x, y);
				if (tColor.a > 0) {
					GameObject tBox = Instantiate<GameObject> (BoxPrefab);
					tBox.transform.parent = transform;
					tBox.transform.localScale = new Vector3 (0.2f, 0.2f, 1);
					tBox.transform.localPosition = new Vector3 (0.2f * (x - tTexture.width / 2), 0.2f * (y - tTexture.height / 2), 0);
					tBox.GetComponent<SpriteRenderer> ().color = tColor;
				}
			}
		}
		gameObject.AddComponent<Rigidbody2D> ();
		gameObject.AddComponent<BreakableContainer> ();
		var tSprite = GetComponent<SpriteRenderer> ();
		if (tSprite)
			tSprite.enabled = false;
		DestroyImmediate (this);
	}
}
