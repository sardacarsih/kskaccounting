# KSK Accounting Changelog

Dokumen ini mencatat perubahan aplikasi KSK Accounting per versi. Gunakan changelog ini untuk membantu user, support, QA, dan developer memahami fitur baru, perbaikan, perubahan perilaku, serta catatan teknis penting.

## Panduan Penulisan

- Tulis perubahan dari sudut pandang user jika perubahan terlihat di aplikasi.
- Gunakan kalimat singkat, jelas, dan spesifik.
- Kelompokkan perubahan dengan kategori berikut:
  - `Added`: fitur atau kemampuan baru.
  - `Changed`: perubahan perilaku, UI, workflow, atau format.
  - `Fixed`: perbaikan bug.
  - `Removed`: fitur, menu, atau behavior yang dihapus.
  - `Technical`: perubahan internal yang penting untuk support, deployment, atau troubleshooting.
- Simpan catatan teknis hanya jika membantu investigasi masalah atau deployment.
- Jangan gabungkan backlog dengan release note. Item yang belum selesai masuk ke `Backlog / ToDo`.

## Release 1.0.9

Tanggal: -

### Added

- Export jurnal berdasarkan pilihan baris.
- Hapus jurnal by range berdasarkan pilihan baris.
- Export all jurnal AIS Borongan dan Harian berbasis data raw query/list, tanpa stored procedure legacy.

### Changed

- Preview detail AIS dan export all AIS memakai logika pembentukan jurnal yang sama agar hasil preview, export per baris, dan export all konsisten.

### Fixed

- Import jurnal Excel dengan periode kosong.
- RJE multi lokasi.

### Technical

- Refactor pembentukan jurnal AIS ke builder berbasis list raw query.

## Release 1.0.8

Tanggal: -

### Added

- Import jurnal dari module lain: AIS, KAS, dan Inventory.
- Jurnal closing optional.
- Neraca konsolidasi per PT.
- Pembebanan kode akun pekerjaan AISPAD.
- Update Developer to User.

### Changed

- Character length fixed: `NOJURNAL` maksimum 30 karakter, `IDDATA` maksimum 20 karakter.
- Full validasi import jurnal balanced per nomor jurnal.

### Fixed

- Filtering jumlah Debet atau Kredit.
- Copy text dari selected cell.
- Validasi format jurnal sebelum import file Excel.
- Validasi format COA sebelum import file Excel.
- Login profile untuk akses hanya satu lokasi.
- ORA-16550 truncated result saat import jurnal karena nomor baris jurnal nilai minus.
- `balanced_cek` null.
- Cek nomor jurnal dan row number double saat import jurnal.
- Jurnal RE atau reversal entry jurnal balik.
- General Ledger: sumber kode akun dari neraca atau laba rugi.

## Release 1.0.7

Tanggal: -

### Added

- Change note.
- Export daftar blok.

### Changed

- Input jurnal: setelah ketik tanggal, kursor otomatis pindah ke bulan sehingga user tidak perlu tekan tombol panah kanan.

### Fixed

- Pencegahan nilai minus jurnal saat input dan import.
- Add new account dan edit account.
- Checking COA parent.
- Closing month: checking delete trigger status.

### Backlog Saat Release 1.0.7

- Setelah save, kursor tidak selalu muncul di kolom kode jurnal; kadang di keterangan, kadang menghilang.
- Setelah save, tanggal sebaiknya mengikuti tanggal terakhir input.
- User kesulitan copy keterangan karena kadang langsung select all dan kadang tidak.
- Diharapkan dapat copy keterangan di bagian daftar jurnal.
- Tombol F4 untuk save tidak berfungsi setelah user menekan tombol F3 perubahan fungsi F2.
- Setelah user edit jurnal, jurnal tidak langsung terupdate; user harus close jurnal umum lalu masuk lagi.
- Fungsi centang lengkap di bagian cari jurnal tetap hanya menampilkan jurnal yang mengandung kata yang dicari.
- Perlu pilihan close/exit di bagian input kode baru, atau auto close setelah kode baru berhasil dibuat.
- Diharapkan ada total transaksi periode tertentu di bagian buku besar.
- User Tjhiang Hia tidak bisa menampilkan buku besar kode 31 dan 91.
- Saat import, keterangan perkiraan tidak berubah otomatis jika ada perubahan kode.
- Menu lookup search sebaiknya ditukar posisinya karena user lebih sering menggunakan mode lookup search langsung aktif.
- Langsung menampilkan menu yang dibuka.
- Kalkulasi nilai minus dari proses import.
- Input tanggal diproses setelah user input tanggal dan bulan.

