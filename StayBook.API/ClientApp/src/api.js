const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

async function request(path, options = {}) {
  const headers = new Headers(options.headers ?? {});
  if (options.body) {
    headers.set('Content-Type', 'application/json');
  }
  if (options.token) {
    headers.set('Authorization', `Bearer ${options.token}`);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
    body: options.body ? JSON.stringify(options.body) : undefined
  });

  const contentType = response.headers.get('content-type') ?? '';
  const payload = contentType.includes('application/json')
    ? await response.json()
    : await response.text();

  if (!response.ok) {
    const message = payload?.message ?? payload?.detail ?? payload?.title;
    throw new Error(message || `Yêu cầu thất bại (${response.status}).`);
  }

  return payload;
}

export function searchAccommodations(filters) {
  const query = new URLSearchParams({ page: '1', pageSize: '40' });
  const values = {
    keyword: filters.keyword,
    location: filters.location,
    checkIn: filters.checkIn,
    checkOut: filters.checkOut,
    guests: filters.guests
  };

  for (const [key, value] of Object.entries(values)) {
    if (value !== undefined && value !== null && value !== '') {
      query.set(key, String(value));
    }
  }

  return request(`/api/accommodations?${query.toString()}`);
}

export function registerAccount(values) {
  return request('/api/accounts/register', { method: 'POST', body: values });
}

export function loginAccount(values) {
  return request('/api/accounts/login', { method: 'POST', body: values });
}

export function createBooking(values, token) {
  return request('/api/bookings', { method: 'POST', body: values, token });
}
