const FALLBACK_IMAGE = 'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1000&q=85';

export default function HotelCard({ hotel, onOpen }) {
  const roomTypes = hotel.roomTypes ?? [];
  const lowestPrice = roomTypes.length
    ? Math.min(...roomTypes.map(room => Number(room.pricePerNight) || 0))
    : 0;
  const image = hotel.images?.find(Boolean) || hotel.coverImage || FALLBACK_IMAGE;

  return (
    <article className="hotel-card" onClick={() => onOpen(hotel)}>
      <button
        type="button"
        className="favorite-button"
        aria-label="Lưu vào yêu thích"
        onClick={event => event.stopPropagation()}
      >
        ♡
      </button>
      <button className="hotel-card-image" type="button" onClick={() => onOpen(hotel)}>
        <img src={image} alt={hotel.name} loading="lazy" onError={event => { event.currentTarget.src = FALLBACK_IMAGE; }} />
        {hotel.demo && <span className="demo-badge">Gợi ý</span>}
      </button>
      <div className="hotel-card-body">
        <div className="hotel-card-title-row">
          <div>
            <p className="hotel-type">{hotel.type || 'Nơi lưu trú'}</p>
            <h3>{hotel.name}</h3>
          </div>
          <span className="rating"><span>★</span> 4.9</span>
        </div>
        <p className="hotel-location"><span aria-hidden="true">⌖</span> {hotel.address}</p>
        <div className="hotel-card-bottom">
          <span className="room-summary">{roomTypes.length || 1} lựa chọn phòng</span>
          <p className="hotel-price">
            {lowestPrice ? `${lowestPrice.toLocaleString('vi-VN')}₫` : 'Liên hệ'}
            <span> / đêm</span>
          </p>
        </div>
      </div>
    </article>
  );
}
