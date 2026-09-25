using System;
using System.Collections.Generic;

namespace Matrix
{
    /// <summary>
    /// Default in-memory <see cref="IMatrix{T}"/>. Responsibility: store cells in a dense grid and implement reshape/query.
    /// Collaborators: none (neighbourhood passed into neighbour queries). Lifetime: owned by caller; not thread-safe.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public sealed class Matrix<T> : IMatrix<T>
    {
        private readonly T[] _cells;
        private readonly int _width;
        private readonly int _height;

        /// <inheritdoc />
        public int Width =>
            _width;

        /// <inheritdoc />
        public int Height =>
            _height;

        /// <summary>
        /// Creates a matrix filled with <paramref name="fillValue"/>.
        /// </summary>
        /// <param name="width">Column count; must be &gt; 0.</param>
        /// <param name="height">Row count; must be &gt; 0.</param>
        /// <param name="fillValue">Initial value for every cell.</param>
        /// <exception cref="ArgumentOutOfRangeException">Width or height is &lt;= 0.</exception>
        public Matrix(int width, int height, T fillValue)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than 0.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than 0.");

            _width = width;
            _height = height;
            _cells = new T[width * height];
            for (int i = 0; i < _cells.Length; i++)
                _cells[i] = fillValue;
        }

        /// <summary>
        /// Creates a matrix from row-major cells (length must equal width×height).
        /// </summary>
        /// <param name="width">Column count; must be &gt; 0.</param>
        /// <param name="height">Row count; must be &gt; 0.</param>
        /// <param name="rowMajorCells">Source data; copied. Must not be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rowMajorCells"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Width/height invalid or length mismatch.</exception>
        public Matrix(int width, int height, T[] rowMajorCells)
        {
            if (rowMajorCells == null)
                throw new ArgumentNullException(nameof(rowMajorCells));
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than 0.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than 0.");
            if (rowMajorCells.Length != width * height)
                throw new ArgumentOutOfRangeException(
                    nameof(rowMajorCells),
                    rowMajorCells.Length,
                    $"Length must equal width×height ({width * height}).");

            _width = width;
            _height = height;
            _cells = new T[rowMajorCells.Length];
            Array.Copy(rowMajorCells, _cells, rowMajorCells.Length);
        }

        /// <inheritdoc />
        public bool InBounds(MatrixIndex index) =>
            index.X >= 0 && index.Y >= 0 && index.X < _width && index.Y < _height;

        /// <inheritdoc />
        public T Get(MatrixIndex index)
        {
            if (!InBounds(index))
                throw new MatrixOutOfBoundsException(index);
            return _cells[ToFlat(index)];
        }

        /// <inheritdoc />
        public bool TryGet(MatrixIndex index, out T value)
        {
            value = default;
            if (!InBounds(index))
                return false;

            value = _cells[ToFlat(index)];
            return true;
        }

        /// <inheritdoc />
        public void Set(MatrixIndex index, T value)
        {
            if (!InBounds(index))
                throw new MatrixOutOfBoundsException(index);
            _cells[ToFlat(index)] = value;
        }

        /// <inheritdoc />
        public bool TrySet(MatrixIndex index, T value)
        {
            if (!InBounds(index))
                return false;
            _cells[ToFlat(index)] = value;
            return true;
        }

        /// <inheritdoc />
        public void Fill(T value)
        {
            for (int i = 0; i < _cells.Length; i++)
                _cells[i] = value;
        }

        /// <inheritdoc />
        public void FillRegion(MatrixRegion region, T value)
        {
            EnsureRegionFits(region);
            int endX = region.X + region.Width;
            int endY = region.Y + region.Height;
            for (int y = region.Y; y < endY; y++)
            {
                int rowOffset = y * _width;
                for (int x = region.X; x < endX; x++)
                    _cells[rowOffset + x] = value;
            }
        }

        /// <inheritdoc />
        public IMatrix<T> Clone()
        {
            T[] copy = new T[_cells.Length];
            Array.Copy(_cells, copy, _cells.Length);
            return new Matrix<T>(_width, _height, copy);
        }

