namespace Skrapr
{
    using EntryPoint;

    public class CliArguments : BaseCliArguments
    {
        public CliArguments()
            : base("Skrapr")
        {
        }

        [Required]
        [Operand(Position: 1)]
        [Help("Specifies the path to the skrapr definition file to use")]
        public string SkraprDefinitionPath
        {
            get;
            set;
        }
    }
}
