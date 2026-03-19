using Accounting._1.Interface;
using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accounting.BusinessLayer
{
    public sealed class MonthlySearchRequest
    {
        public string IdData { get; init; } = string.Empty;
        public string Periode { get; init; } = string.Empty;
        public string NoJurnal { get; init; } = string.Empty;
        public string Tanggal { get; init; } = string.Empty;
        public string Kode { get; init; } = string.Empty;
        public string Keterangan { get; init; } = string.Empty;
        public decimal Jumlah { get; init; }

        public bool HasFilter => !string.IsNullOrEmpty(Tanggal) || Jumlah > 0 || !string.IsNullOrEmpty(Kode) || !string.IsNullOrEmpty(NoJurnal) || !string.IsNullOrEmpty(Keterangan);
    }

    public sealed class MonthlySearchResult
    {
        public bool IsFiltered { get; init; }
        public List<JurnalDetailDTO> DetailRows { get; init; } = [];
        public List<JurnalHeaderDTO> HeaderRows { get; init; } = [];
    }

    public sealed class YearlySearchRequest
    {
        public string IdData { get; init; } = string.Empty;
        public int DariTahun { get; init; }
        public int DariBulan { get; init; }
        public int SampaiTahun { get; init; }
        public int SampaiBulan { get; init; }
        public string NoJurnal { get; init; } = string.Empty;
        public string Tanggal { get; init; } = string.Empty;
        public string Kode { get; init; } = string.Empty;
        public string Keterangan { get; init; } = string.Empty;
        public decimal Jumlah { get; init; }

        public bool HasFilter => !string.IsNullOrEmpty(Tanggal) || Jumlah > 0 || !string.IsNullOrEmpty(Kode) || !string.IsNullOrEmpty(NoJurnal) || !string.IsNullOrEmpty(Keterangan);
    }

    public sealed class YearlySearchResult
    {
        public bool IsValidPeriod { get; init; } = true;
        public List<JurnalDetailDTO> DetailRows { get; init; } = [];
        public List<string> Periodes { get; init; } = [];
    }

    public sealed class YearlyExportRequest
    {
        public IEnumerable<JurnalDetailDTO> SearchRows { get; init; } = [];
        public string Periode { get; init; } = string.Empty;
        public bool ExportLengkap { get; init; }
    }

    public sealed class YearlyExportResult
    {
        public bool IsAllowed { get; init; } = true;
        public string Message { get; init; } = string.Empty;
        public List<JurnalDetailDTO> ExportRows { get; init; } = [];
        public List<JurnalDetailReffID> ReffIds { get; init; } = [];
    }

    public sealed class JurnalDaftarCariService
    {
        public MonthlySearchResult SearchMonthly(IJurnalQueryRepository repository, MonthlySearchRequest request)
        {
            if (!request.HasFilter)
            {
                return new MonthlySearchResult
                {
                    IsFiltered = false
                };
            }

            List<JurnalDetailDTO> detailRows = repository.SearchJurnal_Bulan(
                request.IdData,
                request.Periode,
                request.NoJurnal.ToLower(),
                request.Tanggal,
                request.Kode.ToLower(),
                request.Keterangan.ToLower(),
                request.Jumlah).ToList();

            List<JurnalHeaderDTO> headerRows = detailRows
                .GroupBy(group => new { group.REFFID, group.HIDREFF, group.NoJurnal, group.Tanggal })
                .Select(group => new JurnalHeaderDTO
                {
                    JURNALID = group.Key.REFFID,
                    HID = group.Key.HIDREFF,
                    NoJurnal = group.Key.NoJurnal,
                    Tanggal = group.Key.Tanggal
                })
                .ToList();

            return new MonthlySearchResult
            {
                IsFiltered = true,
                DetailRows = detailRows,
                HeaderRows = headerRows
            };
        }

        public YearlySearchResult SearchYearly(IJurnalQueryRepository repository, YearlySearchRequest request)
        {
            if (!request.HasFilter)
            {
                return new YearlySearchResult();
            }

            int dariTahunBulan = Convert.ToInt32($"{request.DariTahun}{request.DariBulan:00}");
            int sampaiTahunBulan = Convert.ToInt32($"{request.SampaiTahun}{request.SampaiBulan:00}");
            if (dariTahunBulan > sampaiTahunBulan)
            {
                return new YearlySearchResult
                {
                    IsValidPeriod = false
                };
            }

            List<JurnalDetailDTO> detailRows = repository.SearchJurnal(
                request.IdData,
                dariTahunBulan,
                sampaiTahunBulan,
                request.NoJurnal.ToLower(),
                request.Tanggal,
                request.Kode.ToLower(),
                request.Keterangan.ToLower(),
                request.Jumlah).ToList();

            List<string> periodes = detailRows.Select(row => row.Periode).Distinct().OrderBy(row => row).ToList();
            return new YearlySearchResult
            {
                IsValidPeriod = true,
                DetailRows = detailRows,
                Periodes = periodes
            };
        }

        public List<JurnalDetailDTO> FilterDetailRowsByHeaderId(IEnumerable<JurnalDetailDTO> rows, double jurnalId)
        {
            return rows.Where(row => row.REFFID == jurnalId).ToList();
        }

        public List<JurnalDetailDTO> BuildMonthlyExportRows(bool exportLengkap, List<JurnalDetailDTO> detailRows, IEnumerable<JurnalDetailDTO> periodeRows, IEnumerable<JurnalHeaderDTO> filteredHeaders)
        {
            if (!exportLengkap)
            {
                return detailRows;
            }

            HashSet<double> headerIds = filteredHeaders.Select(header => header.JURNALID).ToHashSet();
            return periodeRows.Where(detail => headerIds.Contains(detail.REFFID)).ToList();
        }

        public YearlyExportResult BuildYearlyExportRows(IJurnalQueryRepository repository, YearlyExportRequest request)
        {
            IEnumerable<JurnalDetailDTO> exportRows = string.IsNullOrEmpty(request.Periode)
                ? request.SearchRows
                : request.SearchRows.Where(row => row.Periode == request.Periode);

            List<JurnalDetailReffID> reffIds = exportRows
                .Select(row => new JurnalDetailReffID { REFFID = row.REFFID })
                .Distinct()
                .ToList();

            if (!request.ExportLengkap)
            {
                return new YearlyExportResult
                {
                    ExportRows = exportRows.ToList(),
                    ReffIds = reffIds
                };
            }

            if (reffIds.Count > 999)
            {
                return new YearlyExportResult
                {
                    IsAllowed = false,
                    Message = "Record Jurnal lengkap terlalu banyak, silahkan persempit filter pencarian"
                };
            }

            List<JurnalDetailDTO> lengkapRows = repository.GetJurnalLengkap(reffIds).ToList();
            return new YearlyExportResult
            {
                IsAllowed = true,
                ExportRows = lengkapRows,
                ReffIds = reffIds
            };
        }
    }
}
