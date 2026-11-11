import { useEffect, useMemo, useState } from 'react';
import { createBooking, loginAccount, registerAccount, searchAccommodations } from './api.js';
import AuthModal from './components/AuthModal.jsx';
import HotelCard from './components/HotelCard.jsx';
import SearchPanel from './components/SearchPanel.jsx';
import SiteHeader from './components/SiteHeader.jsx';

const FALLBACK_IMAGE = 'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1400&q=85';

const DEMO_HOTELS = [
  {
    id: 'demo-1', demo: true, name: 'Mây Retreat Đà Lạt', type: 'Homestay', address: 'Đà Lạt, Lâm Đồng',
    description: 'Một khoảng bình yên giữa rừng thông, nơi bạn có thể thức dậy cùng sương sớm và nhâm nhi tách cà phê thật chậm.',
    coverImage: 'https://images.unsplash.com/photo-1449158743715-0a90ebb6d2d8?auto=format&fit=crop&w=1400&q=85',
    roomTypes: [{ id: 'demo-room-type-1', name: 'Phòng thông view đồi', description: 'Phòng ấm cúng dành cho cặp đôi.', capacity: 2, pricePerNight: 1250000, rooms: [{ id: 'demo-room-1', number: 'A01', status: 'Trong' }] }]
  },
  {
    id: 'demo-2', demo: true, name: 'An Nhiên Beach House', type: 'Villa', address: 'Sơn Trà, Đà Nẵng',
    description: 'Không gian nghỉ dưỡng gần biển với thiết kế thoáng mở, phù hợp cho những ngày muốn trốn khỏi nhịp sống bận rộn.',
    coverImage: 'https://images.unsplash.com/photo-1571896349842-33c89424de2d?auto=format&fit=crop&w=1400&q=85',
    roomTypes: [{ id: 'demo-room-type-2', name: 'Phòng hướng biển', description: 'Ban công riêng, có khu vực tiếp khách.', capacity: 3, pricePerNight: 1890000, rooms: [{ id: 'demo-room-2', number: 'B02', status: 'Trong' }] }]
  },
  {
    id: 'demo-3', demo: true, name: 'The Leaf Boutique Hotel', type: 'Khách sạn', address: 'Quận 1, TP. Hồ Chí Minh',
    description: 'Khách sạn boutique tinh tế ngay trung tâm, thuận tiện khám phá ẩm thực và những góc phố thú vị của thành phố.',
    coverImage: 'https://images.unsplash.com/photo-1564501049412-61c2a3083791?auto=format&fit=crop&w=1400&q=85',
    roomTypes: [{ id: 'demo-room-type-3', name: 'Deluxe Garden', description: 'Phòng đôi yên tĩnh, đầy đủ tiện nghi.', capacity: 2, pricePerNight: 1580000, rooms: [{ id: 'demo-room-3', number: 'C03', status: 'Trong' }] }]
  },
  {
    id: 'demo-4', demo: true, name: 'Sapa Cloud Lodge', type: 'Lodge', address: 'Sa Pa, Lào Cai',
    description: 'Tận hưởng trọn vẹn vẻ đẹp núi rừng Tây Bắc trong căn phòng gỗ ấm áp, nhìn ra thung lũng mây.',
    coverImage: 'https://images.unsplash.com/photo-1510798831971-661eb04b3739?auto=format&fit=crop&w=1400&q=85',
    roomTypes: [{ id: 'demo-room-type-4', name: 'Mountain View Suite', description: 'Phòng gia đình với tầm nhìn thung lũng.', capacity: 4, pricePerNight: 2150000, rooms: [{ id: 'demo-room-4', number: 'D04', status: 'Trong' }] }]
  }
];

function dateValue(date) {
  return date.toISOString().slice(0, 10);
}

function initialFilters() {
  const checkIn = new Date();
  checkIn.setDate(checkIn.getDate() + 1);
  const checkOut = new Date(checkIn);
  checkOut.setDate(checkOut.getDate() + 2);
  return { location: '', checkIn: dateValue(checkIn), checkOut: dateValue(checkOut), guests: '2' };
}

function filterDemoHotels(filters) {
  const term = filters.location.trim().toLocaleLowerCase('vi');
  return DEMO_HOTELS.filter(hotel => !term ||
    hotel.name.toLocaleLowerCase('vi').includes(term) || hotel.address.toLocaleLowerCase('vi').includes(term));
}

