// ============================================================
// HỆ THỐNG PHÂN QUYỀN
// ============================================================

const VaiTro = {
    KHACH: 'khach',
    SINH_VIEN: 'sinh_vien',
    CO_SO: 'co_so_kinh_doanh',
    QUAN_TRI: 'quan_tri'
};

// Định nghĩa quyền hạn
const Quyen = {
    // Khách (chưa đăng nhập)
    [VaiTro.KHACH]: {
        tenHienThi: '👤 Khách',
        menu: [
            { text: 'Trang chủ', href: 'index.html' },
            { text: 'Tìm kiếm', href: 'tim-kiem.html' },
            { text: 'Ưu đãi', href: 'uu-dai.html' },
            { text: 'Đăng nhập', href: 'dang-nhap.html', id: 'btnDangNhap' }
        ],
        choPhep: [
            'xem-trang-chu',
            'tim-kiem',
            'xem-chi-tiet',
            'xem-uu-dai',
            'xem-danh-gia'
        ]
    },

    // Sinh viên
    [VaiTro.SINH_VIEN]: {
        tenHienThi: '🎓 Sinh viên',
        menu: [
            { text: 'Trang chủ', href: 'index.html' },
            { text: 'Tìm kiếm', href: 'tim-kiem.html' },
            { text: 'Ưu đãi', href: 'uu-dai.html' },
            { text: 'Cá nhân', href: 'ca-nhan.html', id: 'btnDangNhap' }
        ],
        choPhep: [
            'xem-trang-chu',
            'tim-kiem',
            'xem-chi-tiet',
            'xem-uu-dai',
            'xem-danh-gia',
            'danh-gia',
            'tich-diem',
            'doi-voucher',
            'xem-voucher',
            'bao-cao'
        ]
    },

    // Cơ sở kinh doanh
    [VaiTro.CO_SO]: {
        tenHienThi: '🏪 Cơ sở KD',
        menu: [
            { text: 'Trang chủ', href: 'index.html' },
            { text: 'Tìm kiếm', href: 'tim-kiem.html' },
            { text: 'Ưu đãi', href: 'uu-dai.html' },
            { text: 'Quản lý cơ sở', href: 'quan-ly-co-so.html' },
            { text: 'Cá nhân', href: 'ca-nhan.html', id: 'btnDangNhap' }
        ],
        choPhep: [
            'xem-trang-chu',
            'tim-kiem',
            'xem-chi-tiet',
            'xem-uu-dai',
            'xem-danh-gia',
            'danh-gia',
            'tich-diem',
            'doi-voucher',
            'xem-voucher',
            'bao-cao',
            'quan-ly-co-so',
            'quan-ly-dich-vu',
            'quan-ly-uu-dai',
            'upload-anh',
            'dang-ky-co-so'
        ]
    },

    // Quản trị viên
    [VaiTro.QUAN_TRI]: {
        tenHienThi: '👑 Quản trị viên',
        menu: [
            { text: 'Trang chủ', href: 'index.html' },
            { text: 'Tìm kiếm', href: 'tim-kiem.html' },
            { text: 'Ưu đãi', href: 'uu-dai.html' },
            { text: 'Quản trị', href: 'quan-tri.html' },
            { text: 'Thống kê', href: 'thong-ke.html' },
            { text: 'Cá nhân', href: 'ca-nhan.html', id: 'btnDangNhap' }
        ],
        choPhep: [
            // Tất cả quyền
            'xem-trang-chu',
            'tim-kiem',
            'xem-chi-tiet',
            'xem-uu-dai',
            'xem-danh-gia',
            'danh-gia',
            'tich-diem',
            'doi-voucher',
            'xem-voucher',
            'bao-cao',
            'quan-ly-co-so',
            'quan-ly-dich-vu',
            'quan-ly-uu-dai',
            'upload-anh',
            'dang-ky-co-so',
            'quan-tri',
            'duyet-co-so',
            'quan-ly-nguoi-dung',
            'xem-thong-ke',
            'xem-bao-cao'
        ]
    }
};

// ============================================================
// HÀM XỬ LÝ PHÂN QUYỀN
// ============================================================

