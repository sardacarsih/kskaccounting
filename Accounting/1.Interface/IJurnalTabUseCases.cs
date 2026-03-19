namespace Accounting._1.Interface
{
    public interface IJurnalTabUseCase
    {
        IJurnalDomainRepository Repository { get; }
    }

    public interface IInputJurnalUseCase : IJurnalTabUseCase
    {
    }

    public interface IDaftarJurnalUseCase : IJurnalTabUseCase
    {
    }

    public interface ICariJurnalUseCase : IJurnalTabUseCase
    {
    }

    public interface IImportJurnalUseCase : IJurnalTabUseCase
    {
    }
}
