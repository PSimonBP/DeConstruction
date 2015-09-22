using UnityEngine;
using System.Collections;

public class RoomBuilder : MonoBehaviour
{
	public float FloorHeight = 4;
	public float WallThickness = 0.2f;
	public float StructureThickness = 0.5f;
	public float OverlapLength = 0.5f;
	public float OverlapThickness = 0.1f;
	public Material StructureMaterial = null;
	public Material WallMaterial = null;

	void ShiftPos(GameObject tObj, Vector3 tShift)
	{
		tObj.transform.localPosition += tShift;
	}

	void ShiftRot(GameObject tObj, Vector3 tRot)
	{
		Quaternion tRotation = transform.localRotation;
		tRotation.eulerAngles += tRot;
		tObj.transform.localRotation = tRotation;
	}

	GameObject AddBox(string sName, GameObject Parent, Vector3 Size, Vector3 Position = new Vector3(), Vector3 Rotation = new Vector3())
	{
		GameObject tBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
		if (StructureMaterial != null)
			tBox.GetComponent<MeshRenderer>().material = StructureMaterial;
		tBox.name = sName;
		tBox.transform.parent = Parent.transform;
		tBox.transform.localPosition = Vector3.zero;
		tBox.transform.localScale = Size;
		if (Position != Vector3.zero)
			ShiftPos(tBox, Position);
		if (Rotation != Vector3.zero)
			ShiftRot(tBox, Rotation);
		return tBox;
	}

	GameObject CreateCorner(GameObject Parent, Vector3 Position, Vector3 Rotation)
	{
		float ST = StructureThickness;
		float STH = StructureThickness / 2;
		float OT = OverlapThickness;
		float OTH = OverlapThickness / 2;
		float OL = OverlapLength;
		float OLH = OverlapLength / 2;
		float WT = WallThickness;

		GameObject tCorner = new GameObject("Corner");
		tCorner.transform.parent = Parent.transform;
		tCorner.transform.localPosition = Vector3.zero;
		AddBox("Corner", tCorner, new Vector3(ST, ST, ST));
		AddBox("CornerConnector", tCorner, new Vector3(OL, ST, ST), new Vector3(-(OL / 2 + STH), 0, 0));
		AddBox("CornerConnector", tCorner, new Vector3(OL, ST, ST), new Vector3(+(OL / 2 + STH), 0, 0));
		AddBox("CornerConnector", tCorner, new Vector3(ST, OL, ST), new Vector3(0, -(OL / 2 + STH), 0));
		AddBox("CornerConnector", tCorner, new Vector3(ST, OL, ST), new Vector3(0, +(OL / 2 + STH), 0));
		AddBox("CornerConnector", tCorner, new Vector3(ST, ST, OL), new Vector3(0, 0, -(OL / 2 + STH)));
		AddBox("CornerConnector", tCorner, new Vector3(ST, ST, OL), new Vector3(0, 0, +(OL / 2 + STH)));

		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(-STH - OLH, OTH - STH, -STH - OLH));
		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(-STH - OLH, OTH - STH + OT + WT, -STH - OLH));

 		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(+STH + OLH, OTH - STH, +STH + OLH));
 		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(+STH + OLH, OTH - STH + OT + WT, +STH + OLH));

		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(-OTH + STH, STH + OLH, -STH - OLH), new Vector3(0, 0, 90));
		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(-OTH + STH - OT - WT, STH + OLH, -STH - OLH), new Vector3(0, 0, 90));

		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(-OTH + STH, -STH - OLH, STH + OLH), new Vector3(0, 0, 90));
		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(-OTH + STH - OT - WT, -STH - OLH, STH + OLH), new Vector3(0, 0, 90));

		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(-STH - OLH, STH + OLH, -OTH + STH), new Vector3(90, 0, 0));
		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(-STH - OLH, STH + OLH, -OTH + STH - OT - WT), new Vector3(90, 0, 0));

		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(STH + OLH, -STH - OLH, -OTH + STH), new Vector3(90, 0, 0));
		AddBox("CornerOverlap", tCorner, new Vector3(OL, OT, OL), new Vector3(STH + OLH, -STH - OLH, -OTH + STH - OT - WT), new Vector3(90, 0, 0));

		if (Position != Vector3.zero)
			ShiftPos(tCorner, Position);
		if (Rotation != Vector3.zero)
			ShiftRot(tCorner, Rotation);
 		Rigidbody tRigidBody = tCorner.AddComponent<Rigidbody>();
 		tRigidBody.mass = 10;