## Release 1.0.6

Tanggal: -

### Changed

- Update ke .NET 6 LTS.
- Update DevExpress version 22.1.3.0.

## Release 1.0.5

Tanggal: 20 Desember 2021

### Added

- Analisa biaya blok.
- Laporan kebun:
  - Rekapitulasi biaya TM INTI per divisi.
  - Rincian pekerjaan TBM, Inti dan KKPA.
  - Rincian pekerjaan pemeliharaan TM, Inti dan KKPA.
  - Rincian pekerjaan panen, Inti dan KKPA.
- Kalkulasi luasan divisi TBM/TM berdasarkan penambahan, perubahan, dan penghapusan master blok aktif.

## Release 1.0.4

Tanggal: 06 Desember 2021

### Added

- DevExpress version 21.2.3.0.
- Automatic checking dan fix level account.
- Checking kesalahan induk perkiraan saat tutup tahun.
- Checking kode perkiraan detail yang tidak memiliki induk.
- Generate daftar perkiraan blok:
  - Generate akun blok TBM.
  - Generate akun blok TM.
  - Disable/enable akun blok TBM.
- Indikator selisih saat input/edit jurnal.
- Drag and drop baris jurnal.
- Perkiraan master akun blok `Jenis Pekerjaan`.

### Changed

- Reset saldo awal tahun perkiraan sementara jika saldo Desember akun `40.00000.000` sudah 0.
- Fix view berdasarkan screen resolution.

### Fixed

- Ubah daftar perkiraan.
- Grid view daftar perkiraan.
- Tutup buku jenis akunting lain, seperti Pelsus.
- Keterangan penerima Nota Debet/Nota Kredit.
- Laporan General Ledger multi tahun dan multi periode.
- Edit jurnal pada periode yang memiliki tanggal di luar periode aktif.
- Validasi import daftar perkiraan dan jurnal.
- Export daftar perkiraan agar support import kembali.

### Technical

- Optimalisasi kalkulasi:
  - Sample KSKINTI periode Januari 2007: dari 1 menit 21 detik 513 milidetik menjadi 47 detik 208 milidetik.
  - Sample KSKINTI periode Desember 2007: dari 1 menit 16 detik 322 milidetik menjadi 41 detik 859 milidetik.
- Optimalisasi loading daftar perkiraan.

## Release 1.0.3

Tanggal: 29 Nopember 2021

### Added

- Tipe akunting untuk kantor perwakilan dan lain/Pelsus yang tidak memiliki jurnal closing.
- Library EPPlus untuk export Excel.
- Jurnal balik atau reverse jurnal entry berlaku setiap periode jika ada, dengan format nomor `NOMORJURNALLAMA-RE`.
- Drill down atau proses indikator pada neraca.
- Default closing account jika masih kosong.

### Changed

- Rekalkulasi server side:
  - Rekalkulasi Januari update saldo Januari sampai Desember.
  - Rekalkulasi Oktober update saldo Oktober sampai Desember tahun berjalan.
- Row number import jurnal khusus MSL dinaikkan ke 6 digit untuk menghindari ORA-16550 truncated result.

### Fixed

- Karakter ID daftar perkiraan saat tutup tahunan.
- Neraca menampilkan saldo awal tahun meskipun saldo akhir 0.
- Min dan max tahun berdasarkan tahun daftar perkiraan.
- Format tahun pada semua form tahun.
- Laporan General Ledger tampilan mutasi per periode.
- Saldo neraca tidak seimbang karena double jurnal closing.
- Keterangan perkiraan jurnal `001/LABA`.

