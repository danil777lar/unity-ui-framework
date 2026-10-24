using UnityEngine;
using UnityEngine.UI;

namespace Larje.Core.UI
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public sealed class GridLayoutFitter : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private bool fitEveryFrame;
        [SerializeField] private Vector2Int gridSize = new Vector2Int(3, 3);

        [Header("Minimum")]
        [SerializeField] private Vector2 minCellSize;
        [SerializeField] private Vector2 minSpacing;
        [SerializeField] private RectOffset minPadding = new RectOffset();

        private Vector2 _baseCellSize;
        private Vector2 _baseSpacing;
        private RectOffset _basePadding;
        private GridLayoutGroup _grid;
        private RectTransform _parent;

        private void Start()
        {
            _parent = (RectTransform)transform.parent;
            _grid = GetComponent<GridLayoutGroup>();
            _baseCellSize = _grid.cellSize;
            _baseSpacing = _grid.spacing;
            _basePadding = Copy(_grid.padding);
            Fit();
        }

        private void Update()
        {
            if (fitEveryFrame)
            {
                Fit();
            }
        }

        private void Fit()
        {
            FitPadding();
            FitSpacing();
            FitCellSize();
        }

        private void FitPadding()
        {
            Vector2 overflow = GetSize() - _parent.rect.size;
            RectOffset padding = Copy(_grid.padding);
            padding.left = Shrink(padding.left, padding.left + padding.right, overflow.x, minPadding.left, _basePadding.left);
            padding.right = Shrink(padding.right, padding.left + padding.right, overflow.x, minPadding.right, _basePadding.right);
            padding.top = Shrink(padding.top, padding.top + padding.bottom, overflow.y, minPadding.top, _basePadding.top);
            padding.bottom = Shrink(padding.bottom, padding.top + padding.bottom, overflow.y, minPadding.bottom, _basePadding.bottom);
            _grid.padding = padding;
        }

        private void FitSpacing()
        {
            Vector2 overflow = GetSize() - _parent.rect.size;
            Vector2 spacing = _grid.spacing;
            if (gridSize.x > 1)
            {
                spacing.x = Mathf.Clamp(spacing.x - overflow.x / (gridSize.x - 1), minSpacing.x, _baseSpacing.x);
            }

            if (gridSize.y > 1)
            {
                spacing.y = Mathf.Clamp(spacing.y - overflow.y / (gridSize.y - 1), minSpacing.y, _baseSpacing.y);
            }

            _grid.spacing = spacing;
        }

        private void FitCellSize()
        {
            Vector2 overflow = GetSize() - _parent.rect.size;
            Vector2 cell = _grid.cellSize;
            cell.x = Mathf.Clamp(cell.x - overflow.x / Mathf.Max(1, gridSize.x), minCellSize.x, _baseCellSize.x);
            cell.y = Mathf.Clamp(cell.y - overflow.y / Mathf.Max(1, gridSize.y), minCellSize.y, _baseCellSize.y);
            _grid.cellSize = cell;
        }

        private int Shrink(int current, int total, float overflow, int min, int max)
        {
            if (total <= 0)
            {
                return Mathf.Clamp(current, min, max);
            }

            return Mathf.Clamp((int)(current - overflow * current / total), min, max);
        }

        private Vector2 GetSize()
        {
            return new Vector2(
                gridSize.x * _grid.cellSize.x + Mathf.Max(0, gridSize.x - 1) * _grid.spacing.x + _grid.padding.horizontal,
                gridSize.y * _grid.cellSize.y + Mathf.Max(0, gridSize.y - 1) * _grid.spacing.y + _grid.padding.vertical);
        }

        private RectOffset Copy(RectOffset source)
        {
            return new RectOffset(source.left, source.right, source.top, source.bottom);
        }
    }
}
