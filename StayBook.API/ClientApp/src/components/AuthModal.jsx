import { useState } from 'react';

export default function AuthModal({ mode, onModeChange, onClose, onSubmit, loading, error }) {
  const [values, setValues] = useState({ fullName: '', email: '', phoneNumber: '', password: '' });
  const isRegister = mode === 'register';

  function update(event) {
    setValues(current => ({ ...current, [event.target.name]: event.target.value }));
  }

  function submit(event) {
    event.preventDefault();
    onSubmit(values);
  }

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={event => event.target === event.currentTarget && onClose()}>
      <section className="auth-modal" role="dialog" aria-modal="true" aria-labelledby="auth-title">
        <button className="modal-close" type="button" onClick={onClose} aria-label="Đóng">×</button>
        <div className="auth-brand-mark">S</div>
        <p className="eyebrow">Chào mừng đến với StayBook</p>
        <h2 id="auth-title">{isRegister ? 'Tạo tài khoản mới' : 'Rất vui được gặp lại bạn'}</h2>
        <p className="auth-intro">Đăng nhập để lưu hành trình và đặt nơi nghỉ yêu thích.</p>

        <form className="auth-form" onSubmit={submit}>
          {isRegister && (
            <>
              <label>Họ và tên<input name="fullName" value={values.fullName} onChange={update} required minLength="2" autoComplete="name" /></label>
              <label>Số điện thoại<input name="phoneNumber" value={values.phoneNumber} onChange={update} required minLength="8" autoComplete="tel" /></label>
            </>
          )}
          <label>Email<input name="email" type="email" value={values.email} onChange={update} required autoComplete="email" /></label>
          <label>Mật khẩu<input name="password" type="password" value={values.password} onChange={update} required minLength="8" autoComplete={isRegister ? 'new-password' : 'current-password'} /></label>
          {error && <p className="form-error" role="alert">{error}</p>}
          <button className="button button-primary auth-submit" type="submit" disabled={loading}>
            {loading ? 'Đang xử lý...' : isRegister ? 'Tạo tài khoản' : 'Đăng nhập'}
          </button>
        </form>

        <p className="auth-switch">
          {isRegister ? 'Đã có tài khoản?' : 'Chưa có tài khoản?'}{' '}
          <button type="button" onClick={() => onModeChange(isRegister ? 'login' : 'register')}>
            {isRegister ? 'Đăng nhập' : 'Đăng ký ngay'}
          </button>
        </p>
      </section>
    </div>
  );
}
