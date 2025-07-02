using UnityEditor; 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Path = System.IO.Path;

class ModelPostprocessor : AssetPostprocessor
{
	// import settings
	private IniFile m_iniFile = new IniFile();
	Dictionary<string, string> m_tags = new Dictionary<string, string>();
	//////////////////

	bool IsMeshConvex(Mesh mesh)
	{
		var vertices = mesh.vertices;
		for (int i = 0; i < mesh.subMeshCount; ++i)
		{
			var triangles = mesh.GetTriangles(i);
			for (int j = 0; j < triangles.Length; j += 3)
			{
				int i0 = triangles[j+0];
				int i1 = triangles[j+1];
				int i2 = triangles[j+2];
				var v0 = vertices[i0];
				var v1 = vertices[i1];
				var v2 = vertices[i2];
				var side1 = v1 - v0;
				var side2 = v2 - v0;
				var normal = Vector3.Cross(side1, side2).normalized;
				var centroid = (v0 + v1 + v2) / 3.0f;
				var plane = new Plane(normal, centroid + normal * 0.0001f);
				for (int k = 0; k < vertices.Length; ++k)
				{
					if (plane.GetSide(vertices[k]))
						return false;
				}
			}
		}

		return true;
	}

	public override int GetPostprocessOrder()
	{
		return 1;
	}

	static string SanitizeDirectorySeparators(string path)
	{
		char wrongSeparator;
		if (System.IO.Path.DirectorySeparatorChar == '/')
			wrongSeparator = '\\';
		else
			wrongSeparator = '/';
		return path.Replace(wrongSeparator, System.IO.Path.DirectorySeparatorChar);
	}

