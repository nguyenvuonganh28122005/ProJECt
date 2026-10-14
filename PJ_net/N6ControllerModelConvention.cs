using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace PJ_net;

/// <summary>
/// Convention cho phép các Controller có tên bắt đầu bằng "N6" (ví dụ: N6ThuocController, N6CartController)
/// ánh xạ tự nhiên và tương thích 100% với các Route và Razor Views quy ước ("Thuoc", "Cart", v.v.).
/// </summary>
public class N6ControllerModelConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.ControllerName.StartsWith("N6", StringComparison.OrdinalIgnoreCase))
        {
            controller.ControllerName = controller.ControllerName.Substring(2);
        }
    }
}
