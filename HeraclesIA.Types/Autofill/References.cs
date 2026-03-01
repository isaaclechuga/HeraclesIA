namespace HeraclesIA.Types.Autofill;

public sealed class References
{
    public References()
    {
        Items = new List<Referencia>();
    }

    public List<Referencia> Items { get; set; }
}

public sealed class Referencia
{
    public Referencia()
    {
        Clases = new List<Clase>();
    }

    public string Name { get; set; } = string.Empty;
    public List<Clase> Clases { get; set; }
}

public sealed class Clase
{
    public Clase()
    {
        Metodos = new List<Metodo>();
    }

    public string Nombre { get; set; } = string.Empty;
    public List<Metodo> Metodos { get; set; }
}

public sealed class Metodo
{
    public string Nombre { get; set; } = string.Empty;
}