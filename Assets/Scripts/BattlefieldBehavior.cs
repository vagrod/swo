using UnityEngine;
using SeaWarsOnline.Core;
using System;

public class BattlefieldBehavior 
{

    public class CellClickedEventArgs {
        public Point Location => Cell.Location;
        public FieldCellBase Cell { get; set; }
        public RectTransform CellTransform { get; set; }

        public CellClickedEventArgs(FieldCellBase cell, RectTransform cellTransform)
        {
            CellTransform = cellTransform;
            Cell = cell;
        }
    }

    private GameController _gc;
    private Vector2 _fieldPosition;
    private Vector2 _fieldSize;
    private string _parentName;

    public Action<CellClickedEventArgs> OnCellClicked {get; set;}
    public GameField Field { get; private set; }
    public GridField View { get; private set; }
    public bool IsArrangementUi{get; set;}
    public int CellsOverlap {get; set;}
    public bool ShowHeader {get; set;}

    public BattlefieldBehavior(GameController gc)
    {
        _gc = gc;
    }

    public GameObject Initialize(GameField field, string parentName, Vector2 fieldPosition, Vector2 fieldSize){
        //UnityEngine.Debug.Log($"BattlefieldBehavior is initializing with field {field.FieldId}");

        Field = field;

        _fieldSize = fieldSize;
        _fieldPosition = fieldPosition;
        _parentName = parentName;

        var go = CreateField();

        Field.CellStateChanged += OnCellStateChanged;

        return go;
    }

    private void OnCellStateChanged(object sender, CellStateEventArgs e)
    {
       // UnityEngine.Debug.Log($"BattlefieldBehavior linked field {_field.FieldId} cell state changed. Processing new state {e.NewState} at [{e.CellLocation.X};{e.CellLocation.Y}]: mirroring to view.");

        var cell = Field.CellAtPoint(e.CellLocation);

       // UnityEngine.Debug.Log("BattlefieldBehavior is calling view.SetCellState");

        var usedResource = View.SetCellState(e.CellLocation.X, e.CellLocation.Y, cell.State);

        cell.UsedImageResource = usedResource;
    }

    private GameObject CreateField(){
        var go = new GameObject{
            name = $"GridField-{Field.FieldId.ToString()}"
        };

        View = go.AddComponent<GridField>();

        View.ShowHeader = ShowHeader;
        View.CellsOverlap = CellsOverlap;
        View.IsArrangementUi = IsArrangementUi;
        View.RowsCount = Field.FieldSize;
        View.ColsCount = Field.FieldSize;
        View.ParentName =  _parentName;
        View.GridSize = new Vector2(_fieldSize.x, _fieldSize.y);
        View.AfterInitialization = () => FillCells();
        View.OnCellClicked = p => OnCellClicked?.Invoke(new CellClickedEventArgs(Field.CellAtPoint(p), View.CellTransformAtPoint(p)));

        return View.Grid;
    }

    private void FillCells(){
        View.SetPosition(_fieldPosition);

        Field.Iterate(cell => {
            var usedResource = View.SetCellState(cell.Location.X, cell.Location.Y, cell.State);
            
            cell.UsedImageResource = usedResource;
        });
    }

    public void Destroy(){
        Field?.Clear();
        View?.Destroy();
    }

}
