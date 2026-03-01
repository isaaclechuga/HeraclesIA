// HeraclesIA.Types.Common/AnalyzeAutofillResult.cs
namespace HeraclesIA.Types.Common;

public sealed class AnalyzeAutofillResult
{
    public int Considerados { get; set; }
    public int Procesados { get; set; }
    public int Publicados { get; set; }
    public int PublicacionOmitida { get; set; }
    public int Omitidos { get; set; }
    public int Errores { get; set; }
    public int ErroresSideEffects { get; set; }
}