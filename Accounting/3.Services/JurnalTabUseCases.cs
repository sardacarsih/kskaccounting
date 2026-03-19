using Accounting._1.Interface;

namespace Accounting.BusinessLayer
{
    public sealed class InputJurnalUseCase : IInputJurnalUseCase
    {
        public InputJurnalUseCase(IJurnalDomainRepository repository)
        {
            Repository = repository;
        }

        public IJurnalDomainRepository Repository { get; }
    }

    public sealed class DaftarJurnalUseCase : IDaftarJurnalUseCase
    {
        public DaftarJurnalUseCase(IJurnalDomainRepository repository)
        {
            Repository = repository;
        }

        public IJurnalDomainRepository Repository { get; }
    }

    public sealed class CariJurnalUseCase : ICariJurnalUseCase
    {
        public CariJurnalUseCase(IJurnalDomainRepository repository)
        {
            Repository = repository;
        }

        public IJurnalDomainRepository Repository { get; }
    }

    public sealed class ImportJurnalUseCase : IImportJurnalUseCase
    {
        public ImportJurnalUseCase(IJurnalDomainRepository repository)
        {
            Repository = repository;
        }

        public IJurnalDomainRepository Repository { get; }
    }
}
