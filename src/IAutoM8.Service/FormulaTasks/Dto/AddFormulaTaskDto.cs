namespace IAutoM8.Service.FormulaTasks.Dto
{
    public class AddFormulaTaskDto
    {
        public int FormulaProjectId { get; set; }
        public int InternalFormulaProjectId { get; set; }

        public int PosX { get; set; }
        public int PosY { get; set; }
    }
}
