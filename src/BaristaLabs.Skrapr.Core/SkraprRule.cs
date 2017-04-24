namespace BaristaLabs.Skrapr
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an abstract implementation of the ISkraprRule interface.
    /// </summary>
    public abstract class SkraprRule : ISkraprRule
    {
        public abstract string Type
        {
            get;
        }

        public string Description
        {
            get;
            set;
        }

        public int? Max
        {
            get;
            set;
        }

        public IList<ISkraprTask> Tasks
        {
            get;
            set;
        }

        public abstract Task<bool> IsMatch(SkraprFrameState frameState);

        public override string ToString()
        {
            return Type;
        }
    }
}
