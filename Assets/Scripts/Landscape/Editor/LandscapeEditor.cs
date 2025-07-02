using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

public enum LandscapeEditorOperation
{
	EditHeightField,
	PaintAttributes,
	PaintTextures,
	RotateTile
}

[CustomEditor(typeof(Landscape))]
public class LandscapeEditor : Editor
{
	public LandscapeEditorWindow window;
	public Landscape landscape;

	TileFlag flagToPaint = TileFlag.None;
	Tool lastTool = Tool.None;

	Texture[] paintPreviewTexture;

	LandscapeEditorOperation currentOperation = LandscapeEditorOperation.EditHeightField;
	LandscapeTile[] originalTileState;

	bool hasFocus = false;
	bool hasDragArea = false;
	int startDragX;
	int startDragY;
	int endDragX;
	int endDragY;

	public void OnEnable()
	{
		landscape = (Landscape)target;
		lastTool = Tools.current;
		Tools.current = Tool.None;

		paintPreviewTexture = new Texture[3];
		CreateDefaultTilePreviews();

		if (landscape.tiles == null || landscape.tiles.Length != landscape.w*landscape.h)
		{
			landscape.tiles = new LandscapeTile[landscape.w*landscape.h];
			landscape.InitialiseTiles (ref landscape.tiles);
		}
	}

	public void OnDisable()
	{
		if (window != null)
		{
			window.Close();
			window = null;
		}

		Tools.current = lastTool;
	}

	public override void OnInspectorGUI()
	{
		GUI.changed = false;

		//world space dimensions of each tile
		landscape.tileWidth = EditorGUILayout.FloatField("Tile Width", landscape.tileWidth);
		landscape.tileHeight = EditorGUILayout.FloatField("Tile Height", landscape.tileHeight);
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		//tile sheet basics
		var oldTileSheet = landscape.tileSheet;
		landscape.tileSheet = (Texture2D)EditorGUILayout.ObjectField("Tile Sheet", landscape.tileSheet, typeof(UnityEngine.Object), false, null);
		landscape.tileSheetPixelsPerTile = EditorGUILayout.IntField("Pixels Per Tile", landscape.tileSheetPixelsPerTile);
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		if (oldTileSheet != landscape.tileSheet)
			CreateDefaultTilePreviews();

		//change the rest of the display depending on what edit mode we are in
		currentOperation = (LandscapeEditorOperation)EditorGUILayout.EnumPopup("Edit Mode", currentOperation);
		if (currentOperation == LandscapeEditorOperation.PaintAttributes)
			flagToPaint = (TileFlag)EditorGUILayout.EnumPopup("Paint Tile Attribute", flagToPaint);

		if (currentOperation == LandscapeEditorOperation.PaintTextures)
		{
			EditorGUILayout.LabelField(new GUIContent("Surface (Key 1)"), new GUIContent(paintPreviewTexture[0]));
			EditorGUILayout.LabelField(new GUIContent("Dirt Top (Key 2)"), new GUIContent(paintPreviewTexture[1]));
			EditorGUILayout.LabelField(new GUIContent("Dirt (Key 3)"), new GUIContent(paintPreviewTexture[2]));

			//button to open the tile picker in a separate window
			if (GUILayout.Button("Tile Picker"))
			{
				if (window == null)
					window = new LandscapeEditorWindow();

				window.Initialise(this);
				window.Show(true);
			}
		}

		if (GUILayout.Button("Rebuild"))
			landscape.RebuildMesh();

		if (GUI.changed)
		{
			EditorUtility.SetDirty(landscape);
			landscape.RebuildMesh();
		}
	}