## Release 1.0.2

Tanggal: -

### Added

- Input jurnal dan daftar jurnal dengan filter.
- Export jurnal dengan tambahan formula pada nomor baris.
- Input jurnal dengan keyboard shortcut seperti F1, F2, F3, dan lainnya.
- Filtering daftar perkiraan berdasarkan akun neraca, akun laba rugi, TM, TBM, tampilkan hanya yang memiliki nilai, dan custom filter.
- Laporan Buku Besar support multi periode dan multi akun.

### Changed

- Input jurnal hanya bisa mengisi nilai satu sisi Debet atau Kredit.
- Penomoran ND/NK berdasarkan jenis `NOJURNAL`, misalnya `/JP` dan `/SP`, serta support multi pengiriman ND/NK.
- Buang label tanggal cetak pada laporan.
- Nomor jurnal berdasarkan empat parameter: `IDDATA`, `PERIODE`, `TANGGAL`, dan `NOJURNAL`.
- Dalam satu bulan bisa berisi nomor yang sama pada tanggal yang berbeda, contoh:
  - `001/ND-KSKPUSAT` tanggal 05 Januari 2021.
  - `001/ND-KSKPUSAT` tanggal 31 Januari 2021.
- Redesign export jurnal by filter.

### Fixed

- Fitur export Excel.
- Import cek nomor jurnal double untuk transaksi lama dihapus.
- Export daftar perkiraan:
  - PUSAT/PKS tambahan kolom saldo awal tahun dan mutasi.
  - KEBUN tambahan kolom saldo awal tahun, mutasi, divisi, blok, dan tahun tanam.
- Beberapa database integrity.
- Validasi induk akun saat import COA.

## Release 1.0.1

Tanggal: 25 Oktober 2021

### Added

- Nota Debet dan Nota Kredit.
- Input jurnal full keyboard operation.
- Full filter pada Daftar Perkiraan dengan `CTRL + F`.
- Full filter pada Import Jurnal, Export Jurnal, Nota Debet.
- Pengecekan Neraca pada Tutup Bulanan dan Tutup Akhir Tahun.

### Changed

- Identitas perusahaan lengkap di form utama bagian atas.
- Geser kolom induk di sebelah kanan kolom level.
- Judul laporan Buku Besar tampil di setiap halaman.
- Mode ubah dan hapus jurnal melalui tombol dan klik kanan pada grid.
- Export jurnal sekaligus dengan mode input `N/A`.
- Mode export jurnal tetap di menu Jurnal.
- Export jurnal dengan full filter.
- Daftar jurnal pada mode input hanya untuk kebutuhan edit dan hapus jurnal.
- Tombol baru pada input jurnal dibuang.
- Notifikasi simpan jurnal dihilangkan dan diganti dengan mode entri baru.
- Update daftar lokasi pembukuan.
- Optimasi kecepatan input jurnal.
- Finishing management login user.

### Fixed

- Notifikasi double `NOJURNAL` saat input dan saat simpan.
- Perpindahan baris pada daftar nomor jurnal agar jurnal detail tampil dengan benar.

## Release 1.0.0

Tanggal: 22 Oktober 2021

### Added

- Support unlimited company data.
- Unlimited level account.
- Drill down report.
- Custom access level.
- UI/UX fast respond.
- Report consolidations.
- Design database integrity.

## Backlog / ToDo

- Export luasan block di COA.
- Diharapkan dapat copy code di bagian input jurnal.
- Nota Debet seharusnya juga termasuk jurnal ND dan KK; begitu juga dengan bagian Nota Kredit.
- Pada bagian input kode, hasil pencarian bagian tengah atau akhir tidak perlu ditampilkan. Contoh pencarian `10`, kode `xx.x10xx.xx` tidak perlu muncul. Pisahkan kolom pencarian kode dan keterangan.
- Add COA export advanced, seperti export saldo by range periode dan range `KODEACC`.
