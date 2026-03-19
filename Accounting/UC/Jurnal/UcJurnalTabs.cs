using Accounting._1.Interface;

namespace Accounting.UC.Jurnal
{
    public sealed class UcJurnalInputTab : UcJurnalTabHostBase
    {
        public UcJurnalInputTab(IInputJurnalUseCase useCase) : base(useCase)
        {
        }
    }

    public sealed class UcJurnalDaftarTab : UcJurnalTabHostBase
    {
        public UcJurnalDaftarTab(IDaftarJurnalUseCase useCase) : base(useCase)
        {
        }
    }

    public sealed class UcJurnalCariTab : UcJurnalTabHostBase
    {
        public UcJurnalCariTab(ICariJurnalUseCase useCase) : base(useCase)
        {
        }
    }

    public sealed class UcJurnalKasirTab : UcJurnalTabHostBase
    {
        public UcJurnalKasirTab(IImportJurnalUseCase useCase) : base(useCase)
        {
        }
    }

    public sealed class UcJurnalAisTab : UcJurnalTabHostBase
    {
        public UcJurnalAisTab(IImportJurnalUseCase useCase) : base(useCase)
        {
        }
    }

    public sealed class UcJurnalInventoryTab : UcJurnalTabHostBase
    {
        public UcJurnalInventoryTab(IImportJurnalUseCase useCase) : base(useCase)
        {
        }
    }

    public sealed class UcJurnalHrisTab : UcJurnalTabHostBase
    {
        public UcJurnalHrisTab(IImportJurnalUseCase useCase) : base(useCase)
        {
        }
    }
}