        /// <inheritdoc />
        public IReadOnlyList<T> GetRow(int y)
        {
            if (y < 0 || y >= _height)
                throw new MatrixOutOfBoundsException(new MatrixIndex(0, y));

            T[] row = new T[_width];
            int rowOffset = y * _width;
            for (int x = 0; x < _width; x++)
                row[x] = _cells[rowOffset + x];
            return row;
        }

        /// <inheritdoc />
        public IReadOnlyList<T> GetColumn(int x)
        {
            if (x < 0 || x >= _width)
                throw new MatrixOutOfBoundsException(new MatrixIndex(x, 0));

            T[] column = new T[_height];
            for (int y = 0; y < _height; y++)
                column[y] = _cells[y * _width + x];
            return column;
        }

        /// <inheritdoc />
        public IReadOnlyList<T> GetRegion(MatrixRegion region)
        {
            EnsureRegionFits(region);
            T[] result = new T[region.Width * region.Height];
            int write = 0;
            int endX = region.X + region.Width;
            int endY = region.Y + region.Height;
            for (int y = region.Y; y < endY; y++)
            {
                int rowOffset = y * _width;
                for (int x = region.X; x < endX; x++)
                {
                    result[write] = _cells[rowOffset + x];
                    write++;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public IReadOnlyList<T> GetNeighbours(MatrixIndex index, INeighbourhood neighbourhood)
        {
            if (neighbourhood == null)
                throw new ArgumentNullException(nameof(neighbourhood));
            if (!InBounds(index))
                throw new MatrixOutOfBoundsException(index);

            IReadOnlyList<MatrixIndex> neighbourIndices = neighbourhood.GetNeighbours(index, _width, _height);
            T[] values = new T[neighbourIndices.Count];
            for (int i = 0; i < neighbourIndices.Count; i++)
                values[i] = _cells[ToFlat(neighbourIndices[i])];
            return values;
        }

        /// <inheritdoc />
        public bool Find(T value, out MatrixIndex index)
        {
            index = default;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int y = 0; y < _height; y++)
            {
                int rowOffset = y * _width;
                for (int x = 0; x < _width; x++)
                {
                    if (!comparer.Equals(_cells[rowOffset + x], value))
                        continue;
                    index = new MatrixIndex(x, y);
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public IReadOnlyList<MatrixIndex> FindAll(T value)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            List<MatrixIndex> matches = new List<MatrixIndex>();
            for (int y = 0; y < _height; y++)
            {
                int rowOffset = y * _width;
                for (int x = 0; x < _width; x++)
                {
                    if (!comparer.Equals(_cells[rowOffset + x], value))
                        continue;
                    matches.Add(new MatrixIndex(x, y));
                }
            }

            return matches;
        }

        /// <inheritdoc />
        public IReadOnlyList<MatrixIndex> FindAll(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            List<MatrixIndex> matches = new List<MatrixIndex>();
            for (int y = 0; y < _height; y++)
            {
                int rowOffset = y * _width;
                for (int x = 0; x < _width; x++)
                {
                    if (!predicate(_cells[rowOffset + x]))
                        continue;
                    matches.Add(new MatrixIndex(x, y));
                }
            }

            return matches;
        }

        /// <inheritdoc />
        public IEnumerable<MatrixCell<T>> Enumerate()
        {
            for (int y = 0; y < _height; y++)
            {
                int rowOffset = y * _width;
                for (int x = 0; x < _width; x++)
                    yield return new MatrixCell<T>(new MatrixIndex(x, y), _cells[rowOffset + x]);
            }
        }

        /// <inheritdoc />
        public int Count(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            int count = 0;
            for (int i = 0; i < _cells.Length; i++)
                if (predicate(_cells[i]))
                    count++;

            return count;
        }

        /// <inheritdoc />
        public bool Any(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < _cells.Length; i++)
                if (predicate(_cells[i]))
                    return true;

            return false;
        }

        /// <inheritdoc />
        public bool All(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < _cells.Length; i++)
                if (!predicate(_cells[i]))
                    return false;

            return true;
        }

        /// <inheritdoc />
        public IMatrix<T> Crop(MatrixRegion region)
        {
            EnsureRegionFits(region);
            T[] cropped = new T[region.Width * region.Height];
            int write = 0;
            int endX = region.X + region.Width;
            int endY = region.Y + region.Height;
            for (int y = region.Y; y < endY; y++)
            {
                int rowOffset = y * _width;
                for (int x = region.X; x < endX; x++)
                {
                    cropped[write] = _cells[rowOffset + x];
                    write++;
                }
            }

            return new Matrix<T>(region.Width, region.Height, cropped);
        }

        /// <inheritdoc />
        public IMatrix<T> Expand(ExpandPadding padding, T fillValue)
        {
            if (padding.Left < 0)
                throw new ArgumentOutOfRangeException(nameof(padding), padding.Left, "Left padding must be >= 0.");
            if (padding.Right < 0)
                throw new ArgumentOutOfRangeException(nameof(padding), padding.Right, "Right padding must be >= 0.");
            if (padding.Top < 0)
                throw new ArgumentOutOfRangeException(nameof(padding), padding.Top, "Top padding must be >= 0.");
            if (padding.Bottom < 0)
                throw new ArgumentOutOfRangeException(nameof(padding), padding.Bottom, "Bottom padding must be >= 0.");

            int newWidth = _width + padding.Left + padding.Right;
            int newHeight = _height + padding.Top + padding.Bottom;
            return ExpandTo(newWidth, newHeight, new MatrixIndex(padding.Left, padding.Top), fillValue);
        }

        /// <inheritdoc />
        public IMatrix<T> ExpandTo(int width, int height, MatrixIndex contentOrigin, T fillValue)
        {
            if (width < _width)
                throw new ArgumentOutOfRangeException(nameof(width), width, "Target width must be >= source Width.");
            if (height < _height)
                throw new ArgumentOutOfRangeException(nameof(height), height, "Target height must be >= source Height.");
            if (contentOrigin.X < 0)
                throw new ArgumentOutOfRangeException(nameof(contentOrigin), contentOrigin.X, "contentOrigin.X must be >= 0.");
            if (contentOrigin.Y < 0)
                throw new ArgumentOutOfRangeException(nameof(contentOrigin), contentOrigin.Y, "contentOrigin.Y must be >= 0.");
            if (contentOrigin.X + _width > width)
                throw new ArgumentOutOfRangeException(nameof(contentOrigin), "Source does not fit horizontally at contentOrigin.");
            if (contentOrigin.Y + _height > height)
                throw new ArgumentOutOfRangeException(nameof(contentOrigin), "Source does not fit vertically at contentOrigin.");

            T[] result = new T[width * height];
            for (int i = 0; i < result.Length; i++)
                result[i] = fillValue;

            for (int y = 0; y < _height; y++)
            {
                int sourceRow = y * _width;
                int destRow = (y + contentOrigin.Y) * width + contentOrigin.X;
                for (int x = 0; x < _width; x++)
                    result[destRow + x] = _cells[sourceRow + x];
            }

            return new Matrix<T>(width, height, result);
        }

        private int ToFlat(MatrixIndex index) =>
            index.Y * _width + index.X;

        private void EnsureRegionFits(MatrixRegion region)
        {
            if (region.Width <= 0 || region.Height <= 0)
                throw new InvalidMatrixRegionException($"Region size must be positive (got {region.Width}×{region.Height}).");
            if (region.X < 0 || region.Y < 0
                || region.X + region.Width > _width
                || region.Y + region.Height > _height)
                throw new InvalidMatrixRegionException(
                    $"Region ({region.X}, {region.Y}, {region.Width}×{region.Height}) does not fit inside {_width}×{_height}.");
        }
    }
}
