namespace HeraclesIA.Types.Common
{
    public class HelperEntity
    {
        public int ProcessId { get; set; }          
        public string Process { get; set; } = "";   

        public int CategoryNum { get; set; }
        public string CategoryName { get; set; } = "";

        public string DashboardNum { get; set; } = "";
        public string FolderId { get; set; } = "";

        public bool Estatus { get; set; }           
    }
}
