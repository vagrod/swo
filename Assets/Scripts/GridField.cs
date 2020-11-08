using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SeaWarsOnline.Core;
using SeaWarsOnline.Core.Localization;

public class GridField : SwoScriptBase
{
    public int RowsCount {get; set;}
    public int ColsCount {get; set;}
    public Vector2 GridSize {get; set;}
    public bool IsArrangementUi {get; set;}
    public bool ShowHeader{get;set;}
    public int CellsOverlap { get; set;}
    public System.Action AfterInitialization {get; set;}
    public System.Action<Point> OnCellClicked {get; set;}
    public string ParentName {get; set;}
    public GameObject Grid => gameObject;

    private readonly Dictionary<int, GameObject> _cells = new Dictionary<int, GameObject>();

    private static GameObject _headerLabelTemplate;
    private static GameObject _highlightedCellOverlayTemplate;
    private static System.Random _random;

    private List<Sprite> _spriteEmptyCells;
    private List<Sprite> _spriteShipCells;
    private List<Sprite> _spriteMissedCells;
    private List<Sprite> _spriteDamagedCells;
    private List<Sprite> _spriteForeignCells;
    private List<Sprite> _spriteMineCells;
    private List<Sprite> _spriteMineExplodedCells;
    private List<Sprite> _spriteAvailableCells;
    private List<Sprite> _spriteForeignMineCells;
    private List<Sprite> _rowHeaderCells;
    private List<Sprite> _columnHeaderCells;

    private Vector2 _cellSize;
    private bool _isDestroyed;

    protected override bool IsPaperDistortionEffectAllowed => false;

    protected override void OnStart () {
        InitResources();
        InitCells();
	}

    private void InitResources(){
        if(_random == null)
            _random = new System.Random();

        var postfix = IsArrangementUi ? ".arrangement" : string.Empty;

        _spriteEmptyCells = LoadMultipleCells($"Skins/{_gc.SkinName}/Cells/CellEmpty{postfix}");
        
        _spriteShipCells = LoadMultipleCells($"Skins/{_gc.SkinName}/Cells/CellShip{postfix}");
        _spriteDamagedCells = LoadMultipleCells($"Skins/{_gc.SkinName}/Cells/CellDamaged{postfix}");
        _spriteMissedCells = LoadMultipleCells($"Skins/{_gc.SkinName}/Cells/CellMissed{postfix}");
        _spriteMineCells = LoadMultipleCells($"Skins/{_gc.SkinName}/Cells/CellMine{postfix}");

        _spriteForeignCells = LoadMultipleCells($"Skins/{_gc.SkinName}/Cells/CellForeignShip{postfix}");
        _spriteMineExplodedCells = LoadMultipleCells($"Skins/{_gc.SkinName}/Cells/CellMineExploded{postfix}");
        _spriteAvailableCells = LoadMultipleCells($"Skins/{_gc.SkinName}/Cells/CellAvailable{postfix}");
        _spriteForeignMineCells = LoadMultipleCells($"Skins/{_gc.SkinName}/Cells/CellForeignMine{postfix}");

        _columnHeaderCells = LoadMultipleCells($"Skins/{_gc.SkinName}/Cells/HeaderCell.Column{postfix}");
        _rowHeaderCells = LoadMultipleCells($"Skins/{_gc.SkinName}/Cells/HeaderCell.Row{postfix}");

        if (_headerLabelTemplate == null)
            _headerLabelTemplate = FindInScene("HeaderLabelTemplate");

        if (_headerLabelTemplate != null)
            _headerLabelTemplate.SetActive(false);

        if (_highlightedCellOverlayTemplate == null)
            _highlightedCellOverlayTemplate = FindInScene("HighlightedCellOverlayTemplate");

        if (_highlightedCellOverlayTemplate != null)
            _highlightedCellOverlayTemplate.SetActive(false);
    }

    private List<Sprite> LoadMultipleCells(string nameBase){
        var res = new List<Sprite>();
        var sp = Resources.Load<Sprite>($"{nameBase}.0");

        if (sp == null) // No multiple resources
            return new List<Sprite>(new[]{Resources.Load<Sprite>(nameBase)});

        var n = 0;

        while(sp != null){
            res.Add(sp);
            n++;

            sp = Resources.Load<Sprite>($"{nameBase}.{n}");
        }

        return res;
    }

    private Sprite GetRandomResource(List<Sprite> collection){
        if (collection.Count == 1)
            return collection.Single();
        
        var i = _random.Next(0, collection.Count - 1);

        return collection[i];
    }

    public void SetPosition(Vector2 p){
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(p.x, -p.y);
    }