	public void OnSceneGUI()
	{
		if (UpdateResizing() || UpdateHeightField())
			return;

		if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			int tileX = 0, tileY = 0;
			Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			if (landscape.PickTile(ray, ref tileX, ref tileY))
			{
				//no drag area for rotation (for now, maybe a key shortcut or something for rotation index would help)
				if (currentOperation == LandscapeEditorOperation.RotateTile)
				{
					landscape.RotateTile(tileX, tileY);
					OverrideFocus();
				}
				else
				{
					startDragX = endDragX = tileX;
					startDragY = endDragY = tileY;

					SaveState();
					PerformOperation(tileX, tileY, tileX, tileY);
					OverrideFocus();

					hasFocus = true;
					hasDragArea = true;
				}
			}
		}
		else if (hasFocus)
		{
			if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
			{
				hasFocus = false;
			}
			else if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
			{
				int tileX = 0, tileY = 0;
				Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				if (landscape.PickTile(ray, ref tileX, ref tileY))
				{
					if (tileX != endDragX || tileY != endDragY)
					{
						endDragX = tileX;
						endDragY = tileY;

						PerformOperation(startDragX, startDragY, endDragX, endDragY);
					}
				}

				OverrideFocus();
			}
			else if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
			{
				RestoreState();
				RebuildMesh();
				hasFocus = false;
			}
		}
	}

	void PerformOperation(int x0, int y0, int x1, int y1)
	{
		RestoreState();

		//no idea which way we have dragged a rect. sort the indices first.
		int i0 = Math.Min (x0, x1);
		int i1 = Math.Max (x0, x1);

		int j0 = Math.Min (y0, y1);
		int j1 = Math.Max (y0, y1);

		//apply the tile operation on the entire range
		for (int j = j0; j <= j1; ++j)
		{
			for (int i = i0; i <= i1; ++i)
			{
				if (currentOperation == LandscapeEditorOperation.PaintAttributes && flagToPaint != TileFlag.None)
					ToggleTileAttribute(i, j, flagToPaint);
				
				if (currentOperation == LandscapeEditorOperation.PaintTextures)
					landscape.PaintTile(i, j);
			}
		}

		RebuildMesh();
	}

	void OverrideFocus()
	{
		//normally clicking on a mesh will cause unity to switch selection
		//focus to the game object that contains the meshfilter. we
		//override the focus here in order to prevent that from happening
		GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
		Event.current.Use();
	}

	void ToggleTileAttribute(int tileX, int tileY, TileFlag flag)
	{
		if (landscape.HasFlag(tileX, tileY, flag))
			landscape.RemoveFlag(tileX, tileY, flag);
		else
			landscape.AddFlag(tileX, tileY, flag);

		RebuildMesh();
	}

	bool UpdateHeightField()
	{
		if (landscape == null)
			return false;

		if (currentOperation == LandscapeEditorOperation.EditHeightField && hasDragArea)
		{
			var fill = new Color(1.0f, 1.0f, 0.0f, 0.25f);
			var outline = Color.yellow;
			var verts = new Vector3[4];
			var half = 0.5f * landscape.tileWidth;
			var offset = 0.01f; 
			
			int i0 = Mathf.Min (startDragX, endDragX), i1 = Mathf.Max(startDragX, endDragX);
			int j0 = Mathf.Min (startDragY, endDragY), j1 = Mathf.Max(startDragY, endDragY);
			
			for (int j = j0; j <= j1; ++j)
			{
				for (int i = i0; i <= i1; ++i)
				{
					var centre = landscape.GetTileCentre(i, j);
					verts[0].Set (centre.x - half, centre.y + offset, centre.z - half);
					verts[1].Set (centre.x + half, centre.y + offset, centre.z - half);
					verts[2].Set (centre.x + half, centre.y + offset, centre.z + half);
					verts[3].Set (centre.x - half, centre.y + offset, centre.z + half);
					
					Handles.DrawSolidRectangleWithOutline(verts, fill, outline);
					
					//dont draw the height adjustment handles if we are still dragging out the area to manip
					if (!hasFocus)
					{
						var newHandlePosition = Handles.FreeMoveHandle(centre, Quaternion.identity, 0.1f, Vector3.up, Handles.DotHandleCap);

						var newHeight = (int)(newHandlePosition.y/landscape.tileHeight);

						int tileIndex = i + j*landscape.w;
						if (newHeight != landscape.tiles[tileIndex].height)
						{
							//we moved the handle far enough to adjust the height of this tile. figure out
							//the difference from the original height (so we can adjust all tiles in the selection)
							int difference = newHeight - landscape.tiles[tileIndex].height;

							for (int jj = j0; jj <= j1; ++jj)
								for (int ii = i0; ii <= i1; ++ii)
									landscape.tiles[ii + jj*landscape.w].height += difference;

							RebuildMesh();
							return true;
						}
					}
				}
			}
		}

		return false;
	}

	bool UpdateResizing()
	{
		var x0 = landscape.tileWidth * landscape.x;
		var x1 = landscape.tileWidth * (landscape.x + landscape.w);
		var y0 = landscape.tileWidth * landscape.y;
		var y1 = landscape.tileWidth * (landscape.y + landscape.h);
		
		Vector3[] resizeSrc = { 
			new Vector3((x0 + x1)*0.5f, 0, y0),
			new Vector3((x0 + x1)*0.5f, 0, y1),
			new Vector3(x0, 0, (y0 + y1)*0.5f),
			new Vector3(x1, 0, (y0 + y1)*0.5f)
		};
		
		Vector3[] resizeDest = {
			resizeSrc[0] + Vector3.back * landscape.tileWidth,
			resizeSrc[1] + Vector3.forward * landscape.tileWidth,
			resizeSrc[2] + Vector3.left * landscape.tileWidth,
			resizeSrc[3] + Vector3.right * landscape.tileWidth
		};
		
		Handles.color = Color.black;
		for (int i = 0; i < 4; ++i)
		{
			Handles.DrawLine(resizeSrc[i], resizeDest[i]);
			
			var newPosition = Handles.FreeMoveHandle(resizeDest[i], Quaternion.identity, 0.1f, Vector3.one, Handles.DotHandleCap);

			var newX = landscape.x;
			var newY = landscape.y;
			var newW = landscape.w;
			var newH = landscape.h;
			
			switch (i)
			{
			case 0:
			{
				var pos = (int)((newPosition.z + 0.1f)/landscape.tileWidth);
				if (pos < landscape.y + landscape.h)
				{
					newY = pos;
					newH = (landscape.y + landscape.h) - pos;
				}
				
				break;
			}
				
			case 1:
			{
				var pos = (int)((newPosition.z - 0.1f)/landscape.tileWidth);
				if (pos > landscape.y)
					newH = pos - landscape.y;
				
				break;
			}
				
			case 2:
			{
				var pos = (int)((newPosition.x + 0.1f)/landscape.tileWidth);
				if (pos < landscape.x + landscape.w)
				{
					newX = pos;
					newW = (landscape.x + landscape.w) - pos;
				}
				break;
			}
				
			case 3:
			{
				var pos = (int)((newPosition.x - 0.1f)/landscape.tileWidth);
				if (pos > landscape.x)
					newW = pos - landscape.x;
				
				break;
			}
			}
			
			if (landscape.x != newX || landscape.y != newY || landscape.w != newW || landscape.h != newH)
			{
				ResizeLandscape(newX, newY, newW, newH);
				return true;
			}
		}

		return false;
	}

	void ResizeLandscape(int newX, int newY, int newW, int newH)
	{
		var newTiles = new LandscapeTile[newW*newH];
		landscape.InitialiseTiles(ref newTiles);

		//preserver existing tile information if possible
		for (int j = landscape.y; j < landscape.y + landscape.h; ++j)
		{
			for (int i = landscape.x; i < landscape.x + landscape.w; ++i)
			{
				//ignore old tiles if they are outside the bounds of the new landscape
				if (i < newX || i >= newX + newW || j < newY || j >= newY + newH)
					continue;

				//index of the old tile relative to the new grid size
				int newI = i - newX;
				int newJ = j - newY;
				int oldI = i - landscape.x;
				int oldJ = j - landscape.y;

				//copy.
				newTiles[newI + newJ*newW] = landscape.tiles[oldI + oldJ*landscape.w];
			}
		}

		landscape.tiles = newTiles;
		landscape.x = newX;
		landscape.y = newY;
		landscape.w = newW;
		landscape.h = newH;

		RebuildMesh();
	}

	void RebuildMesh()
	{
		landscape.RebuildMesh();
	}

	public void CreateDefaultTilePreviews()
	{
		CreateTilePreview(0, landscape.paintIndexSurface);
		CreateTilePreview(1, landscape.paintIndexDirtTop);
		CreateTilePreview(2, landscape.paintIndexDirt);
	}
	
	public void CreateTilePreview(int paintIndex, int tileIndex)
	{
		if (landscape.tileSheet == null)
			return;

		var newTexture = new Texture2D(landscape.tileSheetPixelsPerTile, landscape.tileSheetPixelsPerTile);
		var srcPixels = landscape.tileSheet.GetPixels();
		var destPixels = newTexture.GetPixels();

		int tilesPerRow = landscape.tileSheet.width/landscape.tileSheetPixelsPerTile;
		int srcX = (tileIndex % tilesPerRow) * landscape.tileSheetPixelsPerTile;
		int srcY = (tileIndex / tilesPerRow) * landscape.tileSheetPixelsPerTile;

		for (int j = 0; j < newTexture.height; ++j)
		{
			for (int i = 0; i < newTexture.width; ++i)
			{
				int x = Math.Min (landscape.tileSheet.width - 1, srcX + i);
				int y = Math.Min (landscape.tileSheet.height - 1, srcY + j);

				//account for flipping textures
				int yy = newTexture.height - 1 - j;
				int zz = landscape.tileSheet.height - 1 - y;

				destPixels[i + yy*newTexture.width] = srcPixels[x + zz*landscape.tileSheet.width];
			}
		}

		newTexture.SetPixels(destPixels);
		newTexture.Apply();

		if (paintIndex < 3)
		{
			paintPreviewTexture[paintIndex] = newTexture;
			switch (paintIndex)
			{
			case 0: landscape.paintIndexSurface = tileIndex;	break;
			case 1: landscape.paintIndexDirtTop = tileIndex;  	break;
			case 2: landscape.paintIndexDirt = tileIndex; 		break;
			}

			RebuildMesh(); //TEMP
			Repaint ();
			Resources.UnloadUnusedAssets(); //free the old textures
		}
	}

	void SaveState()
	{
		originalTileState = new LandscapeTile[landscape.tiles.Length];
		for (int i = 0; i < landscape.tiles.Length; ++i)
			originalTileState[i] = landscape.tiles[i];
	}

	void RestoreState()
	{
		for (int i = 0; i < landscape.tiles.Length; ++i)
			landscape.tiles[i] = originalTileState[i];
	}
}

