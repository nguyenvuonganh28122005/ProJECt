using Microsoft.AspNetCore.Mvc.Razor;

namespace PJ_net;

/// <summary>
/// Cho phép Razor View Engine tự động tìm kiếm các view trong các thư mục có tiền tố N6
/// (ví dụ: /Views/N6Thuoc/, /Areas/DuocSi/Views/N6BanHang/, /Areas/Admin/Views/N6Thuoc/)
/// mà vẫn giữ nguyên tính tương thích tuyệt đối cho các view Shared và Partial.
/// </summary>
public class N6ViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        // Ưu tiên các thư mục mang tiền tố N6 theo nhóm 6
        yield return "/Areas/{2}/Views/N6{1}/{0}.cshtml";
        yield return "/Views/N6{1}/{0}.cshtml";

        // Fallback về các vị trí quy ước mặc định
        foreach (var location in viewLocations)
        {
            yield return location;
        }
    }
}