    void InitCells() {
        _cells.Clear();

        gameObject.AddComponent<Canvas>();
        gameObject.AddComponent<GraphicRaycaster>();

        var mainTransform = gameObject.GetComponent<RectTransform>();

        transform.SetParent(FindInScene(ParentName).transform);

        mainTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GridSize.x);
        mainTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GridSize.y);

        AnchorTopLeft(mainTransform);

        var templateCell = new GameObject();
        var cellSize = new Vector2(GridSize.x / (ShowHeader ? ColsCount + 1 : ColsCount) + CellsOverlap, GridSize.y / (ShowHeader ? RowsCount + 1 : RowsCount) + CellsOverlap);

        _cellSize = new Vector2(cellSize.x,cellSize.y);

        var img = templateCell.AddComponent<Image>();
        
        img.type = Image.Type.Sliced;
        img.fillCenter = true;
        img.raycastTarget = true;

        var t = templateCell.GetComponent<RectTransform>();

        templateCell.layer = 5; // UI

        AnchorTopLeft(t);

        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize.x);
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize.y);

        var headerOffsetX = ShowHeader ? cellSize.x - CellsOverlap : 0;
        var headerOffsetY = ShowHeader ? cellSize.y - CellsOverlap : 0;

        if (ShowHeader){
            CreateHorizontalHeader(templateCell, cellSize);
            CreateVerticalHeader(templateCell, cellSize);
        }

        //Debug.Log("View adding cells");

        for (int y = 0; y < RowsCount; y++) {
            for (int x = 0; x < ColsCount; x++) {
                var oX = x*CellsOverlap;
                var oY = y*CellsOverlap;

                var newPos = new Vector2(headerOffsetX + x * cellSize.x - oX, GridSize.y - y * cellSize.y - GridSize.y + oY - headerOffsetY);

                var cell = Instantiate(templateCell, newPos, Quaternion.identity);

                cell.name = $"cell-{x}-{y}";

                cell.transform.SetParent(transform);

                cell.GetComponent<RectTransform>().anchoredPosition = newPos;

                var clicker = cell.AddComponent<Button>();

                clicker.interactable = true;

                clicker.onClick.AddListener(delegate{OnCellPressed(cell);});

                _cells.Add(y * RowsCount + x, cell);
            }
        }

        Destroy(templateCell);

        mainTransform.localScale = new Vector3(1,1,1);

        AfterInitialization?.Invoke();
    }

    private void CreateHorizontalHeader(GameObject templateCell, Vector2 cellSize){
        var horHeader = new GameObject();
        horHeader.name = "Header-Horizontal";

        var horTransform = horHeader.AddComponent<RectTransform>();

        AnchorTopLeft(horTransform);

        horTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GridSize.x);
        horTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize.y);

        horTransform.SetParent(gameObject.transform);

        horTransform.position = new Vector3(0,0,0);
        horTransform.anchoredPosition = new Vector2(0,0);

        for (int x = 0; x < ColsCount; x++) {
            var oX = x*CellsOverlap;
            var newPos = new Vector2(cellSize.x - CellsOverlap + x*cellSize.x - oX, 0f);

            var cell = Instantiate(templateCell, newPos, Quaternion.identity);

            cell.name = $"row-{x}";

            cell.transform.SetParent(horTransform);

            cell.GetComponent<RectTransform>().anchoredPosition = newPos;

            cell.GetComponent<Image>().sprite = GetRandomResource(_columnHeaderCells);

            // Label
            var labelGo = Instantiate(_headerLabelTemplate);

            var labelTransform = labelGo.GetComponent<RectTransform>();

            AnchorTopLeft(labelTransform);

            labelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize.x);
            labelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize.y);

            var label = labelGo.GetComponent<Text>();

            label.text = LocalizationManager.Instance.GetString("Field", x.ToString());
            labelGo.name = $"row-{x}-title";

            labelTransform.SetParent(cell.transform);

            labelTransform.position = new Vector3(0,0,0);
            labelTransform.anchoredPosition = new Vector2(0,0);

            labelGo.SetActive(true);
        }
    }

    private void CreateVerticalHeader(GameObject templateCell, Vector2 cellSize){
        var verHeader = new GameObject();
        verHeader.name = $"Header-Vertical";

        var verTransform = verHeader.AddComponent<RectTransform>();

        AnchorTopLeft(verTransform);

        verTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize.x);
        verTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GridSize.y);

        verTransform.SetParent(gameObject.transform);

        verTransform.position = new Vector3(0,0,0);
        verTransform.anchoredPosition = new Vector2(0,0);

        for (int y = 0; y < RowsCount; y++) {
            var oY = y*CellsOverlap;
            var newPos = new Vector2(0f, GridSize.y + CellsOverlap - y * cellSize.y - GridSize.y + oY - cellSize.y);

            var cell = Instantiate(templateCell, newPos, Quaternion.identity);

            cell.name = $"col-{y}";

            cell.transform.SetParent(verTransform);

            cell.GetComponent<RectTransform>().anchoredPosition = newPos;

            cell.GetComponent<Image>().sprite = GetRandomResource(_rowHeaderCells);

            // Label
            var labelGo = Instantiate(_headerLabelTemplate);

            var labelTransform = labelGo.GetComponent<RectTransform>();

            AnchorTopLeft(labelTransform);

            labelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize.x);
            labelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize.y);

            var label = labelGo.GetComponent<Text>();

            label.text = (y+1).ToString();
            labelGo.name = $"col-{y}-title";

            labelTransform.SetParent(cell.transform);

            labelTransform.position = new Vector3(0,0,0);
            labelTransform.anchoredPosition = new Vector2(0,0);

            labelGo.SetActive(true);
        }
    }

    void OnCellPressed(GameObject cell){
        var t = cell.name.Split('-');

        OnCellClicked?.Invoke(new Point(int.Parse(t[1]), int.Parse(t[2])));
    }

    public RectTransform CellTransformAtPoint(Point p){
        return GetAtPoint<RectTransform>(p.X, p.Y);
    }

    public Sprite GetSpriteForState(FieldCellBase.CellStates state){
        switch(state){
            case FieldCellBase.CellStates.Empty:
                return GetRandomResource(_spriteEmptyCells);

            case FieldCellBase.CellStates.ForeignShipPart:
                return GetRandomResource(_spriteForeignCells);

            case FieldCellBase.CellStates.Mine:
                return GetRandomResource(_spriteMineCells);

            case FieldCellBase.CellStates.Miss:
                return GetRandomResource(_spriteMissedCells);

            case FieldCellBase.CellStates.Ship:
                return GetRandomResource(_spriteShipCells);

            case FieldCellBase.CellStates.Damaged:
                return GetRandomResource(_spriteDamagedCells);

            case FieldCellBase.CellStates.MineExploded:
                return GetRandomResource(_spriteMineExplodedCells);

            case FieldCellBase.CellStates.Available:
                return GetRandomResource(_spriteAvailableCells);

            case FieldCellBase.CellStates.ForeignMine:
                return GetRandomResource(_spriteForeignMineCells);

            default:
                return null;
        }
    }

    public string SetCellState(int x, int y, FieldCellBase.CellStates state){
       // UnityEngine.Debug.Log($"GridField SetCellState is called for a new state {state} at [{x};{x}]");

        var cell = GetAtPoint(x, y);

        if (cell == null)
            return null;

        cell.sprite = GetSpriteForState(state);

        // UnityEngine.Debug.Log("SetCellState changed UI");
        return cell.sprite.name;
    }

    public void HighlightAtPoint(int x, int y){
        var cell = GetAtPoint<RectTransform>(x, y);
        var overlay = Instantiate(_highlightedCellOverlayTemplate);
        var t = overlay.GetComponent<RectTransform>();

        t.SetParent(cell);
        t.localScale = new Vector3(1f, 1f, 1f);
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _cellSize.x);
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _cellSize.y);
        t.anchorMin = new Vector2(0.5f, 0.5f);
        t.anchorMax = new Vector2(0.5f, 0.5f);
        t.position = new Vector3(0f, 0f, 0f);
        t.anchoredPosition = new Vector2(0f, 0f);
        t.gameObject.layer = 5; // UI
        t.gameObject.SetActive(true);
    }

    public Image GetAtPoint(int x, int y){
        var im = GetAtPoint<Image>(x,y);

        //UnityEngine.Debug.Log($"Sprite at point is {(im == null ? "NULL" : "not null")}");

        return im;
    }

    public T GetAtPoint<T>(int x, int y) where T:class {
        _cells.TryGetValue(y * RowsCount + x, out var c);
        
        if (c == null)
            return null;

        return c.GetComponent<T>();
    }

    private void AnchorTopLeft(RectTransform t){
        // Top-Left anchor
        t.anchorMin = new Vector2(0, 1);
        t.anchorMax = new Vector2(0, 1);
        t.position = new Vector3(0,0,0);

        // Top-Left corner origin
        t.pivot = new Vector2(0, 1); 

        // Reset the position
        t.position = new Vector3(0,0,0);
        t.anchoredPosition = new Vector2(0,0);
    }

    public void Destroy(){
        if (_isDestroyed)
            return;

        _isDestroyed = true;
        _random = null;
        _headerLabelTemplate = null;
        _highlightedCellOverlayTemplate = null;

        //Debug.Log("View destroy");

        _cells?.Clear();

        if (gameObject != null && gameObject.activeInHierarchy)
            Destroy(gameObject);
    }

}
