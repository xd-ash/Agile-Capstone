namespace WFC
{
    public interface IModule
    {
        public IModule[] North { get; set; }
        public IModule[] East { get; set; }
        public IModule[] South { get; set; }
        public IModule[] West { get; set; }
    }
}