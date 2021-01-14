#region

using Alturos.Yolo.Model;

#endregion

namespace Alturos.Yolo
{
    public interface IYoloSystemValidator
    {
        SystemValidationReport Validate();
    }
}