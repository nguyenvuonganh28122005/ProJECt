export default function SiteHeader({ user, onLogin, onLogout, onSearch }) {
  return (
    <header className="site-header">
      <a className="brand" href="#top" onClick={onSearch} aria-label="StayBook trang chủ">
        <span className="brand-mark" aria-hidden="true">S</span>
        <span>stay<span>book</span></span>
      </a>

      <nav className="main-nav" aria-label="Điều hướng chính">
        <a href="#stays" onClick={onSearch}>Nơi lưu trú</a>
        <a href="#why-staybook">Về StayBook</a>
      </nav>

      <div className="header-actions">
        <span className="header-help">Cần hỗ trợ?</span>
        {user ? (
          <button className="user-chip" type="button" onClick={onLogout} title="Đăng xuất">
            <span className="user-avatar">{user.fullName?.slice(0, 1)?.toUpperCase() ?? 'S'}</span>
            <span className="user-name">{user.fullName}</span>
            <span className="logout-label">Thoát</span>
          </button>
        ) : (
          <button className="button button-outline header-login" type="button" onClick={onLogin}>
            Đăng nhập
          </button>
        )}
      </div>
    </header>
  );
}