// 		tRigidBody.Sleep ();
		return tCorner;
	}

	GameObject CreateConnector(GameObject Parent, float Length, Vector3 Position, Vector3 Rotation = new Vector3())
	{
		float OLH = OverlapLength / 2;
		float OT = OverlapThickness;
		float OTH = OverlapThickness / 2;
		float ST = StructureThickness;
		float STH = StructureThickness / 2;
		GameObject tConnector = new GameObject("Connector");
		tConnector.transform.parent = Parent.transform;
		tConnector.transform.localPosition = Vector3.zero;

		AddBox("Structure", tConnector, new Vector3(Length, ST, ST));

		AddBox("StructureOverLap", tConnector, new Vector3(Length, OverlapThickness, OverlapLength), new Vector3(0, OTH - STH, -STH - OLH));
		AddBox("StructureOverLap", tConnector, new Vector3(Length, OverlapThickness, OverlapLength), new Vector3(0, OTH - STH + OverlapThickness + WallThickness, -STH - OLH));

		AddBox("StructureOverLap", tConnector, new Vector3(Length, OverlapThickness, OverlapLength), new Vector3(0, OTH - STH, +STH + OLH));
		AddBox("StructureOverLap", tConnector, new Vector3(Length, OverlapThickness, OverlapLength), new Vector3(0, OTH - STH + OverlapThickness + WallThickness, +STH + OLH));

		AddBox("StructureOverLap", tConnector, new Vector3(Length, OverlapLength, OverlapThickness), new Vector3(0, STH + OLH, STH - OTH));
		AddBox("StructureOverLap", tConnector, new Vector3(Length, OverlapLength, OverlapThickness), new Vector3(0, STH + OLH, STH - OTH - WallThickness - OT));

		AddBox("StructureOverLap", tConnector, new Vector3(Length, OverlapLength, OverlapThickness), new Vector3(0, -STH - OLH, STH - OTH));
		AddBox("StructureOverLap", tConnector, new Vector3(Length, OverlapLength, OverlapThickness), new Vector3(0, -STH - OLH, STH - OTH - WallThickness - OT));

		if (Position != Vector3.zero)
			ShiftPos(tConnector, Position);
		if (Rotation != Vector3.zero)
			ShiftRot(tConnector, Rotation);
 		Rigidbody tRigidBody = tConnector.AddComponent<Rigidbody>();
 		tRigidBody.mass = 10;
// 		tRigidBody.Sleep();
		return tConnector;
	}

	GameObject CreateWall(GameObject Parent, Vector3 Size, Vector3 Position, Vector3 Rotation = new Vector3())
	{
		GameObject tWall = AddBox("Wall", Parent, Size);
		if (Position != Vector3.zero)
			ShiftPos(tWall, Position);
		if (Rotation != Vector3.zero)
			ShiftRot(tWall, Rotation);
 		Rigidbody tRigidBody = tWall.AddComponent<Rigidbody>();
 		tRigidBody.mass = 10;
// 		tRigidBody.Sleep();
 		if (WallMaterial != null)
			tWall.GetComponent<MeshRenderer>().material = WallMaterial;
		return tWall;
	}

	public void CreateRoom()
	{
		float STH = StructureThickness / 2;
		float STD = StructureThickness * 2;
		float OLD = OverlapLength * 2;
		float WT = WallThickness;
		float WTH = WallThickness / 2;
		float OT = OverlapThickness;
		Vector3 POS = transform.position;
		Vector3 SC = transform.localScale;
		GameObject tBuilding = new GameObject("Room");
		tBuilding.transform.parent = transform.parent;
		tBuilding.transform.position = new Vector3(POS.x, POS.y - SC.y / 2, POS.z);
		tBuilding.transform.rotation = transform.rotation;
		SC.y = FloorHeight;
		Vector3 SCH = SC / 2;

		gameObject.SetActive(false);


		bool bCeiling = false;

		float FH = FloorHeight - StructureThickness;

		for (int i = 0; i < transform.localScale.y / FloorHeight; ++i) {
			if (i >= (transform.localScale.y / FloorHeight) - 1)
				bCeiling = true;
			// Corners
			CreateCorner(tBuilding, new Vector3(+SCH.x - STH, STH + i * FH, +SCH.z - STH), new Vector3(0, 0, 0)).AddComponent<ComplexBreakController>();
			CreateCorner(tBuilding, new Vector3(+SCH.x - STH, STH + i * FH, -SCH.z + STH), new Vector3(0, 90, 0)).AddComponent<ComplexBreakController>();
			CreateCorner(tBuilding, new Vector3(-SCH.x + STH, STH + i * FH, -SCH.z + STH), new Vector3(0, 180, 0)).AddComponent<ComplexBreakController>();
			CreateCorner(tBuilding, new Vector3(-SCH.x + STH, STH + i * FH, +SCH.z - STH), new Vector3(0, 270, 0)).AddComponent<ComplexBreakController>();
			// Upper Corners
			if (bCeiling) {
				CreateCorner(tBuilding, new Vector3(-SCH.x + STH, -STH + SC.y + i * FH, +SCH.z - STH), new Vector3(0, 0, 180)).AddComponent<ComplexBreakController>();
				CreateCorner(tBuilding, new Vector3(+SCH.x - STH, -STH + SC.y + i * FH, +SCH.z - STH), new Vector3(0, 90, 180)).AddComponent<ComplexBreakController>();
				CreateCorner(tBuilding, new Vector3(+SCH.x - STH, -STH + SC.y + i * FH, -SCH.z + STH), new Vector3(0, 180, 180)).AddComponent<ComplexBreakController>();
				CreateCorner(tBuilding, new Vector3(-SCH.x + STH, -STH + SC.y + i * FH, -SCH.z + STH), new Vector3(0, 270, 180)).AddComponent<ComplexBreakController>();
			}
			// Floor Connectors
			CreateConnector(tBuilding, SC.x - STD - OLD, new Vector3(0, STH + i * FH, +SCH.z - STH), new Vector3(0, 0, 0)).AddComponent<ComplexBreakController>();
			CreateConnector(tBuilding, SC.z - STD - OLD, new Vector3(+SCH.x - STH, STH + i * FH, 0), new Vector3(0, 90, 0)).AddComponent<ComplexBreakController>();
			CreateConnector(tBuilding, SC.x - STD - OLD, new Vector3(0, STH + i * FH, -SCH.z + STH), new Vector3(0, 180, 0)).AddComponent<ComplexBreakController>();
			CreateConnector(tBuilding, SC.z - STD - OLD, new Vector3(-SCH.x + STH, STH + i * FH, 0), new Vector3(0, 270, 0)).AddComponent<ComplexBreakController>();
			// Upper connectors
			if (bCeiling) {
				CreateConnector(tBuilding, SC.x - STD - OLD, new Vector3(0, SC.y - STH + i * FH, +SCH.z - STH), new Vector3(0, 0, 180)).AddComponent<ComplexBreakController>();
				CreateConnector(tBuilding, SC.z - STD - OLD, new Vector3(+SCH.x - STH, SC.y - STH + i * FH, 0), new Vector3(0, 90, 180)).AddComponent<ComplexBreakController>();
				CreateConnector(tBuilding, SC.x - STD - OLD, new Vector3(0, SC.y - STH + i * FH, -SCH.z + STH), new Vector3(0, 180, 180)).AddComponent<ComplexBreakController>();
				CreateConnector(tBuilding, SC.z - STD - OLD, new Vector3(-SCH.x + STH, SC.y - STH + i * FH, 0), new Vector3(0, 270, 180)).AddComponent<ComplexBreakController>();
			}
			// Side Connectors
			CreateConnector(tBuilding, SC.y - STD - OLD, new Vector3(+SCH.x - STH, SCH.y + i * FH, -SCH.z + STH), new Vector3(180, 0, 90)).AddComponent<ComplexBreakController>();
			CreateConnector(tBuilding, SC.y - STD - OLD, new Vector3(-SCH.x + STH, SCH.y + i * FH, -SCH.z + STH), new Vector3(180, 90, 90)).AddComponent<ComplexBreakController>();
			CreateConnector(tBuilding, SC.y - STD - OLD, new Vector3(-SCH.x + STH, SCH.y + i * FH, +SCH.z - STH), new Vector3(180, 180, 90)).AddComponent<ComplexBreakController>();
			CreateConnector(tBuilding, SC.y - STD - OLD, new Vector3(+SCH.x - STH, SCH.y + i * FH, +SCH.z - STH), new Vector3(180, 270, 90)).AddComponent<ComplexBreakController>();

			// Walls
 			CreateWall(tBuilding, new Vector3(SC.x - STD, WT, SC.z - STD), new Vector3(0, WTH + OT + i * FH, 0)).AddComponent<BreakController>();
 			if (bCeiling)
 				CreateWall(tBuilding, new Vector3(SC.x - STD, WT, SC.z - STD), new Vector3(0, -WTH - OT + SC.y + i * FH, 0)).AddComponent<BreakController>();
 			CreateWall(tBuilding, new Vector3(WT, SC.y - STD, SC.z - STD), new Vector3(+SCH.x - WTH - OT, SCH.y + i * FH, 0)).AddComponent<BreakController>();
 			CreateWall(tBuilding, new Vector3(WT, SC.y - STD, SC.z - STD), new Vector3(-SCH.x + WTH + OT, SCH.y + i * FH, 0)).AddComponent<BreakController>();
 			CreateWall(tBuilding, new Vector3(SC.x - STD, SC.y - STD, WT), new Vector3(0, SCH.y + i * FH, +SCH.z - WTH - OT)).AddComponent<BreakController>();
 			CreateWall(tBuilding, new Vector3(SC.x - STD, SC.y - STD, WT), new Vector3(0, SCH.y + i * FH, -SCH.z + WTH + OT)).AddComponent<BreakController>();
		}


		// DestroyImmediate(gameObject);
	}

	void Start()
	{
		if (gameObject.activeSelf)
			CreateRoom();
	}
}
