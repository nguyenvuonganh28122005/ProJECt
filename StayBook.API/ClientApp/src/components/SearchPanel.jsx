export default function SearchPanel({ filters, onChange, onSubmit, loading }) {
  return (
    <form className="search-panel" onSubmit={onSubmit}>
      <label className="search-field destination-field">
        <span className="field-icon" aria-hidden="true">⌖</span>
        <span className="field-content">
          <span className="field-label">Bạn muốn đi đâu?</span>
          <input
            name="location"
            value={filters.location}
            onChange={onChange}
            placeholder="Thành phố, địa điểm..."
          />
        </span>
      </label>

      <label className="search-field">
        <span className="field-icon" aria-hidden="true">↗</span>
        <span className="field-content">
          <span className="field-label">Nhận phòng</span>
          <input name="checkIn" type="date" value={filters.checkIn} onChange={onChange} required />
        </span>
      </label>

      <label className="search-field">
        <span className="field-icon" aria-hidden="true">↙</span>
        <span className="field-content">
          <span className="field-label">Trả phòng</span>
          <input name="checkOut" type="date" value={filters.checkOut} onChange={onChange} required />
        </span>
      </label>

      <label className="search-field guests-field">
        <span className="field-icon" aria-hidden="true">♙</span>
        <span className="field-content">
          <span className="field-label">Số khách</span>
          <select name="guests" value={filters.guests} onChange={onChange}>
            {[1, 2, 3, 4, 5, 6].map(count => (
              <option key={count} value={count}>{count} khách</option>
            ))}
          </select>
        </span>
      </label>

      <button className="button button-primary search-button" type="submit" disabled={loading}>
        <span aria-hidden="true">⌕</span>
        {loading ? 'Đang tìm' : 'Tìm kiếm'}
      </button>
    </form>
  );
}