const PhanQuyen = {
    // Lấy vai trò hiện tại
    layVaiTro() {
        const user = Auth.layNguoiDung();
        if (!user) return VaiTro.KHACH;
        return user.loaiNguoiDung || VaiTro.KHACH;
    },

    // Kiểm tra có quyền không
    coQuyen(quyen) {
        const vaiTro = this.layVaiTro();
        const config = Quyen[vaiTro];
        if (!config) return false;
        return config.choPhep.includes(quyen);
    },

    // Kiểm tra có phải admin không
    laAdmin() {
        return this.layVaiTro() === VaiTro.QUAN_TRI;
    },

    // Kiểm tra có phải đối tác không (CSKD hoặc Admin)
    laDoiTac() {
        const vt = this.layVaiTro();
        return vt === VaiTro.CO_SO || vt === VaiTro.QUAN_TRI;
    },

    // Kiểm tra đã đăng nhập chưa
    daDangNhap() {
        return this.layVaiTro() !== VaiTro.KHACH;
    },

    // Bảo vệ trang: nếu không có quyền → chuyển hướng
    yeuCauQuyen(quyen, trangChuyenHuong = 'index.html') {
        if (!this.coQuyen(quyen)) {
            alert('⛔ Bạn không có quyền truy cập trang này');
            window.location.href = trangChuyenHuong;
            return false;
        }
        return true;
    },

    // Bảo vệ trang admin
    yeuCauAdmin() {
        if (!this.laAdmin()) {
            alert('⛔ Chỉ quản trị viên mới được truy cập');
            window.location.href = 'index.html';
            return false;
        }
        return true;
    },

    // Bảo vệ trang đối tác (CSKD hoặc Admin)
    yeuCauDoiTac() {
        if (!this.laDoiTac()) {
            alert('⛔ Chỉ đối tác kinh doanh mới được truy cập');
            window.location.href = 'index.html';
            return false;
        }
        return true;
    },

    // Bảo vệ trang đã đăng nhập
    yeuCauDangNhap() {
        if (!this.daDangNhap()) {
            alert('⚠️ Vui lòng đăng nhập để tiếp tục');
            window.location.href = 'dang-nhap.html';
            return false;
        }
        return true;
    },

    // Cập nhật menu theo vai trò
    capNhatMenu() {
        const vaiTro = this.layVaiTro();
        const config = Quyen[vaiTro];
        if (!config) return;

        const nav = document.querySelector('.nav');
        if (!nav) return;

        const user = Auth.layNguoiDung();

        // Xây menu mới
        let menuHtml = '';
        config.menu.forEach(item => {
            // Nếu là nút Đăng nhập/Cá nhân → xử lý đặc biệt
            if (item.id === 'btnDangNhap') {
                if (user) {
                    menuHtml += `<a href="${item.href}" id="${item.id}">👤 ${user.hoTen}</a>`;
                } else {
                    menuHtml += `<a href="${item.href}" id="${item.id}">Đăng nhập</a>`;
                }
            } else {
                // Kiểm tra menu có phải admin không
                if (item.href === 'quan-tri.html' || item.href === 'thong-ke.html') {
                    if (this.laAdmin()) {
                        menuHtml += `<a href="${item.href}">${item.text}</a>`;
                    }
                } else if (item.href === 'quan-ly-co-so.html') {
                    if (this.laDoiTac()) {
                        menuHtml += `<a href="${item.href}">${item.text}</a>`;
                    }
                } else {
                    menuHtml += `<a href="${item.href}">${item.text}</a>`;
                }
            }
        });

        nav.innerHTML = menuHtml;
    },

    // Ẩn/hiện các phần tử có data-quyen
    capNhatPhanTu() {
        document.querySelectorAll('[data-quyen]').forEach(el => {
            const quyen = el.dataset.quyen;
            if (!this.coQuyen(quyen)) {
                el.style.display = 'none';
            }
        });

        document.querySelectorAll('[data-vai-tro]').forEach(el => {
            const vaiTro = el.dataset.vaiTro.split(',');
            if (!vaiTro.includes(this.layVaiTro())) {
                el.style.display = 'none';
            }
        });
    },

    // Khởi tạo tất cả
    khoiTao() {
        this.capNhatMenu();
        this.capNhatPhanTu();
    }
};

// Tự động chạy khi trang load xong
document.addEventListener('DOMContentLoaded', () => {
    PhanQuyen.khoiTao();
});