function money(value) {
  return `${Number(value || 0).toLocaleString('vi-VN')}₫`;
}

export default function App() {
  const [filters, setFilters] = useState(initialFilters);
  const [hotels, setHotels] = useState([]);
  const [selectedHotel, setSelectedHotel] = useState(null);
  const [selectedRoomTypeId, setSelectedRoomTypeId] = useState(null);
  const [loading, setLoading] = useState(true);
  const [usingDemoData, setUsingDemoData] = useState(false);
  const [apiMessage, setApiMessage] = useState('');
  const [authMode, setAuthMode] = useState(null);
  const [authLoading, setAuthLoading] = useState(false);
  const [authError, setAuthError] = useState('');
  const [token, setToken] = useState(() => localStorage.getItem('staybook_token') ?? '');
  const [user, setUser] = useState(() => {
    try { return JSON.parse(localStorage.getItem('staybook_user') ?? 'null'); }
    catch { return null; }
  });
  const [toast, setToast] = useState('');

  const roomTypes = selectedHotel?.roomTypes ?? [];
  const selectedRoomType = useMemo(() =>
    roomTypes.find(room => String(room.id) === String(selectedRoomTypeId)) ?? roomTypes[0],
    [roomTypes, selectedRoomTypeId]);
  const selectedRoom = selectedRoomType?.rooms?.[0];

  useEffect(() => {
    loadHotels(initialFilters());
  }, []);

  useEffect(() => {
    if (!toast) return undefined;
    const timer = window.setTimeout(() => setToast(''), 4200);
    return () => window.clearTimeout(timer);
  }, [toast]);

  async function loadHotels(nextFilters) {
    setLoading(true);
    setApiMessage('');
    try {
      const results = await searchAccommodations(nextFilters);
      if (Array.isArray(results) && results.length > 0) {
        setHotels(results);
        setUsingDemoData(false);
      } else {
        setHotels(filterDemoHotels(nextFilters));
        setUsingDemoData(true);
        setApiMessage('Đang hiển thị một vài gợi ý mẫu trong khi nơi lưu trú được cập nhật.');
      }
    } catch {
      setHotels(filterDemoHotels(nextFilters));
      setUsingDemoData(true);
      setApiMessage('Không kết nối được dữ liệu đặt phòng. Bạn vẫn có thể xem giao diện với các gợi ý mẫu.');
    } finally {
      setLoading(false);
    }
  }

  function updateFilter(event) {
    setFilters(current => ({ ...current, [event.target.name]: event.target.value }));
  }

  function submitSearch(event) {
    event.preventDefault();
    if (filters.checkOut <= filters.checkIn) {
      setToast('Ngày trả phòng phải sau ngày nhận phòng.');
      return;
    }
    setSelectedHotel(null);
    loadHotels(filters);
  }

  function openHotel(hotel) {
    setSelectedHotel(hotel);
    setSelectedRoomTypeId(hotel.roomTypes?.[0]?.id ?? null);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  async function submitAuth(values) {
    setAuthLoading(true);
    setAuthError('');
    try {
      const result = authMode === 'register'
        ? await registerAccount(values)
        : await loginAccount({ email: values.email, password: values.password });
      localStorage.setItem('staybook_token', result.accessToken);
      localStorage.setItem('staybook_user', JSON.stringify(result.user));
      setToken(result.accessToken);
      setUser(result.user);
      setAuthMode(null);
      setToast(`Xin chào ${result.user.fullName}!`);
    } catch (error) {
      setAuthError(error.message || 'Không thể xác thực tài khoản.');
    } finally {
      setAuthLoading(false);
    }
  }

  function logout() {
    localStorage.removeItem('staybook_token');
    localStorage.removeItem('staybook_user');
    setToken('');
    setUser(null);
    setToast('Bạn đã đăng xuất.');
  }

  async function bookRoom() {
    if (selectedHotel?.demo || usingDemoData) {
      setToast('Đây là dữ liệu minh họa. Hãy thêm nơi lưu trú vào Backend để thử đặt phòng thật.');
      return;
    }
    if (!token) {
      setAuthError('');
      setAuthMode('login');
      return;
    }
    if (!selectedRoom) {
      setToast('Nơi lưu trú này hiện chưa có phòng khả dụng.');
      return;
    }

    try {
      const booking = await createBooking({
        checkIn: `${filters.checkIn}T00:00:00`,
        checkOut: `${filters.checkOut}T00:00:00`,
        guests: Number(filters.guests),
        roomIds: [Number(selectedRoom.id)]
      }, token);
      setToast(`Đặt phòng thành công! Mã đặt phòng #${booking.id}.`);
      await loadHotels(filters);
    } catch (error) {
      setToast(error.message || 'Đặt phòng chưa thành công. Vui lòng thử lại.');
    }
  }

  const price = selectedRoomType?.pricePerNight ?? 0;
  const image = selectedHotel?.images?.find(Boolean) || selectedHotel?.coverImage || FALLBACK_IMAGE;

  return (
    <div id="top" className="app-shell">
      <SiteHeader
        user={user}
        onLogin={() => { setAuthError(''); setAuthMode('login'); }}
        onLogout={logout}
        onSearch={() => { setSelectedHotel(null); document.getElementById('stays')?.scrollIntoView({ behavior: 'smooth' }); }}
      />

      {selectedHotel ? (
        <main className="detail-page">
          <button className="back-link" type="button" onClick={() => setSelectedHotel(null)}>← Quay lại danh sách</button>
          <div className="detail-heading">
            <div>
              <p className="eyebrow">{selectedHotel.type || 'Nơi lưu trú'} · {selectedHotel.address}</p>
              <h1>{selectedHotel.name}</h1>
              <p className="detail-rating"><span>★ 4.9</span> · Được khách yêu thích</p>
            </div>
            <button className="share-button" type="button" onClick={() => setToast('Đã sẵn sàng chia sẻ nơi lưu trú này.')}>↗ Chia sẻ</button>
          </div>

          <div className="detail-gallery">
            <img className="gallery-main" src={image} alt={selectedHotel.name} onError={event => { event.currentTarget.src = FALLBACK_IMAGE; }} />
            <img src="https://images.unsplash.com/photo-1611892440504-42a792e24d32?auto=format&fit=crop&w=800&q=85" alt="Không gian phòng nghỉ" />
            <img src="https://images.unsplash.com/photo-1600210492486-724fe5c67fb0?auto=format&fit=crop&w=800&q=85" alt="Không gian thư giãn" />
          </div>

          <div className="detail-layout">
            <section className="detail-copy">
              <div className="detail-host-row">
                <div><h2>Không gian nghỉ dưỡng dành cho bạn</h2><p>{selectedHotel.type || 'Nơi lưu trú'} · Có phòng phù hợp với {filters.guests} khách</p></div>
                <div className="host-avatar">S</div>
              </div>
              <p className="detail-description">{selectedHotel.description || 'Tận hưởng kỳ nghỉ thoải mái với không gian được chăm chút và vị trí thuận tiện để khám phá địa phương.'}</p>
              <div className="amenity-list">
                <span>✓ Nhận phòng linh hoạt</span><span>✓ Không gian sạch sẽ</span><span>✓ Hỗ trợ khách hàng</span>
              </div>
              <hr />
              <h2>Chọn loại phòng</h2>
              <div className="room-options">
                {roomTypes.map(room => (
                  <label className={`room-option ${String(selectedRoomTypeId ?? roomTypes[0]?.id) === String(room.id) ? 'selected' : ''}`} key={room.id}>
                    <input type="radio" name="roomType" value={room.id} checked={String(selectedRoomTypeId ?? roomTypes[0]?.id) === String(room.id)} onChange={() => setSelectedRoomTypeId(room.id)} />
                    <span className="room-option-main"><strong>{room.name}</strong><small>{room.description || `Tối đa ${room.capacity} khách`}</small><small>Sức chứa {room.capacity} khách · {room.rooms?.length ?? 0} phòng trống</small></span>
                    <strong>{money(room.pricePerNight)}<small> / đêm</small></strong>
                  </label>
                ))}
              </div>
            </section>

            <aside className="booking-card">
              <p className="booking-price"><strong>{money(price)}</strong> <span>/ đêm</span></p>
              <div className="booking-dates"><div><small>NHẬN PHÒNG</small><strong>{filters.checkIn}</strong></div><div><small>TRẢ PHÒNG</small><strong>{filters.checkOut}</strong></div></div>
              <p className="booking-guests">♙ {filters.guests} khách</p>
              <button className="button button-primary booking-submit" type="button" onClick={bookRoom}>Đặt phòng</button>
              <p className="booking-note">Bạn chưa bị tính phí ở bước này</p>
              <div className="price-line"><span>{money(price)} × 1 đêm</span><span>{money(price)}</span></div>
              <div className="price-line"><span>Phí dịch vụ</span><span>Đã bao gồm</span></div>
              <hr />
              <div className="price-line total"><strong>Tổng cộng</strong><strong>{money(price)}</strong></div>
            </aside>
          </div>
        </main>
      ) : (
        <main>
          <section className="hero">
            <div className="hero-image" />
            <div className="hero-content">
              <p className="hero-kicker"><span /> CHUYẾN ĐI ĐÁNG NHỚ BẮT ĐẦU TỪ ĐÂY</p>
              <h1>Tìm một nơi<br />để <em>thấy mình</em> thuộc về.</h1>
              <p className="hero-subtitle">Những chốn nghỉ bình yên, được chọn lọc cho hành trình của bạn.</p>
            </div>
            <SearchPanel filters={filters} onChange={updateFilter} onSubmit={submitSearch} loading={loading} />
            <div className="hero-bottom-note">Ở đâu cũng có một nơi dành cho bạn <span>✦</span></div>
          </section>

          <section id="stays" className="stays-section section-wrap">
            <div className="section-heading">
              <div><p className="eyebrow">GỢI Ý CHO HÀNH TRÌNH TIẾP THEO</p><h2>Nơi lưu trú nổi bật</h2><p>Chọn một điểm dừng chân, phần còn lại cứ để chuyến đi kể.</p></div>
              <button className="text-link" type="button" onClick={() => loadHotels(filters)}>Khám phá tất cả <span>→</span></button>
            </div>
            {apiMessage && <div className="data-notice"><span>i</span>{apiMessage}</div>}
            {loading ? (
              <div className="hotel-grid">{[1, 2, 3, 4].map(item => <div className="hotel-skeleton" key={item}><div /><span /><span /></div>)}</div>
            ) : hotels.length ? (
              <div className="hotel-grid">
                {hotels.map(hotel => <HotelCard key={hotel.id} hotel={hotel} onOpen={openHotel} />)}
              </div>
            ) : (
              <div className="empty-state"><span>⌕</span><h3>Chưa tìm thấy nơi phù hợp</h3><p>Thử đổi địa điểm hoặc ngày lưu trú để khám phá thêm nhé.</p></div>
            )}
            {usingDemoData && hotels.length > 0 && <p className="demo-caption">* Giá và nơi lưu trú trên trang là dữ liệu minh họa.</p>}
          </section>

          <section id="why-staybook" className="promise-section">
            <div className="section-wrap promise-inner">
              <div><p className="eyebrow">ĐI ĐÂU CŨNG AN TÂM</p><h2>Mỗi chuyến đi,<br /><em>một câu chuyện đẹp.</em></h2></div>
              <div className="promise-items">
                <article><span>✦</span><h3>Chọn lọc tinh tế</h3><p>Nơi nghỉ được tuyển chọn để bạn an tâm tận hưởng.</p></article>
                <article><span>♡</span><h3>Đặt phòng dễ dàng</h3><p>Tìm kiếm và lưu lại hành trình chỉ với vài thao tác.</p></article>
                <article><span>☼</span><h3>Luôn có người hỗ trợ</h3><p>Đội ngũ StayBook đồng hành trước và trong chuyến đi.</p></article>
              </div>
            </div>
          </section>
        </main>
      )}

      <footer className="site-footer">
        <a className="brand footer-brand" href="#top"><span className="brand-mark">S</span><span>stay<span>book</span></span></a>
        <span>Đi để tìm thấy những điều thật đẹp.</span>
        <span>© 2026 StayBook</span>
      </footer>

      {authMode && (
        <AuthModal
          mode={authMode}
          onModeChange={mode => { setAuthMode(mode); setAuthError(''); }}
          onClose={() => setAuthMode(null)}
          onSubmit={submitAuth}
          loading={authLoading}
          error={authError}
        />
      )}
      {toast && <div className="toast-message" role="status">{toast}</div>}
    </div>
  );
}
