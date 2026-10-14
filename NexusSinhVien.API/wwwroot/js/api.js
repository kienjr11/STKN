// Cấu hình API URL
const API_URL = '/api';

// Hàm gọi API chung
async function goiAPI(endpoint, method = 'GET', data = null) {
    const token = localStorage.getItem('token');

    const options = {
        method: method,
        headers: {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` })
        }
    };

    if (data) options.body = JSON.stringify(data);

    try {
        const response = await fetch(`${API_URL}${endpoint}`, options);
        const contentType = response.headers.get('content-type');
        let result;

        if (contentType && contentType.includes('application/json')) {
            result = await response.json();
        } else {
            result = await response.text();
        }

        if (!response.ok) {
            return { error: true, status: response.status, message: result.message || 'Có lỗi xảy ra' };
        }
        return result;
    } catch (error) {
        console.error('Lỗi API:', error);
        return { error: true, message: 'Không kết nối được server' };
    }
}

// Các hàm API
const API = {
    // Xác thực
    dangNhap: (email, matKhau) => goiAPI('/xac-thuc/dang-nhap', 'POST', { email, matKhau }),
    dangKy: (data) => goiAPI('/xac-thuc/dang-ky', 'POST', data),
    thongTinCaNhan: () => goiAPI('/xac-thuc/thong-tin'),

    // Danh mục
    layDanhMuc: () => goiAPI('/danh-muc'),

    // Cơ sở
    layDanhSachCoSo: (maDanhMuc, tuKhoa) => {
        let url = '/co-so';
        const params = [];
        if (maDanhMuc) params.push(`maDanhMuc=${maDanhMuc}`);
        if (tuKhoa) params.push(`tuKhoa=${encodeURIComponent(tuKhoa)}`);
        if (params.length) url += '?' + params.join('&');
        return goiAPI(url);
    },
    layCoSoTheoId: (id) => goiAPI(`/co-so/${id}`),

    // Ưu đãi
    layUuDai: () => goiAPI('/uu-dai'),

    // Đánh giá
    layDanhGiaTheoCoSo: (maCoSo) => goiAPI(`/danh-gia/co-so/${maCoSo}`),
    guiDanhGia: (data) => goiAPI('/danh-gia', 'POST', data),

    // Điểm
    layLichSuDiem: () => goiAPI('/diem/lich-su'),

    // Quản trị
    layDanhSachNguoiDung: () => goiAPI('/quan-tri/nguoi-dung')
};

// Quản lý đăng nhập
const Auth = {
    luuDangNhap: (token, nguoiDung) => {
        localStorage.setItem('token', token);
        localStorage.setItem('nguoiDung', JSON.stringify(nguoiDung));
    },
    layNguoiDung: () => {
        const data = localStorage.getItem('nguoiDung');
        return data ? JSON.parse(data) : null;
    },
    layToken: () => localStorage.getItem('token'),
    daDangNhap: () => !!localStorage.getItem('token'),
    dangXuat: () => {
        localStorage.removeItem('token');
        localStorage.removeItem('nguoiDung');
        window.location.href = 'index.html';
    }
};

// Hàm tiện ích
function formatGia(gia) {
    if (!gia) return 'Chưa có';
    return new Intl.NumberFormat('vi-VN').format(gia) + 'đ';
}

function capNhatNavbar() {
    const user = Auth.layNguoiDung();
    const btn = document.getElementById('btnDangNhap');
    if (btn && user) {
        btn.textContent = `👤 ${user.hoTen}`;
        btn.href = 'ca-nhan.html';
    }
}