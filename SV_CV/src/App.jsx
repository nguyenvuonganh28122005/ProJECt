import { useEffect, useRef, useState } from "react";

const KEY = "personal-cv-builder-v1";
const initial = {
  profile: { name: "Nguyễn Văn An", email: "nguyenvanan@gmail.com", phone: "0912 345 678", address: "Hà Nội", summary: "Sinh viên Công nghệ thông tin yêu thích phát triển web, có tinh thần học hỏi và khả năng làm việc nhóm.", photo: "" },
  education: [{ id: 1, school: "Trường Đại học Nguyễn Trãi", major: "Công nghệ thông tin", time: "2023 - 2027", detail: "Điểm trung bình: 3.2/4.0" }],
  skills: [{ id: 2, name: "HTML, CSS, JavaScript" }, { id: 3, name: "React cơ bản" }, { id: 4, name: "SQL Server" }],
  projects: [
    { id: 5, name: "Website quản lý kho", description: "Xây dựng website quản lý hàng hóa, phiếu nhập xuất và tồn kho.", tech: "ASP.NET Core MVC, SQL Server", role: "Lập trình giao diện và xử lý CRUD" },
    { id: 6, name: "Website bán hàng", description: "Thiết kế giao diện danh sách sản phẩm và giỏ hàng.", tech: "React, CSS", role: "Frontend Developer" }
  ],
  template: "modern",
  accent: "#176b6b"
};