	void ReadImportSettingsFile(GameObject root)
	{
		var fullAssetPath = System.IO.Path.ChangeExtension(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), assetPath), ".importsettings");
		fullAssetPath = SanitizeDirectorySeparators(fullAssetPath);
		if (System.IO.File.Exists(fullAssetPath))
		{
			Debug.Log("Loading import settings ini file: " + fullAssetPath);
			m_iniFile.Load(fullAssetPath);
		}
		else
		{
			return;
		}

		var tagsSection = m_iniFile.GetSection("Tags");
		foreach (IniFile.IniSection.IniKey key in tagsSection.Keys)
		{
			string transformPath = key.Name;
			if (transformPath != string.Empty && transformPath[0] != '/')
				transformPath = "/" + key;
			transformPath = root.name + transformPath;
			m_tags.Add(transformPath, key.Value);
		}
	}

	void OnPreprocessModel()
	{
		ModelImporter importer = assetImporter as ModelImporter;
		//importer.tangentImportMode = ModelImporterTangentSpaceMode.Import;
		importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
		importer.materialSearch = ModelImporterMaterialSearch.Everywhere;

		//if (assetPath.Contains("@"))
		//	importer.importMaterials = false;
		//else
		//	importer.importMaterials = true;

		Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
		if (asset == null)
		{
			//importer.animationType = ModelImporterAnimationType.None;
		}

	}

	void OnPostprocessModel(GameObject root)
	{
		root.transform.localPosition = Vector3.zero;
		root.transform.localRotation = Quaternion.identity;
		root.transform.localScale = Vector3.one;
		
		ReadImportSettingsFile(root);

		ModelImporter importer = assetImporter as ModelImporter;

		bool optimizeMesh = importer.optimizeMesh;
		var comparer = new MeshUtils.NegativeZTriangleComparer();

		root.ForEachComponentInDescendants<MeshFilter>(
			delegate(MeshFilter meshFilter)
			{
				var renderer = meshFilter.GetComponent<Renderer>();
				var sharedMesh = meshFilter.sharedMesh;
				var vertices = sharedMesh.vertices;
				var sharedMaterials = renderer.sharedMaterials;
				int sharedMaterialCount = sharedMaterials.Length;
				for (int i = 0; i < sharedMaterialCount; ++i)
				{
					var sharedMaterial = sharedMaterials[i];
					if (sharedMaterial.GetTag("RenderType", false) == "Transparent")
					{
						var triangles = sharedMesh.GetTriangles(i);
						var newTriangles = MeshUtils.SortTriangles(vertices, triangles, comparer);
						sharedMesh.SetTriangles(newTriangles, i);
					}
					else if (optimizeMesh)
					{
						var triangles = sharedMesh.GetTriangles(i);
						var newTriangles = VertexCacheOptimizer.Optimize(vertices, triangles);
						sharedMesh.SetTriangles(newTriangles, i);
					}
				}

				return true;
			}
		);

#if false
		string prefabPath = System.IO.Path.ChangeExtension(assetPath, "prefab");
		GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
		if (prefab != null)
		{
			if (RefreshPrefab.Refresh(prefab) != null)
				Debug.Log("Refreshed prefab: "+prefabPath);
		}
#endif
	}

	static string PathCombine(string path1, string path2)
	{
		string path = System.IO.Path.Combine(path1, path2);
		path = path.Replace('\\', '/');
		return path;
	}
	
	public static T[] GetAtPath<T>(string path)
	{
		ArrayList al = new ArrayList();
		string dataPath = System.IO.Path.GetDirectoryName(Application.dataPath);
		string [] fileEntries = Directory.GetFiles(SanitizeDirectorySeparators(System.IO.Path.Combine(dataPath, path)));
		foreach(string fileName in fileEntries)
		{
			if (fileName.EndsWith(".meta"))
				continue;
				
			string localPath = PathCombine(path, System.IO.Path.GetFileName(fileName));

			Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));
			if(t != null)
				al.Add(t);
		}

		T[] result = new T[al.Count];
		for(int i=0;i<al.Count;i++)
			result[i] = (T)al[i];

		return result;
	}

	/*Material OnAssignMaterialModel(Material material, Renderer renderer)
	{
		ModelImporter importer = assetImporter as ModelImporter;
		if (!importer.importMaterials)
			return null;

		if (renderer.name.EndsWith("_collision"))
			return (Material)Resources.Load("Materials/DebugVertexAlpha", typeof(Material));

		string assetDir = System.IO.Path.GetDirectoryName(assetPath);
		string parentDir = System.IO.Path.GetDirectoryName(assetDir);		
		string materialDir = PathCombine(parentDir, "Materials");
		string materialName = material.name;

		if (string.IsNullOrEmpty(materialName))
			return null;

		string materialPath = PathCombine(materialDir, materialName + ".mat");
		var existingMaterial = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));
		if (existingMaterial != null)
		{
			material = existingMaterial;
		}
		else
		{
			if (material.mainTexture != null)
			{
				var textureAssetPath = AssetDatabase.GetAssetPath(material.mainTexture);
				var textureImporter = (TextureImporter)AssetImporter.GetAtPath(textureAssetPath);
				if (textureImporter.DoesSourceTextureHaveAlpha())
					if (textureImporter.DoesSourceTextureHaveColor())
						material.shader = Shader.Find("PlaySide/UnlitAlpha");
					else
						material.shader = Shader.Find("PlaySide/UnlitAlpha8");
				else
					material.shader = Shader.Find("PlaySide/Unlit");
			}
			else
			{
				Mesh mesh = null;
				if (renderer is SkinnedMeshRenderer)
				{
					mesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
				}
				else if (renderer is MeshRenderer)
				{
					var meshFilter = renderer.GetComponent<MeshFilter>();
					mesh = meshFilter.sharedMesh;
				}

				if (mesh != null && mesh.colors != null)
				{
					bool hasVertexAlpha = false;
					foreach (var color in mesh.colors)
					{
						if (color.a != 1.0f)
						{
							hasVertexAlpha = true;
							break;
						}
					}
					if (hasVertexAlpha)
						material.shader = Shader.Find("PlaySide/UnlitVertexAlpha");
					else
						material.shader = Shader.Find("PlaySide/UnlitVertex");
				}
				else
				{
					material.shader = Shader.Find("PlaySide/UnlitVertex");
				}
			}

			//

			string fullMaterialDir = PathCombine(System.IO.Path.GetDirectoryName(Application.dataPath), materialDir);
			fullMaterialDir = fullMaterialDir.Replace('/', System.IO.Path.DirectorySeparatorChar);
			if (!System.IO.Directory.Exists(fullMaterialDir))
				AssetDatabase.CreateFolder(parentDir, "Materials");
			AssetDatabase.CreateAsset(material, materialPath);
		}

		return material;
	}*/
}
