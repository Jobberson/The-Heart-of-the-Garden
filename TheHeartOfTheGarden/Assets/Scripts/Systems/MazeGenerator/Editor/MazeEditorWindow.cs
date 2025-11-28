// Assets/Editor/MazeGenerator_WithKruskal_Combined.cs
// Editor-only maze generator: DFS or Kruskal, no prefabs required.
// Drop into an "Editor" folder. Open via Tools -> Maze Generator (DFS/Kruskal)

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class MazeEditorWindow : EditorWindow
{
    public enum Algorithm { DFS, Kruskal }

    private int width = 21;
    private int height = 21;
    private float cellSize = 1f;
    private Vector3 center = Vector3.zero;
    private Material wallMaterial;
    private Material floorMaterial;
    private Algorithm algorithm = Algorithm.DFS;

    private int[,] maze;
    private GameObject mazeRoot;
    private Material markerMatGreen;
    private Material markerMatRed;

    // Combine options
    private bool combineIncludeWalls = true;
    private bool combineIncludeFloor = false;
    private bool combineIncludeMarkers = false;
    private bool combineDestroyOriginals = true;
    private bool combineAddMeshCollider = true;

    [MenuItem("Tools/Maze Generator (DFS/Kruskal)")]
    public static void ShowWindow() => GetWindow<MazeEditorWindow>("Maze Generator");

    private void OnGUI()
    {
        GUILayout.Label("Maze Generator (No Prefabs) — DFS or Kruskal", EditorStyles.boldLabel);

        algorithm = (Algorithm)EditorGUILayout.EnumPopup("Algorithm", algorithm);
        width = EditorGUILayout.IntSlider("Width (odd)", width, 5, 201);
        height = EditorGUILayout.IntSlider("Height (odd)", height, 5, 201);
        cellSize = EditorGUILayout.FloatField("Cell Size", cellSize);
        center = EditorGUILayout.Vector3Field("Center", center);
        wallMaterial = (Material)EditorGUILayout.ObjectField("Wall Material", wallMaterial, typeof(Material), false);
        floorMaterial = (Material)EditorGUILayout.ObjectField("Floor Material", floorMaterial, typeof(Material), false);

        GUILayout.Space(6);
        if (GUILayout.Button("Generate Maze"))
            GenerateMaze();

        if (GUILayout.Button("Clear Maze"))
            ClearMaze();

        GUILayout.Space(8);
        GUILayout.Label("Combine / Bake", EditorStyles.boldLabel);
        combineIncludeWalls = EditorGUILayout.Toggle("Include Walls", combineIncludeWalls);
        combineIncludeFloor = EditorGUILayout.Toggle("Include Floor", combineIncludeFloor);
        combineIncludeMarkers = EditorGUILayout.Toggle("Include Markers", combineIncludeMarkers);
        combineAddMeshCollider = EditorGUILayout.Toggle("Add MeshCollider", combineAddMeshCollider);
        combineDestroyOriginals = EditorGUILayout.Toggle("Destroy Originals After Combine", combineDestroyOriginals);
        if (GUILayout.Button("Combine Into Single Mesh"))
            CombineIntoSingleMesh();
    }

    private void GenerateMaze()
    {
        // enforce odd dims
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        ClearMaze();
        mazeRoot = new GameObject("Maze");
        Undo.RegisterCreatedObjectUndo(mazeRoot, "Create Maze Root");

        // init as walls
        maze = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1;

        if (algorithm == Algorithm.DFS)
            CarveMazeDFS();
        else
            CarveMazeKruskal();

        // carve entrance & exit (bottom center and top center)
        Vector2Int entrance = new Vector2Int(1, 0);                  // south edge
        Vector2Int exit = new Vector2Int(width - 2, height - 1);     // north edge
        if (IsInsideGrid(entrance)) maze[entrance.x, entrance.y] = 0;
        if (IsInsideGrid(exit)) maze[exit.x, exit.y] = 0;

        // create floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Undo.RegisterCreatedObjectUndo(floor, "Create Maze Floor");
        floor.name = "Maze_Floor";
        floor.transform.SetParent(mazeRoot.transform, false);
        floor.transform.localScale = new Vector3((width * cellSize) / 10f, 1f, (height * cellSize) / 10f);
        floor.transform.position = center + Vector3.down * 0.01f;
        if (floorMaterial) floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;

        // offset for centering
        Vector3 offset = center - new Vector3((width - 1) * cellSize * 0.5f, 0f, (height - 1) * cellSize * 0.5f);

        // create walls
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (maze[x, y] == 1)
                    CreateWallAt(new Vector2Int(x, y), offset);

        // marker materials
        markerMatGreen = new Material(Shader.Find("Standard")) { color = Color.green };
        markerMatRed = new Material(Shader.Find("Standard")) { color = Color.red };

        CreateMarker(entrance, offset, markerMatGreen, "Entrance_Marker");
        CreateMarker(exit, offset, markerMatRed, "Exit_Marker");

        Selection.activeObject = mazeRoot;
        Debug.Log($"Maze generated ({algorithm}) {width}x{height} centered at {center}. Entrance: {entrance}, Exit: {exit}");
    }

    private void CarveMazeDFS()
    {
        System.Random rng = new System.Random();
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int start = new Vector2Int(1, 1);
        if (!IsInsideGrid(start)) return;

        maze[start.x, start.y] = 0;
        stack.Push(start);

        Vector2Int[] dirs = new[] { new Vector2Int(0, 2), new Vector2Int(0, -2), new Vector2Int(2, 0), new Vector2Int(-2, 0) };

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> neighbors = new List<Vector2Int>();

            foreach (var d in dirs)
            {
                Vector2Int next = current + d;
                if (IsInsideGrid(next) && maze[next.x, next.y] == 1) // unvisited
                    neighbors.Add(next);
            }

            if (neighbors.Count > 0)
            {
                Vector2Int next = neighbors[rng.Next(neighbors.Count)];
                Vector2Int between = new Vector2Int((current.x + next.x) / 2, (current.y + next.y) / 2);
                maze[between.x, between.y] = 0;
                maze[next.x, next.y] = 0;
                stack.Push(next);
            }
            else stack.Pop();
        }
    }

    // Kruskal's algorithm for maze generation
    private void CarveMazeKruskal()
    {
        // cells are at odd coordinates: (1,1), (3,1), ...
        int cellCountX = (width - 1) / 2;
        int cellCountY = (height - 1) / 2;
        int totalCells = cellCountX * cellCountY;
        if (cellCountX <= 0 || cellCountY <= 0) return;

        // mark all cell centers as open
        for (int cx = 0; cx < cellCountX; cx++)
            for (int cy = 0; cy < cellCountY; cy++)
                maze[cx * 2 + 1, cy * 2 + 1] = 0;

        // build edge list (connect each cell to right and up neighbor to avoid duplicates)
        var edges = new List<Edge>();
        for (int cx = 0; cx < cellCountX; cx++)
        {
            for (int cy = 0; cy < cellCountY; cy++)
            {
                int aIndex = cx + cy * cellCountX;
                // right neighbor
                if (cx < cellCountX - 1)
                {
                    int bIndex = (cx + 1) + cy * cellCountX;
                    int wallX = (cx * 2 + 1) + 1; // midpoint x
                    int wallY = cy * 2 + 1;
                    edges.Add(new Edge(aIndex, bIndex, wallX, wallY));
                }
                // up neighbor
                if (cy < cellCountY - 1)
                {
                    int bIndex = cx + (cy + 1) * cellCountX;
                    int wallX = cx * 2 + 1;
                    int wallY = (cy * 2 + 1) + 1; // midpoint y
                    edges.Add(new Edge(aIndex, bIndex, wallX, wallY));
                }
            }
        }

        // shuffle edges
        System.Random rng = new System.Random();
        for (int i = edges.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            var tmp = edges[i]; edges[i] = edges[j]; edges[j] = tmp;
        }

        // union-find init
        int[] parent = new int[totalCells];
        for (int i = 0; i < totalCells; i++) parent[i] = -1; // negative size root

        System.Func<int, int> Find = null;
        Find = (x) =>
        {
            int root = x;
            while (parent[root] >= 0) root = parent[root];
            // path compress
            while (x != root)
            {
                int next = parent[x];
                parent[x] = root;
                x = next;
            }
            return root;
        };

        System.Action<int, int> Union = (a, b) =>
        {
            int ra = Find(a), rb = Find(b);
            if (ra == rb) return;
            // union by size
            if (parent[ra] > parent[rb]) { int t = ra; ra = rb; rb = t; }
            parent[ra] += parent[rb];
            parent[rb] = ra;
        };

        // process edges
        foreach (var e in edges)
        {
            if (Find(e.a) != Find(e.b))
            {
                Union(e.a, e.b);
                if (IsInsideWall(e.wallX, e.wallY))
                    maze[e.wallX, e.wallY] = 0; // remove wall between cells
            }
        }
    }

    private bool IsInsideWall(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    private void CreateWallAt(Vector2Int gridPos, Vector3 offset)
    {
        Vector3 wpos = new Vector3(gridPos.x * cellSize, 0f, gridPos.y * cellSize) + offset;
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(cube, "Create Maze Wall");
        cube.name = $"Wall_{gridPos.x}_{gridPos.y}";
        cube.transform.SetParent(mazeRoot.transform, false);
        cube.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
        cube.transform.position = wpos + Vector3.up * (cellSize * 0.5f);
        if (wallMaterial) cube.GetComponent<Renderer>().sharedMaterial = wallMaterial;
    }

    private void CreateMarker(Vector2Int gridPos, Vector3 offset, Material mat, string name)
    {
        if (!IsInsideGrid(gridPos)) return;
        Vector3 pos = new Vector3(gridPos.x * cellSize, 0f, gridPos.y * cellSize) + offset;
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(marker, "Create Marker");
        marker.name = name;
        marker.transform.SetParent(mazeRoot.transform, false);
        marker.transform.localScale = new Vector3(cellSize * 0.6f, cellSize * 1.2f, cellSize * 0.6f);
        marker.transform.position = pos + Vector3.up * (cellSize * 0.6f);
        marker.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private bool IsInsideGrid(Vector2Int p)
    {
        return p.x >= 0 && p.y >= 0 && p.x < width && p.y < height;
    }

    private void ClearMaze()
    {
        var existing = GameObject.Find("Maze");
        if (existing) Undo.DestroyObjectImmediate(existing);

        if (mazeRoot != null)
        {
            Undo.DestroyObjectImmediate(mazeRoot);
            mazeRoot = null;
        }
    }

    private void CombineIntoSingleMesh()
    {
        if (mazeRoot == null)
        {
            Debug.LogWarning("No maze to combine. Generate the maze first.");
            return;
        }

        // collect MeshFilters based on toggles
        var meshFilters = new List<MeshFilter>();
        var renderers = new List<Renderer>();

        foreach (var mf in mazeRoot.GetComponentsInChildren<MeshFilter>())
        {
            var go = mf.gameObject;
            // decide whether to include this object
            if (!combineIncludeWalls && go.name.StartsWith("Wall_")) continue;
            if (!combineIncludeFloor && go.name == "Maze_Floor") continue;
            if (!combineIncludeMarkers && (go.name == "Entrance_Marker" || go.name == "Exit_Marker")) continue;

            // make sure the mesh exists
            if (mf.sharedMesh == null) continue;
            meshFilters.Add(mf);
            var r = go.GetComponent<Renderer>();
            if (r != null) renderers.Add(r);
            else renderers.Add(null);
        }

        if (meshFilters.Count == 0)
        {
            Debug.LogWarning("No meshes found to combine with current include settings.");
            return;
        }

        // create combined GO
        GameObject combinedGO = new GameObject("Maze_Combined");
        Undo.RegisterCreatedObjectUndo(combinedGO, "Create Combined Maze");
        combinedGO.transform.SetParent(mazeRoot.transform, false);

        var mfCombined = combinedGO.AddComponent<MeshFilter>();
        var mrCombined = combinedGO.AddComponent<MeshRenderer>();

        // build combine instances
        var combineInstances = new List<CombineInstance>();
        Material firstMat = null;
        for (int i = 0; i < meshFilters.Count; i++)
        {
            var mf = meshFilters[i];
            if (firstMat == null)
            {
                var r = renderers[i];
                if (r != null && r.sharedMaterial != null) firstMat = r.sharedMaterial;
            }

            CombineInstance ci = new CombineInstance();
            // transform must be relative to the combined object's transform
            ci.transform = combinedGO.transform.worldToLocalMatrix * mf.transform.localToWorldMatrix;
            ci.mesh = mf.sharedMesh;
            combineInstances.Add(ci);
        }

        if (firstMat == null)
            firstMat = wallMaterial != null ? wallMaterial : (floorMaterial != null ? floorMaterial : new Material(Shader.Find("Standard")));

        // combine meshes
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = IndexFormat.UInt32;
        combinedMesh.name = "Maze_Combined_Mesh";
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true, false);

        mfCombined.sharedMesh = combinedMesh;
        mrCombined.sharedMaterial = firstMat;

        if (combineAddMeshCollider)
        {
            var mc = combinedGO.AddComponent<MeshCollider>();
            mc.sharedMesh = combinedMesh;
        }

        // optionally destroy originals
        if (combineDestroyOriginals)
        {
            // collect originals to destroy (exclude combinedGO)
            var toDestroy = new List<GameObject>();
            foreach (Transform child in mazeRoot.transform)
            {
                if (child.gameObject == combinedGO) continue;
                toDestroy.Add(child.gameObject);
            }

            foreach (var go in toDestroy)
            {
                Undo.DestroyObjectImmediate(go);
            }
        }

        Selection.activeObject = combinedGO;
        Debug.Log($"Combined {meshFilters.Count} meshes into {combinedGO.name}");
    }

    private struct Edge
    {
        public int a, b;
        public int wallX, wallY;
        public Edge(int a, int b, int wx, int wy) { this.a = a; this.b = b; wallX = wx; wallY = wy; }
    }
}
