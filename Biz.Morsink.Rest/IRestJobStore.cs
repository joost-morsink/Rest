using Biz.Morsink.Identity;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    public interface IRestJobStore
    {
        RestJob GetJob(IIdentity<RestJob> id);
        RestJob RegisterJob(Task<RestResponse> task);
    }
}