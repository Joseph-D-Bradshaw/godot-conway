using Godot;
using System;
using System.Collections.Generic;

public enum CellState
{
  DEAD,
  ALIVE
}

public class Cell
{
  private CellState _state;
  private CellState _nextState;
  private int _age = 0;

  readonly public Vector2 GridCoords;

  public CellState State
  {
    get { return _state; }
    private set { _state = value; }
  }

  public CellState NextState
  {
    get { return _nextState; }
    private set { _nextState = value; }
  }

  public int Age
  {
    get { return _age; }
    private set { _age = value; }
  }

  public Sprite2D Sprite;

  public Cell(Vector2 gridCoords, Sprite2D instance)
  {
    GridCoords = gridCoords;
    State = CellState.DEAD;
    NextState = CellState.DEAD;
    Sprite = instance;
    Age = 0;
  }

  public Cell ApplyState()
  {
    State = NextState;
    return this;
  }

  public Cell NextAlive()
  {
    _nextState = CellState.ALIVE;
    return this;
  }

  public Cell Alive()
  {
    _state = CellState.ALIVE;
    return this;
  }

  public Cell NextDead()
  {
    _nextState = CellState.DEAD;
    return this;
  }

  public Cell Dead()
  {
    _state = CellState.DEAD;
    return this;
  }

  public Cell AgeCell()
  {
    _age += 1;
    return this;
  }

  public Cell BabyCell()
  {
    _age += 0;
    return this;
  }

  public void Draw()
  {
    switch (State)
    {
      case CellState.ALIVE:
        Sprite.SelfModulate = Colors.Green;
        Sprite.SelfModulate = Sprite.SelfModulate.Lightened(Age * 0.1f);
        break;
      case CellState.DEAD:
        Sprite.SelfModulate = Colors.Black;
        BabyCell();
        break;
    }
  }
}

public partial class GridManager : Node2D
{
  private PackedScene _cellScene;
  private float _cellSize;

  [ExportGroup("Dev Tools")]
  [Export]
  private bool DEBUG = false;

  private Cell[,] grid;

