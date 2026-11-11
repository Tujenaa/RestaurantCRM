using System;
using System.Collections.Generic;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>Danh sách lựa chọn "Tìm theo" của ô tìm kiếm (PageKit dùng để vẽ nút + menu).</summary>
    public interface ISearchScopes
    {
        IList<string> Names { get; }
        IList<string> ShortNames { get; }
        IList<string> Placeholders { get; }
        int SelectedIndex { get; set; }
        event EventHandler Changed;
    }

    /// <summary>
    /// Lựa chọn "Tìm theo" cho một trang: mỗi lựa chọn là một nhóm trường của đối tượng T.
    /// Lựa chọn thêm vào đầu tiên là mặc định; lựa chọn đang dùng được nhớ trong phiên làm việc (theo key trang).
    /// </summary>
    public class SearchScopes<T> : ISearchScopes
    {
        private static readonly Dictionary<string, int> Remembered = new Dictionary<string, int>();

        private readonly string _key;
        private readonly List<string> _names = new List<string>();
        private readonly List<string> _shortNames = new List<string>();
        private readonly List<string> _placeholders = new List<string>();
        private readonly List<Func<T, string[]>> _fields = new List<Func<T, string[]>>();
        private int _selected;

        public SearchScopes(string key) { _key = key ?? ""; }

        public IList<string> Names { get { return _names; } }
        public IList<string> ShortNames { get { return _shortNames; } }
        public IList<string> Placeholders { get { return _placeholders; } }
        public event EventHandler Changed;

        public int SelectedIndex
        {
            get { return _selected; }
            set
            {
                if (value < 0 || value >= _names.Count || value == _selected) return;
                _selected = value;
                Remembered[_key] = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Thêm một lựa chọn: tên đầy đủ (trong menu), tên ngắn (trên nút), chữ gợi ý, các trường cần tìm.</summary>
        public SearchScopes<T> Add(string name, string shortName, string placeholder, Func<T, string[]> fields)
        {
            _names.Add(name);
            _shortNames.Add(shortName);
            _placeholders.Add(placeholder);
            _fields.Add(fields);
            RestoreRemembered();
            return this;
        }

        /// <summary>
        /// Lựa chọn "Tất cả": gộp trường của mọi lựa chọn khác (kể cả lựa chọn thêm sau),
        /// cộng thêm các trường chỉ tìm khi chọn "Tất cả" (extraFields, có thể null).
        /// </summary>
        public SearchScopes<T> AddAll(string placeholder, Func<T, string[]> extraFields = null)
        {
            int self = _fields.Count;
            return Add("Tất cả", "Tất cả", placeholder, item =>
            {
                var all = new List<string>();
                for (int i = 0; i < _fields.Count; i++)
                {
                    if (i == self) continue;
                    var v = _fields[i](item);
                    if (v != null) all.AddRange(v);
                }
                if (extraFields != null) { var x = extraFields(item); if (x != null) all.AddRange(x); }
                return all.ToArray();
            });
        }

        public bool Matches(string query, T item)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            if (_fields.Count == 0 || item == null) return false;
            return UiStyler.MatchesSearch(query, _fields[_selected](item));
        }

        private void RestoreRemembered()
        {
            int idx;
            if (Remembered.TryGetValue(_key, out idx) && idx >= 0 && idx < _names.Count) _selected = idx;
        }
    }
}