function App() {
  const [cv, setCv] = useState(() => {
    try {
      const saved = JSON.parse(localStorage.getItem(KEY));
      return saved ? { ...initial, ...saved, profile: { ...initial.profile, ...(saved.profile || {}) } } : initial;
    } catch { return initial; }
  });
  const [message, setMessage] = useState("");
  const [emailError, setEmailError] = useState("");
  const importRef = useRef(null);

  useEffect(() => { localStorage.setItem(KEY, JSON.stringify(cv)); }, [cv]);

  const updateProfile = (field, value) => setCv(old => ({ ...old, profile: { ...old.profile, [field]: value } }));
  const updateItem = (type, id, field, value) => setCv(old => ({
    ...old, [type]: old[type].map(item => item.id === id ? { ...item, [field]: value } : item)
  }));
  const addItem = (type, item) => setCv(old => ({ ...old, [type]: [...old[type], { id: Date.now(), ...item }] }));
  const removeItem = (type, id) => setCv(old => ({ ...old, [type]: old[type].filter(item => item.id !== id) }));

  const notify = text => {
    setMessage(text);
    window.setTimeout(() => setMessage(""), 2500);
  };
  const saveDraft = () => {
    localStorage.setItem(KEY, JSON.stringify(cv));
    notify("Đã lưu bản nháp trên trình duyệt!");
  };
  const validateEmail = value => {
    updateProfile("email", value);
    setEmailError(value && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value) ? "Email chưa đúng định dạng." : "");
  };
  const resetDraft = () => {
    if (window.confirm("Bạn có chắc muốn khôi phục CV mẫu ban đầu?")) {
      setCv(initial); localStorage.setItem(KEY, JSON.stringify(initial)); setEmailError("");
    }
  };

  const handlePhoto = event => {
    const file = event.target.files?.[0];
    if (!file) return;
    if (!file.type.startsWith("image/")) { notify("Vui lòng chọn tệp hình ảnh."); return; }
    if (file.size > 2 * 1024 * 1024) { notify("Ảnh tối đa 2MB để tránh đầy bộ nhớ trình duyệt."); return; }
    const reader = new FileReader();
    reader.onload = () => updateProfile("photo", reader.result);
    reader.readAsDataURL(file);
    event.target.value = "";
  };
  const exportJSON = () => {
    const blob = new Blob([JSON.stringify(cv, null, 2)], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url; link.download = "cv-ca-nhan.json"; link.click();
    URL.revokeObjectURL(url);
    notify("Đã xuất dữ liệu CV thành file JSON.");
  };
  const importJSON = event => {
    const file = event.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      try {
        const data = JSON.parse(reader.result);
        if (!data || typeof data !== "object" || !data.profile || !Array.isArray(data.education) || !Array.isArray(data.skills) || !Array.isArray(data.projects)) {
          throw new Error("Cấu trúc không hợp lệ");
        }
        setCv({
          ...initial, ...data,
          profile: { ...initial.profile, ...data.profile },
          education: data.education,
          skills: data.skills,
          projects: data.projects,
          template: data.template === "classic" ? "classic" : "modern",
          accent: /^#[0-9A-Fa-f]{6}$/.test(data.accent || "") ? data.accent : initial.accent
        });
        setEmailError("");
        notify("Đã nhập CV từ file JSON thành công.");
      } catch {
        notify("File JSON không đúng định dạng CV. Dữ liệu hiện tại được giữ nguyên.");
      }
      event.target.value = "";
    };
    reader.readAsText(file);
  };

  return <div className="app" style={{ "--accent": cv.accent || initial.accent }}>
    <header className="topbar">
      <div><div className="brand">CV<span>Studio</span></div><small>Tạo CV chuyên nghiệp của bạn</small></div>
      <div className="top-actions">
        <button className="btn light" onClick={saveDraft}>Lưu bản nháp</button>
        <button className="btn light" onClick={exportJSON}>Xuất JSON</button>
        <button className="btn light" onClick={() => importRef.current?.click()}>Nhập JSON</button>
        <input ref={importRef} type="file" accept=".json,application/json" hidden onChange={importJSON} />
        <button className="btn primary" onClick={() => window.print()}>Xuất PDF / In CV</button>
      </div>
    </header>
    {message && <div className="toast">{message}</div>}
    <main className="workspace">
      <section className="editor">
        <div className="section-heading"><div><span className="eyebrow">THÔNG TIN CỦA BẠN</span><h1>Biên tập CV</h1></div><button className="text-btn" onClick={resetDraft}>Khôi phục mẫu</button></div>
        <div className="panel">
          <h2>01. Thông tin cá nhân</h2>
          <div className="photo-control">
            <div className="photo-thumb">{cv.profile.photo ? <img src={cv.profile.photo} alt="Ảnh đại diện" /> : <span>Ảnh</span>}</div>
            <div><b>Ảnh đại diện</b><p>Chọn ảnh JPG, PNG hoặc WEBP, tối đa 2MB.</p><label className="upload-btn">Chọn ảnh<input type="file" accept="image/*" onChange={handlePhoto} /></label>
              {cv.profile.photo && <button className="remove-photo" onClick={() => updateProfile("photo", "")}>Xóa ảnh</button>}
            </div>
          </div>
          <div className="form-grid">
            <label>Họ và tên<input value={cv.profile.name} onChange={e => updateProfile("name", e.target.value)} placeholder="Nguyễn Văn A" /></label>
            <label>Email<input type="email" value={cv.profile.email} onChange={e => validateEmail(e.target.value)} placeholder="example@gmail.com" />{emailError && <small className="error">{emailError}</small>}</label>
            <label>Số điện thoại<input value={cv.profile.phone} onChange={e => updateProfile("phone", e.target.value)} placeholder="09xxxxxxxx" /></label>
            <label>Địa chỉ<input value={cv.profile.address} onChange={e => updateProfile("address", e.target.value)} placeholder="Hà Nội" /></label>
          </div>
          <label>Giới thiệu bản thân<textarea rows="3" value={cv.profile.summary} onChange={e => updateProfile("summary", e.target.value)} /></label>
        </div>

        <div className="panel">
          <div className="panel-title"><h2>02. Học vấn</h2><button className="add-btn" onClick={() => addItem("education", { school: "", major: "", time: "", detail: "" })}>+ Thêm</button></div>
          {cv.education.map((x, i) => <div className="item-editor" key={x.id}>
            <div className="item-head"><b>Học vấn {i + 1}</b><button className="remove-btn" onClick={() => removeItem("education", x.id)}>Xóa</button></div>
            <label>Trường học<input value={x.school} onChange={e => updateItem("education", x.id, "school", e.target.value)} /></label>
            <div className="form-grid"><label>Ngành học<input value={x.major} onChange={e => updateItem("education", x.id, "major", e.target.value)} /></label><label>Thời gian<input value={x.time} onChange={e => updateItem("education", x.id, "time", e.target.value)} /></label></div>
            <label>Mô tả<input value={x.detail} onChange={e => updateItem("education", x.id, "detail", e.target.value)} /></label>
          </div>)}
        </div>

        <div className="panel">
          <div className="panel-title"><h2>03. Kỹ năng</h2><button className="add-btn" onClick={() => addItem("skills", { name: "" })}>+ Thêm</button></div>
          {cv.skills.map((x, i) => <div className="inline-item" key={x.id}><input aria-label={"Kỹ năng " + (i+1)} value={x.name} onChange={e => updateItem("skills", x.id, "name", e.target.value)} placeholder="Tên kỹ năng" /><button className="remove-btn" onClick={() => removeItem("skills", x.id)}>Xóa</button></div>)}
        </div>

        <div className="panel">
          <div className="panel-title"><h2>04. Dự án</h2><button className="add-btn" onClick={() => addItem("projects", { name: "", description: "", tech: "", role: "" })}>+ Thêm dự án</button></div>
          {cv.projects.map((x, i) => <div className="item-editor" key={x.id}>
            <div className="item-head"><b>Dự án {i + 1}</b><button className="remove-btn" onClick={() => removeItem("projects", x.id)}>Xóa</button></div>
            <label>Tên dự án<input value={x.name} onChange={e => updateItem("projects", x.id, "name", e.target.value)} /></label>
            <label>Mô tả<textarea rows="2" value={x.description} onChange={e => updateItem("projects", x.id, "description", e.target.value)} /></label>
            <div className="form-grid"><label>Công nghệ<input value={x.tech} onChange={e => updateItem("projects", x.id, "tech", e.target.value)} /></label><label>Vai trò<input value={x.role} onChange={e => updateItem("projects", x.id, "role", e.target.value)} /></label></div>
          </div>)}
        </div>
        <div className="panel">
          <h2>05. Tùy chỉnh giao diện</h2>
          <label>Màu chủ đạo
            <div className="color-control"><input type="color" value={cv.accent || initial.accent} onChange={e => setCv(old => ({ ...old, accent: e.target.value }))} /><span>{cv.accent || initial.accent}</span><button className="text-btn" onClick={() => setCv(old => ({ ...old, accent: initial.accent }))}>Đặt lại màu</button></div>
          </label>
        </div>
      </section>

      <section className="preview-area">
        <div className="preview-toolbar"><div><span className="eyebrow">LIVE PREVIEW</span><h2>Xem trước CV</h2></div>
          <div className="template-switch"><button className={cv.template === "modern" ? "selected" : ""} onClick={() => setCv(old => ({ ...old, template: "modern" }))}>Mẫu 01</button><button className={cv.template === "classic" ? "selected" : ""} onClick={() => setCv(old => ({ ...old, template: "classic" }))}>Mẫu 02</button></div>
        </div>
        <article className={"resume " + cv.template}>
          <div className="resume-header">
            <div className="resume-identity">{cv.profile.photo && <img className="resume-photo" src={cv.profile.photo} alt="Ảnh đại diện" />}<div><h1>{cv.profile.name || "Họ và tên của bạn"}</h1><p className="job-title">SINH VIÊN / ỨNG VIÊN THỰC TẬP</p></div></div>
            <div className="contact"><span>✉ {cv.profile.email || "Email"}</span><span>☎ {cv.profile.phone || "Số điện thoại"}</span><span>⌖ {cv.profile.address || "Địa chỉ"}</span></div>
          </div>
          <section className="resume-section"><h3>GIỚI THIỆU</h3><p>{cv.profile.summary || "Phần giới thiệu bản thân..."}</p></section>
          <section className="resume-section"><h3>HỌC VẤN</h3>{cv.education.length ? cv.education.map(x => <div className="resume-entry" key={x.id}><div className="entry-top"><b>{x.school || "Tên trường"}</b><span>{x.time}</span></div><i>{x.major}</i><p>{x.detail}</p></div>) : <p className="muted">Chưa có thông tin học vấn.</p>}</section>
          <section className="resume-section"><h3>KỸ NĂNG</h3>{cv.skills.length ? <div className="skill-list">{cv.skills.map(x => <span key={x.id}>{x.name || "Kỹ năng"}</span>)}</div> : <p className="muted">Chưa có kỹ năng.</p>}</section>
          <section className="resume-section"><h3>DỰ ÁN TIÊU BIỂU</h3>{cv.projects.length ? cv.projects.map(x => <div className="resume-entry" key={x.id}><b>{x.name || "Tên dự án"}</b><p>{x.description}</p><p><strong>Công nghệ:</strong> {x.tech}</p><p><strong>Vai trò:</strong> {x.role}</p></div>) : <p className="muted">Chưa có dự án.</p>}</section>
          <div className="resume-footer">CV được tạo bằng CV Studio</div>
        </article>
        <p className="preview-note">Bản xem trước thay đổi ngay khi bạn chỉnh sửa thông tin.</p>
      </section>
    </main>
    <footer>© 2026 CV Studio · Personal CV Builder</footer>
  </div>;
}
export default App;
