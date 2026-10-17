// ============================================================
// CẤU HÌNH API URL
// ============================================================
const API_URL = '/api';

// ============================================================
// HÀM GỌI API CHUNG
// ============================================================
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
            // ✅ XỬ LÝ KHI TOKEN HẾT HẠN HOẶC SAI (LỖI 401)
            if (response.status === 401) {
                localStorage.removeItem('token');
                localStorage.removeItem('nguoiDung');
                alert('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại!');
                window.location.href = 'dang-nhap.html';
                return { error: true, status: 401, message: 'Hết phiên đăng nhập' };
            }

            return {
                error: true,
                status: response.status,
                message: result.message || 'Có lỗi xảy ra'
            };
        }
        return result;
    } catch (error) {
        console.error('Lỗi API:', error);
        return { error: true, message: 'Không kết nối được server' };
    }
}

// ============================================================
// CÁC HÀM API
// ============================================================
const API = {
    // ============ XÁC THỰC ============
    dangNhap: (email, matKhau) => goiAPI('/xac-thuc/dang-nhap', 'POST', { email, matKhau }),
    dangKy: (data) => goiAPI('/xac-thuc/dang-ky', 'POST', data),
    thongTinCaNhan: () => goiAPI('/xac-thuc/thong-tin'),
    kiemTraQuyen: () => goiAPI('/xac-thuc/kiem-tra-quyen'),
    xacThucSinhVien: () => goiAPI('/xac-thuc/xac-thuc-sinh-vien', 'POST'),

    // ✅ MỚI: Quy trình xác thực chặt chẽ
    guiYeuCauXacThuc: (data) => goiAPI('/xac-thuc/gui-yeu-cau-xac-thuc', 'POST', data),
    layTrangThaiXacThuc: () => goiAPI('/xac-thuc/trang-thai-xac-thuc'),

    // ============ DANH MỤC ============
    layDanhMuc: () => goiAPI('/danh-muc'),

    // ============ CƠ SỞ ============
    layDanhSachCoSo: (filtersOrMaDanhMuc, tuKhoa) => {
        let filters = {};

        if (typeof filtersOrMaDanhMuc === 'object' && filtersOrMaDanhMuc !== null) {
            filters = filtersOrMaDanhMuc;
        } else {
            filters = {
                maDanhMuc: filtersOrMaDanhMuc,
                tuKhoa: tuKhoa
            };
        }

        let url = '/co-so';
        const params = [];
        if (filters.maDanhMuc) params.push(`maDanhMuc=${filters.maDanhMuc}`);
        if (filters.tuKhoa) params.push(`tuKhoa=${encodeURIComponent(filters.tuKhoa)}`);
        if (filters.giaMin) params.push(`giaMin=${filters.giaMin}`);
        if (filters.giaMax) params.push(`giaMax=${filters.giaMax}`);
        if (filters.danhGiaMin) params.push(`danhGiaMin=${filters.danhGiaMin}`);
        if (filters.viDo) params.push(`viDo=${filters.viDo}`);
        if (filters.kinhDo) params.push(`kinhDo=${filters.kinhDo}`);
        if (filters.banKinh) params.push(`banKinh=${filters.banKinh}`);
        if (params.length) url += '?' + params.join('&');
        return goiAPI(url);
    },

    layCoSoTheoId: (id) => goiAPI(`/co-so/${id}`),

    // API cho đối tác
    layCoSoCuaToi: () => goiAPI('/co-so/cua-toi'),
    dangKyCoSo: (data) => goiAPI('/co-so/dang-ky', 'POST', data),
    capNhatCoSo: (id, data) => goiAPI(`/co-so/${id}`, 'PUT', data),

    // ============ DỊCH VỤ ============
    layDichVuTheoCoSo: (maCoSo) => goiAPI(`/dich-vu/co-so/${maCoSo}`),
    themDichVu: (data) => goiAPI('/dich-vu', 'POST', data),
    capNhatDichVu: (id, data) => goiAPI(`/dich-vu/${id}`, 'PUT', data),
    xoaDichVu: (id) => goiAPI(`/dich-vu/${id}`, 'DELETE'),

    // ============ ƯU ĐÃI ============
    layUuDai: (maCoSo) => {
        let url = '/uu-dai';
        if (maCoSo) url += `?maCoSo=${maCoSo}`;
        return goiAPI(url);
    },
    layUuDaiTheoId: (id) => goiAPI(`/uu-dai/${id}`),
    themUuDai: (data) => goiAPI('/uu-dai', 'POST', data),

    // ============ ĐÁNH GIÁ ============
    layDanhGiaTheoCoSo: (maCoSo) => goiAPI(`/danh-gia/co-so/${maCoSo}`),
    guiDanhGia: (data) => goiAPI('/danh-gia', 'POST', data),
    layDanhGiaCuaToi: () => goiAPI('/danh-gia/cua-toi'),

    // ============ ĐIỂM & VOUCHER ============
    layLichSuDiem: () => goiAPI('/diem/lich-su'),
    layChiTietDiem: () => goiAPI('/diem/chi-tiet'),
    chiaSe: () => goiAPI('/diem/chia-se', 'POST'),
    diemDanhHangNgay: () => goiAPI('/diem/dang-nhap-hang-ngay', 'POST'),

    layPhieuGiamGia: () => goiAPI('/phieu-giam-gia/cua-toi'),
    doiDiem: (maUuDai, soDiem) => goiAPI('/phieu-giam-gia/doi-diem', 'POST', { maUuDai, soDiem }),

    // ============ BÁO CÁO ============
    guiBaoCao: (data) => goiAPI('/bao-cao', 'POST', data),
    layBaoCaoCuaToi: () => goiAPI('/bao-cao/cua-toi'),

    // ============ UPLOAD ============
    uploadAnh: async (file) => {
        const token = localStorage.getItem('token');
        const formData = new FormData();
        formData.append('file', file);

        try {
            const res = await fetch('/api/upload/anh', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`
                },
                body: formData
            });
            const kq = await res.json();
            if (!res.ok) {
                return { error: true, message: kq.message || 'Upload thất bại' };
            }
            return kq;
        } catch (error) {
            console.error('Lỗi upload:', error);
            return { error: true, message: 'Không upload được ảnh' };
        }
    },

    // ============ QUẢN TRỊ ============
    layDanhSachNguoiDung: () => goiAPI('/quan-tri/nguoi-dung'),
    layThongKe: () => goiAPI('/quan-tri/thong-ke'),
    layCoSoChoDuyet: () => goiAPI('/quan-tri/duyet-co-so'),
    duyetCoSo: (id) => goiAPI(`/quan-tri/duyet-co-so/${id}`, 'PUT'),
    tuChoiCoSo: (id) => goiAPI(`/quan-tri/tu-choi-co-so/${id}`, 'PUT'),

    // ✅ MỚI: Quản trị xác thực sinh viên
    layYeuCauXacThuc: () => goiAPI('/quan-tri/yeu-cau-xac-thuc'),
    duyetXacThuc: (id) => goiAPI(`/quan-tri/duyet-xac-thuc/${id}`, 'PUT'),
    tuChoiXacThuc: (id, lyDo) => goiAPI(`/quan-tri/tu-choi-xac-thuc/${id}`, 'PUT', { lyDo })
};

// ============================================================
// QUẢN LÝ ĐĂNG NHẬP + PHÂN QUYỀN
// ============================================================
const Auth = {
    // Lưu thông tin đăng nhập
    luuDangNhap: (token, nguoiDung) => {
        localStorage.setItem('token', token);
        localStorage.setItem('nguoiDung', JSON.stringify(nguoiDung));
    },

    // Lấy thông tin người dùng
    layNguoiDung: () => {
        const data = localStorage.getItem('nguoiDung');
        return data ? JSON.parse(data) : null;
    },

    // Lấy token
    layToken: () => localStorage.getItem('token'),

    // Đã đăng nhập chưa
    daDangNhap: () => !!localStorage.getItem('token'),

    // Lấy loại người dùng
    layLoaiNguoiDung: () => {
        const user = Auth.layNguoiDung();
        return user ? user.loaiNguoiDung : null;
    },

    // Kiểm tra có phải admin không
    laAdmin: () => Auth.layLoaiNguoiDung() === 'quan_tri',

    // Kiểm tra có phải cơ sở kinh doanh không
    laCoSo: () => Auth.layLoaiNguoiDung() === 'co_so_kinh_doanh',

    // Kiểm tra có phải sinh viên không
    laSinhVien: () => Auth.layLoaiNguoiDung() === 'sinh_vien',

    // Kiểm tra có phải đối tác không (CSKD hoặc Admin)
    laDoiTac: () => {
        const loai = Auth.layLoaiNguoiDung();
        return loai === 'co_so_kinh_doanh' || loai === 'quan_tri';
    },

    // Kiểm tra có quyền cụ thể không
    coQuyen: (quyen) => {
        if (!Auth.daDangNhap()) return false;

        const loai = Auth.layLoaiNguoiDung();

        // Định nghĩa quyền theo vai trò
        const quyenTheoVaiTro = {
            'sinh_vien': [
                'xem-trang-chu', 'tim-kiem', 'xem-chi-tiet', 'xem-uu-dai', 'xem-danh-gia',
                'danh-gia', // ✅ Chỉ Sinh viên mới có quyền đánh giá
                'tich-diem', 'doi-voucher', 'xem-voucher', 'bao-cao'
            ],
            'co_so_kinh_doanh': [
                'xem-trang-chu', 'tim-kiem', 'xem-chi-tiet', 'xem-uu-dai', 'xem-danh-gia',
                // ✅ Đã xóa 'danh-gia' ở đây
                'tich-diem', 'doi-voucher', 'xem-voucher', 'bao-cao',
                'quan-ly-co-so', 'quan-ly-dich-vu', 'quan-ly-uu-dai', 'upload-anh', 'dang-ky-co-so'
            ],
            'quan_tri': [
                'xem-trang-chu', 'tim-kiem', 'xem-chi-tiet', 'xem-uu-dai', 'xem-danh-gia',
                // ✅ Đã xóa 'danh-gia' ở đây
                'tich-diem', 'doi-voucher', 'xem-voucher', 'bao-cao',
                'quan-ly-co-so', 'quan-ly-dich-vu', 'quan-ly-uu-dai', 'upload-anh', 'dang-ky-co-so',
                'quan-tri', 'duyet-co-so', 'quan-ly-nguoi-dung', 'xem-thong-ke', 'xem-bao-cao',
                'duyet-xac-thuc'
            ]
        };

        return quyenTheoVaiTro[loai]?.includes(quyen) || false;
    },

    // Bảo vệ trang - yêu cầu quyền cụ thể
    yeuCauQuyen: (quyen, trangChuyenHuong = 'index.html') => {
        if (!Auth.coQuyen(quyen)) {
            alert('⛔ Bạn không có quyền truy cập trang này');
            window.location.href = trangChuyenHuong;
            return false;
        }
        return true;
    },

    // Bảo vệ trang admin
    yeuCauAdmin: () => {
        if (!Auth.laAdmin()) {
            alert('⛔ Chỉ quản trị viên mới được truy cập');
            window.location.href = 'index.html';
            return false;
        }
        return true;
    },

    // Bảo vệ trang đối tác (CSKD + Admin)
    yeuCauDoiTac: () => {
        if (!Auth.laDoiTac()) {
            alert('⛔ Chỉ đối tác kinh doanh mới được truy cập');
            window.location.href = 'index.html';
            return false;
        }
        return true;
    },

    // Bảo vệ trang đã đăng nhập
    yeuCauDangNhap: () => {
        if (!Auth.daDangNhap()) {
            alert('⚠️ Vui lòng đăng nhập để tiếp tục');
            window.location.href = 'dang-nhap.html';
            return false;
        }
        return true;
    },

    // Ẩn/hiện các phần tử có data-quyen
    capNhatPhanTu: () => {
        document.querySelectorAll('[data-quyen]').forEach(el => {
            const quyen = el.dataset.quyen;
            if (!Auth.coQuyen(quyen)) {
                el.style.display = 'none';
            }
        });

        document.querySelectorAll('[data-vai-tro]').forEach(el => {
            const vaiTro = el.dataset.vaiTro.split(',').map(s => s.trim());
            const vaiTroHienTai = Auth.layLoaiNguoiDung() || 'khach';
            if (!vaiTro.includes(vaiTroHienTai)) {
                el.style.display = 'none';
            }
        });
    },

    // Đăng xuất
    dangXuat: () => {
        localStorage.removeItem('token');
        localStorage.removeItem('nguoiDung');
        window.location.href = 'index.html';
    }
};

// ============================================================
// HÀM TIỆN ÍCH
// ============================================================

// Format giá tiền VNĐ
function formatGia(gia) {
    if (!gia) return 'Chưa có';
    return new Intl.NumberFormat('vi-VN').format(gia) + 'đ';
}

// Cập nhật navbar (hiển thị tên người dùng)
function capNhatNavbar() {
    const user = Auth.layNguoiDung();
    const btn = document.getElementById('btnDangNhap');
    if (btn && user) {
        btn.textContent = `👤 ${user.hoTen}`;
        btn.href = 'ca-nhan.html';
    }

    // Cập nhật các phần tử có data-quyen
    Auth.capNhatPhanTu();
}

// Format ngày tháng
function formatNgay(ngay) {
    if (!ngay) return '';
    return new Date(ngay).toLocaleDateString('vi-VN');
}

// Hiển thị thông báo (dùng cho toast)
function hienThiThongBao(message, type = 'success') {
    const toast = document.createElement('div');
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 15px 25px;
        background: ${type === 'success' ? '#38a169' : '#e53e3e'};
        color: white;
        border-radius: 10px;
        box-shadow: 0 10px 30px rgba(0,0,0,0.2);
        z-index: 9999;
        font-weight: 600;
        animation: slideIn 0.3s ease;
    `;
    toast.textContent = message;
    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Thêm CSS animation cho toast
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from { transform: translateX(400px); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    @keyframes slideOut {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(400px); opacity: 0; }
    }
`;
document.head.appendChild(style);