  readonly public int[] DeltaRow = new int[] { -1, -1, -1, 0, 0, 1, 1, 1 };
  readonly public int[] DeltaCol = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };

  #region Dev Methods
  private void Log(string x)
  {
    if (DEBUG)
    {
      GD.Print(x);
    }
  }

  private void Block(int x, int y)
  {
    grid[x, y].Alive().Draw();
    grid[x, y + 1].Alive().Draw();
    grid[x + 1, y].Alive().Draw();
    grid[x + 1, y + 1].Alive().Draw();
  }

  private void Beehive(int x, int y)
  {
    grid[x, y].Alive().Draw();
    grid[x + 1, y].Alive().Draw();
    grid[x - 1, y + 1].Alive().Draw();
    grid[x + 2, y + 1].Alive().Draw();
    grid[x, y + 2].Alive().Draw();
    grid[x + 1, y + 2].Alive().Draw();
  }

  private void Blinker(int x, int y)
  {
    grid[x, y].Alive().Draw();
    grid[x, y + 1].Alive().Draw();
    grid[x, y + 2].Alive().Draw();
  }

  private void RPentomino(int x, int y)
  {
    grid[x + 1, y].Alive().Draw();
    grid[x + 2, y].Alive().Draw();
    grid[x, y + 1].Alive().Draw();
    grid[x + 1, y + 1].Alive().Draw();
    grid[x + 1, y + 2].Alive().Draw();
  }

  private void Diehard(int x, int y)
  {
    grid[x + 6, y].Alive().Draw();
    grid[x, y + 1].Alive().Draw();
    grid[x + 1, y + 1].Alive().Draw();
    grid[x + 1, y + 2].Alive().Draw();
    grid[x + 5, y + 2].Alive().Draw();
    grid[x + 6, y + 2].Alive().Draw();
    grid[x + 7, y + 2].Alive().Draw();
  }

  private void Acorn(int x, int y)
  {
    grid[x + 1, y].Alive().Draw();
    grid[x + 3, y + 1].Alive().Draw();
    grid[x, y + 2].Alive().Draw();
    grid[x + 1, y + 2].Alive().Draw();
    grid[x + 4, y + 2].Alive().Draw();
    grid[x + 5, y + 2].Alive().Draw();
    grid[x + 6, y + 2].Alive().Draw();
  }
  #endregion

  private void ApplyRules(Cell cell)
  // Apply rules, this function must set the NextState of the current cell based on the current State
  {
    var neighbours = new List<(int, int)>();
    var aliveCount = 0;

    for (int i = 0; i < DeltaRow.Length; i++)
    {
      var row = (int)cell.GridCoords.X;
      var col = (int)cell.GridCoords.Y;
      int nextRow = row + DeltaRow[i];
      int nextCol = col + DeltaCol[i];

      // Is inside the bounds
      if (nextRow >= 0 && nextRow < grid.GetLength(0) && nextCol >= 0 && nextCol < grid.GetLength(1))
      {
        neighbours.Add((nextRow, nextCol));
        var neighbourCell = grid[nextRow, nextCol];
        aliveCount += neighbourCell.State == CellState.ALIVE ? 1 : 0;
      }

    }

    // Decide the cells fate
    if (cell.State == CellState.ALIVE)
    {
      // Underpopulation and overpopulation
      if (aliveCount < 2 || aliveCount > 3)
      {
        cell.NextDead();
      }
      // Healthy number of neighbours
      else if (aliveCount == 2 || aliveCount == 3)
      {
        cell.NextAlive();
        cell.AgeCell();
      }
    }
    else
    {
      // Reproducing
      if (aliveCount == 3)
      {
        cell.NextAlive();
      }
      else
      {
        cell.NextDead();
      }
    }
  }

  public override void _Ready()
  {
    _cellScene = GD.Load<PackedScene>("res://colored_cell.tscn");
    var viewportSize = GetViewportRect().Size;
    // Relies on window size being divisble by 25
    _cellSize = viewportSize.X / 100;
    var rows = (int)Mathf.Floor(viewportSize.Y / _cellSize);
    var columns = (int)Mathf.Floor(viewportSize.X / _cellSize);

    Log($"cellSize: {_cellSize}");
    Log($"rows: {rows}");
    Log($"columns: {columns}");

    grid = new Cell[rows, columns];

    for (int x = 0; x < columns; x++)
    {
      for (int y = 0; y < rows; y++)
      {
        var instance = _cellScene.Instantiate<Sprite2D>();

        instance.GlobalPosition = new Vector2(x * _cellSize, y * _cellSize);
        instance.ApplyScale(new Vector2(_cellSize, _cellSize));

        AddChild(instance);
        var cell = new Cell(new Vector2(x, y), instance);
        grid[x, y] = cell;

        var clickable = instance.GetNode<Area2D>("Area2D");
        clickable.MouseEntered += () =>
        {
          if (Input.IsMouseButtonPressed(MouseButton.Left) && cell.State == CellState.DEAD)
          {
            cell.Alive().Draw();
            Log(cell.State.ToString());
          }
          else if (Input.IsMouseButtonPressed(MouseButton.Right) && cell.State == CellState.ALIVE)
          {
            cell.Dead().Draw();
            Log(cell.State.ToString());
          }
        };
      }
    }

    RPentomino(12, 20);
    RPentomino(22, 12);
    RPentomino(38, 20);
    Diehard(80, 80);
    Acorn(40, 40);
    Blinker(5, 10);
    Block(2, 2);
    Beehive(12, 12);
  }

  public void StartSimulation()
  {
    var timer = GetNode<Timer>("Timer");
    var startSimButton = GetNode<Button>("UIContainer/VBoxContainer/HBoxContainer/StartSimulation");
    var instructionsLabel = GetNode<Label>("UIContainer/VBoxContainer/HBoxContainer/Instructions");
    startSimButton.Visible = false;
    instructionsLabel.Visible = false;
    timer.Start();
  }

  private void LoopAllAndApply(Action<Cell> process)
  {
    for (int x = 0; x < grid.GetLength(0); x++)
    {
      for (int y = 0; y < grid.GetLength(1); y++)
      {
        process(grid[x, y]);
      }
    }
  }

  public void _Tick()
  {
    Action<Cell> applyRules = (cell) =>
    {
      ApplyRules(cell);
    };

    Action<Cell> applyStateAndDraw = (cell) =>
    {
      cell.ApplyState().Draw();
    };

    LoopAllAndApply(applyRules);
    LoopAllAndApply(applyStateAndDraw);
  }
}