public class LandscapeEditorWindow: EditorWindow
{
	public Landscape landscape;
	public LandscapeEditor editor;

	public void Initialise(LandscapeEditor editor)
	{
		this.editor = editor;
		this.landscape = editor.landscape;
	}

	public void OnGUI()
	{
		if (landscape.tileSheet == null)
			return;

		var size = Mathf.Min (position.width, position.height);
		var rect = new Rect(0, 0, size, size);
		EditorGUI.DrawPreviewTexture(rect, landscape.tileSheet);

		if (Event.current.type == EventType.KeyDown)
		{
			var scale = size/landscape.tileSheet.width;
			var pixelsPerTile = landscape.tileSheetPixelsPerTile * scale;

			int tilesPerRow = landscape.tileSheet.width/landscape.tileSheetPixelsPerTile;
			int tileX = (int)(Event.current.mousePosition.x/pixelsPerTile);
			int tileY = (int)(Event.current.mousePosition.y/pixelsPerTile);
			int index = tileX + tileY*tilesPerRow;

			if (index >= 0 && index < tilesPerRow*tilesPerRow)
			{
				if (Event.current.keyCode == KeyCode.Alpha1)	editor.CreateTilePreview(0, index);
				if (Event.current.keyCode == KeyCode.Alpha2)	editor.CreateTilePreview(1, index);
				if (Event.current.keyCode == KeyCode.Alpha3)	editor.CreateTilePreview(2, index);
			}
		}
	}
}
