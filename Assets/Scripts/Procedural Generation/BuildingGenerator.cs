using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class BuildingGenerator : MonoBehaviour
{
    public int XSize;
    public int YSize;
    public int ZSize;
    public Vector3 Origin;
    public List<StoreyInfo> StoreySpec;
    public List<PuzzlePiece> PuzzlePieces;
    public List<NamedMaterial> Materials;
    public bool MakePrefab;

    private Dictionary<PuzzlePieceFlags, List<GameObject>> _puzzlePieceLookup;
    [SerializeField, HideInInspector] private List<GameObject> _gameObjects;
    private bool _containsFoundation;

#if UNITY_EDITOR
    [SerializeField, HideInInspector] private string PrefabFileName;
#endif

    void OnValidate()
    {
        ClampBuildingSize();
        RebuildMaterialList();
    }

    public void Generate()
    {
#if UNITY_EDITOR
        GameObject parent = this.MakePrefab ? new GameObject() : gameObject;

        if (this.MakePrefab && (string.IsNullOrEmpty(this.PrefabFileName) || !System.IO.File.Exists(this.PrefabFileName)))
        {
            this.PrefabFileName =
                EditorUtility.SaveFilePanelInProject("Save Building Prefab",
                                                     "New Building.prefab",
                                                     "prefab",
                                                     "Please enter a file name for the generated building prefab:");
        }
#else
        GameObject parent = gameObject;
#endif

        Clear();

        gameObject.transform.position = this.Origin;

        RebuildPuzzlePieceLookup();

        _containsFoundation = this.StoreySpec.Any(ss => ss.StoreyType == StoreyType.Foundation);

        int floor = _containsFoundation ? -1 : 0;
        float currentFloorHeight = 0f;

        for (; floor < this.YSize; floor++)
        {
            currentFloorHeight = BuildFloor(floor, currentFloorHeight, parent);
        }

#if UNITY_EDITOR
        if (this.MakePrefab && !string.IsNullOrEmpty(this.PrefabFileName))
        {
            PrefabUtility.CreatePrefab(this.PrefabFileName, parent);
            DestroyImmediate(parent);
        }
#endif
    }

    void ClampBuildingSize()
    {
        if (this.XSize < 1) this.XSize = 1;
        if (this.YSize < 2) this.YSize = 2;
        if (this.ZSize < 1) this.ZSize = 1;
    }

    private void RebuildMaterialList()
    {
        List<NamedMaterial> cachedMaterials;
        if (this.Materials != null)
        {
            cachedMaterials = new List<NamedMaterial>(this.Materials);
            this.Materials.Clear();
        }
        else
        {
            cachedMaterials = new List<NamedMaterial>();
            this.Materials = new List<NamedMaterial>();
        }

        foreach (PuzzlePiece puzzlePiece in this.PuzzlePieces)
        {
            MeshRenderer renderer = puzzlePiece.GameObject.GetComponent<MeshRenderer>();

            foreach (Material material in renderer.sharedMaterials)
            {
                NamedMaterial namedMaterial = new NamedMaterial {Key = material.name, Material = material};

                if (this.Materials.All(mat => mat.Key != namedMaterial.Key)) this.Materials.Add(namedMaterial);
            }
        }

        foreach (NamedMaterial cachedMaterial in cachedMaterials)
        {
            if (this.Materials.Any(mat => mat.Key == cachedMaterial.Key))
            {
                int index = this.Materials.FindIndex(mat => mat.Key == cachedMaterial.Key);

                this.Materials[index] = cachedMaterial;
            }
        }
    }

    private void RebuildPuzzlePieceLookup()
    {
        if (_puzzlePieceLookup != null)
        {
            _puzzlePieceLookup.Clear();
        }
        else
        {
            _puzzlePieceLookup = new Dictionary<PuzzlePieceFlags, List<GameObject>>();
        }

        //_containsFoundation = false;

        //_containsFoundation = this.PuzzlePieces.Any(pp => (pp.Flags & PuzzlePieceFlags.Foundation) != 0);

        for (int i = 0; i < 0x800; i++)
        {
            PuzzlePieceFlags flags = (PuzzlePieceFlags)i;

            var puzzlePieces = this.PuzzlePieces.Where(pp => pp.Flags == flags).Select(pp => pp.GameObject).ToList();

            if (puzzlePieces.Any())
            {
                _puzzlePieceLookup.Add(flags, puzzlePieces);
            }
        }
    }

    public void Clear()
    {
        if (_gameObjects != null)
        {
            foreach (GameObject obj in _gameObjects)
            {
#if UNITY_EDITOR
                DestroyImmediate(obj);
#else
                Destroy(obj);
#endif
            }
            _gameObjects.Clear();
        }
        else
        {
            _gameObjects = new List<GameObject>();
        }
    }

    public float BuildFloor(int floor, float currentFloorHeight, GameObject parent)
    {
        if (floor < -1) throw new ArgumentOutOfRangeException("floor");

        PuzzlePieceFlags globalFlags = 0;
        bool constructFloorPieces = _containsFoundation ? floor == -1 : floor == 0;
        StoreyInfo storeyInfo;

        GameObject floorObj;

        if (floor == -1)
        {
            globalFlags |= PuzzlePieceFlags.Foundation;
            storeyInfo = this.StoreySpec.First(ss => ss.StoreyType == StoreyType.Foundation);
            floorObj = new GameObject("Foundation");
        }
        else if (floor == 0)
        {
            globalFlags |= PuzzlePieceFlags.LobbyPiece;
            storeyInfo = this.StoreySpec.First(ss => ss.StoreyType == StoreyType.Lobby);
            floorObj = new GameObject("Lobby");
        }
        else if (floor == this.YSize - 1)
        {
            globalFlags |= PuzzlePieceFlags.TopFloorPiece;
            storeyInfo = this.StoreySpec.First(ss => ss.StoreyType == StoreyType.TopFloor);
            floorObj = new GameObject(string.Format("{0} Floor", Ordinalise(floor)));
        }
        else
        {
            storeyInfo = this.StoreySpec.First(ss => ss.StoreyType == StoreyType.Standard);
            floorObj = new GameObject(string.Format("{0} Floor", Ordinalise(floor)));
        }

        floorObj.transform.SetParent(parent.transform);

        RegisterGameObject(floorObj);

        for (int xIndex = -1; xIndex <= this.XSize; xIndex++)
        {
            bool isLeftXFacade = xIndex == -1;
            bool isRightXFacade = xIndex == this.XSize;
            bool isXFacade = isLeftXFacade || isRightXFacade;

            for (int zIndex = -1; zIndex <= this.ZSize; zIndex++)
            {
                bool isFrontZFacade = zIndex == -1;
                bool isBackZFacade = zIndex == this.ZSize;
                bool isZFacade = isFrontZFacade || isBackZFacade;

                if (isXFacade || isZFacade)
                {

                    PuzzlePieceFlags facadeFlags = globalFlags | GetFacadeFlags(isXFacade, isZFacade);

                    Quaternion facadeRotation =
                        GetFacadeRotation(isLeftXFacade, isRightXFacade, isFrontZFacade, isBackZFacade);
                    CellSnapping facadeSnapping =
                        GetFacadeSnapping(isLeftXFacade, isRightXFacade, isFrontZFacade, isBackZFacade);
                    Vector3 facadePosition = GetPositionFromIndices(xIndex, currentFloorHeight, zIndex, facadeSnapping, storeyInfo);

                    if ((isFrontZFacade && (storeyInfo.BareWalls & AlternatePieceUsageFlags.Front) != 0)
                        || (isBackZFacade && (storeyInfo.BareWalls & AlternatePieceUsageFlags.Back) != 0)
                        || (isRightXFacade && (storeyInfo.BareWalls & AlternatePieceUsageFlags.Right) != 0)
                        || (isLeftXFacade && (storeyInfo.BareWalls & AlternatePieceUsageFlags.Left) != 0))
                    {
                        facadeFlags |= PuzzlePieceFlags.BarePiece;
                    }

                    InstantiatePuzzlePiece(facadeFlags, facadeRotation, facadePosition, floorObj);
                }
                else
                {
                    if (xIndex > 0 && xIndex < this.XSize && zIndex > 0 && zIndex < this.ZSize)
                    {
                        PuzzlePieceFlags supportFlags = globalFlags | PuzzlePieceFlags.Support;
                        Vector3 supportPosition =
                            GetPositionFromIndices(xIndex,
                                                   currentFloorHeight,
                                                   zIndex,
                                                   CellSnapping.NegativeX | CellSnapping.NegativeZ,
                                                   storeyInfo);
                        InstantiatePuzzlePiece(supportFlags, Quaternion.identity, supportPosition, floorObj);
                    }

                    PuzzlePieceFlags floorFlags = globalFlags | PuzzlePieceFlags.Floor;
                    Vector3 floorPosition = GetPositionFromIndices(xIndex, currentFloorHeight, zIndex, CellSnapping.Centre, storeyInfo);
                    InstantiatePuzzlePiece(floorFlags, Quaternion.identity, floorPosition, floorObj);
                }
            }
        }

        return storeyInfo.GridSize.y + currentFloorHeight;
    }

    private static string Ordinalise(int n)
    {
        const string th = "{0}th";
        const string st = "{0}st";
        const string nd = "{0}nd";
        const string rd = "{0}rd";

        string ordinal;

        if ((n / 10) % 10 == 1)
        {
            ordinal = th;
        }
        else
        {
            switch (n % 10)
            {
                case 1:
                    ordinal = st;
                    break;
                case 2:
                    ordinal = nd;
                    break;
                case 3:
                    ordinal = rd;
                    break;
                default:
                    ordinal = th;
                    break;
            }
        }

        return string.Format(ordinal, n);
    }

    private void InstantiatePuzzlePiece(PuzzlePieceFlags flags, Quaternion rotation, Vector3 position, GameObject parent)
    {
        List<GameObject> puzzlePieces;
        if (_puzzlePieceLookup.TryGetValue(flags, out puzzlePieces))
        {
            rotation = rotation * puzzlePieces[0].transform.rotation;

            GameObject puzzlePiece = GameObject.Instantiate(puzzlePieces[0], position, rotation, parent.transform);

            MeshRenderer renderer = puzzlePiece.GetComponent<MeshRenderer>();

            renderer.materials = FetchMaterials(renderer.sharedMaterials);
            RegisterGameObject(puzzlePiece);
        }
        else
        {
            Debug.LogWarning(string.Format("Puzzle piece not found with the flags \"{0}\".", flags));
        }
    }

    private void RegisterGameObject(GameObject obj)
    {
        _gameObjects.Add(obj);
    }

    private static PuzzlePieceFlags GetFacadeFlags(bool isXFacade, bool isZFacade)
    {
        PuzzlePieceFlags flags = 0;

        if (isXFacade && isZFacade)
        {
            flags |= PuzzlePieceFlags.CornerPiece;
            flags |= PuzzlePieceFlags.Facade;
        }
        else if (isXFacade || isZFacade)
        {
            flags |= PuzzlePieceFlags.Facade;
        }
        else
        {
            flags |= PuzzlePieceFlags.Support;
        }

        return flags;
    }

    private static Quaternion GetFacadeRotation(bool isLeftXFacade, bool isRightXFacade, bool isFrontZFacade, bool isBackZFacade)
    {
        Quaternion rotation = Quaternion.identity;

        float yRotation = 0f;

        if (isFrontZFacade)
        {
            yRotation = isRightXFacade ? 180f : -90f;
        }
        else if (isBackZFacade)
        {
            yRotation = isLeftXFacade ? 0f : 90f;
        }
        else if (isRightXFacade)
        {
            yRotation = 180f;
        }

        if (yRotation != 0f) rotation = Quaternion.Euler(0f, yRotation, 0f);
        return rotation;
    }

    private static CellSnapping GetFacadeSnapping(bool isLeftXFacade, bool isRightXFacade, bool isFrontZFacade, bool isBackZFacade)
    {
        CellSnapping snapping = CellSnapping.Centre;
        if (isBackZFacade) snapping |= CellSnapping.NegativeZ;
        if (isFrontZFacade) snapping |= CellSnapping.PositiveZ;
        if (isRightXFacade) snapping |= CellSnapping.NegativeX;
        if (isLeftXFacade) snapping |= CellSnapping.PositiveX;
        return snapping;
    }

    Vector3 GetPositionFromIndices(int xIndex, float currentHeight, int zIndex, CellSnapping snapping, StoreyInfo storeyInfo)
    {
        float xSnapping = 0f;
        float zSnapping = 0f;

        if ((snapping & CellSnapping.PositiveX) != 0) xSnapping += 0.5f;
        if ((snapping & CellSnapping.NegativeX) != 0) xSnapping += -0.5f;
        if ((snapping & CellSnapping.PositiveZ) != 0) zSnapping += 0.5f;
        if ((snapping & CellSnapping.NegativeZ) != 0) zSnapping += -0.5f;

        float xMidpoint = (this.XSize - 1) / 2f;
        float zMidpoint = (this.ZSize - 1) / 2f;

        float xGridSize = storeyInfo.GridSize.x;
        float zGridSize = storeyInfo.GridSize.z;

        float xDifference = (xIndex + xSnapping) - xMidpoint;
        float zDifference = (zIndex + zSnapping) - zMidpoint;

        float xOffset = xDifference * xGridSize;
        float zOffset = zDifference * zGridSize;

        //return this.Origin + new Vector3(xOffset, currentHeight, zOffset);
        return new Vector3(xOffset, currentHeight, zOffset);
    }

    Material[] FetchMaterials(Material[] sharedMaterials)
    {
        return sharedMaterials.Select(shmat => this.Materials.First(mat => mat.Key == shmat.name).Material).ToArray();
    }

    [Flags]
    private enum CellSnapping
    {
        Centre = 0,
        PositiveX = 1 << 0,
        NegativeX = 1 << 31,
        PositiveZ = 1 << 1,
        NegativeZ = 1 << 30,
    }

    public enum StoreyType
    {
        Foundation = 0,
        Lobby,
        Standard,
        Alternate,
        TopFloor
    }

    [Flags]
    public enum PuzzlePieceFlags
    {
        Greebly = 0x001,
        Facade = 0x002,
        Floor = 0x004,
        Support = 0x008,
        CornerPiece = 0x010,
        LobbyPiece = 0x020,
        TopFloorPiece = 0x040,
        BarePiece = 0x080,
        AlternatePiece = 0x100,
        Entrance = 0x200,
        Foundation = 0x400,
    }

    [Flags]
    public enum AlternatePieceUsageFlags
    {
        Front = 0x01,
        Back = 0x02,
        Right = 0x04,
        Left = 0x08
    }

    [Serializable]
    public class StoreyInfo
    {
        public StoreyType StoreyType;
        public Vector3 GridSize;
        public AlternatePieceUsageFlags BareWalls;
    }

    [Serializable]
    public class PuzzlePiece
    {
        public GameObject GameObject;
        public PuzzlePieceFlags Flags;
    }

    [Serializable]
    public struct NamedMaterial
    {
        [HideInInspector] public string Key;
        public Material Material;
    }
}
